using Game.Core;
using Game.Combat;

namespace Game.Cards.Wizard
{
    public class IceArmorCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int prot = GetScaledProtection();
            GainProtection(prot);

            Owner.StatusEffects?.AddEffect(StatusEffectType.IceArmor, 1);

            ctx.Log($"{Owner.DisplayName} uses Ice Armor, gaining {prot} protection and Ice Armor for 1 turn.");
        }
    }
}
