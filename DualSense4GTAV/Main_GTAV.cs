using DSX_Base;
using DSX_Base.MathExtended;
using DualSense4GTAV.Config;
using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static DSX_Base.Client.iO;

namespace DualSense4GTAV
{
  public class Main_GTAV : Script
  {
    public static readonly ControllerConfig controllerConfig = new();

    public static float max = 60;
    public static float min = 20;
    private static int lastAmmo = 0;

    private static bool spinning;



  public Main_GTAV()
    {
      Tick += this.OnTick;
      //Tick += Main_LEDs.Ontick;
      Connect();
      Process.GetProcessesByName("DSX");
      //Interval = 50;
      KeyUp += controllerConfig.OnKeyDown;
    }

    public static float CurrentWeaponDamageNormalized(WeaponHash weaponHash)
    {
      float weaponDamage = currentDamage;// Function.Call<float>(Hash.GET_WEAPON_DAMAGE, weaponHash);

      // min = Math.Min(weaponDamage, min);
      // min = Math.Max(min, 10);
      // max = Math.Max(weaponDamage, max);
      // max = Math.Min(max, 80);
      float result = MathExtended.InverseLerp(min, max, weaponDamage);

      // GTA.UI.Screen.ShowSubtitle( weaponDamage.ToString("N3"));
      return result;
    }

    Weapon lastWeapon;
    private Vehicle lastVehicle;

    private static float currentAccuracy;
    private static float currentDamage;
    float weaponStrength;

    private void OnTick(object sender, EventArgs e)
    {
      if (ControllerConfig.isDisabled)
      {
        return;
      }
      controllerConfig.pool.Process();




      Packet packet = new();
      int controllerIndex = 0;
      packet.instructions = new Instruction[4];
      Ped playerPed = Game.Player.Character;


      if (Function.Call<bool>(Hash.IS_HUD_COMPONENT_ACTIVE, 19)) //HUD_WEAPON_WHEEL
      {
        SetAndSendPacket(Trigger.Right);
        SetAndSendPacket(Trigger.Left);
      }
      else if ((playerPed.IsInVehicle() || playerPed.IsOnBike || playerPed.IsInBoat || playerPed.IsInHeli) &&
               playerPed.CurrentVehicle.Driver == playerPed)
      {
        Vehicle currentVehicle = playerPed.CurrentVehicle;

        // BUG: Crashing, need the wheel surface for haptic feedback
        //ulong GET_VEHICLE_WHEEL_SURFACE_MATERIAL = 0xA7F04022;

        // uint hash = Function.Call<uint>(Hash.GET_HASH_KEY, "GET_VEHICLE_WHEEL_SURFACE_MATERIAL");

        //GTA.UI.Screen.ShowSubtitle(hash.ToString());
        // GTA.UI.Screen.ShowSubtitle(Function.Call<int>((Hash)hash, currentVehicle.Handle, 0).ToString()); // this one crashes
        // GTA.UI.Screen.ShowSubtitle(Function.Call<int>((Hash)0xA7F04022, currentVehicle.Handle, 0).ToString()); // this one too

        /*
         -- Surfaces which are counted as road (https://docs.fivem.net/natives/?_0xA7F04022)
          roadSurfaces = {
              1, 3, 4, 12
          }
        */

        if (!currentVehicle.IsEngineRunning)
        {
          DoTrigger_Bow( Trigger.Right, 5, 6, 4, 5 );
          DoTrigger_Bow( Trigger.Left, 5, 6, 4, 5 );
        }
        else if (!currentVehicle.IsInAir && currentVehicle.Wheels != null && currentVehicle.Wheels.Any(x => x.IsBursted))
        {
          int resistance = 4;
          int burstCount = currentVehicle.Wheels.Where(x => x.IsBursted).Count();
          int resStart = (int)(140 - burstCount * 25f);
          SetAndSendPacketCustom(packet, Trigger.Right,
            CustomTriggerValueMode.VibrateResistanceAB,
            startOfResistance: resStart, amountOfForceExerted: 255, forceExertedInRange: 144, ab_strengthNearRelease: 90, ab_strengthNearMiddle: 120, ab_strengthPressedState: 220, ab_actuationFrequency: (int)currentVehicle.WheelSpeed);
          SetAndSendPacketCustom(packet, Trigger.Left,
            CustomTriggerValueMode.VibrateResistanceAB,
            startOfResistance: resStart, amountOfForceExerted: 255, forceExertedInRange: 144, ab_strengthNearRelease: 90, ab_strengthNearMiddle: 120, ab_strengthPressedState: 220, ab_actuationFrequency: (int)currentVehicle.WheelSpeed);
          Wait(300);
        }
        else if (playerPed.IsInHeli)
        {
          float health = (currentVehicle.HeliEngineHealth / 1000f * currentVehicle.HeliMainRotorHealth / 1000f * currentVehicle.HeliTailRotorHealth / 1000f);
          float healthMalus = (int)((1f - health) * 4f);

          //GTA.Native.Hash.traction

          //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { (int)(6f -
          //    healthMalus), Math.Min(resistance + (int)healthMalus, 8) });
          //GTA.UI.Screen.ShowSubtitle(LerpInt(0, 6, health) + "-"+ LerpInt(8, 1, health) + "-" + health);

          DoTrigger_Resistance(Trigger.Right, 1 + (int)MathExtended.Lerp(0, 6, health),
            (int)MathExtended.Lerp(8, 1, health));

          DoTrigger_Resistance(Trigger.Left, 1 + (int)MathExtended.Lerp(0, 6, health),
            (int)MathExtended.Lerp(8, 1, health));
        }
        else if (playerPed.IsInBoat && currentVehicle.IsInWater)
        {
          float engineHealthFloat = Math.Min(1, currentVehicle.EngineHealth / 1000f);

          float initialDriveForce = MathExtended.InverseLerp(0.1f, 0.4f, currentVehicle.HandlingData.InitialDriveForce); // most cars 0.1f < df < 0.4f
          float driveInertia = MathExtended.InverseLerp(0.3f, 1.0f, currentVehicle.HandlingData.DriveInertia);

          float startOfGear = 0.8f;
          startOfGear *= MathExtended.Lerp(0.7f, 1f, initialDriveForce);
          startOfGear *= MathExtended.Lerp(0.5f, 1f, engineHealthFloat);

          //startOfGear*= Lerp(0.4f, 1f, currentVehicle.Clutch);

          float lightnessVehicle = 0.9f;
          lightnessVehicle *= MathExtended.Lerp(0.6f, 1f, driveInertia);
          lightnessVehicle *= MathExtended.Lerp(0.3f, 0.8f, currentVehicle.CurrentRPM);
          lightnessVehicle *= engineHealthFloat;
          //lightnessVehicle *= Lerp(0.2f, 1f, currentVehicle.Clutch);

          float brakeForce = MathExtended.InverseLerp(0.2f, 1.2f, currentVehicle.HandlingData.BrakeForce); // Bigger number = harder braking 0.01 - 2.0 and above. 1.0 uses brake force calculation unmodified.

          float startOfResistanceBrake = 1f * MathExtended.Lerp(0.4f, 1f, brakeForce);
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
            DoTrigger_CustomRigid(Trigger.Right, 

              (int)MathExtended.Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear),
              (int)MathExtended.Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lightnessVehicle),
              255);
            //GTA.UI.Screen.ShowSubtitle(spinnie.ToString("N2") + " - "+ (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear) + " - " + (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, vehicleLightness));

            DoTrigger_CustomRigid(Trigger.Left, 
              (int)MathExtended.Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfResistanceBrake),
              (int)MathExtended.Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lighnessBrake),
              255);

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
            DoTrigger_CustomRigid(Trigger.Left,
              (int)MathExtended.Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear),
              (int)MathExtended.Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lightnessVehicle),
              255);

            DoTrigger_CustomRigid(Trigger.Right, 
              (int)MathExtended.Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfResistanceBrake),
              (int)MathExtended.Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lighnessBrake),
              255);
          }
        }
        else if (playerPed.IsInPlane)
        {
          DoTrigger_Resistance(Trigger.Right, 5, 1 );
          DoTrigger_Resistance(Trigger.Left, 5, 1 );
        }
        else if (currentVehicle.IsInAir || !currentVehicle.IsOnAllWheels)
        {
          SetAndSendPacket(Trigger.Right);
          SetAndSendPacket(Trigger.Left);
        }
        else if (currentVehicle.EngineHealth <= 0f)
        {
          SetAndSendPacket(Trigger.Right, TriggerMode.Hardest);
          SetAndSendPacket(Trigger.Left, TriggerMode.Hardest);
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
          if (currentGear > 1)
          {
            engineIdleRpm = 0.2f + 0.025f * currentVehicle.HandlingData.ClutchChangeRateScaleUpShift * currentGear;
          }

          float currentRPMRatio = MathExtended.InverseLerp(engineIdleRpm, 1f, currentRPM);

          //GTA.UI.Screen.ShowSubtitle(currentVehicle.HandlingData.ClutchChangeRateScaleUpShift + " - " + engineIdleRpm + " - " + currentRPMRatio);

          //float currentRPMRatio = MathExtended.InverseLerp(0.2f + 0.6f * (Math.Max(0, currentVehicle.CurrentGear - 1)) / currentVehicle.HighGear, 1f, currentRPM);
          //(currentRPM - engineIdleRpm) / engineRange;
          // float currentSpeed = currentVehicle.Speed;
          // float maxSpeed = Function.Call<float>(Hash.GET_VEHICLE_ESTIMATED_MAX_SPEED, currentVehicle.Handle);

          // GTA.UI.Screen.ShowSubtitle(currentVehicle.HandlingData.ClutchChangeRateScaleDownShift + " - " + currentVehicle.HandlingData.ClutchChangeRateScaleUpShift);

          float initialDriveForce = MathExtended.InverseLerp(0.1f, 0.4f, currentVehicle.HandlingData.InitialDriveForce); // most cars 0.1f < df < 0.4f
          float driveInertia = MathExtended.InverseLerp(0.3f, 1.0f, currentVehicle.HandlingData.DriveInertia);

          float gearForce = MathExtended.InverseLerp(currentVehicle.HighGear, 1, currentGear);

          float spinnie = 1f;
          if (currentVehicle.Speed > 0)
          {
            spinnie = Math.Max(1, currentVehicle.WheelSpeed / currentVehicle.Speed);
          }

          float startOfGear = 1f;
          startOfGear *= MathExtended.Lerp(0.7f, 1f, initialDriveForce);
          startOfGear *= MathExtended.Lerp(0.3f, 1f, engineHealthFloat);
          startOfGear *= MathExtended.Lerp(1f, 0.7f, gearForce);
          startOfGear *= MathExtended.Lerp(0.6f, 1f, driveInertia);
          startOfGear *= spinnie;

          //startOfGear*= Lerp(0.4f, 1f, currentVehicle.Clutch);

          float lightnessVehicle = 1f;
          lightnessVehicle *= MathExtended.Lerp(1f, 0.8f, gearForce);
          lightnessVehicle *= MathExtended.Lerp(0.6f, 1f, currentRPMRatio);
          lightnessVehicle *= engineHealthFloat;
          //lightnessVehicle *= Lerp(0.2f, 1f, currentVehicle.Clutch);

          float brakeForce = MathExtended.InverseLerp(0.2f, 1.2f, currentVehicle.HandlingData.BrakeForce); // Bigger number = harder braking 0.01 - 2.0 and above. 1.0 uses brake force calculation unmodified.

          float startOfResistanceBrake = 1f * MathExtended.Lerp(0.4f, 1f, brakeForce);
          startOfResistanceBrake *= engineHealthFloat;

          float lighnessBrake = 1f * MathExtended.Lerp(0.5f, 1f, gearForce);
          lighnessBrake *= engineHealthFloat;

          /*
          0xA7F04022
          // GetVehicleWheelSurfaceMaterial
          int GET_VEHICLE_WHEEL_SURFACE_MATERIAL(Vehicle vehicle, int wheelIndex);
          */

          if (currentGear != currentVehicle.NextGear) // here comes the clutch!
          {
            SetAndSendPacket(Trigger.Left);
            SetAndSendPacket(Trigger.Right);
            Script.Wait(100);
          }
          else if (currentGear > 0)
          {
            DoTrigger_CustomRigid(Trigger.Right, 
              (int)MathExtended.Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear),
               (int)MathExtended.Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lightnessVehicle),
              255
            );
            //GTA.UI.Screen.ShowSubtitle(spinnie.ToString("N2") + " - "+ (int)Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear) + " - " + (int)Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, vehicleLightness));

            DoTrigger_CustomRigid(Trigger.Left,
              (int)MathExtended.Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfResistanceBrake),
              (int)MathExtended.Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lighnessBrake),
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
            DoTrigger_CustomRigid(Trigger.Left, 
              (int)MathExtended.Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfGear),
              (int)MathExtended.Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lightnessVehicle),
              255);

            DoTrigger_CustomRigid(Trigger.Right, 
              (int)MathExtended.Lerp(controllerConfig.startofResistanceVehicle, controllerConfig.endofResistanceVehicle, startOfResistanceBrake),
              (int)MathExtended.Lerp(controllerConfig.maxResistanceVehicle, controllerConfig.minResistanceVehicle, lighnessBrake),
              255);
          }

          // GTA.UI.Screen.ShowSubtitle(startOfResistance.ToString("N2") +" - " + amountOfForce.ToString("N2"));
        }
      }
      else
      {
        Weapon playerWeapon = playerPed.Weapons.Current;


        if (playerWeapon == null) return;

        if (playerWeapon != lastWeapon )
        {
          WeaponHudStats stats = new();
          Hans.GetTheWeaponHudStats((uint)playerWeapon.Hash, ref stats);

          lastWeapon = playerWeapon;
          currentAccuracy = stats.hudAccuracy / 100f;
          currentDamage = stats.hudDamage;
          weaponStrength = CurrentWeaponDamageNormalized(playerWeapon.Hash);

          // GTA.UI.Screen.ShowSubtitle(stats.hudAccuracy + " - " + stats.hudRange);
        }


        float animationLength = Function.Call<float>(Hash.GET_WEAPON_TIME_BETWEEN_SHOTS, playerWeapon.Hash);
        bool weaponHasNoAmmo =
          playerWeapon.Group == WeaponGroup.Unarmed || playerWeapon.Group == WeaponGroup.PetrolCan;

        if (playerPed.IsReloading || playerWeapon.AmmoInClip == 0 && !weaponHasNoAmmo)
        {
          if (lastAmmo == 1 && playerWeapon.AmmoInClip == 0) // recoil!
          {
            // DoTriggerCustomRigid(Trigger.Right, startOfResistance: 0, amountOfForceExerted: 128, forceExertedInRange: 128);
            DoTrigger_CustomRigid(Trigger.Right, 0, (int)MathExtended.Lerp(2, 255, currentAccuracy),
              (int)MathExtended.Lerp(32, 255, weaponStrength));
            Wait((int)(animationLength * 1000f));
          }
          else
          {
            DoTrigger_Bow(Trigger.Right, 5, 6, 4, 4 );
          }

          SetAndSendPacket(Trigger.Left);
          // Wait(50);
          lastAmmo = playerWeapon.AmmoInClip;

          return;
        }

        //int weaponStrength = Math.Min(8, (int)(weaponDamage / 6f));

        int fireRateAutomaticInt = (int)(1 / animationLength);

        int triggerForce = (int)MathExtended.Lerp(1, 3, currentAccuracy);
        int triggerSnapForce = (int)MathExtended.Lerp(1, 3, weaponStrength);
        switch (playerWeapon.Group)
        {
          case WeaponGroup.Pistol:
          case WeaponGroup.SMG:
          case WeaponGroup.AssaultRifle:
          case WeaponGroup.MG:
          case WeaponGroup.Sniper:
          case WeaponGroup.Shotgun:
          {
            ApplyAutomaticWeaponHandling(playerPed, weaponStrength, fireRateAutomaticInt, animationLength, 1, 5, triggerForce, triggerSnapForce);

            break;
          }
          case WeaponGroup.Heavy:
          {
            if (playerWeapon.Hash == WeaponHash.Minigun)
            {
              ApplyAutomaticWeaponHandling(playerPed, 0.9f, (int)(fireRateAutomaticInt / 5f), animationLength, 1, 5, 1, 1);
            }
            else
            {
              ApplyAutomaticWeaponHandling(playerPed, 0.6f, 1,
                animationLength, 1, 5, 1, 1);
              break;
            }

            break;
          }
          default:
          case WeaponGroup.Unarmed:
          case WeaponGroup.Melee:
          case WeaponGroup.Thrown:
          case WeaponGroup.PetrolCan:
            DoTrigger_Resistance(Trigger.Left, (int)MathExtended.Lerp(7, 4, weaponStrength), (int)MathExtended.Lerp(1, 4, weaponStrength) );
            DoTrigger_Resistance(Trigger.Right, (int)MathExtended.Lerp(7, 3, weaponStrength), (int)MathExtended.Lerp(1, 8, weaponStrength) );
            //GTA.UI.Screen.ShowSubtitle(lastAmmo.ToString());

            // SetAndSendPacket(packet, Trigger.Left);
            // SetAndSendPacket(packet, Trigger.Right);
            break;
        }
      }
    }
  private static void ApplyAutomaticWeaponHandling(Ped playerPed, float weaponStrength,
  int fireRateAutomaticInt, float timeBetweenShots, int start, int end, int force, int snapForce)
  {
    bool doDefault = false;
    int ammoInClip = playerPed.Weapons.Current.AmmoInClip;
    int iMax = 20;
      float accuracy = currentAccuracy;
      if (playerPed.Weapons.Current.Hash != WeaponHash.Minigun)
      {
        DoTrigger_Resistance(Trigger.Left, (int)MathExtended.Lerp(7, 5, accuracy), (int)MathExtended.Lerp(1, 4, weaponStrength) );
      }

      if (playerPed.IsShooting || lastAmmo != ammoInClip)
      {

      //  Wait(10);

      lastAmmo = ammoInClip;
      if (fireRateAutomaticInt > 2) // recoil in general
      {
        DoTrigger_AutomaticGun(Trigger.Right, 0, (int)MathExtended.Lerp(5, 8, weaponStrength), fireRateAutomaticInt);
      }
      // recoil for low firing frequency
      else 
      {
          //GTA.UI.Screen.ShowSubtitle((int)MathExtended.Lerp(8, 255, weaponStrength) + " - "+ (int)MathExtended.Lerp(8, 196, accuracy));

          DoTrigger_CustomRigid(Trigger.Right, startOfResistance: 0,  (int)MathExtended.Lerp(8, 196, weaponStrength), (int)MathExtended.Lerp(8, 196, accuracy));
        // Wait(100);
        return;
      }

      if (playerPed.Weapons.Current.Hash == WeaponHash.Minigun)
      {
        Wait(50);
        spinning = true;
      }
      else
      {
        Wait((int)(timeBetweenShots * 1000f));
        spinning = false;
      }

      return;
    }

    if (playerPed.Weapons.Current.Hash == WeaponHash.Minigun) // not ahooting, only spinning
    {
      bool isAttackPressed = Game.IsControlPressed(Control.Attack);
      if (playerPed.IsAiming || isAttackPressed)
      {
        bool isWeaponReadyToShoot = Function.Call<bool>(Hash.IS_PED_WEAPON_READY_TO_SHOOT, playerPed.Handle);


        if (isWeaponReadyToShoot)
        {
          DoTrigger_Machine(Trigger.Left, 1, 9, 1, 3, iMax, 100);
          DoTrigger_Machine(Trigger.Right, 1, 9, 1, 3, iMax, 100);

          //SetAndSendPacket(packet, Trigger.Left, TriggerMode.Machine,
          //  new() { 1, 9, 1, 3, iMax, 100 });
          //SetAndSendPacket(packet, Trigger.Right, TriggerMode.Machine,
          //  new() { 1, 9, 1, 2, iMax, 100 });
        }
        else 
        {
          // if (!spinning)
          {
            int iii = iMax;
            while (iii > 0)
            {
              DoTrigger_Machine(Trigger.Left, 1, 9, 1, 3, iMax-iii, 100);
              DoTrigger_Machine(Trigger.Right, 1, 9, 1, 3, iMax-iii, 100);
              iii--;
              Wait((int)(iii * (isAttackPressed ? 1f : 4f)));

            }
            for (int i = 1; i < iMax; i++)
            {
            }

            spinning = true;
          }
        }
      }
      else
      {
        //SetAndSendPacket(packet, Trigger.Left, TriggerMode.Machine,
        //  new() { 2, 9, 1, 2, iMax, 1 });

          SetAndSendPacket(Trigger.Left);
        SetAndSendPacket(Trigger.Right);

        //SetAndSendPacket(packet, Trigger.Left, TriggerMode.Machine,
        //  new() { 4, 9, 1, 2, 0, 1 });
        //SetAndSendPacket(packet, Trigger.Right, TriggerMode.Machine,
        //  new() { 4, 9, 1, 2, 0, 1 });
      }
      // if (Game.IsControlPressed(Control.Attack) && !spinning)
      // {
      //   for (int i = 1; i < iMax; i++)
      //   {
      //     SetAndSendPacket(packet, Trigger.Right, TriggerMode.Machine,
      //       new() { 1, 9, 1, 2, i, 1 });
      //     Wait(10);
      //   }
      //
      //   spinning = true;
      // }
      // else
      // {
      //   SetAndSendPacket(packet, Trigger.Right, TriggerMode.Machine,
      //     new() { 1, 9, 1, 2, iMax, 1 });
      //   spinning = false;
      // }
    }
    else
    {
      DoTrigger_Bow(Trigger.Right, start, end, force, snapForce);
    }
    lastAmmo = ammoInClip;

    Wait(50);

    //Script.Wait(fireRateAutomaticInt);
  }
  }

}