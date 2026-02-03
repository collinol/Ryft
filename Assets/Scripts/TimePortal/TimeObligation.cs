using System;

namespace Game.TimePortal
{
    /// <summary>
    /// An obligation the player must fulfill to keep borrowed equipment.
    /// </summary>
    [Serializable]
    public class TimeObligation
    {
        public enum ObligationType
        {
            DefeatElite,       // Must defeat a specific elite enemy type
            VisitTimePortal,   // Must visit a time portal at a specific level
            WinWithoutHealing, // Must win a fight without healing
            CollectGold        // Must collect a certain amount of gold
        }

        public ObligationType type;
        public string targetId;        // e.g., elite enemy type name
        public int targetLevel;        // Level at which this must be done
        public int targetValue;        // e.g., gold amount to collect
        public bool completed;

        public string Description => type switch
        {
            ObligationType.DefeatElite => $"Defeat {targetId} at level {targetLevel}",
            ObligationType.VisitTimePortal => $"Return to a Time Portal at level {targetLevel}",
            ObligationType.WinWithoutHealing => "Win a fight without healing",
            ObligationType.CollectGold => $"Collect {targetValue} gold",
            _ => "Unknown obligation"
        };

        public static TimeObligation CreateDefeatElite(string eliteType, int level)
        {
            return new TimeObligation
            {
                type = ObligationType.DefeatElite,
                targetId = eliteType,
                targetLevel = level,
                completed = false
            };
        }

        public static TimeObligation CreateVisitPortal(int level)
        {
            return new TimeObligation
            {
                type = ObligationType.VisitTimePortal,
                targetLevel = level,
                completed = false
            };
        }
    }
}
