using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.NewContent
{
    public static class RichEnemy
    {
        
        public static List<UnitBody> ParseCombines(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<UnitBody>>(jsonContent);
            } catch (Exception) { PFLog.Mods.Error("读取本地文本失败"); }

            return [];
        }

        public static void ReplaceEnemyEquipments()
        {
            string path = Path.Combine(Main.ModPath, "Configuration", "EnemyEquipments.json");
            var data = ParseCombines(path);

            foreach(UnitBody bodyInfo in data)
            {
                BlueprintUnit unit = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>(bodyInfo.UnitId);
                if (unit.Body == null) continue;

                ModifyCommonEquipment(unit.Body, bodyInfo.Commons);
                ModifyMultiSlots(unit.Body, bodyInfo.Rings, bodyInfo.QuickSlots);
            }            
        }

        public static void ModifyCommonEquipment(BlueprintUnit.UnitBody body, List<string> commons)
        {
            foreach (string id in commons)
            {
                try
                {
                    BlueprintItemEquipment unknown = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipment>(id);
                    if (unknown == null)
                    {
                        continue;
                    }
                    switch (unknown)
                    {
                        case BlueprintItemEquipmentHand m_PrimaryHand://仅主手
                            body.m_PrimaryHand = m_PrimaryHand.ToReference<BlueprintItemEquipmentHandReference>();
                            break;
                        case BlueprintItemArmor m_Armor:
                            body.m_Armor = m_Armor.ToReference<BlueprintItemArmorReference>();
                            break;
                        case BlueprintItemEquipmentShirt m_Shirt:
                            body.m_Shirt = m_Shirt.ToReference<BlueprintItemEquipmentShirtReference>();
                            break;
                        case BlueprintItemEquipmentBelt m_Belt:
                            body.m_Belt = m_Belt.ToReference<BlueprintItemEquipmentBeltReference>();
                            break;
                        case BlueprintItemEquipmentHead m_Head:
                            body.m_Head = m_Head.ToReference<BlueprintItemEquipmentHeadReference>();
                            break;
                        case BlueprintItemEquipmentGlasses m_Glasses:
                            body.m_Glasses = m_Glasses.ToReference<BlueprintItemEquipmentGlassesReference>();
                            break;
                        case BlueprintItemEquipmentFeet m_Feet:
                            body.m_Feet = m_Feet.ToReference<BlueprintItemEquipmentFeetReference>();
                            break;
                        case BlueprintItemEquipmentGloves m_Gloves:
                            body.m_Gloves = m_Gloves.ToReference<BlueprintItemEquipmentGlovesReference>();
                            break;
                        case BlueprintItemEquipmentNeck m_Neck:
                            body.m_Neck = m_Neck.ToReference<BlueprintItemEquipmentNeckReference>();
                            break;
                        case BlueprintItemEquipmentWrist m_Wrist:
                            body.m_Wrist = m_Wrist.ToReference<BlueprintItemEquipmentWristReference>();
                            break;
                        case BlueprintItemEquipmentShoulders m_Shoulders:
                            body.m_Shoulders = m_Shoulders.ToReference<BlueprintItemEquipmentShouldersReference>();
                            break;
                        default:
                            PFLog.Mods.Error("未知装备: " + unknown?.AssetGuid);
                            break;
                    }
                } catch (Exception) {
                    PFLog.Mods.Error("未知装备: " + id);
                }
                
            }
        }

        public static void ModifyMultiSlots(BlueprintUnit.UnitBody body, List<string> rings, List<string> quickSlots)
        {
            foreach (var ring in rings.Select((itemId, index) => (itemId, index)))
            {
                if (!string.IsNullOrEmpty(ring.itemId))
                {
                    BlueprintItemEquipmentRingReference reference = GetBlueprintReference<BlueprintItemEquipmentRingReference>(ring.itemId);
                    _ = ring.index == 0 ? body.m_Ring1 = reference : body.m_Ring2 = reference;
                }
            }

            for (int i = 0; i < quickSlots.Count; i++)
            {
                if (!string.IsNullOrEmpty(quickSlots[i]))
                    body.m_QuickSlots[i] = GetBlueprintReference<BlueprintItemEquipmentUsableReference>(quickSlots[i]);
            }
        }

    }

    public class UnitBody
    {
        //[JsonProperty("name")]
        //public string Name { get; set; }

        [JsonProperty("unitId")]
        public string UnitId { get; set; }

        [JsonProperty("commons")]
        public List<string> Commons { get; set; }

        [JsonProperty("rings")]
        public List<string> Rings { get; set; }

        [JsonProperty("quickSlots")]
        public List<string> QuickSlots { get; set; }

        //[JsonProperty("desc")]
        //public string Desc { get; set; }
    }
}