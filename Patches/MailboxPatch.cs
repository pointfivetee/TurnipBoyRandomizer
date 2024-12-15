using BepInEx5ArchipelagoPluginTemplate.templates;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;

// Patches the method called when the player uses the mailbox
[HarmonyPatch(typeof(MailboxController), nameof(MailboxController.Use))]
class MailboxPatch
{
    // Log requirements for available mail
    static bool Prefix(MailboxController __instance)
    {
        if (Plugin.EnableRandomization)
        {
            var mail = new Traverse(__instance).Method("CheckForMail").GetValue();
            if (mail != null)
            {
                ItemObject itemObject = new Traverse(mail).Field("ItemObject").GetValue<ItemObject>();
                if (itemObject != null && itemObject.CanSpawn())
                {
                    string itemId = itemObject.Index;
                    ArchipelagoConsole.LogMessage("Mailbox opened! " + itemObject.GetName() + " " + itemObject.Index);
                    if (Plugin.ItemIdToLocation.ContainsKey(itemId))
                    {
                        // Report the location as collected
                        ArchipelagoConsole.LogMessage("Reporting collection of " + Plugin.ItemIdToLocation[itemId]);
                        Plugin.ArchipelagoClient.CollectFrom(Plugin.ItemIdToLocation[itemId]);

                        // Set the item's "picked up" flag to true
                        itemObject.Pickup();

                        new Traverse(__instance).Method("RefreshMailbox").GetValue();

                        // Skip the default behavior
                        return false;
                    }

                    // Not a location tracked by Archipelago - proceed normally
                    return true;
                }
            }
        }

        // Proceed normally
        return true;

    }
}