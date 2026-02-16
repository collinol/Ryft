using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Combat;
using System.Text;

namespace Game.UI
{
    /// <summary>
    /// Displays active buffs/debuffs as colored text below an actor's health bar.
    /// Attach via StatusEffectDisplay.Attach() from PlayerCharacter or EnemyBase.
    /// </summary>
    public class StatusEffectDisplay : MonoBehaviour
    {
        private IActor actor;
        private Text label;
        private Canvas canvas;
        private readonly StringBuilder sb = new StringBuilder(128);

        private static readonly float UpdateInterval = 0.25f;
        private float nextUpdate;

        public static StatusEffectDisplay Attach(Transform owner, IActor actor, Vector3 localOffset)
        {
            var existing = owner.GetComponentInChildren<StatusEffectDisplay>(true);
            if (existing != null)
            {
                existing.actor = actor;
                return existing;
            }

            var go = new GameObject("StatusEffects", typeof(RectTransform));
            go.transform.SetParent(owner, false);
            go.transform.localPosition = localOffset;

            var display = go.AddComponent<StatusEffectDisplay>();
            display.actor = actor;
            display.Build();
            return display;
        }

        private void Build()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.sortingLayerName = "Default";
            canvas.sortingOrder = 101;
            transform.localScale = Vector3.one * 0.04f;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100f;

            var cg = gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(transform, false);
            label = labelGO.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 14;
            label.alignment = TextAnchor.UpperCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            label.supportRichText = true;
            label.color = Color.white;

            var lrt = label.rectTransform;
            lrt.sizeDelta = new Vector2(800, 100);
            lrt.anchoredPosition = Vector2.zero;
        }

        void Update()
        {
            if (Time.time < nextUpdate) return;
            nextUpdate = Time.time + UpdateInterval;
            Refresh();
        }

        private void Refresh()
        {
            if (actor == null || actor.StatusEffects == null || label == null)
            {
                if (label != null) label.text = "";
                return;
            }

            var effects = actor.StatusEffects.GetActiveEffects();
            if (effects.Count == 0)
            {
                label.text = "";
                return;
            }

            sb.Clear();
            foreach (var e in effects)
            {
                string name = GetDisplayName(e.Type);
                if (name == null) continue;

                if (sb.Length > 0) sb.Append("  ");

                string color = IsDebuff(e.Type) ? "#FF6666" : "#66CCFF";
                if (e.Type == StatusEffectType.Protection) color = "#AAAAAA";

                if (e.Stacks > 1 || ShowStacks(e.Type))
                    sb.Append($"<color={color}>{name} {e.Stacks}</color>");
                else
                    sb.Append($"<color={color}>{name}</color>");
            }

            label.text = sb.ToString();
        }

        private static string GetDisplayName(StatusEffectType type)
        {
            return type switch
            {
                // Debuffs
                StatusEffectType.Burning => "Burn",
                StatusEffectType.Frozen => "Frost",
                StatusEffectType.Cursed => "Curse",
                StatusEffectType.Agony => "Agony",
                StatusEffectType.Weakness => "Weak",
                StatusEffectType.Doom => "Doom",
                StatusEffectType.Stun => "Stun",

                // Buffs
                StatusEffectType.Protection => "Prot",
                StatusEffectType.Overclock => "OC",
                StatusEffectType.ReflectAll => "Reflect",
                StatusEffectType.DamageImmune => "Immune",
                StatusEffectType.FlameBarrier => "FBarrier",
                StatusEffectType.IceArmor => "IceArmor",
                StatusEffectType.PhoenixForm => "Phoenix",
                StatusEffectType.NextAttackBonus => "+Atk",
                StatusEffectType.NextAttackDouble => "x2 Atk",
                StatusEffectType.CannotAttack => "No Atk",
                StatusEffectType.AnalyzeWeakness => "Exposed",
                StatusEffectType.LivingBomb => "Bomb",
                StatusEffectType.MassSuffering => "Marked",
                StatusEffectType.FrozenImmune => "Permafrost",
                StatusEffectType.CreepingCold => "C.Cold",
                StatusEffectType.EnemySkipAttack => "Disabled",
                StatusEffectType.ElementalMastery => "E.Mastery",
                StatusEffectType.DecoyTarget => "Decoy",
                StatusEffectType.Taunt => "Taunt",
                StatusEffectType.DefenseUp => "Def Up",

                // Skip internal/mechanical effects
                _ => null
            };
        }

        private static bool IsDebuff(StatusEffectType type)
        {
            return type == StatusEffectType.Burning
                || type == StatusEffectType.Frozen
                || type == StatusEffectType.Cursed
                || type == StatusEffectType.Agony
                || type == StatusEffectType.Weakness
                || type == StatusEffectType.Doom
                || type == StatusEffectType.Stun
                || type == StatusEffectType.AnalyzeWeakness
                || type == StatusEffectType.LivingBomb
                || type == StatusEffectType.MassSuffering
                || type == StatusEffectType.EnemySkipAttack
                || type == StatusEffectType.CannotAttack;
        }

        private static bool ShowStacks(StatusEffectType type)
        {
            return type == StatusEffectType.Burning
                || type == StatusEffectType.Frozen
                || type == StatusEffectType.Cursed
                || type == StatusEffectType.Agony
                || type == StatusEffectType.Weakness
                || type == StatusEffectType.Doom
                || type == StatusEffectType.Protection
                || type == StatusEffectType.Overclock;
        }
    }
}
