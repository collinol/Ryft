using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Ryfts
{
    [CreateAssetMenu(menuName="Game/Ryfts/Ryft Effect Database", fileName="RyftEffectDatabase")]
    public class RyftEffectDatabase : ScriptableObject
    {
        [SerializeField] private List<RyftEffectDef> effects = new();
        private Dictionary<string, RyftEffectDef> byId;
        private const string kResourcesPath = "Ryfts/RyftEffectDatabase"; // canonical

        public static RyftEffectDatabase Load()
        {
            var db = Resources.Load<RyftEffectDatabase>(kResourcesPath);

            db.Build();
            return db;
        }

        void OnEnable()   => Build();
        void OnValidate() => Build();

        private void Build()
        {
            byId = new Dictionary<string, RyftEffectDef>(StringComparer.OrdinalIgnoreCase);
            if (effects == null) return;
            foreach (var e in effects)
            {
                if (!e || string.IsNullOrWhiteSpace(e.id)) continue;
                if (!byId.ContainsKey(e.id)) byId.Add(e.id, e);
            }
        }

        public IReadOnlyList<RyftEffectDef> All => effects;
        public RyftEffectDef Get(string id) => (byId != null && byId.TryGetValue(id, out var e)) ? e : null;

        // Editor/Debug helper
        public void DebugDumpContents()
        {
            if (effects == null) return;
            for (int i = 0; i < effects.Count; i++)
            {
                var e = effects[i];
                if (!e) continue;

            }
        }

        // used by the importer/editor only
        public void SetEffects(List<RyftEffectDef> list) { effects = list ?? new(); Build(); }
    }
}
