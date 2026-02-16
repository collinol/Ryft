using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class InstallRailgunCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            WeaponInstallationManager.Instance?.InstallWeapon(WeaponType.Railgun);
            ctx.Log($"{Owner.DisplayName} installs Railgun. Single-target attacks splash 50% to others.");
        }
    }
}
