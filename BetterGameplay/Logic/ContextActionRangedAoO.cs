using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker;
using TurnBased.Controllers;

namespace BetterGameplay.Logic
{
    [TypeId("3a0186a4e4fc4b339c4cfecee1123b00")]
    public class ContextActionRangedAoO : ContextAction
    {
        [ShowIf("ForceStartAnimation")]
        public UnitAnimationType AnimationType = UnitAnimationType.SpecialAttack;

        public override string GetCaption()
        {
            return "Force caster range attack, cost aoo";
        }

        public override void RunAction()
        {
            UnitEntityData attacker = base.Context.MaybeCaster;
            if (attacker == null)
            {
                return;
            }

            WeaponSlot threatHandRanged = attacker.GetThreatHandRanged();
            if (threatHandRanged == null)
            {
                return;
            }

            if ((!attacker.CombatState.CanActInCombat && !attacker.Descriptor.State.HasCondition(UnitCondition.AttackOfOpportunityBeforeInitiative)) 
                || !attacker.CombatState.CanAttackOfOpportunity 
                || !attacker.Descriptor.State.CanAct)
            {
                return;
            }

            UnitEntityData target = base.Target?.Unit;
            if (target == null || !target.Memory.Contains(attacker))
            {
                return;
            }

            RunAttackRule(attacker, target, threatHandRanged);
            attacker.CombatState.AttackOfOpportunityCount--;

            EventBus.RaiseEvent(delegate (IAttackOfOpportunityHandler h)
            {
                h.HandleAttackOfOpportunity(attacker, target);
            });

            AttackHandInfo attackHandInfo = new(threatHandRanged, 0);
            attackHandInfo.CreateAnimationHandleForAttack(null, attacker.GetSaddledUnit() != null);
            if (attackHandInfo.AnimationHandle != null)
            {
                attacker.LookAt(target.Position);
                attackHandInfo.AnimationHandle.SpeedScale = (CombatController.IsInTurnBasedCombat() ? 1f : 3f);
                attacker.View.AnimationManager.ExecuteIfIdle(attackHandInfo.AnimationHandle);
            }
        }

        public void RunAttackRule(UnitEntityData caster, UnitEntityData target, WeaponSlot hand, int attackBonusPenalty = 0)
        {
            RuleAttackWithWeapon ruleAttackWithWeapon = new(caster, target, hand.Weapon, 0)
            {
                IsAttackOfOpportunity = true,
                ForceFlatFooted = true
            };
            
            base.Context.TriggerRule(ruleAttackWithWeapon);
        }

    }
}