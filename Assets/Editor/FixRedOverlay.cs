using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;

public class FixRedOverlay : EditorWindow
{
    [MenuItem("Tools/Fight Scene/Find Red Overlay")]
    public static void FindRedOverlay()
    {
        Debug.Log("=== Searching for Red Overlay ===");

        // Check Camera background color
        var cameras = FindObjectsOfType<Camera>(true);
        Debug.Log($"\n=== Cameras ({cameras.Length}) ===");
        foreach (var cam in cameras)
        {
            Debug.Log($"  - {GetGameObjectPath(cam.gameObject)}");
            Debug.Log($"    Clear Flags: {cam.clearFlags}");
            Debug.Log($"    Background Color: {cam.backgroundColor}");
            Debug.Log($"    Active: {cam.gameObject.activeSelf}, Enabled: {cam.enabled}");
        }

        // Find all Image components
        var allImages = FindObjectsOfType<Image>(true);
        Debug.Log($"\n=== Total Image components: {allImages.Length} ===");

        // Find ANY red-ish images (broader search)
        var redImages = allImages.Where(img =>
            img.color.r > 0.3f &&
            img.color.g < 0.5f &&
            img.color.b < 0.5f
        ).ToArray();

        Debug.Log($"\nFound {redImages.Length} reddish Image components:");
        foreach (var img in redImages)
        {
            var rt = img.GetComponent<RectTransform>();
            Debug.Log($"  - {GetGameObjectPath(img.gameObject)}");
            Debug.Log($"    Color: {img.color}");
            Debug.Log($"    Size: {rt.sizeDelta}, Rect: {rt.rect.width}×{rt.rect.height}");
            Debug.Log($"    AnchorMin: {rt.anchorMin}, AnchorMax: {rt.anchorMax}");
            Debug.Log($"    Active: {img.gameObject.activeSelf}, Enabled: {img.enabled}");
            Debug.Log($"    Sorting Order: {img.canvas?.sortingOrder ?? 0}");
        }

        // Find all Panels
        var allPanels = FindObjectsOfType<GameObject>(true)
                       .Where(go => go.name.ToLower().Contains("panel") ||
                                   go.name.ToLower().Contains("background") ||
                                   go.name.ToLower().Contains("overlay"))
                       .ToArray();

        Debug.Log($"\nFound {allPanels.Length} GameObjects with 'Panel/Background/Overlay' in name:");
        foreach (var panel in allPanels)
        {
            Debug.Log($"  - {GetGameObjectPath(panel)}");
            var img = panel.GetComponent<Image>();
            if (img)
            {
                Debug.Log($"    Image Color: {img.color}, Active: {panel.activeSelf}, Enabled: {img.enabled}");
            }
        }

        // Find large UI elements
        var largeRects = FindObjectsOfType<RectTransform>(true)
                        .Where(rt => rt.rect.width > 500 && rt.rect.height > 300)
                        .ToArray();

        Debug.Log($"\nFound {largeRects.Length} large RectTransforms (>500×300):");
        foreach (var rt in largeRects)
        {
            var img = rt.GetComponent<Image>();
            Debug.Log($"  - {GetGameObjectPath(rt.gameObject)}");
            Debug.Log($"    Size: {rt.rect.width}×{rt.rect.height}");
            Debug.Log($"    Active: {rt.gameObject.activeSelf}");
            if (img)
            {
                Debug.Log($"    Image Color: {img.color}, Enabled: {img.enabled}");
            }
        }

        // Check all canvas renderers with SpriteRenderer
        var spriteRenderers = FindObjectsOfType<SpriteRenderer>(true);
        Debug.Log($"\n=== SpriteRenderers ({spriteRenderers.Length}) ===");
        foreach (var sr in spriteRenderers)
        {
            if (sr.color.r > 0.3f && sr.color.g < 0.5f && sr.color.b < 0.5f)
            {
                Debug.Log($"  - {GetGameObjectPath(sr.gameObject)}");
                Debug.Log($"    Color: {sr.color}");
                Debug.Log($"    Sorting Layer: {sr.sortingLayerName}, Order: {sr.sortingOrder}");
            }
        }
    }

    [MenuItem("Tools/Fight Scene/Fix Red Background")]
    public static void FixRedBackground()
    {
        Debug.Log("=== Fixing Red Background ===");

        var cam = Camera.main;
        if (!cam)
        {
            cam = FindObjectOfType<Camera>();
        }

        if (!cam)
        {
            Debug.LogError("No camera found!");
            return;
        }

        Debug.Log($"Current camera settings:");
        Debug.Log($"  Clear Flags: {cam.clearFlags}");
        Debug.Log($"  Background Color: {cam.backgroundColor}");
        Debug.Log($"  Skybox Material: {RenderSettings.skybox}");

        // The red is likely from a broken skybox. Switch to solid color background.
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.15f, 0.2f, 0.3f, 1f); // Dark blue background

        Debug.Log($"\nNew camera settings:");
        Debug.Log($"  Clear Flags: {cam.clearFlags}");
        Debug.Log($"  Background Color: {cam.backgroundColor}");

        // Also mark the scene as dirty so changes are saved
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(cam);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("Scene marked dirty - remember to save the scene!");
        }

        Debug.Log("\nBackground fixed! The red should be replaced with dark blue.");
        Debug.Log("If in Play mode, stop playing and save the scene to persist changes.");
    }

    [MenuItem("Tools/Fight Scene/Make Red Overlays Transparent")]
    public static void MakeRedOverlaysTransparent()
    {
        Debug.Log("=== Making Red Overlays Transparent ===");

        var allImages = FindObjectsOfType<Image>(true);

        var redImages = allImages.Where(img =>
            img.color.r > 0.5f &&
            img.color.g < 0.5f &&
            img.color.b < 0.5f &&
            img.color.a > 0.5f
        ).ToArray();

        int fixedCount = 0;

        foreach (var img in redImages)
        {
            var rt = img.GetComponent<RectTransform>();

            // Check if it's a large overlay
            if (rt.rect.width > 1000 || rt.rect.height > 500)
            {
                Debug.Log($"Making transparent: {GetGameObjectPath(img.gameObject)}");
                var color = img.color;
                color.a = 0f; // Fully transparent
                img.color = color;
                EditorUtility.SetDirty(img.gameObject);
                fixedCount++;
            }
        }

        Debug.Log($"Made {fixedCount} red overlays transparent");

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }

    [MenuItem("Tools/Fight Scene/List All Canvas Children")]
    public static void ListCanvasChildren()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (!canvas)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        Debug.Log($"=== Canvas: {canvas.gameObject.name} ===");
        Debug.Log($"Children: {canvas.transform.childCount}");

        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            var child = canvas.transform.GetChild(i);
            var img = child.GetComponent<Image>();
            var rt = child.GetComponent<RectTransform>();

            Debug.Log($"\n[{i}] {child.name}");
            Debug.Log($"    Active: {child.gameObject.activeSelf}");

            if (rt)
            {
                Debug.Log($"    Size: {rt.rect.width}×{rt.rect.height}");
                Debug.Log($"    Anchors: {rt.anchorMin} to {rt.anchorMax}");
            }

            if (img)
            {
                Debug.Log($"    Image Color: {img.color}");
            }
        }
    }

    private static string GetGameObjectPath(GameObject obj)
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
}
