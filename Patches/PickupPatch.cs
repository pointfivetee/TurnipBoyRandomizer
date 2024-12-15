using System.Collections.Generic;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;

[HarmonyPatch(typeof(PlayerController), nameof(PlayerController.PickupItem))]
class PickupPatch
{
    // Maps in-game item ID (e.g., "active_watering") to its vanilla location name as defined in the
    // apworld (e.g, "Veggieville - Steal From Lemon").
    // TODO: Add all currently supported locations
    static Dictionary<string, string> itemIdToLocation = new Dictionary<string, string>() {
        // Veggieville
        {"active_watering", "Veggieville - Steal From Lemon"},
        {"key_flower", "Veggieville - Water Florist's Flower"},
    };

    static bool Prefix(ItemObject _itemObject, bool _effect, PlayerController __instance)
    {
        string itemId = _itemObject.Index;
        ArchipelagoConsole.LogMessage("Item Pickup! " + _itemObject.GetName() + " " + itemId);
        if (itemIdToLocation.ContainsKey(itemId))
        {
            // Report the location as collected and skip the default pickup code
            ArchipelagoConsole.LogMessage("Reporting collection of " + itemIdToLocation[itemId]);
            // TODO: Notify the multiclient
            return false;
        }
        // Not a location tracked by Archipelago - proceed normally
        ArchipelagoConsole.LogMessage("not an Archipelago location");
        return true;
    }
}

// How to get the PlayerController object in the absence of an __instance
// Singleton<PlayerManager>.Instance.GetConnectedPlayer()