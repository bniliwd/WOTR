using Kingmaker;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using System.Collections.Generic;

namespace BetterGameplay.Logic
{
    [TypeId("4b22554ea5ff4068ac8ef350e766bc8e")]
    public class AbilityCustomDimensionalRetribution : AbilityCustomDimensionDoor
    {
        public override bool LookAtTarget => true;

        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
        {
            UnitEntityData caster = context.Caster;
            if (target.Unit == null)
            {
                PFLog.Default.Error("Target unit is missing");
                yield break;
            }
            bool success = false;
            IEnumerator<AbilityDeliveryTarget> dimmensionDoor = base.Deliver(context, target);
            while (dimmensionDoor.MoveNext())
            {
                AbilityDeliveryTarget abilityDeliveryTarget = dimmensionDoor.Current;
                success = (abilityDeliveryTarget?.Target) == target;
                yield return null;
            }
            if (success)
            {
                UnitAttackOfOpportunity unitAttack = new(target.Unit, false);
                unitAttack.IgnoreCooldown(null);
                caster.Commands.Run(unitAttack);
                context.Caster.CombatState.AttackOfOpportunityCount -= 1;
                EventBus.RaiseEvent(delegate (IAttackOfOpportunityHandler h)
                {
                    h.HandleAttackOfOpportunity(caster, target.Unit);
                });
            }
            yield break;
        }
    }
}