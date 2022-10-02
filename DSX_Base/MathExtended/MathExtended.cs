namespace DSX_Base.MathExtended;

public struct MathExtended
{


  public static float Clamp01(float value)
  {
    if (value < 0f)
    {
      return 0f;
    }
    if (value > 1f)
    {
      return 1f;
    }
    return value;
  }

  /// <summary>
  /// Linearly interpolates between a and b by t.
  /// 
  /// The parameter t is clamped to the range [0, 1].
  /// 
  /// When t = 0 returns a.
  /// When t = 1 return b.
  /// When t = 0.5 returns the midpoint of a and b.
  /// </summary>
  /// <param name="a">The start value.</param>
  /// <param name="b">The end value.</param>
  /// <param name="t">The interpolation value between the two floats.</param>
  /// <returns>float The interpolated float result between the two float values.</returns>
  public static float Lerp(float a, float b, float t)
  {
    return a + (b - a) * Clamp01(t);
  }





  /// <summary>
  /// Determines where a value lies between two points.
  /// 
  /// The a and b values define the start and end of a linear numeric range. The "value" parameter you supply represents a value which might lie somewhere within that range. This method calculates where, within the specified range, the "value" parameter falls.
  /// If the "value" parameter is within the range, InverseLerp returns a value between zero and one, proportional to the value's position within the range. If the "value" parameter falls outside of the range, InverseLerp returns either zero or one, depending on whether it falls before the start of the range or after the end of the range.
  /// </summary>
  /// <param name="a">The start of the range.</param>
  /// <param name="b">The end of the range.</param>
  /// <param name="value">The point within the range you want to calculate.</param>
  /// <returns>float A value between zero and one, representing where the "value" parameter falls within the range defined by a and b.</returns>
  public static float InverseLerp(float a, float b, float value)
  {
    if (a != b)
    {
      return Clamp01((value - a) / (b - a));
    }
    return 0f;
  }


}