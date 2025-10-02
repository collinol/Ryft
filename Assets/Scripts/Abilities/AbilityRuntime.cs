using System;
using System.Linq;
using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities
{
    public abstract class AbilityRuntime
    {
        public AbilityDef Def { get; private set; }
        public IActor Owner { get; private set; }
        public int CooldownRemaining { get; private set; }

        public void Bind(AbilityDef def, IActor owner)
        {
            Def = def;
            Owner = owner;
            CooldownRemaining = 0;
        }

        public bool IsReady => CooldownRemaining <= 0;

        public virtual bool CanUse(FightContext ctx) => Owner != null && Owner.IsAlive && IsReady;

        public abstract void Execute(FightContext ctx, IActor explicitTarget = null);

        public void PutOnCooldown() => CooldownRemaining = Mathf.Max(Def.baseCooldown, 0);
        public void TickCooldown()   => CooldownRemaining = Mathf.Max(0, CooldownRemaining - 1);
    }
}
