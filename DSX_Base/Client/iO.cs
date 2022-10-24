using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DSX_Base.Client;

public class iO
{
  private static UdpClient client;

  private static int controllerIndex = 0;
  private static IPEndPoint endPoint;

  public static void Connect()
  {
    _ = DateTime.Now;
    client = new UdpClient();
    string value = File.ReadAllText("C:\\Temp\\DualSenseX\\DualSenseX_PortNumber.txt");
    endPoint = new IPEndPoint(Triggers.localhost, Convert.ToInt32(value));
    new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="trigger"></param>
  /// <param name="start">0-8</param>
  /// <param name="end">0-8, > start</param>
  /// <param name="force">0-8</param>
  /// <param name="snapForce">0-8</param>
  public static void DoTrigger_Bow(Trigger trigger, int start, int end, int force, int snapForce)
  {
    Packet packet = new()
    {
      instructions = new Instruction[4]
    };
    packet.instructions[0].type = InstructionType.TriggerUpdate;
    packet.instructions[0].parameters = new object[]
    {
      controllerIndex,
      trigger,
      TriggerMode.Bow,
      start,
      end,
      force,
      snapForce
    };

    Send(packet);
  }

  public static void DoTrigger_BowThreshold(Trigger trigger, int start, int end, int force, int snapForce, int threshold)
  {
    Packet p = new()
    {
      instructions = new Instruction[4]
    };
    p.instructions[0].type = InstructionType.TriggerUpdate;
    p.instructions[0].parameters = new object[]
    {
      controllerIndex,
      trigger,
      TriggerMode.Bow,
      start,
      end,
      force,
      snapForce
    };
    p.instructions[1].type = InstructionType.TriggerThreshold;
    p.instructions[1].parameters = new object[] { controllerIndex, trigger, threshold };

    Send(p);
  }

  public static void DoTrigger_CustomRigid(Trigger trigger, int startOfResistance, int amountOfForceExcerted,
    int forceExcertedInRange) // custom trigger values need ALL their parameters
  {
    Packet packet = new()
    {
      instructions = new Instruction[4]
    };

    packet.instructions[0].type = InstructionType.TriggerUpdate;
    packet.instructions[0].parameters = new object[]
    {
      controllerIndex,
      trigger,
      TriggerMode.CustomTriggerValue,
      CustomTriggerValueMode.Rigid,
      startOfResistance,
      amountOfForceExcerted,
      forceExcertedInRange,
      0, 0, 0, 0
    };
/*    if (trigger == Trigger.Right)
    {
      packet.instructions[2].type = InstructionType.RGBUpdate;
      packet.instructions[2].parameters = new object[] { controllerIndex, 255, 255, 255 };
    }
*/    Send(packet);
  }

  /// <summary>
  /// AutomaticGun needs 3 params
  /// </summary>
  /// <param name="trigger"></param>
  /// <param name="start">Start: 0-8</param>
  /// <param name="strength">End:0-8</param>
  /// <param name="frequency">Max 40 to avoid damage</param>
  public static void DoTrigger_AutomaticGun(Trigger trigger, int start, int strength, int frequency)
  {
    Packet packet = new()
    {
      instructions = new Instruction[4]
    };

    packet.instructions[0].type = InstructionType.TriggerUpdate;
    packet.instructions[0].parameters = new object[]
    {
      controllerIndex,
      trigger,
      TriggerMode.AutomaticGun,
      start,
      strength,
      frequency
    };

    Send(packet);
  }

  public static void DoTrigger_Machine(Trigger trigger, int start, int end, int strengthA, int strengthB, int frequency,
    int period)
  {
    Packet p = new()
    {
      instructions = new Instruction[4]
    };
    p.instructions[0].type = InstructionType.TriggerUpdate;
    p.instructions[0].parameters = new object[]
    {
      controllerIndex,
      trigger,
      TriggerMode.Machine,
      start,
      end,
      strengthA,
      strengthB,
      frequency,
      period
    };

    Send(p);
  }

  /// <summary>
  /// Resistance needs 2 params
  /// </summary>
  /// <param name="trigger"></param>
  /// <param name="start">Start: 0-9</param>
  /// <param name="force">Force:0-8</param>
  public static void DoTrigger_Resistance(Trigger trigger, int start, int force)
  {
    Packet p = new()
    {
      instructions = new Instruction[4]
    };

    p.instructions[0].type = InstructionType.TriggerUpdate;

    p.instructions[0].parameters = new object[]
    {
      controllerIndex,
      trigger,
      TriggerMode.Resistance,
      start,
      force
    };

    Send(p);
  }

  public static void Send(Packet data)
  {
    byte[] bytes = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
    client.Send(bytes, bytes.Length, endPoint);
  }

  public static void SetAndSendPacket(Trigger trigger,
    TriggerMode triggerMode = TriggerMode.Normal, List<int> parameters = null)
  {
    Packet p = new();
    p.instructions = new Instruction[4];


    p.instructions[0].type = InstructionType.TriggerUpdate;
    List<object> newList = new()
    {
      controllerIndex,
      trigger,
      triggerMode
    };
    if (parameters != null)
      foreach (object param in parameters)
        newList.Add(param);
    p.instructions[0].parameters = newList.ToArray();



    Send(p);
  }

  public static void SetAndSendPacketCustom(Packet p, Trigger trigger,
    CustomTriggerValueMode customMode = CustomTriggerValueMode.OFF, int startOfResistance = 0,
    int amountOfForceExerted = 0, int forceExertedInRange = 0, int ab_strengthNearRelease = 0,
    int ab_strengthNearMiddle = 0, int ab_strengthPressedState = 0, int ab_actuationFrequency = 0)
  {
    TriggerMode triggerMode = TriggerMode.CustomTriggerValue;

    p.instructions[0].type = InstructionType.TriggerUpdate;
    p.instructions[0].parameters = new object[]
    {
      controllerIndex,
      trigger,
      triggerMode,
      customMode,
      startOfResistance, amountOfForceExerted, forceExertedInRange, ab_strengthNearRelease,
      ab_strengthNearMiddle, ab_strengthPressedState, ab_actuationFrequency
    };

    Send(p);
  }
}