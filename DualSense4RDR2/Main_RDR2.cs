using DSX_Base;
using RDR2;
using RDR2.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DSX_Base.MathExtended;
using JetBrains.Annotations;
using static DSX_Base.Client.iO;
using static RDR2.Native.WEAPON;
// using DualSense4RDR2.Config;

namespace DualSense4RDR2
{
  public class Main_RDR2 : Script
  {
    private readonly Dictionary<uint, Weapon> playerWeapons = new Dictionary<uint, Weapon>();
    private int lastMainHandAmmo;
    private int lastOffHandAmmo;

    //private float pulseRate = 0;
    //    public static readonly ControllerConfig controllerConfig = new();

    private bool mainHandWillShootNext;

    private bool offHandWillShootNext;

    public Main_RDR2()
    {
      Tick += this.OnTick;
      Connect();
      Process.GetProcessesByName("DSX");
      // var config = new ControllerConfig();
    }



    private static bool ShouldBehaveLikeDoubleAction(Weapon weapon)
    {
      return weapon.Hash 
        is eWeapon.WEAPON_REVOLVER_DOUBLEACTION 
        or eWeapon.WEAPON_REVOLVER_DOUBLEACTION_EXOTIC
        or eWeapon.WEAPON_REVOLVER_DOUBLEACTION_GAMBLER 
        or eWeapon.WEAPON_REVOLVER_DOUBLEACTION_MICAH
        or eWeapon.WEAPON_PISTOL_MAUSER 
        or eWeapon.WEAPON_PISTOL_MAUSER_DRUNK 
        or eWeapon.WEAPON_PISTOL_SEMIAUTO
        or eWeapon.WEAPON_PISTOL_M1899 
        or eWeapon.WEAPON_SHOTGUN_SEMIAUTO;
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
      if (playerWeapons.ContainsKey(weaponHash))
      {
        return playerWeapons[weaponHash];
      }

      Weapon weapon = new(owner, (eWeapon)weaponHash);
      playerWeapons.Add(weaponHash, weapon);
      return weapon;
    }

    private bool spinning;
    private Weapon GetVehicleWeaponHash(uint weaponHash)
    {
      if (playerWeapons!.ContainsKey(weaponHash))
      {
        return playerWeapons[weaponHash];
      }
      Ped owner = Game.Player?.Character;

      Weapon weapon = new(owner, (eWeapon)weaponHash);
      playerWeapons.Add(weaponHash, weapon);
      return weapon;

    }
    private bool cockingActive;
    int GetHorseSpeed(Ped horsie)
    {
      // stopped
      // walking 
      // faster, everything else
      // running
      // sprinting
      if (horsie.IsStopped) return 0;
      if (horsie.IsWalking) return 1;
      if (horsie.IsRunning) return 3;
      if (horsie.IsSprinting) return 4;
      return 2; // gallopping
    }
    private  void OnTick(object sender, EventArgs e)
    {

      Ped playerPed = Game.Player?.Character;
      Player player                = Game.Player;
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
      //bool mainWeaponIsTwoHanded    = _IS_WEAPON_TWO_HANDED((uint)currentMainHandWeapon.Hash);
      bool carriesWeaponOpenly      = currentMainHandWeapon.Hash != eWeapon.WEAPON_UNARMED; /* unarmed*/
      bool canUseOffhandWeapon         = currentMainHandWeapon.IsOneHanded && currentOffHandWeapon.IsOneHanded && currentOffHandWeapon.Hash != eWeapon.WEAPON_UNARMED;
      //uint* numbi                 = null;
      //var mount                   = GET_CURRENT_PED_VEHICLE_WEAPON(characterPed.Handle, numbi); //

      int ammoL = currentOffHandWeapon.AmmoInClip;
      int ammoR = currentMainHandWeapon.AmmoInClip;

      if (currentMainHandWeapon.Hash != eWeapon.WEAPON_UNARMED &&
          (currentMainHandWeapon.Hash == currentOffHandWeapon.Hash || currentOffHandWeapon.Hash == eWeapon.WEAPON_UNARMED)) // get the proper ammo amount
      {
        unsafe
        {
          ulong weaponGuid_L;
          GET_PED_WEAPON_GUID_AT_ATTACH_POINT(playerPed.Handle, (int)eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_SECONDARY, &weaponGuid_L);
          int ammoPointer;
          _GET_AMMO_IN_CLIP_BY_INVENTORY_UID(playerPed.Handle, &ammoPointer, &weaponGuid_L);

          ammoL = ammoPointer;

          ulong weaponGuid_R;
          GET_PED_WEAPON_GUID_AT_ATTACH_POINT(playerPed.Handle, (int)eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_PRIMARY, &weaponGuid_R);
          //WEAPON.GET_PED_WEAPON_GUID_AT_ATTACH_POINT(playerPed.Handle, (int)eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_PRIMARY, &weaponGuid_R);
          //int ammo_R = 0;
          _GET_AMMO_IN_CLIP_BY_INVENTORY_UID(playerPed.Handle, &ammoPointer, &weaponGuid_R);
          // WEAPON._GET_AMMO_IN_CLIP_BY_INVENTORY_UID(playerPed.Handle, &ammo_R, &weaponGuid_R);
          ammoR = ammoPointer;
        }
      }

      //bool hasMountedWeapon = playerPed?.Weapons?.Current?.Group == 0 && playerPed.IsSittingInVehicle();

      //bool isMounted = hasMountedWeapon && playerPed?.CurrentVehicle != null;


      // RDR2.UI.Screen.DisplaySubtitle(currentMainHandWeapon.AmmoInClip + " - " + currentOffHandWeapon.AmmoInClip);

      // RDR2.UI.Screen.DisplaySubtitle(hasOffHandWeapon.ToString());


      // return;
      bool isAimingControlPressed = PAD.IS_CONTROL_PRESSED(0, (uint)eInputType.INPUT_FRONTEND_LT) || player.IsAiming; 

      //RDR2.UI.Screen.DisplaySubtitle(weaponIsAGun.ToString());
      // return;

      //RDR2.UI.Screen.DisplaySubtitle(playerweapon.Group.ToString());
      //RDR2.UI.Screen.DisplaySubtitle((playerPed.IsReloading).ToString());

      // Weapon Wheel
      if (PAD.IS_CONTROL_PRESSED(0, (uint)eInputType.INPUT_FRONTEND_LB))  //INPUT_FRONTEND_LB, weapon wheel
      {
        DoTrigger_Resistance(Trigger.Left, 8, 1 );
        DoTrigger_Resistance(Trigger.Right,  8, 1 );
        return;
        //SetAndSendPacket(packet, Trigger.Right);
        //SetAndSendPacket(packet, Trigger.Left);
      }
      bool ignoreLeftTrigger = false;

      bool pedHasVehicleWeapon;
      uint weaponHash = 0;

      unsafe
      {
        pedHasVehicleWeapon = GET_CURRENT_PED_VEHICLE_WEAPON(playerPed.Handle, &weaponHash);
      }

      if (pedHasVehicleWeapon)
      {
        switch (weaponHash)
        {
          case 2465730487: //hotch - cannons
          case 1609145491: // breach
            {
            if (playerPed.IsShooting)
            {
              DoTrigger_CustomRigid(Trigger.Left, 0, 255, 255);
              DoTrigger_CustomRigid(Trigger.Right, 0, 255, 255);
              Wait(800);
            }
            else
            {
              /*            DoTriggerCustomRigid(Trigger.Left, startOfResistance: 1, amountOfForceExerted: 20);*/
              SetAndSendPacket(Trigger.Left, TriggerMode.Choppy);
              SetAndSendPacket(Trigger.Right, TriggerMode.Choppy);
              /*            SetAndSendPacketCustom(packet, Trigger.Right,
              CustomTriggerValueMode.PulseA, startOfResistance: 255, amountOfForceExerted: 200, forceExertedInRange: 255);*/
            }

            break;
          }
          case 3666182381: // gat
          case 3101324918: // maxi
            {
              int iMax = 20;
              //bool isWeaponReadyToShoot = IS_PED_WEAPON_READY_TO_SHOOT(playerPed.Handle);
              if (playerPed.IsShooting) // Auto
              {
                DoTrigger_AutomaticGun(Trigger.Right, 0, 8, iMax);
                DoTrigger_AutomaticGun(Trigger.Left, 0, 8, iMax);
                Wait(100);
                spinning = false;
              }
              else
              {
                /*            if (PAD.IS_CONTROL_PRESSED(0, (uint)eInputType.INPUT_VEH_ATTACK))
                            {
                              if (!spinning)
                              {
                                Wait(200);
                                for (int i = iMax; i > 0; i--)
                                {
                                  SetAndSendPacket(packet, Trigger.Left, TriggerMode.Machine,
                                    new() { 1, 9, 4, 5, iMax - i, 100 });
                                  SetAndSendPacket(packet, Trigger.Right, TriggerMode.Machine,
                                    new() { 1, 9, 4, 5, iMax - i, 100 });
                                  i--;
                                  Wait((int)(i * 40f));
                                  RDR2.UI.Screen.DisplaySubtitle((i.ToString()));

                                }


                                spinning = true;
                              }
                              else
                              {
                                SetAndSendPacket(packet, Trigger.Left, TriggerMode.Machine,
                                  new() { 1, 9, 1, 2, iMax , 100 });
                                SetAndSendPacket(packet, Trigger.Right, TriggerMode.Machine,
                                  new() { 1, 9, 1, 2, iMax , 100 });

                              }
                            }
                            else // Prepare
                */
                {
                  spinning = false;
                  SetAndSendPacket(Trigger.Left, TriggerMode.Choppy);
                  SetAndSendPacket(Trigger.Right, TriggerMode.Choppy);

                  /*              SetAndSendPacketCustom(packet, Trigger.Left,
                    CustomTriggerValueMode.Rigid, startOfResistance: 30, amountOfForceExerted: 128);
                  SetAndSendPacketCustom(packet, Trigger.Right,
                    CustomTriggerValueMode.Rigid, startOfResistance: 30, amountOfForceExerted: 128);
    */
                }
              }

              break;
            }
        }
        return;

        //RDR2.UI.Screen.DisplaySubtitle((weaponHash + " - " + GetVehicleWeaponHash(weaponHash).GetHashCode()));

      }
      if (weaponIsThrowable || currentMainHandWeapon.Group is eWeaponGroup.GROUP_BOW or eWeaponGroup.GROUP_FISHINGROD or eWeaponGroup.GROUP_LASSO)
      {
        if (currentMainHandWeapon.Group == eWeaponGroup.GROUP_FISHINGROD)
        {
           // ulong currentState;
          // TASK._GET_TASK_FISHING(playerPed.Handle, &currentState); // BUG Crashing https://pastebin.com/NmK5ZLVs
          // TASK._GET_TASK_FISHING(playerPed.Handle, (ulong*)((IntPtr)(&currentState)).ToInt32()); // BUG Crashing https://pastebin.com/NmK5ZLVs

          // Hans.FishTaskState state = Hans.GetFishTaskState();
          // RDR2.UI.Screen.DisplaySubtitle(state.FishingState.ToString());

          //state.FishingState == (int)FishingStates.Caught_fish_holding_in_hand

          DoTrigger_Resistance(Trigger.Left, 6, 1 );

          DoTrigger_Resistance(Trigger.Right, 4, 2 );
        }
        else if (PED._GET_LASSO_TARGET(playerPed.Handle) != 0) // caught someone/-thing
        {
          if (!ignoreLeftTrigger) SetAndSendPacket(Trigger.Left, TriggerMode.Hardest);
          SetAndSendPacket(Trigger.Right, TriggerMode.Hardest);
        }
        /*        else if (player.IsTargettingAnything) // not working on the lasso
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 2, 8 });

                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 6, 8,2,3 });
                }
        */
        else if (player.IsAiming)
        {
          if (!ignoreLeftTrigger) DoTrigger_Resistance(Trigger.Left, 6, 4 );

          DoTrigger_Resistance(Trigger.Right, 1, 6 );
          // RDR2.UI.Screen.DisplaySubtitle("aimy");
        }
        else
        {
          if (!ignoreLeftTrigger) DoTrigger_Resistance(Trigger.Left, 6, 1 );

          DoTrigger_Resistance(Trigger.Right, 4, 2 );
          // RDR2.UI.Screen.DisplaySubtitle("fishing");
        }

        return;
      }

      bool isPedDueling = TASK._IS_PED_DUELLING(player.Handle);
      bool isDeadEyeEnabled = player.IsInDeadEye || player.IsInEagleEye;
        
      if (playerPed.IsReloading) // Mode reloading
      {
        // SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);

        //SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);
        //RDR2.UI.Screen.DisplaySubtitle(lastMainHandAmmo + " - " +  ammoR + " - " + currentOffHandWeapon.Hash + " - " + ammoL);

        if (lastMainHandAmmo != ammoR && canUseOffhandWeapon)
        {
          DoTrigger_CustomRigid(Trigger.Right, 0, 112, 255);
          Wait(75);

        }
        else if (lastMainHandAmmo != ammoR || lastOffHandAmmo != ammoL)
        {
          // DoTrigger_CustomRigid(Trigger.Right, 0, 128, 255);
          DoTrigger_CustomRigid(Trigger.Left, 0, 112, 255);
          Wait(75);
          // SetAndSendPacket(packet, Trigger.Right);

          // DoTriggerCustomRigid(Trigger.Right, startOfResistance: 0, amountOfForceExerted: 255, forceExertedInRange: 255);

          //SetAndSendPacket(packet, Trigger.Left, TriggerMode.Hardest);
          //SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);
          // SetAndSendPacket(packet, Trigger.Left, TriggerMode.AutomaticGun, new() { 2, 6, 4 });
          // SetAndSendPacket(packet, Trigger.Right, TriggerMode.AutomaticGun, new() { 2, 6, 4 });

        }
        else
        {
          //SetAndSendPacket(packet, Trigger.Right);
          SetAndSendPacket(Trigger.Left);
          SetAndSendPacket(Trigger.Right);
          //Wait(20);
        }

        lastMainHandAmmo = ammoR;
        lastOffHandAmmo = ammoL;

        mainHandWillShootNext = true;
        offHandWillShootNext = false;

        return;
        // SetAndSendPacket(packet, Trigger.Right, TriggerMode.AutomaticGun,new(){ 0,8,3 });
      }
      if (weaponIsAGun || isPedDueling || isDeadEyeEnabled)
      {

          
        //SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new(){6,1});
        eWeaponAttachPoint attachPoint;

        if (canUseOffhandWeapon && offHandWillShootNext) // most likely offhand
        {
          attachPoint = eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_SECONDARY;
        }
        else
        {
          attachPoint = eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_PRIMARY;
        }

        int weaponEntityIndex = GET_CURRENT_PED_WEAPON_ENTITY_INDEX(playerPed.Handle, (int)attachPoint);
        float degradation = GET_WEAPON_DEGRADATION(weaponEntityIndex);

        //  RDR2.UI.Screen.DisplaySubtitle(PLAYER._GET_PLAYER_WEAPON_DAMAGE(playerPed.Handle, (uint)currentMainHandWeapon.Hash).ToString("N3"));


        // float permanentDegradation = GET_WEAPON_PERMANENT_DEGRADATION(weaponEntityIndex);
        //
        // 279042 - double action
        bool mainHandIsDoubleAction = ShouldBehaveLikeDoubleAction(currentMainHandWeapon);
        bool offHandIsDoubleAction = ShouldBehaveLikeDoubleAction(currentOffHandWeapon);

        if (playerPed.IsOnHorse)
        {
          Ped horsie = playerPed.CurrentMount;
          int speedie = GetHorseSpeed(horsie); // 0-4, stop => fastest
          if (speedie > 0)
          {
            int start = 0, end, first, second = 7;
            end       = 9;
            first     = 0;
            int freq  = 1;
            switch (speedie)
            {
              case 1:
              {
                start  = 7;
                second = 1;
                freq   = 1;
                break;
              }
              case 2:
              {
                start  = 6;
                second = 3;
                freq   = 1;
                break;
              }
              case 3:
              {
                start  = 5;
                second = 5;
                freq   = 2;
                break;
              }
              case 4:
              {
                start  = 4;
                second = 7;
                freq   = 3;

                break;
              }
            }

            SetAndSendPacket(Trigger.Left, TriggerMode.Galloping, new List<int> { start, end, first, second, freq });
            ignoreLeftTrigger = true;
          }

        }


        //if (playerPed.IsShooting)
        if (playerPed.IsShooting)
        {
          // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new (){0,8, 2} );
          //RDR2.UI.Screen.DisplaySubtitle(ammoL + "- " + ammoR);// + " | " + weaponGuid_L + " - "+ weaponGuid_R);
          cockingActive = false;
          Wait(10);

          //SetAndSendPacket(packet, Trigger.Left, TriggerMode.Hardest);
          if (!canUseOffhandWeapon || (ammoR < lastMainHandAmmo))
          {
              
            DoTrigger_CustomRigid(Trigger.Right, 0, 255, 255);

            if (isAimingControlPressed)
            {
              DoTrigger_CustomRigid(Trigger.Left, 0, 32, 255);
            }

            if (canUseOffhandWeapon && ammoL > 0)
            {
              mainHandWillShootNext = false;
              offHandWillShootNext = true;
            }
          }
          else if (canUseOffhandWeapon && ammoL < lastOffHandAmmo)// && playerIsAiming)
          {
            //   RDR2.UI.Screen.DisplaySubtitle("off");

            if (isAimingControlPressed)
            {
              DoTrigger_CustomRigid(Trigger.Left,  0,  255,  255);
              DoTrigger_CustomRigid(Trigger.Right,  0,  32, 255);
            }
            else
            {
              DoTrigger_CustomRigid(Trigger.Right, 0, 255, 255);
            }
            if (ammoR > 0)
            {
              mainHandWillShootNext = true;
              offHandWillShootNext = false;
            }
          }
          lastMainHandAmmo = ammoR;
          lastOffHandAmmo = ammoL;

          Wait(50);
          return;
        }

        bool isControlAttackPressed = PAD.IS_CONTROL_PRESSED(0, (uint)eInputType.INPUT_FRONTEND_RT);
        if (cockingActive)
        {
          cockingActive = isControlAttackPressed;
        }

        bool uncockVolcanic = !mainWeaponIsReadyToShoot && isControlAttackPressed && currentMainHandWeapon.Hash == eWeapon.WEAPON_PISTOL_VOLCANIC;
        if (uncockVolcanic)
        {
          cockingActive = false;
        }
        if (cockingActive)
        {

          if (mainWeaponIsReadyToShoot)
          {
            if (isControlAttackPressed)
            {
              SetAndSendPacket(Trigger.Right);
              //DoTrigger_CustomRigid(Trigger.Right, 0, 255, 255);
              Wait(10);

            }
            else
            {
              cockingActive = false;
            }

            // Wait(100);
          }
          else
          {
            DoTrigger_CustomRigid(Trigger.Right, 64,
              64 + (int)MathExtended.Lerp(0, 96, degradation), 64 + (int)MathExtended.Lerp(0, 96, degradation));

          }
          cockingActive = true;
          if (!ignoreLeftTrigger) DoTrigger_CustomRigid(Trigger.Left, 120 - (int)(degradation * 110),  (int)(100 + degradation * 120), 180);
          // SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow, new()
          // {
          //   4,
          //   7,
          //   (int)(2 + (6f * degradation)),
          //   (int)(2 + (6f * degradation))
          // });


        }
        else if (!uncockVolcanic && !mainHandIsDoubleAction && !mainWeaponIsReadyToShoot && !canUseOffhandWeapon || !carriesWeaponOpenly) // Mode Gun Cock
        {
          cockingActive = true;
          if (!ignoreLeftTrigger) DoTrigger_CustomRigid(Trigger.Left, 160 - (int)(degradation * 150),  (int)(100 + degradation * 120), 180);
          // SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow, new()
          // {
          //   4,
          //   7,
          //   (int)(2 + (6f * degradation)),
          //   (int)(2 + (6f * degradation))
          // });

          DoTrigger_CustomRigid(Trigger.Right, 64,
            16 + (int)MathExtended.Lerp(0, 128, degradation), 16 + (int)MathExtended.Lerp(0, 128, degradation));

        }
        else // GUN_MANUAL;
        {
          cockingActive = false;
          // uint modelHash = WEAPON.enti(weaponEntityIndex);
          int triggerEnd = 1;
          float force1 = (2 + MathExtended.Lerp(0, 6, degradation));// + (6f * degradation));
          float force2 = (2 + MathExtended.Lerp(0, 6, degradation));

          bool doubleActionActive = false;

          if (mainHandIsDoubleAction)
          {
            if (!canUseOffhandWeapon) { doubleActionActive = true; }
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
            force1 = (3 + MathExtended.Lerp(0, 5, degradation));// + (6f * degradation));
            force2 = (3 + MathExtended.Lerp(0, 5, degradation));

            factor = 0.9f;
          }

          //RDR2.UI.Screen.DisplaySubtitle((doubleActionActive).ToString());
          if (!ignoreLeftTrigger) DoTrigger_CustomRigid(Trigger.Left, 168 - (int)(degradation * 140 * factor), (int)(32 + degradation * 160), 255);

          DoTrigger_Bow(Trigger.Right, 0, (int)triggerEnd, (int)force1, (int)force2);

          //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 0, 4,1+(int)(degradation * 7), 4 });
        }

        return;
      }
        
      // turn off
      SetAndSendPacket(Trigger.Right);
      if (!ignoreLeftTrigger) SetAndSendPacket(Trigger.Left);

      // SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, CustomTriggerValueMode.Rigid, 1, 20);
      // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 0, 2, 3, 4 });

      return;
      // updateLights();
      // health = Math.Min(health, 1);

      //RDR2.UI.Screen.DisplaySubtitle((myred).ToString());
      return;

      //else
    }
  }
}