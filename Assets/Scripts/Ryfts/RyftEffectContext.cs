using Game.Core;
using Game.Cards;
using Game.Combat;

namespace Game.Ryfts
{
    /// Carries whatever a ryft effect might need at trigger time.
    public class RyftEffectContext
    {
        public FightContext fight;          // may be null on map events
        public IActor source;               // usually player
        public IActor target;               // enemy or self
        public CardDef cardDef;       // card involved (if any)
        public int amount;                  // dmg/heal amount (if relevant)
        public RyftTrigger trigger;
    }
}
