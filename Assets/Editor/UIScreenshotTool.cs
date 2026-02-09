using UnityEngine;
using UnityEditor;
using System.IO;
using System;

/// <summary>
/// Simple screenshot tool for capturing Unity UI for review.
///
/// HOW TO USE:
/// ===========
/// 1. Enter Play Mode and navigate to the scene you want to capture
/// 2. Press F12 (or use menu: Tools > UI Screenshots > Capture)
/// 3. Screenshots save to: YourProject/UIScreenshots/
/// 4. Share the screenshot with Claude by dragging the image into the chat
///    OR copy the file path and tell Claude to read it
///
/// TIPS FOR GOOD SCREENSHOTS:
/// - Capture each scene (MapScene, FightScene, ShopScene, RestScene, RewardScene, etc.)
/// - Show different states (full health, low health, with items, empty, etc.)
/// - Include any UI issues you want fixed
/// </summary>
public class UIScreenshotTool : EditorWindow
{
    private static readonly string SaveFolder = "UIScreenshots";

    // Capture with F12
    [MenuItem("Tools/UI Screenshots/Capture Screenshot _F12")]
    public static void Capture()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("<color=yellow>[Screenshot]</color> Enter Play Mode first, then press F12");
            return;
        }

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string timestamp = DateTime.Now.ToString("HHmmss");
        string filename = $"{sceneName}_{timestamp}.png";
        string relativePath = Path.Combine(SaveFolder, filename);

        // Create folder if needed
        string fullFolder = Path.Combine(Application.dataPath, "..", SaveFolder);
        if (!Directory.Exists(fullFolder))
            Directory.CreateDirectory(fullFolder);

        // Capture
        ScreenCapture.CaptureScreenshot(relativePath, 2); // 2x resolution for clarity

        string fullPath = Path.GetFullPath(relativePath);
        Debug.Log($"<color=green>[Screenshot Saved]</color> {fullPath}");

        // Copy to clipboard
        EditorGUIUtility.systemCopyBuffer = fullPath;
        Debug.Log("<color=cyan>[Path copied to clipboard]</color>");
    }

    [MenuItem("Tools/UI Screenshots/Open Folder")]
    public static void OpenFolder()
    {
        string fullFolder = Path.Combine(Application.dataPath, "..", SaveFolder);
        if (!Directory.Exists(fullFolder))
            Directory.CreateDirectory(fullFolder);
        EditorUtility.RevealInFinder(fullFolder);
    }

    [MenuItem("Tools/UI Screenshots/Quick Guide")]
    public static void ShowGuide()
    {
        EditorUtility.DisplayDialog("UI Screenshot Guide",
            "1. Enter PLAY MODE\n" +
            "2. Navigate to the scene you want to capture\n" +
            "3. Press F12 to capture\n" +
            "4. Screenshots save to: UIScreenshots/\n\n" +
            "To share with Claude:\n" +
            "- Drag the PNG file into the chat, OR\n" +
            "- Paste the file path (auto-copied to clipboard)\n\n" +
            "Scenes to capture:\n" +
            "- MapScene (the map view)\n" +
            "- FightScene (combat)\n" +
            "- RewardScene (post-combat rewards)\n" +
            "- ShopScene (the shop)\n" +
            "- RestScene (health station)\n" +
            "- TimePortalScene (time portal)",
            "Got it!");
    }

    [MenuItem("Tools/UI Screenshots/Capture All Scenes (Instructions)")]
    public static void CaptureAllInstructions()
    {
        EditorUtility.DisplayDialog("Capture All Scenes",
            "To capture all UI scenes:\n\n" +
            "1. Play the game from MapScene\n" +
            "2. Press F12 on the map\n" +
            "3. Click an Enemy node → F12 in combat\n" +
            "4. Win the fight → F12 on reward screen\n" +
            "5. Return to map, visit Shop → F12\n" +
            "6. Visit Rest node → F12\n" +
            "7. Visit TimePortal node → F12\n\n" +
            "All screenshots will be in UIScreenshots/",
            "OK");
    }
}
