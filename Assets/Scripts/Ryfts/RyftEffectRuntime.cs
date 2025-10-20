using UnityEngine;

namespace Game.Ryfts
{
    public abstract class RyftEffectRuntime
    {
        public RyftEffectDef Def { get; private set; }

        // stateful runtime data
        public int stacks { get; protected set; } = 1;
        public int turnsRemaining { get; protected set; } = 0;
        public int delayRemaining { get; protected set; } = 0;
        public int internalCdRemaining { get; protected set; } = 0;
        public virtual void OnStacksChanged(RyftEffectManager mgr, int delta) { }

        public void Bind(RyftEffectDef def)
        {
            Def = def;
            delayRemaining = Mathf.Max(0, def.delayTurns);
            turnsRemaining = def.lifetime == EffectLifetime.DurationNTurns ? Mathf.Max(1, def.durationTurns) : 0;
        }

        public virtual void OnAdded(RyftEffectManager mgr) { }
        public virtual void OnRemoved(RyftEffectManager mgr) { }

        public virtual void TickTurn(RyftEffectManager mgr)
        {
            if (internalCdRemaining > 0) internalCdRemaining--;
            if (Def.lifetime == EffectLifetime.DurationNTurns && turnsRemaining > 0)
            {
                turnsRemaining--;
                if (turnsRemaining <= 0) mgr.RemoveEffect(this);
            }
        }

        protected bool ShouldProc()
        {
            if (delayRemaining > 0) { delayRemaining--; return false; }
            if (internalCdRemaining > 0) return false;

            var pct = EffectiveChancePercent;
            if (pct >= 100f) return true;
            return UnityEngine.Random.value <= (pct / 100f);
        }

        public float CurrentProcPercent =>
            (Def == null || delayRemaining > 0 || internalCdRemaining > 0) ? 0f : EffectiveChancePercent;

        public float EffectiveChancePercent =>
            Mathf.Clamp((Def?.chancePercent ?? 0f) * Mathf.Max(1, stacks), 0f, 100f);

        public virtual void AddStack(int count = 1, bool refreshDuration = false, RyftEffectManager mgr = null)
        {
            int before = stacks;
            stacks = Mathf.Min(Def.maxStacks > 0 ? Def.maxStacks : 9999, stacks + Mathf.Max(1, count));
            if (refreshDuration && Def.lifetime == EffectLifetime.DurationNTurns)
                turnsRemaining = Mathf.Max(1, Def.durationTurns);
            if (mgr != null) OnStacksChanged(mgr, stacks - before);
        }

        protected void StartInternalCooldown() => internalCdRemaining = Mathf.Max(0, Def.internalCooldownTurns);

        public abstract void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx);
    }
}
