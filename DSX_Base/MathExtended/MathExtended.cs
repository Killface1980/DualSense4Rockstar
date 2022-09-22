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

  public static float Lerp(float a, float b, float t)
  {
    return a + (b - a) * Clamp01(t);
  }







  public static float InverseLerp(float a, float b, float value)
  {
    if (a != b)
    {
      return Clamp01((value - a) / (b - a));
    }
    return 0f;
  }


}