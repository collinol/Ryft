using UnityEngine;

public class MapSession : MonoBehaviour
{
    public static MapSession I { get; private set; }
    public MapState Saved;  // null if no save yet

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
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
        public RiftColor riftColor;
        public RiftState riftState;

        // outgoing edges (toLevel,toIndex)
        public int[] connToLevel;
        public int[] connToIndex;
    }
}
