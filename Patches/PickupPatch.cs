using System.Collections.Generic;
using BepInEx5ArchipelagoPluginTemplate.templates;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;

// Patches the method called when the player steps on a pickup
[HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnTriggerStay2D))]
class CollisionPatch
{
    // Maps in-game item ID (e.g., "active_watering") to its vanilla location name as defined in the
    // apworld (e.g, "Veggieville - Steal From Lemon").
    // TODO: Add all currently supported locations
    static Dictionary<string, string> itemIdToLocation = new Dictionary<string, string>() {
        // Veggieville
        {"active_watering", "Veggieville - Steal From Lemon"},
        {"key_flower", "Veggieville - Water Florist's Flower"},
    };

    static bool Prefix(UnityEngine.Collider2D _collider2D, PlayerController __instance)
    {
        if (__instance.CanMove() && _collider2D.gameObject.CompareTag("Item"))
        {
            ItemController component = _collider2D.gameObject.GetComponent<ItemController>();
            if (component != null)
            {
                var itemObject = component.GetItemObject();
                string itemId = itemObject.Index;
                ArchipelagoConsole.LogMessage("Item Collision! " + itemObject.GetName() + " " + itemId);
                if (itemIdToLocation.ContainsKey(itemId))
                {
                    // We still need to destroy the item
                    component.Pickup();
                    // Report the location as collected and skip the default trigger
                    ArchipelagoConsole.LogMessage("Reporting collection of " + itemIdToLocation[itemId]);
                    Plugin.ArchipelagoClient.CollectFrom(itemIdToLocation[itemId]);
                    // TODO: The item that was originally here needs to be flagged as picked up, so it won't respawn

                    return false;
                }
            }
        }

        // Not a location tracked by Archipelago - proceed normally
        return true;
    }
}

// How to get the PlayerController object in the absence of an __instance
// Singleton<PlayerManager>.Instance.GetConnectedPlayer()