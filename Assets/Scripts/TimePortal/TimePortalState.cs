using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Equipment;

namespace Game.TimePortal
{
    /// <summary>
    /// Tracks borrowed gear and obligations for the Time Portal system.
    /// </summary>
    [Serializable]
    public class TimePortalState
    {
        public List<BorrowedGear> borrowedGear = new();
        public List<TimeObligation> obligations = new();
        public bool hasVisitedTimePortal = false;
        public int lastVisitLevel = -1;

        /// <summary>
        /// Check and handle any expired borrowed gear.
        /// Returns list of equipment IDs that were removed.
        /// </summary>
        public List<string> CheckExpiredGear(int currentLevel)
        {
            var removed = new List<string>();

            for (int i = borrowedGear.Count - 1; i >= 0; i--)
            {
                var gear = borrowedGear[i];
                if (gear.ShouldExpire(currentLevel))
                {
                    Debug.Log($"[TimePortal] Borrowed gear {gear.equipmentId} expired - obligations not met!");
                    removed.Add(gear.equipmentId);
                    borrowedGear.RemoveAt(i);
                }
            }

            return removed;
        }

        /// <summary>
        /// Add new borrowed gear and create obligations.
        /// </summary>
        public void BorrowGear(string equipId, int currentLevel, string eliteType = null)
        {
            var gear = new BorrowedGear(equipId, currentLevel);
            borrowedGear.Add(gear);

            // Create obligations: defeat elite at level N+2, visit portal at N+3
            if (!string.IsNullOrEmpty(eliteType))
            {
                obligations.Add(TimeObligation.CreateDefeatElite(eliteType, currentLevel + 2));
            }
            obligations.Add(TimeObligation.CreateVisitPortal(currentLevel + 3));

            Debug.Log($"[TimePortal] Borrowed {equipId}, must meet obligations by level {gear.mustReturnByLevel}");
        }

        /// <summary>
        /// Mark an elite defeat and check if it completes any obligations.
        /// </summary>
        public void OnEliteDefeated(string eliteType, int currentLevel)
        {
            foreach (var obl in obligations)
            {
                if (obl.type == TimeObligation.ObligationType.DefeatElite &&
                    obl.targetId == eliteType &&
                    obl.targetLevel == currentLevel &&
                    !obl.completed)
                {
                    obl.completed = true;
                    Debug.Log($"[TimePortal] Obligation completed: Defeated {eliteType}!");
                    CheckAllObligationsMet();
                }
            }
        }

        /// <summary>
        /// Mark a time portal visit and check obligations.
        /// </summary>
        public void OnTimePortalVisited(int currentLevel)
        {
            lastVisitLevel = currentLevel;
            hasVisitedTimePortal = true;

            foreach (var obl in obligations)
            {
                if (obl.type == TimeObligation.ObligationType.VisitTimePortal &&
                    obl.targetLevel == currentLevel &&
                    !obl.completed)
                {
                    obl.completed = true;
                    Debug.Log($"[TimePortal] Obligation completed: Visited Time Portal at level {currentLevel}!");
                    CheckAllObligationsMet();
                }
            }
        }

        private void CheckAllObligationsMet()
        {
            foreach (var gear in borrowedGear)
            {
                if (gear.obligationsMet) continue;

                // Check if all obligations for this gear are met
                bool allMet = true;
                foreach (var obl in obligations)
                {
                    if (!obl.completed)
                    {
                        allMet = false;
                        break;
                    }
                }

                if (allMet)
                {
                    gear.obligationsMet = true;
                    Debug.Log($"[TimePortal] All obligations met! {gear.equipmentId} is permanently yours!");
                }
            }
        }

        /// <summary>
        /// Get pending (incomplete) obligations.
        /// </summary>
        public List<TimeObligation> GetPendingObligations()
        {
            var pending = new List<TimeObligation>();
            foreach (var obl in obligations)
            {
                if (!obl.completed)
                    pending.Add(obl);
            }
            return pending;
        }

        /// <summary>
        /// Check if there's any borrowed gear with pending obligations.
        /// </summary>
        public bool HasPendingObligations()
        {
            foreach (var gear in borrowedGear)
            {
                if (!gear.obligationsMet)
                    return true;
            }
            return false;
        }
    }
}
