using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class ReactiveArmorCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            int prot = GetScaledProtection();
            if (WeaponInstallationManager.Instance != null && WeaponInstallationManager.Instance.HasWeapon)
                prot += 4;
            GainProtection(prot);
            ctx.Log($"{Owner.DisplayName} gains {prot} protection from Reactive Armor.");
        }
    }
}
