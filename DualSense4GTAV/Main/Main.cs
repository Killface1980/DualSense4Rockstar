using GTA;
using GTA.Native;
using Shared;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
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

        private static bool showbatstat = true;
        private static bool showconmes = true;
        private static DateTime TimeSent;
        private add _add2 = null;
        private iO _obj = null;

        public Main()
        {
            base.Tick += this.onTick;
            base.KeyDown += this.onKeyDown;
            Connect();
            Process.GetProcessesByName("DSX");
        }

        public static bool Wanted = false;

        float Lerp(float a, float b, float t)
        {
            //return firstFloat * by + secondFloat * (1 - by);
            return (1f - t) * a + b * t;
        }

        float InvLerp(float a, float b, float v)
        {
            return (v - a) / (b - a);
        }
        float InvLerpCapped(float a, float b, float v)
        {
            return Math.Max(0,Math.Min(1, (v - a) / (b - a)));
        }

        float Remap(float iMin, float iMax, float oMin, float oMax, float v)
        {
            float t = InvLerp(iMin, iMax, v);
            return Lerp(oMin, oMax, t);
        }

        int LerpInt(float a, float b, float t)
        {
            //return firstFloat * by + secondFloat * (1 - by);
            return (int)((1f - t) * a + t * b);
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            Packet packet = new();
            packet.instructions = new Instruction[4];
            if (e.KeyCode == Keys.F9)
            {
                iO obj = new();
                Send(packet);
                obj.getstat(out int bat, out bool isconnected);
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
            Ped playerped = Game.Player.Character;

            Weapon playerWeapon = playerped.Weapons.Current;
            if (Function.Call<bool>(GTA.Native.Hash.IS_HUD_COMPONENT_ACTIVE, 19)) //HUD_WEAPON_WHEEL
            {
                SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                SetAndSendPacket(packet, controllerIndex, Trigger.Left);

            }
            else if (Game .IsPaused)
            {
                SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                SetAndSendPacket(packet, controllerIndex, Trigger.Left);

            }
            else if (playerped.IsInVehicle() || playerped.IsOnBike || playerped.IsInBoat || playerped.IsInHeli)
            {
                Vehicle currentVehicle = playerped.CurrentVehicle;

                if (!currentVehicle.IsEngineRunning)
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Rigid);
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Rigid);
                }
                else if (playerped.IsInHeli)
                {
                    float health = (currentVehicle.HeliEngineHealth/1000f* currentVehicle.HeliMainRotorHealth / 1000f * currentVehicle.HeliTailRotorHealth / 1000f);
                    float healthMalus = (int)((1f - health) * 4f);

                    //GTA.Native.Hash.traction

                    //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { (int)(6f -
                    //    healthMalus), Math.Min(resistance + (int)healthMalus, 8) });
                     //GTA.UI.Screen.ShowSubtitle(LerpInt(0, 6, health) + "-"+ LerpInt(8, 1, health) + "-" + health);

                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
                    {
                        1 + LerpInt(0,6,health),
                        LerpInt(8,1,health)
                    });
                    
                    
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
                    {
                        1 + LerpInt(0, 6, health),
                        LerpInt(8, 1, health)
                    });

                }
                else if (playerped.IsInPlane)
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 5, 1 });
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 5, 1 });
                }
                else if (currentVehicle.IsInAir  || !currentVehicle.IsOnAllWheels)
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left);
                }
                else if (currentVehicle.EngineHealth <= 0f)
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Hardest);
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Hardest);
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
                else if (currentVehicle.Wheels != null && currentVehicle.Wheels.Any(x => x.IsBursted) && (
                             (!playerped.IsInBoat && !playerped.IsInHeli) ||
                             (playerped.IsInPlane && !currentVehicle.IsInAir)))
                {

                    int resistance = 4 ;

                    SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right,
                        CustomTriggerValueMode.VibrateResistanceAB,
                        127, 255, 144, 60, 120, 220, (int)currentVehicle.Speed);
                    SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left,
                        CustomTriggerValueMode.VibrateResistanceAB,
                        127, 255, 144, 60, 120, 220, (int)currentVehicle.Speed);
                }
                else // if (playerped.CurrentVehicle.EngineHealth >= 1000f)
                {
                    float health =  (currentVehicle.EngineHealth / 1000f);
                    float healthMalus = (int)((1f - health) * 4f);
                    int currentGear = currentVehicle.CurrentGear;
                    int maxGear = currentVehicle.HighGear;
                    float currentRpm = currentVehicle.CurrentRPM;
                    float currentSpeed = currentVehicle.Speed;
                    float maxSpeed = Function.Call<float>(GTA.Native.Hash.GET_VEHICLE_ESTIMATED_MAX_SPEED, currentVehicle.Handle);

                    float currentSurfingSpeed = Math.Min(1, currentSpeed/maxSpeed);

                    int resistance = 4 - currentGear;
                    //GTA.Native.Hash.traction

                    //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { (int)(6f -
                    //    healthMalus), Math.Min(resistance + (int)healthMalus, 8) });
                    int forceR = Math.Max(1, Math.Min(resistance + (int)healthMalus, 8));
                    // GTA.UI.Screen.ShowSubtitle(forceR.ToString());

                    float initialDriveForce = InvLerpCapped(0, 0.4f, currentVehicle.HandlingData.InitialDriveForce); // capped at 0.4f
                    float driveInertia = InvLerpCapped(0.3f, 1.0f, currentVehicle.HandlingData.DriveInertia) *
                                         InvLerpCapped(3500, 1200, currentVehicle.HandlingData.Mass);
                    float gearForce = InvLerpCapped(currentVehicle.HighGear, 1, currentGear);


                    float spinnie = 1f;
                    if (currentVehicle.Speed > 0)
                    {
                        spinnie = currentVehicle.WheelSpeed / currentVehicle.Speed;
                    }

                    int startOfResistance = (int)Lerp(2, 7, driveInertia);

                    startOfResistance -= (int)Lerp(8, 0, health);

                    startOfResistance -= (int)((1f - spinnie) * 4f);

                    startOfResistance = Math.Max(1, (Math.Min(7, startOfResistance)));

                    int amountOfForceExerted = LerpInt(3, 1, initialDriveForce); //max 5 force for slow vehicles

                    amountOfForceExerted += (int)(Lerp(0,3,gearForce)); // additional force for low gear
                    amountOfForceExerted += (int)((1f - spinnie) * 6f);

                    amountOfForceExerted = Math.Max(1, (Math.Min(8, amountOfForceExerted)));
                    float brakeForce = InvLerpCapped(0.2f, 1.2f, currentVehicle.HandlingData.BrakeForce);
                    int startOfResistanceBrake = (int)Lerp(1, 6, brakeForce);
                    int amountOfForceExertedBrake = (int)Lerp(8, 1, gearForce * InvLerpCapped(500, 5000, currentVehicle.HandlingData.Mass));


                    if (currentGear >= 1)
                    {
                        // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
                        // {
                        //     1+(int)(currentRpm*health * 7 ),
                        //     forceR
                        // });


                        // SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
                        // {
                        //     6 -
                        //     (currentGear) -
                        //     (int)(healthMalus / 2f),
                        //     8 - resistance
                        // });
                        SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
                        {
                            startOfResistance,
                            amountOfForceExerted,
                        });

                        SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
                        {
                            startOfResistanceBrake,
                            amountOfForceExertedBrake,
                        });


                        //              BrakeForce / DriveInertia / Mass / InitialDriveForce
                        // Betonmisch:  0,3        / 0,5          / 6000 / 0,11 
                        // Barracks:    0,3        / 0,5          / 9000 / 0,11 
                        // Mule:        0,25       / 1            / 5500 / 0,11
                        // Coach:       0,25       / 0,5          / 8500 / 0,12
                        // Ambulance:   0,6        / 1            / 2500 / 0,18 
                        // Tampa:       0,8        / 1            / 1200 / 0,27
                        // Obey Sports: 1          / 1            / 1300 / 0,33 
                        // Banshee:     1          / 1            / 1200 / 0,34
                    }
                    else
                    {
                        // SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
                        // {
                        //     (int)(currentRpm*health * 7),
                        //     forceR
                        // });
                        // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
                        // {
                        //     7-(int)(currentRpm*health * 7),
                        //     forceR
                        // });

                        SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
                        {
                            startOfResistance,
                            amountOfForceExerted,
                        });

                        SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
                        {
                            startOfResistanceBrake,
                            amountOfForceExertedBrake,
                        });


                    }
                    // GTA.UI.Screen.ShowSubtitle(startOfResistance + " - " + amountOfForceExerted + " / " + startOfResistanceBrake + " - " + amountOfForceExertedBrake);

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

                _ = Wanted;
            }
            else
            {
                _ = Wanted;

                if (playerWeapon != null && (playerped.IsReloading || playerWeapon.AmmoInClip == 0))
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 5, 6, 4, 8 });
                    SetAndSendPacket(packet, controllerIndex, Trigger.Left);
                }
                else
                {
                    if (playerWeapon != null)
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
                                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                                    new() { 1, 1 });
                                if (playerWeapon.Hash == WeaponHash.APPistol)
                                {
                                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                                        new() { 8, weaponStrength, fireRateAutomaticInt });
                                }
                                else
                                {
                                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow,
                                        new() { 1, 6, 4, 8 });
                                }

                                //UI.ShowSubtitle("Doing great, aiming widda pistal");
                                break;

                            case WeaponGroup.SMG:
                                // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                                // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.VibrateTrigger, new() { 40 });
                                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                                    new() { 1, 2 });
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Machine,
                                    new() { 7, 9, 4, 4, fireRateAutomaticInt, 0 });
                                break;

                            case WeaponGroup.AssaultRifle:
                                // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                                // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.AutomaticGun, new() { 2, 7, 8 });
                                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                                    new() { 1, 2 });
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                                    new() { 6, weaponStrength, fireRateAutomaticInt });

                                break;

                            case WeaponGroup.MG:
                                // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                                // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.VibrateTrigger, new() { 40 });
                                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                                    new() { 1, 2 }); //1,3
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                                    new() { 8, weaponStrength, fireRateAutomaticInt });
                                break;

                            case WeaponGroup.Shotgun:

                                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                                    new() { 1, 2 });
                                if (playerWeapon.Hash == WeaponHash.AssaultShotgun ||
                                    playerWeapon.Hash == WeaponHash.SweeperShotgun ||
                                    playerWeapon.Hash == WeaponHash.HeavyShotgun ||
                                    playerWeapon.Hash == WeaponHash.BullpupShotgun)
                                {
                                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                                        new() { 8, weaponStrength, fireRateAutomaticInt });
                                }
                                else
                                {
                                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow,
                                        new() { 2, 4, 4, 4 });
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
                                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                                    new() { 1, 2 });
                                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.SemiAutomaticGun,
                                    new() { 2, 7, 4 });
                                //UI.ShowSubtitle("Sniper");

                                break;

                            case WeaponGroup.Heavy:
                                if (playerWeapon.Hash == WeaponHash.Minigun)
                                {
                                    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                                        new() { 1, 2 });
                                    //SetAndSendPacket(packet, controllerIndex, Trigger.Right,  TriggerMode.VibrateTrigger, new() { 39 });

                                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                                        new() { 8, weaponStrength, fireRateAutomaticInt });


                                }
                                else
                                {
                                    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                                        new() { 1, 2 });
                                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow,
                                        new() { 1, 6, 4, 8 });
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
            }

            
            this._add2 ??= InstantiateScript<add>();

            this._obj ??= new iO();

            Script.Wait(235);
            this._obj.getstat(out int bat, out bool _);
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
                    if (playerped.CurrentVehicle != null && playerped.CurrentVehicle.IsSirenActive)
                    {
                        this._add2.rgbupdat2e(10, playerped.Health);
                        Wanted = true;

                    }
                    else
                    {
                        Wanted = false;

                    }

                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, false, false, false, false, false };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.AllOff };
                    Send(packet);

                    Script.Wait(299);
                    Script.Wait(299);
                    break;

                case 1:
                    {
                        this._add2.rgbupdat2e(10, playerped.Health);
                        Wanted = true;
                        packet.instructions[2].type = InstructionType.PlayerLED;
                        packet.instructions[2].parameters = new object[6] { controllerIndex, true, false, false, false, false };
                        Send(packet);

                        packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                        packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.One };
                        Send(packet);

                        break;
                    }
                case 2:
                    this._add2.rgbupdat2e(8, playerped.Health);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, false, false, false };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Two };
                    Send(packet);

                    Wanted = true;
                    break;

                case 3:
                    this._add2.rgbupdat2e(6, playerped.Health);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, false, false };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Three };
                    Send(packet);

                    Wanted = true;
                    break;

                case 4:
                    this._add2.rgbupdat2e(4, playerped.Health);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, true, false };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Four };
                    Send(packet);

                    Wanted = true;
                    break;

                case 5:
                    this._add2.rgbupdat2e(2, playerped.Health);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, true, true };
                    Send(packet);

                    packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
                    packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Five };
                    Send(packet);

                    Wanted = true;
                    break;
            }

            if (!Wanted) 
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

        }
    }
}