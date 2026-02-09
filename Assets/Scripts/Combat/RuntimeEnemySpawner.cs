using UnityEngine;
using Game.Enemies;
using Game.UI;
using System;

namespace Game.Combat
{
    /// <summary>
    /// Spawns enemies at runtime with proper sprites, health bars, and components.
    /// Uses EnemyDatabase to pick random enemies by tier and level.
    /// </summary>
    [DefaultExecutionOrder(-200)] // Run before FightSceneController
    public class RuntimeEnemySpawner : MonoBehaviour
    {
        public static RuntimeEnemySpawner Instance { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] private int numberOfEnemies = 2;
        [SerializeField] private bool randomizeCount = false;
        [SerializeField] private int minEnemies = 2;
        [SerializeField] private int maxEnemies = 4;

        [Header("Positioning")]
        [SerializeField] private float spacingX = 4.0f;
        [SerializeField] private bool centerEnemies = true;

        private EnemyDatabase enemyDb;

        private const float FORCED_Y = 3.0f;

        void Awake()
        {
            Instance = this;

            spacingX = 4.0f;
            Debug.Log("[RuntimeEnemySpawner] Awake() called - forcing baseY=3.0, spacingX=4.0");

            enemyDb = EnemyDatabase.Load();
            if (enemyDb == null)
                Debug.LogWarning("[RuntimeEnemySpawner] EnemyDatabase not found — spawning will use fallback behaviour.");

            // Check if PortalFightSceneSetup exists - if so, let it handle enemy spawning
            var portalSetup = FindObjectOfType<PortalFightSceneSetup>();
            if (portalSetup != null)
            {
                Debug.Log("[RuntimeEnemySpawner] PortalFightSceneSetup detected - skipping enemy spawn (portal fight handles its own enemies)");
                return;
            }

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

        // ─────────────────── Core spawn method ───────────────────

        /// <summary>
        /// Spawn a single enemy from an EnemyDef at the given position.
        /// </summary>
        public EnemyBase SpawnFromDef(EnemyDef def, Vector3 position, string goName = null)
        {
            if (def == null)
            {
                Debug.LogError("[RuntimeEnemySpawner] SpawnFromDef called with null def");
                return null;
            }

            position.y = FORCED_Y;

            // Create GO inactive so we can set up everything before Awake fires
            GameObject go = new GameObject(goName ?? def.displayName);
            go.SetActive(false);
            go.transform.position = position;

            // SpriteRenderer
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;
            sr.sprite = def.sprite != null ? def.sprite : CreateProceduralEnemySprite(def.proceduralColor);

            // Enemy component — custom type or GenericEnemy
            EnemyBase enemy;
            if (!string.IsNullOrEmpty(def.runtimeTypeName))
            {
                Type customType = Type.GetType(def.runtimeTypeName);
                if (customType != null && typeof(EnemyBase).IsAssignableFrom(customType))
                {
                    enemy = go.AddComponent(customType) as EnemyBase;
                }
                else
                {
                    Debug.LogWarning($"[RuntimeEnemySpawner] Type '{def.runtimeTypeName}' not found or invalid, using GenericEnemy");
                    enemy = go.AddComponent<GenericEnemy>();
                }
            }
            else
            {
                enemy = go.AddComponent<GenericEnemy>();
            }

            // Push def data before Awake
            enemy.InitFromDef(def);

            // Collider + click target
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 2f);
            col.isTrigger = false;
            go.AddComponent<EnemyClickTarget>();

            // Activate — Awake fires now with correct stats
            go.SetActive(true);

            Debug.Log($"[RuntimeEnemySpawner] SpawnFromDef: {def.id} ({def.displayName}) at {position}");
            return enemy;
        }

        // ─────────────────── Regular encounter ───────────────────

        [ContextMenu("Spawn Enemies")]
        public void SpawnEnemies()
        {
            Debug.Log("[RuntimeEnemySpawner] SpawnEnemies() called");
            ClearExistingEnemies();

            int count = numberOfEnemies;
            if (randomizeCount)
            {
                count = UnityEngine.Random.Range(minEnemies, maxEnemies + 1);
                Debug.Log($"[RuntimeEnemySpawner] Random count: {count} enemies");
            }

            Debug.Log($"[RuntimeEnemySpawner] Spawning {count} enemies");

            Vector3[] positions = CalculatePositions(count);
            int level = MapSession.I != null ? MapSession.I.CurrentMapLevel : 0;

            for (int i = 0; i < count; i++)
            {
                EnemyDef def = enemyDb != null ? enemyDb.GetRandom(EnemyTier.Regular, level) : null;

                if (def != null)
                {
                    SpawnFromDef(def, positions[i], $"{def.displayName}_{i + 1}");
                }
                else
                {
                    // Fallback: create a goblin the old way
                    FallbackSpawnGoblin(positions[i], $"Goblin_{i + 1}");
                }
            }

            Debug.Log($"[RuntimeEnemySpawner] Finished spawning {count} enemies");
            var allEnemies = FindObjectsOfType<EnemyBase>();
            Debug.Log($"[RuntimeEnemySpawner] Verification: Found {allEnemies.Length} total EnemyBase components in scene");
        }

        // ─────────────────── Elite encounter ───────────────────

        /// <summary>
        /// Spawn an elite encounter — one elite enemy with optional minions from its def.
        /// </summary>
        public void SpawnEliteEncounter()
        {
            Debug.Log("[RuntimeEnemySpawner] Spawning elite encounter");
            ClearExistingEnemies();

            int level = MapSession.I != null ? MapSession.I.CurrentMapLevel : 0;
            EnemyDef eliteDef = enemyDb != null ? enemyDb.GetRandom(EnemyTier.Elite, level) : null;

            if (eliteDef == null)
            {
                Debug.LogWarning("[RuntimeEnemySpawner] No elite def found in database, spawning fallback goblin");
                FallbackSpawnGoblin(new Vector3(0f, FORCED_Y, 0f), "FallbackElite");
                return;
            }

            // Spawn the elite in center
            Vector3 elitePos = new Vector3(0f, FORCED_Y, 0f);
            SpawnFromDef(eliteDef, elitePos, eliteDef.displayName);

            // Spawn minions based on def
            if (eliteDef.minions != null)
            {
                int minionIndex = 0;
                foreach (var entry in eliteDef.minions)
                {
                    if (entry.minionDef == null) continue;
                    if (UnityEngine.Random.value > entry.spawnChance) continue;

                    for (int i = 0; i < entry.count; i++)
                    {
                        float xOffset = (minionIndex % 2 == 0 ? -1 : 1) * spacingX * ((minionIndex / 2) + 1);
                        Vector3 minionPos = new Vector3(xOffset, FORCED_Y, 0f);
                        SpawnFromDef(entry.minionDef, minionPos, $"{entry.minionDef.displayName}_{minionIndex + 1}");
                        minionIndex++;
                    }
                }
            }

            Debug.Log("[RuntimeEnemySpawner] Elite encounter spawned");
        }

        // ─────────────────── Backward-compat string overload ───────────────────

        /// <summary>
        /// Spawn an enemy by id/name string. Used by SummonAbility etc.
        /// </summary>
        public EnemyBase SpawnEnemy(string typeName, Vector3 position, string goName = null)
        {
            // Try database lookup first
            if (enemyDb != null)
            {
                // Try exact id match, then try with "Enemy_" prefix
                EnemyDef def = enemyDb.Get(typeName)
                            ?? enemyDb.Get($"Enemy_{typeName}");
                if (def != null)
                    return SpawnFromDef(def, position, goName ?? typeName);
            }

            // Fallback: create old-style goblin with the given name
            Debug.LogWarning($"[RuntimeEnemySpawner] No def found for '{typeName}', spawning fallback");
            return FallbackSpawnGoblin(position, goName ?? typeName);
        }

        // ─────────────────── Positioning ───────────────────

        private Vector3[] CalculatePositions(int count)
        {
            Vector3[] positions = new Vector3[count];

            if (count == 1)
            {
                positions[0] = new Vector3(0f, FORCED_Y, 0f);
            }
            else if (centerEnemies)
            {
                float totalWidth = (count - 1) * spacingX;
                float startX = -totalWidth / 2f;
                for (int i = 0; i < count; i++)
                    positions[i] = new Vector3(startX + (i * spacingX), FORCED_Y, 0f);
            }
            else
            {
                for (int i = 0; i < count; i++)
                    positions[i] = new Vector3((i * spacingX) - spacingX, FORCED_Y, 0f);
            }

            return positions;
        }

        // ─────────────────── Clear ───────────────────

        private void ClearExistingEnemies()
        {
            var enemies = FindObjectsOfType<EnemyBase>();
            foreach (var enemy in enemies)
            {
                // DestroyImmediate so pre-placed scene enemies are gone
                // before we spawn replacements in the same frame
                DestroyImmediate(enemy.gameObject);
            }

            if (enemies.Length > 0)
                Debug.Log($"[RuntimeEnemySpawner] Cleared {enemies.Length} existing enemies");
        }

        [ContextMenu("Clear All Enemies")]
        public void ClearAllEnemies()
        {
            ClearExistingEnemies();
        }

        // ─────────────────── Public helpers ───────────────────

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

        // ─────────────────── Fallback (no database) ───────────────────

        private EnemyBase FallbackSpawnGoblin(Vector3 position, string goName)
        {
            position.y = FORCED_Y;

            GameObject go = new GameObject(goName);
            go.transform.position = position;

            var enemy = go.AddComponent<GoblinEnemy>();

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;
            sr.sprite = CreateProceduralEnemySprite(Color.green);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 2f);
            col.isTrigger = false;

            go.AddComponent<EnemyClickTarget>();

            Debug.Log($"[RuntimeEnemySpawner] Fallback spawned Goblin: {goName} at {position}");
            return enemy;
        }

        // ─────────────────── Procedural sprite ───────────────────

        private Sprite CreateProceduralEnemySprite(Color color)
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;

            int centerX = size / 2;
            int centerY = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - centerX) / 20f;
                    float dy = (y - centerY) / 28f;
                    if (dx * dx + dy * dy < 1f)
                        pixels[y * size + x] = color;

                    float eyeY = centerY + 8;
                    float leftEyeX = centerX - 8;
                    float rightEyeX = centerX + 8;

                    float dxL = (x - leftEyeX);
                    float dyL = (y - eyeY);
                    float dxR = (x - rightEyeX);
                    float dyR = (y - eyeY);

                    if (dxL * dxL + dyL * dyL < 16)
                        pixels[y * size + x] = Color.white;
                    if (dxR * dxR + dyR * dyR < 16)
                        pixels[y * size + x] = Color.white;

                    if (dxL * dxL + dyL * dyL < 4)
                        pixels[y * size + x] = Color.black;
                    if (dxR * dxR + dyR * dyR < 4)
                        pixels[y * size + x] = Color.black;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        }
    }
}
