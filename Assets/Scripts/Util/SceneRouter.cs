using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Util
{
    public static class SceneRouter
    {
        public static void GoToMap() => SceneManager.LoadScene("MapScene");
        public static void GoToFight(string fightSceneName) => SceneManager.LoadScene(fightSceneName);
    }
}
