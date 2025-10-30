using BetterGameplay.Rule;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;

namespace BetterGameplay.Logic
{
    [ComponentName("Reroll Concealment Checks")]
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("5c2f071c0bba4164bb2b472ee7889205")]
    public class RerollFortification : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleFortificationCheck>,
        IRulebookHandler<RuleFortificationCheck>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public int RerollCount = 1;
        public bool TakeBest = true;
        public bool ConfirmedCriticals = true;
        public bool SneakAttacks = true;
        public bool PreciseStrike = true;
        public bool CheckReason;

        public void OnEventAboutToTrigger(RuleFortificationCheck evt)
        {
            if (evt.Weapon != null && evt.Weapon.Blueprint.Category == base.Param)
            {
                if (!CheckReason)
                {
                    evt.Roll.AddReroll(RerollCount, TakeBest, base.Fact);
                    return;
                }
                if (ConfirmedCriticals && evt.ForCritical)
                {
                    evt.Roll.AddReroll(RerollCount, TakeBest, base.Fact);
                    return;
                }
                if (SneakAttacks && evt.ForSneakAttack)
                {
                    evt.Roll.AddReroll(RerollCount, TakeBest, base.Fact);
                    return;
                }
                if (PreciseStrike && evt.ForPreciseStrike)
                {
                    evt.Roll.AddReroll(RerollCount, TakeBest, base.Fact);
                    return;
                }
            }
        }

        public void OnEventDidTrigger(RuleFortificationCheck evt) {}
    }
}