using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GTA;
using Shared;

namespace DualSense4GTAV
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

        private Main n = new Main();

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
            base.Tick += onTick;
        }

        private void onTick(object sender, EventArgs e)
        {
            Connect();
            if (Game.Player.Character.Health <= 150)
            {
                brig = 40;
            }
            if (Game.Player.Character.Health <= 100)
            {
                brig = 70;
            }
            if (Game.Player.Character.Health <= 40)
            {
                brig = 200;
                Script.Wait(1999);
            }
        }

        public void rgbupdat2e(int speed, int brightnes, out int red, out int blue)
        {
            blue = 255;
            red = 1;
            Connect();
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
                Send(packet);
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
