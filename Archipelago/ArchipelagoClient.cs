using System;
using System.Linq;
using System.Threading;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;

namespace BepInEx5ArchipelagoPluginTemplate.templates.Archipelago;

public class ArchipelagoClient
{
    public const string APVersion = "0.5.0";
    private const string Game = "TurnipBoy";

    public static bool Authenticated;
    private bool attemptingConnection;

    public static ArchipelagoData ServerData = new();
    private DeathLinkHandler DeathLinkHandler;
    private ArchipelagoSession session;

    /// <summary>
    /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
    /// </summary>
    /// <returns></returns>
    public void Connect()
    {
        if (Authenticated || attemptingConnection) return;

        try
        {
            session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri);
            SetupSession();
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
        }

        TryConnect();
    }

    public void CollectFrom(string locationName)
    {
        long locationId = session.Locations.GetLocationIdFromName(session.ConnectionInfo.Game, locationName);
        ArchipelagoConsole.LogMessage("Collecting from location " + locationId);
        session.Locations.CompleteLocationChecks(locationId);

        // Special case: defeating the farmhouse boss despawns that dungeon's enemies, including the
        // one that drops the lost cherry baby. To prevent a softlock, we report the cherrynapper as
        // collected along with the boss arena drop.
        // TODO: Trigger this collection when restarting the generator, not when getting the boss
        // drop
        if (locationName == "Forsaken Farmhouse - Boss Arena")
        {
            this.CollectFrom("Forsaken Farmhouse - Cherrynapper");
        }
    }

    public void SetGoalAchieved()
    {
        // gg!
        session.SetGoalAchieved();
    }

    /// <summary>
    /// add handlers for Archipelago events
    /// </summary>
    private void SetupSession()
    {
        session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString());
        session.Items.ItemReceived += OnItemReceived;
        session.Socket.ErrorReceived += OnSessionErrorReceived;
        session.Socket.SocketClosed += OnSessionSocketClosed;
    }

    /// <summary>
    /// attempt to connect to the server with our connection info
    /// </summary>
    private void TryConnect()
    {
        try
        {
            // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
            ThreadPool.QueueUserWorkItem(
                _ => HandleConnectResult(
                    session.TryConnectAndLogin(
                        Game,
                        ServerData.SlotName,
                        ItemsHandlingFlags.AllItems,
                        new Version(APVersion),
                        password: ServerData.Password,
                        requestSlotData: false // ServerData.NeedSlotData
                    )));
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
            HandleConnectResult(new LoginFailure(e.ToString()));
            attemptingConnection = false;
        }
    }

    /// <summary>
    /// handle the connection result and do things
    /// </summary>
    /// <param name="result"></param>
    private void HandleConnectResult(LoginResult result)
    {
        string outText;
        if (result.Successful)
        {
            var success = (LoginSuccessful)result;

            ServerData.SetupSession(success.SlotData, session.RoomState.Seed);
            Authenticated = true;

            DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName);
            session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
            outText = $"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!";

            ArchipelagoConsole.LogMessage(outText);
        }
        else
        {
            var failure = (LoginFailure)result;
            outText = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.";
            outText = failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

            Plugin.BepinLogger.LogError(outText);

            Authenticated = false;
            Disconnect();
        }

        ArchipelagoConsole.LogMessage(outText);
        attemptingConnection = false;
    }

    /// <summary>
    /// something we wrong or we need to properly disconnect from the server. cleanup and re null our session
    /// </summary>
    private void Disconnect()
    {
        Plugin.BepinLogger.LogDebug("disconnecting from server...");
        session?.Socket.DisconnectAsync();
        session = null;
        Authenticated = false;
    }

    public void SendMessage(string message)
    {
        session.Socket.SendPacketAsync(new SayPacket { Text = message });
    }

    // Get the total number of items matching itemName that have been collected according to the
    // current AP world state.
    private int GetItemCount(string itemName)
    {
        var count = 0;
        foreach(ItemInfo item in session.Items.AllItemsReceived)
        {
            if (item.ItemName == itemName)
            {
                count += 1;
            }
        }
        return count;
    }

    /// <summary>
    /// we received an item so reward it here
    /// </summary>
    /// <param name="helper">item helper which we can grab our item from</param>
    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        var receivedItem = helper.DequeueItem();
        ArchipelagoConsole.LogMessage("Received item " + receivedItem.ItemName);

        if (helper.Index < ServerData.Index) return;

        ServerData.Index++;

        var saveManager = Singleton<ReadWriteSaveManager>.Instance;
        var playerManager = Singleton<PlayerManager>.Instance;
        var playerController = playerManager.GetConnectedPlayer();
        var itemObjectList = playerManager.GetItems();
        var itemName = receivedItem.ItemName;
        var apFlagName = "ap_collected_" + receivedItem.ItemName;
        // The number of items of this type that we marked as collected locally
        var localCollectedCount = saveManager.GetData(apFlagName, 0);
        // The number of items of this type that have been collected according to the AP server
        var serverCollectedCount = GetItemCount(itemName);

        if (localCollectedCount >= serverCollectedCount)
        {
            // We've already awarded the player enough items of this type
            return;
        }

        if (itemName == "Progressive Weapon")
        {
            // Determine the specific weapon item to receive
            if (localCollectedCount == 0)
            {
                itemName = "Soil Sword";
            }
            else if (localCollectedCount == 1)
            {
                itemName = "Fork";
            }
            else if (localCollectedCount == 2)
            {
                itemName = "Shovel";
                // Take away the Soil Sword
                var soilSword = Array.Find(itemObjectList, item => item.GetName() == "Soil Sword");
                playerManager.InventoryRemoveItem(soilSword);
            }
            else
            {
                return;
            }
        }

        var itemObject = Array.Find(itemObjectList, item => item.GetName() == itemName);
        var oldPickupFlag = saveManager.GetData("item_" + itemObject.Index + "_picked_up", false);
        try
        {
            // Run the usual PickupItem() method
            playerController.PickupItem(itemObject);

            // Increment our own counter
            saveManager.SetData(apFlagName, localCollectedCount + 1, false);
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
            // Sometimes PickupItem() throws an error partway through. We need to make sure the
            // pickup flag is reset to its original value no matter what.
            if (itemObject.OnlyOne)
            {
                saveManager.SetData("item_" + itemObject.Index + "_picked_up", oldPickupFlag, false);
            }
        }
    }

    /// <summary>
    /// something went wrong with our socket connection
    /// </summary>
    /// <param name="e">thrown exception from our socket</param>
    /// <param name="message">message received from the server</param>
    private void OnSessionErrorReceived(Exception e, string message)
    {
        Plugin.BepinLogger.LogError(e);
        ArchipelagoConsole.LogMessage(message);
    }

    /// <summary>
    /// something went wrong closing our connection. disconnect and clean up
    /// </summary>
    /// <param name="reason"></param>
    private void OnSessionSocketClosed(string reason)
    {
        Plugin.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
        Disconnect();
    }
}