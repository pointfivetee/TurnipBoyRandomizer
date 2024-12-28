using System.Collections;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;

// Patcher for NPC quests
[HarmonyPatch(typeof(QuestNPCInteraction), nameof(QuestNPCInteraction.Interact))]
class QuestInteractPatch
{
    // Log the quest step's requirements
    static bool Prefix(QuestNPCInteraction __instance)
    {
        if (__instance.name == "Tots NPC Variant")
        {
            // Make sure Tots doesn't take away the Soil Sword, potentially leaving the player
            // without a weapon.
            var steps = new Traverse(__instance).Field("steps").GetValue<IList>();
            var step = new Traverse(steps[0]);
            step.Field("TakeItems").SetValue(false);
        }

        // Proceed normally
        return true;
    }
}