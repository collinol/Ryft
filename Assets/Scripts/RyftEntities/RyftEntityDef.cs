using UnityEngine;
using Game.Ryfts;

namespace Game.RyftEntities
{
    /// <summary>
    /// ScriptableObject definition for a Ryft entity.
    /// Store these in Resources/RyftEntities/ to load at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRyftEntity", menuName = "Ryft/Ryft Entity Definition")]
    public class RyftEntityDef : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("The color of this ryft")]
        public RyftColor ryftColor;

        [Tooltip("Display name for this ryft")]
        public string displayName = "Rift Portal";

        [Header("Stats")]
        [Tooltip("Maximum health of this ryft portal")]
        public int maxHealth = 50;

        [Header("Prefab")]
        [Tooltip("The prefab to instantiate for this ryft. Should have the correct color Ryft script attached.")]
        public GameObject prefab;

        [Header("Visual Fallback")]
        [Tooltip("If no prefab is assigned, this sprite will be used")]
        public Sprite fallbackSprite;

        /// <summary>
        /// Load a RyftEntityDef by color from Resources/RyftEntities/
        /// </summary>
        public static RyftEntityDef Load(RyftColor color)
        {
            string colorName = color.ToString();
            string path = $"RyftEntities/{colorName}Ryft";
            var def = Resources.Load<RyftEntityDef>(path);

            if (def == null)
            {
                Debug.LogWarning($"[RyftEntityDef] Could not load {path}, trying alternate paths...");

                // Try without "Ryft" suffix
                path = $"RyftEntities/{colorName}";
                def = Resources.Load<RyftEntityDef>(path);
            }

            if (def == null)
            {
                Debug.LogError($"[RyftEntityDef] No RyftEntityDef found for color {color}");
            }

            return def;
        }

        /// <summary>
        /// Spawn the ryft entity at the given position.
        /// </summary>
        public RyftPortalEntity Spawn(Vector3 position)
        {
            GameObject go;

            if (prefab != null)
            {
                go = Instantiate(prefab, position, Quaternion.identity);
                go.name = $"{ryftColor}RyftPortal";
            }
            else
            {
                // Create from scratch if no prefab
                go = new GameObject($"{ryftColor}RyftPortal");
                go.transform.position = position;

                // Add the correct ryft component based on color
                RyftPortalEntity entity = ryftColor switch
                {
                    RyftColor.Orange => go.AddComponent<OrangeRyft>(),
                    RyftColor.Green => go.AddComponent<GreenRyft>(),
                    RyftColor.Blue => go.AddComponent<BlueRyft>(),
                    RyftColor.Purple => go.AddComponent<PurpleRyft>(),
                    _ => go.AddComponent<BlueRyft>()
                };

                // Add SpriteRenderer
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 0;

                Debug.Log($"[RyftEntityDef] Created {ryftColor} ryft from scratch");
            }

            // Note: The RyftPortalEntity.Start() will normalize the sprite size automatically

            var portal = go.GetComponent<RyftPortalEntity>();
            if (portal != null)
            {
                portal.Initialize(maxHealth);
            }

            return portal;
        }
    }
}
