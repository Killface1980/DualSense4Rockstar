using DSX_Base;
using DSX_Base.MathExtended;
using RDR2;
using RDR2.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DualSense4RDR2.Config;
using static DSX_Base.Client.iO;
using static DualSense4RDR2.Stats.HorseStat;
using static RDR2.Native.WEAPON;

// using DualSense4RDR2.Config;

namespace DualSense4RDR2;

public class Main_RDR2 : Script
{
  private readonly Dictionary<uint, Weapon> playerWeapons = new();
  private bool cockingActive;
  private int lastMainHandAmmo;
  private int lastOffHandAmmo;

  public static bool canUseThreshold;
  //private float pulseRate = 0;
  //    public static readonly ControllerConfig controllerConfig = new();

  private bool mainHandWillShootNext;

  private bool offHandWillShootNext;

  public Main_RDR2()
  {
    Tick += OnTick;
    Connect();
    Process.GetProcessesByName("DSX");
     var config = new ControllerConfig();
  }

  private static bool ShouldBehaveLikeDoubleAction(Weapon weapon)
  {
    return weapon.Hash
      is eWeapon.RevolverDoubleAction
      or eWeapon.RevolverDoubleActionDualwield
      or eWeapon.RevolverDoubleActionExotic
      or eWeapon.RevolverDoubleActionGambler
      or eWeapon.RevolverDoubleActionMicah
      or eWeapon.RevolverDoubleActionMicahDualwield
      or eWeapon.PistolVolcanic // 
      or eWeapon.PistolVolcanicCollector // Added Volcanic. Right?
      or eWeapon.PistolMauser
      or eWeapon.PistolMauserDrunk
      or eWeapon.PistolSemiauto
      or eWeapon.PistolM1899
      or eWeapon.ShotgunSemiauto;
  }

  private unsafe Weapon GetCurrentWeapon(bool offHand = false)
  {
    Ped owner = Game.Player.Character;
    uint weaponHash;
    GET_CURRENT_PED_WEAPON(owner!.Handle, &weaponHash, true,
      offHand
        ? (int)eWeaponAttachPoint.Secondary
        : (int)eWeaponAttachPoint.Primary, false);

    //int weaponEntityIndex = GET_CURRENT_PED_WEAPON_ENTITY_INDEX(playerPed.Handle, (int)attachPoint);

    //uint weaponHash = WEAPON._GET_PED_CURRENT_HELD_WEAPON(owner.Handle);
    if (playerWeapons.ContainsKey(weaponHash)) return playerWeapons[weaponHash];

    Weapon weapon = new(owner, (eWeapon)weaponHash);
    playerWeapons.Add(weaponHash, weapon);
    return weapon;
  }

  private void OnTick(object sender, EventArgs e)
  {
    Ped playerPed = Game.Player?.Character;
    Player player = Game.Player;
    Weapon currentMainHandWeapon = GetCurrentWeapon(); // playerPed?.Weapons?.Current;
    Weapon currentOffHandWeapon = GetCurrentWeapon(true);
    //playerPed.Weapons.CurrentWeaponEntity.
    //if ((uint)CurrentOffHand.Hash == 0xA2719263) // unarmed
    {
      //RDR2.UI.Screen.DisplaySubtitle((CurrentOffHand.Hash).ToString());
    }

    // SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 1 });
    // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 5, 8, 8 });
    if (false)
    {
      Packet p = new();
      p.instructions = new Instruction[4];
      p.instructions[1].type = InstructionType.TriggerThreshold;
      p.instructions[1].parameters = new object[] { 0, Trigger.Right, 254 };
      Send(p);
      SetAndSendPacket(Trigger.Right);
      return;
    }

    bool mainWeaponIsReadyToShoot = IS_PED_WEAPON_READY_TO_SHOOT(playerPed.Handle);
    bool mainHandWeaponIsAGun = IS_WEAPON_A_GUN((uint)currentMainHandWeapon.Hash); //IS_WEAPON_A_GUN
    //bool mainWeaponIsTwoHanded    = _IS_WEAPON_TWO_HANDED((uint)currentMainHandWeapon.Hash);
    bool carriesWeaponOpenly = currentMainHandWeapon.Hash != eWeapon.Unarmed; /* unarmed*/
    bool canUseOffhandWeapon = currentMainHandWeapon.IsOneHanded && currentOffHandWeapon.IsOneHanded &&
                               currentOffHandWeapon.Hash != eWeapon.Unarmed;
    //uint* numbi                 = null;
    //var mount                   = GET_CURRENT_PED_VEHICLE_WEAPON(characterPed.Handle, numbi); //

    int ammoL = currentOffHandWeapon.AmmoInClip;
    int ammoR = currentMainHandWeapon.AmmoInClip;

    if (currentMainHandWeapon.Hash != eWeapon.Unarmed &&
        (currentMainHandWeapon.Hash == currentOffHandWeapon.Hash ||
         currentOffHandWeapon.Hash == eWeapon.Unarmed)) // get the proper ammo amount
      unsafe // IMPORTANT: Re-use the ammoPointer, otherwise RDR crashes. Also new weaponGuid, otherwise mixup.
      {
        ulong weaponGuid_L;
        GET_PED_WEAPON_GUID_AT_ATTACH_POINT(playerPed.Handle,
          (int)eWeaponAttachPoint.Secondary, &weaponGuid_L);
        int ammoPointer;
        _GET_AMMO_IN_CLIP_BY_INVENTORY_UID(playerPed.Handle, &ammoPointer, &weaponGuid_L);

        ammoL = ammoPointer;

        ulong weaponGuid_R;
        GET_PED_WEAPON_GUID_AT_ATTACH_POINT(playerPed.Handle, (int)eWeaponAttachPoint.Primary,
          &weaponGuid_R);
        //WEAPON.GET_PED_WEAPON_GUID_AT_ATTACH_POINT(playerPed.Handle, (int)eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_PRIMARY, &weaponGuid_R);
        //int ammo_R = 0;
        _GET_AMMO_IN_CLIP_BY_INVENTORY_UID(playerPed.Handle, &ammoPointer, &weaponGuid_R);
        // WEAPON._GET_AMMO_IN_CLIP_BY_INVENTORY_UID(playerPed.Handle, &ammo_R, &weaponGuid_R);
        ammoR = ammoPointer;
      }

    //bool hasMountedWeapon = playerPed?.Weapons?.Current?.Group == 0 && playerPed.IsSittingInVehicle();

    //bool isMounted = hasMountedWeapon && playerPed?.CurrentVehicle != null;

    // RDR2.UI.Screen.DisplaySubtitle(currentMainHandWeapon.AmmoInClip + " - " + currentOffHandWeapon.AmmoInClip);

    // RDR2.UI.Screen.DisplaySubtitle(hasOffHandWeapon.ToString());

    // return;

    //RDR2.UI.Screen.DisplaySubtitle(weaponIsAGun.ToString());
    // return;

    //RDR2.UI.Screen.DisplaySubtitle(playerweapon.Group.ToString());
    //RDR2.UI.Screen.DisplaySubtitle((playerPed.IsReloading).ToString());

    // Weapon Wheel
    if (PAD.IS_CONTROL_PRESSED(0, (uint)eInputType.FrontendLB)) //INPUT_FRONTEND_LB, weapon wheel
    {
      DoTrigger_Resistance(Trigger.Left, 8, 1);
      DoTrigger_Resistance(Trigger.Right, 8, 1);
      return;
      //SetAndSendPacket(packet, Trigger.Right);
      //SetAndSendPacket(packet, Trigger.Left);
    }

    bool ignoreLeftTrigger = false;


    bool isPedDueling = false;
    bool isDeadEyeEnabled = player.IsInDeadEye || player.IsInEagleEye;

    if (!mainHandWeaponIsAGun && !isDeadEyeEnabled)
    {
      isPedDueling = TASK._IS_PED_DUELLING(player.Handle);
    }

    if (mainHandWeaponIsAGun || isDeadEyeEnabled || isPedDueling)
    {
      bool isAimingControlPressed = PAD.IS_CONTROL_PRESSED(0, (uint)eInputType.FrontendLT) || player.IsAiming;
      if (playerPed.IsReloading) // Mode reloading
      {
        // SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);

        //SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);
        //RDR2.UI.Screen.DisplaySubtitle(lastMainHandAmmo + " - " +  ammoR + " - " + currentOffHandWeapon.Hash + " - " + ammoL);

        if (lastMainHandAmmo != ammoR && (canUseOffhandWeapon || !isAimingControlPressed))
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

      bool isControlAttackPressed = PAD.IS_CONTROL_PRESSED(0, (uint)eInputType.FrontendRT);


      //SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new(){6,1});
      eWeaponAttachPoint attachPoint = eWeaponAttachPoint.Primary;

      if (canUseOffhandWeapon && offHandWillShootNext) // most likely offhand
        attachPoint = eWeaponAttachPoint.Secondary;

      int weaponEntityIndex = GET_CURRENT_PED_WEAPON_ENTITY_INDEX(playerPed.Handle, (int)attachPoint);
      float degradation     = GET_WEAPON_DEGRADATION(weaponEntityIndex);
      // var damage = _GET_WEAPON_DAMAGE(weaponEntityIndex);
      //RDR2.UI.Screen.DisplaySubtitle(PLAYER._GET_PLAYER_WEAPON_DAMAGE(playerPed.Handle, (uint)weaponEntityIndex).ToString());
      //RDR2.UI.Screen.DisplaySubtitle(_GET_WEAPON_DAMAGE((int)currentMainHandWeapon.Hash).ToString("N3"));

      // float permanentDegradation = GET_WEAPON_PERMANENT_DEGRADATION(weaponEntityIndex);
      //
      // 279042 - double action
      bool mainHandIsDoubleAction = ShouldBehaveLikeDoubleAction(currentMainHandWeapon);
      bool offHandIsDoubleAction  = ShouldBehaveLikeDoubleAction(currentOffHandWeapon);

      if (playerPed.IsOnHorse)
      {
        Ped horsie = playerPed.CurrentMount;
        CurrentHorseStat speedie = GetHorseSpeed(horsie); // 0-4, stop => fastest
        if (speedie != CurrentHorseStat.Stopped)
        {
          int start = 0, end, first, second = 7;
          end = 9;
          first = 0;
          int freq = 1;
          switch (speedie)
          {
            case CurrentHorseStat.Walking:
            {
              start = 7;
              second = 1;
              freq = 1;
              break;
            }
            case CurrentHorseStat.Galloping:
            {
              start = 6;
              second = 3;
              freq = 1;
              break;
            }
            case CurrentHorseStat.Running:
            {
              start = 5;
              second = 5;
              freq = 2;
              break;
            }
            case CurrentHorseStat.Sprinting:
            {
              start = 4;
              second = 7;
              freq = 3;

              break;
            }
          }

          SetAndSendPacket(Trigger.Left, TriggerMode.Galloping, new List<int> { start, end, first, second, freq });
          ignoreLeftTrigger = true;
        }
      }

      if (playerPed.IsShooting)
      {
        // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new (){0,8, 2} );
        //RDR2.UI.Screen.DisplaySubtitle(ammoL + "- " + ammoR);// + " | " + weaponGuid_L + " - "+ weaponGuid_R);
        cockingActive = false;
        Wait(10);


        //SetAndSendPacket(packet, Trigger.Left, TriggerMode.Hardest);
        if (!canUseOffhandWeapon || ammoR < lastMainHandAmmo)
        {
          DoTrigger_CustomRigid(Trigger.Right, 0, 255, 255);

          if (isAimingControlPressed) DoTrigger_CustomRigid(Trigger.Left, 0, 32, 255);

          if (canUseOffhandWeapon && ammoL > 0)
          {
            mainHandWillShootNext = false;
            offHandWillShootNext = true;
          }
        }
        else if (canUseOffhandWeapon && ammoL < lastOffHandAmmo) // && playerIsAiming)
        {
          //   RDR2.UI.Screen.DisplaySubtitle("off");

          if (isAimingControlPressed)
          {
            DoTrigger_CustomRigid(Trigger.Left, 0, 255, 255);
            DoTrigger_CustomRigid(Trigger.Right, 0, 32, 255);
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

      if (cockingActive) cockingActive = isControlAttackPressed;

      bool uncockVolcanic = !mainWeaponIsReadyToShoot && isControlAttackPressed &&
                            (currentMainHandWeapon.Hash == eWeapon.PistolVolcanic || currentMainHandWeapon.Hash == eWeapon.PistolVolcanicCollector);

      if (uncockVolcanic) cockingActive = false;
      if (cockingActive)
      {
        if (mainWeaponIsReadyToShoot)
        {
          if (isControlAttackPressed)
          {
            //SetAndSendPacket(Trigger.Right);
            //Wait(50);
            DoTrigger_CustomRigid(Trigger.Right, 0, 255, 255);
            Wait(25);
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

       // cockingActive = true;
        if (!ignoreLeftTrigger)
          DoTrigger_CustomRigid(Trigger.Left, 80 - (int)(degradation * 70), (int)(100 + degradation * 120), 180);
        // SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow, new()
        // {
        //   4,
        //   7,
        //   (int)(2 + (6f * degradation)),
        //   (int)(2 + (6f * degradation))
        // });
      }
      else if ((!uncockVolcanic && !mainHandIsDoubleAction && !mainWeaponIsReadyToShoot && !canUseOffhandWeapon) ||
               !carriesWeaponOpenly) // Mode Gun Cock
      {
        cockingActive = true;
        if (!ignoreLeftTrigger)
          DoTrigger_CustomRigid(Trigger.Left, 160 - (int)(degradation * 150), (int)(100 + degradation * 120), 180);
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
        float force1 = 1 + MathExtended.Lerp(0, 7, degradation); // + (6f * degradation));
        float force2 = 1 + MathExtended.Lerp(0, 7, degradation);

        bool doDoubleActionNow = false;

        if (mainHandIsDoubleAction)
        {
          if (!canUseOffhandWeapon)
            doDoubleActionNow = true;
          else if (mainHandWillShootNext) doDoubleActionNow = true;
          // RDR2.UI.Screen.DisplaySubtitle("main is double");
        }

        if (offHandIsDoubleAction && !doDoubleActionNow)
          if (offHandWillShootNext)
            doDoubleActionNow = true;

        float factor = 1f;
        if (doDoubleActionNow) // harder triggers for double action
        {
          triggerEnd = 3;
          force1 = 3 + MathExtended.Lerp(0, 5, degradation); // + (6f * degradation));
          force2 = 3 + MathExtended.Lerp(0, 5, degradation);

          factor = 0.9f;
        }

        // RDR2.UI.Screen.DisplaySubtitle(mainHandWillShootNext + " - " + offHandWillShootNext + " | " + force2);
        if (!ignoreLeftTrigger)
          DoTrigger_CustomRigid(Trigger.Left, 168 - (int)(degradation * 140 * factor), (int)(32 + degradation * 160),
            64+(int)(degradation * 140) );

        if (canUseThreshold)
        {
          int offieset = 2;// doDoubleActionNow ? 1 : 3;
          DoTrigger_BowThreshold(Trigger.Right, offieset, (int)triggerEnd + offieset, (int)force1, (int)force2, (int)((triggerEnd + offieset + 1) * 32f));
        }else{
          DoTrigger_Bow(Trigger.Right, 0, (int)triggerEnd, (int)force1, (int)force2);
        }




        //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 0, 4,1+(int)(degradation * 7), 4 });
      }

      return;

    }

    bool weaponIsThrowable = _IS_WEAPON_THROWABLE((uint)currentMainHandWeapon.Hash); //

    if (weaponIsThrowable ||
        currentMainHandWeapon.Group is eWeaponGroup.Bow or eWeaponGroup.FishingRod 
          or eWeaponGroup.Lasso)
    {
      if (currentMainHandWeapon.Group == eWeaponGroup.FishingRod)
      {
        // ulong currentState;
        // TASK._GET_TASK_FISHING(playerPed.Handle, &currentState); // BUG Crashing https://pastebin.com/NmK5ZLVs
        // TASK._GET_TASK_FISHING(playerPed.Handle, (ulong*)((IntPtr)(&currentState)).ToInt32()); // BUG Crashing https://pastebin.com/NmK5ZLVs

        // Hans.FishTaskState state = Hans.GetFishTaskState();
        // RDR2.UI.Screen.DisplaySubtitle(state.FishingState.ToString());

        //state.FishingState == (int)FishingStates.Caught_fish_holding_in_hand

        /*
         * https://github.com/Halen84/RDR3-Decompiled-Scripts/tree/master/1491-16
         *
         * TuffyTown — gestern um 22:08 Uhr
           its probably crashing because of an incorrect struct being passed.
           note that you will have to convert the struct to type Any* or in this case a ulong* and then pass the struct back as a pointer.
           c++ example:
           struct SampleStruct
           {
           alignas(8) int f_0 = 0;
           alignas(8) int f_1 = 0;
           alignas(8) int f_2 = 0;
           }
           SampleStruct sample{};
           NATIVE::INVOKE((Any*)&sample); // note the "Any*" cast and the "&"
           
           i have decompiled scripts as of 1491.16 found here: https://github.com/Halen84/RDR3-Decompiled-Scripts 
           i havent researched any fishing stuff, so you're kinda on your own there.
           you'll just have to look at the decompiled scripts and still continue your research
           Ked28 — heute um 00:03 Uhr
           I'd look at fishing_core to start
         *
         * https://gist.github.com/NoNameSet/c17cd089cbfab97411af7c036ce1630d RDR2 Native Renamer
         *
         * https://github.com/alloc8or/rdr3-nativedb-data
         *
         */

        DoTrigger_Resistance(Trigger.Left, 6, 1);

        DoTrigger_Resistance(Trigger.Right, 4, 2);
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
        if (!ignoreLeftTrigger) DoTrigger_Resistance(Trigger.Left, 6, 4);

        DoTrigger_Resistance(Trigger.Right, 1, 6);
        // RDR2.UI.Screen.DisplaySubtitle("aimy");
      }
      else
      {
        if (!ignoreLeftTrigger) DoTrigger_Resistance(Trigger.Left, 6, 1);

        DoTrigger_Resistance(Trigger.Right, 4, 2);
        // RDR2.UI.Screen.DisplaySubtitle("fishing");
      }

      return;
    }


    bool pedHasVehicleWeapon;
    uint weaponHash = 0;

    unsafe
    {
      pedHasVehicleWeapon = GET_CURRENT_PED_VEHICLE_WEAPON(playerPed.Handle, &weaponHash);
    }

    //var mount = playerPed.CurrentMount;
    // RDR2.UI.Screen.DisplaySubtitle((((PedVehicleWeaponHash)weaponHash).ToString()));

    if (pedHasVehicleWeapon)
    {
      switch ((PedVehicleWeaponHash)weaponHash)
      {
        case PedVehicleWeaponHash.HotchkissCannon: //hotch - cannons
        case PedVehicleWeaponHash.BreachCannon: // breach
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
        case PedVehicleWeaponHash.GatlingGun: // gat
        case PedVehicleWeaponHash.GatlingMaxim02: // maxi
          {
            int iMax = 20;
            //bool isWeaponReadyToShoot = IS_PED_WEAPON_READY_TO_SHOOT(playerPed.Handle);
            if (playerPed.IsShooting) // Auto
            {
              DoTrigger_AutomaticGun(Trigger.Right, 0, 8, iMax);
              DoTrigger_AutomaticGun(Trigger.Left, 0, 8, iMax);
              Wait(100);
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
  }
}