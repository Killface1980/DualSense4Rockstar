using GTA;
using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using DSX_Base.MathExtended;
using static DSX_Base.Client.iO;
using static DualSense4GTAV.Main_GTAV;

namespace DualSense4GTAV.Main_LEDs
{
  public class Main_LEDs : Script
  {
    public static bool isWanted = false;
    public static bool isSirenOn = false;
    private static float currentIdleRPM = 0.2f;
    private static bool willShift = false;
    private add _add2 = null;
    private static int brakeLight = 228;
    private static int reverseLight = 96;
    private iO _obj = null;


    public Main_LEDs()
    {
      rpmColorDict = new Dictionary<float, Color> {
        { 0f,    Color.FromArgb(0, 32,0) },
        { 0.4f,  Color.FromArgb(0, 96,0) },
        { 0.65f, Color.FromArgb(0, 255,0) },
        { 0.75f, Color.FromArgb(255, 255,0) },
        { 0.85f, Color.FromArgb(255, 127,0) },
        { 0.95f, Color.FromArgb(255, 0,0) },
        { 1f,    Color.FromArgb(255, 64,64) },
      };
      rpmRed = new();
      rpmGreen= new();
      rpmBlue= new();

      foreach (KeyValuePair<float, Color> rpm in rpmColorDict)
      {
        rpmRed.Add(rpm.Key, rpm.Value.R);
        rpmGreen.Add(rpm.Key, rpm.Value.G);
        rpmBlue.Add(rpm.Key, rpm.Value.B);
      }

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
        GTA.UI.Notification.Show("Your controller is disconnected or discharged, please fix or press " + KeyConf
          .showCommStat + " to hide this message.") ;
        return;
      }
      if (bat <= 15 && controllerConfig.showbatstat)
      {
        GTA.UI.Notification.Show("Your controller battery is  " + bat + " to hide this message press "+KeyConf
          .toggleBatStat, true);
        //return;
      }
      Ped playerped = Game.Player.Character;

      Vehicle currentVehicle = playerped.CurrentVehicle;

      isSirenOn = controllerConfig.showWanted && playerped.IsInVehicle() && currentVehicle.IsSirenActive;

      if (controllerConfig.showWanted)
      {
        if (isSirenOn)
        {
          this._add2.rgbupdat2e(40, playerped.Health);
          return;
        }

        switch (Game.Player.WantedLevel)
        {
          case 0:
            packet.instructions[2].type = InstructionType.PlayerLED;
            packet.instructions[2].parameters = new object[6]
                { controllerIndex, false, false, false, false, false };
            Send(packet);

            packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
            packet.instructions[2].parameters = new object[]
                { controllerIndex, PlayerLEDNewRevision.AllOff };
            Send(packet);
            isWanted = false;
            break;

          case 1:
            {
              this._add2.rgbupdat2e(40, playerped.Health);
              isWanted = true;
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

            isWanted = true;
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

            isWanted = true;
            break;

          case 4:
            this._add2.rgbupdat2e(15, playerped.Health);
            packet.instructions[2].type = InstructionType.PlayerLED;
            packet.instructions[2].parameters = new object[6]
                { controllerIndex, true, true, true, true, false };
            Send(packet);

            packet.instructions[2].type = InstructionType.PlayerLEDNewRevision;
            packet.instructions[2].parameters = new object[] { controllerIndex, PlayerLEDNewRevision.Four };
            Send(packet);

            isWanted = true;
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

            isWanted = true;
            break;
        }
      }
      else { isWanted = false; }

      if (isSirenOn ) return;
      if (isWanted ) return;
      if (controllerConfig.showRPM && playerped.IsInVehicle() && currentVehicle.Driver == playerped)
      {
        float engineHealthFloat = Math.Min(1, currentVehicle.EngineHealth / 1000f);

        int RedChannel;
        int GreenChannel;
        int BlueChannel;

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

        // float currentRPMRatio = MathExtended.InverseLerp(engineIdleRpm, 1f, currentRPM);

        engineIdleRpm = 0.2f * (float)Math.Pow(1.2f, (currentGear - 1) * currentVehicle.HandlingData.ClutchChangeRateScaleUpShift);
        float currentRPMRatio = MathExtended.InverseLerp(engineIdleRpm, 1f, currentRPM);

        

        float curVal ;
        if (currentGear > 0 || currentRPM == 0.2f)
        {
          if (!Game.IsControlPressed(Control.VehicleBrake))
          {
            GetColorForRPM(currentRPMRatio, out RedChannel, out GreenChannel, out BlueChannel);
          }
          else
          {
            RedChannel = GreenChannel = BlueChannel = brakeLight;
          }
          /*          if (currentRPMRatio < rpmShiftDown)
                    {
                      curVal = Mathf.InverseLerp(0, rpmShiftDown, currentRPMRatio);
                      BlueChannel = (int)Mathf.Lerp(96, 0, curVal);
                      GreenChannel = (int)Mathf.Lerp(0, 96, curVal);
                    }
                    else if (currentRPMRatio <= rpmLow)
                    {
                      curVal = Mathf.InverseLerp(rpmShiftDown, rpmLow, currentRPMRatio);
                      //BlueChannel = (int)Main.Lerp(128, 0, curVal);
                      GreenChannel = (int)Mathf.Lerp(96, 255, curVal);
                    }
                    else if (currentRPMRatio <= rpmMed)
                    {
                      curVal = Mathf.InverseLerp(rpmLow, rpmMed, currentRPMRatio);

                      RedChannel = (int)Mathf.Lerp(0, 255, curVal);
                      GreenChannel = (int)Mathf.Lerp(255, 127, curVal);
                      //GreenChannel = (int)Main.Lerp(0, 255, evalValue);
                      //BlueChannel = (int)Main.Lerp(255, 0, evalValue);
                    }
                    else if (currentRPMRatio <= rpmHigh)
                    {
                      curVal = Mathf.InverseLerp(rpmMed, rpmHigh, currentRPMRatio);
                      RedChannel = (int)Mathf.Lerp(255, 180, curVal);
                      GreenChannel = (int)Mathf.Lerp(127, 5, curVal);
                    }
                    else
                    {
                      curVal = Mathf.InverseLerp(rpmHigh, 1f, currentRPMRatio);
                      RedChannel = (int)Mathf.Lerp(180, 255, curVal);
                      GreenChannel = 5; // (int)Main.Lerp(173, 0, evalValue);
                    }
          */

        }
        else
        {
          // curVal = MathExtended.InverseLerp(0.2f, 1f, currentRPM);
          // 
          // RedChannel = BlueChannel = (int)MathExtended.Lerp(0, 255, curVal);
          // GreenChannel = (int)MathExtended.Lerp(32, 255, curVal);

          RedChannel = GreenChannel = BlueChannel = reverseLight;
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


        int red = (int)MathExtended.Lerp(255, 0, health);
        int green = (int)MathExtended.Lerp(0, 255, health);
        int blue = 0;

        packet.instructions[1].parameters = new object[] { controllerIndex, red, green, blue };

        Send(packet);
      }
      else if (controllerConfig.showPlayerColor) // TODO: options
      {
        packet.instructions[1].type = InstructionType.RGBUpdate;
        if (playerped.Model == "player_zero")
        {
          packet.instructions[1].parameters = new object[] { controllerIndex, 0, 0, 255 };
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

    private Dictionary<float, Color> rpmColorDict;
    BasicCurve rpmRed;
    BasicCurve rpmGreen;
    BasicCurve rpmBlue;
    public void GetColorForRPM(float theRPM, out int red, out int green, out int blue)
    {
      red = (int)rpmRed.Evaluate(theRPM);
      green = (int)rpmGreen.Evaluate(theRPM);
      blue = (int)rpmBlue.Evaluate(theRPM);
    }
  }
}