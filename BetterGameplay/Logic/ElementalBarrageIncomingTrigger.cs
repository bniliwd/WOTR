using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System.Linq;

namespace BetterGameplay.Logic
{
    [AllowMultipleComponents]
    [TypeId("d8a317a699f041868c65268a97cc6920")]
    public class ElementalBarrageIncomingTrigger : UnitBuffComponentDelegate, ITargetRulebookHandler<RuleDealDamage>,
        IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public bool TriggerOnStatDamageOrEnergyDrain;

        public bool NotZeroDamage;

        public bool CheckDamageDealt;

        public CompareOperation.Type CompareType;

        public ContextValue TargetValue;

        public bool CheckEnergyDamageType;

        public DamageEnergyType EnergyType;

        public DamageEnergyType[] EnergyTypes;

        public bool IgnoreDamageFromThisFact = true;

        public ActionList TriggerActions;

        private void RunAction(RuleDealDamage evt, UnitEntityData target)
        {
            if ((!this.IgnoreDamageFromThisFact || evt.Reason.Fact != base.Fact) && this.TriggerActions.HasActions)
            {
                if (base.Fact is not IFactContextOwner factContextOwner)
                {
                    return;
                }
                factContextOwner.RunActionInContext(this.TriggerActions, base.Owner);
            }
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (!CheckSource(evt))
            {
                return;
            }
            if (!CheckEnergyDamageType && !AboveDamageThreshold(evt.Result))
            {
                return;
            }
            if (!CheckEnergyType(evt))
            {
                return;
            }
            this.RunAction(evt, evt.Target);
        }

        private bool CheckSource(RuleDealDamage evt)
        {
            AbilityData ability = evt.Reason.Ability;
            AbilityType abilityType = ability?.Blueprint?.Type ?? (AbilityType)(-1);
            if (abilityType == AbilityType.Spell && ability.Caster == this.Buff?.Context?.MaybeCaster)
            {
                return false;
            }
            return true;
        }

        private bool CheckEnergyType(RuleDealDamage evt)
        {
            //{2,3,6,12,23} {Fire, Cold, Sonic, Electricity, Acid}
            if (this.CheckEnergyDamageType)
            {
                Buff buff = GetElementalBarrageBuff(evt.Target);
                foreach (DamageValue damageValue in evt.ResultList)
                {
                    if (damageValue.Source.Type == DamageType.Energy)
                    {
                        DamageEnergyType energyType = (damageValue.Source as EnergyDamage).EnergyType;
                        if (EnergyTypes.Contains(energyType) && AboveDamageThreshold(damageValue.FinalValue))
                        {
                            return buff?.Rank != energyType switch
                            {
                                DamageEnergyType.Fire => 2,
                                DamageEnergyType.Cold => 3,
                                DamageEnergyType.Sonic => 6,
                                DamageEnergyType.Electricity => 12,
                                DamageEnergyType.Acid => 23,
                                _ => 0,
                            };
                        }
                    }
                }
            }
            return false;
        }

        private Buff GetElementalBarrageBuff(UnitEntityData target)
        {
            Buff[] array = [.. target.Buffs.Enumerable];
            foreach (Buff buff in array)
            {
                if (buff.Blueprint == base.OwnerBlueprint)
                {
                    return buff;
                }
            }
            return null;
        }

        private bool AboveDamageThreshold(int damageValue)
        {
            return !this.CheckDamageDealt || this.CompareType.CheckCondition((float)damageValue, (float)this.TargetValue.Calculate(base.Fact.MaybeContext));
        }
    }
}
