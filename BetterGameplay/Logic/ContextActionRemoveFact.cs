using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace BetterGameplay.Logic
{
    [TypeId("9ff5049069d8499bb227a6eb327c96e9")]
    public class ContextActionRemoveFact : ContextAction
    {
        [SerializeField]
        public BlueprintUnitFactReference m_Fact;

        public BlueprintUnitFact Fact => m_Fact.Get();

        public override string GetCaption()
        {
            return "Remove fact from target";
        }

        public override void RunAction()
        {
            if (base.Target.Unit == null)
            {
                PFLog.Mods.Error("Can't find target to remove fact!");
                return;
            }

            base.Target.Unit.RemoveFact(Fact);
        }
    }
}