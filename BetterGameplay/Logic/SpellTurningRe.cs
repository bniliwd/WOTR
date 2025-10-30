using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Controllers;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using UnityEngine;

namespace SpellTurningRedone.Logic
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("07eef00f7c4a460eb5f128f4b9e4936b")]
    public class SpellTurningRe : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, ISubscriber, ITargetRulebookSubscriber
    {
        [InfoBox("If true works only for spells with SR")]
        [SerializeField]
        public bool m_SpellResistanceOnly;

        [SerializeField]
        [InfoBox("Only spells with any of those descriptors will be turned. None means all")]
        public SpellDescriptorWrapper m_SpellDescriptorOnly;

        [SerializeField]
        public BlueprintAbilityReference[] m_SpecificSpellsOnly;

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            //不影响自己的法术
            if (evt.Initiator.Equals(Owner))
            {
                return;
            }

            AbilityExecutionProcess result = evt.Result;
            if ((!m_SpellResistanceOnly || evt.Spell.SpellResistance)
                && evt.Success && result != null
                && evt.Spell.Blueprint.IsSpell && !evt.Spell.Blueprint.CanTargetPoint
                && (!(m_SpellDescriptorOnly != SpellDescriptor.None) || evt.Context.SpellDescriptor.HasAnyFlag(m_SpellDescriptorOnly))
                && (m_SpecificSpellsOnly.Length == 0 || m_SpecificSpellsOnly.HasReference(evt.Spell.Blueprint)))
            {
                evt.SetSuccess(value: false);
                evt.CancelAbilityExecution();
                AbilityExecutionContext context = result.Context.CloneFor(evt.Initiator, evt.Initiator);
                Game.Instance.AbilityExecutor.Execute(context);

                EventBus.RaiseEvent(delegate (ISpellTurningHandler h)
                {
                    h.HandleSpellTurned(evt.Initiator, Owner, evt.Spell);
                });
            }
        }
    }
}