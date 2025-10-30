using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic.ActivatableAbilities;

namespace BetterGameplay.Logic
{

    [TypeId("6fe880e5ff114227931409aa33e9effe")]
    public class BlueprintActivatableAbilityRe : BlueprintActivatableAbility
    {
        public BlueprintItemEnchantmentReference m_Enchant;

        public BlueprintItemEnchantment Enchant => m_Enchant?.Get();
    }
}