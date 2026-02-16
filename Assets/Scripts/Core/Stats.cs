using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Core
{
    [Serializable]
    public struct Stats
    {
        public int maxHealth;
        public int strength;
        [FormerlySerializedAs("mana")]
        public int intellect;
        public int engineering;

        public static Stats Zero => new Stats { maxHealth = 0, strength = 0, intellect = 0, engineering = 0 };

        public static Stats operator +(Stats a, Stats b) => new Stats
        {
            maxHealth   = a.maxHealth   + b.maxHealth,
            strength    = a.strength    + b.strength,
            intellect   = a.intellect   + b.intellect,
            engineering = a.engineering + b.engineering
        };
    }
}

namespace Game.Core
{
    public enum StatField { Strength, Intellect, Engineering, MaxHealth, Energy }

    public static class StatsUtil
    {
        public static int Get(Stats s, StatField f) => f switch
        {
            StatField.MaxHealth   => s.maxHealth,
            StatField.Strength    => s.strength,
            StatField.Intellect        => s.intellect,
            StatField.Engineering => s.engineering,
            // Energy is a battle resource (not stored in Stats)
            _ => 0
        };

        public static void Set(ref Stats s, StatField f, int v)
        {
            v = Mathf.Max(0, v);
            switch (f)
            {
                case StatField.MaxHealth:   s.maxHealth   = v; break;
                case StatField.Strength:    s.strength    = v; break;
                case StatField.Intellect:        s.intellect        = v; break;
                case StatField.Engineering: s.engineering = v; break;
                // Energy is not stored in Stats
            }
        }

        public static void AddClamped(ref Stats dst, StatField f, int add, Stats cap)
        {
            int cur = Get(dst, f);
            int lim = Get(cap, f);
            Set(ref dst, f, Mathf.Min(lim, cur + Mathf.Max(0, add)));
        }
    }
}
