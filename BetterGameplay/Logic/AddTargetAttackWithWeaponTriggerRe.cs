using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.Utility;

namespace BetterGameplay.Logic
{
    [TypeId("a1d393bc738946dcbb3be32f13641f92")]
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [AllowMultipleComponents]
    public class AddTargetAttackWithWeaponTriggerRe : UnitFactComponentDelegate, IRulebookHandler<RuleAttackWithWeapon>, ITargetRulebookHandler<RuleAttackWithWeapon>, ISubscriber, ITargetRulebookSubscriber, IRulebookHandler<RuleAttackWithWeaponResolve>, ITargetRulebookHandler<RuleAttackWithWeaponResolve>
    {
        public bool TriggerBeforeAttack = true;

        [HideIf("TriggerBeforeAttack")]
        public bool WaitForAttackResolve;

        [ShowIf("TriggerAfterNotCritical")]
        public bool OnlyHit;

        public bool OnAttackOfOpportunity;

        public bool OnlyMelee;

        public bool OnlyRanged;

        public bool NotReach;

        public bool isFlanked;

        public BlueprintFeatureReference m_CheckedFact;

        public BlueprintUnitFact CheckedFact => m_CheckedFact?.Get();

        public ActionList Action;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            if (TriggerBeforeAttack)
            {
                TryRunActions(evt);
            }
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (!TriggerBeforeAttack && !WaitForAttackResolve)
            {
                TryRunActions(evt);
            }
        }
        public void OnEventAboutToTrigger(RuleAttackWithWeaponResolve evt){}
        public void OnEventDidTrigger(RuleAttackWithWeaponResolve evt)
        {
            if (!TriggerBeforeAttack && WaitForAttackResolve)
            {
                TryRunActions(evt.AttackWithWeapon);
            }
        }

        public void TryRunActions(RuleAttackWithWeapon evt)
        {
            if (CheckConditions(evt))
            {
                using (TriggerBeforeAttack ? null : ContextData<ContextAttackData>.Request().Setup(evt.AttackRoll))
                {
                    (base.Fact as IFactContextOwner)?.RunActionInContext(Action, evt.Target);
                }
            }
        }

        public bool CheckConditions(RuleAttackWithWeapon evt)
        {
            if (!TriggerBeforeAttack && OnlyHit && !evt.AttackRoll.IsHit)
            {
                return false;
            }

            if (OnlyMelee && (evt.Weapon == null || !evt.Weapon.Blueprint.IsMelee))
            {
                return false;
            }

            if (OnlyRanged && (evt.Weapon == null || !evt.Weapon.Blueprint.IsRanged))
            {
                return false;
            }

            if (NotReach && (evt.Weapon == null || evt.Weapon.Blueprint.Type.AttackRange > GameConsts.MinWeaponRange))
            {
                return false;
            }

            if (OnAttackOfOpportunity && !evt.IsAttackOfOpportunity)
            {
                return false;
            }

            UnitEntityData attacker = evt.Initiator;
            UnitEntityData target = evt.Target;

            if (target.Descriptor.State.IsDead)
            {
                return false;
            }

            if (isFlanked && !evt.Target.CombatState.IsFlanked && !evt.Initiator.HasFact(CheckedFact))
            {
                return false;
            }

            if (attacker.CombatState.PreventAttacksOfOpporunityNextFrame || target.CombatState.PreventAttacksOfOpporunityNextFrame)
            {
                return false;
            }

            return true;
        }
    }
}