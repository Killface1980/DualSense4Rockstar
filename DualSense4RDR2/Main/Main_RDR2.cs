using DSX_Base.MathExtended;
using RDR2;
using RDR2.Native;
using Shared;
using System;
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
    private static Weapon playerweapon;
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
      else if (interval_direction == -1 && interval_pos <= 0)
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

    private unsafe void OnTick(object sender, EventArgs e)
    {
      Packet packet = new();
      int controllerIndex = 0;
      packet.instructions = new Instruction[4];
      Player player = Game.Player;
      playerPed = player.Character;
      playerweapon = playerPed?.Weapons?.Current;

      // SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 1, 1 });
      // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 5, 8, 8 });
      bool weaponIsReadyToShoot = IS_PED_WEAPON_READY_TO_SHOOT(playerPed.Handle);
      bool weaponIsAGun = IS_WEAPON_A_GUN((uint)playerweapon.Hash); //IS_WEAPON_A_GUN
      bool weaponIsThrowable = _IS_WEAPON_THROWABLE((uint)playerweapon.Hash); //
      bool twoHanded = _IS_WEAPON_TWO_HANDED((uint)playerweapon.Hash);
      //uint* numbi = null;
      //var mount = GET_CURRENT_PED_VEHICLE_WEAPON(characterPed.Handle, numbi); //

      bool hasMountedWeapon = playerPed?.Weapons?.Current?.Group == 0 && playerPed.IsSittingInVehicle();

      bool isMounted = hasMountedWeapon && playerPed?.CurrentVehicle != null;

      uint number = 0;

      bool currentPedVehicleWeapon = GET_CURRENT_PED_VEHICLE_WEAPON(playerPed.Handle, &number);

      // RDR2.UI.Screen.DisplaySubtitle(weaponIsAGun.ToString());

      if (isMounted)
      {
      }
      // return;
      bool playerIsAiming = player.IsAiming;

      //RDR2.UI.Screen.DisplaySubtitle(weaponIsAGun.ToString());
      // return;

      //RDR2.UI.Screen.DisplaySubtitle(playerweapon.Group.ToString());
      
      // Weapon Wheel
      if (PAD.IS_CONTROL_PRESSED(0, 3901091606))  //INPUT_FRONTEND_LB
      {
        SetAndSendPacket(packet, controllerIndex, Trigger.Right);
        SetAndSendPacket(packet, controllerIndex, Trigger.Left);
      }
      // no gun accidents
      else if (playerPed.IsReloading) // Mode reloading
      {
        SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Hardest);
        SetAndSendPacket(packet, controllerIndex, Trigger.Left);
      }
      else if (weaponIsThrowable || playerweapon.Group == eWeaponGroup.GROUP_BOW || playerweapon.Group == eWeaponGroup.GROUP_FISHINGROD || playerweapon.Group == eWeaponGroup.GROUP_LASSO)
      {
        if (PED._GET_LASSO_TARGET(playerPed.Handle) != 0)
        {
          SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Hardest);
          SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Hardest);
        }
        /*        else if (player.IsTargettingAnything) // not working on the lasso
                {
                  SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 2, 8 });

                  SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 6, 8,2,3 });
                }
        */
        else if (player.IsAiming)
        {
          SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 2, 8 });

          SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 1, 6});
        }
        else
        {
          SetAndSendPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new() { 4, 2 });

          SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new() { 4, 2 });
        }
      }
      else
      {
        bool isPedDuelling = TASK._IS_PED_DUELLING(player.Handle);
        if (weaponIsAGun || isPedDuelling)
        {
          SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, CustomTriggerValueMode.Rigid, 1, 20);

          float degradation = GET_WEAPON_DEGRADATION(GET_CURRENT_PED_WEAPON_ENTITY_INDEX(playerPed.Handle, 0));

          // RDR2.UI.Screen.DisplaySubtitle(degradation.ToString());
          if (isPedDuelling)
          {
            SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new()
            {
              4,
              7,
              (int)(4 + (4f * degradation)),
              (int)(4 + degradation * 4)
            });
            Wait(500);
          }
          else if (playerPed.IsShooting)
          {
            //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Hardest);
            SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right, CustomTriggerValueMode.Rigid, 0,255,255);
            Wait(75);
          }
          else if ((playerIsAiming || twoHanded) && !weaponIsReadyToShoot) // Mode Gun Cock
          {
            SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new()
            {
              7,
              8,
              4,
              (int)(2 + degradation * 6)
            });

            // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 4, 1 + (int)(degradation * 3), 2 });
          }
          else // GUN_MANUAL
          {
            SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new()
            {
              0,
              2,
              (int)(2 + (6f * degradation)),
              (int)(4 + degradation * 4)
            });
            //RDR2.UI.Screen.DisplaySubtitle(degradation.ToString());

            //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 0, 4,1+(int)(degradation * 7), 4 });
          }
        }
        else if (currentPedVehicleWeapon)
        {
          if (number == 3666182381 || //gat
              number == 3101324918)// maxi
          {
            SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, CustomTriggerValueMode.Rigid, 1, 20);
            if (playerPed.IsShooting) // Auto
            {
              SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right,
                CustomTriggerValueMode.PulseB, 9, 190);
            }
            else // Prepare
            {
              SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right,
                CustomTriggerValueMode.Rigid, 30, 255);
            }
          }
          else if (number == 2465730487 || //hotch - cannons
                   number == 1609145491)// breach
          {
            SetAndSendPacketCustom(packet, controllerIndex, Trigger.Left, CustomTriggerValueMode.Rigid, 1, 20);
            SetAndSendPacketCustom(packet, controllerIndex, Trigger.Right,
              CustomTriggerValueMode.PulseA, 255, 200, 255);
          }
        }
        else // turn off
        {
          SetAndSendPacket(packet, controllerIndex, Trigger.Right);
          SetAndSendPacket(packet, controllerIndex, Trigger.Left);
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

      switch (player.WantedLevel)
      {
        case 0:
          packet.instructions[2].type = InstructionType.PlayerLED;
          packet.instructions[2].parameters = new object[6] { controllerIndex, false, false, false, false, false };
          Send(packet);
          Wait(299);
          Wait(299);
          wanted = false;
          break;

        case 1:
          {
            add2.rgbupdat2e(10, playerPed.Health, out int red, out int blue);
            wanted = true;
            packet.instructions[2].type = InstructionType.PlayerLED;
            packet.instructions[2].parameters = new object[6] { controllerIndex, true, false, false, false, false };
            Send(packet);
            packet.instructions[1].type = InstructionType.RGBUpdate;
            packet.instructions[1].parameters = new object[4] { controllerIndex, blue, 0, red };
            Send(packet);
            break;
          }
        case 2:
          add2.rgbupdat2e(30, playerPed.Health, out int _, out int _);
          packet.instructions[2].type = InstructionType.PlayerLED;
          packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, false, false, false };
          Send(packet);
          wanted = true;
          break;

        case 3:
          add2.rgbupdat2e(40, playerPed.Health, out int _, out int _);
          packet.instructions[2].type = InstructionType.PlayerLED;
          packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, false, false };
          Send(packet);
          wanted = true;
          break;

        case 4:
          packet.instructions[2].type = InstructionType.PlayerLED;
          packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, true, false };
          Send(packet);
          add2.rgbupdat2e(50, playerPed.Health, out int _, out int _);
          wanted = true;
          break;

        case 5:
          add2.rgbupdat2e(70, playerPed.Health, out int _, out int _);
          packet.instructions[2].type = InstructionType.PlayerLED;
          packet.instructions[2].parameters = new object[6] { controllerIndex, true, true, true, true, true };
          Send(packet);
          wanted = true;
          break;
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