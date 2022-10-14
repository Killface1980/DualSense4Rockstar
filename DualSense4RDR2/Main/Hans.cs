using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using RDR2;
using RDR2.Native;

namespace DualSense4RDR2.Main
{
  internal static class Hans
  {
    public static FishTaskState GetFishTaskState()
    {
      UnsafeFishTaskState data =  default(UnsafeFishTaskState);
      unsafe
      {
        Function.Call(0xF3735ACD11ACD500, Game.Player.Character.Handle, ((IntPtr)(&data)).ToInt32()); //_GET_TASK_FISHING
        return data.GetData();
      }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0xE0)]
    [SecurityCritical]
    internal unsafe struct UnsafeFishTaskState
    {
      [FieldOffset(offset: 0x00)] internal int fishingState;
      [FieldOffset(offset: 0x08)] internal float throwingTargetDistance;
      [FieldOffset(offset: 0x10)] internal float distance;
      [FieldOffset(offset: 0x18)] internal float curvature;
      [FieldOffset(offset: 0x20)] internal float unk0;
      [FieldOffset(offset: 0x28)] internal int hookFlag;
      [FieldOffset(offset: 0x30)] internal int transitionFlag;
      [FieldOffset(offset: 0x38)] internal int fishHandle;
      [FieldOffset(offset: 0x40)] internal float fishweight;
      [FieldOffset(offset: 0x48)] internal float fishPower;
      [FieldOffset(offset: 0x50)] internal int scriptTimer;
      [FieldOffset(offset: 0x58)] internal int hookHandle;
      [FieldOffset(offset: 0x60)] internal int bobberHandle;
      [FieldOffset(offset: 0x68)] internal float rodShakeMult;
      [FieldOffset(offset: 0x70)] internal float unk1;
      [FieldOffset(offset: 0x78)] internal float unk2;
      [FieldOffset(offset: 0x80)] internal int unk3;
      [FieldOffset(offset: 0x88)] internal float unk4; // 17?
      [FieldOffset(offset: 0x90)] internal int fishSizeIndex; // 18?
      [FieldOffset(offset: 0x98)] internal float unk5; // 19 Any unk ?
      [FieldOffset(offset: 0xA0)] internal float tension; // 20 float unk ??
      [FieldOffset(offset: 0xA8)] internal float shakeFightMult; //21
      [FieldOffset(offset: 0xB0)] internal float fishingRodX; //22
      [FieldOffset(offset: 0xB8)] internal float fishingRodY; //23
      [FieldOffset(offset: 0xC0)] internal int unk6;
      [FieldOffset(offset: 0xC8)] internal int unk7;
      [FieldOffset(offset: 0xD0)] internal int unk8;
      [FieldOffset(offset: 0xD8)] internal int unk9;


      public FishTaskState GetData()
      {
        return new FishTaskState(fishingState, throwingTargetDistance, distance, curvature, unk0, hookFlag,
          transitionFlag, fishHandle, fishweight, fishPower, scriptTimer, hookHandle, bobberHandle, rodShakeMult, unk1,
          unk2, unk3, shakeFightMult, fishSizeIndex, fishingRodX, fishingRodY, tension);

        /*
           0:  int minigame_state;             // One of the states above
           1:  float throwing_distance;        // Max throwing distance
           2:  float distance_to_hook;         // Distance between the ped and the fishing hook
           3:  float fishing_line_curvature;
           4:  float unk0;
           5:  int hookFlag;                       // Flag; (n | 1) when pressing INPUT_ATTACK (hooking); (n | 4096) when fishing on a boat
           6:  int transition_flag
           7:  Entity fish;
           8:  float fishweight                      // Calculated fish weight (value / 54.25).
           9:  float fishPower;                     // Fish current power? / Heading?
           10: int script_timer;
           11: Entity hook;
           12: Entity bobber;
           13: float fishing_rod_line_shake;
           14: float unk1;
           15: float unk2;
           16: int unk3;                       // Some kind of state (0 - 7)
           17: float
           18: int fishing_rod_weight;         // (0 - 4); when on 4 the ped starts reeling in
           19: Any unk;
           20: float unk;
           21: float fishing_rod_line_shake;
           22: float fishing_rod_position_lr;  // -1.0: Center, 0.0: Right, 1.0: Left
           23: float fishing_rod_position_ud;  // -1.0: Center, 0.0: Down, 1.0: Up
           24: float unk;                      // range 7.0 - 15.0 on decompiled scripts
           25: float unk;                      // range 10.0 - 20.0 on decompiled scripts
           26: float unk;
           27: float unk;                      // in radians
         */
      }
    }

    public struct FishTaskState
    {
      public int FishingState { get; set; }
      public float ThrowingTargetDistance { get; set; }
      public float Distance { get; set; }
      public float Curvature { get; set; }
      public float Unk0 { get; set; }
      public int HookFlag { get; set; }
      public int TransitionFlag { get; set; }
      public int FishHandle { get; set; }
      public float FishWeight { get; set; }
      public float FishPower { get; set; }
      public int ScriptTimer { get; set; }
      public int HookHandle { get; set; }
      public int BobberHandle { get; set; }
      public float RodShakeMultiplier { get; set; }
      public float Unk1 { get; set; }
      public float Unk2 { get; set; }
      public int Unk3 { get; set; }
      public float ShakeFightMultiplier { get; set; }
      public int FishsizeIndex { get; set; }
      public float FishingRodX { get; set; }
      public float FishingRodY { get; set; }
      public float Tension { get; set; }

      public FishTaskState(int fishingState, float throwingTargetDistance, float distance, float curvature, float unk0, int hookFlag, int transitionFlag, int fishHandle, float fishweight, float fishPower, int scriptTimer, int hookHandle, int bobberHandle, float rodShakeMult, float unk1, float unk2, int unk3, float shakeFightMult, int fishSizeIndex, float fishingRodX, float fishingRodY, float tension)
      {
        FishingState = fishingState;
        ThrowingTargetDistance = throwingTargetDistance;
        Distance = distance;
        Curvature = curvature;
        Unk0 = unk0;
        HookFlag = hookFlag;
        TransitionFlag = transitionFlag;
        FishHandle = fishHandle;
        FishWeight = fishweight;
        FishPower = fishPower;
        ScriptTimer = scriptTimer;
        HookHandle = hookHandle;
        BobberHandle = bobberHandle;
        RodShakeMultiplier = rodShakeMult;
        Unk1 = unk1;
        Unk2 = unk2;
        Unk3 = unk3;
        ShakeFightMultiplier = shakeFightMult;
        FishsizeIndex = fishSizeIndex;
        FishingRodX = fishingRodX;
        FishingRodY = fishingRodY;
        Tension = tension;

      }

      internal UnsafeFishTaskState GetStruct()
      {
        return new UnsafeFishTaskState
        {
          fishingState = FishingState,
          throwingTargetDistance = ThrowingTargetDistance,
          distance = Distance,
          curvature = Curvature,
          unk0 = Unk0,
          hookFlag = HookFlag,
          transitionFlag = TransitionFlag,
          fishHandle = FishHandle,
          fishweight = FishWeight,
          fishPower = FishPower,
          scriptTimer = ScriptTimer,
          hookHandle = HookHandle,
          bobberHandle = BobberHandle,
          rodShakeMult = RodShakeMultiplier,
          unk1 = Unk1,
          unk2 = Unk2,
          unk3 = Unk3,
          shakeFightMult = ShakeFightMultiplier,
          fishSizeIndex = FishsizeIndex,
          fishingRodX = FishingRodX,
          fishingRodY = FishingRodY,
          tension = Tension,

        };

      }

    }

    public enum FishingStates
    {
      Not_holding_a_fishind_rod, //0
      Idling, //1
      Aiming, //2
      About_to_throw, //3
      Throwing, // 4
      unknown1,// (never seen, unused in scripts). //5
      Fishing_idle, //6
      Pos_hook_battling_with_the_fish, //7
      unknown2, //(never seen). Removes prompts it seems. //8
      Removes_the_bobber, //9
      unknown3,//(never seen). //10
      unknown4, //... //11
      Caught_fish_holding_in_hand, //12
      Changing_bait //13
    }

  }
}
