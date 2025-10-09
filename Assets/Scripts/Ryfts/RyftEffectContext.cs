using Game.Core;
using Game.Abilities;
using Game.Combat;

namespace Game.Ryfts
{
    /// Carries whatever a ryft effect might need at trigger time.
    public class RyftEffectContext
    {
        public FightContext fight;          // may be null on map events
        public IActor source;               // usually player
        public IActor target;               // enemy or self
        public AbilityDef abilityDef;       // ability involved (if any)
        public int amount;                  // dmg/heal amount (if relevant)
        public RyftTrigger trigger;
    }
}
