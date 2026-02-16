using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class InstallMinigunCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            WeaponInstallationManager.Instance?.InstallWeapon(WeaponType.Minigun);
            ctx.Log($"{Owner.DisplayName} installs Minigun. Attacks hit 1 extra time at 50% damage.");
        }
    }
}
