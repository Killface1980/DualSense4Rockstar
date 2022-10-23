using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDR2;

namespace DualSense4RDR2.Stats
{
  public class HorseStat
  {
    public static CurrentHorseStat GetHorseSpeed(Ped horsie)
  {
    // stopped
    // walking 
    // faster, everything else
    // running
    // sprinting
    if (horsie.IsStopped) return CurrentHorseStat.Stopped;
    if (horsie.IsWalking) return CurrentHorseStat.Walking;
    if (horsie.IsRunning) return CurrentHorseStat.Running;
    if (horsie.IsSprinting) return CurrentHorseStat.Sprinting;
    return CurrentHorseStat.Galloping; // galloping
  }

    public enum CurrentHorseStat
    {
      Stopped   = 0,
      Walking   = 1,
      Galloping = 2,
      Running   = 3,
      Sprinting = 4
    }
  }
}
