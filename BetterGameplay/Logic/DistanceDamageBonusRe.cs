using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;

namespace BetterGameplay.Logic
{
    [TypeId("63cdc1200ef94f80a51b832eaa1ed45d")]
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [ComponentName("Distance damage bonus re")]
    public class DistanceDamageBonusRe : UnitFactComponentDelegate, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>
    {
        public Feet Close;

        public Feet Range;

        public int DamageBonus;

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            UnitPartDisableBonusForDamage unitPartDisableBonusForDamage = base.Context.MaybeCaster?.Parts.Get<UnitPartDisableBonusForDamage>();
            if (unitPartDisableBonusForDamage == null || !unitPartDisableBonusForDamage.DisableAdditionalDamage)
            {
                bool isRangedWeapon = evt.DamageBundle.Weapon != null && evt.DamageBundle.Weapon.Blueprint.IsRanged;
                bool flag = (evt.Reason.Ability?.GetDeliverProjectileFromDeliverBlueprint(TacticalCombatHelper.IsActive))?.NeedAttackRoll ?? false;

                float distance = evt.Target.DistanceTo(evt.Initiator);
                float corpulence = evt.Target.Corpulence + evt.Initiator.Corpulence;
                bool inRange = distance <= Range.Meters + corpulence;
                int num2;
                if ((bool)evt.Reason.Ability?.Blueprint.GetComponent<AbilityKineticist>())
                {
                    ItemEntityWeapon weapon = evt.DamageBundle.Weapon;
                    num2 = ((weapon != null && weapon.Blueprint.Category == WeaponCategory.KineticBlast) ? 1 : 0);
                }
                else
                {
                    num2 = 0;
                }

                bool flag3 = (byte)num2 != 0;
                if ((isRangedWeapon || (flag && !flag3)) && inRange)
                {
                    bool close = distance <= Close.Meters + corpulence;
                    evt.DamageBundle.First?.AddModifierTargetRelated(close ? DamageBonus * 2 : DamageBonus, base.Fact);
                }
            }
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }
    }
}