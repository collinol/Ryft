using System.Collections.Generic;
using UnityEngine;

namespace Game.Talent
{
    [CreateAssetMenu(menuName = "Game/Talent Tree", fileName = "TalentTree")]
    public class TalentTreeDef : ScriptableObject
    {
        public List<TalentNodeDef> nodes = new();
    }
}
