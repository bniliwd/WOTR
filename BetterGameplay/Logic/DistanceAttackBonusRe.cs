using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;

namespace BetterGameplay.Logic
{
    [TypeId("33cd4bf27904489991e6f17f3fd79fc3")]
    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [ComponentName("Distance attack bonus re")]
    public class DistanceAttackBonusRe : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAttackBonus>, IRulebookHandler<RuleCalculateAttackBonus>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public Feet Close;

        public Feet Range;

        public int AttackBonus;

        public ModifierDescriptor Descriptor = ModifierDescriptor.UntypedStackable;

        public void OnEventAboutToTrigger(RuleCalculateAttackBonus evt)
        {
            if (evt.Weapon != null && evt.Weapon.Blueprint.IsRanged)
            {
                float distance = evt.Target.DistanceTo(evt.Initiator);
                float corpulence = evt.Target.Corpulence + evt.Initiator.Corpulence;
                if(distance > Range.Meters + corpulence) { return; }

                int bonus = distance <= Close.Meters + corpulence ? AttackBonus * 2 : AttackBonus;
                evt.AddModifier(bonus, base.Fact, Descriptor);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonus evt)
        {
        }
    }
}