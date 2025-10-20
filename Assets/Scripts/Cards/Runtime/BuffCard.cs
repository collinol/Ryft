using Game.Core; using Game.Combat;

namespace Game.Cards
{
    public class BuffCard : CardRuntime
    {
        // Choose which stat to buff by naming convention (displayName contains STR/MANA/ENG) or add a small param to description.
        protected override StatField CostField => StatField.Strength;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayCost()) return;

            var pc = Owner as Game.Player.PlayerCharacter;
            if (pc == null) return;

            // Simple heuristic: if name contains "Strength" buff strength; if "Mana" buff mana; if "Engineering" buff eng.
            StatField f = StatField.Strength;
            var name = (Def.displayName ?? "").ToLowerInvariant();
            if (name.Contains("mana")) f = StatField.Mana;
            else if (name.Contains("engineer") || name.Contains("overclock")) f = StatField.Engineering;

            var add = new Stats(); StatsUtil.Set(ref add, f, Def.power);
            pc.Gain(add);
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} and gains +{Def.power} {f} this turn.");
        }
    }
}
