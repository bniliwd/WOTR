using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using System.Collections.Generic;
using static BetterGameplay.Logic.UnitPartCustomMechanicsFeatures;

namespace BetterGameplay.Logic
{
    [ClassInfoBox("抄自ttt")]
    public class UnitPartCustomMechanicsFeatures : OldStyleUnitPart {

        private readonly Dictionary<CustomMechanicsFeature, CountableFlag> MechanicsFeatures = [];

        //If you want to extend this externally please use something > 1000
        public enum CustomMechanicsFeature : int
        {
            BypassSneakAttackImmunity,
            BypassCriticalHitImmunity
        }

        public CountableFlag GetMechanicsFeature(CustomMechanicsFeature type) {
            MechanicsFeatures.TryGetValue(type, out CountableFlag MechanicsFeature);
            if (MechanicsFeature == null) {
                MechanicsFeature = new CountableFlag();
                MechanicsFeatures[type] = MechanicsFeature;
            }
            return MechanicsFeature;
        }
    }

    public static class CustomMechanicsFeaturesExtentions {
        public static CountableFlag CustomMechanicsFeature(this UnitDescriptor unit, CustomMechanicsFeature type) {
            var mechanicsFeatures = unit.Ensure<UnitPartCustomMechanicsFeatures>();
            return mechanicsFeatures.GetMechanicsFeature(type);
        }

        public static CountableFlag CustomMechanicsFeature(this UnitEntityData unit, CustomMechanicsFeature type) {
            return unit.Descriptor.CustomMechanicsFeature(type);
        }
    }
}