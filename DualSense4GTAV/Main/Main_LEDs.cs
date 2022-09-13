using GTA;
using Shared;
using System;
using static DSX_Base.Client.iO;
using static DualSense4GTAV.Main;

namespace DualSense4GTAV.Main_LEDs
{
  public class Main_LEDs : Script
  {
    public static bool Wanted = false;
    private static float currentIdleRPM = 0.2f;
    private static bool willShift = false;
    private add _add2 = null;

    private iO _obj = null;


    public Main_LEDs()
    {
      Tick += this.OnTick;

      // Connect();
      // Process.GetProcessesByName("DSX");
      // Interval = 50;
    }

    private void OnTick(object sender, EventArgs e)
    {
      Packet packet = new();
      int controllerIndex = 0;
      packet.instructions = new Instruction[4];

      this._add2 ??= InstantiateScript<add>();

      this._obj ??= new iO();

      this._obj.getstat(out int bat, out bool isConnected);
      if (!isConnected && controllerConfig.showconmes)
      {
        GTA.UI.Notification.Show("Your controller is disconnected or discharged, please fix or press " + nameof(KeyConf
          .showCommStat) );
        return;
      }
      if (bat <= 15 && controllerConfig.showbatstat)
      {
        GTA.UI.Notification.Show("Your controller battery is  " + bat + " to hide this message press "+nameof(KeyConf
          .hideBatStat), true);
        //return;
      }
      Ped playerped = Game.Player.Character;

      Vehicle currentVehicle = playerped.CurrentVehicle;
      if (controllerConfig.showWanted)
      {
        switch (Game.Player.WantedLevel)
        {
          case 0:
            if (currentVehicle != null && currentVehicle.IsSirenActive)
            {
              this._add2.rgbupdat2e(40, playerped.Health);
              Wanted = true;
            }
            else
            {
              Wanted = false;
            }

            packet.instructions[2].type = InstructionType.PlayerLED;
            packet.instructions[2].parameters = new object[6]
                { controllerIndex, false, false, false, false, false };
            Send(packet);

            packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
            packet.instructions[2].parameters = new object[]
                { controllerIndex, PlayerLEDNewRevision.AllOff };
            Send(packet);

            break;

          case 1:
            {
              this._add2.rgbupdat2e(40, playerped.Health);
              Wanted = true;
              packet.instructions[2].type = InstructionType.PlayerLED;
              packet.instructions[2].parameters = new object[6]
                  { controllerIndex, true, false, false, false, false };
              Send(packet);

              packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
              packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.One };
              Send(packet);

              break;
            }
          case 2:
            this._add2.rgbupdat2e(30, playerped.Health);
            packet.instructions[2].type = InstructionType.PlayerLED;
            packet.instructions[2].parameters = new object[6]
                { controllerIndex, true, true, false, false, false };
            Send(packet);

            packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
            packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Two };
            Send(packet);

            Wanted = true;
            break;

          case 3:
            this._add2.rgbupdat2e(20, playerped.Health);
            packet.instructions[2].type = InstructionType.PlayerLED;
            packet.instructions[2].parameters = new object[6]
                { controllerIndex, true, true, true, false, false };
            Send(packet);

            packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
            packet.instructions[2].parameters = new object[]
                { controllerIndex, PlayerLEDNewRevision.Three };
            Send(packet);

            Wanted = true;
            break;

          case 4:
            this._add2.rgbupdat2e(10, playerped.Health);
            packet.instructions[2].type = InstructionType.PlayerLED;
            packet.instructions[2].parameters = new object[6]
                { controllerIndex, true, true, true, true, false };
            Send(packet);

            packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
            packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Four };
            Send(packet);

            Wanted = true;
            break;

          case 5:
            this._add2.rgbupdat2e(5, playerped.Health);
            packet.instructions[2].type = InstructionType.PlayerLED;
            packet.instructions[2].parameters = new object[6]
                { controllerIndex, true, true, true, true, true };
            Send(packet);

            packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
            packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Five };
            Send(packet);

            Wanted = true;
            break;
        }
      }
      else { Wanted = false; }

      if (Wanted && controllerConfig.showWanted) return;
      if (controllerConfig.showRPM && playerped.IsInVehicle() && currentVehicle.Driver == playerped)
      {
        float engineHealthFloat = Math.Min(1, currentVehicle.EngineHealth / 1000f);

        int RedChannel = 0;
        int GreenChannel = 0;
        int BlueChannel = 0;

        float rpmShiftDown = 0.4f;
        float rpmLow = 0.65f;
        float rpmMed = 0.8f;
        float rpmHigh = 0.95f;

        float currentRPM = currentVehicle.CurrentRPM;

        if (willShift)
        {
          willShift = false;
        }

        float engineIdleRpm = 0.2f;
        int currentGear = currentVehicle.CurrentGear;
        if (currentGear != currentVehicle.NextGear)
        {
          willShift = true;
        }
        //if (currentVehicle.CurrentGear > 1)
        //{
        //
        //  if (Game.IsControlPressed(GTA.Control.VehicleBrake))
        //  {
        //    packet.instructions[1].type = InstructionType.MicLED;
        //    packet.instructions[1].parameters = new object[] { controllerIndex, MicLEDMode.On };
        //    Send(packet);
        //
        //  }
        //}
        //else
        //{
        //  packet.instructions[1].type = InstructionType.MicLED;
        //  packet.instructions[1].parameters = new object[] { controllerIndex, MicLEDMode.Off };
        //  Send(packet);
        //
        //}

        float currentRPMRatio = InvLerpCapped(engineIdleRpm, 1f, currentRPM);

        float curVal = 0f;
        if (currentGear > 0)
        {
          if (currentRPMRatio < rpmShiftDown)
          {
            curVal = InvLerp(0, rpmShiftDown, currentRPMRatio);
            BlueChannel = (int)Lerp(96, 0, curVal);
            GreenChannel = (int)Lerp(0, 32, curVal);
          }
          else if (currentRPMRatio <= rpmLow)
          {
            curVal = InvLerp(rpmShiftDown, rpmLow, currentRPMRatio);
            //BlueChannel = (int)Main.Lerp(128, 0, curVal);
            GreenChannel = (int)Lerp(32, 255, curVal);
          }
          else if (currentRPMRatio <= rpmMed)
          {
            curVal = InvLerp(rpmLow, rpmMed, currentRPMRatio);

            RedChannel = (int)Lerp(0, 255, curVal);
            GreenChannel = (int)Lerp(255, 127, curVal);
            //GreenChannel = (int)Main.Lerp(0, 255, evalValue);
            //BlueChannel = (int)Main.Lerp(255, 0, evalValue);
          }
          else if (currentRPMRatio <= rpmHigh)
          {
            curVal = InvLerp(rpmMed, rpmHigh, currentRPMRatio);
            RedChannel = (int)Lerp(255, 180, curVal);
            GreenChannel = (int)Lerp(127, 5, curVal);
          }
          else
          {
            curVal = InvLerp(rpmHigh, 1f, currentRPMRatio);
            RedChannel = (int)Lerp(180, 255, curVal);
            GreenChannel = 5; // (int)Main.Lerp(173, 0, evalValue);
          }
        }
        else
        {
          RedChannel = BlueChannel = GreenChannel = (int)Lerp(10, 255, currentRPM);
        }

        RedChannel = (int)(RedChannel * engineHealthFloat * engineHealthFloat);
        GreenChannel = (int)(GreenChannel * engineHealthFloat * engineHealthFloat);
        BlueChannel = (int)(BlueChannel * engineHealthFloat * engineHealthFloat);

        packet.instructions[1].type = InstructionType.RGBUpdate;
        packet.instructions[1].parameters = new object[] { controllerIndex, RedChannel, GreenChannel, BlueChannel };
        Send(packet);
      }
      else if (controllerConfig.showHealth) // health indicator
      {
        packet.instructions[1].type = InstructionType.RGBUpdate;
        float health = playerped.HealthFloat / playerped.MaxHealthFloat;

        int red = 0;
        int green = 0;
        int blue = 0;

        green = (int)Lerp(0, 255, health);
        red = (int)Lerp(255, 0, health);

        packet.instructions[1].parameters = new object[4] { controllerIndex, red, green, blue };

        Send(packet);
      }
      else if (controllerConfig.showPlayerColor) // TODO: options
      {
        packet.instructions[1].type = InstructionType.RGBUpdate;
        if (playerped.Model == "player_zero")
        {
          packet.instructions[1].parameters = new object[4] { controllerIndex, 0, 0, 255 };
        }
        if (playerped.Model == "player_two")
        {
          packet.instructions[1].parameters = new object[4] { controllerIndex, 255, 121, 0 };
        }
        if (playerped.Model == "player_one")
        {
          packet.instructions[1].parameters = new object[4] { controllerIndex, 0, 180, 0 };
        }
        Send(packet);
        Wait(100);
      }
      else
      {
        packet.instructions[1].type = InstructionType.RGBUpdate;
        packet.instructions[1].parameters = new object[4] { controllerIndex, 0, 0, 0 };
        Send(packet);
        Wait(100);
      }
      /*
      if (playerped.IsInVehicle())
      {
          Vehicle currentVehicle = playerped.CurrentVehicle;
          OutputArgument outR = new();
          OutputArgument outG = new();
          OutputArgument outB = new();

          Function.Call(GTA.Native.Hash.GET_VEHICLE_COLOR, currentVehicle, outR, outG, outB);
          int red = outR.GetResult<int>();
          int green = outG.GetResult<int>();
          int blue = outB.GetResult<int>();
          // GTA.UI.Screen.ShowSubtitle(red + " - " + green + " - " + blue);
          packet.instructions[1].type = InstructionType.RGBUpdate;
          packet.instructions[1].parameters = new object[4] { controllerIndex, red, green, blue };
      }
      */
    }
  }
}