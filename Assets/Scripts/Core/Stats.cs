using System;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public struct Stats
    {
        public int maxHealth;
        public int strength;
        public int defense;

        public static Stats Zero => new Stats { maxHealth = 0, strength = 0, defense = 0 };

        public static Stats operator +(Stats a, Stats b) => new Stats
        {
            maxHealth = a.maxHealth + b.maxHealth,
            strength  = a.strength  + b.strength,
            defense   = a.defense   + b.defense
        };
    }
}
