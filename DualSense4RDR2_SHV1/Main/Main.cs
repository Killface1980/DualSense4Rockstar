using RDR2;
using RDR2.Native;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using DSX_Base;
using static DSX_Base.Client.iO;

namespace DualSense4RDR2
{
  public class Main : Script
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
    private float health = 0; 
    private float pulseRate = 0;
    private int staminaTarget = 0;
    private float interval_pos;
    private int interval_direction;

    public Main()
    {
      playerPed = Game.Player.Character;
      Tick += this.OnTick;
      KeyDown += this.OnKeyDown;
      Connect();
      Process.GetProcessesByName("DSX");
    }

    public void rgbupdat2e(int speed, int brightnes, out int red, out int blue)
    {
      blue = 255;
      red = 1;
      Connect();
      Packet packet = new();
      int num = 0;
      packet.instructions = new Instruction[4];
      while (red <= 255)
      {
        Wait(10);
        red += speed;
        blue -= speed;
        packet.instructions[1].type = InstructionType.RGBUpdate;
        packet.instructions[1].parameters = new object[4]
        {
                    num,
                    blue - this.brig,
                    0,
                    red - this.brig
        };
        Send(packet);
      }
      while (blue <= 255)
      {
        packet.instructions[1].type = InstructionType.RGBUpdate;
        packet.instructions[1].parameters = new object[4]
        {
                    num,
                    blue - this.brig,
                    0,
                    red - this.brig
        };
        Send(packet);
        Wait(10);
        red -= speed;
        blue += speed;
      }
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
        RDR2.UI.Screen.ShowSubtitle("Controller connection status: " + isconnected + " controller battery status: " + bat + "% \n to hide this Press F10");
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
      bool weaponIsReadyToShoot = Function.Call<bool>(Hash.IS_PED_WEAPON_READY_TO_SHOOT, playerPed.Handle);
      bool weaponIsAGun = Function.Call<bool>(Hash._0x705BE297EEBDB95D, (uint)playerweapon.Hash); //IS_WEAPON_A_GUN
      bool weaponIsThrowable = Function.Call<bool>(Hash._0x30E7C16B12DA8211, (uint)playerweapon.Hash); // _IS_WEAPON_THROWABLE

      //uint* numbi = null;
      //var mount = GET_CURRENT_PED_VEHICLE_WEAPON(characterPed.Handle, numbi); //

      bool hasMountedWeapon = playerPed?.Weapons?.Current?.Group == 0 && playerPed.IsSittingInVehicle();

      bool isMounted = hasMountedWeapon && playerPed?.CurrentVehicle != null;

      uint number = 0;

      bool currentPedVehicleWeapon = Function.Call<bool>(Hash.GET_CURRENT_PED_VEHICLE_WEAPON, playerPed.Handle, &number);

      // RDR2.UI.Screen.DisplaySubtitle(weaponIsAGun.ToString());

      if (isMounted)
      {
      }
      // return;
      bool playerIsAiming = player.IsAiming;

      //RDR2.UI.Screen.DisplaySubtitle(weaponIsAGun.ToString());
      // return;

      //RDR2.UI.Screen.DisplaySubtitle(playerweapon.Group.ToString());
      if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED,0, 3901091606 ) )  //INPUT_FRONTEND_LB
      {
        SetAndSendPacket(Trigger.Right);
        SetAndSendPacket(Trigger.Left);
      }
      else if (playerPed.IsReloading) // Mode reloading
      {
        SetAndSendPacket(Trigger.Right);
        SetAndSendPacket(Trigger.Left);
      }
      else if (weaponIsThrowable)
      {
        SetAndSendPacketCustom(Trigger.Left, CustomTriggerValueMode.Rigid, 1, 20);

        SetAndSendPacketCustom(Trigger.Right,
            CustomTriggerValueMode.Pulse, 160, 30, 230);
      }
      else if (playerweapon.Group == WeaponGroup.Bow)
      {
        SetAndSendPacketCustom(Trigger.Left, CustomTriggerValueMode.Rigid, 1, 20);

        if (player.IsAiming)
        {
          SetAndSendPacket(Trigger.Right, TriggerMode.Resistance, new() { 2, 8 });
        }
        else
        {
          SetAndSendPacket(Trigger.Right, TriggerMode.Resistance, new() { 1, 1 });
        }
      }
      else if (weaponIsAGun)
      {
        SetAndSendPacketCustom(Trigger.Left, CustomTriggerValueMode.Rigid, 1, 20);

        float degradation = Function.Call<float>(Hash._0x0D78E1097F89E637, Function.Call<int>(Hash.GET_CURRENT_PED_WEAPON_ENTITY_INDEX, playerPed.Handle, 0)); //GET_WEAPON_DEGRADATION

        // RDR2.UI.Screen.DisplaySubtitle(degradation.ToString());

        if (playerIsAiming && !weaponIsReadyToShoot) // Mode Gun Cock
        {
          SetAndSendPacketCustom(Trigger.Right, CustomTriggerValueMode.Pulse, 1, 15 *
              (int)(1 + degradation));

          // SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 1, 4, 1 + (int)(degradation * 3), 2 });
        }
        else // GUN_MANUAL
        {
          SetAndSendPacketCustom(Trigger.Right, CustomTriggerValueMode.Pulse, 1, 90 *
              (int)(1 + degradation));

          //SetAndSendPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Bow, new() { 0, 4,1+(int)(degradation * 7), 4 });
        }
      }
      else if (currentPedVehicleWeapon)
      {
        if (number == 3666182381 || //gat
         number == 3101324918)// maxi
        {
          SetAndSendPacketCustom(Trigger.Left, CustomTriggerValueMode.Rigid, 1, 20);
          if (playerPed.IsShooting) // Auto
          {
            SetAndSendPacketCustom(Trigger.Right,
                CustomTriggerValueMode.PulseB, 9, 190);
          }
          else // Prepare
          {
            SetAndSendPacketCustom(Trigger.Right,
                CustomTriggerValueMode.Rigid, 30, 255);
          }
        }
        else if (number == 2465730487 || //hotch - cannons
                 number == 1609145491)// breach
        {
          SetAndSendPacketCustom(Trigger.Left, CustomTriggerValueMode.Rigid, 1, 20);
          SetAndSendPacketCustom(Trigger.Right,
              CustomTriggerValueMode.PulseA, 255, 200, 255);
        }
      }
      else // turn off
      {
        SetAndSendPacket(Trigger.Right);
        SetAndSendPacket(Trigger.Left);
      }
      updateLights();
      HandleLEDs();
      return;

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
        RDR2.UI.Screen.ShowSubtitle("controller is disconnected or discharged, please fix or press F11");
      }
      else if (bat <= 15 && showbatstat)
      {
        RDR2.UI.Screen.ShowSubtitle("Your controller battery is  " + bat + " to hide this message press F10");
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
        this.health = playerPed.Health / (playerPed.MaxHealth * 1f);
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
          this.staminaTarget = Function.Call<int>(Hash.GET_MOUNT,  playerPed.Handle);
        }

        float stamina = Function.Call<int>(Hash._0x775A1CA7893AA8B5, this.staminaTarget) * 1f / Function.Call<int>(Hash._GET_PED_MAX_STAMINA, this.staminaTarget) * 1f;
        this.pulseRate = Math.Max(0.2f, Math.Min(1f, 1f - stamina));
        //RDR2.UI.Screen.DisplaySubtitle(playerPed.Health + " - " + playerPed.MaxHealth + " - " + pulseRate);

      }
      // else
      // {
      //     pulseRate = 0;
      // }
    }

    public void HandleLEDs()
    {

      if (pulseRate == 0)
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


      interval_pos += interval_direction * 0.02f * pulseRate;
    }
  }
}