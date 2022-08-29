using GTA;
using GTA.Native;
using Shared;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using static DSX_Base.Client.Class1;

namespace DualSense4GTAV
{
    public class Main : Script
    {
        private static bool engine;
        private static int lastBrakeFreq = 0;
        private static int lastBrakeResistance = 200;
        private static int lastThrottleResistance = 1;
        private static bool noammo;
        private static Ped playerped;
        private static Weapon playerWeapon;
        private static bool showbatstat = true;
        private static bool showconmes = true;
        private static DateTime TimeSent;
        private static bool wanted = false;

        public Main()
        {
            playerped = Game.Player.Character;
            base.Tick += onTick;
            base.KeyDown += onKeyDown;
            Connect();
            Process.GetProcessesByName("DSX");
        }


        private void onKeyDown(object sender, KeyEventArgs e)
        {
            Packet packet = new Packet();
            packet.instructions = new Instruction[4];
            if (e.KeyCode == Keys.F9)
            {
                iO obj = new iO();
                int bat = 0;
                bool isconnected = false;
                Send(packet);
                obj.getstat(out bat, out isconnected);
                UI.Notify("Controller connection status: " + isconnected + " controller battery status: " + bat + "% \n to hide this Press F10");
            }
            if (e.KeyCode == Keys.F10)
            {
                showbatstat = !showbatstat;
            }
            if (e.KeyCode == Keys.F11)
            {
                showconmes = !showconmes;
            }
        }

        private void onTick(object sender, EventArgs e)
        {
            Packet packet = new Packet();
            int controllerIndex = 0;
            packet.instructions = new Instruction[4];
            playerped = Game.Player.Character;
            playerWeapon = playerped.Weapons.Current;
            if (!wanted)
            {
                if (playerped.Model == "player_zero")
                {
                    packet.instructions[1].type = InstructionType.RGBUpdate;
                    packet.instructions[1].parameters = new object[4] { controllerIndex, 0, 0, 255 };
                }
                if (playerped.Model == "player_two")
                {
                    packet.instructions[1].type = InstructionType.RGBUpdate;
                    packet.instructions[1].parameters = new object[4] { controllerIndex, 255, 121, 0 };
                }
                if (playerped.Model == "player_one")
                {
                    packet.instructions[1].type = InstructionType.RGBUpdate;
                    packet.instructions[1].parameters = new object[4] { controllerIndex, 0, 255, 0 };
                }
                Send(packet);
            }
            if (playerped.IsInVehicle())
            {
                Vehicle currentVehicle = playerped.CurrentVehicle;

                if (!currentVehicle.EngineRunning)
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left);
                }
                else if (currentVehicle.IsInAir)
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left);

                }
                else if (currentVehicle.EngineHealth <= 0f)
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left);
                }
                // else if (currentVehicle.EngineHealth <= 400f)
                // {
                //     SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VibrateTrigger, new() { 6 });
                //     SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VibrateTrigger, new() { 6 });
                // }
                // else if (currentVehicle.EngineHealth <= 600f)
                // {
                //     SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VibrateTrigger, new() { 3 });
                //     SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VibrateTrigger, new() { 3 });
                // }
                //else if (currentVehicle.EngineHealth <= 800f)
                //{
                //    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Choppy);
                //    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Choppy);
                //}
                else if (currentVehicle.IsTireBurst(1) ||
                         currentVehicle.IsTireBurst(2) ||
                         currentVehicle.IsTireBurst(3) ||
                         currentVehicle.IsTireBurst(4) ||
                         currentVehicle.IsTireBurst(5) ||
                         currentVehicle.IsTireBurst(6))
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Hardest);
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Hardest);
                }
                else // if (playerped.CurrentVehicle.EngineHealth >= 1000f)
                {
                    float health = 1f - (currentVehicle.EngineHealth / 1000f);
                    float healthMalus = (int)(health * 2.5f);
                    int currentGear = currentVehicle.CurrentGear;
                    float currentRpm = currentVehicle.CurrentRPM;
                    var currentSpeed = currentVehicle.Speed;
                
                    int resistance = 5 - currentGear;
                
                    // UI.ShowSubtitle(resistance.ToString());

                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { (int)(7f -
                        healthMalus), resistance + (int)healthMalus });

                    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 8-
                        (currentGear) -
                        (int)(healthMalus / 2f), 8-resistance });

                    // SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 1, 220 *
                    //     (int)( health), 40);
                    // SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 1, 220 *
                    //     (int)( health), 40);



                    // if (currentGear <= 1)
                    // {
                    //     SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 6, 3 });
                    //     SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 6, 3 });
                    // }
                    // else
                    // {
                    //     SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 6, 5 - currentGear });
                    //     SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 6, 2 + currentGear });
                    // }
                }

                _ = wanted;
            }
            else
            {
                _ = wanted;

                if (playerped.IsReloading)
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left);

                }
                else
                {
                    // switch (playerweapon.Hash)
                    // {
                    //     case WeaponHash.Knife:
                    //     case WeaponHash.Nightstick:
                    //     case WeaponHash.Hammer:
                    //     case WeaponHash.Bat:
                    //     case WeaponHash.GolfClub:
                    //     case WeaponHash.Crowbar:
                    //     case WeaponHash.Bottle:
                    //     case WeaponHash.SwitchBlade:
                    //     case WeaponHash.BattleAxe:
                    //     case WeaponHash.PoolCue:
                    //     case WeaponHash.Wrench:
                    //     case WeaponHash.StoneHatchet:
                    //     case WeaponHash.Pistol:
                    //     case WeaponHash.PistolMk2:
                    //     case WeaponHash.CombatPistol:
                    //     case WeaponHash.APPistol:
                    //     case WeaponHash.Pistol50:
                    //     case WeaponHash.FlareGun:
                    //     case WeaponHash.MarksmanPistol:
                    //     case WeaponHash.Revolver:
                    //     case WeaponHash.RevolverMk2:
                    //     case WeaponHash.DoubleActionRevolver:
                    //     case WeaponHash.UpNAtomizer:
                    //     case WeaponHash.CeramicPistol:
                    //     case WeaponHash.NavyRevolver:
                    //     case WeaponHash.PericoPistol:
                    //     case WeaponHash.MicroSMG:
                    //     case WeaponHash.SMG:
                    //     case WeaponHash.SMGMk2:
                    //     case WeaponHash.AssaultSMG:
                    //     case WeaponHash.CombatPDW:
                    //     case WeaponHash.MiniSMG:
                    //     case WeaponHash.AssaultRifle:
                    //     case WeaponHash.AssaultrifleMk2:
                    //     case WeaponHash.CarbineRifle:
                    //     case WeaponHash.CarbineRifleMk2:
                    //     case WeaponHash.AdvancedRifle:
                    //     case WeaponHash.CompactRifle:
                    //     case WeaponHash.MilitaryRifle:
                    //     case WeaponHash.HeavyRifle:
                    //     case WeaponHash.MG:
                    //     case WeaponHash.CombatMG:
                    //     case WeaponHash.CombatMGMk2:
                    //     case WeaponHash.UnholyHellbringer:
                    //     case WeaponHash.PumpShotgun:
                    //     case WeaponHash.PumpShotgunMk2:
                    //     case WeaponHash.SawnOffShotgun:
                    //     case WeaponHash.AssaultShotgun:
                    //     case WeaponHash.BullpupShotgun:
                    //     case WeaponHash.DoubleBarrelShotgun:
                    //     case WeaponHash.SweeperShotgun:
                    //     case WeaponHash.CombatShotgun:
                    //     case WeaponHash.StunGun:
                    //     case WeaponHash.StunGunMultiplayer:
                    //     case WeaponHash.SniperRifle:
                    //     case WeaponHash.HeavySniper:
                    //     case WeaponHash.HeavySniperMk2:
                    //     case WeaponHash.GrenadeLauncher:
                    //     case WeaponHash.GrenadeLauncherSmoke:
                    //     case WeaponHash.CompactGrenadeLauncher:
                    //     case WeaponHash.CompactEMPLauncher:
                    //     case WeaponHash.RPG:
                    //     case WeaponHash.Minigun:
                    //     case WeaponHash.Widowmaker:
                    //     case WeaponHash.Grenade:
                    //     case WeaponHash.StickyBomb:
                    //     case WeaponHash.SmokeGrenade:
                    //     case WeaponHash.BZGas:
                    //     case WeaponHash.Molotov:
                    //     case WeaponHash.PipeBomb:
                    //     case WeaponHash.FireExtinguisher:
                    //     case WeaponHash.PetrolCan:
                    //     case WeaponHash.HazardousJerryCan:
                    //     case WeaponHash.FertilizerCan:
                    //     case WeaponHash.SNSPistol:
                    //     case WeaponHash.SNSPistolMk2:
                    //     case WeaponHash.SpecialCarbine:
                    //     case WeaponHash.SpecialCarbineMk2:
                    //     case WeaponHash.HeavyPistol:
                    //     case WeaponHash.BullpupRifle:
                    //     case WeaponHash.BullpupRifleMk2:
                    //     case WeaponHash.HomingLauncher:
                    //     case WeaponHash.ProximityMine:
                    //     case WeaponHash.Snowball:
                    //     case WeaponHash.VintagePistol:
                    //     case WeaponHash.Dagger:
                    //     case WeaponHash.Firework:
                    //     case WeaponHash.Musket:
                    //     case WeaponHash.MarksmanRifle:
                    //     case WeaponHash.MarksmanRifleMk2:
                    //     case WeaponHash.HeavyShotgun:
                    //     case WeaponHash.Gusenberg:
                    //     case WeaponHash.Hatchet:
                    //     case WeaponHash.Railgun:
                    //     case WeaponHash.Unarmed:
                    //     case WeaponHash.KnuckleDuster:
                    //     case WeaponHash.Machete:
                    //     case WeaponHash.MachinePistol:
                    //     case WeaponHash.Flashlight:
                    //     case WeaponHash.Ball:
                    //     case WeaponHash.Flare:
                    //     case WeaponHash.NightVision:
                    //     case WeaponHash.Parachute:
                    //     default:
                    //         break;
                    // }
                    int frequency = 7;
                    switch (playerWeapon.Group)
                    {
                        case WeaponGroup.Pistol:

                            // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Soft);
                            // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.SemiAutomaticGun, new() { 2, 7, 8 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 1 });
                            if (playerWeapon.Hash == WeaponHash.APPistol)
                            {
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new() { 7, 8, 12 });

                            }
                            else
                            {
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 6, 8, 8 });
                            }
                            //UI.ShowSubtitle("Doing great, aiming widda pistal");
                            break;

                        case WeaponGroup.SMG:
                            // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                            // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.VibrateTrigger, new() { 40 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 2 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Machine, new() { 7, 9, 4, 4, frequency, 0 });
                            break;

                        case WeaponGroup.AssaultRifle:
                            // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                            // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.AutomaticGun, new() { 2, 7, 8 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 3 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new() { 7, 8, 15 });
                            break;

                        case WeaponGroup.MG:
                            // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                            // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.VibrateTrigger, new() { 40 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 4 }); //1,3
                            SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new() { 7, 8, 9 });
                            break;

                        case WeaponGroup.Shotgun:

                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 2 });
                            if (playerWeapon.Hash == WeaponHash.AssaultShotgun || playerWeapon.Hash == WeaponHash.SweeperShotgun || playerWeapon.Hash == WeaponHash.HeavyShotgun)
                            {
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new() { 7, 8, 3 });
                            }
                            else if (playerWeapon.Hash == WeaponHash.BullpupShotgun)
                            {
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new() { 7, 8, 1 });
                            }
                            else
                            {
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 2, 4, 8, 4 });

                            }
                            // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hardest);
                            // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.Bow, new() { 0, 8, 2, 5 });
                            //UI.ShowSubtitle("Shotgun");
                            break;

                        case WeaponGroup.Sniper:
                            //SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hardest);
                            //SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.Hard);
                            //SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Galloping, new(){4,9,1,7,1});
                            //SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.SemiAutomaticGun, new(){4,6,8});
                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 4 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.SemiAutomaticGun, new() { 2, 6, 8 });
                            //UI.ShowSubtitle("Sniper");

                            break;

                        case WeaponGroup.Heavy:
                            if (playerWeapon.Hash == WeaponHash.Minigun)
                            {
                                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 4 });
                                //SetAndSendPacket(packet, controllerIndex, Trigger.Right,  TriggerMode.VibrateTrigger, new() { 39 });
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new() { 8, 8, 40 });

                            }
                            else
                            {
                                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 4 });
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 6, 8, 8 });
                            }
                            break;

                        default:
                        case WeaponGroup.Unarmed:
                        case WeaponGroup.Melee:
                        case WeaponGroup.Thrown:
                        case WeaponGroup.PetrolCan:
                            SetAndSendPacket(packet, controllerIndex, Trigger.Left);
                            SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                            break;
                    }
                }
            }
            add add2 = new add();
            iO obj = new iO();
            int bat = 0;
            Send(packet);
            Script.Wait(235);
            obj.getstat(out bat, out bool _);
            if ((false) && showconmes)
            {
                UI.ShowSubtitle("controller is disconnected or discharged, please fix or press F11");
            }
            if (bat <= 15 && showbatstat)
            {
                UI.ShowHelpMessage("Your controller battery is  " + bat + " to hide this message press F10", 1, sound: false);
            }
            switch (Game.Player.WantedLevel)
            {
                case 0:
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, false, false, false, false, false };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.AllOff };
                    Send(packet);


                    Script.Wait(299);
                    Script.Wait(299);
                    wanted = false;
                    break;

                case 1:
                {
                    int blue = 0;
                    int red = 0;
                    add2.rgbupdat2e(10, playerped.Health, out red, out blue);
                    wanted = true;
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, false, false, false, false };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.One};
                    Send(packet);


                    packet.instructions[1].type = InstructionType.RGBUpdate;
                    packet.instructions[1].parameters = new object[4] { controllerIndex, blue, 0, red };
                    Send(packet);
                    break;
                }
                case 2:
                    add2.rgbupdat2e(30, playerped.Health, out int _, out int _);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, false, false, false };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Two };
                    Send(packet);


                    wanted = true;
                    break;

                case 3:
                    add2.rgbupdat2e(40, playerped.Health, out int _, out int _);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, false, false };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Three};
                    Send(packet);


                    wanted = true;
                    break;

                case 4:
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, true, false };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Four};
                    Send(packet);


                    add2.rgbupdat2e(50, playerped.Health, out int _, out int _);
                    wanted = true;
                    break;

                case 5:
                    add2.rgbupdat2e(70, playerped.Health, out int _, out int _);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, true, true };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Five};
                    Send(packet);


                    wanted = true;
                    break;
            }
        }

    }
}