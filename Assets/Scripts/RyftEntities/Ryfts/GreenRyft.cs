using UnityEngine;
using Game.Ryfts;

namespace Game.RyftEntities
{
    /// <summary>
    /// Green Ryft portal entity for Portal Fight scenes.
    /// </summary>
    public class GreenRyft : RyftPortalEntity
    {
        public override RyftColor RyftColor => RyftColor.Green;

        protected override void Awake()
        {
            displayName = "Green Rift Portal";
            base.Awake();
        }

        protected override void Start()
        {
            LoadRyftSprite();
            base.Start();
        }
    }
}
