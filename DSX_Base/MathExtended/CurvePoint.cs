
using System.Numerics;

namespace DSX_Base.MathExtended;

public struct CurvePoint
{
  private Vector2 loc;

  public float x => loc.X;

  public float y => loc.Y;

  public CurvePoint(float x, float y)
  {
    loc = new Vector2(x, y);
  }


  public static implicit operator Vector2(CurvePoint pt)
  {
    return pt.loc;
  }
}