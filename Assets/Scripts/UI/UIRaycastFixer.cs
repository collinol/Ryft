using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI
{
    /// <summary>
    /// Helper component to fix common UI raycast issues
    /// Attach this to your Canvas or ability bar to automatically fix raycast blocking
    /// </summary>
    [ExecuteInEditMode]
    public class UIRaycastFixer : MonoBehaviour
    {
        [Header("Auto-Fix Settings")]
        [SerializeField] private bool fixOnAwake = true;
        [SerializeField] private bool fixTextRaycasts = true;
        [SerializeField] private bool fixIconRaycasts = true;
        [SerializeField] private bool ensureEventSystem = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        void Awake()
        {
            if (fixOnAwake && Application.isPlaying)
            {
                FixUIRaycasts();
            }
        }

        void Start()
        {
            if (Application.isPlaying && ensureEventSystem)
            {
                EnsureEventSystem();
            }
        }

        [ContextMenu("Fix UI Raycasts")]
        public void FixUIRaycasts()
        {
            int fixedCount = 0;

            // Fix all TMP_Text components (labels shouldn't block raycasts)
            if (fixTextRaycasts)
            {
                var texts = GetComponentsInChildren<TMP_Text>(true);
                foreach (var text in texts)
                {
                    if (text.raycastTarget)
                    {
                        text.raycastTarget = false;
                        fixedCount++;
                        if (showDebugLogs)
                            Debug.Log($"[UIRaycastFixer] Disabled raycast on text: {text.gameObject.name}");
                    }
                }
            }

            // Fix icon images (usually shouldn't block raycasts)
            if (fixIconRaycasts)
            {
                var images = GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    // Icons and decorative images shouldn't block raycasts
                    if (img.name.Contains("Icon") || img.name.Contains("icon"))
                    {
                        if (img.raycastTarget)
                        {
                            img.raycastTarget = false;
                            fixedCount++;
                            if (showDebugLogs)
                                Debug.Log($"[UIRaycastFixer] Disabled raycast on icon: {img.gameObject.name}");
                        }
                    }
                }
            }

            // Ensure button backgrounds ARE raycast targets
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                var img = btn.GetComponent<Image>();
                if (img && !img.raycastTarget)
                {
                    img.raycastTarget = true;
                    fixedCount++;
                    if (showDebugLogs)
                        Debug.Log($"[UIRaycastFixer] Enabled raycast on button: {btn.gameObject.name}");
                }
            }

            Debug.Log($"[UIRaycastFixer] Fixed {fixedCount} raycast issues");
        }

        [ContextMenu("Check EventSystem")]
        public void EnsureEventSystem()
        {
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogWarning("[UIRaycastFixer] No EventSystem found! Creating one...");
                var go = new GameObject("EventSystem");
                go.AddComponent<UnityEngine.EventSystems.EventSystem>();
                go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[UIRaycastFixer] EventSystem created");
            }
            else
            {
                Debug.Log($"[UIRaycastFixer] EventSystem found: {eventSystem.gameObject.name}");

                // Check for multiple EventSystems
                var allEventSystems = FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();
                if (allEventSystems.Length > 1)
                {
                    Debug.LogWarning($"[UIRaycastFixer] Found {allEventSystems.Length} EventSystems! This can cause issues.");
                    for (int i = 0; i < allEventSystems.Length; i++)
                    {
                        Debug.LogWarning($"  EventSystem {i+1}: {allEventSystems[i].gameObject.name}");
                    }
                }
            }
        }

        [ContextMenu("Debug AbilityButtons")]
        public void DebugAbilityButtons()
        {
            var buttons = GetComponentsInChildren<AbilityButton>(true);
            Debug.Log($"[UIRaycastFixer] Found {buttons.Length} AbilityButtons:");

            foreach (var btn in buttons)
            {
                var buttonComponent = btn.GetComponent<Button>();
                var image = btn.GetComponent<Image>();

                Debug.Log($"  Button: {btn.gameObject.name}");
                Debug.Log($"    - Active: {btn.gameObject.activeInHierarchy}");
                Debug.Log($"    - Button.interactable: {(buttonComponent ? buttonComponent.interactable.ToString() : "null")}");
                Debug.Log($"    - Image.raycastTarget: {(image ? image.raycastTarget.ToString() : "null")}");

                // Check for blocking children
                var childImages = btn.GetComponentsInChildren<Image>(true);
                foreach (var child in childImages)
                {
                    if (child != image && child.raycastTarget)
                    {
                        Debug.LogWarning($"    - WARNING: Child image blocking raycasts: {child.gameObject.name}");
                    }
                }

                var childTexts = btn.GetComponentsInChildren<TMP_Text>(true);
                foreach (var child in childTexts)
                {
                    if (child.raycastTarget)
                    {
                        Debug.LogWarning($"    - WARNING: Child text blocking raycasts: {child.gameObject.name}");
                    }
                }
            }
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // In editor, you can manually trigger the fix
                if (showDebugLogs)
                    Debug.Log("[UIRaycastFixer] Ready to fix raycasts. Use context menu or enable fixOnAwake.");
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Force Fix Now (Editor)")]
        private void ForceFixInEditor()
        {
            FixUIRaycasts();
            EnsureEventSystem();
            DebugAbilityButtons();
        }
#endif
    }
}
