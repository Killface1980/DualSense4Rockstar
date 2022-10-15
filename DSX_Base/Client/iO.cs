using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DSX_Base.Client
{
    public class iO
    {
        private static UdpClient client;

        private static IPEndPoint endPoint;

        public static void Connect()
        {
            _ = DateTime.Now;
            client = new UdpClient();
            string value = File.ReadAllText("C:\\Temp\\DualSenseX\\DualSenseX_PortNumber.txt");
            endPoint = new IPEndPoint(Triggers.localhost, Convert.ToInt32(value));
            new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public static void Send(Packet data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
            client.Send(bytes, bytes.Length, endPoint);
        }

        static int controllerIndex = 0;

    public static void SetAndSendPacket(Packet packet, Trigger trigger,
          TriggerMode triggerMode = TriggerMode.Normal, List<int> parameters = null)
        {
            packet.instructions[0].type = InstructionType.TriggerUpdate;
            List<object> newList = new()
            {
                controllerIndex,
                trigger,
                triggerMode
            };
            if (parameters != null)
            {
                foreach (object param in parameters)
                {
                    newList.Add(param);
                }
            }
            packet.instructions[0].parameters = newList.ToArray();

            Send(packet);
        }
    public static void SetAndSendPacketAutomaticGun(Trigger trigger,
          int start, int strength, int frequency)
    {
      Packet packet = new();
      int controllerIndex = 0;
      packet.instructions = new Instruction[4];

      TriggerMode triggerMode = TriggerMode.AutomaticGun;
            packet.instructions[0].type = InstructionType.TriggerUpdate;
            List<object> newList = new()
            {
                controllerIndex,
                trigger,
                triggerMode,
                start,
                strength,
                frequency
            };
            packet.instructions[0].parameters = newList.ToArray();

            Send(packet);
        }
    public static void SetAndSendPacketResistance(Trigger trigger,
          int start, int force)
    {
      Packet packet = new();
      int controllerIndex = 0;
      packet.instructions = new Instruction[4];

      TriggerMode triggerMode = TriggerMode.Resistance;
            packet.instructions[0].type = InstructionType.TriggerUpdate;
            List<object> newList = new()
            {
                controllerIndex,
                trigger,
                triggerMode,
                start,
                force
            };

            packet.instructions[0].parameters = newList.ToArray();

            Send(packet);
        }



        public static void SetAndSendPacketCustom(Packet packet, Trigger trigger,
          CustomTriggerValueMode customMode = CustomTriggerValueMode.OFF, int startOfResistance = 0,
          int amountOfForceExerted = 0, int forceExertedInRange = 0, int ab_strengthNearRelease = 0,
          int ab_strengthNearMiddle = 0, int ab_strengthPressedState = 0, int ab_actuationFrequency = 0)
        {
            TriggerMode triggerMode = TriggerMode.CustomTriggerValue;

            packet.instructions[0].type = InstructionType.TriggerUpdate;
            List<object> newList = new()
            {
                controllerIndex,
                trigger,
                triggerMode,
                customMode,
                startOfResistance, amountOfForceExerted, forceExertedInRange, ab_strengthNearRelease,
                ab_strengthNearMiddle, ab_strengthPressedState, ab_actuationFrequency
            };

            packet.instructions[0].parameters = newList.ToArray();

            Send(packet);
        }

    }
}
