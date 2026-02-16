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
            DefeatElite,            // Must defeat a specific elite enemy type
            VisitTimePortal,        // Must visit a time portal at a specific level (legacy)
            WinWithoutHealing,      // Must win a fight without healing
            CollectGold,            // Must collect a certain amount of gold
            VisitTimePortalOnWorld  // Must visit a time portal on a specific WorldLevel
        }

        public ObligationType type;
        public string targetId;        // e.g., elite enemy type name
        public int targetLevel;        // Level at which this must be done
        public int targetValue;        // e.g., gold amount to collect
        public bool completed;
        public int borrowWorldLevel;   // WorldLevel when the associated borrow happened

        public string Description => type switch
        {
            ObligationType.DefeatElite => targetLevel >= 0
                ? $"Defeat {targetId} at level {targetLevel}"
                : $"Defeat the highlighted elite",
            ObligationType.VisitTimePortal => $"Return to a Time Portal at level {targetLevel}",
            ObligationType.VisitTimePortalOnWorld => $"Visit a Time Portal on map {targetValue + 1}",
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

        public static TimeObligation CreateVisitPortalOnWorld(int worldLevel)
        {
            return new TimeObligation
            {
                type = ObligationType.VisitTimePortalOnWorld,
                targetValue = worldLevel,
                completed = false
            };
        }
    }
}
