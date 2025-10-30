using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System.Collections.Generic;

namespace BetterGameplay.Logic
{
    [TypeId("513b62c66c934137b1e455007f43dbb7")]
    public class ContextActionRemoveBuffs : ContextAction
    {
        public bool RemoveRank;

        [InfoBox("Apply this action to caster = Remove buff From caster of context")]
        public bool ToCaster;

        [InfoBox("Remove buff from target that was attached by caster")]
        public bool OnlyFromCaster;

        public HashSet<BlueprintGuid> Buffs;

        public override string GetCaption()
        {
            return "Remove Buffs: " + Buffs.Count;
        }

        public override void RunAction()
        {
            if (ContextData<MechanicsContext.Data>.Current?.Context == null)
            {
                PFLog.Mods.Error(this, "Unable to remove buff: no context found");
                return;
            }

            UnitEntityData maybeCaster = base.Context.MaybeCaster;
            UnitEntityData unitEntityData = (ToCaster ? maybeCaster : base.Target.Unit);
            if (unitEntityData == null)
            {
                PFLog.Mods.Error(this, "Unable to remove buff: no target found");
                return;
            }

            Buff[] array = [.. unitEntityData.Buffs.Enumerable];
            using (ContextData<BuffCollection.RemoveByRank>.RequestIf(RemoveRank))
            {
                Buff[] array2 = array;
                foreach (Buff buff in array2)
                {
                    if (Buffs.Contains(buff.Blueprint.AssetGuid) && (!OnlyFromCaster || buff.Context.MaybeCaster == maybeCaster))
                    {
                        buff.Remove();
                        break;
                    }
                }
            }
        }
    }
}