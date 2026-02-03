using UnityEngine;
using Game.Core;
using Game.UI;
using Game.Ryfts;

namespace Game.Combat
{
    /// <summary>
    /// A destructible portal that enemies will target during Portal Fight encounters.
    /// Implements IActor so it can be targeted and damaged like any other entity.
    /// </summary>
    public class RiftPortal : MonoBehaviour, IActor
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "Rift Portal";

        [Header("Stats")]
        [SerializeField] private int maxHealth = 50;

        public string DisplayName => displayName;
        public int Health { get; private set; }
        public bool IsAlive => Health > 0;

        // IActor required properties - portal has minimal stats
        public Stats BaseStats => new Stats { maxHealth = maxHealth };
        public Stats TotalStats => BaseStats;
        public StatusEffectManager StatusEffects { get; private set; }

        private HealthBarView hpBar;

        /// <summary>
        /// Event fired when the portal is destroyed (health reaches 0).
        /// </summary>
        public event System.Action OnPortalDestroyed;

        void Awake()
        {
            Health = Mathf.Max(1, maxHealth);
            StatusEffects = new StatusEffectManager(this);
        }

        void Start()
        {
            // Create health bar positioned above the portal
            hpBar = HealthBarView.Attach(transform, new Vector3(0f, 1.5f, 0f), new Vector2(1.5f, 0.2f));
            hpBar.Set(Health, maxHealth);
        }

        public void ApplyDamage(int amount)
        {
            if (!IsAlive) return;

            var mitigated = Mathf.Max(0, amount);
            bool wasAlive = IsAlive;
            Health = Mathf.Max(0, Health - mitigated);
            hpBar?.Set(Health, maxHealth);

            Debug.Log($"[RiftPortal] Took {mitigated} damage. HP: {Health}/{maxHealth}");

            if (wasAlive && !IsAlive)
            {
                OnDeath();
            }
        }

        public void Heal(int amount)
        {
            Health = Mathf.Min(maxHealth, Health + Mathf.Max(0, amount));
            hpBar?.Set(Health, maxHealth);
        }

        private void OnDeath()
        {
            Debug.Log("[RiftPortal] Portal destroyed!");

            // Visual feedback - hide the portal
            var sr = GetComponent<SpriteRenderer>();
            if (sr) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.3f);
            if (hpBar) hpBar.gameObject.SetActive(false);

            // Notify listeners (FightSceneController)
            OnPortalDestroyed?.Invoke();
        }

        /// <summary>
        /// Factory method to create a portal in the scene with a specific rift color.
        /// </summary>
        /// <param name="position">World position for the portal</param>
        /// <param name="health">Maximum health of the portal</param>
        /// <param name="ryftColor">The color of the rift (determines sprite)</param>
        /// <returns>The created RiftPortal instance</returns>
        public static RiftPortal Create(Vector3 position, int health, RyftColor ryftColor)
        {
            var go = new GameObject("RiftPortal");
            go.transform.position = position;

            var portal = go.AddComponent<RiftPortal>();
            portal.maxHealth = health;
            portal.Health = health;
            portal.displayName = $"{ryftColor} Rift Portal";

            // Add visual representation
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 0;

            // Load the rift sprite based on color (using Open state sprite)
            string colorName = ryftColor.ToString().ToLower();
            string spritePath = $"Map/Ryfts/{colorName}Rift";
            sr.sprite = Resources.Load<Sprite>(spritePath);

            if (sr.sprite == null)
            {
                // Try alternate naming convention
                spritePath = $"Map/Ryfts/{colorName}Open";
                sr.sprite = Resources.Load<Sprite>(spritePath);
            }

            if (sr.sprite == null)
            {
                Debug.LogWarning($"[RiftPortal] Could not load sprite at {spritePath}, using fallback");
            }
            else
            {
                Debug.Log($"[RiftPortal] Loaded sprite: {spritePath}");
            }

            // Scale the portal to be visible
            go.transform.localScale = Vector3.one * 2f;

            // Apply a subtle color tint based on rift color
            sr.color = GetColorTint(ryftColor);

            return portal;
        }

        /// <summary>
        /// Factory method to create a portal with default blue color.
        /// </summary>
        public static RiftPortal Create(Vector3 position, int health = 50)
        {
            return Create(position, health, RyftColor.Blue);
        }

        /// <summary>
        /// Get a color tint based on the rift color for visual distinction.
        /// </summary>
        private static Color GetColorTint(RyftColor color)
        {
            return color switch
            {
                RyftColor.Orange => new Color(1f, 0.8f, 0.6f, 1f),
                RyftColor.Green => new Color(0.6f, 1f, 0.7f, 1f),
                RyftColor.Blue => new Color(0.6f, 0.8f, 1f, 1f),
                RyftColor.Purple => new Color(0.9f, 0.6f, 1f, 1f),
                _ => Color.white
            };
        }

        /// <summary>
        /// Initialize the portal with specific settings.
        /// </summary>
        public void Initialize(int health)
        {
            maxHealth = health;
            Health = health;
            hpBar?.Set(Health, maxHealth);
        }
    }
}
