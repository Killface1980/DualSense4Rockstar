using GTA;
using GTA.Native;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using DSX_Base;
using static DSX_Base.Client.iO;
using static DSX_Base.MathExtended.MathExtended;

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

    public static readonly ControllerConfig controllerConfig = new();

    public Main()
    {
      Tick += this.OnTick;
      //Tick += Main_LEDs.Ontick;
      Connect();
      Process.GetProcessesByName("DSX");
      //Interval = 50;
      KeyDown += controllerConfig.OnKeyDown;
    }



    /// <summary>
    /// Returns float: A value between zero and one, representing where the "value" parameter falls within the range defined by a and b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    private void OnTick(object sender, EventArgs e)
    {
      controllerConfig.pool.Process();

      Packet packet = new();
      int controllerIndex = 0;
      packet.instructions = new Instruction[4];
      Ped playerped = Player.Character;
      
      Weapon playerWeapon = playerped.Weapons.Current;
      //if (Function.Call<bool>(Hash.IS_HUD_COMPONENT_ACTIVE, 19)) //HUD_WEAPON_WHEEL
      //{
      //  SetAndSendPacket(packet, Trigger.Right);
      //  SetAndSendPacket(packet, Trigger.Left);
      //}
      // else 
      if ((playerped.isSittingInVehicle() || playerped.isInVehicle() || playerped.isInWater || playerped.isInAir) &&
               playerped.CurrentVehicle.CreatePedOnSeat(VehicleSeat.Driver) == playerped)
      {
        Vehicle currentVehicle = playerped.CurrentVehicle;


        if (!currentVehicle.EngineRunning)
        {
          SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow, new() { 5, 6, 4, 5 });
          SetAndSendPacket(packet, Trigger.Left, TriggerMode.Bow, new() { 5, 6, 4, 5 });
        }
        else if (currentVehicle.isOnAllWheels  && currentVehicle.Wheels != null && currentVehicle.Wheels.Any(x => x.IsBursted))
        {
          bool anyTireBurst = currentVehicle.Model.isBike
            ? currentVehicle.IsTireBurst(VehicleWheel.RearLeft) || currentVehicle.IsTireBurst(VehicleWheel.FrontLeft) :
            currentVehicle.IsTireBurst(VehicleWheel.FrontLeft) || currentVehicle.IsTireBurst(VehicleWheel.FrontRight)|| currentVehicle.IsTireBurst(VehicleWheel.RearLeft) || currentVehicle.IsTireBurst(VehicleWheel.RearRight);
          int resistance = 4;
          int burstCount = currentVehicle.Wheels.Where(x => x.IsBursted).Count();
          int resStart = (int)(140 - burstCount * 25f);
          SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right,
            CustomTriggerValueMode.VibrateResistanceAB,
            resStart, 255, 144, 90, 120, 220, (int)currentVehicle.WheelSpeed);
          SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left,
            CustomTriggerValueMode.VibrateResistanceAB,
            resStart, 255, 144, 90, 120, 220, (int)currentVehicle.WheelSpeed);
          Wait(300);
        }
        else if (playerped.Model.isHelicopter)
        {
          float health = (currentVehicle.HeliEngineHealth / 1000f * currentVehicle.HeliMainRotorHealth / 1000f * currentVehicle.HeliTailRotorHealth / 1000f);
          float healthMalus = (int)((1f - health) * 4f);

          //GTA.Native.Hash.traction

          //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { (int)(6f -
          //    healthMalus), Math.Min(resistance + (int)healthMalus, 8) });
          //GTA.UI.Screen.ShowSubtitle(LerpInt(0, 6, health) + "-"+ LerpInt(8, 1, health) + "-" + health);

          SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new()
          {
            1 + Lerp(0, 6, health),
            Lerp(8, 1, health)
          });

          SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new()
          {
            1 + Lerp(0, 6, health),
            Lerp(8, 1, health)
          });
        }
        else if (playerped.isInWater && !playerped.isSwimming)
        {
          float engineHealthFloat = Math.Min(1, currentVehicle.EngineHealth / 1000f);

          float initialDriveForce = InverseLerp(0.1f, 0.4f, currentVehicle.HandlingData.InitialDriveForce); // most cars 0.1f < df < 0.4f
          float driveInertia = InverseLerp(0.3f, 1.0f, currentVehicle.HandlingData.DriveInertia);

          float startOfGear = 0.8f;
          startOfGear *= Lerp(0.7f, 1f, initialDriveForce);
          startOfGear *= Lerp(0.5f, 1f, engineHealthFloat);

          //startOfGear*= Lerp(0.4f, 1f, currentVehicle.Clutch);

          float lightnessVehicle = 0.9f;
          lightnessVehicle *= Lerp(0.6f, 1f, driveInertia);
          lightnessVehicle *= Lerp(0.3f, 0.8f, currentVehicle.CurrentRPM);
          lightnessVehicle *= engineHealthFloat;
          //lightnessVehicle *= Lerp(0.2f, 1f, currentVehicle.Clutch);

          float brakeForce = InverseLerp(0.2f, 1.2f, currentVehicle.HandlingData.BrakeForce); // Bigger number = harder braking 0.01 - 2.0 and above. 1.0 uses brake force calculation unmodified.

          float startOfResistanceBrake = 1f * Lerp(0.4f, 1f, brakeForce);
          startOfResistanceBrake *= engineHealthFloat;

          float lighnessBrake = 0.8f;
          lighnessBrake *= engineHealthFloat;
          int currentGear = currentVehicle.CurrentGear;
          /*
          0xA7F04022
          // GetVehicleWheelSurfaceMaterial
          int GET_VEHICLE_WHEEL_SURFACE_MATERIAL(Vehicle vehicle, int wheelIndex);
          */

          if (currentGear > 0)
          {
            SetAndSendPacketCustom(packet, Trigger.Right, CustomTriggerValueMode.Rigid,

              (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear),
              (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lightnessVehicle),
              255
            );
            //GTA.UI.Screen.ShowSubtitle(spinnie.ToString("N2") + " - "+ (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear) + " - " + (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, vehicleLightness));

            SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid,
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
            SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid,
              (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear),
              (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lightnessVehicle),
              255);

            SetAndSendPacketCustom(packet, Trigger.Right, CustomTriggerValueMode.Rigid,
              (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfResistanceBrake),
              (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lighnessBrake),
              255);
          }
        }
        else if (playerped.Model.isPlane)
        {
          SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 5, 1 });
          SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 5, 1 });
        }
        else if (currentVehicle.IsInAir || !currentVehicle.IsOnAllWheels)
        {
          SetAndSendPacket(packet, Trigger.Right);
          SetAndSendPacket(packet, Trigger.Left);
        }
        else if (currentVehicle.EngineHealth <= 0f)
        {
          SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);
          SetAndSendPacket(packet, Trigger.Left, TriggerMode.Hardest);
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
          float currentRPMRatio = InverseLerp(0.2f + 0.6f * (Math.Max(0, currentVehicle.CurrentGear - 1)) / currentVehicle.HighGear, 1f, currentRPM);
          //(currentRPM - engineIdleRpm) / engineRange;
          float currentSpeed = currentVehicle.Speed;
          float maxSpeed = Function.Call<float>(Hash.GET_VEHICLE_ESTIMATED_MAX_SPEED, currentVehicle.Handle);

          //GTA.UI.Screen.ShowSubtitle(engineHealthFloat.ToString());

          float initialDriveForce = InverseLerp(0.1f, 0.4f, currentVehicle.HandlingData.InitialDriveForce); // most cars 0.1f < df < 0.4f
          float driveInertia = InverseLerp(0.3f, 1.0f, currentVehicle.HandlingData.DriveInertia);

          float gearForce = InverseLerp(currentVehicle.HighGear, 1, currentGear);

          float spinnie = 1f;
          if (currentVehicle.Speed > 0)
          {
            spinnie = Math.Max(1, currentVehicle.WheelSpeed / currentVehicle.Speed);
          }

          float startOfGear = 1f;
          startOfGear *= Lerp(0.7f, 1f, initialDriveForce);
          startOfGear *= Lerp(0.5f, 1f, engineHealthFloat);
          startOfGear *= Lerp(1f, 0.7f, gearForce);
          startOfGear *= Lerp(0.6f, 1f, driveInertia);
          startOfGear *= spinnie;

          //startOfGear*= Lerp(0.4f, 1f, currentVehicle.Clutch);

          float lightnessVehicle = 1f;
          lightnessVehicle *= Lerp(1f, 0.8f, gearForce);
          lightnessVehicle *= Lerp(0.8f, 1f, currentRPMRatio);
          lightnessVehicle *= engineHealthFloat;
          //lightnessVehicle *= Lerp(0.2f, 1f, currentVehicle.Clutch);

          float brakeForce = InverseLerp(0.2f, 1.2f, currentVehicle.HandlingData.BrakeForce); // Bigger number = harder braking 0.01 - 2.0 and above. 1.0 uses brake force calculation unmodified.

          float startOfResistanceBrake = 1f * Lerp(0.4f, 1f, brakeForce);
          startOfResistanceBrake *= engineHealthFloat;

          float lighnessBrake = 1f * Lerp(0.5f, 1f, gearForce);
          lighnessBrake *= engineHealthFloat;

          /*
          0xA7F04022
          // GetVehicleWheelSurfaceMaterial
          int GET_VEHICLE_WHEEL_SURFACE_MATERIAL(Vehicle vehicle, int wheelIndex);
          */

          if (currentGear != currentVehicle.NextGear) // here comes the clutch!
          {
            SetAndSendPacket(packet, Trigger.Left);
            SetAndSendPacket(packet, Trigger.Right);
            Wait(100);
          }
          else if (currentGear > 0)
          {
            SetAndSendPacketCustom(packet, Trigger.Right, CustomTriggerValueMode.Rigid,

              (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear),
              (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lightnessVehicle),
              255
            );
            //GTA.UI.Screen.ShowSubtitle(spinnie.ToString("N2") + " - "+ (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear) + " - " + (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, vehicleLightness));

            SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid,
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
            SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid,
              (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear),
              (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lightnessVehicle),
              255);

            SetAndSendPacketCustom(packet, Trigger.Right, CustomTriggerValueMode.Rigid,
              (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfResistanceBrake),
              (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lighnessBrake),
              255);
          }

          // GTA.UI.Screen.ShowSubtitle(startOfResistance.ToString("N2") +" - " + amountOfForce.ToString("N2"));
          
        }
      }
      else
      {
        if (playerWeapon != null && (playerped.IsReloading || playerWeapon.AmmoInClip == 0))
        {
          SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow, new() { 5, 6, 4, 4 });
          SetAndSendPacket(packet, Trigger.Left);
          Wait(50);
          return;
        }
        else
        {
          if (playerWeapon != null)
          {
            float animationLength = Function.Call<float>(Hash._GET_WEAPON_TIME_BETWEEN_SHOTS, playerWeapon.Hash);
            float weaponDamage = Function.Call<float>(Hash.GET_WEAPON_DAMAGE, playerWeapon.Hash);

            int weaponStrength = 2 + (int)(weaponDamage / 6f);
            weaponStrength = Math.Min(weaponStrength, 8);

            int fireRateAutomaticInt = (int)(1.4f / animationLength);
            // GTA.UI.Notification.Show(fireFrequency.ToString());

            int triggerForce = weaponStrength;
            int triggerSnapForce = weaponStrength;
            switch (playerWeapon.Group)
            {
              case WeaponGroup.Pistol:
              case WeaponGroup.SMG:
              case WeaponGroup.AssaultRifle:
              case WeaponGroup.MG:
              case WeaponGroup.Shotgun:
                ApplyAutomaticWeaponHandling(packet, controllerIndex, playerped, weaponStrength, fireRateAutomaticInt, new() { 1, 6, triggerForce, triggerSnapForce }, animationLength);
                break;

              case WeaponGroup.Sniper:
                SetAndSendPacket(packet, Trigger.Left, TriggerMode.Resistance,
                    new() { 1, 2 });
                SetAndSendPacket(packet, Trigger.Right, TriggerMode.SemiAutomaticGun,
                    new() { 2, 7, 4 });
                //UI.ShowSubtitle("Sniper");
                break;

              case WeaponGroup.Heavy:
                if (playerWeapon.Hash == WeaponHash.Minigun)
                {
                  ApplyAutomaticWeaponHandling(packet, playerped, Math.Min (8, weaponStrength+2), fireRateAutomaticInt, new() { 1, 6, 4, 8 }, animationLength);
                }
                else
                {
                  SetAndSendPacket(packet, Trigger.Left, TriggerMode.Resistance,
                      new() { 1, 2 });
                  SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow,
                      new() { 1, 6, 4, 8 });
                }

                break;

              default:
              case WeaponGroup.Unarmed:
              case WeaponGroup.Melee:
              case WeaponGroup.Thrown:
              case WeaponGroup.PetrolCan:
                SetAndSendPacket(packet, Trigger.Left);
                SetAndSendPacket(packet, Trigger.Right);
                break;
            }
          }
        }
      }
    }

    private static int lastAmmo = 0;

    private void ApplyAutomaticWeaponHandling(Packet packet, Ped playerPed, int weaponStrength,
      int fireRateAutomaticInt, List<int> triggerParameters, float timeBetweenShots)
    {
      SetAndSendPacket(packet, Trigger.Left, TriggerMode.Resistance,
        new() { (int)(weaponStrength / 5f), (int)(weaponStrength / 3f) });


      bool doDefault = false;
      int ammoInClip = playerPed.Weapons.Current.AmmoInClip;
      if (Game.isGameKeyPressed(GameKey.Attack) || playerPed.isShooting)
      {
        if (lastAmmo != ammoInClip)
        {
          SetAndSendPacket(packet, Trigger.Right, TriggerMode.AutomaticGun,
            new() { 6, weaponStrength, fireRateAutomaticInt });

          // GTA.UI.Screen.ShowSubtitle(lastAmmo + " - " + ammoInClip+" - "+ (int)(timeBetweenShots * 1000f));

          if (playerPed.Weapons.Current.Hash == WeaponHash.Minigun)
            Wait((int)(timeBetweenShots * 10000f));
          else
            Wait((int)(timeBetweenShots * 1000f));


          lastAmmo = ammoInClip;

          return;
        }

      }

      if (playerPed.Weapons.Current.Hash == WeaponHash.Minigun) // not ahooting, only spinning
      {
        SetAndSendPacket(packet, Trigger.Right, TriggerMode.Machine,
          new(){1,9,2,2,25,1});
      }
      else
      {
        SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow,
          triggerParameters);
      }        
      lastAmmo = ammoInClip;

      Wait(50);

      
      //Script.Wait(fireRateAutomaticInt);
    }

    private float Remap(float iMin, float iMax, float oMin, float oMax, float v)
    {
      float t = InverseLerp(iMin, iMax, v);
      return Lerp(oMin, oMax, t);
    }
  }
}