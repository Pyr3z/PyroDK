/**
@file   PyroDK/Core/Utilities/Floats.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-06

@brief
Provides utilities for native floating-point primitives.
**/

using UnityEngine;


namespace PyroDK
{

  public static class Floats
  {
    // my chosen epsilon that is accurate enough for most game applications:
    public const float EPSILON  = 1e-6f; // (Unity tends to use 1e-5f, i.e. Vector3.kEpsilon)
    public const float EPSILON2 = EPSILON * EPSILON;

    public const float PIPI  = 2 * Mathf.PI;
    public const float IPIPI = 1f / PIPI;
    
    public const float SQRT2    = 1.414213562373095f; // precomputed.
    public const float SQRT_MAX = 1.844674352e+19f;   // the 32-bit IEEE max squarable value


    public static float SinusoidalParameter(float t)
    {
      // "stretches" the normalized value `t` so that 1.0 corresponds to 2 * PI,
      // then uses cosine to produce an oscillating value between 0.0 and 1.0
      return Clamp01((Mathf.Cos(t * PIPI) - 1f) * 0.5f);
    }

    public static float InverseSinusoidalParameter(float t)
    {
      return (Mathf.Acos(-2f * t + 1f)) * IPIPI;
    }

    public static float SmoothStepParameter(float t)
    {
      if (t < 0f) return 0f;
      if (1f < t) return 1f;
      return (t * t) * (3f - 2f * t);
    }



    public static float SinusoidalLoop(float a, float b, float t)
    {
      return Mathf.LerpUnclamped(a, b, SinusoidalParameter(t));
    }



    public static void Swap(ref float val0, ref float val1)
    {
      float swap = val0;
      val0 = val1;
      val1 = swap;
    }

    public static void EnsureAscending(ref float val0, ref float val1)
    {
      if (val1 < val0)
      {
        float swap = val0;
        val0 = val1;
        val1 = swap;
      }
    }



    public static bool Approximately(this float a, float b)
    {
      return (a -= b) * a < EPSILON2;
    }

    public static bool Approximately(this float a, float b, float epsilon)
    {
      return (a -= b) * a < epsilon * epsilon;
    }

    public static bool IsZero(this float val)
    {
      return val * val < EPSILON2;
    }

    public static bool IsZero(this float val, float epsilon)
    {
      return val * val < epsilon * epsilon;
    }

    public static bool IsNegativeZero(this float val)
    {
      return val == 0f && Bitwise.GetByte(val, 3) == 0x80;
    }

    public static bool IsPositive(this float val) // only to check for IEEE negative zero
    {
      // no longer uses EPSILON -- merely checks for the sign bit.
      return (Bitwise.GetByte(val, 3) & 0x80) != 0x80;
    }
    public static bool IsNegative(this float val) // only to check for IEEE negative zero
    {
      // no longer uses EPSILON -- merely checks for the sign bit.
      return (Bitwise.GetByte(val, 3) & 0x80) == 0x80;
    }

    public static bool IsNaN(this float val)
    {
      return float.IsNaN(val);
    }

    public static bool IsFinite(this float val)
    {
      return !float.IsNaN(val) && !float.IsInfinity(val);
    }

    public static bool IsSquarable(this float val)
    {
      return !float.IsNaN(val) && !float.IsPositiveInfinity(val * val);
    }

    public static float FixNaN(this float val)
    {
      return float.IsNaN(val) ? 0f : val;
    }

    public static float FixNaN(this float val, float fallback)
    {
      return float.IsNaN(val) ? fallback : val;
    }


    public static float MakeFinite(this float val, float finite)
    {
      if (float.IsNaN(val))
        return finite;
      if (float.IsInfinity(val))
        return (0f < val) ? finite : -finite;

      return val;
    }

    public static float ClampSquarable(this float val)
    {
      if (float.IsNaN(val) || !float.IsPositiveInfinity(val * val))
        return val;

      return Sign(val) * SQRT_MAX;
    }


    public static float Abs(this float val)
    {
      return System.Math.Abs(val);
    }


    public static float Sign(this float val)
    {
      if (EPSILON < val)
        return 1f;
      if (val < -EPSILON)
        return -1f;

      return 0f;
    }

    public static float SignOrNaN(this float val)
    {
      if (EPSILON < val)
        return 1f;
      if (val < -EPSILON)
        return -1f;
      if (float.IsNaN(val))
        return float.NaN;

      return 0f;
    }

    public static float SignNoZero(this float val)
    {
      if (val < 0f)
        return -1f;
      return 1f;
    }


    public static float Sqrt(this float val)
    {
      return (float)System.Math.Sqrt(val);
    }

    public static float Clamp(this float val, float min, float max)
    {
      if (min <= max)
      {
        if (val < min) return min;
        if (max < val) return max;
      }
      
      return val;
    }

    public static float Clamp01(this float val)
    {
      if (val < 0f) return 0f;
      if (1f < val) return 1f;
      return val;
    }

    public static float AtMost(this float val, float most)
    {
      return (most < val) ? most : val;
    }

    public static float AtLeast(this float val, float least)
    {
      return (val < least) ? least : val;
    }

    public static float Squeezed(this float val)
    {
      if (float.IsNaN(val) || val * val < EPSILON2)
        return 0f;

      return val;
    }

    public static float Squeezed(this float val, float epsilon)
    {
      if (float.IsNaN(val) || val * val < epsilon * epsilon)
        return 0f;

      return val;
    }


    public static float SmoothSteppedTo(this float from, float to, float t)
    {
      return Mathf.LerpUnclamped(from, to, SmoothStepParameter(t));
    }

    public static float LerpedTo(this float from, float to, float t)
    {
      return Mathf.LerpUnclamped(from, to, t);
    }

  }


  public static class Doubles
  {

    public const double EPSILON   = 1e-10;
    public const double EPSILON2  = EPSILON * EPSILON;
    public const double PI        = System.Math.PI;
    public const double PIPI      = PI + PI;
    public const double SQRT2     = 1.414213562373095;


    public static double SinusoidalParameter(double t)
    {
      // "stretches" the normalized value `t` so that 1.0 corresponds to 2 * PI,
      // then uses cosine to produce an oscillating value between 0.0 and 1.0
      return (System.Math.Cos(t * PIPI) - 1.0) / -2.0;
    }

    public static double SinusoidalLoop(double a, double b, double t)
    {
      return SinusoidalParameter(t) * (b - a) + a;
    }

    public static bool Approximately(this double a, double b)
    {
      return (a -= b) * a < EPSILON2;
    }

    public static bool Approximately(this double a, double b, double epsilon)
    {
      return (a -= b) * a < epsilon * epsilon;
    }

    public static bool IsZero(this double val)
    {
      return val * val < EPSILON2;
    }

    public static double Squeezed(this double val)
    {
      if (val * val < EPSILON2)
        return 0.0;

      return val;
    }

    public static double Squeezed(this double val, double epsilon)
    {
      if (val * val < epsilon * epsilon)
        return 0.0;

      return val;
    }

  }

}
