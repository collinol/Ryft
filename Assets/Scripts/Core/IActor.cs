using UnityEngine;
using Game.Combat;

namespace Game.Core
{
    public interface IActor
    {
        string DisplayName { get; }
        bool IsAlive { get; }
        int Health { get; }
        Stats BaseStats { get; }
        Stats TotalStats { get; }
        StatusEffectManager StatusEffects { get; }

        void ApplyDamage(int amount);
        void Heal(int amount);
    }
}
