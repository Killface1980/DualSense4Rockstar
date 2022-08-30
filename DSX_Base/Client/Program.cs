using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;

namespace Client
{
    internal class Program
    {
        private static UdpClient client;

        private static IPEndPoint endPoint;

        private static void Connect()
        {
            client = new UdpClient();
            string value = File.ReadAllText("C:\\Temp\\DualSenseX\\DualSenseX_PortNumber.txt");
            endPoint = new IPEndPoint(Triggers.localhost, Convert.ToInt32(value));
        }

        private static void Send(Packet data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
            client.Send(bytes, bytes.Length, endPoint);
        }

        private static void Main(string[] args)
        {
            Connect();
            while (true)
            {
                Packet packet = new();
                int num = 0;
                packet.instructions = new Instruction[4];
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Normal
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.GameCube
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.VerySoft
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Soft
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Hard
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.VeryHard
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Hardest
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Rigid
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[4]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.VibrateTrigger,
                    10
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Choppy
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Medium
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[3]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.VibrateTriggerPulse
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[11]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.CustomTriggerValue,
                    CustomTriggerValueMode.PulseAB,
                    0,
                    101,
                    255,
                    255,
                    0,
                    0,
                    0
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[5]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Resistance,
                    0,
                    8
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[7]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Bow,
                    0,
                    8,
                    2,
                    5
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[8]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Galloping,
                    0,
                    9,
                    2,
                    4,
                    10
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[6]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.SemiAutomaticGun,
                    2,
                    7,
                    8
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[6]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.AutomaticGun,
                    0,
                    8,
                    10
                };
                packet.instructions[0].type = InstructionType.TriggerUpdate;
                packet.instructions[0].parameters = new object[9]
                {
                    num,
                    Trigger.Left,
                    TriggerMode.Machine,
                    0,
                    9,
                    7,
                    7,
                    10,
                    0
                };
                packet.instructions[1].type = InstructionType.RGBUpdate;
                packet.instructions[1].parameters = new object[4] { num, 0, 255, 0 };
                packet.instructions[2].type = InstructionType.PlayerLED;
                packet.instructions[2].parameters = new object[6] { num, true, false, true, false, true };
                packet.instructions[3].type = InstructionType.TriggerThreshold;
                packet.instructions[3].parameters = new object[3]
                {
                    num,
                    Trigger.Right,
                    0
                };
                Send(packet);
                Console.WriteLine("Press any key to send again");
                Console.ReadKey();
            }
        }
    }
}
