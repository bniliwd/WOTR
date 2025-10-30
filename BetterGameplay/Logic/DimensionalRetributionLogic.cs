using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using UnityEngine;

namespace BetterGameplay.Logic
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("df76ed3ead394b31a24b8bea647810d5")]
    public class DimensionalRetributionLogic : UnitFactComponentDelegate<DweomerLeapLogicData>, IApplyAbilityEffectHandler, IGlobalSubscriber, ISubscriber
    {
        // Ability to cast when triggered.
        [SerializeField]
        public BlueprintAbilityReference m_Ability;

        private BlueprintAbility Ability => m_Ability.Get();

        private Feet m_MaxDistance = 50.Feet();

        public override void OnActivate()
        {
            base.Data.AppliedAbility = base.Owner.AddFact<Ability>(Ability, null, null);
        }

        public override void OnDeactivate()
        {
            base.Owner.RemoveFact(base.Data.AppliedAbility);
            base.Data.AppliedAbility = null;
        }

        public void OnAbilityEffectApplied(AbilityExecutionContext context) {}

        public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
        {
            if(context.MaybeCaster?.IsEnemy(target.Unit) ?? false)
            {
                Feet weaponRange = base.Owner.GetThreatHand()?.Weapon?.AttackRange ?? GameConsts.MinWeaponRange;
                float attackRange = (weaponRange > 5.Feet() ? weaponRange : 5.Feet()).Meters + (base.Owner.View.Corpulence + context.Caster.View.Corpulence);
                float distance = base.Owner.DistanceTo(context.Caster);

                if (target.Unit == base.Owner
                    && context.SourceAbility.IsSpell
                    && (distance > attackRange && distance <= m_MaxDistance.Meters)
                    && base.Owner.State.CanAct
                    && base.Owner.CombatState.CanAttackOfOpportunity)
                {
                    Rulebook.Trigger(new RuleCastSpell(this.Data.AppliedAbility, context.Caster)
                    {
                        IsDuplicateSpellApplied = true
                    });
                }
            }
        }

        public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, TargetWrapper target)
        {
        }
        
    }
}