using UnityEngine;
using Game.Util;

namespace Game.UI.Inventory
{
    /// <summary>
    /// Handles global input for the CharacterMenu scene, including returning to the previous scene with Escape
    /// </summary>
    public class CharacterMenuController : MonoBehaviour
    {
        void Update()
        {
            // Press Escape to return to the previous scene (likely MapScene)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToPreviousScene();
            }
        }

        void ReturnToPreviousScene()
        {
            Debug.Log("[CharacterMenu] Returning to previous scene");
            SceneRouter.ReturnToPreviousScene();
        }
    }
}
