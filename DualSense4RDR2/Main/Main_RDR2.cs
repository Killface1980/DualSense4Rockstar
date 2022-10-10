using DSX_Base;
using RDR2;
using RDR2.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static DSX_Base.Client.iO;
using static RDR2.Native.WEAPON;

namespace DualSense4RDR2
{
  public class Main_RDR2 : Script
  {
    private static Ped playerPed;
    private static bool wanted = false;
    private readonly Dictionary<uint, Weapon> playerWeapons = new Dictionary<uint, Weapon>();
    private int brig;
    private int lastMainHandAmmo;
    private int lastOffHandAmmo;

    //private float pulseRate = 0;
    //    public static readonly ControllerConfig controllerConfig = new();

    private bool mainHandWillShootNext;

    private bool offHandWillShootNext;

    public Main_RDR2()
    {
      playerPed = Game.Player.Character;
      Tick += this.OnTick;
      Connect();
      Process.GetProcessesByName("DSX");
    }

    private static bool ShouldBehaveLikeDoubleAction(Weapon weapon)
    {
      switch (weapon.Hash)
      {
        case eWeapon.WEAPON_REVOLVER_DOUBLEACTION:
        case eWeapon.WEAPON_REVOLVER_DOUBLEACTION_EXOTIC:
        case eWeapon.WEAPON_REVOLVER_DOUBLEACTION_GAMBLER:
        case eWeapon.WEAPON_REVOLVER_DOUBLEACTION_MICAH:
        case eWeapon.WEAPON_PISTOL_MAUSER:
        case eWeapon.WEAPON_PISTOL_MAUSER_DRUNK:
        case eWeapon.WEAPON_PISTOL_SEMIAUTO:
        case eWeapon.WEAPON_PISTOL_M1899:
        case eWeapon.WEAPON_SHOTGUN_SEMIAUTO:
          return true;
      }

      return false;
    }

    private unsafe Weapon GetCurrentWeapon(bool offHand = false)
    {
      Ped owner = Game.Player.Character;
      uint weaponHash;
      GET_CURRENT_PED_WEAPON(owner!.Handle, &weaponHash, true,
        offHand ?
          (int)eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_SECONDARY :
        (int)eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_PRIMARY, false);

      //int weaponEntityIndex = GET_CURRENT_PED_WEAPON_ENTITY_INDEX(playerPed.Handle, (int)attachPoint);


      //uint weaponHash = WEAPON._GET_PED_CURRENT_HELD_WEAPON(owner.Handle);
      if (playerWeapons!.ContainsKey(weaponHash))
      {
        return playerWeapons[weaponHash];
      }

      Weapon weapon = new(owner, (eWeapon)weaponHash);
      playerWeapons.Add(weaponHash, weapon);
      return weapon;
    }

    private unsafe void OnTick(object sender, EventArgs e)
    {
      Packet packet                = new();
      packet.instructions          = new Instruction[4];
      Player player                = Game.Player;
      playerPed                    = player.Character;
      Weapon currentMainHandWeapon = GetCurrentWeapon();// playerPed?.Weapons?.Current;
      Weapon currentOffHandWeapon  = GetCurrentWeapon(true);
      //playerPed.Weapons.CurrentWeaponEntity.
      //if ((uint)CurrentOffHand.Hash == 0xA2719263) // unarmed
      {
        //RDR2.UI.Screen.DisplaySubtitle((CurrentOffHand.Hash).ToString());
      }

      // SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 1 });
      // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 5, 8, 8 });

      bool mainWeaponIsReadyToShoot = IS_PED_WEAPON_READY_TO_SHOOT(playerPed.Handle);
      bool weaponIsAGun             = IS_WEAPON_A_GUN((uint)currentMainHandWeapon.Hash); //IS_WEAPON_A_GUN
      bool weaponIsThrowable        = _IS_WEAPON_THROWABLE((uint)currentMainHandWeapon.Hash); //
      bool mainWeaponIsTwoHanded    = _IS_WEAPON_TWO_HANDED((uint)currentMainHandWeapon.Hash);
      bool carriesWeaponOpenly      = currentMainHandWeapon.Hash != eWeapon.WEAPON_UNARMED; /* unarmed*/
      bool hasOffHandWeapon         = currentOffHandWeapon.Hash != eWeapon.WEAPON_UNARMED;
      //uint* numbi                 = null;
      //var mount                   = GET_CURRENT_PED_VEHICLE_WEAPON(characterPed.Handle, numbi); //

      int ammoL = currentOffHandWeapon.AmmoInClip;
      int ammoR = currentMainHandWeapon.AmmoInClip;

      if (currentMainHandWeapon.Hash == currentOffHandWeapon.Hash)
      {
        ulong weaponGuid_L;
        WEAPON.GET_PED_WEAPON_GUID_AT_ATTACH_POINT(playerPed.Handle, (int)eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_SECONDARY, &weaponGuid_L);
        int ammoPointer;
        WEAPON._GET_AMMO_IN_CLIP_BY_INVENTORY_UID(playerPed.Handle, &ammoPointer, &weaponGuid_L);

        ammoL = ammoPointer;

        ulong weaponGuid_R;
        WEAPON.GET_PED_WEAPON_GUID_AT_ATTACH_POINT(playerPed.Handle, (int)eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_PRIMARY, &weaponGuid_R);
        //WEAPON.GET_PED_WEAPON_GUID_AT_ATTACH_POINT(playerPed.Handle, (int)eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_PRIMARY, &weaponGuid_R);
        //int ammo_R = 0;
        WEAPON._GET_AMMO_IN_CLIP_BY_INVENTORY_UID(playerPed.Handle, &ammoPointer, &weaponGuid_R);
        // WEAPON._GET_AMMO_IN_CLIP_BY_INVENTORY_UID(playerPed.Handle, &ammo_R, &weaponGuid_R);
        ammoR = ammoPointer;
      }

      bool hasMountedWeapon = playerPed?.Weapons?.Current?.Group == 0 && playerPed.IsSittingInVehicle();

      bool isMounted = hasMountedWeapon && playerPed?.CurrentVehicle != null;

      uint weaponHash = 0;

      bool currentPedVehicleWeapon = GET_CURRENT_PED_VEHICLE_WEAPON(playerPed.Handle, &weaponHash);
      // RDR2.UI.Screen.DisplaySubtitle(currentMainHandWeapon.AmmoInClip + " - " + currentOffHandWeapon.AmmoInClip);

      // RDR2.UI.Screen.DisplaySubtitle(hasOffHandWeapon.ToString());

      if (isMounted)
      {
      }
      // return;
      bool isAimingControlPressed = /*PAD.IS_CONTROL_PRESSED(0, (uint)eInputType.INPUT_FRONTEND_LT) ||*/ player.IsAiming; ////

      //RDR2.UI.Screen.DisplaySubtitle(weaponIsAGun.ToString());
      // return;

      //RDR2.UI.Screen.DisplaySubtitle(playerweapon.Group.ToString());
      //RDR2.UI.Screen.DisplaySubtitle((playerPed.IsReloading).ToString());
      int ammo;

      // Weapon Wheel
      if (PAD.IS_CONTROL_PRESSED(0, (uint)eInputType.INPUT_FRONTEND_LB))  //INPUT_FRONTEND_LB, weapon wheel
      {
        SetAndSendPacket(packet, Trigger.Right);
        SetAndSendPacket(packet, Trigger.Left);
      }
      // no gun accidents
      else if (playerPed.IsReloading) // Mode reloading
      {
        // SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);
        
        //SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);
        SetAndSendPacketCustom(packet, Trigger.Right, CustomTriggerValueMode.Rigid, startOfResistance: 120, amountOfForceExerted: 255, forceExertedInRange: 255);

        if (lastMainHandAmmo != ammoR ||
            hasOffHandWeapon && lastOffHandAmmo != ammoL)
        {
          SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid, startOfResistance: 0, amountOfForceExerted: 255, forceExertedInRange: 255);
         // SetAndSendPacket(packet, Trigger.Right);

          // SetAndSendPacketCustom(packet, Trigger.Right, CustomTriggerValueMode.Rigid, startOfResistance: 0, amountOfForceExerted: 255, forceExertedInRange: 255);

          //SetAndSendPacket(packet, Trigger.Left, TriggerMode.Hardest);
          //SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);
          // SetAndSendPacket(packet, Trigger.Left, TriggerMode.AutomaticGun, new() { 2, 6, 4 });
          // SetAndSendPacket(packet, Trigger.Right, TriggerMode.AutomaticGun, new() { 2, 6, 4 });
          // Wait(100);
        }
        else
        {
          //SetAndSendPacket(packet, Trigger.Right);
          SetAndSendPacket(packet, Trigger.Left);
          Wait(20);
        }

        mainHandWillShootNext = true;
        offHandWillShootNext = false;

        lastMainHandAmmo = ammoR;
        lastOffHandAmmo = ammoL;

        // SetAndSendPacket(packet, Trigger.Right, TriggerMode.AutomaticGun,new(){ 0,8,3 });
      }
      else if (currentPedVehicleWeapon)
      {
        //RDR2.UI.Screen.DisplaySubtitle((number).ToString());

        if (weaponHash == 3666182381 || //gat
            weaponHash == 3101324918)// maxi
        {
          SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid, startOfResistance: 1, amountOfForceExerted: 20);
          if (playerPed.IsShooting) // Auto
          {
            SetAndSendPacket(packet, Trigger.Right,
              TriggerMode.AutomaticGun, new() { 0, 4, 30 });
            Script.Wait(100);
          }
          else // Prepare
          {
            SetAndSendPacketCustom(packet, Trigger.Right,
              CustomTriggerValueMode.Rigid, startOfResistance: 30, amountOfForceExerted: 128);
          }
        }
        else if (weaponHash == 2465730487 || //hotch - cannons
                 weaponHash == 1609145491)// breach
        {
          SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid, startOfResistance: 1, amountOfForceExerted: 20);
          SetAndSendPacketCustom(packet, Trigger.Right,
            CustomTriggerValueMode.PulseA, startOfResistance: 255, amountOfForceExerted: 200, forceExertedInRange: 255);
        }
      }
      else if (weaponIsThrowable || currentMainHandWeapon.Group == eWeaponGroup.GROUP_BOW || currentMainHandWeapon.Group == eWeaponGroup.GROUP_FISHINGROD || currentMainHandWeapon.Group == eWeaponGroup.GROUP_LASSO)
      {
        if (currentMainHandWeapon.Group == eWeaponGroup.GROUP_FISHINGROD)
        {
          // ulong currentState;
          // TASK._GET_TASK_FISHING(playerPed.Handle, &currentState); // BUG Crashing https://pastebin.com/NmK5ZLVs

          SetAndSendPacket(packet, Trigger.Left, TriggerMode.Resistance, new() { 6, 1 });

          SetAndSendPacket(packet, Trigger.Right, TriggerMode.Resistance, new() { 4, 2 });
        }
        else if (PED._GET_LASSO_TARGET(playerPed.Handle) != 0)
        {
          SetAndSendPacket(packet, Trigger.Left, TriggerMode.Hardest);
          SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);
        }
        /*        else if (player.IsTargettingAnything) // not working on the lasso
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 2, 8 });

                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 6, 8,2,3 });
                }
        */
        else if (player.IsAiming)
        {
          SetAndSendPacket(packet, Trigger.Left, TriggerMode.Resistance, new() { 6, 4 });

          SetAndSendPacket(packet, Trigger.Right, TriggerMode.Resistance, new() { 1, 6 });
          // RDR2.UI.Screen.DisplaySubtitle("aimy");
        }
        else
        {
          SetAndSendPacket(packet, Trigger.Left, TriggerMode.Resistance, new() { 6, 1 });

          SetAndSendPacket(packet, Trigger.Right, TriggerMode.Resistance, new() { 4, 2 });
          // RDR2.UI.Screen.DisplaySubtitle("fishing");
        }
      }
      else
      {
        bool isPedDuelling = TASK._IS_PED_DUELLING(player.Handle);
        bool isDeadEyeEnabled = player.IsInDeadEye || player.IsInEagleEye;
        if (weaponIsAGun || isPedDuelling || isDeadEyeEnabled)
        {
          //SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new(){6,1});
          eWeaponAttachPoint attachPoint;

          if (hasOffHandWeapon && offHandWillShootNext) // most likely offhand
          {
            attachPoint = eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_SECONDARY;
          }
          else
          {
            attachPoint = eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_PRIMARY;
          }

          int weaponEntityIndex = GET_CURRENT_PED_WEAPON_ENTITY_INDEX(playerPed.Handle, (int)attachPoint);
          float degradation = GET_WEAPON_DEGRADATION(weaponEntityIndex);

          // float permanentDegradation = GET_WEAPON_PERMANENT_DEGRADATION(weaponEntityIndex);
          //

          // 279042 - double action
          bool mainHandIsDoubleAction = ShouldBehaveLikeDoubleAction(currentMainHandWeapon);
          bool offHandIsDoubleAction = ShouldBehaveLikeDoubleAction(currentOffHandWeapon);

          //if (playerPed.IsShooting)
          if (playerPed.IsShooting)
          {
            // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new (){0,8, 2} );
            //RDR2.UI.Screen.DisplaySubtitle(ammoL + "- " + ammoR);// + " | " + weaponGuid_L + " - "+ weaponGuid_R);

            Wait(10);

            //SetAndSendPacket(packet, Trigger.Left, TriggerMode.Hardest);
            if (!hasOffHandWeapon || (ammoR < lastMainHandAmmo))
            {
              // RDR2.UI.Screen.DisplaySubtitle("main");

              SetAndSendPacketCustom(packet, Trigger.Right, CustomTriggerValueMode.Rigid, startOfResistance: 0, amountOfForceExerted: 255, forceExertedInRange: 255);

              if (isAimingControlPressed)
              {
                //    SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid, startOfResistance: 0, amountOfForceExerted: 40, forceExertedInRange: 40);
              }

              if (hasOffHandWeapon && ammoL > 0)
              {
                mainHandWillShootNext = false;
                offHandWillShootNext = true;
              }
            }
            else if (hasOffHandWeapon && ammoL < lastOffHandAmmo)// && playerIsAiming)
            {
              //   RDR2.UI.Screen.DisplaySubtitle("off");

              if (isAimingControlPressed)
              {
                SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid, startOfResistance: 0, amountOfForceExerted: 255, forceExertedInRange: 255);
                //       SetAndSendPacketCustom(packet, Trigger.Right, CustomTriggerValueMode.Rigid, startOfResistance: 0, amountOfForceExerted: 40, forceExertedInRange: 40);
              }
              else
              {
                SetAndSendPacketCustom(packet, Trigger.Right, CustomTriggerValueMode.Rigid, startOfResistance: 0, amountOfForceExerted: 255, forceExertedInRange: 255);
              }
              if (ammoR > 0)
              {
                mainHandWillShootNext = true;
                offHandWillShootNext = false;
              }
            }
            lastMainHandAmmo = ammoR;
            lastOffHandAmmo = ammoL;

            Wait(150);
          }
          else if (!mainHandIsDoubleAction && (isAimingControlPressed && mainWeaponIsTwoHanded || !hasOffHandWeapon) && !mainWeaponIsReadyToShoot || isPedDuelling || !carriesWeaponOpenly) // Mode Gun Cock
          {
            SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid, startOfResistance: 140 - (int)(degradation * 110), amountOfForceExerted: (int)(100 + degradation * 120), 180);
            SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow, new()
            {
              5,
              7,
              (int)(2 + (6f * degradation)),
              (int)(2 + (6f * degradation))
            });

            // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 4, 1 + (int)(degradation * 3), 2 });
          }
          else // GUN_MANUAL;
          {
            // uint modelHash = WEAPON.enti(weaponEntityIndex);
            int triggerEnd = 1;
            float force1 = (2 + (6f * degradation));
            float force2 = (2 + (6f * degradation));

            bool doubleActionActive = false;

            if (mainHandIsDoubleAction)
            {
              if (!hasOffHandWeapon) { doubleActionActive = true; }
              else if (mainHandWillShootNext) { doubleActionActive = true; }
              // RDR2.UI.Screen.DisplaySubtitle("main is double");
            }

            if (offHandIsDoubleAction && !doubleActionActive)
            {
              if (offHandWillShootNext) { doubleActionActive = true; }
            }

            float factor = 1f;
            if (doubleActionActive) // harder triggers for double action
            {
              triggerEnd = 3;
              force1 = (4 + (4f * degradation));
              force2 = (4 + (4f * degradation));
              factor = 0.85f;
            }

            //RDR2.UI.Screen.DisplaySubtitle((doubleActionActive).ToString());
            SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid, startOfResistance: 180 - (int)(degradation * 120 * factor), amountOfForceExerted: (int)(100 + degradation * 120), 255);

            SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow, new()
            {
              0,
              (int)triggerEnd,
              (int)force1,
              (int)force2
            });

            //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 0, 4,1+(int)(degradation * 7), 4 });
          }
        }
        else // turn off
        {
          SetAndSendPacket(packet, Trigger.Right);
          SetAndSendPacket(packet, Trigger.Left);

          // SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, CustomTriggerValueMode.Rigid, 1, 20);
          // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 0, 2, 3, 4 });
        }
      }

      return;
      // updateLights();
      // health = Math.Min(health, 1);

      //RDR2.UI.Screen.DisplaySubtitle((myred).ToString());
      return;

      //else
    }
  }
}