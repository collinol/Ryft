using UnityEngine;
using Game.Enemies;
using Game.Enemies.Elite;
using Game.UI;
using System.Collections.Generic;
using System;

namespace Game.Combat
{
    /// <summary>
    /// Spawns enemies at runtime with proper sprites, health bars, and components
    /// Can spawn random numbers of enemies for procedural encounters
    /// </summary>
    [DefaultExecutionOrder(-200)] // Run before FightSceneController
    public class RuntimeEnemySpawner : MonoBehaviour
    {
        public static RuntimeEnemySpawner Instance { get; private set; }

        [Header("Enemy Configuration")]
        [SerializeField] private GameObject enemyPrefab; // Optional: use a prefab
        [SerializeField] private Sprite goblinSprite;    // Assign in Inspector or use Resources
        [SerializeField] private string spriteResourcePath = "Sprites/Goblin"; // Path in Resources folder

        // Enemy type to class mapping
        private static readonly Dictionary<string, Type> EnemyTypes = new()
        {
            // Regular enemies
            { "Goblin", typeof(GoblinEnemy) },
            { "Skeleton", typeof(SkeletonEnemy) },
            { "Slime", typeof(SlimeEnemy) },
            { "Bandit", typeof(BanditEnemy) },
            { "Cultist", typeof(CultistEnemy) },
            // Elite enemies
            { "OrcChieftain", typeof(OrcChieftainEnemy) },
            { "DarkKnight", typeof(DarkKnightEnemy) },
            { "Golem", typeof(GolemEnemy) },
            { "Necromancer", typeof(NecromancerEnemy) },
        };

        private static readonly string[] RegularEnemyTypes = { "Goblin", "Skeleton", "Slime", "Bandit", "Cultist" };
        private static readonly string[] EliteEnemyTypes = { "OrcChieftain", "DarkKnight", "Golem", "Necromancer" };

        [Header("Spawn Settings")]
        [SerializeField] private int numberOfEnemies = 2;
        [SerializeField] private bool randomizeCount = false;
        [SerializeField] private int minEnemies = 2;
        [SerializeField] private int maxEnemies = 4;

        [Header("Positioning")]
        [SerializeField] private float baseY = 3.0f;  // Set to 3.0 to prevent health bars from overlapping card hand
        [SerializeField] private float spacingX = 4.0f;  // Set to 4.0 to prevent health bar overlap between enemies
        [SerializeField] private bool centerEnemies = true;

        [Header("Health Bar")]
        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private float healthBarOffsetY = -1.2f;  // Note: Health bars now created by EnemyBase.Awake()

        void Awake()
        {
            Instance = this;

            // Force correct Y position to avoid overlap with card hand UI
            baseY = 3.0f;
            // Force wider spacing to prevent health bar overlap
            spacingX = 4.0f;
            Debug.Log("[RuntimeEnemySpawner] Awake() called - forcing baseY=3.0, spacingX=4.0, spawning enemies...");

            // Check if this is an elite fight
            bool isElite = MapSession.I != null && MapSession.I.IsEliteFight;
            if (isElite)
            {
                SpawnEliteEncounter();
            }
            else
            {
                SpawnEnemies();
            }
        }

        [ContextMenu("Spawn Enemies")]
        public void SpawnEnemies()
        {
            Debug.Log("[RuntimeEnemySpawner] SpawnEnemies() called");

            // Clear any existing enemies first
            ClearExistingEnemies();

            // Determine how many to spawn
            int count = numberOfEnemies;
            if (randomizeCount)
            {
                count = Random.Range(minEnemies, maxEnemies + 1);
                Debug.Log($"[RuntimeEnemySpawner] Random count: {count} enemies");
            }

            Debug.Log($"[RuntimeEnemySpawner] Spawning {count} enemies at baseY={baseY}");

            // Calculate positions
            Vector3[] positions = CalculatePositions(count);

            // Spawn each enemy
            for (int i = 0; i < count; i++)
            {
                SpawnEnemy(positions[i], $"Goblin_{i + 1}");
            }

            Debug.Log($"[RuntimeEnemySpawner] Finished spawning {count} enemies");

            // Verify they were created
            var allEnemies = FindObjectsOfType<EnemyBase>();
            Debug.Log($"[RuntimeEnemySpawner] Verification: Found {allEnemies.Length} total EnemyBase components in scene");
        }

        private Vector3[] CalculatePositions(int count)
        {
            Vector3[] positions = new Vector3[count];

            // FORCE Y position to 3.0 to avoid card hand overlap
            const float FORCED_Y = 3.0f;

            if (count == 1)
            {
                // Single enemy in center
                positions[0] = new Vector3(0f, FORCED_Y, 0f);
            }
            else if (centerEnemies)
            {
                // Center the group
                float totalWidth = (count - 1) * spacingX;
                float startX = -totalWidth / 2f;

                for (int i = 0; i < count; i++)
                {
                    positions[i] = new Vector3(startX + (i * spacingX), FORCED_Y, 0f);
                }
            }
            else
            {
                // Start from left
                for (int i = 0; i < count; i++)
                {
                    positions[i] = new Vector3((i * spacingX) - spacingX, FORCED_Y, 0f);
                }
            }

            return positions;
        }

        private void SpawnEnemy(Vector3 position, string name)
        {
            // FORCE Y position to 3.0 (override any serialized baseY value)
            position.y = 3.0f;

            GameObject enemy;

            if (enemyPrefab != null)
            {
                // Use prefab if assigned
                enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
                enemy.name = name;
                Debug.Log($"[RuntimeEnemySpawner] Spawned from prefab: {name} at {position}");
            }
            else
            {
                // Create from scratch
                enemy = CreateEnemyFromScratch(position, name);
            }

            // Ensure it has a GoblinEnemy component
            if (enemy.GetComponent<GoblinEnemy>() == null)
            {
                enemy.AddComponent<GoblinEnemy>();
            }

            // Add EnemyClickTarget for clicking/targeting
            if (enemy.GetComponent<Game.Enemies.EnemyClickTarget>() == null)
            {
                enemy.AddComponent<Game.Enemies.EnemyClickTarget>();
                Debug.Log($"[RuntimeEnemySpawner] Added EnemyClickTarget to {enemy.name}");
            }

            // Note: Health bar is created automatically by EnemyBase.Awake()
            // No need to create it here to avoid duplication
        }

        private GameObject CreateEnemyFromScratch(Vector3 position, string name)
        {
            GameObject enemy = new GameObject(name);
            enemy.transform.position = position;

            // Add GoblinEnemy component (this will set up base stats)
            var goblinEnemy = enemy.AddComponent<GoblinEnemy>();

            // Add SpriteRenderer
            var sr = enemy.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;  // Enemies at 5, Player at 10, so enemies behind player

            // Try to load sprite
            Sprite sprite = LoadGoblinSprite();
            if (sprite != null)
            {
                sr.sprite = sprite;
                Debug.Log($"[RuntimeEnemySpawner] Loaded sprite for {name}");
            }
            else
            {
                Debug.LogWarning($"[RuntimeEnemySpawner] No sprite found for {name} - enemy will be invisible!");
            }

            // Add Collider for clicking
            var collider = enemy.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.5f, 2f);  // Larger collider for easier clicking
            collider.isTrigger = false;  // Not a trigger, needs to be clickable

            Debug.Log($"[RuntimeEnemySpawner] Created from scratch: {name} at {position}");

            return enemy;
        }

        private Sprite LoadGoblinSprite()
        {
            // Priority 1: Inspector-assigned sprite
            if (goblinSprite != null)
            {
                return goblinSprite;
            }

            // Priority 2: Load from Resources
            if (!string.IsNullOrEmpty(spriteResourcePath))
            {
                Sprite sprite = Resources.Load<Sprite>(spriteResourcePath);
                if (sprite != null)
                {
                    Debug.Log($"[RuntimeEnemySpawner] Loaded sprite from Resources: {spriteResourcePath}");
                    return sprite;
                }
            }

            // Priority 3: Find existing goblin in scene and copy its sprite
            var existingGoblin = FindObjectOfType<GoblinEnemy>();
            if (existingGoblin != null)
            {
                var sr = existingGoblin.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    Debug.Log($"[RuntimeEnemySpawner] Copied sprite from existing goblin");
                    return sr.sprite;
                }
            }

            Debug.LogWarning("[RuntimeEnemySpawner] Could not load goblin sprite from any source!");
            return null;
        }

        private void CreateHealthBar(GameObject enemy)
        {
            GameObject healthBar;

            if (healthBarPrefab != null)
            {
                // Use prefab if assigned
                healthBar = Instantiate(healthBarPrefab, enemy.transform);
                healthBar.transform.localPosition = new Vector3(0, healthBarOffsetY, 0);
            }
            else
            {
                // Create simple health bar from scratch
                healthBar = CreateSimpleHealthBar(enemy);
            }

            healthBar.name = "HealthBar";

            // Try to link it to the enemy
            var enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                // Use reflection to set the hpBar field if it exists
                var hpBarField = typeof(EnemyBase).GetField("hpBar",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (hpBarField != null)
                {
                    var healthBarView = healthBar.GetComponent<HealthBarView>();
                    if (healthBarView != null)
                    {
                        hpBarField.SetValue(enemyBase, healthBarView);
                        Debug.Log($"[RuntimeEnemySpawner] Linked health bar to {enemy.name}");
                    }
                }
            }
        }

        private GameObject CreateSimpleHealthBar(GameObject parent)
        {
            // Use HealthBarView instead of creating from scratch
            var enemyBase = parent.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                // Use the proper HealthBarView.Attach method
                var healthBarView = Game.UI.HealthBarView.Attach(
                    parent.transform,
                    new Vector3(0, healthBarOffsetY, 0),
                    new Vector2(1.2f, 0.15f)
                );

                Debug.Log($"[RuntimeEnemySpawner] Created HealthBarView for {parent.name}");
                return healthBarView.gameObject;
            }

            Debug.LogWarning($"[RuntimeEnemySpawner] No EnemyBase found on {parent.name}, skipping health bar");
            return null;
        }

        private void ClearExistingEnemies()
        {
            var enemies = FindObjectsOfType<GoblinEnemy>();
            foreach (var enemy in enemies)
            {
                if (Application.isPlaying)
                    Destroy(enemy.gameObject);
                else
                    DestroyImmediate(enemy.gameObject);
            }

            if (enemies.Length > 0)
            {
                Debug.Log($"[RuntimeEnemySpawner] Cleared {enemies.Length} existing enemies");
            }
        }

        [ContextMenu("Clear All Enemies")]
        public void ClearAllEnemies()
        {
            ClearExistingEnemies();
        }

        // Public API for dynamic spawning
        public void SpawnSpecificCount(int count)
        {
            numberOfEnemies = count;
            randomizeCount = false;
            SpawnEnemies();
        }

        public void SpawnRandomCount()
        {
            randomizeCount = true;
            SpawnEnemies();
        }

        /// <summary>
        /// Spawn an elite encounter - one elite enemy with optional minions.
        /// </summary>
        public void SpawnEliteEncounter()
        {
            Debug.Log("[RuntimeEnemySpawner] Spawning elite encounter");
            ClearExistingEnemies();

            // Pick a random elite type
            string eliteType = EliteEnemyTypes[UnityEngine.Random.Range(0, EliteEnemyTypes.Length)];
            Debug.Log($"[RuntimeEnemySpawner] Selected elite: {eliteType}");

            // Calculate positions - elite in center
            Vector3 elitePos = new Vector3(0f, 3.0f, 0f);
            SpawnEnemy(eliteType, elitePos, eliteType);

            // Some elites spawn with minions
            if (eliteType == "Necromancer")
            {
                // Necromancer starts with 2 skeletons
                SpawnEnemy("Skeleton", new Vector3(-4f, 3.0f, 0f), "Skeleton_1");
                SpawnEnemy("Skeleton", new Vector3(4f, 3.0f, 0f), "Skeleton_2");
            }
            else if (eliteType == "OrcChieftain")
            {
                // Orc Chieftain may have a goblin guard
                if (UnityEngine.Random.value < 0.5f)
                {
                    SpawnEnemy("Goblin", new Vector3(-4f, 3.0f, 0f), "OrcGuard_1");
                }
            }

            Debug.Log("[RuntimeEnemySpawner] Elite encounter spawned");
        }

        /// <summary>
        /// Spawn an enemy by type name at a specific position.
        /// </summary>
        public EnemyBase SpawnEnemy(string typeName, Vector3 position, string name = null)
        {
            position.y = 3.0f; // Force Y position

            if (!EnemyTypes.TryGetValue(typeName, out Type enemyType))
            {
                Debug.LogWarning($"[RuntimeEnemySpawner] Unknown enemy type: {typeName}, defaulting to Goblin");
                enemyType = typeof(GoblinEnemy);
            }

            GameObject enemy = new GameObject(name ?? typeName);
            enemy.transform.position = position;

            // Add the specific enemy component
            var enemyComponent = enemy.AddComponent(enemyType) as EnemyBase;

            // Add SpriteRenderer
            var sr = enemy.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;

            // Try to load sprite for this enemy type
            Sprite sprite = LoadSpriteForEnemy(typeName);
            if (sprite != null)
            {
                sr.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"[RuntimeEnemySpawner] No sprite found for {typeName}");
            }

            // Add Collider for clicking
            var collider = enemy.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.5f, 2f);
            collider.isTrigger = false;

            // Add EnemyClickTarget
            if (enemy.GetComponent<Game.Enemies.EnemyClickTarget>() == null)
            {
                enemy.AddComponent<Game.Enemies.EnemyClickTarget>();
            }

            Debug.Log($"[RuntimeEnemySpawner] Spawned {typeName}: {enemy.name} at {position}");
            return enemyComponent;
        }

        private Sprite LoadSpriteForEnemy(string typeName)
        {
            // Try to load from Resources/Sprites/{typeName}
            string path = $"Sprites/{typeName}";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null) return sprite;

            // Try alternate paths
            sprite = Resources.Load<Sprite>($"Enemies/{typeName}");
            if (sprite != null) return sprite;

            // Fallback to goblin sprite
            if (goblinSprite != null) return goblinSprite;

            // Try loading default goblin
            return Resources.Load<Sprite>(spriteResourcePath);
        }

        /// <summary>
        /// Get a random regular enemy type name.
        /// </summary>
        public static string GetRandomRegularEnemy()
        {
            return RegularEnemyTypes[UnityEngine.Random.Range(0, RegularEnemyTypes.Length)];
        }

        /// <summary>
        /// Get a random elite enemy type name.
        /// </summary>
        public static string GetRandomEliteEnemy()
        {
            return EliteEnemyTypes[UnityEngine.Random.Range(0, EliteEnemyTypes.Length)];
        }
    }
}
