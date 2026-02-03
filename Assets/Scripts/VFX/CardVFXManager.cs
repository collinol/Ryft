using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.VFX
{
    /// <summary>
    /// Manages visual effects for card abilities
    /// </summary>
    public class CardVFXManager : MonoBehaviour
    {
        public static CardVFXManager Instance { get; private set; }

        [Header("Effect Settings")]
        [SerializeField] private float defaultEffectDuration = 0.5f;
        [SerializeField] private AnimationCurve punchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // VFX pools for reusable objects
        private readonly Queue<GameObject> effectPool = new Queue<GameObject>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Play a damage hit effect on a target
        /// </summary>
        public void PlayDamageEffect(Transform target, Color color, int damageAmount)
        {
            if (target == null) return;
            StartCoroutine(DamageEffectCoroutine(target, color, damageAmount));
        }

        /// <summary>
        /// Play a heal effect on a target
        /// </summary>
        public void PlayHealEffect(Transform target, int healAmount)
        {
            if (target == null) return;
            StartCoroutine(HealEffectCoroutine(target, healAmount));
        }

        /// <summary>
        /// Play a buff effect on a target
        /// </summary>
        public void PlayBuffEffect(Transform target, Color color)
        {
            if (target == null) return;
            StartCoroutine(BuffEffectCoroutine(target, color));
        }

        /// <summary>
        /// Play a projectile effect from source to target
        /// </summary>
        public void PlayProjectileEffect(Transform source, Transform target, Color color, System.Action onHit = null)
        {
            if (source == null || target == null) return;
            StartCoroutine(ProjectileEffectCoroutine(source, target, color, onHit));
        }

        /// <summary>
        /// Play an area effect around a position
        /// </summary>
        public void PlayAreaEffect(Vector3 position, float radius, Color color)
        {
            StartCoroutine(AreaEffectCoroutine(position, radius, color));
        }

        /// <summary>
        /// Play a status effect visual (shield, buff aura, etc)
        /// </summary>
        public void PlayStatusEffect(Transform target, StatusEffectVisualType visualType)
        {
            if (target == null) return;
            StartCoroutine(StatusEffectCoroutine(target, visualType));
        }

        // ========== Coroutines ==========

        private IEnumerator DamageEffectCoroutine(Transform target, Color color, int damageAmount)
        {
            // Create floating damage text
            CreateFloatingText(target.position + Vector3.up * 2f, $"-{damageAmount}", color, 1.5f);

            // Screen shake effect
            if (Camera.main != null)
            {
                StartCoroutine(CameraShake(0.1f, 0.1f));
            }

            // Hit flash on sprite renderer
            var renderer = target.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                Color original = renderer.color;
                renderer.color = color;
                yield return new WaitForSeconds(0.1f);
                renderer.color = original;
            }

            // Punch scale animation
            Vector3 originalScale = target.localScale;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                float punch = punchCurve.Evaluate(t) * 0.2f;
                target.localScale = originalScale * (1f + punch);
                yield return null;
            }
            target.localScale = originalScale;
        }

        private IEnumerator HealEffectCoroutine(Transform target, int healAmount)
        {
            // Create floating heal text
            CreateFloatingText(target.position + Vector3.up * 2f, $"+{healAmount}", Color.green, 1.5f);

            // Healing particles (green sparkles)
            CreateParticleEffect(target.position + Vector3.up, Color.green, 20);

            // Gentle pulse animation
            Vector3 originalScale = target.localScale;
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.5f;
                float pulse = Mathf.Sin(t * Mathf.PI) * 0.1f;
                target.localScale = originalScale * (1f + pulse);
                yield return null;
            }
            target.localScale = originalScale;
        }

        private IEnumerator BuffEffectCoroutine(Transform target, Color color)
        {
            // Create buff text
            CreateFloatingText(target.position + Vector3.up * 2f, "BUFF!", color, 1.2f);

            // Buff particles (colored sparkles)
            CreateParticleEffect(target.position + Vector3.up, color, 30);

            // Glow effect
            var renderer = target.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                Color original = renderer.color;
                float elapsed = 0f;
                while (elapsed < 0.8f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / 0.8f;
                    float glow = Mathf.Sin(t * Mathf.PI * 2) * 0.3f;
                    renderer.color = Color.Lerp(original, color, glow);
                    yield return null;
                }
                renderer.color = original;
            }
        }

        private IEnumerator ProjectileEffectCoroutine(Transform source, Transform target, Color color, System.Action onHit)
        {
            // Create projectile object
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.localScale = Vector3.one * 0.3f;
            projectile.transform.position = source.position + Vector3.up;

            var renderer = projectile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = color;
            }

            // Add trail effect
            var trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.2f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0);

            // Move projectile to target
            Vector3 startPos = projectile.transform.position;
            Vector3 endPos = target.position + Vector3.up;
            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Parabolic arc
                Vector3 linePos = Vector3.Lerp(startPos, endPos, t);
                float arc = Mathf.Sin(t * Mathf.PI) * 1.5f;
                projectile.transform.position = linePos + Vector3.up * arc;

                yield return null;
            }

            // Hit effect
            onHit?.Invoke();
            CreateParticleEffect(endPos, color, 15);

            Destroy(projectile);
        }

        private IEnumerator AreaEffectCoroutine(Vector3 position, float radius, Color color)
        {
            // Create expanding ring
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.transform.position = position;
            ring.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);

            var renderer = ring.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = color;
            }

            // Expand ring
            float elapsed = 0f;
            float duration = 0.6f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(0.1f, radius * 2f, t);
                ring.transform.localScale = new Vector3(scale, 0.01f, scale);

                // Fade out
                Color currentColor = color;
                currentColor.a = 1f - t;
                renderer.material.color = currentColor;

                yield return null;
            }

            Destroy(ring);
        }

        private IEnumerator StatusEffectCoroutine(Transform target, StatusEffectVisualType visualType)
        {
            Color effectColor = GetStatusEffectColor(visualType);
            string effectText = GetStatusEffectText(visualType);

            // Show status text
            CreateFloatingText(target.position + Vector3.up * 2f, effectText, effectColor, 1.0f);

            // Create visual indicator
            CreateParticleEffect(target.position + Vector3.up, effectColor, 25);

            yield return null;
        }

        private IEnumerator CameraShake(float duration, float magnitude)
        {
            Vector3 originalPos = Camera.main.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                Camera.main.transform.localPosition = originalPos + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            Camera.main.transform.localPosition = originalPos;
        }

        // ========== Helper Methods ==========

        private void CreateFloatingText(Vector3 position, string text, Color color, float scale)
        {
            GameObject textObj = new GameObject("FloatingText");
            textObj.transform.position = position;

            var textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 32;
            textMesh.color = color;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.transform.localScale = Vector3.one * scale;

            StartCoroutine(FloatingTextAnimation(textObj));
        }

        private IEnumerator FloatingTextAnimation(GameObject textObj)
        {
            Vector3 startPos = textObj.transform.position;
            float elapsed = 0f;
            float duration = 1.5f;

            var textMesh = textObj.GetComponent<TextMesh>();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Float upward
                textObj.transform.position = startPos + Vector3.up * (t * 1.5f);

                // Fade out
                Color color = textMesh.color;
                color.a = 1f - t;
                textMesh.color = color;

                yield return null;
            }

            Destroy(textObj);
        }

        private void CreateParticleEffect(Vector3 position, Color color, int particleCount)
        {
            for (int i = 0; i < particleCount; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.transform.position = position;
                particle.transform.localScale = Vector3.one * 0.1f;

                var renderer = particle.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Sprites/Default"));
                    renderer.material.color = color;
                }

                // Random velocity
                Vector3 velocity = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-1f, 1f)
                ).normalized * Random.Range(1f, 3f);

                StartCoroutine(ParticleAnimation(particle, velocity, color));
            }
        }

        private IEnumerator ParticleAnimation(GameObject particle, Vector3 velocity, Color color)
        {
            float elapsed = 0f;
            float lifetime = Random.Range(0.3f, 0.8f);
            Vector3 startPos = particle.transform.position;
            var renderer = particle.GetComponent<Renderer>();

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                // Physics
                particle.transform.position += velocity * Time.deltaTime;
                velocity.y -= 5f * Time.deltaTime; // Gravity

                // Fade out and shrink
                Color particleColor = color;
                particleColor.a = 1f - t;
                renderer.material.color = particleColor;
                particle.transform.localScale = Vector3.one * 0.1f * (1f - t);

                yield return null;
            }

            Destroy(particle);
        }

        private Color GetStatusEffectColor(StatusEffectVisualType visualType)
        {
            return visualType switch
            {
                StatusEffectVisualType.Shield => new Color(0.5f, 0.8f, 1f),
                StatusEffectVisualType.Buff => Color.yellow,
                StatusEffectVisualType.Debuff => new Color(1f, 0.3f, 0.3f),
                StatusEffectVisualType.Stun => new Color(0.8f, 0.8f, 0.2f),
                StatusEffectVisualType.Poison => Color.green,
                _ => Color.white
            };
        }

        private string GetStatusEffectText(StatusEffectVisualType visualType)
        {
            return visualType switch
            {
                StatusEffectVisualType.Shield => "SHIELD",
                StatusEffectVisualType.Buff => "BUFF",
                StatusEffectVisualType.Debuff => "DEBUFF",
                StatusEffectVisualType.Stun => "STUN",
                StatusEffectVisualType.Poison => "POISON",
                _ => "EFFECT"
            };
        }
    }

    /// <summary>
    /// Types of visual effects for status effects
    /// </summary>
    public enum StatusEffectVisualType
    {
        Shield,
        Buff,
        Debuff,
        Stun,
        Poison
    }

    /// <summary>
    /// Predefined colors for different damage/effect types
    /// </summary>
    public static class VFXColors
    {
        public static readonly Color Physical = new Color(1f, 0.3f, 0.2f);      // Red
        public static readonly Color Magic = new Color(0.5f, 0.3f, 1f);         // Purple
        public static readonly Color Engineering = new Color(0.2f, 0.8f, 1f);   // Cyan
        public static readonly Color Heal = new Color(0.2f, 1f, 0.3f);          // Green
        public static readonly Color Energy = new Color(1f, 0.9f, 0.2f);        // Yellow
    }
}
