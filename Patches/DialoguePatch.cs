using BepInEx5ArchipelagoPluginTemplate.templates;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;
using UnityEngine;

// Intercept a dialogue upon closing if it rewards an item
[HarmonyPatch(typeof(DialoguePopupController), nameof(DialoguePopupController.Nuke))]
class DialoguePatch
{
    static bool Prefix(DialoguePopupController __instance)
    {
        if (Plugin.EnableRandomization)
        {
            var dialogue = new Traverse(__instance).Field("dialogue").GetValue<Dialogue>();
            if (dialogue.RewardItemObject != null && dialogue.RewardItemObject.CanSpawn())
            {
                var itemObject = dialogue.RewardItemObject;
                string itemId = itemObject.Index;
                if (Plugin.ItemIdToLocation.ContainsKey(itemId))
                {
                    // Perform the normal cleanup
                    Singleton<GameManager>.Instance.LosePopup();
                    Object.Destroy(__instance.gameObject);
                    // Report the location as collected and skip the default pickup
                    ArchipelagoConsole.LogMessage("Reporting collection of " + Plugin.ItemIdToLocation[itemId]);
                    Plugin.ArchipelagoClient.CollectFrom(Plugin.ItemIdToLocation[itemId]);
                    return false;
                }
            }
        }

        // Proceed normally
        return true;
    }

    // Goal detection
    // For now, the goal is to deliver the laser pointer to Mayor Onion
    static void Postfix(DialoguePopupController __instance)
    {
        if (Plugin.EnableRandomization)
        {
            if (Singleton<ReadWriteSaveManager>.Instance.GetData("quest_main_step_3_completed", false))
            {
                Plugin.ArchipelagoClient.SetGoalAchieved();
            }
        }
    }
}