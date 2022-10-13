// CitizenFX.Core.Weapon
/// <summary>
/// Gets the <see cref="T:CitizenFX.Core.Game.WeaponHudStats" /> data from this <see cref="T:CitizenFX.Core.Weapon" />.
/// </summary>




// CitizenFX.Core.Game.WeaponComponentHudStats
public struct WeaponComponentHudStats
{
  public int hudDamage;

  public int hudSpeed;

  public int hudCapacity;

  public int hudAccuracy;

  public int hudRange;

  public WeaponComponentHudStats(int hudDamage, int hudSpeed, int hudCapacity, int hudAccuracy, int hudRange)
  {
    this.hudDamage   = hudDamage;
    this.hudSpeed    = hudSpeed;
    this.hudCapacity = hudCapacity;
    this.hudAccuracy = hudAccuracy;
    this.hudRange    = hudRange;
  }

  public override bool Equals(object obj)
  {
    if (obj is WeaponComponentHudStats stat)
    {
      if (stat.hudDamage == hudDamage && stat.hudSpeed == hudSpeed && stat.hudCapacity == hudCapacity && stat.hudAccuracy == hudAccuracy)
      {
        return stat.hudRange == hudRange;
      }
      return false;
    }
    return false;
  }

  public override int GetHashCode()
  {
    return ((((13 * 7 + hudDamage.GetHashCode()) * 7 + hudSpeed.GetHashCode()) * 7 + hudCapacity.GetHashCode()) * 7 + hudAccuracy.GetHashCode()) * 7 + hudRange.GetHashCode();
  }

  public static bool operator ==(WeaponComponentHudStats left, WeaponComponentHudStats right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(WeaponComponentHudStats left, WeaponComponentHudStats right)
  {
    return !(left == right);
  }
}

// CitizenFX.Core.Game.WeaponHudStats
public struct WeaponHudStats
{
  public int hudDamage;

  public int hudSpeed;

  public int hudCapacity;

  public int hudAccuracy;

  public int hudRange;

  public WeaponHudStats(int hudDamage, int hudSpeed, int hudCapacity, int hudAccuracy, int hudRange)
  {
    this.hudDamage = hudDamage;
    this.hudSpeed = hudSpeed;
    this.hudCapacity = hudCapacity;
    this.hudAccuracy = hudAccuracy;
    this.hudRange = hudRange;
  }

  public override bool Equals(object obj)
  {
    if (obj is WeaponHudStats stat)
    {
      if (stat.hudDamage == hudDamage && stat.hudSpeed == hudSpeed && stat.hudCapacity == hudCapacity && stat.hudAccuracy == hudAccuracy)
      {
        return stat.hudRange == hudRange;
      }
      return false;
    }
    return false;
  }

  public override int GetHashCode()
  {
    return ((((13 * 7 + hudDamage.GetHashCode()) * 7 + hudSpeed.GetHashCode()) * 7 + hudCapacity.GetHashCode()) * 7 + hudAccuracy.GetHashCode()) * 7 + hudRange.GetHashCode();
  }

  public static bool operator ==(WeaponHudStats left, WeaponHudStats right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(WeaponHudStats left, WeaponHudStats right)
  {
    return !(left == right);
  }
}
