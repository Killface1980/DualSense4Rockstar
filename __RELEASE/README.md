---------------------------
# 2024-07-20
Updated version of the mods can currently be found at the 2024 branch here: https://github.com/Killface1980/DualSense4Rockstar/tree/update-2024/__RELEASE

---------------------------

# Installation instructions
- Head over to the RDR2/GTAV folder above.
- Click the zip file inside. *For the RDR2SHDN v1 mod, select the dlls one by one.*
- Then click on the "Download" button on the right side.
- Extract the contents of the zip files in their respective GTA V / RDR 2 **Scripts** folder inside.
- You might have to create the folder in case it doesn't exist, please also make sure to to match all the prerequisites listed below.
- Modded GTA V and RDR 2 might require to be run in administrator mode. 
- Make sure there's an (empty) text file at `C:\Temp\DualSenseX\DualSenseX_PortNumber.txt` and DSX is using it as Text Source (green light under 'Home')

## Prerequisites
### GTA V
- Requires [Script Hook V](http://www.dev-c.com/gtav/scripthookv/). Download and install. For a video guide see below.
- Requires [ScriptHookVDotNet v3.5.1](https://github.com/crosire/scripthookvdotnet/releases) by crosire and its [dependencies](https://github.com/crosire/scripthookvdotnet). Extract the contents of the zip found under the releases link in your GTA V main directory. Make sure to download the current version from GitHub, info and installation instructions can be found [here](https://gta5-mods.com/tools/scripthookv-net).
- Requires [LemonUI for .NET v1.8](https://github.com/LemonUIbyLemon/LemonUI/releases) by Lemon and its dependencies. [Homepage](https://gta5-mods.com/tools/lemonui) | [Installation instructions](https://github.com/LemonUIbyLemon/LemonUI#installation). In case you're running this mod with ScriptHookDotNet, select the files from the SHVDN3 folder.  

Example folder structure with SH v3  
`<Grand Theft Auto V>`  
`├ <...>`  
`├ <Scripts>`  
`│ ├─ DSX_Base.dll`  
`│ ├─ DualSense4GTAV.dll`  
`│ ├─ LemonUI.SHVDN3.dll`  
`│ ├─ LemonUI.SHVDN3.pdb`  
`│ ├─ LemonUI.SHVDN3.xml`  
`│ └─ Newtonsoft.Json.dll`  
`├─ <...>`  
`├─ dinput8.dll`  
`├─ NativeTrainer.asi` *<= optional*  
`├─ ScriptHookV.dll`  
`├─ ScriptHookVDotNet.asi`  
`├─ ScriptHookVDotNet.ini`  
`├─ ScriptHookVDotNet2.dll`  
`├─ ScriptHookVDotNet2.xml`  
`├─ ScriptHookVDotNet3.dll`  
`├─ ScriptHookVDotNet3.xml`  
`├─ <...>`  

### RDR 2
- Requires [Script Hook RDR2](http://www.dev-c.com/rdr2/scripthookrdr2/). Download and install. For a video guide see below.
- Requires [ScriptHookRDR2DotNet V2](https://github.com/Halen84/ScriptHookRDR2DotNet-V2/releases) by TuffyTown and its [dependencies](https://github.com/Halen84/ScriptHookRDR2DotNet-V2). Extract the contents of the zip found under the releases link in your RDR2 main directory. The mod is primarily created to be used with ScriptHookRDR2DotNet V2.
- *SHRDN v1 version of this mod is outdated and is unlikely to be updated as I currently have no mods installed depending on v1*

Example folder structure with SH v2  
`<Red Dead Redemption 2>`  
`├ <...>`  
`├ <Scripts>`  
`│ ├─ DSX_Base.dll`  
`│ ├─ DualSense4RDR2.dll`  
`│ └─ Newtonsoft.Json.dll`  
`├ <...>`  
`├─ dinput8.dll`  
`├─ NativeTrainer.asi` *<= optional*  
`├─ ScriptHookRDR2.dll`  
`├─ ScriptHookRDRDotNet.asi`  
`├─ ScriptHookRDRDotNet.ini`  
`├─ ScriptHookRDRNetAPI.dll`  
`├─ <...>`  

### Rumble / Vibration from Game
The mod doesn't alter the vibration. In case you're missing the regular rumble/vibration from the game (GTA V, RDR 2), try setting the emulation in DSX to XBOX360. This worked in my case. *Emulation off could also work (unverified/cannot emember).* Emulating the DS4 with DSX v2.3.0 turned off all rumble on my machine. The only downside to the 360 emu is that the game will not use the PS icons. Additional mods to face the issue: [RDR 2 PlayStation Icons Replacement](https://www.nexusmods.com/reddeadredemption2/mods/660) - [GTAV PS4 Gamepad Icons 1.3](https://www.gta5-mods.com/misc/ps4-gamepad-icons)

### ScriptHook video installation instructions 
[![image](https://user-images.githubusercontent.com/16738568/197985654-8c0ea9c1-b99d-498e-908c-9e7cb2c8796e.png)](https://www.youtube.com/watch?v=cGW27hvRRWI&ab_channel=BallerMcBallerson)

for RDR2 can be found [here](https://www.youtube.com/watch?v=cGW27hvRRWI&ab_channel=BallerMcBallerson)
. These instructions can also be applied to GTA V and ScriptHookDotNet.
