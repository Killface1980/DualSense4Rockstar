using System.Runtime.InteropServices;
using System.Security;
using GTA.Native;

public static class Hans
{
  // CitizenFX.Core.Game

  public static bool GetTheWeaponHudStats(uint weaponHash, ref WeaponHudStats weaponStats)
  {
    // if (!API.IsWeaponValid(weaponHash))
    {
      //   return false;
    }
    weaponStats = _GetWeaponHudStats(weaponHash);
    return true;
  }

  [SecuritySafeCritical]
  private unsafe static WeaponHudStats _GetWeaponHudStats(uint weaponHash)
  {
    UnsafeWeaponHudStats unsafeStats = default(UnsafeWeaponHudStats);
    Function.Call(Hash.GET_WEAPON_HUD_STATS, weaponHash, &unsafeStats);
    return unsafeStats.GetSafeStats();
  }


// CitizenFX.Core.Game.UnsafeWeaponHudStats

  [StructLayout(LayoutKind.Explicit, Size = 40)]
  [SecuritySafeCritical]
  internal struct UnsafeWeaponHudStats
  {
    [FieldOffset(0)] private int hudDamage;

    [FieldOffset(8)] private int hudSpeed;

    [FieldOffset(16)] private int hudCapacity;

    [FieldOffset(24)] private int hudAccuracy;

    [FieldOffset(32)] private int hudRange;

    public WeaponHudStats GetSafeStats()
    {
      return new WeaponHudStats(hudDamage, hudSpeed, hudCapacity, hudAccuracy, hudRange);
    }
  }
}
