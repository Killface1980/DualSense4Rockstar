using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSX_Base.Client
{
  public static class DSX_Math
  {
    /// <summary>
    /// Returns float: A value between zero and one, representing where the "value" parameter falls within the range defined by a and b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static float InvLerp(float a, float b, float v)
    {
      return (v - a) / (b - a);
    }

    /// <summary>
    /// Returns float: The interpolated float result between the two float values. Linearly interpolates between a and b by t. The parameter t is clamped to the range[0, 1].
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static float Lerp(float a, float b, float t)
    {
      //return firstFloat * by + secondFloat * (1 - by);
      return (1f - t) * a + b * t;
    }

    /// <summary>
    /// Returns float: A value between zero and one, representing where the "value" parameter falls within the range defined by a and b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static float InvLerpCapped(float a, float b, float v)
    {
      return Math.Max(0, Math.Min(1, (v - a) / (b - a)));
    }

    /// <summary>
    /// Returns float: The interpolated float result between the two float values. Linearly interpolates between a and b by t. The parameter t is clamped to the range[0, 1].
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static float LerpCapped(float a, float b, float t)
    {
      //return firstFloat * by + secondFloat * (1 - by);
      return Math.Max(0, Math.Min(1, (1f - t) * a + b * t));
    }


    private static int LerpInt(float a, float b, float t)
    {
      //return firstFloat * by + secondFloat * (1 - by);
      return (int)((1f - t) * a + t * b);
    }
  }
}
