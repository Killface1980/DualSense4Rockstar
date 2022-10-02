using DSX_Base.MathExtended;
using RDR2;
using RDR2.Native;
using Shared;
using System;
using System.Windows.Forms;
using static DSX_Base.Client.iO;

namespace DualSense4RDR2.Main;

public class LEDs_RDR2 : Script
{
  private static bool showbatstat = true;
  private static bool showconmes = true;
  private readonly BasicCurve pulseRateCurve;
  private float currentHealth = 0;
  private float currentStaminaDisplay = 1f;
  private float flashy;
  private int interval_direction = 1;
  private float interval_pos = 0;
  private float lastHealth = 0;
  private int staminaTarget = 0;

  public LEDs_RDR2()
  {
    pulseRateCurve = new BasicCurve()
    {
      new(0f, 25f),
      new(0.2f, 8f),
      new(0.7f, 1.2f),
      new(1f, 0.3f)
    };

    Tick += OnTick;
    KeyDown += OnKeyDown;
  }

  public void HandleLEDs()
  {
    if (flashy > 0)
    {
      //RDR2.UI.Screen.DisplaySubtitle(flashy.ToString("N2"));
      flashy -= 0.03f;
    }

    bool playerIsInDeadEye = Game.Player.IsInDeadEye;

    if (!playerIsInDeadEye && currentStaminaDisplay >= 0.99f && interval_pos >= 0.95f)
      interval_pos = 1;
    else if (interval_direction == -1 && interval_pos <= 0.05f)
      interval_direction = 1;
    else if (interval_direction == 1 && interval_pos >= 1)
    {
      interval_direction = -1;
    }

    float green = currentHealth * 255f;
    float red = (255f - green);
    float blue = 0;

    if (playerIsInDeadEye)
    {
      blue = 0;
      red = 255 * interval_pos;
      green = 120 * interval_pos;
    }
    else
    {
      red *= interval_pos;
      green *= interval_pos;

      red += flashy * 255;
      green += flashy * 255;
      blue += flashy * 255;

    }

    Packet packet = new();
    int num = 0;
    packet.instructions = new Instruction[4];
    packet.instructions[1].type = InstructionType.RGBUpdate;
    packet.instructions[1].parameters = new object[]
    {
      num,
      (int)Math.Min(255, red),
      (int)Math.Min(255, green),
      (int)Math.Min(255, blue)
    };
    Send(packet);

    float pulseRate =  pulseRateCurve.Evaluate(playerIsInDeadEye ? 0.5f : currentStaminaDisplay);
    interval_pos += interval_direction * 0.01f * pulseRate;
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
      RDR2.UI.Screen.DisplaySubtitle("Controller connection status: " + isconnected + " controller battery status: " +
                                     bat + "% \n to hide this Press F10");
    }

    if (e.KeyCode == Keys.F10) showbatstat = !showbatstat;
    if (e.KeyCode == Keys.F11) showconmes = !showconmes;
  }

  private void OnTick(object sender, EventArgs e)
  {
    Packet packet = new();
    packet.instructions = new Instruction[4];

    updateLights();
    HandleLEDs();

    return;

    //RDR2.UI.Screen.DisplaySubtitle(health + " - " + pulseRate + " - " + staminaTarget);
    add add2 = new();
    iO obj = new();
    Send(packet);
    Wait(235);
    obj.getstat(out int bat, out bool isConnected);

    if (!isConnected && showconmes)
      RDR2.UI.Screen.DisplaySubtitle("controller is disconnected or discharged, please fix or press F11");
    else if (bat <= 15 && showbatstat)
      RDR2.UI.Screen.DisplaySubtitle("Your controller battery is  " + bat + " to hide this message press F10");
    // RDR2.UI.Screen.ShowHelpMessage("Your controller battery is  " + bat + " to hide this message press F10", 1, sound: false);
  }

  private void updateLights()
  {
    var playerPed = Game.Player.Character;

    // if (ScriptSettings::getBool("HealthIndication"))
    {
      // currentHealth = ENTITY._GET_ENTITY_HEALTH_FLOAT(playerPed.Handle); //not working?

      currentHealth = playerPed.Health * 1f / playerPed.MaxHealth * 1f;
    }
    if (currentHealth < lastHealth - 0.03f)
    {
      flashy += MathExtended.InverseLerp(0, lastHealth, currentHealth);
    }

    lastHealth = currentHealth;
    // else
    // {
    //     health = 1;
    // }

    // if (ScriptSettings::getBool("StaminaIndication"))
    {
      staminaTarget = playerPed.Handle;
      if (playerPed.IsOnMount) staminaTarget = PED.GET_MOUNT(playerPed.Handle);

      //float stamina = RDR2.Native.PED._GET_PED_STAMINA(this.staminaTarget)*1f / RDR2.Native.PED._GET_PED_MAX_STAMINA(this.staminaTarget)*1f;
      float stamina = PED._GET_PED_STAMINA_NORMALIZED(staminaTarget);
      currentStaminaDisplay = stamina; // DSX_Math.LerpCapped(0.1f,0.8f,stamina);
      //this.pulseRate = DSX_Math.LerpCapped(1f, 0.2f, stamina);// Math.Max(0.2f, Math.Min(1f, 1f - stamina));
      //RDR2.UI.Screen.DisplaySubtitle(playerPed.Health + " - " + playerPed.MaxHealth + " - " + pulseRate);
    }
    // else
    // {
    //     pulseRate = 0;
    // }
  }
}