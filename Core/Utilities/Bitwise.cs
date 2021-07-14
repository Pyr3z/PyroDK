/**
@file   PyroDK/Core/Utilities/Bitwise.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-06

@brief
Bitwise utilities and fast algorithms.
**/




namespace PyroDK
{

  using IConvertible  = System.IConvertible;
  using Enum          = System.Enum;


  public static class Bitwise
  {

    #if UNITY_EDITOR
    #pragma warning disable IDE0051 // this private static function is called via reflection.
    
    //[SanityTest]
    private static void TestCTZ()
    {
      const bool ctz_test_zeros = true;
      const int ctz_test_count = 64;

      var report = new System.Text.StringBuilder("PyroDK.Core.Bitwise.TestCTZ();\n");

      if (ctz_test_count > 0)
      {
        report.AppendLine("BEGIN: Valid input for function \"PyroDK.Core.Bitwise.CTZ\" (8 overloads):");

        int i = 0;
        while (i < 4 && i < ctz_test_count)
        {
          report.AppendLine($"CTZ((sbyte)(1 << {i}))    returned {CTZ((sbyte)(1 << i))}");
          ++i;
        }

        while (i < 8 && i < ctz_test_count)
        {
          report.AppendLine($"CTZ((byte)(1 << {i}))     returned {CTZ((byte)(1 << i))}");
          ++i;
        }

        while (i < 12 && i < ctz_test_count)
        {
          report.AppendLine($"CTZ((short)(1 << {i}))    returned {CTZ((short)(1 << i))})");
          ++i;
        }

        while (i < 16 && i < ctz_test_count)
        {
          report.AppendLine($"CTZ((ushort)(1 << {i}))   returned {CTZ((ushort)(1 << i))}");
          ++i;
        }

        while (i < 24 && i < ctz_test_count)
        {
          report.AppendLine($"CTZ((int)(1 << {i}))      returned {CTZ((int)(1 << i))}");
          ++i;
        }

        while (i < 32 && i < ctz_test_count)
        {
          report.AppendLine($"CTZ((uint)(1 << {i}))     returned {CTZ((uint)(1 << i))}");
          ++i;
        }

        while (i < 48 && i < ctz_test_count)
        {
          report.AppendLine($"CTZ((long)(1UL << {i}))   returned {CTZ((long)(1UL << i))}");
          ++i;
        }

        while (i < 64 && i < ctz_test_count)
        {
          report.AppendLine($"CTZ((ulong)(1UL << {i}))  returned {Bitwise.CTZ((ulong)(1UL << i))}");
          ++i;
        }

        report.AppendLine("\nEND: Valid input for function \"PyroDK.Core.Bitwise.CTZ\".");
        report.AppendLine("------------------------------------------------------------------------");
      }

      if (ctz_test_zeros)
      {
        report.AppendLine("BEGIN: Invalid input for function \"PyroDK.Core.Bitwise.CTZ\" (8 overloads):\n"
                + "------------------------------------------------------------------------\n"
                + $"CTZ((sbyte)0)   returned {CTZ((sbyte)0)}\n"
                + $"CTZ((byte)0)    returned {CTZ((byte)0)}\n"
                + $"CTZ((short)0)   returned {CTZ((short)0)}\n"
                + $"CTZ((ushort)0)  returned {CTZ((ushort)0)}\n"
                + $"CTZ((int)0)     returned {CTZ((int)0)}\n"
                + $"CTZ((uint)0)    returned {CTZ((uint)0)}\n"
                + $"CTZ((long)0)    returned {CTZ((long)0)}\n"
                + $"CTZ((ulong)0)   returned {CTZ((ulong)0)}\n");
      }

      report.AppendLine("-- REPORT END --");

      report.ToString().Log();
    }
    
    #pragma warning restore IDE0051
    #endif // UNITY_EDITOR


    public const int  SIGN_BIT_32 = int.MinValue;
    public const long SIGN_BIT_64 = long.MinValue;


    public static byte GetByte(long val, int i) // `i` is WRAPPED
    {
      return System.BitConverter.GetBytes(val)[i % sizeof(long)];
    }
    public static byte GetByte(ulong val, int i)
    {
      return System.BitConverter.GetBytes(val)[i % sizeof(ulong)];
    }
    public static byte GetByte(double val, int i)
    {
      return System.BitConverter.GetBytes(val)[i % sizeof(double)];
    }

    public static byte GetByte(int val, int i)
    {
      return System.BitConverter.GetBytes(val)[i % sizeof(int)];
    }
    public static byte GetByte(uint val, int i)
    {
      return System.BitConverter.GetBytes(val)[i % sizeof(uint)];
    }
    public static byte GetByte(float val, int i)
    {
      return System.BitConverter.GetBytes(val)[i % sizeof(float)];
    }


    private static bool TryParseHexNybble(char ascii, ref byte write)
    {
      const byte CLEAR_LO = 0xF0;

      if (ascii < '0')
        return false;

      if (ascii <= '9')
      {
        write = (byte)((write & CLEAR_LO) | (ascii - '0'));
        return true;
      }

      if (ascii < 'A')
        return false;

      if (ascii <= 'F')
      {
        write = (byte)((write & CLEAR_LO) | (ascii - 'A' + 10));
        return true;
      }

      if (ascii < 'a')
        return false;

      if (ascii <= 'f')
      {
        write = (byte)((write & CLEAR_LO) | (ascii - 'a' + 10));
        return true;
      }

      return false;
    }

    public static bool TryParseHexByte(char ascii_hi, char ascii_lo, ref byte write)
    {
      if (!TryParseHexNybble(ascii_hi, ref write))
        return false;

      write <<= 4;
      return TryParseHexNybble(ascii_lo, ref write);
    }


    public static char GetHexLoNybble(byte b)
    {
      b %= 16;

      if (b < 10)
        return (char)(b + '0');

      return (char)(b - 10 + 'A');
    }

    public static char GetHexHiNybble(byte b)
    {
      b /= 16;

      if (b < 10)
        return (char)(b + '0');

      return (char)(b - 10 + 'A');
    }



    public static bool IsOneBit(ulong value)
    {
      return value != 0UL && ShaveLSB(value) == 0;
    }

    public static bool IsOneBit(uint value)
    {
      return value != 0U && ShaveLSB(value) == 0;
    }


    public static bool IsOneBit(long value)
    {
      return value != 0L && ShaveLSB(value) == 0;
    }

    public static bool IsOneBit(int value)
    {
      return value != 0 && ShaveLSB(value) == 0;
    }

    
    public static bool IsOneBit(IConvertible value)
    {
      return IsOneBit(value.ToInt64(null));
    }


    
    public static int CountBitsSet(ulong value)
    {
      int count;
      for (count = 0; value != 0; ++count)
        value &= value - 1;
      return count;
    }

    public static int CountBitsSet(uint value)
    {
      int count;
      for (count = 0; value != 0; ++count)
        value &= value - 1;
      return count;
    }


    public static int CountBitsSet(long value)
    {
      int count;
      for (count = 0; value != 0; ++count)
        value &= value - 1;
      return count;
    }

    public static int CountBitsSet(int value)
    {
      int count;
      for (count = 0; value != 0; ++count)
        value &= value - 1;
      return count;
    }


    public static int CountBitsSet(IConvertible value)
    {
      return CountBitsSet(value.ToInt64(null));
    }



    public static int CountDifferentBits(long a, long b)
    {
      return CountBitsSet(a ^ b);
    }

    public static int CountDifferentBits(int a, int b)
    {
      return CountBitsSet(a ^ b);
    }


    public static int CountDifferentBits(IConvertible a, IConvertible b)
    {
      return CountBitsSet(a.ToInt64(null) ^ b.ToInt64(null));
    }



    public static ulong ClearBits(this ulong self, ulong bits)
    {
      return self & ~bits;
    }

    public static uint ClearBits(this uint self, uint bits)
    {
      return self & ~bits;
    }

    public static long ClearBits(this long self, long bits)
    {
      return self & ~bits;
    }

    public static int ClearBits(this int self, int bits)
    {
      return self & ~bits;
    }

    public static TEnum ClearBits<TEnum>(this TEnum self, TEnum bits)
      where TEnum : unmanaged, Enum, IConvertible
    {
      return EnumSpy<TEnum>.ConvertFrom(self.ToInt64(null) & ~bits.ToInt64(null));
    }



    public static ulong SetBits(this ulong self, ulong bits)
    {
      return self | bits;
    }

    public static uint SetBits(this uint self, uint bits)
    {
      return self | bits;
    }

    public static long SetBits(this long self, long bits)
    {
      return self | bits;
    }

    public static int SetBits(this int self, int bits)
    {
      return self | bits;
    }


    public static TEnum SetBits<TEnum>(this TEnum self, TEnum bits)
      where TEnum : unmanaged, Enum, IConvertible
    {
      return EnumSpy<TEnum>.ConvertFrom(self.ToInt64(null) | bits.ToInt64(null));
    }



    public static ulong SetBits(this ulong self, ulong bits, bool set)
    {
      return (set) ? self | bits : self & ~bits;
    }

    public static uint SetBits(this uint self, uint bits, bool set)
    {
      return (set) ? self | bits : self & ~bits;
    }

    public static long SetBits(this long self, long bits, bool set)
    {
      return (set) ? self | bits : self & ~bits;
    }

    public static int SetBits(this int self, int bits, bool set)
    {
      return (set) ? self | bits : self & ~bits;
    }


    public static TEnum SetBits<TEnum>(this TEnum self, TEnum bits, bool set)
      where TEnum : unmanaged, Enum, IConvertible
    {
      if (set)
        return EnumSpy<TEnum>.ConvertFrom(self.ToInt64(null) | bits.ToInt64(null));
      else
        return EnumSpy<TEnum>.ConvertFrom(self.ToInt64(null) & ~bits.ToInt64(null));
    }

    public static TEnum SetBits<TEnum>(this TEnum self, TEnum bits, bool set, out int delta)
      where TEnum : unmanaged, Enum, IConvertible
    {
      long s = self.ToInt64(null);
      long b = bits.ToInt64(null);

      if (b == 0)
      {
        delta = 0;
        return self;
      }
      else if (set)
      {
        delta = CountBitsSet((s ^ b) & b);
        return EnumSpy<TEnum>.ConvertFrom(s | b);
      }
      else
      {
        delta = -1 * CountBitsSet(s & (s ^ ~b));
        return EnumSpy<TEnum>.ConvertFrom(s & ~b);
      }
    }



    public static ulong ToggleBits(this ulong self, ulong bits)
    {
      return self ^ bits;
    }

    public static uint ToggleBits(this uint self, uint bits)
    {
      return self ^ bits;
    }

    public static long ToggleBits(this long self, long bits)
    {
      return self ^ bits;
    }

    public static int ToggleBits(this int self, int bits)
    {
      return self ^ bits;
    }


    public static TEnum ToggleBits<TEnum>(this TEnum self, TEnum bits)
      where TEnum : unmanaged, Enum, IConvertible
    {
      return EnumSpy<TEnum>.ConvertFrom(self.ToInt64(null) ^ bits.ToInt64(null));
    }



    public static bool HasSignBit(this long self)
    {
      return (self & SIGN_BIT_64) != 0;
    }

    public static bool HasSignBit(this ulong self)
    {
      return ((long)self & SIGN_BIT_64) != 0;
    }


    public static bool HasSignBit(this int self)
    {
      return (self & SIGN_BIT_32) != 0;
    }
    public static bool HasSignBit(this uint self)
    {
      return ((int)self & SIGN_BIT_32) != 0;
    }



    public static long SetSignBit(long value)
    {
      return value | SIGN_BIT_64;
    }

    public static int SetSignBit(int value)
    {
      return value | SIGN_BIT_32;
    }


    public static long SetSignBit(this long self, bool set)
    {
      if (set)
        return self | SIGN_BIT_64;
      else
        return self & ~SIGN_BIT_64;
    }

    public static int SetSignBit(this int self, bool set)
    {
      if (set)
        return self | SIGN_BIT_32;
      else
        return self & ~SIGN_BIT_32;
    }

    

    public static long ToggleSignBit(long value)
    {
      return value ^ SIGN_BIT_64;
    }

    public static int ToggleSignBit(int value)
    {
      return value ^ SIGN_BIT_32;
    }



    public static ulong LSB(ulong bits)
    {
      return bits & (~bits + 1);
    }

    public static uint LSB(uint bits)
    {
      return bits & (~bits + 1);
    }

    public static ushort LSB(ushort bits)
    {
      return (ushort)(bits & (~bits + 1));
    }

    public static byte LSB(byte bits)
    {
      return (byte)(bits & (~bits + 1));
    }


    public static long LSB(long bits)
    {
      return bits & -bits;
    }

    public static int LSB(int bits)
    {
      return bits & -bits;
    }

    public static short LSB(short bits)
    {
      return (short)(bits & -bits);
    }

    public static sbyte LSB(sbyte bits)
    {
      return (sbyte)(bits & -bits);
    }




    public static ulong ShaveLSB(ulong bits)
    {
      return bits & ~(bits & (~bits + 1));
    }

    public static uint ShaveLSB(uint bits)
    {
      return bits & ~(bits & (~bits + 1));
    }

    public static ushort ShaveLSB(ushort bits)
    {
      return (ushort)(bits & ~(bits & -bits));
    }

    public static byte ShaveLSB(byte bits)
    {
      return (byte)(bits & ~(bits & -bits));
    }


    public static long ShaveLSB(long bits)
    {
      return bits & ~(bits & -bits);
    }

    public static int ShaveLSB(int bits)
    {
      return bits & ~(bits & -bits);
    }

    public static short ShaveLSB(short bits)
    {
      return (short)(bits & ~(bits & -bits));
    }

    public static sbyte ShaveLSB(sbyte bits)
    {
      return (sbyte)(bits & ~(bits & -bits));
    }



    public static bool HasAnyBits(this ulong self, ulong bits)
    {
      return (self & bits) != 0;
    }

    public static bool HasAnyBits(this uint self, uint bits)
    {
      return (self & bits) != 0;
    }


    public static bool HasAnyBits(this long self, long bits)
    {
      return (self & bits) != 0;
    }

    public static bool HasAnyBits(this int self, int bits)
    {
      return (self & bits) != 0;
    }


#if false // old, superfluous generic versions for HasAnyBits()

    public static bool HasAnyBits<T>(this ulong self, T bits) where T : System.IConvertible
    {
      return (self & bits.ToUInt64(null)) != 0;
    }

    public static bool HasAnyBits<T>(this uint self, T bits) where T : System.IConvertible
    {
      return (self & bits.ToUInt32(null)) != 0;
    }

    public static bool HasAnyBits<T>(this ushort self, T bits) where T : System.IConvertible
    {
      return (self & bits.ToUInt16(null)) != 0;
    }

    public static bool HasAnyBits<T>(this byte self, T bits) where T : System.IConvertible
    {
      return (self & bits.ToByte(null)) != 0;
    }

    public static bool HasAnyBits<T>(this long self, T bits) where T : System.IConvertible
    {
      return (self & bits.ToInt64(null)) != 0;
    }

    public static bool HasAnyBits<T>(this int self, T bits) where T : System.IConvertible
    {
      return (self & bits.ToInt32(null)) != 0;
    }

    public static bool HasAnyBits<T>(this short self, T bits) where T : System.IConvertible
    {
      return (self & bits.ToInt16(null)) != 0;
    }

    public static bool HasAnyBits<T>(this sbyte self, T bits) where T : System.IConvertible
    {
      return (self & bits.ToSByte(null)) != 0;
    }


    public static bool HasAnyBits<T0, T1>(this T0 self, T1 bits)
      where T0 : System.IConvertible
      where T1 : System.IConvertible
    {
      return (self.ToUInt64(null) & bits.ToUInt64(null)) != 0;
    }

#endif



    public static bool HasAllBits(this ulong self, ulong bits)
    {
      return (self & bits) == bits;
    }

    public static bool HasAllBits(this uint self, uint bits)
    {
      return (self & bits) == bits;
    }


    public static bool HasAllBits(this long self, long bits)
    {
      return (self & bits) == bits;
    }

    public static bool HasAllBits(this int self, int bits)
    {
      return (self & bits) == bits;
    }



#if false // generic versions for HasAllBits()

    public static bool HasAllBits<T>(this ulong self, T bits) where T : System.IConvertible
    {
      var bits_cvtd = bits.ToUInt64(null);
      return (self & bits_cvtd) == bits_cvtd;
    }

    public static bool HasAllBits<T>(this uint self, T bits) where T : System.IConvertible
    {
      var bits_cvtd = bits.ToUInt32(null);
      return (self & bits_cvtd) == bits_cvtd;
    }

    public static bool HasAllBits<T>(this ushort self, T bits) where T : System.IConvertible
    {
      var bits_cvtd = bits.ToUInt16(null);
      return (self & bits_cvtd) == bits_cvtd;
    }

    public static bool HasAllBits<T>(this byte self, T bits) where T : System.IConvertible
    {
      var bits_cvtd = bits.ToByte(null);
      return (self & bits_cvtd) == bits_cvtd;
    }


    public static bool HasAllBits<T>(this long self, T bits) where T : System.IConvertible
    {
      var bits_cvtd = bits.ToInt64(null);
      return (self & bits_cvtd) == bits_cvtd;
    }

    public static bool HasAllBits<T>(this int self, T bits) where T : System.IConvertible
    {
      var bits_cvtd = bits.ToInt32(null);
      return (self & bits_cvtd) == bits_cvtd;
    }

    public static bool HasAllBits<T>(this short self, T bits) where T : System.IConvertible
    {
      var bits_cvtd = bits.ToInt16(null);
      return (self & bits_cvtd) == bits_cvtd;
    }

    public static bool HasAllBits<T>(this sbyte self, T bits) where T : System.IConvertible
    {
      var bits_cvtd = bits.ToSByte(null);
      return (self & bits_cvtd) == bits_cvtd;
    }


    public static bool HasAllBits<T0, T1>(this T0 self, T1 bits)
      where T0 : System.IConvertible
      where T1 : System.IConvertible
    {
      var bits_cvtd = bits.ToUInt64(null);
      return (self.ToUInt64(null) & bits_cvtd) == bits_cvtd;
    }

#endif



    public static int ComposeInt32(byte b0, byte b1, byte b2, byte b3)
    {
      return (b3 << 24) | (b2 << 16) | (b1 << 8) | b0;
    }



    public static int CTZ(ulong bits)
    {
      /* Valid return range: [0,63] */
      /* Returns bits.GetBitWidth() (64 in this case) when passed 0. */
#if PDK_CTZ_NOJUMP

      int ctz  = 0;
      int last = 0;

      ctz += ((bits & 0xFFFFFFFF).Not() << 5); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x0000FFFF).Not() << 4); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x000000FF).Not() << 3); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x0000000F).Not() << 2); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x00000003).Not() << 1); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x00000001).Not()     );

      return ctz + ((bits >> (ctz - last)) & 1).Not();
#else // A lookup with a pre-computed DeBruijn sequence is fastest:
      if (bits == 0)
        return 64;
      
      return s_CTZLookup64[ ( LSB(bits) * c_DeBruijnKey64 ) >> 58 ];
#endif
    }

    public static int CTZ(uint bits)
    {
      /* Valid return range: [0,31] */
      /* Returns bits.GetBitWidth() (32 in this case) when passed 0. */
#if PDK_CTZ_NOJUMP

      int ctz = 0;
      int last = 0;

      ctz += ((bits & 0xFFFF).Not() << 4); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x00FF).Not() << 3); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x000F).Not() << 2); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x0003).Not() << 1); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x0001).Not()     );

      return ctz + ((bits >> (ctz - last)) & 1).Not();
#else // A lookup with a pre-computed DeBruijn sequence is fastest:
      if (bits == 0)
        return 32;

      return s_CTZLookup32[ ( LSB(bits) * c_DeBruijnKey32 ) >> 27 ];
#endif
    }

    public static int CTZ(ushort bits)
    {
      /* Valid return range: [0,15] */
      /* Returns bits.GetBitWidth() (16 in this case) when passed 0. */
#if PDK_CTZ_NOJUMP

      int ctz = 0;
      int last = 0;

      ctz += ((bits & 0xFF).Not() << 3); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x0F).Not() << 2); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x03).Not() << 1); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x01).Not()     );

      return ctz + ((bits >> (ctz - last)) & 1).Not();
#else // A lookup with a pre-computed DeBruijn sequence is fastest:
      if (bits == 0)
        return 16;

      return s_CTZLookup16[ (ushort)( LSB(bits) * c_DeBruijnKey16 ) >> 12 ];
#endif
    }

    public static int CTZ(byte bits)
    {
      /* Valid return range: [0,7] */
      /* Returns bits.GetBitWidth() (8 in this case) when passed 0. */
#if PDK_CTZ_NOJUMP

      int ctz = 0;
      int last = 0;

      ctz += ((bits & 0xF).Not() << 2); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x3).Not() << 1); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x1).Not()     );

      return ctz + ((bits >> (ctz - last)) & 1).Not();
#else // A lookup with a pre-computed DeBruijn sequence is fastest:
      if (bits == 0)
        return 8;

      return s_CTZLookup8[ (byte)( LSB(bits) * c_DeBruijnKey8 ) >> 5 ];
#endif
    }

    //
    // The signed variants simply cast and call the unsigned versions:
    //

    public static int CTZ(long bits)
    {
      return CTZ((ulong)bits);
    }

    public static int CTZ(int bits)
    {
      return CTZ((uint)bits);
    }

    public static int CTZ(short bits)
    {
      return CTZ((ushort)bits);
    }

    public static int CTZ(sbyte bits)
    {
      return CTZ((byte)bits);
    }


    //
    // Enum variant converts to ulong:
    //

    public static T LSB<T>(T bits)
      where T : unmanaged, Enum, IConvertible
    {
      return EnumSpy<T>.ConvertFrom(LSB(bits.ToUInt64(null)));
    }

    public static bool HasAny<TEnum>(this TEnum self, TEnum flags)
      where TEnum : unmanaged, Enum, IConvertible
    {
      return HasAnyBits(self.ToInt64(null), flags.ToInt64(null));
    }

    public static int CTZ<T>(T bits, int fallback = -1)
      where T : unmanaged, Enum, IConvertible
    {
      ulong value = bits.ToUInt64(null);

      if (value == 0UL)
        return fallback;

      return CTZ(value);
    }


    //
    // Private data section: Lookups
    //

    // The following lookups were custom generated with a DeBruijn sequence tool
    // written in C: (yes, custom-coded by yours truly, L. Perez)

    private static readonly int[] s_CTZLookup8 = { 0, 1, 2, 4, 7, 3, 6, 5 };

    private static readonly int[] s_CTZLookup16 = { 0,  1, 2, 5,  3,  9, 6,  11,
                                                    15, 4, 8, 10, 14, 7, 13, 12 };

    private static readonly int[] s_CTZLookup32 = { 0,  1,  2,  6,  3,  11, 7,  16, 4,  14, 12,
                                                    21, 8,  23, 17, 26, 31, 5,  10, 15, 13, 20,
                                                    22, 25, 30, 9,  19, 24, 29, 18, 28, 27 };

    private static readonly int[] s_CTZLookup64 = {
      0,  1,  2,  7,  3,  13, 8,  19, 4,  25, 14, 28, 9,  34, 20, 40, 5,  17, 26, 38, 15, 46,
      29, 48, 10, 31, 35, 54, 21, 50, 41, 57, 63, 6,  12, 18, 24, 27, 33, 39, 16, 37, 45, 47,
      30, 53, 49, 56, 62, 11, 23, 32, 36, 44, 52, 55, 61, 22, 43, 51, 60, 42, 59, 58
    };

    // and the corresponding DeBruijn keys:
    private const byte c_DeBruijnKey8 = 0x17;                /* B(2,3) */
    private const ushort c_DeBruijnKey16 = 0x9AF;            /* B(2,4) */
    private const uint c_DeBruijnKey32 = 0x4653ADF;          /* B(2,5) */
    private const ulong c_DeBruijnKey64 = 0x218A392CD3D5DBF; /* B(2,6) */
  }

}