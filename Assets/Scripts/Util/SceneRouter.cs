using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Util
{
    public static class SceneRouter
    {
        private static string _returnScene = null;

        public static void GoToMap() => SceneManager.LoadScene("MapScene");
        public static void GoToFight(string fightSceneName) => SceneManager.LoadScene(fightSceneName);

        public static void GoToCharacterMenu(string fromScene = null)
        {
            if (!string.IsNullOrEmpty(fromScene))
                _returnScene = fromScene;
            SceneManager.LoadScene("CharacterMenuScene");
        }

        public static void ReturnToPreviousScene()
        {
            Debug.Log($"[SceneRouter] ReturnToPreviousScene called, _returnScene={_returnScene ?? "NULL"}");
            if (!string.IsNullOrEmpty(_returnScene))
            {
                string scene = _returnScene;
                _returnScene = null;
                Debug.Log($"[SceneRouter] Loading return scene: {scene}");
                SceneManager.LoadScene(scene);
            }
            else
            {
                // Fallback to MapScene if no return scene is set
                Debug.Log("[SceneRouter] No return scene set, falling back to MapScene");
                GoToMap();
            }
        }
    }
}
