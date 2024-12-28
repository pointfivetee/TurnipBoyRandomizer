using System.Collections;
using BepInEx5ArchipelagoPluginTemplate.templates;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;

// Patcher for NPC quests
[HarmonyPatch(typeof(QuestNPCInteraction), nameof(QuestNPCInteraction.CheckIfStepIsComplete))]
class QuestStepCompletePatch
{
    // Log the quest step's requirements
    static bool Prefix(int _index, QuestNPCInteraction __instance)
    {
        ArchipelagoConsole.LogMessage("Checking quest " + __instance.name + " step " + _index);
        var steps = new Traverse(__instance).Field("steps").GetValue<IList>();
        var currentStep = new Traverse(steps[_index]);

        var itemsRequired = currentStep.Field("ItemsRequired").GetValue<ItemObject[]>();
        if (itemsRequired.Length != 0)
        {
            foreach (ItemObject itemObject in itemsRequired)
            {
                ArchipelagoConsole.LogMessage("required item: " + itemObject.GetName());
            }
        }

        var neededIdentifier = currentStep.Field("NeededIdentifier").GetValue<string>();
        if (neededIdentifier != "")
        {
            ArchipelagoConsole.LogMessage("required identifier: " + neededIdentifier);
        }

        // Proceed normally
        return true;
    }

    // Make necessary adjustments if the player sequence broke a quest
    static void Postfix(int _index, QuestNPCInteraction __instance, ref bool __result)
    {
        if (Plugin.EnableRandomization)
        {
            // Florist quest
            // If we've already given Strawberry the flower, it's okay if the plant hasn't been watered
            if (__instance.name == "Blueberry NPC Variant" && _index == 0 && !__result)
            {
                __result = Singleton<ReadWriteSaveManager>.Instance.GetData("quest_flower_completed", false);
            }

            // Belch quest
            // If we've already given slayQueen32 the tier 3 sub, we can move Belch's quest along
            if (__instance.name == "Belch NPC Variant" && _index == 0 && !__result)
            {
                __result = Singleton<ReadWriteSaveManager>.Instance.GetData("quest_simp_step_0_completed", false);
            }

            // Shady Blueberry quest
            // If we've already given Bald Beet the wood, we can move along
            if (__instance.name == "Shady Blueberry NPC Variant" && _index == 0 && !__result)
            {
                __result = Singleton<ReadWriteSaveManager>.Instance.GetData("quest_wood_completed", false);
            }

            // Shady Carrot quest
            // If we've already given the Pickled Gang the hammer, we can move along
            if (__instance.name == "Shady Carrot NPC Variant" && _index == 0 && !__result)
            {
                __result = Singleton<ReadWriteSaveManager>.Instance.GetData("quest_jail_completed", false);
            }
        }
    }
}