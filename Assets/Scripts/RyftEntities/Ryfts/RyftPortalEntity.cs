using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.UI;
using Game.Ryfts;

namespace Game.RyftEntities
{
    /// <summary>
    /// Base class for Ryft portal entities that can be spawned in Portal Fight scenes.
    /// Implements IActor so enemies can target and damage it.
    /// </summary>
    public abstract class RyftPortalEntity : MonoBehaviour, IActor
    {
        [Header("Identity")]
        [SerializeField] protected string displayName = "Rift Portal";

        [Header("Stats")]
        [SerializeField] protected int maxHealth = 50;

        [Header("Visual")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected float targetWorldSize = 1.5f; // Target size in world units

        public string DisplayName => displayName;
        public int Health { get; protected set; }
        public bool IsAlive => Health > 0;

        // IActor required properties - portal only has health
        public Stats BaseStats => new Stats { maxHealth = maxHealth };
        public Stats TotalStats => BaseStats;
        public StatusEffectManager StatusEffects { get; protected set; }

        protected HealthBarView hpBar;

        /// <summary>
        /// The color of this ryft portal.
        /// </summary>
        public abstract RyftColor RyftColor { get; }

        /// <summary>
        /// Event fired when the portal is destroyed (health reaches 0).
        /// </summary>
        public event System.Action OnPortalDestroyed;

        protected virtual void Awake()
        {
            Health = Mathf.Max(1, maxHealth);
            StatusEffects = new StatusEffectManager(this);

            if (!spriteRenderer)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected virtual void Start()
        {
            // Normalize sprite size to target world size
            NormalizeSpriteSize();

            // Create health bar positioned above the portal (smaller, appropriate size)
            hpBar = HealthBarView.Attach(transform, new Vector3(0f, 0.9f, 0f), new Vector2(1.2f, 0.15f));
            hpBar.Set(Health, maxHealth);
        }

        /// <summary>
        /// Normalize the sprite to fit within the target world size.
        /// </summary>
        protected void NormalizeSpriteSize()
        {
            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
            if (!spriteRenderer || !spriteRenderer.sprite) return;

            var bounds = spriteRenderer.sprite.bounds.size;
            float maxDim = Mathf.Max(bounds.x, bounds.y);
            if (maxDim <= 0.001f) return;

            float scale = targetWorldSize / maxDim;
            transform.localScale = new Vector3(scale, scale, 1f);

            Debug.Log($"[{GetType().Name}] Normalized sprite from {maxDim} to {targetWorldSize} (scale: {scale})");
        }

        public virtual void ApplyDamage(int amount)
        {
            if (!IsAlive) return;

            var mitigated = Mathf.Max(0, amount);
            bool wasAlive = IsAlive;
            Health = Mathf.Max(0, Health - mitigated);
            hpBar?.Set(Health, maxHealth);

            Debug.Log($"[{GetType().Name}] Took {mitigated} damage. HP: {Health}/{maxHealth}");

            if (wasAlive && !IsAlive)
            {
                OnDeath();
            }
        }

        public virtual void Heal(int amount)
        {
            Health = Mathf.Min(maxHealth, Health + Mathf.Max(0, amount));
            hpBar?.Set(Health, maxHealth);
        }

        protected virtual void OnDeath()
        {
            Debug.Log($"[{GetType().Name}] Portal destroyed!");

            // Visual feedback - fade the portal
            if (spriteRenderer)
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.3f);

            if (hpBar)
                hpBar.gameObject.SetActive(false);

            // Notify listeners (FightSceneController)
            OnPortalDestroyed?.Invoke();
        }

        /// <summary>
        /// Initialize the portal with specific health.
        /// </summary>
        public void Initialize(int health)
        {
            maxHealth = health;
            Health = health;
            hpBar?.Set(Health, maxHealth);
        }

        /// <summary>
        /// Load and apply the sprite for this ryft color.
        /// </summary>
        protected void LoadRyftSprite()
        {
            if (!spriteRenderer)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (!spriteRenderer)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingLayerName = "Default";
                spriteRenderer.sortingOrder = 0;
            }

            string colorName = RyftColor.ToString().ToLower();
            string spritePath = $"Map/Ryfts/{colorName}Rift";
            spriteRenderer.sprite = Resources.Load<Sprite>(spritePath);

            if (spriteRenderer.sprite == null)
            {
                // Try alternate naming
                spritePath = $"Map/Ryfts/{colorName}Open";
                spriteRenderer.sprite = Resources.Load<Sprite>(spritePath);
            }

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Could not load sprite at {spritePath}");
            }
            else
            {
                Debug.Log($"[{GetType().Name}] Loaded sprite: {spritePath}");
            }

            // Apply color tint
            spriteRenderer.color = GetColorTint();
        }

        /// <summary>
        /// Get a color tint based on the rift color for visual distinction.
        /// </summary>
        protected Color GetColorTint()
        {
            return RyftColor switch
            {
                RyftColor.Orange => new Color(1f, 0.8f, 0.6f, 1f),
                RyftColor.Green => new Color(0.6f, 1f, 0.7f, 1f),
                RyftColor.Blue => new Color(0.6f, 0.8f, 1f, 1f),
                RyftColor.Purple => new Color(0.9f, 0.6f, 1f, 1f),
                _ => Color.white
            };
        }
    }
}
