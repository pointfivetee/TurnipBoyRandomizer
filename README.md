# Turnip Boy Commits Tax Evasion Archipelago Client
This is a client mod for Turnip Boy Commits Tax Evasion that makes it compatible with the Archipelago Multiworld Randomizer.

This mod randomizes all inventory items and hats. Keys, heart fruits, and documents are currently not randomized. The goal is to reach and defeat Corrupt Onion (normal version of the fight).

# Tips
- You can use shift-R to die on command and respawn at the entrance to your current area or in front of your greenhouse, depending on your current location. In addition to being a time-saver, this may be necessary to prevent a soft-lock in certain situations. For instance, collecting the check from the mural cave does not currently unlock the door behind you, so the only escape is death.
- If your items have gotten out of sync, try quitting and restarting the game, selecting Continue from the main menu, and then reconnecting.

## Installation
- Must be using a PC version of Turnip Boy Commits Tax Evasion
- [Download the latest version of this mod](https://github.com/pointfivetee/TurnipBoyRandomizer/releases/latest/download/turnip_boy_mod.zip).
- Extract and copy the zip file into the root folder where Turnip Boy is located. By default it should be something like:
`C:\Program Files\Epic Games\TurnipBoyCommitsTaxEvad` or `C:\SteamLibrary\steamapps\common\Turnip Boy Commits Tax Evasion`
	- Windows likes to place an intermediary folder when extracting zip files. Make sure that the `BepInEx` folder, `doorstop_config.ini`, and `winhttp.dll` are all located directly in the game's install folder.
- Launch the game and close it. This will finalize the installation for BepInEx.
- Launch the game again and you should see a new bit of UI in the top-left of the screen showing `Archipelago v0.5.0 Status: Not Connected`. After starting or continuing a game, you will also see text fields to enter connection info.
- To uninstall the mod, either remove/delete the `TBCTEArchipelagoPlugin` folder from the `plugins` folder, or rename the winhttp.dll file in the game's root directory (this will disable all mods from running).

## Generating a Multiworld
- In order to setup a multiworld you must first install the latest version of [Archipelago](https://github.com/ArchipelagoMW/Archipelago/releases/latest)
	- When running the Archipelago setup exe you'll want to at least install the Generator and Text Client
- After installing, run `Archipelago Launcher` and click on `Browse Files`. This will open the local file directory for your Archipelago installation.
- [Download the yaml template file](https://github.com/pointfivetee/TurnipBoyRandomizer/releases/latest/download/TurnipBoy.yaml). Place `TurnipBoy.yaml` inside the `Players` folder
	- The yaml file needs to be edited before you can generate a game. Open the file and fill in your player name and choose the settings you want.
	- You will also need to get the .yaml files for everyone who is joining your multiworld and place them inside
- [Download the latest version of the .apworld file](https://github.com/pointfivetee/TurnipBoyRandomizer/releases/latest/download/turnipboy.apworld).
- Place the `turnipboy.apworld` file inside of `lib/worlds`. 
- Once all of the .yaml files have been configured and placed into Players, click `Generate` in the Archipelago Launcher.
- A .zip file will be created in the `output` folder containing information for your multiworld. In order to host your game, go to [https://archipelago.gg/uploads](https://archipelago.gg/uploads) and upload the .zip file, then click `Create New Room`.

## Connecting to a Room
Once you start or continue a game from the main menu with the client mod installed, you'll see connection info in the top left. Enter the link to your room, your player name, and the room's password. Then click Connect.