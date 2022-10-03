# DualSense4Rockstar

Adds support for DualSense controllers to GTAV and RDR2.
Merge of the GTAV mod by JohnD https://github.com/zelmer69/dualsense4GTA5 and the RDR2 mod by Shtivi https://github.com/Shtivi/RDR2-DualSense
 with rewrites and refinements.
New controller scheme for GTA V.

### Requires DualSenseX by Paliverse
Github Link: https://github.com/Paliverse/DualSenseX | Steam Link:  https://store.steampowered.com/app/1812620/DSX/

These mods were built against the Steam version of DualSenseX which is fully supported. The GitHub version of DSX is *not* supported and/or recommended.

- GTA V requires ScriptHookVDotNet v3.5.1 by crosire https://github.com/crosire/scripthookvdotnet/releases and its dependencies and LemonUI for .NET v1.8 by Lemon https://github.com/LemonUIbyLemon/LemonUI and its dependencies
- RDR 2 ScriptHook v1 requires ScriptHookRDR2DotNet v1.0.5.5 by Saltyq https://github.com/saltyq/scripthookrdr2dotnet/releases and its dependencies
- RDR 2 requires ScriptHookRDR2DotNet v2 by TuffyTown https://github.com/Halen84/ScriptHookRDR2DotNet-V2/releases and its dependencies


## Features
Based upon JohnD's DualSense mod for Grand Theft Auto V using ScriptHook, this project adds support for Red Dead Redemption 2 with the adapted control layout originally by Shtivi.

### GTA V
Redefined controller layout and bug fixing
- Adds support for all weapons and switches the adaptive feedback. Pistols and shotguns e.g. use a pistol trigger, while automatic guns will provide feedback according to their fire rate and strength (WIP) Automatic weapons will switch from semi automativ trigger mode to automatic mode once the gun starts firing.


![image](https://user-images.githubusercontent.com/16738568/190234195-d3b623f1-ab29-48db-b357-997e5ba13d5f.png)

- Adds support for all weapons and switches the adaptive feedback. Pistols and shotguns e.g. use a pistol trigger, while automatic guns will provide feedback according to their fire rate and strength (WIP). Automatic weapons will switch from semi automativ trigger mode to automatic mode once the gun actually starts firing.
- (WIP)Reworked feedback during driving, depending on gear, if wheels are on ground, rpm, vehicle health etc. Adds haptic feedback for shifting gears.
- adds colored speedometer, brake light (mic LED)
- Changed Player LED, police siren loop; LED will also go into siren loop if the player is not wanted and has the vehicle's siren turned on
- Added support for DS controller new LED layout, fixing the Wanted stars not showing up
- Support for LemonUI for config (currently triggered with F10)

### RDR 2
- Based upon the layout and features from Shtivi
- Added haptic feedback for gun recoil, adapted gun trigger and gun cocking to the triggers
- Different haptics for double action revolver, hardened triggers while reloading
- Supports off hand weapons
- LEDs show current player health, can indicate the stamina and underpowered health core

## Installation Instructions & Download
Nightly builts version can be found here. As it's currently WIP, I have no 1.0 release planned.
https://github.com/Killface1980/DualSense4Rockstar/tree/master/__RELEASE