using UnityEngine;
using UnityEditor;
using Game.UI;

public class FixAbilityBarUI : EditorWindow
{
    [MenuItem("Tools/Card Layout/Fix AbilityBarUI Inspector Values")]
    public static void FixInspectorValues()
    {
        var abilityBar = FindObjectOfType<AbilityBarUI>(true);

        if (abilityBar == null)
        {
            Debug.LogError("[Fix] AbilityBarUI not found in scene!");
            return;
        }

        // Use SerializedObject to modify private serialized fields
        SerializedObject so = new SerializedObject(abilityBar);

        // Set vertical card dimensions
        so.FindProperty("slotWidth").floatValue = 100f;
        so.FindProperty("slotHeight").floatValue = 140f;
        so.FindProperty("spacing").floatValue = 12f;
        so.FindProperty("padding").intValue = 15;

        so.ApplyModifiedProperties();

        Debug.Log("[Fix] Set AbilityBarUI Inspector values:");
        Debug.Log("  slotWidth: 100");
        Debug.Log("  slotHeight: 140");
        Debug.Log("  spacing: 12");
        Debug.Log("  padding: 15");

        EditorUtility.SetDirty(abilityBar);

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }

    [MenuItem("Tools/Card Layout/Show AbilityBarUI Values")]
    public static void ShowInspectorValues()
    {
        var abilityBar = FindObjectOfType<AbilityBarUI>(true);

        if (abilityBar == null)
        {
            Debug.LogError("[Check] AbilityBarUI not found in scene!");
            return;
        }

        SerializedObject so = new SerializedObject(abilityBar);

        float slotWidth = so.FindProperty("slotWidth").floatValue;
        float slotHeight = so.FindProperty("slotHeight").floatValue;
        float spacing = so.FindProperty("spacing").floatValue;
        int padding = so.FindProperty("padding").intValue;

        Debug.Log("=== AbilityBarUI Current Inspector Values ===");
        Debug.Log($"  slotWidth: {slotWidth}");
        Debug.Log($"  slotHeight: {slotHeight}");
        Debug.Log($"  spacing: {spacing}");
        Debug.Log($"  padding: {padding}");

        if (slotWidth == 100f && slotHeight == 140f)
        {
            Debug.Log("✅ Values are correct for vertical cards!");
        }
        else if (slotWidth == 160f && slotHeight == 60f)
        {
            Debug.Log("❌ Values are still set for horizontal cards - run 'Fix AbilityBarUI Inspector Values'");
        }
        else
        {
            Debug.Log($"⚠️ Unexpected values (should be 100×140 for vertical or 160×60 for horizontal)");
        }
    }
}
