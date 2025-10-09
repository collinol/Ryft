using UnityEngine;

public static class MapBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureMapSession()
    {
        if (MapSession.I != null) return;

        var go = new GameObject("MapSession (auto)");
        go.AddComponent<MapSession>();

    }
}
