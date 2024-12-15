using System;
using System.Collections;
using System.Collections.Generic;
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
            ArchipelagoConsole.LogMessage("required identifier: " +neededIdentifier);
        }

        // Proceed normally
        return true;
    }
}