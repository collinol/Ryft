using UnityEngine;
using UnityEditor;
using Game.UI;

[InitializeOnLoad]
public class CardLayoutDiagnostic : EditorWindow
{
    [MenuItem("Tools/Card Layout/Diagnose Card Issues")]
    public static void DiagnoseCards()
    {
        var buttons = FindObjectsOfType<AbilityButton>(true);
        Debug.Log($"[Diagnostic] Found {buttons.Length} AbilityButtons");

        foreach (var button in buttons)
        {
            Debug.Log($"\n=== {button.gameObject.name} ===");

            // Check references
            Debug.Log($"  icon: {(button.icon != null ? button.icon.name : "NULL")}");
            Debug.Log($"  label: {(button.label != null ? button.label.name : "NULL")}");
            Debug.Log($"  cooldownText: {(button.cooldownText != null ? button.cooldownText.name : "NULL")}");

            // Check children
            Debug.Log($"  Children:");
            foreach (Transform child in button.transform)
            {
                Debug.Log($"    - {child.name} (active: {child.gameObject.activeSelf})");
            }

            // Check RectTransform
            var rt = button.GetComponent<RectTransform>();
            if (rt)
            {
                Debug.Log($"  RectTransform sizeDelta: {rt.sizeDelta}");
            }

            // Check icon details
            if (button.icon != null)
            {
                var iconRT = button.icon.GetComponent<RectTransform>();
                Debug.Log($"  Icon details:");
                Debug.Log($"    - sizeDelta: {iconRT.sizeDelta}");
                Debug.Log($"    - anchoredPosition: {iconRT.anchoredPosition}");
                Debug.Log($"    - sprite: {(button.icon.sprite != null ? button.icon.sprite.name : "NULL")}");
                Debug.Log($"    - enabled: {button.icon.enabled}");
                Debug.Log($"    - gameObject.activeSelf: {button.icon.gameObject.activeSelf}");
            }

            // Check label details
            if (button.label != null)
            {
                var labelRT = button.label.GetComponent<RectTransform>();
                Debug.Log($"  Label details:");
                Debug.Log($"    - sizeDelta: {labelRT.sizeDelta}");
                Debug.Log($"    - anchoredPosition: {labelRT.anchoredPosition}");
                Debug.Log($"    - text: '{button.label.text}'");
                Debug.Log($"    - enabled: {button.label.enabled}");
                Debug.Log($"    - gameObject.activeSelf: {button.label.gameObject.activeSelf}");
            }

            // Check Layout Element
            var le = button.GetComponent<UnityEngine.UI.LayoutElement>();
            if (le != null)
            {
                Debug.Log($"  LayoutElement:");
                Debug.Log($"    - preferredWidth: {le.preferredWidth}");
                Debug.Log($"    - preferredHeight: {le.preferredHeight}");
            }
        }

        // Check AbilityBarUI
        var abilityBar = FindObjectOfType<AbilityBarUI>(true);
        if (abilityBar != null)
        {
            Debug.Log($"\n=== AbilityBarUI ===");
            var hlg = abilityBar.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (hlg != null)
            {
                Debug.Log($"  HorizontalLayoutGroup:");
                Debug.Log($"    - spacing: {hlg.spacing}");
                Debug.Log($"    - padding: L={hlg.padding.left} R={hlg.padding.right} T={hlg.padding.top} B={hlg.padding.bottom}");
                Debug.Log($"    - childControlWidth: {hlg.childControlWidth}");
                Debug.Log($"    - childControlHeight: {hlg.childControlHeight}");
            }
            else
            {
                Debug.Log($"  No HorizontalLayoutGroup found!");
            }
        }
    }

    [MenuItem("Tools/Card Layout/Fix Card References")]
    public static void FixCardReferences()
    {
        var buttons = FindObjectsOfType<AbilityButton>(true);
        int fixedCount = 0;

        foreach (var button in buttons)
        {
            bool needsFix = false;

            // Try to auto-assign missing references by finding children
            if (button.icon == null)
            {
                var iconTransform = button.transform.Find("Icon");
                if (iconTransform != null)
                {
                    var iconImage = iconTransform.GetComponent<UnityEngine.UI.Image>();
                    if (iconImage != null)
                    {
                        // Use SerializedObject to assign the field
                        SerializedObject so = new SerializedObject(button);
                        so.FindProperty("icon").objectReferenceValue = iconImage;
                        so.ApplyModifiedProperties();
                        needsFix = true;
                        Debug.Log($"[Fix] Assigned Icon to {button.gameObject.name}");
                    }
                }
            }

            if (button.label == null)
            {
                var labelTransform = button.transform.Find("Label");
                if (labelTransform != null)
                {
                    var labelText = labelTransform.GetComponent<TMPro.TMP_Text>();
                    if (labelText != null)
                    {
                        SerializedObject so = new SerializedObject(button);
                        so.FindProperty("label").objectReferenceValue = labelText;
                        so.ApplyModifiedProperties();
                        needsFix = true;
                        Debug.Log($"[Fix] Assigned Label to {button.gameObject.name}");
                    }
                }
            }

            if (button.cooldownText == null)
            {
                var cooldownTransform = button.transform.Find("CooldownText");
                if (cooldownTransform != null)
                {
                    var cooldownTMP = cooldownTransform.GetComponent<TMPro.TMP_Text>();
                    if (cooldownTMP != null)
                    {
                        SerializedObject so = new SerializedObject(button);
                        so.FindProperty("cooldownText").objectReferenceValue = cooldownTMP;
                        so.ApplyModifiedProperties();
                        needsFix = true;
                        Debug.Log($"[Fix] Assigned CooldownText to {button.gameObject.name}");
                    }
                }
            }

            if (needsFix)
            {
                EditorUtility.SetDirty(button);
                fixedCount++;
            }
        }

        Debug.Log($"[Fix] Fixed references on {fixedCount} buttons");

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }

    [MenuItem("Tools/Card Layout/Reset Cards to Horizontal")]
    public static void ResetToHorizontal()
    {
        var buttons = FindObjectsOfType<AbilityButton>(true);

        foreach (var button in buttons)
        {
            var rt = button.GetComponent<RectTransform>();
            if (rt)
            {
                rt.sizeDelta = new Vector2(160f, 60f);
            }

            var le = button.GetComponent<UnityEngine.UI.LayoutElement>();
            if (le != null)
            {
                le.preferredWidth = 160f;
                le.preferredHeight = 60f;
                le.minWidth = 160f;
                le.minHeight = 60f;
            }

            EditorUtility.SetDirty(button.gameObject);
        }

        Debug.Log($"[Reset] Reset {buttons.Length} buttons to horizontal (160×60)");

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
