using RDR2;
using Shared;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using static DSX_Base.Client.Class1;
using static RDR2.Native.WEAPON;

namespace DualSense4RDR2
{
    public class Main : Script
    {
        private static Ped characterPed;
        private static bool engine;
        private static int lastBrakeFreq = 0;
        private static int lastBrakeResistance = 200;
        private static int lastThrottleResistance = 1;
        private static bool noammo;
        private static Weapon playerweapon;
        private static bool showbatstat = true;
        private static bool showconmes = true;
        private static bool wanted = false;
        private int health = 0;
        private int staminaTarget = 0;
        private float pulseRate = 0;
        private int brig;

        public Main()
        {
            characterPed = Game.Player.Character;
            base.Tick += OnTick;
            base.KeyDown += OnKeyDown;
            Connect();
            Process.GetProcessesByName("DSX");
        }
        public void rgbupdat2e(int speed, int brightnes, out int red, out int blue)
        {
            blue = 255;
            red = 1;
            DSX_Base.Client.Class1.Connect();
            Packet packet = new Packet();
            int num = 0;
            packet.instructions = new Instruction[4];
            while (red <= 255)
            {
                Script.Wait(10);
                red += speed;
                blue -= speed;
                packet.instructions[1].type = InstructionType.RGBUpdate;
                packet.instructions[1].parameters = new object[4]
                {
                    num,
                    blue - brig,
                    0,
                    red - brig
                };
                DSX_Base.Client.Class1.Send(packet);
            }
            while (blue <= 255)
            {
                packet.instructions[1].type = InstructionType.RGBUpdate;
                packet.instructions[1].parameters = new object[4]
                {
                    num,
                    blue - brig,
                    0,
                    red - brig
                };
                DSX_Base.Client.Class1.Send(packet);
                Script.Wait(10);
                red -= speed;
                blue += speed;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            return;
            Packet packet = new Packet();
            packet.instructions = new Instruction[4];
            if (e.KeyCode == Keys.F9)
            {
                iO obj = new iO();
                int bat = 0;
                bool isconnected = false;
                Send(packet);
                obj.getstat(out bat, out isconnected);
                RDR2.UI.Screen.DisplaySubtitle("Controller connection status: " + isconnected + " controller battery status: " + bat + "% \n to hide this Press F10");
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
        void updateLights()
        {
            // if (ScriptSettings::getBool("HealthIndication"))
            {
                health = characterPed.Health / characterPed.MaxHealth;
            }
            // else
            // {
            //     health = 1;
            // }

            // if (ScriptSettings::getBool("StaminaIndication"))
            {
                staminaTarget = characterPed.Handle;
                if (characterPed.IsOnMount)
                {
                    staminaTarget = RDR2.Native.PED.GET_MOUNT(characterPed.Handle);
                }

                float stamina = RDR2.Native.PED._GET_PED_STAMINA(staminaTarget) / RDR2.Native.PED._GET_PED_MAX_STAMINA(staminaTarget);
                pulseRate = 1 - stamina;
            }
            // else
            // {
            //     pulseRate = 0;
            // }
        }

        private unsafe void OnTick(object sender, EventArgs e)
        {
            Packet packet = new Packet();
            int controllerIndex = 0;
            packet.instructions = new Instruction[4];
            Player player = Game.Player;
            characterPed = player.Character;
            playerweapon = characterPed?.Weapons?.Current;

            // SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 1 });
            // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 5, 8, 8 });
            bool weaponIsReadyToShoot = IS_PED_WEAPON_READY_TO_SHOOT(characterPed.Handle);
            bool weaponIsAGun = IS_WEAPON_A_GUN((uint)playerweapon.Hash); //IS_WEAPON_A_GUN
            bool weaponIsThrowable = _IS_WEAPON_THROWABLE((uint)playerweapon.Hash); //

            //uint* numbi = null;
            //var mount = GET_CURRENT_PED_VEHICLE_WEAPON(characterPed.Handle, numbi); //

            bool hasMountedWeapon = characterPed?.Weapons?.Current?.Group == 0 && characterPed.IsSittingInVehicle();

            bool isMounted = hasMountedWeapon && characterPed?.CurrentVehicle != null;

            uint number = 0;

            bool currentPedVehicleWeapon = GET_CURRENT_PED_VEHICLE_WEAPON(characterPed.Handle, &number);

            // RDR2.UI.Screen.DisplaySubtitle(weaponIsAGun.ToString());

            if (isMounted)
            {
            }
            // return;
            bool playerIsAiming = player.IsAiming;

            //RDR2.UI.Screen.DisplaySubtitle(weaponIsAGun.ToString());
            // return;

            //RDR2.UI.Screen.DisplaySubtitle(playerweapon.Group.ToString());
            if (characterPed.IsReloading) // Mode reloading
            {
                SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                SetAndSendPacket(packet, controllerIndex, Trigger.Left);
            }
            else if (weaponIsThrowable)
            {
                SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 1, 20);

                SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue,
                    CustomTriggerValueMode.Pulse, 160, 30, 230);
            }
            else if (playerweapon.Group == eWeaponGroup.GROUP_BOW)
            {
                SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 1, 20);

                if (player.IsAiming)
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 2, 8 });
                }
                else
                {
                    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 1, 1 });
                }
            }
            else if (weaponIsAGun)
            {
                SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 1, 20);

                var degradation = GET_WEAPON_DEGRADATION(GET_CURRENT_PED_WEAPON_ENTITY_INDEX(characterPed.Handle, 0));

                // RDR2.UI.Screen.DisplaySubtitle(degradation.ToString());

                if (playerIsAiming && !weaponIsReadyToShoot) // Mode Gun Cock
                {
                    SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 1, 20 *
                        (int)(1 + degradation));

                    // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 4, 1 + (int)(degradation * 3), 2 });
                }
                else // GUN_MANUAL
                {
                    SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 1, 90 *
                        (int)(1 + degradation));

                    //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 0, 4,1+(int)(degradation * 7), 4 });
                }
            }
            else if (currentPedVehicleWeapon)
            {
                if (number == 3666182381 || //gat
                 number == 3101324918)// maxi
                {
                    SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 1, 20);
                    if (characterPed.IsShooting) // Auto
                    {
                        SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue,
                            CustomTriggerValueMode.PulseB, 9, 190);
                    }
                    else // Prepare
                    {
                        SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue,
                            CustomTriggerValueMode.Rigid, 30, 255);
                    }
                }
                else if (number == 2465730487 || //hotch - cannons
                         number == 1609145491)// breach
                {
                    SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 1, 20);
                    SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue,
                        CustomTriggerValueMode.PulseA, 255, 200, 255);
                }
            }
            else // turn off
            {
                SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                SetAndSendPacket(packet, controllerIndex, Trigger.Left);
            }
            
            // updateLights();
            // health = Math.Min(health, 1);
            health = (int)((float)characterPed.Health / (float)characterPed.MaxHealth * 255f);
            int myblue = (health);
            int myred = ((255 - health));
            packet.instructions = new Instruction[4];
            packet.instructions[1].type = InstructionType.RGBUpdate;
            packet.instructions[1].parameters = new object[4]
            {
                0,
                myred, // red
                myblue, // green
                0 // blue
            };
            Send(packet);
            RDR2.UI.Screen.DisplaySubtitle((myred).ToString());
            return;

            //else
            return;


            //RDR2.UI.Screen.DisplaySubtitle(health + " - " + pulseRate + " - " + staminaTarget);
            add add2 = new add();
            iO obj = new iO();
            int bat = 0;
            Send(packet);
            Script.Wait(235);
            obj.getstat(out bat, out bool _);


            if ((false) && showconmes)
            {
                RDR2.UI.Screen.DisplaySubtitle("controller is disconnected or discharged, please fix or press F11");
            }
            if (bat <= 15 && showbatstat)
            {
                RDR2.UI.Screen.DisplaySubtitle("Your controller battery is  " + bat + " to hide this message press F10");
                // RDR2.UI.Screen.ShowHelpMessage("Your controller battery is  " + bat + " to hide this message press F10", 1, sound: false);
            }

            switch (player.WantedLevel)
            {
                case 0:
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, false, false, false, false, false };
                    Send(packet);
                    Script.Wait(299);
                    Script.Wait(299);
                    wanted = false;
                    break;

                case 1:
                    {
                        int blue = 0;
                        int red = 0;
                        add2.rgbupdat2e(10, characterPed.Health, out red, out blue);
                        wanted = true;
                        packet.instructions[2].type = InstructionType.PlayerLED;
                        packet.instructions[2].parameters = new object[6] { controllerIndex, true, false, false, false, false };
                        Send(packet);
                        packet.instructions[1].type = InstructionType.RGBUpdate;
                        packet.instructions[1].parameters = new object[4] { controllerIndex, blue, 0, red };
                        Send(packet);
                        break;
                    }
                case 2:
                    add2.rgbupdat2e(30, characterPed.Health, out int _, out int _);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, false, false, false };
                    Send(packet);
                    wanted = true;
                    break;

                case 3:
                    add2.rgbupdat2e(40, characterPed.Health, out int _, out int _);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, false, false };
                    Send(packet);
                    wanted = true;
                    break;

                case 4:
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, true, false };
                    Send(packet);
                    add2.rgbupdat2e(50, characterPed.Health, out int _, out int _);
                    wanted = true;
                    break;

                case 5:
                    add2.rgbupdat2e(70, characterPed.Health, out int _, out int _);
                    packet.instructions[2].type = InstructionType.PlayerLED;
                    packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, true, true };
                    Send(packet);
                    wanted = true;
                    break;
            }
        }
    }
}