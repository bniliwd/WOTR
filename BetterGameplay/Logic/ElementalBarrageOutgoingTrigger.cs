using BetterGameplay.Util;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterGameplay.Logic
{
    [AllowMultipleComponents]
    [TypeId("98cbc7a6c41240199426a87ac018686c")]
    public class ElementalBarrageOutgoingTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>,
        IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public bool TriggerOnStatDamageOrEnergyDrain;

        public bool CheckAbilityType;

        public AbilityType m_AbilityType;

        public bool NotZeroDamage;

        public bool CheckDamageDealt;

        public CompareOperation.Type CompareType;

        public ContextValue TargetValue;

        public bool ApplyToAreaEffectDamage;

        public bool IgnoreDamageFromThisFact = true;

        public BlueprintBuffReference m_ElementalBarrageBuff;

        public ContextDurationValue MarkDuration;

        public ActionList TriggerActions;

        private BlueprintBuff ElementalBarrageBuff => m_ElementalBarrageBuff?.Get();

        private void RunAction(RuleDealDamage e, UnitEntityData target)
        {
            if (!this.IgnoreDamageFromThisFact || e.Reason.Fact != base.Fact)
            {
                var existedBuff = GetElementalBarrageBuff();
                var elementTypes = new Dictionary<DamageEnergyType, int>
                {
                    { DamageEnergyType.Fire, 2 },
                    { DamageEnergyType.Cold, 3 },
                    { DamageEnergyType.Sonic, 6 },
                    { DamageEnergyType.Electricity, 12 },
                    { DamageEnergyType.Acid, 23 }
                };

                foreach (var type in elementTypes.Keys)
                {
                    var maxDamage = e.ResultList
                        .Select(r => (Damage: r, Energy: r.Source as EnergyDamage))
                        .Where(t => t.Energy?.EnergyType == type)
                        .Select(t => t.Damage)
                        .MaxBy(r => r.FinalValue);

                    if (maxDamage.Equals(default(DamageValue)) || !AboveDamageThreshold(maxDamage.FinalValue))
                    {
                        continue;
                    }
                    ApplyBuff(ref existedBuff, ElementalBarrageBuff, elementTypes[type]);
                }
            }

            void ApplyBuff(ref Buff markedBuff, BlueprintBuff toMarkBuff, int EnergyType)
            {
                string energyTypeStr = EnergyType switch
                {
                    2 => "火焰", 3 => "寒冷", 6 => "音波", 12 => "电", 23 => "酸", _ => string.Empty
                };
                toMarkBuff.m_Description = LocaleStringUtils.CreateLocalizedString($"ElementalBarrage{EnergyType}", $"如果受到{energyTypeStr}以外的元素伤害，造成额外神力伤害并清除标记。");
                TimeSpan? duration = MarkDuration.Calculate(this.Context).Seconds;
                if (markedBuff == null)
                {
                    Buff buff = target.Descriptor.AddBuff(toMarkBuff, Fact.MaybeContext, duration);
                    buff.IsFromSpell = false;
                    buff.IsNotDispelable = true;
                    markedBuff = buff;
                }
                else if (markedBuff.Rank != EnergyType)
                {
                    markedBuff.RunActionInContext(this.TriggerActions, target);
                    if (duration.HasValue) markedBuff.SetDuration(duration.Value);
                }

                markedBuff.Rank = EnergyType;
            }

            Buff GetElementalBarrageBuff()
            {
                Buff[] array = [.. target.Buffs.Enumerable];
                foreach (Buff buff in array)
                {
                    if (buff.Blueprint == ElementalBarrageBuff)
                    {
                        return buff;
                    }
                }
                return null;
            }
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            this.Apply(evt);
        }

        private void Apply(RuleDealDamage evt)
        {
            if (this.CheckAbilityType)
            {
                var ability = evt.Reason.Ability?.Blueprint ?? evt.Reason.Context.SourceAbility;
                AbilityType? abilityType = (ability != null) ? new AbilityType?(ability.Type) : null;
                if (!(abilityType.GetValueOrDefault() == m_AbilityType & abilityType != null))
                {
                    return;
                }
            }
            if (!this.ApplyToAreaEffectDamage && evt.SourceArea)
            {
                return;
            }
            this.RunAction(evt, evt.Target);
        }

        private bool AboveDamageThreshold(int damageValue)
        {
            return !this.CheckDamageDealt || this.CompareType.CheckCondition((float)damageValue, (float)this.TargetValue.Calculate(base.Fact.MaybeContext));
        }
    }
}
