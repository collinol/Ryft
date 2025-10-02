// Assets/Scripts/UI/WorldHealthBar.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class WorldHealthBar : MonoBehaviour
    {
        [Header("Binding")]
        public IActor actor;
        public Transform anchor;
        public Camera cam;
        public Vector3 worldOffset = new(0f, 1.5f, 0f);

        [Header("UI")]
        [SerializeField] private Slider slider;   // existing slider in your prefab

        RectTransform rt;
        Image fillImage;      // the image we’ll actually drive
        bool isBound;

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            if (!slider) slider = GetComponentInChildren<Slider>(true);
            SetupFillImageFromSlider();
            // hide until bound so you never see a 0 state flash
            if (fillImage) fillImage.canvasRenderer.SetAlpha(0f);
            isBound = false;
        }

        void SetupFillImageFromSlider()
        {
            if (!slider) return;
            var fillRect = slider.fillRect;
            if (!fillRect) return;

            fillImage = fillRect.GetComponent<Image>();
            if (!fillImage) fillImage = fillRect.gameObject.AddComponent<Image>(); // just in case

            // Ensure the image can be driven by fillAmount
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0; // left to right
            fillImage.fillAmount = 1f; // default visible when we fade in
            fillImage.raycastTarget = false;

            // We won’t use the slider’s visual math; keep it harmless
            slider.wholeNumbers = true;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.SetValueWithoutNotify(1);
            slider.interactable = false;
        }

        public void Bind(IActor actor, Transform anchor, Camera cam)
        {
            this.actor  = actor;
            this.anchor = anchor;
            this.cam    = cam;

            isBound = (actor != null && anchor != null && cam != null && slider != null && fillImage != null);
            if (!isBound) return;

            // Snap now and also after layout finishes
            SyncImmediate();
            StartCoroutine(SyncEndOfFrame());
        }

        IEnumerator SyncEndOfFrame()
        {
            yield return null;    // wait one frame to let all Start/layout settle
            SyncImmediate();
            // fade in instantly (we already have correct fill)
            if (fillImage) fillImage.canvasRenderer.SetAlpha(1f);
        }

        void SyncImmediate()
        {
            if (!isBound) return;

            int max = Mathf.Max(1, actor.TotalStats.maxHealth);
            int hp  = Mathf.Clamp(actor.Health, 0, max);
            float frac = (float)hp / max;

            // Drive the fill image directly — avoids all Slider quirks
            fillImage.fillAmount = frac;

            // Also keep slider book-keeping sane for any external UI that queries it
            slider.maxValue = max;
            slider.SetValueWithoutNotify(hp);
        }

        void OnEnable()
        {
            if (isBound) SyncImmediate();
        }

        void LateUpdate()
        {
            if (!isBound) return;

            // Keep the UI element over the world anchor
            Vector3 worldPos  = anchor.position + worldOffset;
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
            rt.position = screenPos;

            // Drive values each frame
            int max = Mathf.Max(1, actor.TotalStats.maxHealth);
            int hp  = Mathf.Clamp(actor.Health, 0, max);
            float frac = (float)hp / max;

            if (fillImage.fillAmount != frac) fillImage.fillAmount = frac;

            // Keep slider values consistent (in case other code reads it)
            if (slider.maxValue != max) slider.maxValue = max;
            if ((int)slider.value != hp) slider.SetValueWithoutNotify(hp);
        }
    }
}
