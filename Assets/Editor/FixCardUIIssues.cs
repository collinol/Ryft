using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Game.UI;

public class FixCardUIIssues : EditorWindow
{
    [MenuItem("Tools/Card Layout/Fix Card Positioning and White Squares")]
    public static void FixAllCardIssues()
    {
        FixWhiteSquares();
        FixAbilityBarPosition();
    }

    [MenuItem("Tools/Card Layout/Fix White Squares Under Cards")]
    public static void FixWhiteSquares()
    {
        var buttons = FindObjectsOfType<AbilityButton>(true);
        int fixedCount = 0;

        foreach (var button in buttons)
        {
            // Check for white Image components that shouldn't be there
            var images = button.GetComponentsInChildren<Image>(true);

            foreach (var img in images)
            {
                // Skip the icon
                if (button.icon != null && img == button.icon) continue;

                // Check if this is the button's main background image
                if (img.gameObject == button.gameObject)
                {
                    // This is the button background - it should be visible but might need color adjustment
                    // Check if it's pure white (1,1,1,1) which might be the issue
                    if (img.color == Color.white)
                    {
                        // Make it slightly transparent or darker
                        img.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                        Debug.Log($"[Fix] Adjusted button background color for {button.gameObject.name}");
                        fixedCount++;
                    }
                    continue;
                }

                // Check if this is a duplicate/unwanted white image
                if (img.name.Contains("Background") || img.name.Contains("White") ||
                    (img.sprite == null && img.color == Color.white))
                {
                    // Found a likely culprit - make it transparent or disable it
                    img.color = new Color(1f, 1f, 1f, 0f); // Transparent
                    Debug.Log($"[Fix] Made {img.gameObject.name} transparent in {button.gameObject.name}");
                    fixedCount++;
                }
            }

            EditorUtility.SetDirty(button.gameObject);
        }

        Debug.Log($"[Fix] Fixed white square issues on {fixedCount} objects");

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }

    [MenuItem("Tools/Card Layout/Move Ability Bar Down")]
    public static void FixAbilityBarPosition()
    {
        var abilityBar = FindObjectOfType<AbilityBarUI>(true);

        if (abilityBar == null)
        {
            Debug.LogError("[Fix] AbilityBarUI not found in scene!");
            return;
        }

        var rt = abilityBar.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogError("[Fix] AbilityBarUI has no RectTransform!");
            return;
        }

        // Position at bottom of screen
        rt.anchorMin = new Vector2(0.5f, 0f);  // Bottom center
        rt.anchorMax = new Vector2(0.5f, 0f);  // Bottom center
        rt.pivot = new Vector2(0.5f, 0f);      // Pivot at bottom

        // Position with some offset from bottom
        rt.anchoredPosition = new Vector2(0, 20); // 20 pixels from bottom

        Debug.Log("[Fix] Moved AbilityBar to bottom of screen");
        Debug.Log($"  Anchors: min={rt.anchorMin}, max={rt.anchorMax}");
        Debug.Log($"  Position: {rt.anchoredPosition}");

        EditorUtility.SetDirty(abilityBar.gameObject);

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }

    [MenuItem("Tools/Card Layout/Diagnose Card Rendering Issues")]
    public static void DiagnoseRenderingIssues()
    {
        var buttons = FindObjectsOfType<AbilityButton>(true);

        Debug.Log($"=== Card Rendering Diagnostic ===");
        Debug.Log($"Found {buttons.Length} AbilityButtons");

        foreach (var button in buttons)
        {
            Debug.Log($"\n{button.gameObject.name}:");

            // Check all Image components
            var images = button.GetComponentsInChildren<Image>(true);
            Debug.Log($"  Total Image components: {images.Length}");

            foreach (var img in images)
            {
                bool isIcon = (button.icon != null && img == button.icon);
                bool isButton = (img.gameObject == button.gameObject);

                Debug.Log($"    - {img.gameObject.name}:");
                Debug.Log($"      Type: {(isIcon ? "Icon" : isButton ? "Button Background" : "Other")}");
                Debug.Log($"      Color: {img.color}");
                Debug.Log($"      Sprite: {(img.sprite != null ? img.sprite.name : "NULL")}");
                Debug.Log($"      Enabled: {img.enabled}");
                Debug.Log($"      Active: {img.gameObject.activeSelf}");
            }
        }

        // Check AbilityBar position
        var abilityBar = FindObjectOfType<AbilityBarUI>(true);
        if (abilityBar != null)
        {
            var rt = abilityBar.GetComponent<RectTransform>();
            Debug.Log($"\n=== AbilityBar Position ===");
            Debug.Log($"  AnchorMin: {rt.anchorMin}");
            Debug.Log($"  AnchorMax: {rt.anchorMax}");
            Debug.Log($"  AnchoredPosition: {rt.anchoredPosition}");
            Debug.Log($"  Pivot: {rt.pivot}");
        }
    }
}
