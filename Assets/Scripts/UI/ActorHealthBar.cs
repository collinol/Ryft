// Assets/Scripts/UI/ActorHealthBar.cs
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    [DisallowMultipleComponent]
    public class ActorHealthBar : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField] private Vector2 size = new(1.2f, 0.15f);
        [SerializeField] private Vector3 worldOffset = new(0f, 1.5f, 0f);
        [SerializeField] private Color bgColor   = new(0.85f, 0.85f, 0.85f, 0.95f);
        [SerializeField] private Color fillColor = Color.white;

        // Built runtime objects
        private Canvas canvas;
        private Image bg;
        private Image fill;
        private Transform owner;
        private Camera cam;

        // Optional bound actor (recommended)
        private IActor boundActor;

        // ------- Static helper -------
        public static ActorHealthBar AttachTo(Transform owner, IActor actor, Vector3 offset, Vector2? sizeOverride = null)
        {
            // Reuse if already present
            var existing = owner.GetComponentInChildren<ActorHealthBar>(true);
            if (!existing)
            {
                var go = new GameObject("HealthBar", typeof(RectTransform));
                go.transform.SetParent(owner, false);
                existing = go.AddComponent<ActorHealthBar>();
            }
            existing.Initialize(owner, actor, offset, sizeOverride);
            return existing;
        }

        // ------- Lifecycle -------
        public void Initialize(Transform owner, IActor actor, Vector3 offset, Vector2? sizeOverride)
        {
            this.owner = owner;
            this.boundActor = actor;
            this.worldOffset = offset;
            if (sizeOverride.HasValue) this.size = sizeOverride.Value;

            cam = Camera.main;
            BuildIfNeeded();
            UpdateFrom(boundActor); // snap visual immediately
        }

        void BuildIfNeeded()
        {
            if (canvas) return;

            // world-space canvas
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100f;

            // Load default UI sprites (so "Filled" works properly)
            var bgSprite   = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            var fillSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

            // Background
            var bgGO = new GameObject("BG", typeof(RectTransform));
            bgGO.transform.SetParent(transform, false);
            bg = bgGO.AddComponent<Image>();
            bg.sprite = bgSprite;
            bg.type   = Image.Type.Sliced;     // nice rounded track
            bg.color  = bgColor;
            bg.raycastTarget = false;
            bg.rectTransform.sizeDelta = size;

            // Fill (drive fillAmount directly)
            var fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(bg.rectTransform, false);
            fill = fillGO.AddComponent<Image>();
            fill.sprite = fillSprite;
            fill.type   = Image.Type.Filled;   // <-- requires a sprite
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.color  = fillColor;
            fill.raycastTarget = false;

            var frt = fill.rectTransform;
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;
        }


        void LateUpdate()
        {
            if (!owner) return;
            if (!cam) cam = Camera.main;

            // follow + face camera
            transform.position = owner.position + worldOffset;
            if (cam) transform.forward = cam.transform.forward;
            if (canvas && canvas.worldCamera != cam && cam) canvas.worldCamera = cam;

            // live mirror if bound
            if (boundActor != null) UpdateFrom(boundActor);
        }

        // ------- Public API -------
        /// Mirror from an actor (preferred).
        public void UpdateFrom(IActor actor)
        {

            if (!fill || actor == null) return;
            int max = Mathf.Max(1, actor.TotalStats.maxHealth);
            int hp  = Mathf.Clamp(actor.Health, 0, max);
            fill.fillAmount = (float)hp / max;
            Debug.Log($"updating health for {actor} to {fill.fillAmount}");
            if (canvas) canvas.enabled = hp > 0;
        }

        /// Use these if you don’t bind to an actor (standalone mode).
        public void SetMaxAndCurrent(int max, int current)
        {
            if (!fill) return;
            max = Mathf.Max(1, max);
            current = Mathf.Clamp(current, 0, max);
            fill.fillAmount = (float)current / max;
            if (canvas) canvas.enabled = current > 0;
        }
        public void TakeDamage(int current, int max) => SetMaxAndCurrent(max, current);
        public void Heal(int current, int max)      => SetMaxAndCurrent(max, current);
    }
}
