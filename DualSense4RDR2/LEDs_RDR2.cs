using DSX_Base.MathExtended;
using RDR2;
using RDR2.Native;
using DSX_Base;
using System;
using System.Drawing;
using System.Windows.Forms;
using static DSX_Base.Client.iO;

namespace DualSense4RDR2;

public class LEDs_RDR2 : Script
{
    private static bool showbatstat = true;
    private static bool showconmes  = true;
    private readonly BasicCurve pulseRateCurve;
    private float currentHealth         = 0;
    private float currentStaminaDisplay = 1f;
    private float flashy;
    private int interval_direction = 1;
    private float interval_pos     = 0;
    private float lastHealth       = 0;
    private int staminaTarget      = 0;

    public LEDs_RDR2()
    {
        pulseRateCurve = new BasicCurve()
    {
      new(0f, 4f),
      new(0.2f, 2f),
      new(0.7f, 1.2f),
      new(1f, 0.3f)
    };

        Tick    += OnTick;
        KeyDown += OnKeyDown;

    }


    public void HandleLEDs()
    {
        Ped playerPed = Game.Player.Character;

        // if (ScriptSettings::getBool("HealthIndication"))
        {
          // currentHealth = ENTITY._GET_ENTITY_HEALTH_FLOAT(playerPed.Handle); //not working?

          currentHealth = playerPed.Health * 1f / playerPed.MaxHealth * 1f;
        }
        if (currentHealth < lastHealth - 0.01f)
        {
          flashy += MathExtended.InverseLerp(lastHealth, 0, currentHealth);
          flashy = Math.Min(flashy, 1);
        }

        lastHealth = currentHealth;
        // else
        // {
        //     health = 1;
        // }

        // if (ScriptSettings::getBool("StaminaIndication"))
        {
          staminaTarget = playerPed.IsOnMount ? PED.GET_MOUNT(playerPed.Handle) : playerPed.Handle;

          //float stamina = RDR2.Native.PED._GET_PED_STAMINA(this.staminaTarget)*1f / RDR2.Native.PED._GET_PED_MAX_STAMINA(this.staminaTarget)*1f;
          float stamina = PED._GET_PED_STAMINA_NORMALIZED(staminaTarget);
          currentStaminaDisplay = MathExtended.Lerp(0, 2, stamina); // DSX_Math.LerpCapped(0.1f,0.8f,stamina);
          //this.pulseRate = DSX_Math.LerpCapped(1f, 0.2f, stamina);// Math.Max(0.2f, Math.Min(1f, 1f - stamina));
          //RDR2.UI.Screen.DisplaySubtitle(playerPed.Health + " - " + playerPed.MaxHealth + " - " + pulseRate);
        }
        // else
        // {
        //     pulseRate = 0;
        // }


        bool playerIsInDeadEye = Game.Player.IsInDeadEye;

        bool healthCoreOverpowered  = ATTRIBUTE._IS_ATTRIBUTE_CORE_OVERPOWERED(playerPed.Handle, (int)eAttributeCore.Health);
        bool staminaCoreOverpowered = ATTRIBUTE._IS_ATTRIBUTE_CORE_OVERPOWERED(playerPed.Handle, (int)eAttributeCore.Stamina);
        bool deadEyeCoreOverpowered = ATTRIBUTE._IS_ATTRIBUTE_CORE_OVERPOWERED(playerPed.Handle, (int)eAttributeCore.DeadEye);

        float healthCore  = ATTRIBUTE._GET_ATTRIBUTE_CORE_VALUE(playerPed.Handle, (int)eAttributeCore.Health) * 0.01f + (healthCoreOverpowered ? 1f : 0);
        float staminaCore = ATTRIBUTE._GET_ATTRIBUTE_CORE_VALUE(playerPed.Handle, (int)eAttributeCore.Stamina) * 0.01f + (staminaCoreOverpowered ? 1f : 0);
        float deadEyeCore = ATTRIBUTE._GET_ATTRIBUTE_CORE_VALUE(playerPed.Handle, (int)eAttributeCore.DeadEye) * 0.01f + (deadEyeCoreOverpowered ? 1f : 0);

        bool healthCoreLow  = healthCore < 0.2f;
        bool staminaCoreLow = staminaCore < 0.2f;
        bool deadEyeCoreLow = deadEyeCore < 0.2f;

        bool shouldFlash = playerIsInDeadEye || currentStaminaDisplay <= 0.99f || healthCoreLow || staminaCoreLow || deadEyeCoreLow;

        if (!shouldFlash && interval_pos >= 0.99f)
        {
            interval_pos = 1;
        }
        else if (interval_direction == -1 && interval_pos <= 0.05f)
        {
            interval_direction = 1;
            interval_pos = Math.Max(0.05f, interval_pos);
        }
        else if (interval_direction == 1 && interval_pos >= 1)
        {
            interval_pos = 1;
            interval_direction = -1;
        }

        float green = currentHealth * 180f;
        float red   = 180f - green;
        float blue  = 0;

        //if (ATTRIBUTE._IS_ATTRIBUTE_CORE_OVERPOWERED(playerPed.Handle, (int)PedCore.Health))
        //{
        // RDR2.UI.Screen.DisplaySubtitle(ATTRIBUTE._GET_ATTRIBUTE_CORE_OVERPOWER_SECONDS_LEFT(playerPed.Handle, (int)PedCore.Health).ToString());
        //}

        // Color neutral = Color.FromArgb(120,120,120);
        // 
        // Color lowHealth = Color.FromArgb(180,0,0);
        // Color damageFlash = Color.FromArgb(255,0,0);
        // 
        // Color inDeadEye= Color.FromArgb(255,120,0);
        // 
        // Color overPoweredHealth = Color.FromArgb(255,0,255);
        // Color overPoweredStamina = Color.FromArgb(0,255,255);
        // Color overPoweredDeadEye = Color.FromArgb(255,255,0);
        // 
        // Color coreLowHealth = Color.FromArgb(90, 0, 90);
        // Color coreLowStamina = Color.FromArgb(0, 90, 90);
        // Color coreLowDeadEye = Color.FromArgb(90, 90, 0);

        float evaluator = 0f;

        if (playerIsInDeadEye)
        {
            // DoColourLerp(neutral, inDeadEye, interval_pos, out red, out green, out blue);

            blue      = 0;
            red       = 255 * interval_pos;
            green     = 120 * interval_pos;
            evaluator = 0.1f;

        }
        else if (healthCoreLow)
        {
            // DoColourLerp(neutral, coreLowHealth, interval_pos, out red, out green, out blue);

            red      += interval_pos * 255;
            green    -= red / 2;
            evaluator = healthCore * 5f;
        }
        // else if (staminaCoreLow)
        // {
        //   DoColourLerp(neutral, coreLowStamina, interval_pos, out red, out green, out blue);
        // 
        //   // red += interval_pos * 255;
        //   // green -= red / 2;
        //   evaluator = staminaCore * 5f;
        // }
        // else if (deadEyeCoreLow)
        // {
        //   DoColourLerp(neutral, coreLowDeadEye, interval_pos, out red, out green, out blue);
        // 
        //   // red += interval_pos * 255;
        //   // green -= red / 2;
        //   evaluator = deadEyeCore * 5f;
        // }
        else if (currentStaminaDisplay <= 0.99f) // stamina
        {
            //DoColourLerp(neutral, coreLowDeadEye, interval_pos, out red, out green, out blue);

            red      *= interval_pos / 2;
            green    *= interval_pos / 2;
            blue     *= interval_pos / 2;
            evaluator = currentStaminaDisplay;
        }


        red += flashy * 255;

        // RDR2.UI.Screen.DisplaySubtitle( + " - " + healthCore.ToString());

        SendPacketRGB(red, green, blue);

        interval_pos += interval_direction * 0.01f * pulseRateCurve.Evaluate(evaluator);
        if (flashy > 0)
        {
            flashy -= 0.05f;
        }
    }

    private void DoColourLerp(Color color1, Color color2, float t, out float red, out float green, out float blue)
    {
        red   = MathExtended.Lerp(color1.R, color2.R, t);
        green = MathExtended.Lerp(color1.G, color2.G, t);
        blue  = MathExtended.Lerp(color1.B, color2.B, t);
    }

    private static void SendPacketRGB(float red, float green, float blue)
    {
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

        if (e.KeyCode == Keys.F10)
        {
            showbatstat = !showbatstat;
        }

        if (e.KeyCode == Keys.F11)
        {
            showconmes = !showconmes;
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        HandleLEDs();

        return;
    }


}