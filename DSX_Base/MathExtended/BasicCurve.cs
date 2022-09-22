using System;
using System.Collections;
using System.Collections.Generic;

namespace DSX_Base.MathExtended;

public class BasicCurve : IEnumerable<CurvePoint>, IEnumerable
{
  private List<CurvePoint> points = new();


  private static Comparison<CurvePoint> CurvePointsComparer = delegate (CurvePoint a, CurvePoint b)
  {
    if (a.x < b.x)
    {
      return -1;
    }
    return (b.x < a.x) ? 1 : 0;
  };





  public BasicCurve()
  {
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  public IEnumerator<CurvePoint> GetEnumerator()
  {
    foreach (CurvePoint point in points)
    {
      yield return point;
    }
  }


  public void Add(float x, float y, bool sort = true)
  {
    CurvePoint newPoint = new(x, y);
    Add(newPoint, sort);
  }

  public void Add(CurvePoint newPoint, bool sort = true)
  {
    points.Add(newPoint);
    if (sort)
    {
      SortPoints();
    }
  }

  public void SortPoints()
  {
    points.Sort(CurvePointsComparer);
  }


  public float Evaluate(float x)
  {
    if (points.Count == 0)
    {
      //Log.Error("Evaluating a SimpleCurve with no points.");
      return 0f;
    }
    if (x <= points[0].x)
    {
      return points[0].y;
    }
    if (x >= points[points.Count - 1].x)
    {
      return points[points.Count - 1].y;
    }
    CurvePoint curvePoint = points[0];
    CurvePoint curvePoint2 = points[points.Count - 1];
    for (int i = 0; i < points.Count; i++)
    {
      if (x <= points[i].x)
      {
        curvePoint2 = points[i];
        if (i > 0)
        {
          curvePoint = points[i - 1];
        }
        break;
      }
    }
    float t = (x - curvePoint.x) / (curvePoint2.x - curvePoint.x);
    return MathExtended.Lerp(curvePoint.y, curvePoint2.y, t);
  }

}