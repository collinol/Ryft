using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Ryfts;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class MapNode : MonoBehaviour
{
    [Header("Identity")]
    public MapNodeType type;

    [Header("Graph")]
    public List<MapNode> connections = new();  // outgoing edges

    [Header("Discovery/Reachability")]
    public bool isDiscovered;
    public bool isReachable = true;
    public bool visited = false;

    [Header("Rift (only if type == Rift)")]
    public RyftColor ryftColor;
    public RiftState riftState = RiftState.Open;

    [Header("Visual")]
    [Tooltip("Target max dimension (world units) for the sprite after auto-scaling.")]
    public float targetWorldSize = 0.9f;
    public float zOffset = 0f;
    [Range(0.1f,1f)] public float visitedAlpha = 0.5f;

    [Header("Interaction / Hitbox")]
    [Tooltip("Multiply the collider size by this to make clicking easier.")]
    public float hitboxScale = 1.5f;

    [Tooltip("Minimum collider size (world units) so tiny sprites are still easy to click).")]
    public Vector2 minHitbox = new Vector2(0.7f, 0.7f);

    private SpriteRenderer sr;
    private Collider2D col;
    private MapController controller;

    [Header("Clicking")]
    [Tooltip("Invisible hitbox around the node for easy clicking.")]
    public float clickRadius = 0.4f;   // tweak to taste

    [Header("Sizing")]
    [Tooltip("How wide (world units) the icon should appear, regardless of sprite PPU.")]
    public float desiredWorldDiameter = 1.1f;   // tweak to taste
    [Tooltip("Extra pad added to hitbox radius so clicking is easy.")]
    public float clickPadding = 0.25f;

    [SerializeField] SpriteRenderer icon;       // assign in prefab, or we’ll find one
    [SerializeField] CircleCollider2D hitbox;

    void Reset()
    {
        if (!icon)  icon  = GetComponentInChildren<SpriteRenderer>();
        if (!hitbox) hitbox = GetComponent<CircleCollider2D>();
    }

    void Awake()
    {
        if (!icon)  icon  = GetComponentInChildren<SpriteRenderer>();
        if (!hitbox)
        {
            hitbox = GetComponent<CircleCollider2D>();
            if (!hitbox) hitbox = gameObject.AddComponent<CircleCollider2D>();
        }
        hitbox.isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("MapNode"); // make sure this layer exists in Project Settings > Tags & Layers
    }

    public void RefreshVisualSize(float? diameterOverride = null)
    {
        if (!icon || !icon.sprite) return;

        // Prefer tight bounds to ignore transparent borders (see import tips below).
        var localSize = icon.sprite.bounds.size;    // in local units at scale=1
        float maxDim = Mathf.Max(localSize.x, localSize.y);
        if (maxDim <= 0f) return;

        float target = diameterOverride ?? desiredWorldDiameter;
        float scale  = target / maxDim;
        icon.transform.localScale = new Vector3(scale, scale, 1f);

        // Make clicking forgiving
        if (hitbox)
        {
            hitbox.radius = (target * 0.5f) + clickPadding;
            hitbox.isTrigger = true;
        }
    }

    // -------- init / binding ----------
    public void Init(MapNodeType t, Sprite nonRiftSprite, MapController owner)
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        controller = owner;
        type = t;
        isDiscovered = false;
        isReachable = true;
        visited = false;

        if (t == MapNodeType.Rift)
        {
            int colorCount = System.Enum.GetValues(typeof(RyftColor)).Length;
            ryftColor = (RyftColor)Random.Range(0, colorCount);
            riftState = RiftState.Open;
            ApplySprite(controller.GetRiftSprite(ryftColor, riftState));
        }
        else
        {
            ApplySprite(nonRiftSprite);
        }

        var p = transform.localPosition;
        transform.localPosition = new Vector3(p.x, p.y, zOffset);

        UpdateVisibility();
        UpdateInteractable();
    }

    private void ApplySprite(Sprite s)
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();

        // If the load failed, DO NOT clear the sprite. Keep whatever is currently shown.
        if (s == null)
        {
            // Still refit collider to current visual
            NormalizeSize();
            RefitCollider();
            return;
        }

        sr.sprite = s;
        NormalizeSize();
        RefitCollider();
    }


    private void NormalizeSize()
    {
        if (!sr || !sr.sprite) return;
        var size = sr.sprite.bounds.size; // world size at scale=1
        float maxSide = Mathf.Max(size.x, size.y);
        if (maxSide <= 0.0001f) return;
        float scale = targetWorldSize / maxSide;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void RefitCollider()
    {
        if (!col || !sr || !sr.sprite) return;

        // sprite world size with current transform scale
        Vector2 spriteWorldSize = Vector2.Scale(sr.sprite.bounds.size, (Vector2)transform.localScale);

        // desired hitbox (apply padding + minimums)
        float w = Mathf.Max(spriteWorldSize.x * hitboxScale, minHitbox.x);
        float h = Mathf.Max(spriteWorldSize.y * hitboxScale, minHitbox.y);

        if (col is CircleCollider2D cc)
        {
            float r = Mathf.Max(w, h) * 0.5f;
            cc.radius = r;
            cc.offset = Vector2.zero;
            cc.isTrigger = false;
        }
        else if (col is BoxCollider2D bc)
        {
            bc.size = new Vector2(w, h);
            bc.offset = Vector2.zero;
            bc.isTrigger = false;
        }
        else if (col is CapsuleCollider2D cap)
        {
            cap.size = new Vector2(w, h);
            cap.direction = (w >= h) ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
            cap.offset = Vector2.zero;
            cap.isTrigger = false;
        }
        // If you use a different Collider2D, add handling here similarly.
    }

    // ───────── reachability helpers (with/without exploding rifts) ─────────
    public void SetReachable(bool val) => SetReachableDefault(val);

    public void SetReachableDefault(bool val) => SetReachable(val, true);   // preserves old behavior
    public void SetReachableSilently(bool val) => SetReachable(val, false); // no exploding when disabling

    public void SetReachable(bool val, bool explodeWhenFalse)
    {
        isReachable = val;

        if (!isReachable && explodeWhenFalse && type == MapNodeType.Rift && riftState == RiftState.Open)
        {
            riftState = RiftState.Exploded;
            ApplySprite(controller.GetRiftSprite(ryftColor, riftState));
        }

        UpdateVisibility();
        UpdateInteractable();
    }

    public void Discover()
    {
        isDiscovered = true;
        UpdateVisibility();
        UpdateInteractable();
    }

    public void MarkVisited()
    {
        visited = true;
        UpdateVisibility();
        UpdateInteractable();
    }

    public void SetRiftState(RiftState state)
    {
        if (type != MapNodeType.Rift) return;

        // Keep collider enabled through the swap so the click never feels "lost".
        bool wasEnabled = false;
        if (col) { wasEnabled = col.enabled; col.enabled = true; }

        riftState = state;
        var newSprite = controller.GetRiftSprite(ryftColor, riftState);
        ApplySprite(newSprite);

        if (col) col.enabled = wasEnabled;

        UpdateVisibility();
        UpdateInteractable();
    }


    private void UpdateVisibility()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        var c = sr.color;

        c.a = isDiscovered ? 1f : 0f;
        if (isDiscovered && !isReachable) c.a *= 0.6f;
        if (visited) c.a *= visitedAlpha;

        sr.color = c;
    }

    private void UpdateInteractable()
    {
        if (!col) col = GetComponent<Collider2D>();
        col.enabled = isDiscovered && isReachable && !visited;
    }

    private void OnMouseDown()
    {
        Activate();
    }

    public void Activate()
    {
        if (!isDiscovered || !isReachable || visited) return;

        if (type == MapNodeType.Rift)
        {
            // DO NOT change rift state here - wait for fight outcome
            // DO NOT call OnNodeChosen here - wait for fight outcome
            // DO NOT mark visited here - wait for fight outcome

            // Ensure MapSession exists
            if (MapSession.I == null)
            {
                new GameObject("MapSession (auto)").AddComponent<MapSession>();
            }

            // Save map state before transitioning
            var st = controller.BuildState();
            MapSession.I.Saved = st;

            // Store which rift node we're fighting so we can update it on return
            MapSession.I.PortalFightLevel = controller.FindLevelOfNode(this);
            MapSession.I.PortalFightIndex = controller.FindIndexInLevel(this);
            MapSession.I.PortalFightRyftColor = ryftColor;  // Pass color to portal fight scene

            SceneManager.LoadScene("PortalFight", LoadSceneMode.Single);
            return;
        }

        if (type == MapNodeType.Enemy || type == MapNodeType.Elite)
        {
            // mirror mouse-click path: choose node, save state, load scene
            controller?.OnNodeChosen(this);

            if (MapSession.I == null)
            {
                new GameObject("MapSession (auto)").AddComponent<MapSession>();
            }

            var st = controller.BuildState();
            MapSession.I.Saved = st;
            MapSession.I.IsEliteFight = (type == MapNodeType.Elite);

            MarkVisited();
            SceneManager.LoadScene("FightScene", LoadSceneMode.Single);
            return;
        }

        if (type == MapNodeType.Shop)
        {
            controller?.OnNodeChosen(this);

            if (MapSession.I == null)
            {
                new GameObject("MapSession (auto)").AddComponent<MapSession>();
            }

            var st = controller.BuildState();
            MapSession.I.Saved = st;

            MarkVisited();
            SceneManager.LoadScene("ShopScene", LoadSceneMode.Single);
            return;
        }

        if (type == MapNodeType.Rest)
        {
            controller?.OnNodeChosen(this);

            if (MapSession.I == null)
            {
                new GameObject("MapSession (auto)").AddComponent<MapSession>();
            }

            var st = controller.BuildState();
            MapSession.I.Saved = st;

            MarkVisited();
            SceneManager.LoadScene("RestScene", LoadSceneMode.Single);
            return;
        }

        if (type == MapNodeType.TimePortal)
        {
            controller?.OnNodeChosen(this);

            if (MapSession.I == null)
            {
                new GameObject("MapSession (auto)").AddComponent<MapSession>();
            }

            var st = controller.BuildState();
            MapSession.I.Saved = st;

            MarkVisited();
            SceneManager.LoadScene("TimePortalScene", LoadSceneMode.Single);
            return;
        }

        controller?.OnNodeChosen(this);
    }


    // In case someone changes scale in the editor at runtime:
    void OnValidate()
    {
        if (Application.isPlaying) return;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (sr && sr.sprite) { NormalizeSize(); RefitCollider(); }
    }
}
