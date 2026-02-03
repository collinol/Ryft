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
            if (!string.IsNullOrEmpty(_returnScene))
            {
                string scene = _returnScene;
                _returnScene = null;
                SceneManager.LoadScene(scene);
            }
            else
            {
                // Fallback to MapScene if no return scene is set
                GoToMap();
            }
        }
    }
}
