﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;

namespace DSX_Base.Client
{
    public static class Class1
    {
        private static UdpClient client;
        private static IPEndPoint endPoint;
        private static DateTime TimeSent;

        public static void Connect()
        {
            client = new UdpClient();
            string value = File.ReadAllText("C:\\Temp\\DualSenseX\\DualSenseX_PortNumber.txt");
            endPoint = new IPEndPoint(Triggers.localhost, Convert.ToInt32(value));
            new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public static void Send(Packet data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
            client.Send(bytes, bytes.Length, endPoint);
            TimeSent = DateTime.Now;
        }


        public static void SetAndSendPacket(Packet packet, int controllerIndex, Trigger trigger, TriggerMode triggerMode = TriggerMode.Normal, List<int> parameters = null)
        {
            packet.instructions[0].type = InstructionType.TriggerUpdate;
            List<object> newList = new List<object>()
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
        public static void SetAndSendPacketCustom(Packet packet, int controllerIndex, Trigger trigger, TriggerMode triggerMode = TriggerMode.CustomTriggerValue, CustomTriggerValueMode customMode = CustomTriggerValueMode.OFF, int startOfResistance = 0, int amountOfForceExerted = 0, int forceExertedInRange = 0, int ab_strengthNearRelease = 0, int ab_strengthNearMiddle = 0, int ab_strengthPressedState = 0, int ab_actuationFrequency = 0)
        {

            packet.instructions[0].type = InstructionType.TriggerUpdate;
            List<object> newList = new List<object>()
            {
                controllerIndex,
                trigger,
                triggerMode,
                customMode,
                startOfResistance,amountOfForceExerted,forceExertedInRange,ab_strengthNearRelease,ab_strengthNearMiddle,ab_strengthPressedState,ab_actuationFrequency
            };

            packet.instructions[0].parameters = newList.ToArray();

            Send(packet);
        }

    }
}
