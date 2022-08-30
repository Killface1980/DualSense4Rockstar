using GTA;
using GTA.Native;
using Shared;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DSX_Base.Client;
using static DSX_Base.Client.iO;

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
        private add _add2 = null;
        private iO _obj = null;

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
            Packet packet = new();
            packet.instructions = new Instruction[4];
            if (e.KeyCode == Keys.F9)
            {
                iO obj = new();
                int bat = 0;
                bool isconnected = false;
                Send(packet);
                obj.getstat(out bat, out isconnected);
                GTA.UI.Notification.Show("Controller connection status: " + isconnected + " controller battery status: " + bat + "% \n to hide this Press F10");
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
            Packet packet = new();
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
                /*
                if (playerped.IsInVehicle())
                {
                    Vehicle currentVehicle = playerped.CurrentVehicle;
                    OutputArgument outR = new();
                    OutputArgument outG = new();
                    OutputArgument outB = new();

                    Function.Call(GTA.Native.Hash.GET_VEHICLE_COLOR, currentVehicle, outR, outG, outB);
                    int red = outR.GetResult<int>();
                    int green = outG.GetResult<int>();
                    int blue = outB.GetResult<int>();
                    // GTA.UI.Screen.ShowSubtitle(red + " - " + green + " - " + blue);
                    packet.instructions[1].type = InstructionType.RGBUpdate;
                    packet.instructions[1].parameters = new object[4] { controllerIndex, red, green, blue };

                }
                */

                Send(packet);
            }
            if (playerped.IsInVehicle())
            {
                Vehicle currentVehicle = playerped.CurrentVehicle;


                if (!currentVehicle.IsEngineRunning)
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
                
                else if (currentVehicle.Wheels.Any(x=> x.IsBursted))
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
                
                    int resistance = 4 - currentGear;
                    //GTA.Native.Hash.traction
                    // UI.ShowSubtitle(resistance.ToString());

                    //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { (int)(6f -
                    //    healthMalus), Math.Min(resistance + (int)healthMalus, 8) });
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 5, Math.Max(1, Math.Min(resistance + (int)healthMalus, 8)) });

                    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 7-
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

                if (playerped.IsReloading || playerWeapon.AmmoInClip == 0)
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 6, 8, 8 });
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left);
                }
                else
                {
                    

                    float fireRate = Function.Call<float>(Hash._GET_WEAPON_TIME_BETWEEN_SHOTS, playerWeapon.Hash);
                    float weaponDamage = Function.Call<float>(Hash.GET_WEAPON_DAMAGE, playerWeapon.Hash);

                    int weaponStrength = 4 + (int)(weaponDamage / 8f);
                    weaponStrength = Math.Min(weaponStrength, 8);
                    int fireRateAutomaticInt = (int)(1.4f / fireRate);
                    //GTA.UI.Notification.Show(weaponStrength.ToString());

                    
                    switch (playerWeapon.Group)
                    {
                        case WeaponGroup.Pistol:

                            // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Soft);
                            // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.SemiAutomaticGun, new() { 2, 7, 8 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 1 });
                          //  if (playerWeapon.Hash == WeaponHash.APPistol)
                        {
                            //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                            //    new() { 8, weaponStrength, fireRateAutomaticInt });

                        }
                         //   else
                            {
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 6, 8, 8 });
                            }
                            //UI.ShowSubtitle("Doing great, aiming widda pistal");
                            break;

                        case WeaponGroup.SMG:
                            // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                            // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.VibrateTrigger, new() { 40 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 2 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Machine, new() { 7, 9, 4, 4, fireRateAutomaticInt, 0 });
                            break;

                        case WeaponGroup.AssaultRifle:
                            // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                            // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.AutomaticGun, new() { 2, 7, 8 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 3 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                                    new() { 8, weaponStrength, fireRateAutomaticInt });

                            break;

                        case WeaponGroup.MG:
                            // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                            // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.VibrateTrigger, new() { 40 });
                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 4 }); //1,3
                            SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new() { 8, weaponStrength, fireRateAutomaticInt });
                            break;

                        case WeaponGroup.Shotgun:

                            SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 2 });
                            if (playerWeapon.Hash == WeaponHash.AssaultShotgun || playerWeapon.Hash == WeaponHash.SweeperShotgun || playerWeapon.Hash == WeaponHash.HeavyShotgun || playerWeapon.Hash == WeaponHash.BullpupShotgun)
                            {
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new() { 8, weaponStrength, fireRateAutomaticInt });
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
                                if(playerped.IsShooting)
                                {
                                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                                        new() { 8, weaponStrength, fireRateAutomaticInt });
                                }                                
                                else
                                {
                                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 1, 1 });

                                }
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

            if (_add2 == null)
            {
                _add2 = InstantiateScript<add>();
            }

            if (_obj == null)
            {
                _obj = new iO();
            }
            int bat = 0;
            Send(packet);
            Script.Wait(235);
            _obj.getstat(out bat, out bool _);
            if ((false) && showconmes)
            {
                GTA.UI.Notification.Show("controller is disconnected or discharged, please fix or press F11");
            }
            if (bat <= 15 && showbatstat)
            {
                GTA.UI.Notification.Show("Your controller battery is  " + bat + " to hide this message press F10", true);
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
                    _add2.rgbupdat2e(10, playerped.Health, out red, out blue);
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
                    _add2.rgbupdat2e(30, playerped.Health, out int _, out int _);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, false, false, false };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Two };
                    Send(packet);


                    wanted = true;
                    break;

                case 3:
                    _add2.rgbupdat2e(40, playerped.Health, out int _, out int _);
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


                    _add2.rgbupdat2e(50, playerped.Health, out int _, out int _);
                    wanted = true;
                    break;

                case 5:
                    _add2.rgbupdat2e(70, playerped.Health, out int _, out int _);
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