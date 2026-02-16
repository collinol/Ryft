using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class InstallFlamethrowerCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            WeaponInstallationManager.Instance?.InstallWeapon(WeaponType.Flamethrower);
            ctx.Log($"{Owner.DisplayName} installs Flamethrower. Attacks apply 2 Burning.");
        }
    }
}
