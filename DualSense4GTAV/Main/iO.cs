using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using DSX_Base;

namespace DualSense4GTAV
{
    public class iO
    {
        private static bool wanted;

        private static UdpClient client;

        private static IPEndPoint endPoint;

        private static Socket Socket;

        private static bool engine;

        private static ServicePoint ser;

        private bool playeralive;


        private int brig;

        private int batterylevel;

        private static void Connect()
        {
            _ = DateTime.Now;
            client = new UdpClient();
            string value = File.ReadAllText("C:\\Temp\\DualSenseX\\DualSenseX_PortNumber.txt");
            endPoint = new IPEndPoint(Triggers.localhost, Convert.ToInt32(value));
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private static void Send(Packet data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
            client.Send(bytes, bytes.Length, endPoint);
        }

        public void GetStat(out int bat, out bool isConnected)
        {
            Connect();
            bat = 0;
            isConnected = false;
            Packet data = new()
            {
                instructions = new Instruction[6]
            };
            Console.WriteLine("Instructions Sent\n");
            Send(data);
            Console.WriteLine("Waiting for Server Response...\n");
            if (Process.GetProcessesByName("DSX").Length == 0 && Main_GTAV.controllerConfig.showconmes) 
            {
                GTA.UI.Notification.Show("DSX is not running but mod is installed");
            }
            else
            {
                byte[] array = client.Receive(ref endPoint);
                if (array.Length != 0)
                {
                    ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(Encoding.ASCII.GetString(array, 0, array.Length) ?? "");
                    Console.WriteLine("===================================================================");
                    Console.WriteLine("Status: " + serverResponse.Status);
                    _ = DateTime.Now;
                    bat = serverResponse.BatteryLevel;
                    isConnected = serverResponse.isControllerConnected;
                    Console.WriteLine($"isControllerConnected: {serverResponse.isControllerConnected}");
                    Console.WriteLine($"BatteryLevel: {serverResponse.BatteryLevel}");
                    Console.WriteLine("===================================================================\n");
                }
                else
                {
                    GTA.UI.Notification.Show("DSX is not installed or UDP is off check the app settings ");
                }
            }
            Console.WriteLine("Press any key to send again\n");
        }
    }
}
