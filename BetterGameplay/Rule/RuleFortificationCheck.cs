using JetBrains.Annotations;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Parts;
using System;

namespace BetterGameplay.Rule
{
    public class RuleFortificationCheck : RulebookTargetEvent
    {
        public readonly ItemEntityWeapon Weapon;

        public readonly RuleRollD100 Roll;
        public int FortificationChance => Math.Max(0, Math.Min(100, (this.Target.Get<UnitPartFortification>()?.Value ?? 0)));
        public bool TargetUseFortification => FortificationChance > 0;
        public bool AutoPass { get; set; }
        public bool IsPassed
        {
            get
            {
                if (!this.AutoPass)
                {
                    return Roll > FortificationChance;
                }
                return true;
            }
        }
        public RuleAttackRoll AttackRoll { get; }
        public bool ForCritical { get; }
        public bool ForSneakAttack { get; }
        public bool ForPreciseStrike { get; }

        public RuleFortificationCheck([NotNull] RuleAttackRoll evt, [NotNull] ItemEntityWeapon weapon) : base(evt.Initiator, evt.Target)
        {
            this.Weapon = weapon;
            this.Roll = new RuleRollD100(Initiator);
            this.ForCritical = evt.IsCriticalConfirmed;
            this.ForSneakAttack = evt.IsSneakAttack;
            this.ForPreciseStrike = evt.PreciseStrike > 0;
            this.AttackRoll = evt;
        }

        public override void OnTrigger(RulebookEventContext context)
        {
            Rulebook.Trigger<RuleRollD100>(this.Roll);
        }
    }
}