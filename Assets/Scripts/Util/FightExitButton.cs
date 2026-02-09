using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class FightExitButton : MonoBehaviour
{
    [Header("Positioning")]
    [SerializeField] private bool autoPosition = true;
    [SerializeField] private Vector2 position = new Vector2(-10, -10); // Top-right offset

    void Awake()
    {
        // Auto-position to top-right to avoid overlapping with stats label
        if (autoPosition)
        {
            PositionAtTopRight();
        }

        GetComponent<Button>().onClick.AddListener(() =>
        {
            // Simply load MapScene; MapController will detect MapSession.I.Saved and restore.
            SceneManager.LoadScene("MapScene", LoadSceneMode.Single);
        });
    }

    private void PositionAtTopRight()
    {
        var rt = GetComponent<RectTransform>();
        if (rt == null) return;

        // Anchor to top-right
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = position;

        Debug.Log($"[FightExitButton] Positioned at top-right: {position}");
    }
}
