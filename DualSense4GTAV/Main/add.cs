using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GTA;
using DSX_Base;

namespace DualSense4GTAV
{
    [ScriptAttributes(NoDefaultInstance = true)]
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

        private static void Connect()
        {
            client = new UdpClient();
            string value = File.ReadAllText("C:\\Temp\\DualSenseX\\DualSenseX_PortNumber.txt");
            endPoint = new IPEndPoint(Triggers.localhost, Convert.ToInt32(value));
            Socke = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private static void Send(Packet data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
            client.Send(bytes, bytes.Length, endPoint);
        }

        public add()
        {
            Tick += this.onTick;
        }

        private void onTick(object sender, EventArgs e)
        {
            Connect();
            if (Game.Player.Character.Health <= 150)
            {
                this.brig = 40;
            }
            if (Game.Player.Character.Health <= 100)
            {
                this.brig = 70;
            }
            if (Game.Player.Character.Health <= 40)
            {
                this.brig = 200;
                Wait(1999);
            }
        }


        int red = 250;
        private int blue = 0;
        public void rgbupdat2e(int speed, int brightnes)
        {
            Packet packet = new();
            int num = 0;
            packet.instructions = new Instruction[4];
            while (blue <= 255)
            {
                packet.instructions[1].type = InstructionType.RGBUpdate;
                packet.instructions[1].parameters = new object[4]
                {
                    num,
                    red - this.brig,
                    0,
                    blue - this.brig
                };
                Send(packet);

                Wait(speed);
                red -= 50;
                blue += 50;
            }
            //Script.Wait(speed*50);

            while (red <= 255)
            {
                packet.instructions[1].type = InstructionType.RGBUpdate;
                packet.instructions[1].parameters = new object[4]
                {
                    num,
                    red - this.brig,
                    0,
                    blue - this.brig
                };
                Send(packet);

                Wait(speed);
                red+=50;
                blue-=50;

            }
        }
    }
}
