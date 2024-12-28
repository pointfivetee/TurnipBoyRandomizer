using System.Collections;
using BepInEx5ArchipelagoPluginTemplate.templates;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;

// Patcher for NPC quests
[HarmonyPatch(typeof(FlammableController), nameof(FlammableController.IsImmune))]
class FlammablePatch
{
    // Tie fire immunity to having the Hazmat Suit *item*, not having checked its *location*
    static void Postfix(FlammableController __instance, ref bool __result)
    {
        var immuneItemObject =  new Traverse(__instance).Field("immuneItemObject").GetValue<ItemObject>();
        if (immuneItemObject != null)
        {
            __result = Singleton<PlayerManager>.Instance.InventoryContainsItem(immuneItemObject);
        }
    }
}