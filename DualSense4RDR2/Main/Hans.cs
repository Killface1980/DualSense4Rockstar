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
        Function.Call(0xF3735ACD11ACD500, Game.Player.Character.Handle, &data); //_GET_TASK_FISHING
        /*        Function.Call(0xF3735ACD11ACD500, Game.Player.Character.Handle,  ((IntPtr)(&data)).ToInt32()); //_GET_TASK_FISHING
        */
        return data.GetData();
      }
    }

    [StructLayout(LayoutKind.Explicit, Size = 224)]
    [SecurityCritical]
    internal unsafe struct UnsafeFishTaskState
    {
      [FieldOffset(0)] internal int fishingState;//01 One of the states above
      [FieldOffset(8)] internal float throwingTargetDistance;//02 Max throwing distance
      [FieldOffset(16)] internal float distance;//03 Distance between the ped and the fishing hook
      [FieldOffset(24)] internal float fishing_line_curvature;//04
      [FieldOffset(32)] internal float unk0;//05
      [FieldOffset(40)] internal int hookFlag;// 06 Flag; (n | 1) when pressing INPUT_ATTACK (hooking); (n | 4096) when fishing on a boat
      [FieldOffset(48)] internal int transitionFlag;//07
      [FieldOffset(56)] internal int fishHandle;//08
      [FieldOffset(64)] internal float fishweight;//09 Calculated fish weight (value / 54.25).
      [FieldOffset(72)] internal float fishPower;//10 Fish current power? / Heading?
      [FieldOffset(80)] internal int scriptTimer;//11
      [FieldOffset(88)] internal int hookHandle;//12
      [FieldOffset(96)] internal int bobberHandle;//13
      [FieldOffset(104)] internal float rodShakeMult;//14
      [FieldOffset(112)] internal float unk1;//14
      [FieldOffset(120)] internal float unk2;//15
      [FieldOffset(128)] internal int unk3;// 16 Some kind of state (0 - 7)
      [FieldOffset(136)] internal float unk4; // 17?
      [FieldOffset(144)] internal int fishSizeIndex; // 18?
      [FieldOffset(152)] internal float unk5; // 19 Any unk ?
      [FieldOffset(160)] internal float tension; // 20 float unk ??
      [FieldOffset(168)] internal float shakeFightMult; //21
      [FieldOffset(176)] internal float fishingRodX; //22
      [FieldOffset(184)] internal float fishingRodY; //23
      [FieldOffset(192)] internal float unk6;//24
      [FieldOffset(200)] internal float unk7;//25
      [FieldOffset(208)] internal float unk8;//26
      [FieldOffset(216)] internal float unk9; //27


      public FishTaskState GetData()
      {
        return new FishTaskState(fishingState, throwingTargetDistance, distance, fishing_line_curvature, unk0, hookFlag,
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
          fishing_line_curvature = Curvature,
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
