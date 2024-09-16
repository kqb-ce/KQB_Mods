# Mods Description
- `JoinServerShortcuts.dll` - Adds shortcuts to dev console to allow direct connection to community servers, ex `joinserver dfw1` instead of `joinserver 333.333.333.333 5555`, also lists community servers with `joinserver list`
- `LosersCanDanceMod.dll` - Losing team can dance on the end screen (yes, it works serverside as well)
- `PanAudioMod.dll` - Fixes queen and mace audio to work in stereo

## Installing
### Windows:
1. Install BepInEx to load the mod [Download BepInEx](https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip)
2. extract BepInEx and move `doorstop_config`, `winhttp.dll` and `BepInEx` to `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black`
3. Start KQB to initialize, then quit. There should now be a `Plugins` folder under `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\`
4. Download The [latest release zip](https://github.com/kqb-ce/KQB_Mods/releases/download/v1.0.0/KQB-Mods-v1_0_0.zip) of the KQB mods from the [Releases Page](https://github.com/kqb-ce/KQB_Mods/releases/)
5. Extract any DLLs you want to add to KQB to `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\Plugins` start the game and enjoy!

### Mac & Linux: 
TODO

---
## Uninstalling
To uninstall any individual mod simply remove the `.dll` C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\plugins\PanAudioMod.dll

To completely uninstall BepInEx, remove the following objects:
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\doorstop_config
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\winhttp.dll
 
now your game is back to normal.
 
---
## Building from source

1. Install visual studio 2019
2. clone this repo
3. open Visual Studio and choose `File -> Open -> Solution` and select `KQB_Mods\Source\KQBMods\KQBMods.sln`
4. This repo does not contain third party libraries, you must provide those yourself
   - In the file explorer, expand a Mod Project's references and observe any with a yellow triangle, this means that required library is not in the projects path
   - Copy missing libraries into the directory `KQB_Mods\Source\KQBMods\Libraries`
6. Build the project with `ctrl+shift+b` 
7. Find each mod in a directory like `KQB_Mods\Source\KQBMods\<ModName>\obj\Debug\<ModName>.dll` and add it to `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\Plugins`
