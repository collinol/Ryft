using System.Collections.Generic;
using System.Linq;

namespace Game.Talent
{
    public class PlayerTalentState
    {
        private HashSet<string> unlocked = new();

        public bool IsUnlocked(string nodeId) => unlocked.Contains(nodeId);

        public bool TryUnlock(TalentNodeDef node)
        {
            if (node == null) return false;
            if (node.prerequisites.Any(p => p && !IsUnlocked(p.id))) return false;
            unlocked.Add(node.id);
            return true;
        }

        public IEnumerable<string> CollectUnlockedAbilityIds(IEnumerable<TalentNodeDef> allNodes)
        {
            foreach (var n in allNodes)
                if (IsUnlocked(n.id))
                    foreach (var a in n.unlockAbilityIds)
                        yield return a;
        }
    }
}
