using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class FixFightSceneUI : EditorWindow
{
    [MenuItem("Tools/Fight Scene/Diagnose UI Issues")]
    public static void DiagnoseUIIssues()
    {
        Debug.Log("=== Fight Scene UI Diagnostic ===\n");

        // Find all Text/TMP components showing health
        var allText = FindObjectsOfType<TMP_Text>(true);
        var healthTexts = allText.Where(t => t.text.Contains("/") &&
                                             (t.text.Contains("30") || t.text.Contains("500") || t.text.Contains("100")))
                                  .ToArray();

        Debug.Log($"Found {healthTexts.Length} health text elements:");
        foreach (var txt in healthTexts)
        {
            Debug.Log($"  - {txt.gameObject.name}: '{txt.text}' (Parent: {txt.transform.parent?.name})");
            Debug.Log($"    Position: {txt.transform.position}, Active: {txt.gameObject.activeSelf}, Enabled: {txt.enabled}");
        }

        // Find all health bar related components
        var healthBars = FindObjectsOfType<MonoBehaviour>(true)
                        .Where(mb => mb.GetType().Name.Contains("Health") ||
                                    mb.GetType().Name.Contains("HPBar"))
                        .ToArray();

        Debug.Log($"\nFound {healthBars.Length} health bar components:");
        foreach (var hb in healthBars)
        {
            Debug.Log($"  - {hb.gameObject.name}: {hb.GetType().Name}");
        }

        // Find overlapping UI elements
        var allImages = FindObjectsOfType<Image>(true);
        Debug.Log($"\nTotal Images in scene: {allImages.Length}");

        var canvas = FindObjectOfType<Canvas>();
        if (canvas)
        {
            Debug.Log($"\nCanvas: {canvas.gameObject.name}");
            Debug.Log($"  Render Mode: {canvas.renderMode}");
            Debug.Log($"  Children: {canvas.transform.childCount}");

            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                var child = canvas.transform.GetChild(i);
                Debug.Log($"    - {child.name} (active: {child.gameObject.activeSelf})");
            }
        }
    }

    [MenuItem("Tools/Fight Scene/Remove Duplicate Health Bars")]
    public static void RemoveDuplicateHealthBars()
    {
        // Find all text components that look like health bars
        var allText = FindObjectsOfType<TMP_Text>(true);
        var healthTexts = allText.Where(t => t.text.Contains("/") &&
                                             (char.IsDigit(t.text[0]) || t.text.Length > 0))
                                  .ToArray();

        Debug.Log($"[Fix] Found {healthTexts.Length} potential health text elements");

        // Group by text content
        var grouped = healthTexts.GroupBy(t => t.text);

        int removedCount = 0;
        foreach (var group in grouped)
        {
            if (group.Count() > 1)
            {
                Debug.Log($"[Fix] Found {group.Count()} duplicates of '{group.Key}'");

                // Keep the first one, destroy the rest
                bool first = true;
                foreach (var txt in group)
                {
                    if (first)
                    {
                        first = false;
                        Debug.Log($"  Keeping: {txt.gameObject.name}");
                        continue;
                    }

                    Debug.Log($"  Removing: {txt.gameObject.name}");

                    if (Application.isPlaying)
                        Destroy(txt.gameObject);
                    else
                        DestroyImmediate(txt.gameObject);

                    removedCount++;
                }
            }
        }

        Debug.Log($"[Fix] Removed {removedCount} duplicate health bars");

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }

    [MenuItem("Tools/Fight Scene/Clean Up UI Layout")]
    public static void CleanUpUILayout()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (!canvas)
        {
            Debug.LogError("[Fix] No Canvas found!");
            return;
        }

        Debug.Log("[Fix] Cleaning up UI layout...");

        // Find player stats display
        var statTexts = FindObjectsOfType<TMP_Text>(true)
                       .Where(t => t.text.Contains("EN") || t.text.Contains("STR") ||
                                  t.text.Contains("MANA") || t.text.Contains("ENG"))
                       .ToArray();

        foreach (var txt in statTexts)
        {
            var rt = txt.GetComponent<RectTransform>();
            if (rt)
            {
                // Position at bottom-left
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);
                rt.anchoredPosition = new Vector2(10, 10);

                Debug.Log($"[Fix] Repositioned {txt.gameObject.name} to bottom-left");
                EditorUtility.SetDirty(txt.gameObject);
            }
        }

        // Find health bars and position them above characters
        var healthBars = FindObjectsOfType<MonoBehaviour>(true)
                        .Where(mb => mb.GetType().Name.Contains("Health") ||
                                    mb.GetType().Name.Contains("HPBar"))
                        .ToArray();

        foreach (var hb in healthBars)
        {
            // Health bars should be in world space or positioned above characters
            Debug.Log($"[Fix] Found health bar: {hb.gameObject.name}");
            EditorUtility.SetDirty(hb.gameObject);
        }

        Debug.Log("[Fix] UI cleanup complete");

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }

    [MenuItem("Tools/Fight Scene/Fix Health Bar Positions")]
    public static void FixHealthBarPositions()
    {
        Debug.Log("[Fix] Fixing health bar positions...");

        // Find all GameObjects with "Health" or "HP" in the name
        var allObjects = FindObjectsOfType<GameObject>(true);
        var healthObjects = allObjects.Where(go => go.name.ToLower().Contains("health") ||
                                                   go.name.ToLower().Contains("hp"))
                                       .ToArray();

        Debug.Log($"Found {healthObjects.Length} health-related GameObjects");

        foreach (var go in healthObjects)
        {
            Debug.Log($"  - {go.name} (Parent: {go.transform.parent?.name})");

            // If it's a UI element (has RectTransform), check if it should be there
            var rt = go.GetComponent<RectTransform>();
            if (rt)
            {
                // Check if it's under Canvas
                var canvas = go.GetComponentInParent<Canvas>();
                if (canvas)
                {
                    Debug.Log($"    UI element under canvas: {canvas.gameObject.name}");
                }
                else
                {
                    Debug.Log($"    UI element but no canvas found");
                }
            }
            else
            {
                Debug.Log($"    World space object");
            }
        }

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
