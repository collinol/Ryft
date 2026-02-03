using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Phoenix Form - When reduced to 0 HP, restore to full and double spell power.
    /// </summary>
    public class PhoenixForm : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        private bool used = false;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var deathPrevention = DeathPreventionSystem.Instance;
            if (deathPrevention != null)
            {
                deathPrevention.RegisterPrevention(Owner, DeathPreventionType.PhoenixForm, Def.id, (actor) => {
                    // Double spell power (Mana)
                    var player = actor as Game.Player.PlayerCharacter;
                    if (player != null)
                    {
                        player.Gain(new Stats { mana = player.CurrentTurnStats.mana }, allowExceedCap: true);
                    }
                });
                ctx.Log($"{Owner.DisplayName} assumes Phoenix Form! Will rise from death with full HP and doubled spell power.");
            }
            used = false;
        }
    }
}
