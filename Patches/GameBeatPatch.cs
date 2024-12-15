using BepInEx5ArchipelagoPluginTemplate.templates;
using HarmonyLib;

// Intercept a dialogue upon closing if it rewards an item
[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.GameBeat))]
class GameBeatPatch
{
    // Goal detection
    // For now, the goal is to deliver the laser pointer to Mayor Onion
    static void Postfix()
    {
        if (Plugin.EnableRandomization)
        {
            Plugin.ArchipelagoClient.SetGoalAchieved();
        }
    }
}