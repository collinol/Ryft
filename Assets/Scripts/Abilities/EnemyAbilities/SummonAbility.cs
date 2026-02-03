using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// Spawns additional enemies during combat (Necromancer ability).
    /// </summary>
    public class SummonAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            // Find RuntimeEnemySpawner to spawn new enemies
            var spawner = Object.FindObjectOfType<RuntimeEnemySpawner>();
            if (spawner == null)
            {
                ctx.Log($"{Owner.DisplayName} tries to summon but fails...");
                PutOnCooldown();
                return;
            }

            // Spawn a skeleton minion
            int spawnCount = Mathf.Clamp(Def.power / 10, 1, 2);

            for (int i = 0; i < spawnCount; i++)
            {
                // Position new enemy near the owner
                Vector3 spawnPos = Owner is MonoBehaviour mb
                    ? mb.transform.position + new Vector3(Random.Range(-1f, 1f), 0, 0)
                    : new Vector3(Random.Range(-2f, 2f), 3f, 0);

                var newEnemy = spawner.SpawnEnemy("Skeleton", spawnPos);
                if (newEnemy != null)
                {
                    ctx.AddEnemy(newEnemy);
                }
            }

            ctx.Log($"{Owner.DisplayName} summons {spawnCount} skeleton minion(s)!");

            PutOnCooldown();
        }
    }
}
