using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Game.UI;

[InitializeOnLoad]
public class CardLayoutEditor : EditorWindow
{
    private static float cardWidth = 100f;
    private static float cardHeight = 140f;
    private static float spacing = 12f;
    private static int padding = 15;

    [MenuItem("Tools/Card Layout/Apply Vertical Card Layout")]
    public static void ApplyVerticalCardLayout()
    {
        // Find all AbilityButtons in the scene
        var buttons = FindObjectsOfType<AbilityButton>(true);
        int count = 0;

        foreach (var button in buttons)
        {
            ApplyCardLayoutToButton(button);
            count++;
        }

        // Find and configure AbilityBarUI
        var abilityBar = FindObjectOfType<AbilityBarUI>(true);
        if (abilityBar != null)
        {
            ConfigureAbilityBar(abilityBar);
        }

        Debug.Log($"[CardLayoutEditor] Applied vertical card layout to {count} buttons");
        Debug.Log($"[CardLayoutEditor] Card size: {cardWidth}×{cardHeight}, Spacing: {spacing}, Padding: {padding}");

        // Mark scene as dirty so changes are saved
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }

    private static void ApplyCardLayoutToButton(AbilityButton button)
    {
        // Set RectTransform size
        var rt = button.GetComponent<RectTransform>();
        if (rt)
        {
            rt.sizeDelta = new Vector2(cardWidth, cardHeight);
        }

        // Set LayoutElement
        var le = button.GetComponent<LayoutElement>();
        if (le == null) le = button.gameObject.AddComponent<LayoutElement>();

        le.preferredWidth = cardWidth;
        le.preferredHeight = cardHeight;
        le.minWidth = cardWidth;
        le.minHeight = cardHeight;

        // Configure child elements
        ConfigureIcon(button);
        ConfigureLabel(button);
        ConfigureCostText(button);

        EditorUtility.SetDirty(button.gameObject);
    }

    private static void ConfigureIcon(AbilityButton button)
    {
        var icon = button.icon;
        if (icon == null) return;

        var iconRT = icon.GetComponent<RectTransform>();

        // Position at top center
        iconRT.anchorMin = new Vector2(0.5f, 1f);
        iconRT.anchorMax = new Vector2(0.5f, 1f);
        iconRT.pivot = new Vector2(0.5f, 1f);
        iconRT.anchoredPosition = new Vector2(0, -15);
        iconRT.sizeDelta = new Vector2(60, 60);

        // Disable raycast
        icon.raycastTarget = false;

        // If icon has no sprite, make it transparent to avoid white square
        if (icon.sprite == null)
        {
            icon.color = new Color(1f, 1f, 1f, 0f); // Fully transparent
        }
        else
        {
            // Ensure icon is visible if it has a sprite
            if (icon.color.a < 0.5f)
            {
                icon.color = Color.white;
            }
        }

        EditorUtility.SetDirty(icon.gameObject);
    }

    private static void ConfigureLabel(AbilityButton button)
    {
        var label = button.label;
        if (label == null) return;

        var labelRT = label.GetComponent<RectTransform>();

        // Position at bottom, take up bottom 40%
        labelRT.anchorMin = new Vector2(0f, 0f);
        labelRT.anchorMax = new Vector2(1f, 0.4f);
        labelRT.pivot = new Vector2(0.5f, 0f);
        labelRT.anchoredPosition = new Vector2(0, 5);
        labelRT.sizeDelta = Vector2.zero;

        // Configure text
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.fontSize = 14;
        label.fontSizeMin = 8;
        label.fontSizeMax = 14;
        label.enableAutoSizing = true;

        // Disable raycast
        if (label is Graphic graphic)
            graphic.raycastTarget = false;

        EditorUtility.SetDirty(label.gameObject);
    }

    private static void ConfigureCostText(AbilityButton button)
    {
        var costText = button.cooldownText;
        if (costText == null) return;

        var costRT = costText.GetComponent<RectTransform>();

        // Position at top-left corner
        costRT.anchorMin = new Vector2(0f, 1f);
        costRT.anchorMax = new Vector2(0f, 1f);
        costRT.pivot = new Vector2(0f, 1f);
        costRT.anchoredPosition = new Vector2(5, -5);
        costRT.sizeDelta = new Vector2(30, 30);

        // Configure text
        costText.alignment = TextAlignmentOptions.Center;
        costText.fontSize = 16;
        costText.fontStyle = FontStyles.Bold;

        // Disable raycast
        if (costText is Graphic graphic)
            graphic.raycastTarget = false;

        EditorUtility.SetDirty(costText.gameObject);
    }

    private static void ConfigureAbilityBar(AbilityBarUI abilityBar)
    {
        var hlg = abilityBar.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) hlg = abilityBar.gameObject.AddComponent<HorizontalLayoutGroup>();

        hlg.spacing = spacing;
        hlg.padding = new RectOffset(padding, padding, padding, padding);
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleCenter;

        // Add ContentSizeFitter
        var csf = abilityBar.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = abilityBar.gameObject.AddComponent<ContentSizeFitter>();

        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        EditorUtility.SetDirty(abilityBar.gameObject);
    }

    [MenuItem("Tools/Card Layout/Show Card Layout Window")]
    public static void ShowWindow()
    {
        GetWindow<CardLayoutEditor>("Card Layout");
    }

    void OnGUI()
    {
        GUILayout.Label("Vertical Card Layout", EditorStyles.boldLabel);
        GUILayout.Space(10);

        cardWidth = EditorGUILayout.FloatField("Card Width", cardWidth);
        cardHeight = EditorGUILayout.FloatField("Card Height", cardHeight);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);
        padding = EditorGUILayout.IntField("Padding", padding);

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Vertical Card Layout", GUILayout.Height(40)))
        {
            ApplyVerticalCardLayout();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "This will apply vertical card dimensions (100×140) to all AbilityButtons in the scene.\n\n" +
            "Card Width: Width of each card\n" +
            "Card Height: Height of each card (should be ~1.4× width)\n" +
            "Spacing: Gap between cards\n" +
            "Padding: Border around card bar",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Reset to Defaults"))
        {
            cardWidth = 100f;
            cardHeight = 140f;
            spacing = 12f;
            padding = 15;
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Current settings will create cards that are " +
            cardHeight / cardWidth + "× taller than wide.\n\n" +
            "Standard playing card ratio: ~1.4\n" +
            "Tarot card ratio: ~1.7",
            MessageType.None);
    }
}
