using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using OnePearl.Utils;

namespace OnePearl.Components;

[AllowedOn(typeof(BlueprintItemEnchantment), false)]
[AllowMultipleComponents]
[TypeId("2f1bfd8c6f28419b887e9f6960863b2b")]
public class AddOnePearlFeatureEquipment : ItemEnchantmentComponentDelegate<ItemEntity, AddOnePearlFeatureEquipmentData>
{
    public BlueprintFeatureReference m_Feature;

    public BlueprintFeature Feature => this.m_Feature?.Get();

    public override void OnActivate()
    {
        UnitDescriptor wielder1 = this.Owner.Wielder;
        wielder1?.RemoveFact(this.Data.AppliedFact);
        this.Data.AppliedFact = null;
        if (this.Feature == null) return;
        AddOnePearlFeatureEquipmentData data = this.Data;
        UnitDescriptor wielder2 = this.Owner.Wielder;
        EntityFact entityFact = wielder2?.AddFact(Feature, this.Context);
        data.AppliedFact = entityFact;
        this.Data.AppliedFact?.SetSourceItem(this.Owner);
        if (wielder2 != null && entityFact != null)
        {
            var pearls = PearlUtils.CollectPearls(wielder2.Inventory, false, false);
            PearlUtils.UpdateResources(wielder2, pearls, true);
        }
    }

    public override void OnDeactivate()
    {
        UnitDescriptor wielder = this.Owner.Wielder;
        wielder?.RemoveFact(this.Data.AppliedFact);
        this.Data.AppliedFact = null;
    }

    public override void OnApplyPostLoadFixes() => this.Data.AppliedFact?.SetSourceItem(this.Owner);
}

public class AddOnePearlFeatureEquipmentData
{
    [JsonProperty]
    public EntityFact AppliedFact;
}
