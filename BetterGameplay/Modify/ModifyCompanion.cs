using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using System.Linq;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.Modify
{
    internal class ModifyCompanion
    {
        public static void MultiModify()
        {
            ModifyCompanionFeatureList();
        }

        static void ModifyCompanionFeatureList()
        {
            var ArueshalaeFeatureList = GetBlueprint<BlueprintFeature>("7993c81bd04ffda4bac123eb7f6752c4");
            var NenioFeatureList = GetBlueprint<BlueprintFeature>("751afafb3b7017544ac6373901747f60");

            BlueprintGuid targetClass = BlueprintGuid.Parse("cda0615668a6df14eb36ba19ee881af6");
            var RangerClass = ArueshalaeFeatureList.GetComponent<AddClassLevels>(cl => targetClass == cl?.m_CharacterClass.Guid);
            if (RangerClass != null)
            {
                BlueprintGuid target1 = BlueprintGuid.Parse("16cc2c937ea8d714193017780e7d4fc6");
                BlueprintGuid target2 = BlueprintGuid.Parse("c1be13839472aad46b152cf10cf46179");
                bool flag1 = false, flag2 = false;

                foreach (SelectionEntry selection in RangerClass.Selections)
                {
                    if (flag1 && flag2) { break; }
                    if (target1 == selection?.m_Selection.Guid)
                    {
                        flag1 = true;
                        selection.m_Features = [
                            GetBlueprintReference<BlueprintFeatureReference>("7283344b0309d8e4cb77eb22f1e7c57a"),
                            GetBlueprintReference<BlueprintFeatureReference>("f643b38acc23e8e42a3ed577daeb6949"),
                            GetBlueprintReference<BlueprintFeatureReference>("5941963eae3e9864d91044ba771f2cc2"),
                            GetBlueprintReference<BlueprintFeatureReference>("f807fac786faa86438428c79f5629654"),
                            GetBlueprintReference<BlueprintFeatureReference>("6ea5a4a19ccb81a498e18a229cc5038a"),
                        ];
                    }
                    else if (target2 == selection?.m_Selection.Guid)
                    {
                        flag2 = true;
                        BlueprintFeatureReference only = GetBlueprintReference<BlueprintFeatureReference>("f643b38acc23e8e42a3ed577daeb6949");
                        selection.m_Features = [only, only, only, only];
                    }
                }
            }

            targetClass = BlueprintGuid.Parse("ba34257984f4c41408ce1dc2004e342e");
            var WizardClass = NenioFeatureList.GetComponent<AddClassLevels>(cl => targetClass == cl?.m_CharacterClass.Guid);
            if (WizardClass != null)
            {
                BlueprintGuid target = BlueprintGuid.Parse("6c29030e9fea36949877c43a6f94ff31");
                BlueprintGuid targetFeature = BlueprintGuid.Parse("7f8c1b838ff2d2e4f971b42ccdfa0bfd");

                foreach (SelectionEntry selection in WizardClass.Selections)
                {
                    if (target == selection?.m_Selection.Guid && selection.m_Features.Any(r => targetFeature == r.Guid))
                    {
                        selection.m_Features = [GetBlueprintReference<BlueprintFeatureReference>("09595544116fe5349953f939aeba7611")];
                        break;
                    }
                }
            }
        }
    }
}
