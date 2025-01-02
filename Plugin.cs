using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx5ArchipelagoPluginTemplate.templates.Archipelago;
using BepInEx5ArchipelagoPluginTemplate.templates.Utils;
using HarmonyLib;
using UnityEngine;

namespace BepInEx5ArchipelagoPluginTemplate.templates;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGUID = "com.halftime.tbcte_ap";
    public const string PluginName = "Turnip Boy Commits Tax Evasion AP";
    public const string PluginVersion = "0.1.2";

    public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
    private const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
    public static ManualLogSource BepinLogger;
    public static ArchipelagoClient ArchipelagoClient;

    // Set to false to playtest a "vanilla" game without any item randomization or communication
    // with the AP server. Patches that only produce logs will still run.
    public static bool EnableRandomization = true;


    // Maps in-game item ID (e.g., "active_watering") to its vanilla location name as defined in the
    // apworld (e.g, "Veggieville - Steal From Lemon").
    public static Dictionary<string, string> ItemIdToLocation = new()
    {
        // Veggieville
        {"active_watering", "Veggieville - Steal From Lemon"},
        {"key_flower", "Veggieville - Water Florist's Flower"},
        {"hat_sunhat", "Veggieville - Florist Reward"},
        {"hat_dlc", "Veggieville - Mailbox (Free Item)"},
        {"hat_cat", "Veggieville - Mailbox (Lost Cat Reward)"},
        // Weapon Woods
        {"active_sword", "Weapon Woods - Soil Sword Patch"},
        {"key_money", "Weapon Woods - Murder Jerry"},
        {"key_stool", "Weapon Woods - Babysitter Reward"},
        // Layer Lane
        {"key_trophy", "Layer Lane - Trophy Corner"},
        {"key_sub", "Layer Lane - Sandwich Stall"},
        {"key_wood", "Layer Lane - Shady Blueberry"},
        {"key_hammer", "Layer Lane - Shady Carrot"},
        {"key_estate", "Layer Lane - Invest in Real Estate"},
        {"hat_scissors", "Layer Lane - Edgar Reward"},
        // Bustling Barn
        {"hat_crown", "Bustling Barn - slayQueen32 Reward"},
        {"key_boots", "Bustling Barn - Boom Boots Room"},
        {"key_fertilizer", "Bustling Barn - Past The King Pig"},
        {"hat_hardhat", "Bustling Barn - Bald Beet Reward"},
        // Plain Plains
        {"key_carrot", "Plain Plains - Trash Can"},
        // Forsaken Farmhouse
        {"key_mask", "Forsaken Farmhouse - Donut Loot"},
        {"key_medicine", "Forsaken Farmhouse - Behind Key Block"},
        {"key_bandage", "Forsaken Farmhouse - Boom Boots Puzzle"},
        {"key_mittens", "Forsaken Farmhouse - Move Mittens Room"},
        {"key_cherry", "Forsaken Farmhouse - Cherrynapper"},
        {"key_cat", "Forsaken Farmhouse - Boss Arena"},
        {"key_paint", "Forsaken Farmhouse - Generator Room"},
        // Idle Icebox
        {"active_fork", "Idle Icebox - Deb Reward"},
        {"hat_fedora", "Idle Icebox - Pickled Gang Reward"},
        {"key_pops", "Idle Icebox - Pops' Reply"},
        {"hat_farmer", "Idle Icebox - Pops' Reward"},
        // Rocky Ramp
        {"key_phone", "Rocky Ramp - Bottom of Cliff"},
        // Forgotten Forest
        {"key_doodle", "Forgotten Forest - Mural Cave"},
        {"key_turnip", "Forgotten Forest - Turnip(?) Field"},
        {"active_petalportal", "Forgotten Forest - Petalportal Cave"},
        {"key_goop", "Forgotten Forest - Past the Stag"},
        {"hat_explorer", "Forgotten Forest - Annie Reward"},
        {"key_leaf", "Forgotten Forest - Acorn's Down Payment"},
        // Grim Graveyard
        {"hat_tophat", "Grim Graveyard - Graverobbing"},
        {"active_shovel", "Grim Graveyard - Tots' Trade"},
        {"key_tots", "Grim Graveyard - Tots' Request"},
        {"hat_bird", "Grim Graveyard - Tots' Reward"},
        // Bomb Bunker
        {"key_dye", "Bomb Bunker - Bathroom"},
        {"key_laser", "Bomb Bunker - Grum's Room"},
        {"active_pizza", "Bomb Bunker - Raid the Fridge"},
        {"key_hazmat", "Bomb Bunker - Bedroom 2 Item"},
    };

    private void Awake()
    {
        // Plugin startup logic
        BepinLogger = Logger;
        ArchipelagoClient = new ArchipelagoClient();
        ArchipelagoConsole.Awake();

        ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");

        // TODO: Fix GUID
        var harmony = new Harmony("foo.bar");
        harmony.PatchAll();
    }

    private void Start()
    {
        // Load the connection info
        var saveManager = Singleton<ReadWriteSaveManager>.Instance;
        ArchipelagoClient.ServerData.Uri = saveManager.GetData("ap_uri", "localhost");
        ArchipelagoClient.ServerData.SlotName = saveManager.GetData("ap_slot_name", "Player1");
        ArchipelagoClient.ServerData.Password = saveManager.GetData("ap_password", "");
    }

    private void OnGUI()
    {
        var playerManager = Singleton<PlayerManager>.Instance;
        var playerController = playerManager.GetConnectedPlayer();

        ArchipelagoConsole.OnGUI();

        string statusMessage;
        // show the Archipelago Version and whether we're connected or not
        if (!playerController)
        {
            // The player is still on the main menu
            // Hide the cursor
            Cursor.visible = false;

            GUI.color = Color.black;
            GUI.Label(new Rect(16, 10, 300, 20), ModDisplayInfo);
            GUI.Label(new Rect(16, 30, 300, 25), "Start or continue a game to connect.");
        }
        else if (ArchipelagoClient.Authenticated)
        {
            // Hide the cursor now that we're connected
            Cursor.visible = false;

            GUI.Label(new Rect(16, 10, 300, 20), ModDisplayInfo);
            statusMessage = " Status: Connected";
            GUI.Label(new Rect(16, 30, 300, 20), APDisplayInfo + statusMessage);
        }
        else
        {
            // Show and unlock the cursor until we're connected to the server
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            GUI.Label(new Rect(16, 10, 300, 20), ModDisplayInfo);
            statusMessage = " Status: Disconnected";
            GUI.Label(new Rect(16, 30, 300, 20), APDisplayInfo + statusMessage);
            GUI.Label(new Rect(16, 120, 150, 20), "Host: ");
            GUI.Label(new Rect(16, 140, 150, 20), "Player Name: ");
            GUI.Label(new Rect(16, 160, 150, 20), "Password: ");

            ArchipelagoClient.ServerData.Uri = GUI.TextField(new Rect(150, 120, 150, 20),
                ArchipelagoClient.ServerData.Uri);
            ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(150, 140, 150, 20),
                ArchipelagoClient.ServerData.SlotName);
            ArchipelagoClient.ServerData.Password = GUI.TextField(new Rect(150, 160, 150, 20),
                ArchipelagoClient.ServerData.Password);

            // requires that the player at least puts *something* in the slot name
            if (GUI.Button(new Rect(16, 180, 100, 20), "Connect") &&
                !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
            {
                ArchipelagoClient.Connect();

                // Save the connection info
                var saveManager = Singleton<ReadWriteSaveManager>.Instance;
                saveManager.SetData("ap_uri", ArchipelagoClient.ServerData.Uri);
                saveManager.SetData("ap_slot_name", ArchipelagoClient.ServerData.SlotName);
                saveManager.SetData("ap_password", ArchipelagoClient.ServerData.Password);
            }
        }

        // shift-R shortcut to die (to prevent potential softlocks)
        if (Event.current.Equals(Event.KeyboardEvent("#R")) && playerController)
        {
            playerController.Die();
        }
    }
}