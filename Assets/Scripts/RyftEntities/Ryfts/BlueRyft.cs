using UnityEngine;
using Game.Ryfts;

namespace Game.RyftEntities
{
    /// <summary>
    /// Blue Ryft portal entity for Portal Fight scenes.
    /// </summary>
    public class BlueRyft : RyftPortalEntity
    {
        public override RyftColor RyftColor => RyftColor.Blue;

        protected override void Awake()
        {
            displayName = "Blue Rift Portal";
            base.Awake();
        }

        protected override void Start()
        {
            LoadRyftSprite();
            base.Start();
        }
    }
}
