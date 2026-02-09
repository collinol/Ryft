using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Positions UI elements in the fight scene to prevent overlap.
    /// Attach to the Canvas in FightScene/PortalFight.
    /// </summary>
    [DefaultExecutionOrder(100)] // Run after other UI scripts
    public class FightUIPositioner : MonoBehaviour
    {
        [Header("Stats Label")]
        [SerializeField] private PlayerStatsLabel statsLabel;
        [SerializeField] private Vector2 statsPosition = new Vector2(10, -10); // Top-left offset
        [SerializeField] private AnchorPreset statsAnchor = AnchorPreset.TopLeft;

        [Header("Exit Button")]
        [SerializeField] private Button exitButton;
        [SerializeField] private Vector2 exitPosition = new Vector2(-10, -10); // Top-right offset
        [SerializeField] private AnchorPreset exitAnchor = AnchorPreset.TopRight;

        [Header("Auto-Find")]
        [SerializeField] private bool autoFind = true;

        void Start()
        {
            if (autoFind)
            {
                FindUIElements();
            }

            PositionElements();
        }

        private void FindUIElements()
        {
            if (statsLabel == null)
            {
                statsLabel = FindObjectOfType<PlayerStatsLabel>();
            }

            if (exitButton == null)
            {
                var exitComp = FindObjectOfType<FightExitButton>();
                if (exitComp != null)
                {
                    exitButton = exitComp.GetComponent<Button>();
                }
            }
        }

        [ContextMenu("Position UI Elements")]
        public void PositionElements()
        {
            // Position stats label at top-left
            if (statsLabel != null)
            {
                var rt = statsLabel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    SetAnchor(rt, statsAnchor);
                    rt.anchoredPosition = statsPosition;
                    Debug.Log($"[FightUIPositioner] Stats label positioned at {statsAnchor}: {statsPosition}");
                }
            }

            // Position exit button at top-right
            if (exitButton != null)
            {
                var rt = exitButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    SetAnchor(rt, exitAnchor);
                    rt.anchoredPosition = exitPosition;
                    Debug.Log($"[FightUIPositioner] Exit button positioned at {exitAnchor}: {exitPosition}");
                }
            }
        }

        private void SetAnchor(RectTransform rt, AnchorPreset preset)
        {
            switch (preset)
            {
                case AnchorPreset.TopLeft:
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    break;

                case AnchorPreset.TopRight:
                    rt.anchorMin = new Vector2(1f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
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

                case AnchorPreset.BottomCenter:
                    rt.anchorMin = new Vector2(0.5f, 0f);
                    rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0.5f, 0f);
                    break;
            }
        }

        public enum AnchorPreset
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            TopCenter,
            BottomCenter
        }
    }
}
