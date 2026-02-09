using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI.Inventory
{
    /// <summary>
    /// Automatically initializes the CharacterMenuScene when it loads.
    /// This ensures the scene has all required components even if they weren't
    /// manually added to the scene in the editor.
    /// </summary>
    public static class CharacterMenuAutoInit
    {
        private static bool _registered = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnSceneLoaded()
        {
            // Only register once to avoid duplicate callbacks
            if (!_registered)
            {
                Debug.Log("[CharacterMenuAutoInit] RuntimeInitializeOnLoadMethod triggered, registering scene callback");
                SceneManager.sceneLoaded += OnSceneLoadedCallback;
                _registered = true;
            }

            // Also check if we're already in CharacterMenuScene
            if (SceneManager.GetActiveScene().name == "CharacterMenuScene")
            {
                Debug.Log("[CharacterMenuAutoInit] Already in CharacterMenuScene, initializing now");
                InitializeCharacterMenuScene();
            }
        }

        static void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
        {
            // Only handle CharacterMenuScene - do nothing for other scenes
            if (scene.name != "CharacterMenuScene")
            {
                return;
            }

            Debug.Log($"[CharacterMenuAutoInit] Scene '{scene.name}' loaded, initializing...");
            InitializeCharacterMenuScene();
        }

        static void InitializeCharacterMenuScene()
        {
            Debug.Log("[CharacterMenuAutoInit] Initializing CharacterMenuScene...");

            // Ensure CharacterMenuController exists (handles input and creates UI)
            if (Object.FindObjectOfType<CharacterMenuController>() == null)
            {
                var controllerGo = new GameObject("CharacterMenuController");
                controllerGo.AddComponent<CharacterMenuController>();
                Debug.Log("[CharacterMenuAutoInit] Created CharacterMenuController");
            }
            else
            {
                Debug.Log("[CharacterMenuAutoInit] CharacterMenuController already exists");
            }

            // Ensure CharacterMenuSetup exists (creates equipment grids)
            if (Object.FindObjectOfType<CharacterMenuSetup>() == null)
            {
                var setupGo = new GameObject("CharacterMenuSetup");
                setupGo.AddComponent<CharacterMenuSetup>();
                Debug.Log("[CharacterMenuAutoInit] Created CharacterMenuSetup");
            }
            else
            {
                Debug.Log("[CharacterMenuAutoInit] CharacterMenuSetup already exists");
            }
        }
    }
}
