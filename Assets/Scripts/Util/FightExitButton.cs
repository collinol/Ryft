using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class FightExitButton : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            // Simply load MapScene; MapController will detect MapSession.I.Saved and restore.
            SceneManager.LoadScene("MapScene", LoadSceneMode.Single);
        });
    }
}
