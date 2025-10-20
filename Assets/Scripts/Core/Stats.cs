// Game.Core/Stats.cs
using System;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public struct Stats
    {
        public int maxHealth;
        public int strength;
        public int mana;
        public int engineering;
        public int defense;

        public static Stats Zero => new Stats { maxHealth = 0, strength = 0, mana = 0, engineering = 0, defense = 0 };

        public static Stats operator +(Stats a, Stats b) => new Stats
        {
            maxHealth  = a.maxHealth  + b.maxHealth,
            strength   = a.strength   + b.strength,
            mana       = a.mana       + b.mana,
            engineering= a.engineering+ b.engineering,
            defense    = a.defense    + b.defense
        };
    }
}

namespace Game.Core
{
    public enum StatField { Strength, Mana, Engineering, Defense, MaxHealth }

    public static class StatsUtil
    {
        public static int Get(Stats s, StatField f) => f switch
        {
            StatField.MaxHealth   => s.maxHealth,
            StatField.Strength    => s.strength,
            StatField.Mana        => s.mana,           // NEW
            StatField.Engineering => s.engineering,
            StatField.Defense     => s.defense,
            _ => 0
        };

        public static void Set(ref Stats s, StatField f, int v)
        {
            v = Mathf.Max(0, v);
            switch (f)
            {
                case StatField.MaxHealth:   s.maxHealth   = v; break;
                case StatField.Strength:    s.strength    = v; break;
                case StatField.Mana:        s.mana        = v; break;        // NEW
                case StatField.Engineering: s.engineering = v; break;
                case StatField.Defense:     s.defense     = v; break;
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
