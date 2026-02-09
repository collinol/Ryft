using UnityEngine;
using Game.Enemies;
using Game.Ryfts;
using Game.RyftEntities;

namespace Game.Combat
{
    /// <summary>
    /// Spawns the RyftPortalEntity and enemies for the PortalFight scene.
    /// Enemies will target the portal instead of the player.
    /// Attach this to a GameObject in the PortalFight scene.
    /// </summary>
    [DefaultExecutionOrder(-200)] // Run before FightSceneController
    public class PortalFightSceneSetup : MonoBehaviour
    {
        [Header("Portal Settings")]
        [SerializeField] private int portalHealth = 50;
        [SerializeField] private Vector3 portalPosition = new Vector3(0f, 3.5f, 0f);

        [Header("Enemy Spawn Settings")]
        [SerializeField] private Transform leftSpawnPoint;
        [SerializeField] private Transform rightSpawnPoint;

        [Header("Fallback Positions (enemies on right side)")]
        [SerializeField] private Vector3 leftPosition = new Vector3(4f, 3.0f, 0f);
        [SerializeField] private Vector3 rightPosition = new Vector3(8f, 3.0f, 0f);

        [Header("Auto-Spawn")]
        [SerializeField] private bool autoSpawnOnAwake = true;
        [SerializeField] private bool forcePositions = true;

        /// <summary>
        /// The spawned portal instance.
        /// </summary>
        public RyftPortalEntity SpawnedPortal { get; private set; }

        void Awake()
        {
            if (forcePositions)
            {
                portalPosition = new Vector3(0f, 3.5f, 0f);
                leftPosition = new Vector3(4f, 3.0f, 0f);
                rightPosition = new Vector3(8f, 3.0f, 0f);
                Debug.Log($"[PortalFightSetup] Forced positions - Portal: {portalPosition}, Enemies: {leftPosition}, {rightPosition}");
            }

            if (autoSpawnOnAwake)
            {
                ClearAllEnemies();
                SpawnPortal();
                SpawnTwoEnemies();
            }
        }

        [ContextMenu("Spawn Portal")]
        public void SpawnPortal()
        {
            var existingPortal = FindObjectOfType<RyftPortalEntity>();
            if (existingPortal != null)
            {
                Debug.Log("[PortalFightSetup] Portal already exists, skipping spawn");
                SpawnedPortal = existingPortal;
                return;
            }

            RyftColor portalColor = RyftColor.Blue;
            if (MapSession.I != null)
            {
                portalColor = MapSession.I.PortalFightRyftColor;
                Debug.Log($"[PortalFightSetup] Using rift color from MapSession: {portalColor}");
            }

            var entityDef = RyftEntityDef.Load(portalColor);
            if (entityDef != null)
            {
                SpawnedPortal = entityDef.Spawn(portalPosition);
                Debug.Log($"[PortalFightSetup] Spawned {portalColor} portal from RyftEntityDef at {portalPosition} with {portalHealth} HP");
            }
            else
            {
                SpawnedPortal = CreatePortalFromComponent(portalColor, portalPosition);
                Debug.Log($"[PortalFightSetup] Spawned {portalColor} portal from component at {portalPosition} with {portalHealth} HP");
            }
        }

        private RyftPortalEntity CreatePortalFromComponent(RyftColor color, Vector3 position)
        {
            var go = new GameObject($"{color}RyftPortal");
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 0;

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

        [ContextMenu("Spawn Two Enemies")]
        public void SpawnTwoEnemies()
        {
            var existing = FindObjectsOfType<EnemyBase>();
            if (existing.Length >= 2)
            {
                Debug.Log("[PortalFightSetup] Two enemies already exist in scene, skipping spawn");
                return;
            }

            Vector3 pos1 = leftSpawnPoint != null ? leftSpawnPoint.position : leftPosition;
            Vector3 pos2 = rightSpawnPoint != null ? rightSpawnPoint.position : rightPosition;

            SpawnEnemyAtPosition(pos1, "Enemy_1");
            SpawnEnemyAtPosition(pos2, "Enemy_2");

            Debug.Log($"[PortalFightSetup] Spawned 2 enemies at {pos1} and {pos2}");
        }

        private void SpawnEnemyAtPosition(Vector3 position, string goName)
        {
            // Try RuntimeEnemySpawner.SpawnFromDef if Instance is ready
            var spawner = RuntimeEnemySpawner.Instance;
            var db = EnemyDatabase.Load();

            if (db != null)
            {
                int level = MapSession.I != null ? MapSession.I.CurrentMapLevel : 0;
                var def = db.GetRandom(EnemyTier.Regular, level);
                if (def != null)
                {
                    if (spawner != null)
                    {
                        spawner.SpawnFromDef(def, position, goName);
                    }
                    else
                    {
                        // Spawner not ready yet — do the same thing SpawnFromDef does
                        SpawnFromDefLocal(def, position, goName);
                    }
                    return;
                }
            }

            // Fallback: bare goblin
            Debug.LogWarning($"[PortalFightSetup] No database or def, spawning fallback goblin: {goName}");
            var go = new GameObject(goName);
            go.transform.position = position;
            go.AddComponent<GoblinEnemy>();

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;
            sr.sprite = CreateProceduralEnemySprite(Color.green);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 2f);
            col.isTrigger = false;

            go.AddComponent<EnemyClickTarget>();
        }

        /// <summary>
        /// Local version of SpawnFromDef for when RuntimeEnemySpawner.Instance isn't ready yet.
        /// </summary>
        private EnemyBase SpawnFromDefLocal(EnemyDef def, Vector3 position, string goName)
        {
            GameObject go = new GameObject(goName ?? def.displayName);
            go.SetActive(false);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;
            sr.sprite = def.sprite != null ? def.sprite : CreateProceduralEnemySprite(def.proceduralColor);

            EnemyBase enemy;
            if (!string.IsNullOrEmpty(def.runtimeTypeName))
            {
                System.Type customType = System.Type.GetType(def.runtimeTypeName);
                if (customType != null && typeof(EnemyBase).IsAssignableFrom(customType))
                    enemy = go.AddComponent(customType) as EnemyBase;
                else
                    enemy = go.AddComponent<GenericEnemy>();
            }
            else
            {
                enemy = go.AddComponent<GenericEnemy>();
            }

            enemy.InitFromDef(def);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 2f);
            col.isTrigger = false;
            go.AddComponent<EnemyClickTarget>();

            go.SetActive(true);
            Debug.Log($"[PortalFightSetup] SpawnFromDefLocal: {def.id} at {position}");
            return enemy;
        }

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
                    float dxL = (x - leftEyeX), dyL = (y - eyeY);
                    float dxR = (x - rightEyeX), dyR = (y - eyeY);

                    if (dxL * dxL + dyL * dyL < 16) pixels[y * size + x] = Color.white;
                    if (dxR * dxR + dyR * dyR < 16) pixels[y * size + x] = Color.white;
                    if (dxL * dxL + dyL * dyL < 4)  pixels[y * size + x] = Color.black;
                    if (dxR * dxR + dyR * dyR < 4)  pixels[y * size + x] = Color.black;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        }

        [ContextMenu("Clear All Enemies")]
        public void ClearAllEnemies()
        {
            var enemies = FindObjectsOfType<EnemyBase>();
            foreach (var enemy in enemies)
            {
                if (Application.isPlaying)
                    Destroy(enemy.gameObject);
                else
                    DestroyImmediate(enemy.gameObject);
            }
            Debug.Log($"[PortalFightSetup] Cleared {enemies.Length} enemies");
        }
    }
}
