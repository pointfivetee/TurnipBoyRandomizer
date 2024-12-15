using BepInEx5ArchipelagoPluginTemplate.templates;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;

// Fix logic for item spawnability
[HarmonyPatch(typeof(ItemObject), nameof(ItemObject.CanSpawn))]
class CanSpawnPatch
{
    static void Postfix(ItemObject __instance, ref bool __result)
    {
        if (Plugin.ItemIdToLocation.ContainsKey(__instance.Index))
        {
            // We have to rely solely on the pickup flag because the item normally in this location
            // might already be in the inventory.
            __result = !__instance.OnlyOne || !Singleton<ReadWriteSaveManager>.Instance.GetData("item_" + __instance.Index + "_picked_up", false, false);
        }
    }
}