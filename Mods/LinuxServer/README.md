# Linux Server
Running a headless linux server

## Installing
1. Copy your game files to a temp location `/server/Killer Queen Black`
2. Install Unity 2019.4.17f1
3. Copy the LinuxPlayer, UnityPlayer, and _s.debug files &  from unity (TODO: verify this path...)`C:\Program Files\Unity\Hub\Editor\Editor\Data\PlaybackEngines\LinuxStandaloneSupport\Variations\linux64_headless_nondevelopment_mono`to `server/Killer Queen Black`
4. rename LinuxPlayer to `Killer Queen Black.x86_64`
5. replace `/server/Killer Queen Black/Killer Queen Black_Data/MonoBleedingEdge` with `C:\Program Files\Unity\Hub\Editor\Editor\Data\PlaybackEngines\LinuxStandaloneSupport\Variations\linux64_headless_nondevelopment_mono\Data\MonoBleedingEdge`
6. Install BepInEx for linux [Download BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html?tabs=tabid-nix)
7. Start KQB to initialize BepInEx, then quit. There should now be a `Plugins` folder under `/server/Killer Queen Black/BepInEx/`
8. Clone this repo 
9. From the cloned repo Add `libnetcode-io.so`, `libnext.so`, `libreliable-io.so`, `libsteam_api64.so` TO `/server/Killer Queen Black`
10. Add `BepInEx/LinuxServer.dll` to `/server/Killer Queen Black/BepInEx/Plugins`
11. In the script `run_bepinex.sh` set the executable to be` executable_name="Killer Queen Black.x86_64";`
12. Update the last line of the script to include the --start-server args: `"${executable_path}" --spectator --start-server 2444 -batchmode -nographics`