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
        /// Obligations are now WorldLevel-based: visit TimePortal on the next map.
        /// </summary>
        public void BorrowGear(string equipId, int currentLevel, string eliteType = null)
        {
            int worldLevel = MapSession.I != null ? MapSession.I.WorldLevel : 0;
            var gear = new BorrowedGear(equipId, currentLevel, worldLevel);
            borrowedGear.Add(gear);

            // Create obligation: defeat the highlighted obligation elite on the next map
            if (!string.IsNullOrEmpty(eliteType))
            {
                var oblElite = TimeObligation.CreateDefeatElite(eliteType, -1);
                oblElite.borrowWorldLevel = worldLevel;
                obligations.Add(oblElite);
            }

            // Create obligation: visit TimePortal on the next map (WorldLevel-based)
            var oblPortal = TimeObligation.CreateVisitPortalOnWorld(worldLevel + 1);
            oblPortal.borrowWorldLevel = worldLevel;
            obligations.Add(oblPortal);

            Debug.Log($"[TimePortal] Borrowed {equipId} on WorldLevel {worldLevel}, must defeat obligation elite and visit portal on WorldLevel {worldLevel + 1}");
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
        /// Only completes portal visit obligations for the relevant borrow event,
        /// and only if all DefeatElite obligations for that borrow are already met.
        /// </summary>
        public void OnTimePortalVisited(int currentLevel)
        {
            lastVisitLevel = currentLevel;
            hasVisitedTimePortal = true;

            int worldLevel = MapSession.I != null ? MapSession.I.WorldLevel : 0;
            // The TimePortal on WorldLevel N closes the loop from WorldLevel N-1
            int targetBorrowWorld = worldLevel - 1;

            // Check that all DefeatElite obligations for this borrow are done
            bool allElitesDefeated = true;
            foreach (var obl in obligations)
            {
                if (obl.borrowWorldLevel == targetBorrowWorld &&
                    obl.type == TimeObligation.ObligationType.DefeatElite &&
                    !obl.completed)
                {
                    allElitesDefeated = false;
                    break;
                }
            }

            if (!allElitesDefeated)
            {
                Debug.Log($"[TimePortal] Portal visited but DefeatElite obligations for borrow world {targetBorrowWorld} still pending.");
                return;
            }

            foreach (var obl in obligations)
            {
                if (obl.completed) continue;
                if (obl.borrowWorldLevel != targetBorrowWorld) continue;

                // Check WorldLevel-based portal obligation
                if (obl.type == TimeObligation.ObligationType.VisitTimePortalOnWorld &&
                    obl.targetValue == worldLevel)
                {
                    obl.completed = true;
                    Debug.Log($"[TimePortal] Obligation completed: Visited Time Portal on WorldLevel {worldLevel}!");
                    CheckAllObligationsMet();
                }

                // Legacy: level-based portal obligation
                if (obl.type == TimeObligation.ObligationType.VisitTimePortal &&
                    obl.targetLevel == currentLevel)
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

                // Check if all obligations for THIS gear's borrow event are met
                bool allMet = true;
                foreach (var obl in obligations)
                {
                    if (obl.borrowWorldLevel == gear.borrowWorldLevel && !obl.completed)
                    {
                        allMet = false;
                        break;
                    }
                }

                if (allMet)
                {
                    gear.obligationsMet = true;
                    Debug.Log($"[TimePortal] All obligations met for WorldLevel {gear.borrowWorldLevel}! {gear.equipmentId} is permanently yours!");
                }
            }
        }

        /// <summary>
        /// Get obligations for a specific borrow event (by WorldLevel).
        /// </summary>
        public List<TimeObligation> GetObligationsForBorrow(int borrowWorldLevel)
        {
            var result = new List<TimeObligation>();
            foreach (var obl in obligations)
            {
                if (obl.borrowWorldLevel == borrowWorldLevel)
                    result.Add(obl);
            }
            return result;
        }

        /// <summary>
        /// Get the borrowed gear from a specific WorldLevel.
        /// </summary>
        public BorrowedGear GetBorrowedGearForWorld(int borrowWorldLevel)
        {
            foreach (var gear in borrowedGear)
            {
                if (gear.borrowWorldLevel == borrowWorldLevel)
                    return gear;
            }
            return null;
        }

        /// <summary>
        /// Handle a failed time loop: player didn't select the borrowed item at the obligation elite.
        /// Removes the borrowed gear entry and its obligations.
        /// Returns the equipment ID that should be removed from the player.
        /// </summary>
        public string FailLoop(int borrowWorldLevel)
        {
            string removedEquipId = null;

            // Remove the borrowed gear entry
            for (int i = borrowedGear.Count - 1; i >= 0; i--)
            {
                if (borrowedGear[i].borrowWorldLevel == borrowWorldLevel)
                {
                    removedEquipId = borrowedGear[i].equipmentId;
                    Debug.Log($"[TimePortal] Loop failed for WorldLevel {borrowWorldLevel}! Removing {removedEquipId}");
                    borrowedGear.RemoveAt(i);
                }
            }

            // Remove associated obligations
            for (int i = obligations.Count - 1; i >= 0; i--)
            {
                if (obligations[i].borrowWorldLevel == borrowWorldLevel)
                {
                    obligations.RemoveAt(i);
                }
            }

            return removedEquipId;
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
