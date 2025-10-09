using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Ryfts;

public class MapController : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject nodePrefab;
    public Material dottedEdgeMaterial;

    [Header("Sprite Library (non-Rift)")]
    public List<NodeSprite> sprites = new();   // Enemy/Shop/Rest/Elite/Unknown

    [Header("Layout")]
    public Transform nodeParent;
    [Min(0.25f)] public float verticalSpacing = 2.5f;
    [Min(0.25f)] public float horizontalSpacing = 2.5f;

    [Header("Camera")]
    [SerializeField] Camera cam;
    [SerializeField, Min(1f)] float camSize = 6f;
    [SerializeField, Min(0f)] float camLerp = 8f;

    [Header("Current Node Marker")]
    public float markerRadius = 0.65f;
    public float markerWidth  = 0.06f;
    [Range(8,128)] public int   markerSegments = 48;
    public Color markerColor   = new Color(1f, 1f, 1f, 0.9f);

    [Header("Keyboard Focus Ring")]
    [Tooltip("Radius of the keyboard focus ring (the movable selector).")]
    public float focusRadius = 0.75f;
    [Tooltip("Line width of the focus ring.")]
    public float focusWidth  = 0.06f;
    [Range(8,128)] public int   focusSegments = 48;
    public Color focusColor   = new Color(1f, 1f, 0.25f, 0.95f); // a bit golden

    [Header("Debug")]
    public bool verboseLogging = true;
    public bool drawGizmos     = false;

    // runtime
    private readonly List<List<MapNode>> levels = new();
    private readonly List<DottedEdge> edges = new();
    private readonly HashSet<string> edgeKeys = new();
    private MapNode currentNode;           // where the player "is"
    private LineRenderer markerLR;         // ring around current node

    // Keyboard nav runtime
    private MapNode focusedNode;           // which node the keyboard is hovering
    private readonly List<MapNode> focusCandidates = new(); // children on next row
    private int focusIndex = 0;
    private LineRenderer focusLR;          // ring that follows focusedNode

    private readonly Dictionary<string, Sprite> _riftCache = new();

    [Serializable]
    public class NodeSprite { public MapNodeType type; public Sprite sprite; }

    // Prevents double-resolving the same ryft node
    private readonly HashSet<MapNode> _resolvedRyfts = new HashSet<MapNode>();


    // ─────────────────────────────────────────────────────────────────────────────
    void OnValidate()
    {
        verticalSpacing   = Mathf.Max(0.25f, verticalSpacing);
        horizontalSpacing = Mathf.Max(0.25f, horizontalSpacing);
        camSize           = Mathf.Max(1f, camSize);
        camLerp           = Mathf.Max(0f, camLerp);

        markerSegments = Mathf.Clamp(markerSegments, 8, 128);
        markerRadius   = Mathf.Max(0.05f, markerRadius);
        markerWidth    = Mathf.Max(0.01f, markerWidth);

        focusSegments = Mathf.Clamp(focusSegments, 8, 128);
        focusRadius   = Mathf.Max(0.05f, focusRadius);
        focusWidth    = Mathf.Max(0.01f, focusWidth);
    }

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (cam) { cam.orthographic = true; cam.orthographicSize = camSize; }
        if (!RyftEffectManager.Instance)
            new GameObject("RyftEffectManager").AddComponent<RyftEffectManager>();
    }

    void Start()
    {
        if (!nodePrefab) { Debug.LogError("MapController: Node Prefab not assigned."); enabled = false; return; }
        if (!nodeParent) nodeParent = this.transform;

        nodeParent.transform.localPosition = Vector3.zero;
        nodeParent.transform.localRotation = Quaternion.identity;
        nodeParent.transform.localScale    = Vector3.one;

        ValidateRiftSprites();

        if (MapSession.I != null && MapSession.I.Saved != null)
        {
            RestoreState(MapSession.I.Saved);
            CenterCameraNow();
            BuildCurrentMarker();
            MoveCurrentMarkerToCurrent();

            // keyboard ring
            BuildFocusRing();
            FocusCurrentOrChildren();

            RevealFrom(currentNode, 3);
            return;
        }
        else
        {
            GenerateLevels(6);
            ApplyReachabilityForCurrent(includeChildren:false);
            RevealFrom(currentNode, 3);
            CenterCameraNow();
        }

        BuildCurrentMarker();
        MoveCurrentMarkerToCurrent();

        // keyboard ring
        BuildFocusRing();
        FocusCurrentOrChildren();

        if (verboseLogging) DumpMap();
    }

    // ───────────────────────── generation / graph ────────────────────────────────
    void GenerateLevels(int depthToAdd)
    {
        int startLevel = levels.Count;

        for (int level = startLevel; level < startLevel + depthToAdd; level++)
        {
            var row = new List<MapNode>();

            // First row must have exactly one node
            int count = (level == 0) ? 1 : UnityEngine.Random.Range(1, 3);

            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(nodePrefab, nodeParent);
                go.name = $"Node L{level} N{i}";

                float x = (i - (count - 1) * 0.5f) * horizontalSpacing;
                float y = level * verticalSpacing;
                go.transform.localPosition = new Vector3(x, y, 0f);

                var node   = go.GetComponent<MapNode>();
                var type   = RandomNodeType();
                var sprite = (type == MapNodeType.Rift) ? null : GetSprite(type);
                node.Init(type, sprite, this);
                node.RefreshVisualSize();
                row.Add(node);

                // All newly created nodes start unreachable (SILENT) until actually reached.
                node.SetReachableSilently(false);

            }

            levels.Add(row);

            if (level > 0)
            {
                var prevRow = levels[level - 1];
                ConnectRowsNonCrossing(prevRow, row, level - 1, level);
            }
        }

        void ConnectRowsNonCrossing(List<MapNode> prevRow, List<MapNode> row, int fromLevel, int toLevel)
        {
            // 1) Sweep left->right across parents and only choose children with index >= a moving cursor.
            int cursor = 0;
            for (int p = 0; p < prevRow.Count; p++)
            {
                var parent = prevRow[p];

                int desired = (row.Count == 1) ? 1 : UnityEngine.Random.Range(1, 3); // 1–2

                for (int k = 0; k < desired; k++)
                {
                    if (cursor >= row.Count) break;

                    int minIdx = cursor;
                    int maxIdx = row.Count - 1;
                    int idx = (minIdx == maxIdx)
                        ? minIdx
                        : UnityEngine.Random.Range(minIdx, Mathf.Min(minIdx + 2, maxIdx + 1));

                    Connect(parent, row[idx], fromLevel, toLevel);
                    cursor = Mathf.Max(cursor, idx);
                }
            }

            // 2) Ensure every child has at least one incoming, without crossings.
            var hasIncoming = new HashSet<MapNode>();
            foreach (var p in prevRow)
                foreach (var c in p.connections)
                    if (c) hasIncoming.Add(c);

            for (int j = 0; j < row.Count; j++)
            {
                var child = row[j];
                if (hasIncoming.Contains(child)) continue;

                int pIdx = Mathf.Clamp(j, 0, prevRow.Count - 1);
                var parent = prevRow[pIdx];
                Connect(parent, child, fromLevel, toLevel);
            }
        }

        // first-time start node
        if (currentNode == null)
        {
            currentNode = levels[0][0];
            currentNode.Discover();
            currentNode.SetReachableSilently(true); // starting node reachable
        }
    }

    void Connect(MapNode from, MapNode to, int fromLevel, int toLevel)
    {
        if (!from || !to) return;

        string key = $"{from.GetInstanceID()}->{to.GetInstanceID()}";
        if (!edgeKeys.Add(key)) return;

        if (!from.connections.Contains(to)) from.connections.Add(to);

        var eGo = new GameObject($"Edge L{fromLevel}->{toLevel}");
        var edge = eGo.AddComponent<DottedEdge>();
        edge.transform.SetParent(nodeParent, false);
        edge.a = from.transform;
        edge.b = to.transform;
        edge.dottedMaterial = dottedEdgeMaterial; // ok if null
        edges.Add(edge);
    }

    MapNodeType RandomNodeType()
    {
        var pool = new[] { MapNodeType.Enemy, MapNodeType.Shop, MapNodeType.Rest, MapNodeType.Elite, MapNodeType.Rift };
        return pool[UnityEngine.Random.Range(0, pool.Length)];
    }

    // ────────────────────────── sprites ─────────────────────────────────────────
    public Sprite GetSprite(MapNodeType t)
    {
        foreach (var ns in sprites) if (ns.type == t) return ns.sprite;
        return null;
    }

    // Loader for rift sprites from Resources/Map/Ryfts/<color><State>.jpg
    public Sprite GetRiftSprite(RyftColor color, RiftState state)
    {
        // Resources/Map/Ryfts/<color><State>.jpg
        string baseName = color.ToString().ToLower(); // blue/orange/green/yellow/purple
        string suffix = state switch
        {
            RiftState.Open     => "Rift",
            RiftState.Closed   => "Closed",
            RiftState.Exploded => "Exploded",
            _ => "Rift"
        };

        string key  = $"{baseName}{suffix}";
        string path = $"Map/Ryfts/{key}";

        if (_riftCache.TryGetValue(key, out var cached) && cached != null)
            return cached;

        var s = Resources.Load<Sprite>(path);

        if (!s)
        {

            string fkKey  = $"{baseName}Rift";
            string fkPath = $"Map/Ryfts/{fkKey}";
            var fallback = _riftCache.TryGetValue(fkKey, out var fk) ? fk : Resources.Load<Sprite>(fkPath);

            _riftCache[key] = fallback;
            return fallback;
        }

        _riftCache[key] = s;
        return s;
    }

    // ───────────────── movement / reveal / branch logic ─────────────────────────
    public void OnNodeChosen(MapNode node)
    {
        int levelIdx = FindLevelOf(node);

        // Collect same-row open ryfts we’re about to explode
        var toMaybeExplode = new List<MapNode>();
        foreach (var n in levels[levelIdx])
        {
            if (n != node && n.type == MapNodeType.Rift && n.riftState == RiftState.Open)
            {
                toMaybeExplode.Add(n);
                n.SetReachableDefault(false); // your code: "will explode"
            }
        }

        currentNode = node;
        currentNode.MarkVisited();

        // If we just CLOSED a ryft, resolve its Positive effect
        if (currentNode.type == MapNodeType.Rift)
            ResolveRyftOutcome(currentNode, closed:true);

        // Any siblings that actually EXPLODED? resolve their Negative effects
        foreach (var n in toMaybeExplode)
            if (n.riftState == RiftState.Exploded)
                ResolveRyftOutcome(n, closed:false);

        // After choosing, allow only children in the immediate next row
        ApplyReachabilityForCurrent(includeChildren:true);

        RevealFrom(currentNode, 3);

        if ((levels.Count - 1 - levelIdx) < 3)
        {
            GenerateLevels(3);
            ApplyReachabilityForCurrent(includeChildren:true);
        }

        MoveCurrentMarkerToCurrent();
        PanCameraTo(node.transform.position);

        // Update keyboard focus to reflect the new current/children
        FocusCurrentOrChildren();
    }

    private void ResolveRyftOutcome(MapNode ryftNode, bool closed)
    {
        if (ryftNode == null || ryftNode.type != MapNodeType.Rift) return;
        if (_resolvedRyfts.Contains(ryftNode)) return;

        var color    = ryftNode.ryftColor;
        var polarity = closed ? EffectPolarity.Positive : EffectPolarity.Negative;

        var db = RyftEffectDatabase.Load();
        db?.DebugDumpContents(); // todo - rm after debugging
        var all = db?.All;

        // collect all candidates that match color + polarity
        var matches = new List<RyftEffectDef>();
        foreach (var e in all)
            if (e && e.color == color && e.polarity == polarity)
                matches.Add(e);

        var verb = closed ? "CLOSED" : "EXPLODED";
        if (matches.Count == 0)
        {

            _resolvedRyfts.Add(ryftNode);
            return;
        }

        // pick one at random
        int idx = UnityEngine.Random.Range(0, matches.Count);
        var chosen = matches[idx];
        RyftEffectManager.Ensure().OnRyftOutcome(chosen);
        _resolvedRyfts.Add(ryftNode);
    }



    /// <summary>
    /// Locks everything except the current node; optionally makes immediate children clickable.
    /// All rows beyond "next" are SILENTLY unreachable (no Rift explosions).
    /// </summary>
    void ApplyReachabilityForCurrent(bool includeChildren)
    {
        int levelIdx = FindLevelOf(currentNode);

        // previous rows: silently lock
        for (int l = 0; l < levelIdx; l++)
            foreach (var n in levels[l]) n.SetReachableSilently(false);

        // current row: only current is reachable (silent change for siblings)
        foreach (var n in levels[levelIdx])
            n.SetReachable(n == currentNode, explodeWhenFalse:false);

        // next row:
        int next = levelIdx + 1;
        if (next < levels.Count)
        {
            var allow = new HashSet<MapNode>(currentNode.connections);

            foreach (var n in levels[next])
            {
                if (includeChildren)
                {
                    bool permitted = allow.Contains(n);
                    n.SetReachable(permitted, explodeWhenFalse: !permitted);
                }
                else
                {
                    n.SetReachableSilently(false);
                }
            }

        }

        // ALL rows further ahead: silently locked
        for (int l = levelIdx + 2; l < levels.Count; l++)
            foreach (var n in levels[l]) n.SetReachableSilently(false);
    }

    void RevealFrom(MapNode node, int rowsAhead)
    {
        int start = FindLevelOf(node);
        for (int l = start; l <= start + rowsAhead && l < levels.Count; l++)
            foreach (var n in levels[l]) n.Discover();
    }

    int FindLevelOf(MapNode node)
    {
        for (int l = 0; l < levels.Count; l++) if (levels[l].Contains(node)) return l;
        return 0;
    }

    // ───────────────────────── camera helpers ───────────────────────────────────
    void CenterCameraNow()
    {
        if (!cam || !currentNode) return;
        var p = currentNode.transform.position;
        cam.transform.position = new Vector3(p.x, p.y, cam.transform.position.z);
    }

    void PanCameraTo(Vector3 worldPos)
    {
        if (!cam) return;
        StopAllCoroutines();
        if (camLerp <= 0f)
        {
            var start = cam.transform.position;
            cam.transform.position = new Vector3(worldPos.x, worldPos.y, start.z);
        }
        else StartCoroutine(PanRoutine(worldPos));
    }

    IEnumerator PanRoutine(Vector3 worldPos)
    {
        var start  = cam.transform.position;
        var target = new Vector3(worldPos.x, worldPos.y, start.z);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * camLerp;
            cam.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        cam.transform.position = target;
    }

    // ───────────────────────── current marker ───────────────────────────────────
    void BuildCurrentMarker()
    {
        if (markerLR) return;

        var go = new GameObject("CurrentMarker");
        go.transform.SetParent(nodeParent, false);
        markerLR = go.AddComponent<LineRenderer>();
        markerLR.useWorldSpace = true;
        markerLR.loop = true;
        markerLR.positionCount = Mathf.Max(8, markerSegments);
        markerLR.widthMultiplier = markerWidth;
        markerLR.material = new Material(Shader.Find("Sprites/Default"));
        markerLR.startColor = markerColor;
        markerLR.endColor   = markerColor;
        markerLR.sortingOrder = 9;

        var pts = new Vector3[markerLR.positionCount];
        for (int i = 0; i < pts.Length; i++)
        {
            float a = i / (float)pts.Length * Mathf.PI * 2f;
            pts[i] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
        }
        markerLR.SetPositions(pts);
    }

    void MoveCurrentMarkerToCurrent()
    {
        if (!markerLR || !currentNode) return;

        int n = markerLR.positionCount;
        var pts = new Vector3[n];
        Vector3 c = currentNode.transform.position;
        for (int i = 0; i < n; i++)
        {
            float a = i / (float)n * Mathf.PI * 2f;
            pts[i] = new Vector3(c.x + Mathf.Cos(a) * markerRadius,
                                 c.y + Mathf.Sin(a) * markerRadius,
                                 c.z);
        }
        markerLR.SetPositions(pts);
        markerLR.enabled = true;
    }

    // ───────────────────────── keyboard focus ring ──────────────────────────────
    void BuildFocusRing()
    {
        if (focusLR) return;

        var go = new GameObject("FocusRing");
        go.transform.SetParent(nodeParent, false);
        focusLR = go.AddComponent<LineRenderer>();
        focusLR.useWorldSpace = false; // we’ll parent it to the focused node
        focusLR.loop = true;
        focusLR.positionCount = Mathf.Max(8, focusSegments);
        focusLR.widthMultiplier = focusWidth;
        focusLR.material = new Material(Shader.Find("Sprites/Default"));
        focusLR.startColor = focusColor;
        focusLR.endColor   = focusColor;
        focusLR.sortingOrder = 11; // above current marker

        var pts = new Vector3[focusLR.positionCount];
        for (int i = 0; i < pts.Length; i++)
        {
            float a = i / (float)pts.Length * Mathf.PI * 2f;
            pts[i] = new Vector3(Mathf.Cos(a) * focusRadius, Mathf.Sin(a) * focusRadius, 0f);
        }
        focusLR.SetPositions(pts);
        focusLR.enabled = false;
    }

    void SetFocus(MapNode node)
    {
        focusedNode = node;
        if (!focusLR) BuildFocusRing();

        if (focusedNode != null && focusLR != null)
        {
            // Parent the ring to the node so it follows automatically
            var t = focusLR.transform;
            t.SetParent(focusedNode.transform, false);
            t.localPosition = Vector3.zero;
            focusLR.enabled = true;
        }
        else if (focusLR)
        {
            focusLR.enabled = false;
        }
    }

    void FocusCurrentOrChildren()
    {
        // If current node has reachable children, focus that choice row; else focus current node
        focusCandidates.Clear();

        if (currentNode != null)
        {
            var children = GetChildrenOf(currentNode).Where(n => n && n.isReachable).ToList();
            if (children.Count > 0)
            {
                // keep index if possible, else nearest in X to current node
                var ordered = OrderLeftToRight(children);
                focusCandidates.AddRange(ordered);

                // pick nearest by X to previous focus if it was among candidates
                float anchorX = (focusedNode ? focusedNode.transform.position : currentNode.transform.position).x;
                focusIndex = NearestByX(focusCandidates, anchorX);
                SetFocus(focusCandidates[focusIndex]);
                return;
            }
        }

        // fallback: focus current node itself
        SetFocus(currentNode);
    }

    List<MapNode> GetChildrenOf(MapNode parent)
        => (parent != null && parent.connections != null) ? parent.connections : new List<MapNode>();

    List<MapNode> GetParentsOf(MapNode child)
    {
        var parents = new List<MapNode>();
        int cl = FindLevelOf(child);
        int pl = cl - 1;
        if (pl < 0 || pl >= levels.Count) return parents;

        foreach (var p in levels[pl])
            if (p != null && p.connections != null && p.connections.Contains(child))
                parents.Add(p);

        return parents;
    }

    List<MapNode> OrderLeftToRight(List<MapNode> nodes)
        => nodes.OrderBy(n => n.transform.position.x).ToList();

    int NearestByX(List<MapNode> list, float x)
    {
        int idx = 0; float best = float.MaxValue;
        for (int i = 0; i < list.Count; i++)
        {
            float d = Mathf.Abs(list[i].transform.position.x - x);
            if (d < best) { best = d; idx = i; }
        }
        return idx;
    }

    void MoveFocusUpRow()
    {
        if (currentNode == null) return;

        var children = GetChildrenOf(currentNode).Where(n => n && n.isReachable).ToList();
        if (children.Count == 0) return;

        focusCandidates.Clear();
        focusCandidates.AddRange(OrderLeftToRight(children));

        float anchorX = (focusedNode ? focusedNode.transform.position : currentNode.transform.position).x;
        focusIndex = NearestByX(focusCandidates, anchorX);
        SetFocus(focusCandidates[focusIndex]);
    }

    void MoveFocusDownRow()
    {
        // Down returns focus to the current node (the one you are on)
        if (currentNode == null) return;
        focusCandidates.Clear();
        SetFocus(currentNode);
    }

    void MoveFocusHorizontal(int dir)
    {
        if (focusCandidates.Count <= 1) return;
        focusIndex = (focusIndex + dir) % focusCandidates.Count;
        if (focusIndex < 0) focusIndex += focusCandidates.Count;
        SetFocus(focusCandidates[focusIndex]);
    }

    void ConfirmSelection()
    {
        if (!focusedNode) return;

        // If focus is on a child of current and it's reachable, choose it.
        if (currentNode != null &&
            focusedNode != currentNode &&
            GetChildrenOf(currentNode).Contains(focusedNode) &&
            focusedNode.isReachable)
        {
            focusedNode.Activate();
            return;
        }

        // If focus is on current node or an invalid target, do nothing.
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Keyboard arrows + Enter
        if (Input.GetKeyDown(KeyCode.UpArrow))    MoveFocusUpRow();
        if (Input.GetKeyDown(KeyCode.DownArrow))  MoveFocusDownRow();
        if (Input.GetKeyDown(KeyCode.LeftArrow))  MoveFocusHorizontal(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveFocusHorizontal(+1);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ConfirmSelection();
    }

    // ───────────────────────── debug helpers ────────────────────────────────────
    void DumpMap()
    {
        var sb = new StringBuilder();

        for (int l = 0; l < levels.Count; l++)
        {
            var row = levels[l];

            for (int i = 0; i < row.Count; i++)
            {
                var n = row[i];
                sb.Append(n.type);
                if (i < row.Count - 1) sb.Append(" | ");
            }
            sb.AppendLine();
        }
        sb.AppendLine("[Map] =========================");
        Debug.Log(sb.ToString());
    }

    void ValidateRiftSprites()
    {
        string[] colors = { "blue", "orange", "green", "yellow", "purple" };
        string[] states = { "Rift", "Closed", "Exploded" };
        int ok = 0, miss = 0;
        foreach (var c in colors)
            foreach (var s in states)
                if (Resources.Load<Sprite>($"Map/Ryfts/{c}{s}")) ok++;
                else { miss++; Debug.LogWarning($"[Map] Missing: Resources/Map/Ryfts/{c}{s}"); }
        Debug.Log($"[Map] Rift sprites check: {ok} found, {miss} missing.");
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || levels == null) return;
        Gizmos.color = Color.cyan;
        foreach (var row in levels)
            foreach (var n in row)
                if (n) Gizmos.DrawSphere(n.transform.position, 0.08f);
    }

    /* ---- Store Map State --- */
    public MapState BuildState()
    {
        var st = new MapState();
        int L = levels.Count;

        // find current
        int curL = 0, curI = 0;
        for (int l = 0; l < L; l++)
        {
            int idx = levels[l].IndexOf(currentNode);
            if (idx >= 0) { curL = l; curI = idx; break; }
        }
        st.currentLevel = curL;
        st.currentIndex = curI;

        // camera
        if (cam)
        {
            st.camX = cam.transform.position.x;
            st.camY = cam.transform.position.y;
        }

        st.levels = new MapState.LevelState[L];
        for (int l = 0; l < L; l++)
        {
            var row = levels[l];
            var Ls = new MapState.LevelState { nodes = new MapState.NodeState[row.Count] };
            st.levels[l] = Ls;

            for (int i = 0; i < row.Count; i++)
            {
                var n = row[i];
                var ns = new MapState.NodeState
                {
                    type = n.type,
                    x = n.transform.localPosition.x,
                    y = n.transform.localPosition.y,
                    discovered = n.isDiscovered,
                    reachable  = n.isReachable,
                    visited    = n.visited,
                    isRift     = (n.type == MapNodeType.Rift),
                    ryftColor  = n.ryftColor,
                    riftState  = n.riftState
                };

                // connections
                int cCount = n.connections.Count;
                ns.connToLevel = new int[cCount];
                ns.connToIndex = new int[cCount];
                for (int k = 0; k < cCount; k++)
                {
                    var dst = n.connections[k];
                    // find destination indices
                    for (int dl = 0; dl < L; dl++)
                    {
                        int di = levels[dl].IndexOf(dst);
                        if (di >= 0) { ns.connToLevel[k] = dl; ns.connToIndex[k] = di; break; }
                    }
                }

                Ls.nodes[i] = ns;
            }
        }

        return st;
    }

    public void RestoreState(MapState st)
    {
        // clear any existing runtime graph/lines
        foreach (var e in edges) if (e) Destroy(e.gameObject);
        edges.Clear(); edgeKeys.Clear();
        foreach (var row in levels)
            foreach (var n in row)
                if (n) Destroy(n.gameObject);
        levels.Clear();

        // rebuild nodes
        for (int l = 0; l < st.levels.Length; l++)
        {
            var rowList = new List<MapNode>();
            var Ls = st.levels[l];
            for (int i = 0; i < Ls.nodes.Length; i++)
            {
                var ns = Ls.nodes[i];
                var go = Instantiate(nodePrefab, nodeParent);
                go.name = $"Node L{l} N{i}";
                go.transform.localPosition = new Vector3(ns.x, ns.y, 0f);

                var node = go.GetComponent<MapNode>();

                Sprite sprite = (ns.type == MapNodeType.Rift) ? null : GetSprite(ns.type);
                node.Init(ns.type, sprite, this);
                node.RefreshVisualSize();
                // restore rift specifics
                if (ns.isRift)
                {
                    node.ryftColor = ns.ryftColor;
                    node.SetRiftState(ns.riftState);
                }

                // restore flags (do reachability visually, collider later)
                node.isDiscovered = ns.discovered;
                node.isReachable  = ns.reachable;
                node.visited      = ns.visited;
                // push visuals/collider from flags
                node.Discover();
                if (!ns.reachable) node.SetReachableSilently(false);
                if (ns.visited)    node.MarkVisited();

                rowList.Add(node);
            }
            levels.Add(rowList);
        }

        // restore connections + edges
        for (int l = 0; l < st.levels.Length; l++)
        {
            var Ls = st.levels[l];
            for (int i = 0; i < Ls.nodes.Length; i++)
            {
                var from = levels[l][i];
                var ns = Ls.nodes[i];
                for (int k = 0; k < ns.connToLevel.Length; k++)
                {
                    int tl = ns.connToLevel[k];
                    int ti = ns.connToIndex[k];
                    var to = levels[tl][ti];
                    // wire runtime + draw line
                    if (!from.connections.Contains(to)) from.connections.Add(to);
                    Connect(from, to, l, tl);
                }
            }
        }

        // current node
        currentNode = levels[st.currentLevel][st.currentIndex];

        // camera
        if (cam) cam.transform.position = new Vector3(st.camX, st.camY, cam.transform.position.z);

        // marker & gating
        BuildCurrentMarker();
        MoveCurrentMarkerToCurrent();

        // keyboard ring
        BuildFocusRing();
        FocusCurrentOrChildren();
    }
}
