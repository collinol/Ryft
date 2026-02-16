using System;
using Game.Equipment;

namespace Game.TimePortal
{
    /// <summary>
    /// Represents equipment borrowed from a future version of the player.
    /// </summary>
    [Serializable]
    public class BorrowedGear
    {
        public string equipmentId;         // ID of the borrowed equipment
        public int borrowedAtLevel;        // Map level where it was borrowed
        public int mustReturnByLevel;      // Level by which obligations must be met
        public bool obligationsMet;        // True if all obligations fulfilled
        public int borrowWorldLevel;       // WorldLevel when this was borrowed

        public BorrowedGear(string equipId, int currentLevel, int worldLevel, int futureLevels = 3)
        {
            equipmentId = equipId;
            borrowedAtLevel = currentLevel;
            mustReturnByLevel = currentLevel + futureLevels;
            obligationsMet = false;
            borrowWorldLevel = worldLevel;
        }

        /// <summary>
        /// Check if the gear should be taken away (failed obligations).
        /// </summary>
        public bool ShouldExpire(int currentLevel)
        {
            return !obligationsMet && currentLevel >= mustReturnByLevel;
        }
    }
}
