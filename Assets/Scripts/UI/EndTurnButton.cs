using UnityEngine;
using UnityEngine.UI;
using Game.Combat;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class EndTurnButton : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
                FindObjectOfType<FightSceneController>()?.EndPlayerTurnButton());
        }
    }
}
