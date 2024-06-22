using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DSX_Base;

namespace DSX_UDP_Example
{
  internal class Program
  {
    static UdpClient client;
    static IPEndPoint endPoint;

    static DateTime TimeSent;

    static void Connect()
    {
      client = new UdpClient();
      string portNumber = "6969"
      try
      {
          portNumber = File.ReadAllText(@"C:\Temp\DualSenseX\DualSenseX_PortNumber.txt");
          Console.WriteLine($"Port number found is: {portNumber}\n");
      }
            catch (Exception e)
      {
          Console.WriteLine("Using default UDP port 6969.");
      }
      endPoint = new IPEndPoint(Triggers.localhost, Convert.ToInt32(portNumber));
    }

    static void Send(Packet data)
    {
      byte[] RequestData = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
      client.Send(RequestData, RequestData.Length, endPoint);
      TimeSent = DateTime.Now;
    }

    static void Main(string[] args)
    {
      Connect();

      while (true)
      {
        Packet p = new();

        int controllerIndex = 0;

        // Set how many instructions you want to send at one time
        p.instructions = new Instruction[7];

        // ----------------------------------------------------------------------------------------------------------------------------

        /*                //Normal:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Normal};*/

        //GameCube:
        /*                p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.GameCube};*/

        /*                //VerySoft:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.VerySoft};

                        //Soft:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Soft};

                        //Hard:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Hard};

                        //VeryHard:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.VeryHard};

                        //Hardest:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Hardest};

                        //Rigid:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Rigid};

                        //VibrateTrigger needs 1 param of value from 0-255:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.VibrateTrigger, 10};

                        //Choppy:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Choppy};

                        //Medium:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Medium};

                        //VibrateTriggerPulse:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.VibrateTriggerPulse};

                        //CustomTriggerValue with CustomTriggerValueMode:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] {controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseAB, 0, 101, 255, 255, 0, 0, 0};

                        //Resistance needs 2 params Start: 0-9 Force:0-8:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Resistance, 0, 8};

                        //Bow needs 4 params Start: 0-8 End:0-8 Force:0-8 SnapForce:0-8:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Bow, 0, 8, 2, 5 };

                        //Galloping needs 5 params Start: 0-8 End:0-9 FirstFoot:0-6 SecondFoot:0-7 Frequency:0-255:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Galloping, 0, 9, 2, 4, 10 };

                        //SemiAutomaticGun needs 3 params Start: 2-7 End:0-8 Force:0-8:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.SemiAutomaticGun, 2, 7, 8};

                        //AutomaticGun needs 3 params Start: 0-8 End:0-9 StrengthA:0-7 StrengthB:0-7 Frequency:0-255 Period 0-2:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.AutomaticGun, 0, 8, 10 };

                        //Machine needs 6 params Start: 0-9 Strength:0-8 Frequency:0-255:
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Machine, 0, 9, 7, 7, 10, 0};*/

        // ----------------------------------------------------------------------------------------------------------------------------
        /*
                        p.instructions[0].type = InstructionType.TriggerUpdate;
                        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.VibrateTrigger, 10 };*/



        // Example Below:

        // ----------------------------------------------------------------------------------------------------------------------------

        // Adaptive Triggers:

        // *NOTE* If you're gonna be applying Adaptive triggers for both R2 And L2, you should send 2 UDP messages, one for L2 and one for R2

        // Left Trigger - Normal
        p.instructions[0].type = InstructionType.TriggerUpdate;
        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Normal };

        // Right Trigger - Very Soft
        p.instructions[0].type = InstructionType.TriggerUpdate;
        p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.VerySoft };

        // ----------------------------------------------------------------------------------------------------------------------------

        // TriggerThreshold needs 2 params LeftTrigger:0-255 RightTrigger:0-255
        // This is used for telling the emulation when to send the "pressed state"
        // Like the above, send it seperatly for each Trigger

        p.instructions[1].type = InstructionType.TriggerThreshold;
        p.instructions[1].parameters = new object[] { controllerIndex, Trigger.Left, 0 };

        p.instructions[1].type = InstructionType.TriggerThreshold;
        p.instructions[1].parameters = new object[] { controllerIndex, Trigger.Right, 0 };

        // ----------------------------------------------------------------------------------------------------------------------------

        // Touchpad LED

        p.instructions[2].type = InstructionType.RGBUpdate;
        p.instructions[2].parameters = new object[] { controllerIndex, 255, 255, 255 };

        // ----------------------------------------------------------------------------------------------------------------------------

        // *NOTE* If you're gonna be adjusting Player LED's, should apply for both, example below:

        // PLAYER LED 1-5 true/false state
        p.instructions[3].type = InstructionType.PlayerLED;
        p.instructions[3].parameters = new object[] { controllerIndex, false, false, false, false, false };

        // Player LED for new revision controllers
        p.instructions[4].type = InstructionType.PlayerLEDNewRevision;
        p.instructions[4].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Five };

        // Layout for new revision controller looks like this::
        //  - -x- -  Player 1
        //  - x-x -  Player 2
        //  x -x- x  Player 3
        //  x x-x x  Player 4
        //  x xxx x  Player 5

        // -----------------------------------------------------------------------------------------------------------------------------

        // Mic LED Three modes: On, Pulse, or Off
        p.instructions[5].type = InstructionType.MicLED;
        p.instructions[5].parameters = new object[] { controllerIndex, MicLEDMode.Pulse };

        // -----------------------------------------------------------------------------------------------------------------------------

        // Reset to user settings LED's Triggers etc..
        p.instructions[0].type = InstructionType.ResetToUserSettings;
        p.instructions[0].parameters = new object[] { controllerIndex };

        // -----------------------------------------------------------------------------------------------------------------------------


        Console.WriteLine("Instructions Sent\n");
        Send(p);

        Console.WriteLine("Waiting for Server Response...\n");

        // Make sure you setup some timeout for server response incase DSX has a bug or not running

        Process[] process = Process.GetProcessesByName("DSX");

        if (process.Length == 0)
        {
          Console.WriteLine("DSX is not running... \n");
        }
        else
        {
          byte[] bytesReceivedFromServer = client.Receive(ref endPoint);

          if (bytesReceivedFromServer.Length > 0)
          {
            ServerResponse ServerResponseJson = JsonConvert.DeserializeObject<ServerResponse>($"{Encoding.ASCII.GetString(bytesReceivedFromServer, 0, bytesReceivedFromServer.Length)}");
            Console.WriteLine("===================================================================");

            Console.WriteLine($"Status: {ServerResponseJson.Status}");
            DateTime CurrentTime = DateTime.Now;
            TimeSpan Timespan = CurrentTime - TimeSent;
            // First send shows high Milliseconds response time for some reason
            Console.WriteLine($"Time Received: {ServerResponseJson.TimeReceived}, took: {Timespan.TotalMilliseconds} to receive response from DSX");
            Console.WriteLine($"isControllerConnected: {ServerResponseJson.isControllerConnected}");
            Console.WriteLine($"BatteryLevel: {ServerResponseJson.BatteryLevel}");

            Console.WriteLine("===================================================================\n");
          }
        }




        Console.WriteLine("Press any key to send again\n");
        Console.ReadKey();
      }
    }
  }
}
