// e.g., Assets/Scripts/Bootstrap.cs
using UnityEngine;
using Game.Ryfts;

public class Bootstrap : MonoBehaviour
{
    void Awake()
    {
        if (!RyftEffectManager.Instance)
            new GameObject("RyftEffectManager").AddComponent<RyftEffectManager>();
    }
}
