using Game.Core;
using Game.Combat;
using System.Linq;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Chain Reaction - Destroy ALL devices. Deal 3^N damage to all enemies (N = count destroyed). Rewind keyword.
    /// </summary>
    public class ChainReactionCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            int destroyed = dm.DestroyAllAndCount();
            if (destroyed == 0)
            {
                ctx.Log($"{Owner.DisplayName} uses Chain Reaction but has no devices!");
                return;
            }

            int dmg = (int)Mathf.Pow(3, destroyed);
            int kills = DealDamageToAll(ctx, dmg);
            ctx.Log($"{Owner.DisplayName} triggers Chain Reaction! {destroyed} devices explode for {dmg} damage. ({kills} kills)");

            bool anyKilled = kills > 0;
            HandleRewind(anyKilled);
        }
    }
}
