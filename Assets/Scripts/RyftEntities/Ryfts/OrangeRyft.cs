using UnityEngine;
using Game.Ryfts;

namespace Game.RyftEntities
{
    /// <summary>
    /// Orange Ryft portal entity for Portal Fight scenes.
    /// </summary>
    public class OrangeRyft : RyftPortalEntity
    {
        public override RyftColor RyftColor => RyftColor.Orange;

        protected override void Awake()
        {
            displayName = "Orange Rift Portal";
            base.Awake();
        }

        protected override void Start()
        {
            LoadRyftSprite();
            base.Start();
        }
    }
}
