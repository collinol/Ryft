// Assets/Editor/CardDatabaseEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Game.Cards;
using System.Linq;

[CustomEditor(typeof(CardDatabase))]
public class CardDatabaseEditor : Editor
{
    private bool showTestingTools = true;
    private bool showCatalog = false;
    private bool showAvailability = true;

    public override void OnInspectorGUI()
    {
        var db = (CardDatabase)target;
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Card Database", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Testing Tools Section
        showTestingTools = EditorGUILayout.Foldout(showTestingTools, "Testing Tools", true);
        if (showTestingTools)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable All Cards (1 copy)"))
            {
                SetAllCardsAvailable(db, 1);
            }
            if (GUILayout.Button("Enable All Cards (3 copies)"))
            {
                SetAllCardsAvailable(db, 3);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Disable All Cards"))
            {
                SetAllCardsAvailable(db, 0);
            }
            if (GUILayout.Button("Reset to Defaults"))
            {
                ResetToDefaults(db);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("By Rarity", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable Commons Only"))
            {
                SetCardsByRarity(db, CardRarity.Common, 2);
            }
            if (GUILayout.Button("Enable Up to Rare"))
            {
                SetAllCardsAvailable(db, 0);
                SetCardsByRarity(db, CardRarity.Common, 2);
                SetCardsByRarity(db, CardRarity.Uncommon, 2);
                SetCardsByRarity(db, CardRarity.Rare, 1);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Deck Statistics", EditorStyles.boldLabel);
            ShowDeckStats(db);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        // Catalog Section
        showCatalog = EditorGUILayout.Foldout(showCatalog, $"Catalog ({db.Catalog.Count} cards)", true);
        if (showCatalog)
        {
            EditorGUI.indentLevel++;
            var catalogProp = serializedObject.FindProperty("catalog");
            EditorGUILayout.PropertyField(catalogProp, true);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Availability Section
        showAvailability = EditorGUILayout.Foldout(showAvailability, $"Availability ({db.AvailabilityList.Count} entries)", true);
        if (showAvailability)
        {
            EditorGUI.indentLevel++;

            // Filter by enabled/disabled
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter:", GUILayout.Width(50));
            if (GUILayout.Button("Show All"))
            {
                ShowAvailabilityFiltered(db, null);
            }
            if (GUILayout.Button("Enabled Only"))
            {
                ShowAvailabilityFiltered(db, true);
            }
            if (GUILayout.Button("Disabled Only"))
            {
                ShowAvailabilityFiltered(db, false);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            var availProp = serializedObject.FindProperty("availability");

            // Show each availability entry with quick controls
            for (int i = 0; i < availProp.arraySize; i++)
            {
                var elemProp = availProp.GetArrayElementAtIndex(i);
                var cardProp = elemProp.FindPropertyRelative("card");
                var availableProp = elemProp.FindPropertyRelative("available");
                var maxCopiesProp = elemProp.FindPropertyRelative("maxCopies");
                var rewardEligibleProp = elemProp.FindPropertyRelative("rewardEligible");

                var cardDef = cardProp.objectReferenceValue as CardDef;
                if (cardDef == null) continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Header line with card name (clickable to select asset)
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($"{cardDef.displayName}", EditorStyles.boldLabel, GUILayout.Width(150)))
                {
                    Selection.activeObject = cardDef;
                    EditorGUIUtility.PingObject(cardDef);
                }

                // Edit rarity directly
                EditorGUI.BeginChangeCheck();
                var newRarity = (CardRarity)EditorGUILayout.EnumPopup(cardDef.rarity, GUILayout.Width(100));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(cardDef, "Change Card Rarity");
                    cardDef.rarity = newRarity;
                    EditorUtility.SetDirty(cardDef);
                }

                // Edit energy cost directly
                EditorGUILayout.LabelField("Energy:", GUILayout.Width(50));
                EditorGUI.BeginChangeCheck();
                var newEnergy = EditorGUILayout.IntField(cardDef.energyCost, GUILayout.Width(30));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(cardDef, "Change Energy Cost");
                    cardDef.energyCost = Mathf.Max(0, newEnergy);
                    EditorUtility.SetDirty(cardDef);
                }

                EditorGUILayout.EndHorizontal();

                // Controls line
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Copies:", GUILayout.Width(50));
                EditorGUILayout.PropertyField(availableProp, GUIContent.none, GUILayout.Width(40));

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    availableProp.intValue = Mathf.Max(0, availableProp.intValue - 1);
                }
                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    availableProp.intValue = Mathf.Min(maxCopiesProp.intValue, availableProp.intValue + 1);
                }

                EditorGUILayout.PropertyField(rewardEligibleProp, new GUIContent("Reward?"), GUILayout.Width(100));

                // Button to open card asset for full editing
                if (GUILayout.Button("Edit Full", GUILayout.Width(60)))
                {
                    Selection.activeObject = cardDef;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Reward Weights
        var rewardWeightsProp = serializedObject.FindProperty("rewardWeights");
        EditorGUILayout.PropertyField(rewardWeightsProp, true);

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(db);
        }
    }

    private void SetAllCardsAvailable(CardDatabase db, int count)
    {
        var so = new SerializedObject(db);
        var availProp = so.FindProperty("availability");

        for (int i = 0; i < availProp.arraySize; i++)
        {
            var elem = availProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("available").intValue = count;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(db);
    }

    private void SetCardsByRarity(CardDatabase db, CardRarity rarity, int count)
    {
        var so = new SerializedObject(db);
        var availProp = so.FindProperty("availability");

        for (int i = 0; i < availProp.arraySize; i++)
        {
            var elem = availProp.GetArrayElementAtIndex(i);
            var cardRef = elem.FindPropertyRelative("card").objectReferenceValue as CardDef;
            if (cardRef && cardRef.rarity == rarity)
            {
                elem.FindPropertyRelative("available").intValue = count;
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(db);
    }

    private void ResetToDefaults(CardDatabase db)
    {
        // Set common cards to 2 copies, uncommon to 1, rest to 0
        var so = new SerializedObject(db);
        var availProp = so.FindProperty("availability");

        for (int i = 0; i < availProp.arraySize; i++)
        {
            var elem = availProp.GetArrayElementAtIndex(i);
            var cardRef = elem.FindPropertyRelative("card").objectReferenceValue as CardDef;
            if (cardRef)
            {
                int defaultCount = cardRef.rarity switch
                {
                    CardRarity.Common => 2,
                    CardRarity.Uncommon => 1,
                    _ => 0
                };
                elem.FindPropertyRelative("available").intValue = defaultCount;
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(db);
    }

    private void ShowDeckStats(CardDatabase db)
    {
        var deck = db.BuildPlayerDeck();
        var byRarity = deck.GroupBy(c => c.rarity)
                          .OrderBy(g => g.Key)
                          .ToDictionary(g => g.Key, g => g.Count());

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Total Cards in Deck: {deck.Count}", EditorStyles.boldLabel);

        foreach (var rarity in System.Enum.GetValues(typeof(CardRarity)).Cast<CardRarity>())
        {
            if (byRarity.TryGetValue(rarity, out int count) && count > 0)
            {
                EditorGUILayout.LabelField($"{rarity}: {count}");
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void ShowAvailabilityFiltered(CardDatabase db, bool? enabledFilter)
    {
        // This is just a visual helper - actual filtering would require state management
        // For now this just serves as a button action feedback
        if (enabledFilter.HasValue)
        {
            Debug.Log($"Filter set to show {(enabledFilter.Value ? "enabled" : "disabled")} cards only");
        }
    }
}
#endif
