﻿// Verse.CurvePoint

using System.Numerics;

public struct CurvePoint
{
  private Vector2 loc;

  public Vector2 Loc => loc;

  public float x => loc.X;

  public float y => loc.Y;

  public CurvePoint(float x, float y)
  {
    loc = new Vector2(x, y);
  }

  public CurvePoint(Vector2 loc)
  {
    this.loc = loc;
  }

  // public static CurvePoint FromString(string str)
  // {
  //   return new CurvePoint(ParseHelper.FromString<Vector2>(str));
  // }
  // 
  // public override string ToString()
  // {
  //   return loc.ToStringTwoDigits();
  // }

  public static implicit operator Vector2(CurvePoint pt)
  {
    return pt.loc;
  }
}