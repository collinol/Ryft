using UnityEngine;
using Game.Enemies;
using Game.Ryfts;
using Game.RyftEntities;

namespace Game.Combat
{
    /// <summary>
    /// Spawns the RyftPortalEntity and Goblin enemies for the PortalFight scene.
    /// Enemies will target the portal instead of the player.
    /// Attach this to a GameObject in the PortalFight scene.
    /// </summary>
    [DefaultExecutionOrder(-200)] // Run before FightSceneController
    public class PortalFightSceneSetup : MonoBehaviour
    {
        [Header("Portal Settings")]
        [SerializeField] private int portalHealth = 50;
        [SerializeField] private Vector3 portalPosition = new Vector3(0f, 2.5f, 0f); // Centered, above cards

        [Header("Enemy Spawn Settings")]
        [SerializeField] private GameObject goblinPrefab;
        [SerializeField] private Transform leftSpawnPoint;
        [SerializeField] private Transform rightSpawnPoint;

        [Header("Fallback Positions")]
        [SerializeField] private Vector3 leftPosition = new Vector3(-3f, 3.0f, 0f);
        [SerializeField] private Vector3 rightPosition = new Vector3(3f, 3.0f, 0f);

        [Header("Auto-Spawn")]
        [SerializeField] private bool autoSpawnOnAwake = true;

        /// <summary>
        /// The spawned portal instance.
        /// </summary>
        public RyftPortalEntity SpawnedPortal { get; private set; }

        void Awake()
        {
            if (autoSpawnOnAwake)
            {
                SpawnPortal();
                SpawnTwoGoblins();
            }
        }

        [ContextMenu("Spawn Portal")]
        public void SpawnPortal()
        {
            // Check if portal already exists
            var existingPortal = FindObjectOfType<RyftPortalEntity>();
            if (existingPortal != null)
            {
                Debug.Log("[PortalFightSetup] Portal already exists, skipping spawn");
                SpawnedPortal = existingPortal;
                return;
            }

            // Get the rift color from MapSession (set when clicking the rift on the map)
            RyftColor portalColor = RyftColor.Blue; // Default
            if (MapSession.I != null)
            {
                portalColor = MapSession.I.PortalFightRyftColor;
                Debug.Log($"[PortalFightSetup] Using rift color from MapSession: {portalColor}");
            }

            // Try to load the RyftEntityDef asset for this color
            var entityDef = RyftEntityDef.Load(portalColor);
            if (entityDef != null)
            {
                SpawnedPortal = entityDef.Spawn(portalPosition);
                Debug.Log($"[PortalFightSetup] Spawned {portalColor} portal from RyftEntityDef at {portalPosition} with {portalHealth} HP");
            }
            else
            {
                // Fallback: create portal directly from component
                SpawnedPortal = CreatePortalFromComponent(portalColor, portalPosition);
                Debug.Log($"[PortalFightSetup] Spawned {portalColor} portal from component at {portalPosition} with {portalHealth} HP");
            }
        }

        /// <summary>
        /// Create a portal directly by adding the appropriate component.
        /// Used as fallback when no RyftEntityDef asset is found.
        /// </summary>
        private RyftPortalEntity CreatePortalFromComponent(RyftColor color, Vector3 position)
        {
            var go = new GameObject($"{color}RyftPortal");
            go.transform.position = position;
            // Note: Scale will be set by RyftPortalEntity.NormalizeSpriteSize() in Start()

            // Add SpriteRenderer
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 0;

            // Add the correct Ryft component based on color
            RyftPortalEntity entity = color switch
            {
                RyftColor.Orange => go.AddComponent<OrangeRyft>(),
                RyftColor.Green => go.AddComponent<GreenRyft>(),
                RyftColor.Blue => go.AddComponent<BlueRyft>(),
                RyftColor.Purple => go.AddComponent<PurpleRyft>(),
                _ => go.AddComponent<BlueRyft>()
            };

            entity.Initialize(portalHealth);
            return entity;
        }

        [ContextMenu("Spawn Two Goblins")]
        public void SpawnTwoGoblins()
        {
            // Check if goblins already exist in scene
            var existingGoblins = FindObjectsOfType<GoblinEnemy>();
            if (existingGoblins.Length >= 2)
            {
                Debug.Log("[PortalFightSetup] Two goblins already exist in scene, skipping spawn");
                return;
            }

            // Spawn first goblin (left)
            Vector3 pos1 = leftSpawnPoint != null ? leftSpawnPoint.position : leftPosition;
            GameObject goblin1 = SpawnGoblin(pos1, "Goblin_1");

            // Spawn second goblin (right)
            Vector3 pos2 = rightSpawnPoint != null ? rightSpawnPoint.position : rightPosition;
            GameObject goblin2 = SpawnGoblin(pos2, "Goblin_2");

            Debug.Log($"[PortalFightSetup] Spawned 2 goblins at {pos1} and {pos2}");
        }

        private GameObject SpawnGoblin(Vector3 position, string name)
        {
            GameObject goblin;

            if (goblinPrefab != null)
            {
                // Use prefab if assigned
                goblin = Instantiate(goblinPrefab, position, Quaternion.identity);
                goblin.name = name;
            }
            else
            {
                // Create from scratch if no prefab
                goblin = new GameObject(name);
                goblin.transform.position = position;

                // Add GoblinEnemy component
                var goblinEnemy = goblin.AddComponent<GoblinEnemy>();

                // Add SpriteRenderer if needed
                if (goblin.GetComponent<SpriteRenderer>() == null)
                {
                    var sr = goblin.AddComponent<SpriteRenderer>();
                    sr.sortingLayerName = "Default";
                    sr.sortingOrder = 1;
                }

                Debug.Log($"[PortalFightSetup] Created goblin from scratch: {name}");
            }

            return goblin;
        }

        [ContextMenu("Clear All Goblins")]
        public void ClearAllGoblins()
        {
            var goblins = FindObjectsOfType<GoblinEnemy>();
            foreach (var goblin in goblins)
            {
                if (Application.isPlaying)
                    Destroy(goblin.gameObject);
                else
                    DestroyImmediate(goblin.gameObject);
            }
            Debug.Log($"[PortalFightSetup] Cleared {goblins.Length} goblins");
        }
    }
}
