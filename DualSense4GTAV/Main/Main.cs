using GTA;
using GTA.Native;
using Shared;
using System;
using System.Diagnostics;
using System.Linq;
using static DSX_Base.Client.iO;

namespace DualSense4GTAV
{
  public class Main : Script
  {
    private static bool engine;
    private static int lastBrakeFreq = 0;
    private static int lastBrakeResistance = 200;
    private static int lastThrottleResistance = 1;
    private static bool noammo;

    private static DateTime TimeSent;

    private readonly ControllerConfig controllerConfig;

    public Main()
    {
      base.Tick += this.OnTick;

      Connect();
      Process.GetProcessesByName("DSX");
      //Interval = 50;

      controllerConfig = new ControllerConfig();
      base.KeyDown += controllerConfig.OnKeyDown;
    }

    /// <summary>
    /// Returns float: A value between zero and one, representing where the "value" parameter falls within the range defined by a and b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static float InvLerp(float a, float b, float v)
    {
      return (v - a) / (b - a);
    }

    /// <summary>
    /// Returns float: The interpolated float result between the two float values. Linearly interpolates between a and b by t. The parameter t is clamped to the range[0, 1].
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static float Lerp(float a, float b, float t)
    {
      //return firstFloat * by + secondFloat * (1 - by);
      return (1f - t) * a + b * t;
    }

    private float InvLerpCapped(float a, float b, float v)
    {
      return Math.Max(0, Math.Min(1, (v - a) / (b - a)));
    }

    private int LerpInt(float a, float b, float t)
    {
      //return firstFloat * by + secondFloat * (1 - by);
      return (int)((1f - t) * a + t * b);
    }

    private void OnTick(object sender, EventArgs e)
    {
      controllerConfig.pool.Process();

      Packet packet = new();
      int controllerIndex = 0;
      packet.instructions = new Instruction[4];
      Ped playerped = Game.Player.Character;

      Weapon playerWeapon = playerped.Weapons.Current;
      if (Function.Call<bool>(GTA.Native.Hash.IS_HUD_COMPONENT_ACTIVE, 19)) //HUD_WEAPON_WHEEL
      {
        SetAndSendPacket(packet, controllerIndex, Trigger.Right);
        SetAndSendPacket(packet, controllerIndex, Trigger.Left);
      }
      else if (playerped.IsInVehicle() || playerped.IsOnBike || playerped.IsInBoat || playerped.IsInHeli)
      {
        Vehicle currentVehicle = playerped.CurrentVehicle;

        if (!currentVehicle.IsEngineRunning)
        {
          SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 5, 6, 4, 5 });
          SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Bow, new() { 5, 6, 4, 5 });
        }
        else if (playerped.IsInHeli)
        {
          float health = (currentVehicle.HeliEngineHealth / 1000f * currentVehicle.HeliMainRotorHealth / 1000f * currentVehicle.HeliTailRotorHealth / 1000f);
          float healthMalus = (int)((1f - health) * 4f);

          //GTA.Native.Hash.traction

          //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { (int)(6f -
          //    healthMalus), Math.Min(resistance + (int)healthMalus, 8) });
          //GTA.UI.Screen.ShowSubtitle(LerpInt(0, 6, health) + "-"+ LerpInt(8, 1, health) + "-" + health);

          SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
          {
            1 + LerpInt(0, 6, health),
            LerpInt(8, 1, health)
          });

          SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
          {
            1 + LerpInt(0, 6, health),
            LerpInt(8, 1, health)
          });
        }
        else if (playerped.IsInPlane)
        {
          SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 5, 1 });
          SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 5, 1 });
        }
        else if (currentVehicle.IsInAir || !currentVehicle.IsOnAllWheels)
        {
          SetAndSendPacket(packet, controllerIndex, Trigger.Right);
          SetAndSendPacket(packet, controllerIndex, Trigger.Left);
        }
        else if (currentVehicle.EngineHealth <= 0f)
        {
          SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Hardest);
          SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Hardest);
          Wait(300);
        }
        // else if (currentVehicle.EngineHealth <= 400f)
        // {
        //     SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VibrateTrigger, new() { 6 });
        //     SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VibrateTrigger, new() { 6 });
        // }
        // else if (currentVehicle.EngineHealth <= 600f)
        // {
        //     SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VibrateTrigger, new() { 3 });
        //     SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VibrateTrigger, new() { 3 });
        // }
        //else if (currentVehicle.EngineHealth <= 800f)
        //{
        //    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Choppy);
        //    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Choppy);
        //}
        else if (currentVehicle.Wheels != null && currentVehicle.Wheels.Any(x => x.IsBursted) && (
                             (!playerped.IsInBoat && !playerped.IsInHeli) ||
                             (playerped.IsInPlane && !currentVehicle.IsInAir)))
        {
          int resistance = 4;
          var burstCount = currentVehicle.Wheels.Where(x => x.IsBursted).Count();
          int resStart = (int)(180 - burstCount * 25f);
          SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right,
              CustomTriggerValueMode.VibrateResistanceAB,
              resStart, 255, 144, 60, 120, 220, (int)currentVehicle.WheelSpeed);
          SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left,
              CustomTriggerValueMode.VibrateResistanceAB,
              resStart, 255, 144, 60, 120, 220, (int)currentVehicle.WheelSpeed);
          Wait(300);
        }
        else // if (playerped.CurrentVehicle.EngineHealth >= 1000f)
        {
          float engineHealthFloat = Math.Min(1, currentVehicle.EngineHealth / 1000f);
          float healthMalus = (int)((1f - engineHealthFloat) * 4f);
          int currentGear = currentVehicle.CurrentGear;
          int maxGear = currentVehicle.HighGear;
          float currentRPM = currentVehicle.CurrentRPM;
          float engineIdleRpm = 0.2f;
          float engineRange = 1f - engineIdleRpm;
          float currentRPMRatio = InvLerp(0.2f + 0.6f* (Math.Max(0,currentVehicle.CurrentGear - 1)) / currentVehicle.HighGear, 1f, currentRPM);
          //(currentRPM - engineIdleRpm) / engineRange;
          float currentSpeed = currentVehicle.Speed;
          float maxSpeed = Function.Call<float>(GTA.Native.Hash.GET_VEHICLE_ESTIMATED_MAX_SPEED, currentVehicle.Handle);

          //GTA.UI.Screen.ShowSubtitle(engineHealthFloat.ToString());

          float initialDriveForce = InvLerpCapped(0.1f, 0.4f, currentVehicle.HandlingData.InitialDriveForce); // most cars 0.1f < df < 0.4f
          float driveInertia = InvLerpCapped(0.3f, 1.0f, currentVehicle.HandlingData.DriveInertia);

          float gearForce = InvLerp(currentVehicle.HighGear, 1, currentGear);

          float spinnie = 1f;
          if (currentVehicle.Speed > 0)
          {
            spinnie = Math.Max(1, currentVehicle.WheelSpeed / currentVehicle.Speed);
          }

          float startOfGear = 1f;
          startOfGear *= Lerp(0.7f, 1f, initialDriveForce);
          startOfGear *= Lerp(0.5f, 1f, engineHealthFloat);
          startOfGear *= Lerp(1f, 0.5f, gearForce);
          startOfGear *= Lerp(0.6f, 1f, driveInertia);
          //startOfGear*= Lerp(0.4f, 1f, currentVehicle.Clutch);

          float lightnessVehicle = 1f;
          lightnessVehicle *= Lerp(1f, 0.6f, gearForce);
          lightnessVehicle *= spinnie;
          lightnessVehicle *= Lerp(0.6f, 1f, currentRPMRatio);
          lightnessVehicle *= engineHealthFloat;
          //lightnessVehicle *= Lerp(0.2f, 1f, currentVehicle.Clutch);

          float startOfResistance = initialDriveForce;

          startOfResistance *= Lerp(0.3f, 1f, engineHealthFloat);

          startOfResistance *= spinnie;

          startOfResistance *= Lerp(1f, 0.6f, gearForce);
          //startOfResistance = Math.Max(controllerConfig.startofResistanceVehicle, (Math.Min(controllerConfig.endofResistanceVehicle, startOfResistance)));

          float amountOfForce = driveInertia; //max  force for slow vehicles

          amountOfForce *= (Lerp(1f, 0.6f, gearForce)); // additional force for low gear
          amountOfForce /= spinnie;

          amountOfForce *= (Lerp(0.6f, 1f, currentRPMRatio));

          // amountOfForceExerted = Math.Max(controllerConfig.minResistanceVehicle, (Math.Min(controllerConfig.maxResistanceVehicle, amountOfForceExerted)));
          float brakeForce = InvLerpCapped(0.2f, 1.2f, currentVehicle.HandlingData.BrakeForce); // Bigger number = harder braking 0.01 - 2.0 and above. 1.0 uses brake force calculation unmodified.

          float startOfResistanceBrake = 1f * Lerp(0.4f, 1f, brakeForce);
          startOfResistanceBrake *= engineHealthFloat;

          float lighnessBrake = 1f * Lerp(0.5f, 1f, gearForce);
          lighnessBrake *= engineHealthFloat;
          if (currentGear != currentVehicle.NextGear)
          {
            SetAndSendPacket(packet, controllerIndex, Trigger.Left);
            SetAndSendPacket(packet, controllerIndex, Trigger.Right);
          }
          else if (currentGear >= 1)
          {
            SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right, CustomTriggerValueMode.Rigid,

              (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear),
              (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lightnessVehicle),
              255
            );
            //GTA.UI.Screen.ShowSubtitle(spinnie.ToString("N2") + " - "+ (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear) + " - " + (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, vehicleLightness));

            SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, CustomTriggerValueMode.Rigid,
              (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfResistanceBrake),
              (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lighnessBrake),
              255
            );

            //              BrakeForce / DriveInertia / Mass / InitialDriveForce
            // Betonmisch:  0,3        / 0,5          / 6000 / 0,11
            // Barracks:    0,3        / 0,5          / 9000 / 0,11
            // Mule:        0,25       / 1            / 5500 / 0,11
            // Coach:       0,25       / 0,5          / 8500 / 0,12
            // Ambulance:   0,6        / 1            / 2500 / 0,18
            // Tampa:       0,8        / 1            / 1200 / 0,27
            // Obey Sports: 1          / 1            / 1300 / 0,33
            // Banshee:     1          / 1            / 1200 / 0,34
          }
          else
          {
            SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, CustomTriggerValueMode.Rigid,
              (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear),
              (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lightnessVehicle),
              255);

            SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right, CustomTriggerValueMode.Rigid,
              (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfResistanceBrake),
              (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lighnessBrake),
              255);
          }

          // GTA.UI.Screen.ShowSubtitle(startOfResistance.ToString("N2") +" - " + amountOfForce.ToString("N2"));
          /*
          if (false)
          {
  float health = (currentVehicle.EngineHealth / 1000f);
  float healthMalus = (int)((1f - health) * 4f);
  int currentGear = currentVehicle.CurrentGear;
  int maxGear = currentVehicle.HighGear;
  float currentRPM = currentVehicle.CurrentRPM;
  float engineIdleRpm = 0.2f;
  float engineRange = 1f - engineIdleRpm;
  float currentRPMRatio = (currentRPM - engineIdleRpm) / engineRange;
  float currentSpeed = currentVehicle.Speed;
  float maxSpeed = Function.Call<float>(GTA.Native.Hash.GET_VEHICLE_ESTIMATED_MAX_SPEED, currentVehicle.Handle);

  float currentSurfingSpeed = Math.Min(1, currentSpeed / maxSpeed);

  int resistance = 4 - currentGear;
  //GTA.Native.Hash.traction

  //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { (int)(6f -
  //    healthMalus), Math.Min(resistance + (int)healthMalus, 8) });
  int forceR = Math.Max(1, Math.Min(resistance + (int)healthMalus, 8));
  // GTA.UI.Screen.ShowSubtitle(forceR.ToString());

  float initialDriveForce = InvLerpCapped(0, 0.4f, currentVehicle.HandlingData.InitialDriveForce); // capped at 0.4f
  float driveInertia = InvLerpCapped(0.3f, 1.0f, currentVehicle.HandlingData.DriveInertia) *
                       InvLerpCapped(3500, 1200, currentVehicle.HandlingData.Mass);
  float gearForce = InvLerpCapped(currentVehicle.HighGear, 1, currentGear);

  float spinnie = 1f;
  if (currentVehicle.Speed > 0)
  {
    spinnie = currentVehicle.WheelSpeed / currentVehicle.Speed;
  }

  int startOfResistance = (int)Lerp(1, 7, driveInertia);

  startOfResistance -= (int)Lerp(8, -1, health);

  startOfResistance -= (int)((1f - spinnie) * 4f);

  startOfResistance -= (int)(Lerp(4, 0, gearForce));
  startOfResistance = Math.Max(controllerConfig.startofResistanceVehicle, (Math.Min(controllerConfig.endofResistanceVehicle, startOfResistance)));

  int amountOfForceExerted = (int)Lerp(3, 1, initialDriveForce); //max  force for slow vehicles

  amountOfForceExerted += (int)(Lerp(0, 3, gearForce)); // additional force for low gear
  amountOfForceExerted -= (int)((1f - spinnie) * 6f);
  amountOfForceExerted += (int)(Lerp(0, 2f, spinnie));
  amountOfForceExerted -= (int)(Lerp(0, 4, currentRPMRatio));

  amountOfForceExerted = Math.Max(controllerConfig.minResistanceVehicle, (Math.Min(controllerConfig.maxResistanceVehicle, amountOfForceExerted)));

  float brakeForce = InvLerpCapped(0.2f, 1.2f, currentVehicle.HandlingData.BrakeForce);

  int startOfResistanceBrake = (int)Lerp(3, 7, brakeForce);
  startOfResistanceBrake -= (int)Lerp(5, -1, health);
  startOfResistanceBrake = Math.Max(1, (Math.Min(7, startOfResistanceBrake)));

  int amountOfForceExertedBrake = (int)Lerp(4, 1, gearForce * InvLerpCapped(500, 5000, currentVehicle.HandlingData.Mass));
  amountOfForceExertedBrake += (int)Lerp(4, 0, health);
  amountOfForceExertedBrake = Math.Max(1, (Math.Min(8, amountOfForceExertedBrake)));

  if (currentGear >= 1)
  {
    // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
    // {
    //     1+(int)(currentRpm*health * 7 ),
    //     forceR
    // });

    // SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
    // {
    //     6 -
    //     (currentGear) -
    //     (int)(healthMalus / 2f),
    //     8 - resistance
    // });
    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
    {
      startOfResistance,
      amountOfForceExerted,
    });

    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
    {
      startOfResistanceBrake,
      amountOfForceExertedBrake,
    });

    //              BrakeForce / DriveInertia / Mass / InitialDriveForce
    // Betonmisch:  0,3        / 0,5          / 6000 / 0,11
    // Barracks:    0,3        / 0,5          / 9000 / 0,11
    // Mule:        0,25       / 1            / 5500 / 0,11
    // Coach:       0,25       / 0,5          / 8500 / 0,12
    // Ambulance:   0,6        / 1            / 2500 / 0,18
    // Tampa:       0,8        / 1            / 1200 / 0,27
    // Obey Sports: 1          / 1            / 1300 / 0,33
    // Banshee:     1          / 1            / 1200 / 0,34
  }
  else
  {
    // SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
    // {
    //     (int)(currentRpm*health * 7),
    //     forceR
    // });
    // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
    // {
    //     7-(int)(currentRpm*health * 7),
    //     forceR
    // });

    SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
    {
      startOfResistance,
      amountOfForceExerted,
    });

    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
    {
      startOfResistanceBrake,
      amountOfForceExertedBrake,
    });
  }
}
          */
        }
      }
      else
      {
        if (playerWeapon != null && (playerped.IsReloading || playerWeapon.AmmoInClip == 0))
        {
          SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 5, 6, 4, 8 });
          SetAndSendPacket(packet, controllerIndex, Trigger.Left);
        }
        else
        {
          if (playerWeapon != null)
          {
            float fireRate = Function.Call<float>(Hash._GET_WEAPON_TIME_BETWEEN_SHOTS, playerWeapon.Hash);
            float weaponDamage = Function.Call<float>(Hash.GET_WEAPON_DAMAGE, playerWeapon.Hash);

            int weaponStrength = 4 + (int)(weaponDamage / 8f);
            weaponStrength = Math.Min(weaponStrength, 8);
            int fireRateAutomaticInt = (int)(1.4f / fireRate);
            //GTA.UI.Notification.Show(weaponStrength.ToString());

            switch (playerWeapon.Group)
            {
              case WeaponGroup.Pistol:

                // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Soft);
                // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.SemiAutomaticGun, new() { 2, 7, 8 });
                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                    new() { 1, 1 });
                if (!Game.IsControlPressed(GTA.Control.Attack))
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow,
                      new() { 1, 6, 4, 8 });
                }
                else
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                      new() { 8, weaponStrength, fireRateAutomaticInt });
                }

                //UI.ShowSubtitle("Doing great, aiming widda pistal");
                break;

              case WeaponGroup.SMG:
                // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.VibrateTrigger, new() { 40 });
                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                    new() { 1, 2 });
                if (!Game.IsControlPressed(GTA.Control.Attack))
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow,
                      new() { 1, 6, 4, 8 });
                }
                else SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Machine,
                    new() { 7, 9, 4, 4, fireRateAutomaticInt, 0 });
                break;

              case WeaponGroup.AssaultRifle:
                // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.AutomaticGun, new() { 2, 7, 8 });
                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                    new() { 1, 2 });
                if (!Game.IsControlPressed(GTA.Control.Attack))
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow,
                      new() { 1, 6, 4, 8 });
                }
                else SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                    new() { 6, weaponStrength, fireRateAutomaticInt });

                break;

              case WeaponGroup.MG:
                // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hard);
                // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.VibrateTrigger, new() { 40 });
                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                    new() { 1, 2 }); //1,3
                if (!Game.IsControlPressed(GTA.Control.Attack))
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow,
                      new() { 1, 6, 4, 8 });
                }
                else SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                    new() { 8, weaponStrength, fireRateAutomaticInt });
                break;

              case WeaponGroup.Shotgun:

                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                    new() { 1, 2 });
                //if (playerWeapon.Hash == WeaponHash.AssaultShotgun ||
                //    playerWeapon.Hash == WeaponHash.SweeperShotgun ||
                //    playerWeapon.Hash == WeaponHash.HeavyShotgun ||
                //    playerWeapon.Hash == WeaponHash.BullpupShotgun)
                //{
                if (!Game.IsControlPressed(GTA.Control.Attack))
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow,
                      new() { 1, 6, 4, 8 });
                }
                else SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                    new() { 8, weaponStrength, fireRateAutomaticInt });
                //}
                //else
                //{
                //    SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow,
                //        new() { 2, 4, 4, 4 });
                //}

                // SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hardest);
                // SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.Bow, new() { 0, 8, 2, 5 });
                //UI.ShowSubtitle("Shotgun");
                break;

              case WeaponGroup.Sniper:
                //SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Hardest);
                //SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.Hard);
                //SetAndSendPacket(packet, num, Trigger.Left, TriggerMode.Galloping, new(){4,9,1,7,1});
                //SetAndSendPacket(packet, num, Trigger.Right, TriggerMode.SemiAutomaticGun, new(){4,6,8});
                SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                    new() { 1, 2 });
                SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.SemiAutomaticGun,
                    new() { 2, 7, 4 });
                //UI.ShowSubtitle("Sniper");

                break;

              case WeaponGroup.Heavy:
                if (playerWeapon.Hash == WeaponHash.Minigun)
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                      new() { 1, 2 });
                  //SetAndSendPacket(packet, controllerIndex, Trigger.Right,  TriggerMode.VibrateTrigger, new() { 39 });

                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun,
                      new() { 8, weaponStrength, fireRateAutomaticInt });
                }
                else
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance,
                      new() { 1, 2 });
                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow,
                      new() { 1, 6, 4, 8 });
                }

                break;

              default:
              case WeaponGroup.Unarmed:
              case WeaponGroup.Melee:
              case WeaponGroup.Thrown:
              case WeaponGroup.PetrolCan:
                SetAndSendPacket(packet, controllerIndex, Trigger.Left);
                SetAndSendPacket(packet, controllerIndex, Trigger.Right);
                break;
            }
          }
        }
      }
    }

    private float Remap(float iMin, float iMax, float oMin, float oMax, float v)
    {
      float t = InvLerp(iMin, iMax, v);
      return Lerp(oMin, oMax, t);
    }
  }
}