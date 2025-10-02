using System.Collections.Generic;
using UnityEngine;

namespace Game.Talent
{
    [CreateAssetMenu(menuName = "Game/Talent Node", fileName = "Talent_")]
    public class TalentNodeDef : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public List<string> unlockAbilityIds = new();  // e.g., ["shoot"] or ["medkit"]
        public List<TalentNodeDef> prerequisites = new();
    }
}
