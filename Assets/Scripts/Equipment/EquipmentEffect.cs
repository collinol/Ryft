using Game.Combat;
using Game.Core;

namespace Game.Equipment
{
    public interface IEquipmentEffect
    {
        void Bind(IActor owner); // set once on equip
        void OnBattleStarted(FightContext ctx) {}
        void OnBattleEnded(FightContext ctx) {}
        void OnTurnStarted(FightContext ctx, IActor whoseTurn) {}
        void OnTurnEnded(FightContext ctx, IActor whoseTurn) {}
        void OnOwnerDamaged(FightContext ctx, IActor attacker, int damage) {}
        void OnOwnerDealtDamage(FightContext ctx, IActor target, int damage) {}
    }
}
