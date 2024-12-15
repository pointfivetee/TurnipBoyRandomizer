using System.Collections.Generic;
using BepInEx5ArchipelagoPluginTemplate.templates;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;

// Patches the method called when the player steps on a pickup
[HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnTriggerStay2D))]
class CollisionPatch
{
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
                    if (Plugin.ItemIdToLocation.ContainsKey(itemId))
                    {
                        // Destroy the item
                        component.Pickup();
                        // Set the item's "picked up" flag to true (if unique)
                        itemObject.Pickup();
                        // Report the location as collected and skip the default trigger
                        ArchipelagoConsole.LogMessage("Reporting collection of " + Plugin.ItemIdToLocation[itemId]);
                        Plugin.ArchipelagoClient.CollectFrom(Plugin.ItemIdToLocation[itemId]);

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