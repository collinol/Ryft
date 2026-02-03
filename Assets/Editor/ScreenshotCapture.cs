using UnityEngine;
using UnityEditor;
using System.IO;
using System;

/// <summary>
/// Editor tool for capturing screenshots of Unity scenes for UI review.
///
/// Usage:
/// - Press F12 in Play Mode to capture a screenshot
/// - Use menu: Tools > Screenshot > Capture Screenshot
/// - Screenshots are saved to: [ProjectRoot]/Screenshots/
///
/// After capturing, share the screenshots with Claude for UI feedback!
/// </summary>
public class ScreenshotCapture : EditorWindow
{
    private static string screenshotFolder = "Screenshots";
    private static int superSize = 1; // 1 = normal, 2 = 2x resolution, etc.
    private static bool includeTimestamp = true;
    private static bool includeSceneName = true;
    private static string customPrefix = "";

    [MenuItem("Tools/Screenshot/Capture Screenshot _F12")]
    public static void CaptureScreenshot()
    {
        string path = GetScreenshotPath();
        EnsureDirectoryExists();

        if (Application.isPlaying)
        {
            // In Play mode, use ScreenCapture
            ScreenCapture.CaptureScreenshot(path, superSize);
            Debug.Log($"<color=green>[Screenshot]</color> Captured: {path}");
        }
        else
        {
            // In Edit mode, capture the Game view
            CaptureGameViewScreenshot(path);
        }

        // Refresh asset database so it shows up
        AssetDatabase.Refresh();

        // Copy path to clipboard for easy sharing
        EditorGUIUtility.systemCopyBuffer = Path.GetFullPath(path);
        Debug.Log($"<color=cyan>[Screenshot]</color> Path copied to clipboard!");
    }

    [MenuItem("Tools/Screenshot/Capture All Scenes")]
    public static void CaptureAllScenesInBuild()
    {
        Debug.Log("[Screenshot] To capture all scenes, enter Play mode and visit each scene, pressing F12 to capture.");
    }

    [MenuItem("Tools/Screenshot/Open Screenshots Folder")]
    public static void OpenScreenshotsFolder()
    {
        EnsureDirectoryExists();
        string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", screenshotFolder));
        EditorUtility.RevealInFinder(fullPath);
    }

    [MenuItem("Tools/Screenshot/Settings...")]
    public static void OpenSettings()
    {
        var window = GetWindow<ScreenshotCapture>("Screenshot Settings");
        window.minSize = new Vector2(350, 250);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Screenshot Capture Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Press F12 anytime to capture a screenshot.\n" +
            "Screenshots are saved to the 'Screenshots' folder in your project root.",
            MessageType.Info);

        GUILayout.Space(10);

        screenshotFolder = EditorGUILayout.TextField("Folder Name", screenshotFolder);
        superSize = EditorGUILayout.IntSlider("Resolution Multiplier", superSize, 1, 4);
        includeTimestamp = EditorGUILayout.Toggle("Include Timestamp", includeTimestamp);
        includeSceneName = EditorGUILayout.Toggle("Include Scene Name", includeSceneName);
        customPrefix = EditorGUILayout.TextField("Custom Prefix", customPrefix);

        GUILayout.Space(20);

        if (GUILayout.Button("Capture Now (F12)", GUILayout.Height(40)))
        {
            CaptureScreenshot();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Open Screenshots Folder"))
        {
            OpenScreenshotsFolder();
        }

        GUILayout.Space(20);
        GUILayout.Label("Recent Screenshots:", EditorStyles.boldLabel);

        // List recent screenshots
        string folderPath = Path.Combine(Application.dataPath, "..", screenshotFolder);
        if (Directory.Exists(folderPath))
        {
            var files = Directory.GetFiles(folderPath, "*.png");
            Array.Sort(files);
            Array.Reverse(files);

            int shown = 0;
            foreach (var file in files)
            {
                if (shown >= 5) break;
                string fileName = Path.GetFileName(file);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(fileName, EditorStyles.miniLabel);
                if (GUILayout.Button("Copy Path", GUILayout.Width(70)))
                {
                    EditorGUIUtility.systemCopyBuffer = Path.GetFullPath(file);
                    Debug.Log($"Copied: {file}");
                }
                EditorGUILayout.EndHorizontal();
                shown++;
            }

            if (files.Length == 0)
            {
                EditorGUILayout.LabelField("No screenshots yet", EditorStyles.miniLabel);
            }
        }
    }

    private static string GetScreenshotPath()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(sceneName)) sceneName = "Unknown";

        string fileName = "";

        if (!string.IsNullOrEmpty(customPrefix))
        {
            fileName += customPrefix + "_";
        }

        if (includeSceneName)
        {
            fileName += sceneName + "_";
        }

        if (includeTimestamp)
        {
            fileName += DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }
        else
        {
            // Use incrementing number if no timestamp
            fileName += GetNextScreenshotNumber(sceneName);
        }

        fileName += ".png";

        return Path.Combine(screenshotFolder, fileName);
    }

    private static int GetNextScreenshotNumber(string sceneName)
    {
        string folderPath = Path.Combine(Application.dataPath, "..", screenshotFolder);
        if (!Directory.Exists(folderPath)) return 1;

        var files = Directory.GetFiles(folderPath, $"*{sceneName}*.png");
        return files.Length + 1;
    }

    private static void EnsureDirectoryExists()
    {
        string folderPath = Path.Combine(Application.dataPath, "..", screenshotFolder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"[Screenshot] Created folder: {folderPath}");
        }
    }

    private static void CaptureGameViewScreenshot(string path)
    {
        // Find the Game view and capture it
        var gameView = GetGameView();
        if (gameView == null)
        {
            Debug.LogWarning("[Screenshot] Could not find Game view. Try entering Play mode.");
            return;
        }

        // Get the Game view's render texture
        var renderTexture = new RenderTexture(
            (int)gameView.position.width * superSize,
            (int)gameView.position.height * superSize,
            24);

        // For edit mode, we need to use a different approach
        // This captures whatever is currently rendered in the Game view
        gameView.Focus();

        // Use reflection to access the Game view's render target
        var assembly = typeof(EditorWindow).Assembly;
        var type = assembly.GetType("UnityEditor.GameView");
        var getMainGameViewMethod = type.GetMethod("GetMainGameView",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (getMainGameViewMethod != null)
        {
            var mainGameView = getMainGameViewMethod.Invoke(null, null) as EditorWindow;
            if (mainGameView != null)
            {
                mainGameView.Repaint();
            }
        }

        // Fallback: just notify user to enter play mode
        Debug.Log("[Screenshot] For best results, enter Play mode before capturing. Path will be: " + path);

        // Still try to capture using the basic method
        try
        {
            ScreenCapture.CaptureScreenshot(path, superSize);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Screenshot] Capture failed in Edit mode: {e.Message}. Enter Play mode and try again.");
        }
    }

    private static EditorWindow GetGameView()
    {
        var assembly = typeof(EditorWindow).Assembly;
        var type = assembly.GetType("UnityEditor.GameView");
        return EditorWindow.GetWindow(type, false, "Game", false);
    }
}

/// <summary>
/// Runtime component for capturing screenshots during gameplay.
/// Add this to a GameObject in your scene for additional capture options.
/// </summary>
public class RuntimeScreenshotCapture : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode captureKey = KeyCode.F12;
    public int superSize = 1;
    public string folderName = "Screenshots";

    [Header("Auto-Capture (Optional)")]
    public bool autoCaptureOnSceneLoad = false;
    public float autoCaptureDelay = 1f;

    private void Start()
    {
        if (autoCaptureOnSceneLoad)
        {
            Invoke(nameof(CaptureScreenshot), autoCaptureDelay);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(captureKey))
        {
            CaptureScreenshot();
        }
    }

    public void CaptureScreenshot()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{sceneName}_{timestamp}.png";
        string path = Path.Combine(folderName, fileName);

        // Ensure directory exists
        string fullFolderPath = Path.Combine(Application.dataPath, "..", folderName);
        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
        }

        ScreenCapture.CaptureScreenshot(path, superSize);
        Debug.Log($"<color=green>[Screenshot]</color> Captured: {path}");
    }
}
