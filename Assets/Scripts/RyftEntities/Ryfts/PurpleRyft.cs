using UnityEngine;
using Game.Ryfts;

namespace Game.RyftEntities
{
    /// <summary>
    /// Purple Ryft portal entity for Portal Fight scenes.
    /// </summary>
    public class PurpleRyft : RyftPortalEntity
    {
        public override RyftColor RyftColor => RyftColor.Purple;

        protected override void Awake()
        {
            displayName = "Purple Rift Portal";
            base.Awake();
        }

        protected override void Start()
        {
            LoadRyftSprite();
            base.Start();
        }
    }
}
