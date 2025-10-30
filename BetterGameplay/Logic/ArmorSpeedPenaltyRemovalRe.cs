using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

namespace BetterGameplay.Logic
{
    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintFeature), false)]
    [TypeId("f70f4d71099a49c58d741948b019f12b")]
    public class ArmorSpeedPenaltyRemovalRe : UnitFactComponentDelegate
    {
        public override void OnTurnOn()
        {
            base.OnTurnOn();
            base.Owner.State.Features.ImmuneToArmorSpeedPenalty.Retain();

            if (RestrictionsHelper.CheckHasArmor(base.Owner))
            {
                base.Owner.Body.Armor.Armor.RecalculateStats();
            }
        }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            base.Owner.State.Features.ImmuneToArmorSpeedPenalty.Release();

            if (RestrictionsHelper.CheckHasArmor(base.Owner))
            {
                base.Owner.Body.Armor.Armor.RecalculateStats();
            }
        }
    }
}