// UnityEngine.Mathf
using System;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;

public struct Mathf
{
  public const float PI = (float)Math.PI;

  public const float Infinity = float.PositiveInfinity;

  public const float NegativeInfinity = float.NegativeInfinity;

  public const float Deg2Rad = (float)Math.PI / 180f;

  public const float Rad2Deg = 57.29578f;



  public static float Sin(float f)
  {
	return (float)Math.Sin(f);
  }

  public static float Cos(float f)
  {
	return (float)Math.Cos(f);
  }

  public static float Tan(float f)
  {
	return (float)Math.Tan(f);
  }

  public static float Asin(float f)
  {
	return (float)Math.Asin(f);
  }

  public static float Acos(float f)
  {
	return (float)Math.Acos(f);
  }

  public static float Atan(float f)
  {
	return (float)Math.Atan(f);
  }

  public static float Atan2(float y, float x)
  {
	return (float)Math.Atan2(y, x);
  }

  public static float Sqrt(float f)
  {
	return (float)Math.Sqrt(f);
  }

  public static float Abs(float f)
  {
	return Math.Abs(f);
  }

  public static int Abs(int value)
  {
	return Math.Abs(value);
  }

  public static float Min(float a, float b)
  {
	return (a < b) ? a : b;
  }

  public static float Min(params float[] values)
  {
	int num = values.Length;
	if (num == 0)
	{
	  return 0f;
	}
	float num2 = values[0];
	for (int i = 1; i < num; i++)
	{
	  if (values[i] < num2)
	  {
		num2 = values[i];
	  }
	}
	return num2;
  }

  public static int Min(int a, int b)
  {
	return (a < b) ? a : b;
  }

  public static int Min(params int[] values)
  {
	int num = values.Length;
	if (num == 0)
	{
	  return 0;
	}
	int num2 = values[0];
	for (int i = 1; i < num; i++)
	{
	  if (values[i] < num2)
	  {
		num2 = values[i];
	  }
	}
	return num2;
  }

  public static float Max(float a, float b)
  {
	return (a > b) ? a : b;
  }

  public static float Max(params float[] values)
  {
	int num = values.Length;
	if (num == 0)
	{
	  return 0f;
	}
	float num2 = values[0];
	for (int i = 1; i < num; i++)
	{
	  if (values[i] > num2)
	  {
		num2 = values[i];
	  }
	}
	return num2;
  }

  public static int Max(int a, int b)
  {
	return (a > b) ? a : b;
  }

  public static int Max(params int[] values)
  {
	int num = values.Length;
	if (num == 0)
	{
	  return 0;
	}
	int num2 = values[0];
	for (int i = 1; i < num; i++)
	{
	  if (values[i] > num2)
	  {
		num2 = values[i];
	  }
	}
	return num2;
  }

  public static float Pow(float f, float p)
  {
	return (float)Math.Pow(f, p);
  }

  public static float Exp(float power)
  {
	return (float)Math.Exp(power);
  }

  public static float Log(float f, float p)
  {
	return (float)Math.Log(f, p);
  }

  public static float Log(float f)
  {
	return (float)Math.Log(f);
  }

  public static float Log10(float f)
  {
	return (float)Math.Log10(f);
  }

  public static float Ceil(float f)
  {
	return (float)Math.Ceiling(f);
  }

  public static float Floor(float f)
  {
	return (float)Math.Floor(f);
  }

  public static float Round(float f)
  {
	return (float)Math.Round(f);
  }

  public static int CeilToInt(float f)
  {
	return (int)Math.Ceiling(f);
  }

  public static int FloorToInt(float f)
  {
	return (int)Math.Floor(f);
  }

  public static int RoundToInt(float f)
  {
	return (int)Math.Round(f);
  }

  public static float Sign(float f)
  {
	return (f >= 0f) ? 1f : (-1f);
  }

  public static float Clamp(float value, float min, float max)
  {
	if (value < min)
	{
	  value = min;
	}
	else if (value > max)
	{
	  value = max;
	}
	return value;
  }

  public static int Clamp(int value, int min, int max)
  {
	if (value < min)
	{
	  value = min;
	}
	else if (value > max)
	{
	  value = max;
	}
	return value;
  }

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

  public static float LerpUnclamped(float a, float b, float t)
  {
	return a + (b - a) * t;
  }

  public static float LerpAngle(float a, float b, float t)
  {
	float num = Repeat(b - a, 360f);
	if (num > 180f)
	{
	  num -= 360f;
	}
	return a + num * Clamp01(t);
  }

  public static float MoveTowards(float current, float target, float maxDelta)
  {
	if (Abs(target - current) <= maxDelta)
	{
	  return target;
	}
	return current + Sign(target - current) * maxDelta;
  }

  public static float MoveTowardsAngle(float current, float target, float maxDelta)
  {
	float num = DeltaAngle(current, target);
	if (0f - maxDelta < num && num < maxDelta)
	{
	  return target;
	}
	target = current + num;
	return MoveTowards(current, target, maxDelta);
  }

  public static float SmoothStep(float from, float to, float t)
  {
	t = Clamp01(t);
	t = -2f * t * t * t + 3f * t * t;
	return to * t + from * (1f - t);
  }

  public static float Gamma(float value, float absmax, float gamma)
  {
	bool flag = false;
	if (value < 0f)
	{
	  flag = true;
	}
	float num = Abs(value);
	if (num > absmax)
	{
	  return flag ? (0f - num) : num;
	}
	float num2 = Pow(num / absmax, gamma) * absmax;
	return flag ? (0f - num2) : num2;
  }




  public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
  {
	smoothTime = Max(0.0001f, smoothTime);
	float num = 2f / smoothTime;
	float num2 = num * deltaTime;
	float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
	float value = current - target;
	float num4 = target;
	float num5 = maxSpeed * smoothTime;
	value = Clamp(value, 0f - num5, num5);
	target = current - value;
	float num6 = (currentVelocity + num * value) * deltaTime;
	currentVelocity = (currentVelocity - num * num6) * num3;
	float num7 = target + (value + num6) * num3;
	if (num4 - current > 0f == num7 > num4)
	{
	  num7 = num4;
	  currentVelocity = (num7 - num4) / deltaTime;
	}
	return num7;
  }


  public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
  {
	target = current + DeltaAngle(current, target);
	return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
  }

  public static float Repeat(float t, float length)
  {
	return Clamp(t - Floor(t / length) * length, 0f, length);
  }

  public static float PingPong(float t, float length)
  {
	t = Repeat(t, length * 2f);
	return length - Abs(t - length);
  }

  public static float InverseLerp(float a, float b, float value)
  {
	if (a != b)
	{
	  return Clamp01((value - a) / (b - a));
	}
	return 0f;
  }

  public static float DeltaAngle(float current, float target)
  {
	float num = Repeat(target - current, 360f);
	if (num > 180f)
	{
	  num -= 360f;
	}
	return num;
  }

  internal static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 result)
  {
	float num = p2.X - p1.X;
	float num2 = p2.Y - p1.Y;
	float num3 = p4.X - p3.X;
	float num4 = p4.Y - p3.Y;
	float num5 = num * num4 - num2 * num3;
	if (num5 == 0f)
	{
	  return false;
	}
	float num6 = p3.X - p1.X;
	float num7 = p3.Y - p1.Y;
	float num8 = (num6 * num4 - num7 * num3) / num5;
	result.X = p1.X + num8 * num;
	result.Y = p1.Y + num8 * num2;
	return true;
  }

  internal static bool LineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 result)
  {
	float num = p2.X - p1.X;
	float num2 = p2.Y - p1.Y;
	float num3 = p4.X - p3.X;
	float num4 = p4.Y - p3.Y;
	float num5 = num * num4 - num2 * num3;
	if (num5 == 0f)
	{
	  return false;
	}
	float num6 = p3.X - p1.X;
	float num7 = p3.Y - p1.Y;
	float num8 = (num6 * num4 - num7 * num3) / num5;
	if (num8 < 0f || num8 > 1f)
	{
	  return false;
	}
	float num9 = (num6 * num2 - num7 * num) / num5;
	if (num9 < 0f || num9 > 1f)
	{
	  return false;
	}
	result.X = p1.X + num8 * num;
	result.Y = p1.Y + num8 * num2;
	return true;
  }

  internal static long RandomToLong(System.Random r)
  {
	byte[] array = new byte[8];
	r.NextBytes(array);
	return (long)(BitConverter.ToUInt64(array, 0) & 0x7FFFFFFFFFFFFFFFL);
  }

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void CorrelatedColorTemperatureToRGB_Injected(float kelvin, out Color ret);
}
