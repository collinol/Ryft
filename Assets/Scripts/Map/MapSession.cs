using UnityEngine;
using Game.Ryfts;
using Game.TimePortal;

public class MapSession : MonoBehaviour
{
    public static MapSession I { get; private set; }
    public MapState Saved;  // null if no save yet

    // Portal fight outcome tracking
    public bool? PortalFightVictory = null;  // null = pending/no fight, true = won, false = lost
    public int PortalFightLevel = -1;        // Which level the rift was on
    public int PortalFightIndex = -1;        // Which index in that level
    public RyftColor PortalFightRyftColor;   // The color of the rift being fought

    // Run progression
    public int Gold = 0;                     // Currency for shop purchases
    public bool IsEliteFight = false;        // True if current/last fight was an elite
    public bool IsBossFight = false;         // True if current fight is a boss fight
    public int CurrentMapLevel = 0;          // Current map depth for scaling
    public bool PendingReward = false;       // True if player should see reward scene
    public string LastDefeatedEliteType;     // Track which elite was last defeated
    public bool ShouldAdvanceNode = false;   // True if player should move to next node after returning to map

    // Boss progression
    public int ClosedRiftCount = 0;          // Rifts closed this map (triggers boss at 3)
    public int WorldLevel = 0;               // Which map we're on (0 = first, 1 = second, etc.)
    public bool BossFightPending = false;    // Auto-transition to boss fight
    public bool NewMapPending = false;       // After boss victory, generate fresh map

    // Time Portal mode
    public enum TimePortalMode { Receive, Return }
    public TimePortalMode PortalMode = TimePortalMode.Receive;

    // Obligation elite tracking (the highlighted elite the player must defeat before TimePortal appears)
    public int ObligationEliteLevel = -1;    // Row on the map (-1 = not placed)
    public int ObligationEliteIndex = -1;    // Node index in that row
    public bool ObligationEliteDefeated = false;
    public bool IsObligationEliteFight = false; // Set when entering the obligation elite fight

    // Time Portal system
    public TimePortalState TimePortal;       // Tracks borrowed gear and obligations

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Add gold from combat victory.
    /// </summary>
    public void AddGold(int amount)
    {
        // Ryft passive: gold gain percent modifier
        if (amount > 0)
        {
            var mgr = Game.Ryfts.RyftEffectManager.Instance;
            if (mgr != null)
            {
                float goldPct = mgr.SumFloat(Game.Ryfts.BuiltInOp.GoldGainPercent);
                if (Mathf.Abs(goldPct) > 0.001f)
                    amount = Mathf.RoundToInt(amount * (1f + goldPct));
            }
        }
        Gold = Mathf.Max(0, Gold + amount);
        Debug.Log($"[MapSession] Gold: {Gold} (+{amount})");
    }

    /// <summary>
    /// Spend gold, returns true if successful.
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount > Gold) return false;
        Gold -= amount;
        Debug.Log($"[MapSession] Gold: {Gold} (-{amount})");
        return true;
    }

    /// <summary>
    /// Clear portal fight tracking data after it has been resolved.
    /// </summary>
    public void ClearPortalFightData()
    {
        PortalFightVictory = null;
        PortalFightLevel = -1;
        PortalFightIndex = -1;
        PortalFightRyftColor = RyftColor.Blue; // Reset to default
    }
}

// Serializable data container (no Unity components here)
[System.Serializable]
public class MapState
{
    public int currentLevel;
    public int currentIndex;
    public float camX, camY; // camera position

    public LevelState[] levels; // [level] -> row

    [System.Serializable] public class LevelState
    {
        public NodeState[] nodes; // [index] -> node
    }

    [System.Serializable] public class NodeState
    {
        public MapNodeType type;
        public float x, y;      // local position in Map scene
        public bool discovered, reachable, visited;

        // Rift-only
        public bool isRift;
        public RyftColor ryftColor;
        public RiftState riftState;

        // Obligation elite
        public bool isObligationElite;

        // outgoing edges (toLevel,toIndex)
        public int[] connToLevel;
        public int[] connToIndex;
    }
}
