using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic;

namespace BetterGameplay.Logic
{
    [TypeId("869a76aae5674ded917515f6ab3e6279")]
    public class AddCustomMechanicsFeature : UnitFactComponentDelegate
    {

        public UnitPartCustomMechanicsFeatures.CustomMechanicsFeature Feature;

        public override void OnTurnOn()
        {
            Owner.CustomMechanicsFeature(Feature).Retain();
        }

        public override void OnTurnOff()
        {
            Owner.CustomMechanicsFeature(Feature).Release();
        }
    }
}
