# DualSense4Rockstar

Adds support for DualSense controllers to GTAV and RDR2.
Merge of the GTAV mod by JohnD https://github.com/zelmer69/dualsense4GTA5 and the RDR2 mod by Shtivi https://github.com/Shtivi/RDR2-DualSense
 with rewrites and refinements.
New controller scheme for GTA V.

Requires DualSenseX by Paliverse https://dualsensex.com/download/

GTA V requires ScriptHookVDotNet v3.5.1 by crosire https://github.com/crosire/scripthookvdotnet/releases and its dependencies

RDR 2 ScriptHook v1 requires ScriptHookRDR2DotNet v1.0.5.5 by Saltyq https://github.com/saltyq/scripthookrdr2dotnet/releases and its dependencies

RDR 2 requires ScriptHookRDR2DotNet v2 by TuffyTown https://github.com/Halen84/ScriptHookRDR2DotNet-V2/releases and its dependencies


## Features
Based upon JohnD's DualSense mod for Grand Theft Auto V using ScriptHook, this project adds support for Red Dead Redemption 2 with the adapted control layout originally by Shtivi.

### GTA V
Redefined controller layout and bug fixing
- Adds support for all weapons and switches the adaptive feedback. Pistols and shotguns e.g. use a pistol trigger, thile automatic guns will provide feedback according to their fire rate and strength
- (WIP)Reworked feedback during driving, depending on gear, if wheels are on ground etc.
- Changed Player LED, police siren loop; LED will also go into siren loop if the player is not wanted and has the vehicle's siren turned on
- Added support for DS controller new LED layout, fixing the Wanted stars not showing up

### RDR 2
- Added support for DSX using ScriptHook.
- Adapted controller layout and features from Shtivi. All credits to the excellent feel of this layout go to him.