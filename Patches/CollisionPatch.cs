using System.Collections.Generic;
using BepInEx5ArchipelagoPluginTemplate.templates;
using BepInEx5ArchipelagoPluginTemplate.templates.Archipelago;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;

// Patches the method called when the player steps on a pickup
[HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnTriggerStay2D))]
class CollisionPatch
{
    // Maps in-game item ID (e.g., "active_watering") to its vanilla location name as defined in the
    // apworld (e.g, "Veggieville - Steal From Lemon").
    static Dictionary<string, string> itemIdToLocation = new Dictionary<string, string>()
    {
        // Veggieville
        {"active_watering", "Veggieville - Steal From Lemon"},
        {"key_flower", "Veggieville - Water Florist's Flower"},
        // Weapon Woods
        {"active_sword", "Weapon Woods - Soil Sword Patch"},
        {"key_money", "Weapon Woods - Murder Jerry"},
        // Layer Lane
        {"key_trophy", "Layer Lane - Trophy Corner"},
        {"key_sub", "Layer Lane - Sandwich Stall"},
        // Bustling Barn
        {"hat_crown", "Bustling Barn - slayQueen32 Reward"},
        {"key_boots", "Bustling Barn - Boom Boots Room"},
        {"key_fertilizer", "Bustling Barn - Past The King Pig"},
    };

    static bool Prefix(UnityEngine.Collider2D _collider2D, PlayerController __instance)
    {
        if (Plugin.EnableRandomization)
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
                        // Destroy the item
                        component.Pickup();
                        // Set the item's "picked up" flag to true (if unique)
                        itemObject.Pickup();
                        // Report the location as collected and skip the default trigger
                        ArchipelagoConsole.LogMessage("Reporting collection of " + itemIdToLocation[itemId]);
                        Plugin.ArchipelagoClient.CollectFrom(itemIdToLocation[itemId]);

                        return false;
                    }
                }
            }

            // Not a location tracked by Archipelago - proceed normally
            return true;
        }

        // Proceed normally
        return true;
    }
}