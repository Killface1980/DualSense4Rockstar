using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using RDR2;
using Shared;
using static DSX_Base.Client.iO;

namespace DualSense4RDR2
{
    public class add : Script
    {
        private static bool wanted;

        private static UdpClient client;

        private static IPEndPoint endPoint;

        private static Socket Socke;

        private static bool engine;

        private static ServicePoint ser;

        private bool playeralive;

        private int brig;

        private int batterylevel;

        public DateTime TimeSent { get; private set; }



        private static Ped characterPed;


        public add()
        {
            characterPed = Game.Player.Character;

            Tick += OnTick;
            Connect();
            Process.GetProcessesByName("DSX");

        }
        void updateLights()
        {
            // if (ScriptSettings::getBool("HealthIndication"))
            {
                health = RDR2.Native.ENTITY.GET_ENTITY_HEALTH(characterPed.Handle) / RDR2.Native.PED.GET_PED_MAX_HEALTH(characterPed.Handle);
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

        public int health = 0;
        private int staminaTarget = 0;
        public float pulseRate = 0;


        private void OnTick(object sender, EventArgs e)
        {
            updateLights();
            health = Math.Max(Math.Min(health, 1), 0);

            if (pulseRate > 0)
            {
                pulseRate = Math.Max(Math.Min(pulseRate, 1), 0.2f);

            }

            //RDR2.UI.Screen.ShowSubtitle(Game.Player.Character.Health.ToString());
            //return;
            if (health <= 1)
            {
                brig = 40;
            }
            if (health <= 0.66f)
            {
                brig = 70;
            }
            if (health <= 0.3f)
            {
                brig = 200;
                Script.Wait(1999);
            }
        }

        /*
         New stuff
                     health = Math.Max(Math.Min(health, 1), 0);

            if (pulseRate > 0)
            {
                pulseRate = Math.Max(Math.Min(pulseRate, 1), 0.2f);

            }
            if (health <= 1)
            {
                brig = 40;
            }
            if (health <= 0.66f)
            {
                brig = 70;
            }
            if (health <= 0.3f)
            {
                brig = 200;
                Script.Wait(1999);
            }

            void updateLights()
            {
                // if (ScriptSettings::getBool("HealthIndication"))
                {
                    health = RDR2.Native.ENTITY.GET_ENTITY_HEALTH(characterPed.Handle) / RDR2.Native.PED.GET_PED_MAX_HEALTH(characterPed.Handle);
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

         */

        public void rgbupdat2e(int speed, int brightnes, out int red, out int blue)
        {
            blue = 255;
            red = 1;
            DSX_Base.Client.iO.Connect();
            Packet packet = new();
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
                DSX_Base.Client.iO.Send(packet);
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
                Send(packet);
                Script.Wait(10);
                red -= speed;
                blue += speed;
            }
        }
    }
}
