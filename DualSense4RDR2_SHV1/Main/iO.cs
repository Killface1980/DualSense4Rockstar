using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Shared;

namespace DualSense4RDR2
{
    public class iO
    {

        private static UdpClient client;

        private static IPEndPoint endPoint;

        private static Socket Socke;

        private static void Connect()
        {
            _ = DateTime.Now;
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


        public void getstat(out int bat, out bool isconnected)
        {
            Connect();
            bat = 0;
            isconnected = false;
            Packet data = new()
            {
                instructions = new Instruction[6]
            };
            Console.WriteLine("Instructions Sent\n");
            Send(data);
            Console.WriteLine("Waiting for Server Response...\n");
            if (Process.GetProcessesByName("DSX").Length == 0)
            {
                RDR2.UI.Screen.ShowSubtitle("DSX is not running but mod is installed");
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
                    isconnected = serverResponse.isControllerConnected;
                    Console.WriteLine($"isControllerConnected: {serverResponse.isControllerConnected}");
                    Console.WriteLine($"BatteryLevel: {serverResponse.BatteryLevel}");
                    Console.WriteLine("===================================================================\n");
                }
                else
                {
                    RDR2.UI.Screen.ShowSubtitle("DSX is not installed or UDP is off check the app settings ");
                }
            }
            Console.WriteLine("Press any key to send again\n");
        }
    }
}
