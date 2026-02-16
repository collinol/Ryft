using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class InstallCryoCannonCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            WeaponInstallationManager.Instance?.InstallWeapon(WeaponType.CryoCannon);
            ctx.Log($"{Owner.DisplayName} installs Cryo Cannon. Attacks apply 2 Frozen.");
        }
    }
}
