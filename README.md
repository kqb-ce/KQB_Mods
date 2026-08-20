# Mods Description
- `kqb.treebones.dancinglosers.dll` - Dance your heart out, even when you lose
- `kqb.treebones.dancingisforbidden.dll` - Stop those losers from dancing! If you are host, loser dances will change back to clapping for all people in the lobby. If you are not host, losers will only clap for you.
- `kqb.devconsole.dll` - Re-enable dev console. It's pretty useless these days.
- `kqb.treebones.custommenubg.dll` - Miss the old, non-AI title screen? So did I. By default, this replaces it with the old art. But if you want custom art, add whatever jpgs or pngs you want to in `<KQB-Game-Directory>/Bepinex/plugins/images` and it'll randomly select an image when you launch the game.
- `kqb.treebones.striprtf.dll` - Removes rtf tags from usernames
- `kqb.treebones.serveronly.dll` - allows you to run the game as a headless server. 
  - To enable this mod, include `-batchmode -nographics --lobby <lobbyName>` in the steam launch options and a lobby with the named <lobbyName> will be started
  - you can include `--pass <password>` to create a password-protected private lobby
  - for example to create a lobby named `WEST` with the password `nice` use the launch options: `-batchmode -nographics --lobby WEST --pass nice`

    <img src="https://i.imgur.com/tWeQXX0.png" width="800"/>
## Windows
### To Install for the first time
1. Download The [Latest Release](https://github.com/kqb-ce/KQB_Mods/releases/download/v1.1.2/KQB_Mods.zip)
2. extract to `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black`

The mods are now installed & should be working the next time you start the game.

### Adding a new plugin after installing:
1. Download the `.dll` you wish to add from the [Mods directory]https://github.com/kqb-ce/KQB_Mods/tree/main/Mods
2. Add it to `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\plugins`

### To Uninstall Completely

Remove the following objects:
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\doorstop_config
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\.doorstop_version
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\winhttp.dll
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\changelog

now your game is back to normal.

### To Uninstall a single mod:
Simply remove the `.dll` from the `plugins` directory. 

For example, to remove EmoteMapping but keep SnailFix, remove `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\plugins\kqb.treebones.emotemapping.dll`


## Linux: 
### To Install for the first time

1. Download The [Latest Release](https://github.com/kqb-ce/KQB_Mods/releases/download/v1.0.0/KQB_Mods.zip)
2. extract to `/home/$USER/.local/share/Steam/steamapps/common/Killer Queen Black` (or wherever your local game files are)
3. In your steam library, select Killer Queen Black, click the settings icon, select properties and in the "Launch Options" section add the following: `WINEDLLOVERRIDES="winhttp.dll=n,b" %command%`

### Adding a new plugin after installing:
1. Download the `.dll` you wish to add from the [Mods directory]https://github.com/kqb-ce/KQB_Mods/tree/main/Mods
2. Add it to `/home/$USER/.local/share/Steam/steamapps/common/Killer Queen Black/BepInEx/plugins`

### To Uninstall Completely

To completely uninstall BepInEx and all mods, remove the following objects:
 - /home/$USER/.local/share/Steam/steamapps/common/Killer Queen Black/BepInEx
 - /home/$USER/.local/share/Steam/steamapps/common/Killer Queen Black/doorstop_config
 - /home/$USER/.local/share/Steam/steamapps/common/Killer Queen Black/.doorstop_version
 - /home/$USER/.local/share/Steam/steamapps/common/Killer Queen Black/winhttp.dll
 - /home/$USER/.local/share/Steam/steamapps/common/Killer Queen Black/changelog

And remove `WINEDLLOVERRIDES="winhttp.dll=n,b" %command%` from the games launch options

now your game is back to normal.

### To Uninstall a single mod:

Simply remove the `.dll` from the `plugins` directory. 

For example, to remove EmoteMapping but keep SnailFix, remove `/home/$USER/.local/share/Steam/steamapps/common/Killer Queen Black/BepInEx/plugins/kqb.treebones.emotemapping.dll`

---
## Building from source

This assumes you have already have BepInEx installed 

1. Install visual studio 2026
2. clone this repo
3. Open the solution file for the mod you wish to build
4. This repo does not contain third party libraries, you must provide those yourself
   - In the file explorer, expand a Mod Project's references and observe any with a yellow triangle, this means that required library is not in the projects path
   - Copy missing libraries into the directory `KQB_Mods\Source\lib`
6. Build the project with `ctrl+shift+b` 
7. Find each mod in a directory like `KQB_Mods\Source\PanAudio\obj\Debug\netstandard2.1\<ModName>.dll` and add it to `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\Plugins\`
