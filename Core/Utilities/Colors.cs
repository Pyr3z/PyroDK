/**
@file   PyroDK/Core/Utilities/Colors.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  Color / Color32 predefinitions and utilities.
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{

  public static class Colors
  {
    public static Color32 FromHex(string hex)
    {
      if (hex == null || hex.Length < 2)
      {
        "Hexadecimal color string was empty or too short. Magenta incoming!"
          .LogError();
        return Color.magenta;
      }

      int i = 0;

      if (hex[0] == '#')
        ++i;

      if (hex[i] == '0' && (hex[i + 1] == 'x' || hex[i + 1] == 'X'))
      {
        if (i + 3 < hex.Length)
        {
          i += 2;
        }
        else
        {
          $"Hexadecimal color string \"{hex}\" seems to be too short after removing prefix. Magenta incoming:"
            .LogError();
          return Color.magenta;
        }
      }

      var result = new Color32();

      char hexhi, hexlo;
      byte write = 0x00;

      int j = 0;
      while (j < 4 && i < hex.Length - 1)
      {
        hexhi = hex[i++];
        hexlo = hex[i++];

        if (Bitwise.TryParseHexByte(hexhi, hexlo, ref write))
        {
          result[j++] = write;
        }
        else
        {
          $"Failure to fully parse hexadecimal string \"{hex}\". Expect a weird Color, at the very least."
            .LogWarning();
          break;
        }
      }

      while (j < 4)
        result[j++] = 0xFF;

      return result;
    }

    public static string ToHex(this Color32 c)
    {
      return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a);
    }
    public static string ToHex(this Color32 c, string prev)
    {
      if (!prev.IsEmpty())
      {
        if (prev[0] == '#')
          prev = "#";
        else if (prev.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase))
          prev = "0x";
        else
          prev = "";
      }

      return $"{prev}{string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a)}";
    }

    public static bool TryReparseHex(string hex, ref Color32 result)
    {
      if (hex == null)
        return false;

      hex = hex.Trim(' ');

      if (hex.Length < 2)
        return false;

      int i = 0;

      if (hex[0] == '#')
      {
        i = 1;
      }
      else if ((hex[1] == 'x' || hex[1] == 'X') && hex[0] == '0')
      {
        if (hex.Length < 3)
          return false;
        
        i = 2;
      }

      var   temp = result;
      char  hexhi, hexlo;
      byte  write = 0x00;

      int j = 0;
      while (j < 4 && i < hex.Length - 1)
      {
        hexhi = hex[i++];
        hexlo = hex[i++];

        if (Bitwise.TryParseHexByte(hexhi, hexlo, ref write))
          temp[j++] = write;
        else if (j == 0)
          return false;
        else
          break;
      }

      while (j < 4)
        temp[j++] = 0xFF;

      result = temp;
      return true;
    }


    public static bool IsClear(this Color32 c)
    {
      return c.a == 0x00;
    }

    public static bool IsDefault(this Color32 c)
    {
      return ToInt32(c) == 0;
    }


    public static byte[] ToArray(this Color32 c)
    {
      return new byte[]
      {
        c.r, c.g, c.b, c.a
      };
    }

    public static int ToInt32(this Color32 c)
    {
    //return Bitwise.ComposeInt32(c.r, c.g, c.b, c.a);
      return c.a << 24 | c.b << 16 | c.g << 8 | c.r;
    }



    public static Color32 Alpha(this Color32 c, float a)
    {
      c.a = (byte)(a * 0xFF);
      return c;
    }
    public static Color32 Alpha(this Color32 c, byte a)
    {
      c.a = a;
      return c;
    }

    public static Color32 ToGrayscale(this Color32 c)
    {
      c.r = c.g = c.b = (byte)(((float)c.r + c.g + c.b) / 3f);
      return c;
    }

    public static Color32 LerpToGrayscale(this Color32 c, float t)
    {
      float gray = ((float)c.r + c.g + c.b) / 3f;

      c.r = (byte)(c.r + (gray - c.r) * t);
      c.g = (byte)(c.g + (gray - c.g) * t);
      c.b = (byte)(c.b + (gray - c.b) * t);

      return c;
    }

    public static Color32 SmoothToGrayscale(this Color32 c, float t)
    {
      return LerpToGrayscale(c, Floats.SmoothStepParameter(t));
    }

    public static Color32 Inverted(this Color32 c)
    {
      float r = 1f - ((float)c.r / 0xFF);
      float g = 1f - ((float)c.g / 0xFF);
      float b = 1f - ((float)c.b / 0xFF);

      return new Color32( (byte)(r * 0xFF),
                          (byte)(g * 0xFF),
                          (byte)(b * 0xFF),
                                 c.a );
    }

    public const byte HEX_BUMP = 0x28;
    public const byte HEX_CEIL = 0xFF - HEX_BUMP;

    public static Color32 AlphaBump(this Color32 c)
    {
      if (c.a >= HEX_CEIL)
        c.a = 0xFF;
      else
        c.a += HEX_BUMP;
      return c;
    }
    
    public static Color32 AlphaBump(this Color32 c, int i)
    {
      int alpha = c.a + HEX_BUMP * i;

      if (alpha >= 0xFF)
        c.a = 0xFF;
      else
        c.a = (byte)alpha;
      return c;
    }

    public static Color32 AlphaWash(this Color32 c)
    {
      if (c.a < HEX_BUMP)
        c.a = 0x00;
      else
        c.a -= HEX_BUMP;
      return c;
    }

    public static Color32 AlphaWash(this Color32 c, int i)
    {
      int alpha = c.a - HEX_BUMP * i;

      if (alpha <= 0x00)
        c.a = 0x00;
      else
        c.a = (byte)alpha;
      return c;
    }


    public static Color32 AlphaInvert(this Color32 c)
    {
      c.a = (byte)(0xFF - c.a);
      return c;
    }


    public struct Comparer :
      IComparer<Color32>,         IComparer<Color>,
      IEqualityComparer<Color32>, IEqualityComparer<Color>
    {
      public int Compare(Color32 a, Color32 b)
      {
        return GetHashCode(a).CompareTo(GetHashCode(b));
      }

      public int Compare(Color a, Color b)
      {
        return Compare((Color32)a, (Color32)b);
      }

      public bool Equals(Color32 a, Color32 b)
      {
        return ToInt32(a) == ToInt32(b);
      }

      public bool Equals(Color a, Color b)
      {
        return a == b;
      }

      public int GetHashCode(Color32 c)
      {
        return ToInt32(c);
      }

      public int GetHashCode(Color c)
      {
        return c.GetHashCode();
      }
    }

    public static readonly Comparer Comparator = new Comparer();


    public static Color32 Black = FromHex("#000000FF");
    public static Color32 White = FromHex("#FFFFFFFF");
    public static Color32 Gray  = FromHex("#888888FF");

    public static Color32 Red   = FromHex("#FF0000FF");
    public static Color32 Green = FromHex("#00FF00FF");
    public static Color32 Blue  = FromHex("#0000FFFF");
    public static Color32 Clear = FromHex("#FFFFFF00");

    public static Color32 Yellow = FromHex("#FFFF00FF");


    public static class Debug
    {

      [SerializeStatic]
      public static Color32 Attention             = FromHex("#CA2622FF");
      [SerializeStatic]
      public static Color32 Important             = FromHex("#834283FF");
      [SerializeStatic]
      public static Color32 Pending               = FromHex("#FF763899");
      [SerializeStatic]
      public static Color32 Boring                = FromHex("#5C5C5CFF");

      [SerializeStatic]
      public static Color32 Log                   = FromHex("#9A6C78FF");
      [SerializeStatic]
      public static Color32 Success               = FromHex("#54AA54");
      [SerializeStatic]
      public static Color32 Warning               = FromHex("#FFB300FF");
      [SerializeStatic]
      public static Color32 Error                 = FromHex("#910329FF");

    }


    public static class GUI
    {

      [SerializeStatic]
      public static Color32 Text                  = FromHex("#D3D8D8FF");
      [SerializeStatic]
      public static Color32 TextBright            = FromHex("#FFF9F9FF");
      [SerializeStatic]
      public static Color32 TextDim               = FromHex("#AA9C9CFF");

      [SerializeStatic]
      public static Color32 Comment               = FromHex("#57A64AFF");

      [SerializeStatic]
      public static Color32 TypeByRef             = FromHex("#4EC9B1FF");
      [SerializeStatic]
      public static Color32 TypeByVal             = FromHex("#86C691FF");

      [SerializeStatic]
      public static Color32 String                = FromHex("#FF834EFF");
      [SerializeStatic]
      public static Color32 StringGood            = FromHex("#89E245FF");
      
      [SerializeStatic]
      public static Color32 Keyword               = FromHex("#569AD1FF");

      [SerializeStatic]
      public static Color32 Value                 = FromHex("#B5CEA8FF");
      [SerializeStatic]
      public static Color32 ValueInfo             = FromHex("#2C8FABFF");

      [SerializeStatic]
      public static Color32 GizmoBounds           = FromHex("#F8B300FF");
      [SerializeStatic]
      public static Color32 GizmoCollider         = FromHex("#009D9DFF");
      [SerializeStatic]
      public static Color32 GizmoTrigger          = FromHex("#00CE00FF");

      [SerializeStatic]
      public static Color32 FieldBG               = FromHex("#2C2A2A77");
      [SerializeStatic]
      public static Color32 FieldOutline          = FromHex("#1D242488");
      
      [SerializeStatic]
      public static Color32 SectionBG             = FromHex("#121010FF"); // ~ 383636, 3B4545

      [SerializeStatic]
      public static Color32 HashMapBG             = FromHex("#0B1111FF");
      [SerializeStatic]
      public static Color32 HashMapOutline        = FromHex("#3A4545FF");

    }

  }


  // TODO move to separate file
  public static class Textures
  {

    public static readonly Texture2D Clear = new Texture2D(1, 1, TextureFormat.Alpha8, mipChain: false)
    {
      name       = "PyroDK.Textures.Clear",
      filterMode = FilterMode.Point,
      wrapMode   = TextureWrapMode.Clamp,
      wrapModeU  = TextureWrapMode.Clamp,
      wrapModeV  = TextureWrapMode.Clamp,
      wrapModeW  = TextureWrapMode.Clamp
    };

    public static readonly Texture2D FieldBG = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false)
    {
      name       = "PyroDK.Textures.FieldBG",
      filterMode = FilterMode.Point,
      wrapMode   = TextureWrapMode.Clamp,
      wrapModeU  = TextureWrapMode.Clamp,
      wrapModeV  = TextureWrapMode.Clamp,
      wrapModeW  = TextureWrapMode.Clamp
    };

    public static readonly Texture2D SectionBG = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false)
    {
      name       = "PyroDK.Textures.SectionBG",
      filterMode = FilterMode.Point,
      wrapMode   = TextureWrapMode.Clamp,
      wrapModeU  = TextureWrapMode.Clamp,
      wrapModeV  = TextureWrapMode.Clamp,
      wrapModeW  = TextureWrapMode.Clamp
    };


    static Textures()
    {
      Clear.SetPixels32(new Color32[] { Colors.Clear });
      Clear.Apply(updateMipmaps: true, makeNoLongerReadable: true);

      FieldBG.SetPixels32(new Color32[] { Colors.GUI.FieldBG });
      FieldBG.Apply(updateMipmaps: true, makeNoLongerReadable: true);

      SectionBG.SetPixels32(new Color32[] { Colors.GUI.SectionBG });
      SectionBG.Apply(updateMipmaps: true, makeNoLongerReadable: true);
    }


    public static Texture2D FindForEditor(string name)
    {
      #if UNITY_EDITOR
      return UnityEditor.EditorGUIUtility.FindTexture(name);
      #else
      return null;
      #endif
    }

  }

}