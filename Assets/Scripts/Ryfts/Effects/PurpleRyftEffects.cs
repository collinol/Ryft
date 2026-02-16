using UnityEngine;
using Game.Combat;

namespace Game.Ryfts.Effects
{
    /// <summary>Heal 2 HP per equipped piece per turn.</summary>
    public class EquipmentHealPerTurnEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnTurnStart) return;

            int pieces = mgr.GetEquippedPieceCount();
            if (pieces <= 0) return;

            int heal = Def.intMagnitude * stacks * pieces;
            mgr.HealPlayer(heal);
            mgr.DebugLogEffectAction("HEAL", $"+{heal} HP ({Def.intMagnitude}/piece, {pieces} pieces)");
        }
    }

    /// <summary>+1 energy per equipped piece per turn.</summary>
    public class EquipmentEnergyPerTurnEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnTurnStart) return;

            int pieces = mgr.GetEquippedPieceCount();
            if (pieces <= 0) return;

            int energy = Def.intMagnitude * stacks * pieces;
            var fsc = FightSceneController.Instance;
            if (fsc != null)
            {
                fsc.GainEnergy(energy);
                mgr.DebugLogEffectAction("ENERGY", $"+{energy} energy ({Def.intMagnitude}/piece, {pieces} pieces)");
            }
        }
    }

    /// <summary>1% chance each equipped piece breaks after combat.</summary>
    public class EquipmentBreakChanceEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleEnd) return;

            var em = Game.Equipment.EquipmentManager.Instance;
            if (em == null) return;

            float breakChance = Def.floatMagnitude * stacks;

            foreach (Game.Equipment.EquipmentSlot slot in System.Enum.GetValues(typeof(Game.Equipment.EquipmentSlot)))
            {
                if (slot == Game.Equipment.EquipmentSlot.None) continue;
                var equipped = em.GetEquipped(slot);
                if (equipped == null || equipped.IsBroken) continue;

                if (Random.value < breakChance)
                {
                    // Damage to 0 durability
                    equipped.currentDurability = 0;
                    mgr.DebugLogEffectAction("BREAK", $"{equipped.def.displayName} broke! ({breakChance:P1} chance)");
                    Debug.Log($"[EquipmentBreak] {equipped.def.displayName} broke from ryft effect!");
                }
            }
        }
    }
}
