using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Game.Combat
{
    /// <summary>
    /// Debug tool to find and fix the red overlay issue at runtime
    /// Add this to any GameObject in the scene and it will auto-detect and fix red overlays
    /// </summary>
    public class RedOverlayDebugger : MonoBehaviour
    {
        [Header("Auto-Fix Settings")]
        [SerializeField] private bool autoFixOnStart = true;
        [SerializeField] private bool logDetails = true;

        void Start()
        {
            if (autoFixOnStart)
            {
                Invoke(nameof(FindAndFixRedOverlays), 0.5f); // Delay to let everything spawn
            }
        }

        [ContextMenu("Find and Fix Red Overlays")]
        public void FindAndFixRedOverlays()
        {
            Debug.Log("=== Red Overlay Debugger ===\n");

            // Find all Canvas components
            var canvases = FindObjectsOfType<Canvas>(true);
            Debug.Log($"Found {canvases.Length} Canvas components");

            foreach (var canvas in canvases)
            {
                Debug.Log($"\nCanvas: {GetPath(canvas.gameObject)}");
                Debug.Log($"  RenderMode: {canvas.renderMode}");
                Debug.Log($"  SortingOrder: {canvas.sortingOrder}");
                Debug.Log($"  Enabled: {canvas.enabled}");

                var rt = canvas.GetComponent<RectTransform>();
                if (rt)
                {
                    Debug.Log($"  Size: {rt.rect.width} x {rt.rect.height}");
                    Debug.Log($"  SizeDelta: {rt.sizeDelta}");
                    Debug.Log($"  LocalScale: {rt.localScale}");
                    Debug.Log($"  Position: {rt.position}");
                }

                // Check for problematic world-space canvases
                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    if (rt && rt.sizeDelta.magnitude > 1000)
                    {
                        Debug.LogWarning($"  ⚠ HUGE world-space canvas detected! This might be the red overlay.");
                        Debug.LogWarning($"     Size: {rt.sizeDelta}");
                    }
                }

                // Check children for red images
                var images = canvas.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (IsRedish(img.color))
                    {
                        var imgRt = img.GetComponent<RectTransform>();
                        Debug.LogWarning($"  🔴 RED IMAGE: {GetPath(img.gameObject)}");
                        Debug.LogWarning($"     Color: {img.color}");
                        Debug.LogWarning($"     Size: {imgRt.rect.width} x {imgRt.rect.height}");
                        Debug.LogWarning($"     Active: {img.gameObject.activeSelf}, Enabled: {img.enabled}");

                        // Check if it's abnormally large
                        if (imgRt.rect.width > 500 || imgRt.rect.height > 300)
                        {
                            Debug.LogError($"     ❌ GIANT RED OVERLAY FOUND!");
                            Debug.LogError($"        This is likely the problem!");

                            if (autoFixOnStart)
                            {
                                Debug.Log($"     Disabling this image...");
                                img.enabled = false;
                                Debug.Log($"     ✓ Red overlay disabled!");
                            }
                        }
                    }
                }
            }

            // Check all Image components (not just in canvases)
            var allImages = FindObjectsOfType<Image>(true);
            var redImages = allImages.Where(img => IsRedish(img.color)).ToArray();

            Debug.Log($"\n=== All Red Images ({redImages.Length}) ===");
            foreach (var img in redImages)
            {
                var rt = img.GetComponent<RectTransform>();
                Debug.Log($"Red Image: {GetPath(img.gameObject)}");
                Debug.Log($"  Color: {img.color}");
                Debug.Log($"  Size: {rt.rect.width} x {rt.rect.height}");
                Debug.Log($"  Canvas: {img.canvas?.name ?? "None"}");
                Debug.Log($"  Active: {img.gameObject.activeSelf}, Enabled: {img.enabled}");

                // Auto-fix giant red overlays
                if (autoFixOnStart && (rt.rect.width > 500 || rt.rect.height > 300))
                {
                    Debug.LogError($"  ❌ Disabling giant red overlay!");
                    img.enabled = false;
                }
            }

            Debug.Log("\n=== Debugger Complete ===");
        }

        private bool IsRedish(Color color)
        {
            return color.r > 0.5f && color.g < 0.5f && color.b < 0.5f && color.a > 0.1f;
        }

        private string GetPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        void Update()
        {
            // Press F9 to trigger debug
            if (Input.GetKeyDown(KeyCode.F9))
            {
                FindAndFixRedOverlays();
            }
        }
    }
}
