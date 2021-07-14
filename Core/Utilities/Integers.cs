﻿/**
@file   PyroDK/Core/Utilities/Integers.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-06

@brief
  Provides utilities for native integral primitives and other types that
  implement `System.IConvertible`.
**/


namespace PyroDK
{
  using IConvertible = System.IConvertible;


  public static class Integers
  {
    // Maximum 1D array size, slightly smaller than int.MaxValue
    public const int MAX_1D_SIZE    = 2146435069;

    // logical 2D arrays assume a square grid:
    public const int MAX_2D_SIZE    = 46329; // floor(sqrt(MAX_1D_SIZE))
    public const int MAX_2D_EXTENT  = 23164; // MAX_2D_SIZE / 2


    public static int CalcDecimalDigits(int self)
    {
      return ( self < 10 ) ? 1 : (int)System.Math.Log10(self - 1) + 1;
    }

    public static string MakeIndexPreformattedString(int size)
    {
      return $"[{{0,{CalcDecimalDigits(size)}}}]";
    }


    public static long Abs(this long self) // branchless!
    {
      long mask = self >> 63;
      return (self + mask) ^ mask;
    }

    public static int Abs(this int self)
    {
      int mask = self >> 31;
      return (self + mask) ^ mask;
    }


    public static int Sign(this int self)
    {
      return (self > 0).GetHashCode() - (self < 0).GetHashCode();
    }

    public static int SignNoZero(this int self)
    {
      return (self < 0) ? -1 : 1;
    }


    public static int AtLeast(this int self, int min)
    {
      return (self < min) ? min : self;
    }

    public static int AtMost(this int self, int max)
    {
      return (self > max) ? max : self;
    }

    public static int AtLeast(this int self, int min, bool warn)
    {
      if (self < min)
      {
        #if DEBUG
        if (warn) Logging.WarnReached();
        #endif
        return min;
      }

      return self;
    }

    public static int AtMost(this int self, int max, bool warn)
    {
      if (max < self)
      {
        #if DEBUG
        if (warn) Logging.WarnReached();
        #endif
        return max;
      }

      return self;
    }


    public static int Clamp(this int self, int min, int max)
    {
      return (self < min) ? min : (self > max) ? max : self;
    }


    public static int ClampIndex(this int idx, int size)
    {
      //#if DEBUG
      //if (Logging.Assert(count >= 0, $"Negative count detected! idx={idx} count={count}"))
      //  return -1;
      //#endif

      if (size == 0 || size <= idx)
        return size - 1;

      return idx;
    }


    public static int RandomIndex(int size)
    {
      // order of operations here is intentional.
      return (int)(size * UnityEngine.Random.value - Floats.EPSILON);
    }


    public static int CalcExtent(int size)
    {
      if (size < 0)
        size *= -1;

      if (size < 4)
        return 1;
      else
        return AtMost(size / 2, MAX_2D_EXTENT, warn: true);
    }

    public static int CalcExtent(uint size)
    {
      if (size < 4u)
        return 1;
      else
        return AtMost((int)(size / 2), MAX_2D_EXTENT, warn: true);
    }


    public static int Compare(int lhs, int rhs)
    {
      return lhs - rhs;
    }

    public static int FlipCompare(int lhs, int rhs)
    {
      return rhs - lhs;
    }


    public static bool HasFlag<TFlag>(this int self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      // equivalent to Bitwise.HasAllBits()
      int value = flag.ToInt32(null);
      return (self & value) == value;
    }


    public static bool HasFlag<TFlag>(this long self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      // equivalent to Bitwise.HasAllBits()
      long value = flag.ToInt64(null);
      return (self & value) == value;
    }



    public static long Mask<TFlag>(this long self, TFlag mask)
      where TFlag : unmanaged, IConvertible
    {
      return self & mask.ToInt64(null);
    }

    public static int Mask<TFlag>(this int self, TFlag mask)
      where TFlag : unmanaged, IConvertible
    {
      return self & mask.ToInt32(null);
    }



    public static void SetFlag<TFlag>(ref this long self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self |= flag.ToInt64(null);
    }

    public static void SetFlag<TFlag>(ref this int self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self |= flag.ToInt32(null);
    }


    public static void SetFlag<TFlag>(ref this long self, TFlag flag, bool set)
      where TFlag : unmanaged, IConvertible
    {
      if (set)
        self |= flag.ToInt64(null);
      else
        self &= ~flag.ToInt64(null);
    }

    public static void SetFlag<TFlag>(ref this int self, TFlag flag, bool set)
      where TFlag : unmanaged, IConvertible
    {
      if (set)
        self |= flag.ToInt32(null);
      else
        self &= ~flag.ToInt32(null);
    }



    public static void RemoveFlag<TFlag>(ref this long self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self &= ~flag.ToInt64(null);
    }

    public static void RemoveFlag<TFlag>(ref this int self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self &= ~flag.ToInt32(null);
    }



    public static void ToggleFlag<TFlag>(ref this long self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self ^= flag.ToInt64(null);
    }

    public static void ToggleFlag<TFlag>(ref this int self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self ^= flag.ToInt32(null);
    }



    public static bool IsEven(this long self)
    {
      return ( (self & long.MinValue) == 0 ) == ( (self & 1) == 0 );
    }

    public static bool IsEven(this ulong self)
    {
      return (self & 1) == 0;
    }


    public static bool IsEven(this int self)
    {
      return ( (self & int.MinValue) == 0 ) == ( (self & 1) == 0 );
    }

    public static bool IsEven(this uint self)
    {
      return (self & 1) == 0;
    }


    public static bool ToBool(this int self)
    {
      return self != 0;
    }
    public static int ToInt(this bool self)
    {
      return self ? 1 : 0;
    }


    public static int Truncate(this ulong self)
    {
      return (0 < self) ? 1 : 0;
    }

    public static int Truncate(this uint self)
    {
      return (0 < self) ? 1 : 0;
    }


    public static int Truncate(this long self)
    {
      return (self == 0) ? 0 : 1;
    }

    public static int Truncate(this int self)
    {
      return (self == 0) ? 0 : 1;
    }



    public static int NOT(this ulong self)
    {
      return (0 < self) ? 0 : 1;
    }

    public static int NOT(this uint self)
    {
      return (0 < self) ? 0 : 1;
    }


    public static int NOT(this long self)
    {
      return (self == 0) ? 1 : 0;
    }

    public static int NOT(this int self)
    {
      return (self == 0) ? 1 : 0;
    }

  }


  public static class Hashing // TODO move new file
  {

    public static int SafeGetHashCode(object obj)
    {
      return obj?.GetHashCode() ?? 0;
    }


    public static uint MixHashes(uint h0, uint h1)
    {
      return (h0 + ((h0 << 5) | (h0 >> 27))) ^ h1;
    }

    public static uint MixHashes(uint h0, uint h1, uint h2)
    {
        h0 = (h0 + ((h0 << 5) | (h0 >> 27))) ^ h1;
      return (h0 + ((h0 << 5) | (h0 >> 27))) ^ h2;
    }

    public static uint MixHashes(uint h0, uint h1, uint h2, uint h3)
    {
        h0 = (h0 + ((h0 << 5) | (h0 >> 27))) ^ h1;
        h0 = (h0 + ((h0 << 5) | (h0 >> 27))) ^ h2;
      return (h0 + ((h0 << 5) | (h0 >> 27))) ^ h3;
    }


    public static int MakeHash(object a, object b)
    {
      return (int)MixHashes((uint)SafeGetHashCode(a),
                            (uint)SafeGetHashCode(b));
    }

    public static int MakeHash(object a, object b, object c)
    {
      return (int)MixHashes((uint)SafeGetHashCode(a),
                            (uint)SafeGetHashCode(b),
                            (uint)SafeGetHashCode(c));
    }

    public static int MakeHash(object a, object b, object c, object d)
    {
      return (int)MixHashes((uint)SafeGetHashCode(a),
                            (uint)SafeGetHashCode(b),
                            (uint)SafeGetHashCode(c),
                            (uint)SafeGetHashCode(d));
    }

  }

}
