using DSX_Base.MathExtended;
using RDR2;
using RDR2.Native;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using static DSX_Base.Client.iO;
using static RDR2.Native.WEAPON;

namespace DualSense4RDR2
{
  public class Main_RDR2 : Script
  {
    private static bool engine;
    private static int lastBrakeFreq = 0;
    private static int lastBrakeResistance = 200;
    private static int lastThrottleResistance = 1;
    private static bool noammo;
    private static Ped playerPed;
    private static Weapon playerWeapon;
    private static bool showbatstat = true;
    private static bool showconmes = true;
    private static bool wanted = false;
    private int brig;
    private float currentStaminaDisplay = 1f;
    private float health = 0;
    private int interval_direction = 1;

    private float interval_pos = 0;

    private BasicCurve pulseRateCurve;

    //private float pulseRate = 0;
    private int staminaTarget = 0;
//    public static readonly ControllerConfig controllerConfig = new();


    public Main_RDR2()
    {
      playerPed = Game.Player.Character;
      Tick += this.OnTick;
      KeyDown += this.OnKeyDown;
      Connect();
      Process.GetProcessesByName("DSX");
      pulseRateCurve = new BasicCurve()
      {
        new(0f,25f),
        new(0.2f,8f),
        new(0.7f, 1.2f),
        new(1f,0.3f)
      };
    }

    public void HandleLEDs()
    {
      if (currentStaminaDisplay >= 0.99f && interval_pos >= 0.95f)
      {
        interval_pos = 1;
      }
      else if (interval_direction == -1 && interval_pos <= 0.05f)
      {
        interval_direction = 1;
      }
      else if (interval_direction == 1 && interval_pos >= 1)
      {
        interval_direction = -1;
      }

      float green = health * 255f;
      float red = 255f - green;

      Packet packet = new();
      int num = 0;
      packet.instructions = new Instruction[4];
      packet.instructions[1].type = InstructionType.RGBUpdate;
      packet.instructions[1].parameters = new object[]
      {
        num,
        (int)(red * interval_pos),
        (int)(green * interval_pos),
        0
      };
      Send(packet);

      interval_pos += interval_direction * 0.01f * pulseRateCurve.Evaluate(currentStaminaDisplay);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      return;
      Packet packet = new();
      packet.instructions = new Instruction[4];
      if (e.KeyCode == Keys.F9)
      {
        iO obj = new();
        Send(packet);
        obj.getstat(out int bat, out bool isconnected);
        RDR2.UI.Screen.DisplaySubtitle("Controller connection status: " + isconnected + " controller battery status: " + bat + "% \n to hide this Press F10");
      }
      if (e.KeyCode == Keys.F10)
      {
        showbatstat = !showbatstat;
      }
      if (e.KeyCode == Keys.F11)
      {
        showconmes = !showconmes;
      }
    }
    private readonly Dictionary<uint, Weapon> sideArms = new Dictionary<uint, Weapon>();

    public unsafe Weapon CurrentOffHand
    {
      get
      {
        Ped owner = Game.Player.Character;
        uint weaponHash;
        GET_CURRENT_PED_WEAPON(owner!.Handle, &weaponHash, true, (int)eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_SECONDARY, false);
        
        //uint weaponHash = WEAPON._GET_PED_CURRENT_HELD_WEAPON(owner.Handle);
        if (sideArms!.ContainsKey(weaponHash))
        {
          return sideArms[weaponHash];
        }

        Weapon weapon = new Weapon(owner, (eWeapon)weaponHash);
        sideArms.Add(weaponHash, weapon);
        return weapon;
      }
    }

    private unsafe void OnTick(object sender, EventArgs e)
    {
      Packet packet = new();
      packet.instructions = new Instruction[4];
      Player player = Game.Player;
      playerPed = player.Character;
      playerWeapon = playerPed?.Weapons?.Current;

      //if ((uint)CurrentOffHand.Hash == 0xA2719263) // unarmed
      {
        //RDR2.UI.Screen.DisplaySubtitle((CurrentOffHand.Hash).ToString());

      }

      // SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 1 });
      // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 5, 8, 8 });

      bool mainWeaponIsReadyToShoot = IS_PED_WEAPON_READY_TO_SHOOT(playerPed.Handle);
      bool weaponIsAGun = IS_WEAPON_A_GUN((uint)playerWeapon.Hash); //IS_WEAPON_A_GUN
      bool weaponIsThrowable = _IS_WEAPON_THROWABLE((uint)playerWeapon.Hash); //
      bool mainWeaponIsTwoHanded = _IS_WEAPON_TWO_HANDED((uint)playerWeapon.Hash);
      bool carriesWeaponOpenly = (uint)playerWeapon.Hash != 0xA2719263; /* unarmed*/
      bool hasOffHandWeapon = (uint)CurrentOffHand.Hash != 0xA2719263 /* unarmed*/ &&
                              (CurrentOffHand.Group == eWeaponGroup.GROUP_PISTOL ||
                               CurrentOffHand.Group == eWeaponGroup.GROUP_REVOLVER);
      //uint* numbi = null;
      //var mount = GET_CURRENT_PED_VEHICLE_WEAPON(characterPed.Handle, numbi); //

      bool hasMountedWeapon = playerPed?.Weapons?.Current?.Group == 0 && playerPed.IsSittingInVehicle();

      bool isMounted = hasMountedWeapon && playerPed?.CurrentVehicle != null;

      uint number = 0;

      bool currentPedVehicleWeapon = GET_CURRENT_PED_VEHICLE_WEAPON(playerPed.Handle, &number);

      // RDR2.UI.Screen.DisplaySubtitle(hasOffHandWeapon.ToString());

      if (isMounted)
      {
      }
      // return;
      bool playerIsAiming = player.IsAiming;

      //RDR2.UI.Screen.DisplaySubtitle(weaponIsAGun.ToString());
      // return;

      //RDR2.UI.Screen.DisplaySubtitle(playerweapon.Group.ToString());

      // Weapon Wheel
      if (PAD.IS_CONTROL_PRESSED(0, 3901091606))  //INPUT_FRONTEND_LB, weapon wheel
      {
        SetAndSendPacket(packet, Trigger.Right);
        SetAndSendPacket(packet, Trigger.Left);
      }
      // no gun accidents
      else if (playerPed.IsReloading) // Mode reloading
      {
        SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);
        SetAndSendPacket(packet, Trigger.Left, TriggerMode.Hardest);
      }
      else if (currentPedVehicleWeapon)
      {
        //RDR2.UI.Screen.DisplaySubtitle((number).ToString());

        if (number == 3666182381 || //gat
            number == 3101324918)// maxi
        {
          SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid, startOfResistance: 1, amountOfForceExerted: 20);
          if (playerPed.IsShooting) // Auto
          {
            SetAndSendPacket(packet, Trigger.Right,
              TriggerMode.AutomaticGun, new(){0,4,30});
            Script.Wait(100);
          }
          else // Prepare
          {
            SetAndSendPacketCustom(packet, Trigger.Right,
              CustomTriggerValueMode.Rigid, startOfResistance: 30, amountOfForceExerted: 128);
          }
        }
        else if (number == 2465730487 || //hotch - cannons
                 number == 1609145491)// breach
        {
          SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid, startOfResistance: 1, amountOfForceExerted: 20);
          SetAndSendPacketCustom(packet, Trigger.Right,
            CustomTriggerValueMode.PulseA, startOfResistance: 255, amountOfForceExerted: 200, forceExertedInRange: 255);
        }
      }
      else if (weaponIsThrowable || playerWeapon.Group == eWeaponGroup.GROUP_BOW || playerWeapon.Group == eWeaponGroup.GROUP_FISHINGROD || playerWeapon.Group == eWeaponGroup.GROUP_LASSO)
      {
        if (PED._GET_LASSO_TARGET(playerPed.Handle) != 0)
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
          SetAndSendPacket(packet, Trigger.Left, TriggerMode.Resistance, new() { 2, 8 });

          SetAndSendPacket(packet, Trigger.Right, TriggerMode.Resistance, new() { 1, 6 });
        }
        else
        {
          SetAndSendPacket(packet, Trigger.Left, TriggerMode.Resistance, new() { 4, 1 });

          SetAndSendPacket(packet, Trigger.Right, TriggerMode.Resistance, new() { 4, 2 });
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

          if (hasOffHandWeapon && !mainWeaponIsReadyToShoot || !carriesWeaponOpenly) // most likely offhand
          {
            attachPoint = eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_SECONDARY;
          }
          else
          {
            attachPoint =  eWeaponAttachPoint.WEAPON_ATTACH_POINT_HAND_PRIMARY;
          }

          int weaponEntityIndex = GET_CURRENT_PED_WEAPON_ENTITY_INDEX(playerPed.Handle, (int)attachPoint);
          float degradation = GET_WEAPON_DEGRADATION(weaponEntityIndex);
          // float permanentDegradation = GET_WEAPON_PERMANENT_DEGRADATION(weaponEntityIndex);
          // 
          // RDR2.UI.Screen.DisplaySubtitle(weaponEntityIndex.ToString());

           // 279042 - double action
           bool isDoubleAction = weaponEntityIndex == 279042;
          SetAndSendPacketCustom(packet, Trigger.Left, CustomTriggerValueMode.Rigid, startOfResistance: 8-(int)(degradation * 7),  amountOfForceExerted: (int)(degradation *100));

          if (playerPed.IsShooting)
          {

           // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new (){0,8, 2} );

            Wait(50);

            SetAndSendPacket(packet, Trigger.Right, TriggerMode.Hardest);
             SetAndSendPacketCustom(packet, Trigger.Right, CustomTriggerValueMode.Rigid, startOfResistance: 0, amountOfForceExerted: 255, forceExertedInRange: 255);
             Wait(75);
          }
          else if (!isDoubleAction && (playerIsAiming && mainWeaponIsTwoHanded || !hasOffHandWeapon )  && !mainWeaponIsReadyToShoot || isPedDuelling || !carriesWeaponOpenly) // Mode Gun Cock
          {
            SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow, new()
            {
              6,
              7,
              (int)(2 + (6f * degradation)),
              (int)(2 + (6f * degradation))
            });

            // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 4, 1 + (int)(degradation * 3), 2 });
          }
          else // GUN_MANUAL; 
          {
           // uint modelHash = WEAPON.enti(weaponEntityIndex);




            //RDR2.UI.Screen.DisplaySubtitle((degradation).ToString());

            SetAndSendPacket(packet, Trigger.Right, TriggerMode.Bow, new()
            {
              0,
              1,
              (int)(3 + (5f * degradation)),
              (int)(4 + (4f * degradation))
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

      updateLights();
      HandleLEDs();
      return;
      // updateLights();
      // health = Math.Min(health, 1);

      //RDR2.UI.Screen.DisplaySubtitle((myred).ToString());
      return;

      //else
      return;

      //RDR2.UI.Screen.DisplaySubtitle(health + " - " + pulseRate + " - " + staminaTarget);
      add add2 = new();
      iO obj = new();
      Send(packet);
      Wait(235);
      obj.getstat(out int bat, out bool isConnected);

      if (!isConnected && showconmes)
      {
        RDR2.UI.Screen.DisplaySubtitle("controller is disconnected or discharged, please fix or press F11");
      }
      else if (bat <= 15 && showbatstat)
      {
        RDR2.UI.Screen.DisplaySubtitle("Your controller battery is  " + bat + " to hide this message press F10");
        // RDR2.UI.Screen.ShowHelpMessage("Your controller battery is  " + bat + " to hide this message press F10", 1, sound: false);
      }

    }

    private void updateLights()
    {
      // if (ScriptSettings::getBool("HealthIndication"))
      {
        this.health = playerPed.Health * 1f / (playerPed.MaxHealth * 1f);
      }

      // else
      // {
      //     health = 1;
      // }

      // if (ScriptSettings::getBool("StaminaIndication"))
      {
        this.staminaTarget = playerPed.Handle;
        if (playerPed.IsOnMount)
        {
          this.staminaTarget = PED.GET_MOUNT(playerPed.Handle);
        }

        //float stamina = RDR2.Native.PED._GET_PED_STAMINA(this.staminaTarget)*1f / RDR2.Native.PED._GET_PED_MAX_STAMINA(this.staminaTarget)*1f;
        float stamina = PED._GET_PED_STAMINA_NORMALIZED(this.staminaTarget);
        currentStaminaDisplay = stamina;// DSX_Math.LerpCapped(0.1f,0.8f,stamina);
        //this.pulseRate = DSX_Math.LerpCapped(1f, 0.2f, stamina);// Math.Max(0.2f, Math.Min(1f, 1f - stamina));
        //RDR2.UI.Screen.DisplaySubtitle(playerPed.Health + " - " + playerPed.MaxHealth + " - " + pulseRate);
      }
      // else
      // {
      //     pulseRate = 0;
      // }
    }
  }
}