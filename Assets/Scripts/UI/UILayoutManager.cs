using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Manages the overall UI layout for combat
    /// Ensures proper spacing, anchoring, and responsive design
    /// </summary>
    [ExecuteInEditMode]
    public class UILayoutManager : MonoBehaviour
    {
        [Header("Ability Bar Settings")]
        [SerializeField] private AbilityBarUI abilityBar;
        [SerializeField] private bool autoFindAbilityBar = true;

        [Header("Layout Configuration")]
        [SerializeField] private AnchorPreset abilityBarAnchor = AnchorPreset.BottomCenter;
        [SerializeField] private Vector2 abilityBarOffset = new Vector2(0, 80);
        [SerializeField] private bool centerAbilityBar = true;

        [Header("Safe Area")]
        [SerializeField] private float screenPadding = 20f;
        [SerializeField] private bool preventOverflow = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        void Start()
        {
            if (Application.isPlaying)
            {
                SetupUI();
            }
        }

        [ContextMenu("Setup UI Layout")]
        public void SetupUI()
        {
            // Find ability bar if not assigned
            if (autoFindAbilityBar && abilityBar == null)
            {
                abilityBar = FindObjectOfType<AbilityBarUI>();
            }

            if (abilityBar != null)
            {
                SetupAbilityBar();
            }

            if (showDebugInfo)
            {
                Debug.Log("[UILayoutManager] UI setup complete");
            }
        }

        private void SetupAbilityBar()
        {
            var rt = abilityBar.GetComponent<RectTransform>();
            if (rt == null) return;

            // Set anchor preset
            SetAnchor(rt, abilityBarAnchor);

            // Set position
            rt.anchoredPosition = abilityBarOffset;

            // Ensure proper pivot for centering
            if (centerAbilityBar)
            {
                rt.pivot = new Vector2(0.5f, 0.5f);
            }

            // Add Canvas Group for fade effects (optional)
            var cg = abilityBar.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = abilityBar.gameObject.AddComponent<CanvasGroup>();
            }
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            // Ensure it's not stretched
            rt.sizeDelta = Vector2.zero;

            if (showDebugInfo)
            {
                Debug.Log($"[UILayoutManager] Ability bar positioned at {rt.anchoredPosition}");
            }
        }

        private void SetAnchor(RectTransform rt, AnchorPreset preset)
        {
            switch (preset)
            {
                case AnchorPreset.BottomCenter:
                    rt.anchorMin = new Vector2(0.5f, 0f);
                    rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0.5f, 0f);
                    break;

                case AnchorPreset.BottomLeft:
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(0f, 0f);
                    rt.pivot = new Vector2(0f, 0f);
                    break;

                case AnchorPreset.BottomRight:
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                    break;

                case AnchorPreset.TopCenter:
                    rt.anchorMin = new Vector2(0.5f, 1f);
                    rt.anchorMax = new Vector2(0.5f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f);
                    break;

                case AnchorPreset.MiddleCenter:
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }
        }

        void OnValidate()
        {
            if (!Application.isPlaying && autoFindAbilityBar && abilityBar == null)
            {
                abilityBar = FindObjectOfType<AbilityBarUI>();
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (abilityBar == null) return;

            var rt = abilityBar.GetComponent<RectTransform>();
            if (rt == null) return;

            // Draw bounds
            Gizmos.color = Color.cyan;
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            }

            // Draw safe area
            if (preventOverflow)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = (corners[0] + corners[2]) / 2f;
                Gizmos.DrawWireCube(center, new Vector3(screenPadding * 2, screenPadding * 2, 0.1f));
            }
        }
#endif
    }

    public enum AnchorPreset
    {
        BottomCenter,
        BottomLeft,
        BottomRight,
        TopCenter,
        MiddleCenter
    }
}
