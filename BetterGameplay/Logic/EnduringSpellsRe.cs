using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Controllers;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;

namespace BetterGameplay.Logic
{

    /**
     * 订阅式接口，使用prefix无法有效拦截
     */
    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("5184430fac75447aa855f9a11b09b63c")]
    public class EnduringSpellsRe : UnitFactComponentDelegate, IUnitBuffHandler, IGlobalSubscriber, ISubscriber
    {
        internal BlueprintUnitFactReference m_Greater;

        public BlueprintUnitFact Greater => m_Greater?.Get();

        public void HandleBuffDidAdded(Buff buff)
        {
            if (!buff.Blueprint.EmulateAbilityContext)
            {
                AbilityData abilityData = buff.Context.SourceAbilityContext?.Ability;
                if (abilityData == null || abilityData.Spellbook == null || abilityData.SourceItem != null)
                {
                    return;
                }
            }

            //处理逻辑会分发
            if (!(buff.MaybeContext?.MaybeCaster == Owner))
            {
                return;
            }
            bool enduringSpells = buff.TimeLeft >= 10.Minutes();
            bool enduringSpellsGreater = buff.TimeLeft >= 5.Minutes() && Owner.HasFact(Greater);

            if (enduringSpellsGreater && buff.TimeLeft <= 24.Hours())
            {
                buff.SetEndTime(24.Hours() + buff.AttachTime);
                return;
            }

            if (enduringSpells && buff.TimeLeft <= 1.Hours())
            {
                buff.SetEndTime(1.Hours() + buff.AttachTime);
            }
        }

        public void HandleBuffDidRemoved(Buff buff) {}
    }
}