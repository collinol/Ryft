using UnityEngine;

namespace Game.Core
{
    public interface IActor
    {
        string DisplayName { get; }
        bool IsAlive { get; }
        int Health { get; }
        Stats BaseStats { get; }
        Stats TotalStats { get; }

        void ApplyDamage(int amount);
        void Heal(int amount);
    }
}
