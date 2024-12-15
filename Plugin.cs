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
    public const string PluginVersion = "0.1.0";

    public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
    private const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
    public static ManualLogSource BepinLogger;
    public static ArchipelagoClient ArchipelagoClient;

    // Set to false to playtest a "vanilla" game without any item randomization or fommunitcation
    // with the AP server. Patches that only produce logs will still run.
    public static bool EnableRandomization = true;

    
    // Maps in-game item ID (e.g., "active_watering") to its vanilla location name as defined in the
    // apworld (e.g, "Veggieville - Steal From Lemon").
    public static Dictionary<string, string> ItemIdToLocation = new()
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

    private void OnGUI()
    {
        // show the mod is currently loaded in the corner
        GUI.Label(new Rect(16, 16, 300, 20), ModDisplayInfo);
        ArchipelagoConsole.OnGUI();

        string statusMessage;
        // show the Archipelago Version and whether we're connected or not
        if (ArchipelagoClient.Authenticated)
        {
            // Hide the cursor now that we're connected
            Cursor.visible = false;

            statusMessage = " Status: Connected";
            GUI.Label(new Rect(16, 50, 300, 20), APDisplayInfo + statusMessage);
        }
        else
        {
            // Show and unlock the cursor until we're connected to the server
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            statusMessage = " Status: Disconnected";
            GUI.Label(new Rect(16, 50, 300, 20), APDisplayInfo + statusMessage);
            GUI.Label(new Rect(16, 70, 150, 20), "Host: ");
            GUI.Label(new Rect(16, 90, 150, 20), "Player Name: ");
            GUI.Label(new Rect(16, 110, 150, 20), "Password: ");

            ArchipelagoClient.ServerData.Uri = GUI.TextField(new Rect(150, 70, 150, 20),
                ArchipelagoClient.ServerData.Uri);
            ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(150, 90, 150, 20),
                ArchipelagoClient.ServerData.SlotName);
            ArchipelagoClient.ServerData.Password = GUI.TextField(new Rect(150, 110, 150, 20),
                ArchipelagoClient.ServerData.Password);

            // requires that the player at least puts *something* in the slot name
            if (GUI.Button(new Rect(16, 130, 100, 20), "Connect") &&
                !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
            {
                ArchipelagoClient.Connect();
            }
        }
        // this is a good place to create and add a bunch of debug buttons
    }
}