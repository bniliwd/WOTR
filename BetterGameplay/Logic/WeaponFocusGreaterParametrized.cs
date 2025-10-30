using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using System.Linq;
using UnityEngine;

namespace BetterGameplay.Logic
{
    [AllowedOn(typeof(BlueprintParametrizedFeature), false)]
    [TypeId("1f3431bc5c764d07840b058edec5d619")]
    public class WeaponFocusGreaterParametrized : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, ISubscriber, IInitiatorRulebookSubscriber
    {
        [SerializeField]
        public BlueprintUnitFactReference m_MythicFocus;

        public ModifierDescriptor Descriptor = ModifierDescriptor.UntypedStackable;

        public BlueprintUnitFact MythicFocus => m_MythicFocus?.Get();

        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            int bonus = ((!evt.Initiator.Progression.Features.Enumerable.Any(p => p.Param == base.Param && p.Blueprint == MythicFocus)) ? 1 : 3);
            if (evt.Weapon.Blueprint.Type.Category == base.Param)
            {
                evt.AddModifier(bonus, base.Fact, Descriptor);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt) {}
    }
}