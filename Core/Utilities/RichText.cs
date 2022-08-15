/**
@file   PyroDK/Core/Utilities/RichText.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  Rich text string utilities.
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  using StringBuilder = System.Text.StringBuilder;
  using Type          = System.Type;
  using MethodInfo    = System.Reflection.MethodBase;
  using ParameterInfo = System.Reflection.ParameterInfo;


  public static class RichText
  {
    [System.Flags]
    public enum Style
    {
      Normal      = (0 << 0),

      Bold        = (1 << 0),
      Italic      = (1 << 1),

      BraceAngle  = (1 << 2),
      BraceSoft   = (1 << 3),
      BraceHard   = (1 << 4),

      Quotes      = (1 << 5),

      Large       = (1 << 6),
      Small       = (1 << 7),

      Emphasis    = Bold | Italic,
      Aside       = Italic | Small,
    }


    [SerializeStatic]
    public static int FontSizeLarge = 14;
    [SerializeStatic]
    public static int FontSizeSmall = 10;


    public static readonly string True  = Color("true",  Colors.GUI.Keyword);
    public static readonly string False = Color("false", Colors.GUI.Keyword);

    public static readonly string Null  = Color("null",  Colors.GUI.Keyword);

    public static readonly string EmptyStringLiteral = Color("\"\"", Colors.GUI.String);


    public static string Make(string text, Style style, Color32 color)
    {
      if (text.IsEmpty())
        return string.Empty;

      if (style == Style.Normal && color.IsDefault())
        return text;

      return new StringBuilder(text.Length * 2).OpenRichText(style, color)
                                               .Append(text)
                                               .CloseRichText(style, color)
                                               .ToString();
    }

    public static string Make(string text, Style style)
    {
      if (text.IsEmpty() || style == Style.Normal)
        return text;

      return new StringBuilder(text.Length * 2).OpenRichText(style)
                                               .Append(text)
                                               .CloseRichText(style)
                                               .ToString();
    }


    public static StringBuilder OpenRichText(this StringBuilder strb, Style style)
    {
      if (style.HasFlag(Style.Large))
        strb.Append("<size=").Append(FontSizeLarge).Append('>');
      else if (style.HasFlag(Style.Small))
        strb.Append("<size=").Append(FontSizeSmall).Append('>');

      if (style.HasFlag(Style.BraceHard))
        strb.Append('[');

      if (style.HasFlag(Style.BraceSoft))
        strb.Append('(');

      if (style.HasFlag(Style.BraceAngle))
        strb.Append('<');

      if (style.HasFlag(Style.Italic))
        strb.Append("<i>");

      if (style.HasFlag(Style.Bold))
        strb.Append("<b>");

      return strb;
    }

    public static StringBuilder OpenRichText(this StringBuilder strb, Color32 color)
    {
      return strb.Append("<color=#").Append(color.ToHex()).Append('>');
    }

    public static StringBuilder OpenRichText(this StringBuilder strb, Style style, Color32 color)
    {
      return strb.OpenRichText(style)
                 .OpenRichText(color);
    }


    public static StringBuilder CloseRichText(this StringBuilder strb, Style style)
    {
      if (style.HasFlag(Style.Bold))
        strb.Append("</b>");

      if (style.HasFlag(Style.Italic))
        strb.Append("</i>");

      if (style.HasFlag(Style.BraceAngle))
        strb.Append('>');

      if (style.HasFlag(Style.BraceSoft))
        strb.Append(')');

      if (style.HasFlag(Style.BraceHard))
        strb.Append(']');

      if (style.HasAny(Style.Large | Style.Small))
        strb.Append("</size>");

      return strb;
    }

    public static StringBuilder CloseRichText(this StringBuilder strb, Color32 color)
    {
      return strb.Append("</color>");
    }

    public static StringBuilder CloseRichText(this StringBuilder strb, Style style, Color32 color)
    {
      return strb.CloseRichText(color)
                 .CloseRichText(style);
    }


    public static string ColorStringLiterals(string text)
    {
      if (text.IsEmpty())
        return string.Empty;

      return AppendColoredStringLiterals(new StringBuilder(capacity: text.Length * 2 + 21), text).ToString();
    }

    public static string ColorTypeNames(string text)
    {
      if (text.IsEmpty())
        return string.Empty;

      return AppendColoredTypeNames(new StringBuilder(capacity: text.Length * 2 + 21), text).ToString();
    }

    public static StringBuilder AppendColoredStringLiterals(this StringBuilder strb, string text)
    {
      return strb.AppendColoredRegions(text:        text,
                                       color:       Colors.GUI.String,
                                       brack0:      '\"',
                                       brack1:      '\"',
                                       skip_angles: true);
    }

    public static StringBuilder AppendColoredTypeNames(this StringBuilder strb, string text)
    {
      return strb.AppendColoredRegions(text:        text,
                                       color:       Colors.GUI.TypeByRef,
                                       brack0:      '<',
                                       brack1:      '>',
                                       skip_angles: false);
    }

    public static StringBuilder AppendColoredRegions(this StringBuilder strb,
                                                          string        text,
                                                          Color32       color,
                                                          char          brack0,
                                                          char          brack1,
                                                          bool          skip_angles)
    {
      var promise = Strings.SplitBracketRegions(text, brack0, brack1, skip_angles,
                                                out List<string> split_buffer);
      if (promise.IsVoid())
      {
        return strb.Append(text);
      }

      foreach (string section in split_buffer)
      {
        int last = section.Length - 1;

        if ((last > 0)              &&
            (section[0] == brack0)  &&
            !Logging.Assert(section[last] == brack1))
        {
          _ = strb.OpenRichText(color)
                  .Append(section)
                  .CloseRichText(color);
        }
        else
        {
          _ = strb.Append(section);
        }
      }

      promise.Dispose();
      return strb;
    }


    public static string Attribute(Type attr_type)
    {
      return Attribute(attr_type, Colors.GUI.TypeByRef);
    }

    public static string Attribute(Type attr_type, Color32 color)
    {
      string name = attr_type.Name;

      if (name.EndsWith("Attribute"))
        name = name.Remove(name.Length - 9);

      return $"<b>[{Color(name, color)}]</b>";
    }

    public static string Attribute(string name)
    {
      return $"<b>[{Color(name, Colors.GUI.TypeByRef)}]</b>";
    }

    public static string Attribute(string name, object param0)
    {
      return $"<b>[{Color(name, Colors.GUI.TypeByRef)}({param0})]</b>";
    }


    public static string Wrap(string text, string wrapping)
    {
      if (wrapping.IsEmpty())
        return text;

      int split = wrapping.Length / 2;

      if (split == 0)
        return wrapping[0] + text;

      return $"{wrapping.Substring(0, split)}{text}{wrapping.Substring(split)}";
    }


    public static string Color(string text, Color32 color)
    {
      return $"<color=#{color.ToHex()}>{text}</color>";
    }


    public static string String(string str)
    {
      if (str == null)
        return Null;
      if (str.Length == 0)
        return EmptyStringLiteral;

      if (str[0] != '\"') // add quotes
        return Color($"\"{str}\"", Colors.GUI.String);

      return Color(str, Colors.GUI.String);
    }
    public static string String(object obj)
    {
      return String(obj?.ToString());
    }


    public static string Comment(string str)
    {
      return Color(" // " + str, Colors.GUI.Comment);
    }


    public static string Value(object value)
    {
      string v = value?.ToString();
      if (v == null)
        return Null;

      return Color(v, Colors.GUI.Value);
    }
    public static string Value(string prefix, object value)
    {
      string v = value?.ToString();
      if (v == null)
        return prefix + Null;

      return prefix + Color(v, Colors.GUI.Value);
    }

    public static string Value<T>(T value)
      where T : struct
    {
      return Color(value.ToString(), Colors.GUI.Value);
    }
    public static string Value<T>(string prefix, T value)
      where T : struct
    {
      return prefix + Color(value.ToString(), Colors.GUI.Value);
    }


    public static string RefType(string class_name)
    {
      if (class_name.IsEmpty() || class_name[0] != '<')
        return Color($"<{class_name}>", Colors.GUI.TypeByRef);

      return Color(class_name, Colors.GUI.TypeByRef);
    }

    public static string ValType(string struct_name)
    {
      if (struct_name.IsEmpty() || struct_name[0] != '<')
        return Color($"<{struct_name}>", Colors.GUI.TypeByVal);

      return Color(struct_name, Colors.GUI.TypeByVal);
    }



    public static string Italics(string text)
    {
      return $"<i>{text}</i>";
    }

    public static string Bold(string text)
    {
      return $"<b>{text}</b>";
    }

    public static string Emphasis(string text)
    {
      return $"<i><b>{text}</b></i>";
    }

    public static StringBuilder AppendEmphasis(this StringBuilder strb, string text)
    {
      return strb.Append("<i><b>").Append(text).Append("</b></i>");
    }


    public static string Size(string text, int size)
    {
      return $"<size={size}>{text}</size>";
    }

    public static string Small(string text)
    {
      return Size(text, FontSizeSmall);
    }


    [System.Obsolete("This was a drunk algorithm. (UnityUpgradable) -> RichText.RemoveSoberly()", error: false)]
    public static string Remove(string text)
    {
      // probably a better way to do this but am dronk.

      // This does not handle <>-enclosed slugs that are not proper rich text tags!
      // See "Sober" version below.

      if (text == null || text.Length < 3)
        return text;

      var bob = new StringBuilder(text.Length);

      bool good_text = true;
      foreach (char c in text)
      {
        if (good_text)
        {
          if (good_text = (c != '<'))
          {
            bob.Append(c);
          }
        }
        else
        {
          good_text = c == '>';
        }
      }

      return bob.ToString();
    }


    public static string RemoveSoberly(string text)
    {
      // this is a more sober attempt at the above method.

      if (text == null || text.Length < 3)
        return text;

      var bob = new StringBuilder(text.Length / 2);

      int i = 0, size = text.Length;

      (int start, int end) lhs = (0, 0),
                           rhs = (0, size - 1);

      int found = 0; // 1 = found open, 2 = found pair
      while (i < size)
      {
        if (text[i] == '<')
        {
          if (found == 0 && text[i + 1] != '/')
          {
            int lhs_end = i - 1;

            while (++i < size)
            {
              if (text[i] == '>')
              {
                found     = 1;
                lhs.end   = lhs_end;
                rhs.start = i + 1;
                break;
              }
            }
          }
          else if (found == 1 && text[i + 1] == '/')
          {
            int rhs_end = i - 1;

            while (++i < size)
            {
              if (text[i] == '>')
              {
                found = 2;
                rhs.end = rhs_end;
                break;
              }
            }
          }
        }

        ++i;

        if (found == 2)
        {
          // by now, should have two matching tags.
          // Append them and prepare for next loop:

          lhs.end -= lhs.start - 1;
          rhs.end -= rhs.start - 1;

          if (lhs.end > 0)
            bob.Append(text, lhs.start, lhs.end);
          if (rhs.end > 0)
            bob.Append(text, rhs.start, rhs.end);

          lhs.start = i;
          lhs.end   = i;
          rhs.start = i;
          rhs.end   = size - 1;
          found     = 0;
        }
      } // end outermost while loop

      if (found == 0 && (rhs.end -= rhs.start - 1) > 0)
      {
        bob.Append(text, rhs.start, rhs.end);
      }

      return bob.ToString();
    }


    public static int LastIndexOf(string str, char find)
    {
      if (str.IsEmpty())
        return -1;

      var splits = str.Split(new string[] { "</" },
                             System.StringSplitOptions.RemoveEmptyEntries);

      if (splits.Length == 1)
        return str.LastIndexOf(find);

      int i = splits.Length;
      while (i --> 0)
      {
        char next_brack = '>';
        string split = splits[i];
        int j = split.Length;
        
        while (j --> 0)
        {
          char c = split[j];

          if (c == next_brack)
          {
            // ('\0' implicitly means we're not looking for anymore brackets this split)
            if (c == '<' && i < splits.Length - 1)
              next_brack = '\0';
            else
              next_brack = '<';
          }
          else if (next_brack != '<' && c == find)
          {
            for (int k = 0; k < i; ++k)
              j += splits[k].Length + 2; // 2 from "</" that was cut

            return j;
          }
        }
      }

      return -1; // not found
    }


    private static HashMap<Type, string> s_PlainTypeNameCache = new HashMap<Type, string>();

    //private static HashMap<Color32, HashMap<Type, string>>
    //  s_TypeNameCaches = new HashMap<Color32, HashMap<Type, string>>()
    //  {
    //    { default(Color32), s_PlainTypeNameCache }
    //  };

    public static string TypeNamePlain(Type type)
    {
      if (type == null)
        return "null";

      return TypeNamePlainRecursive(type);
    }
    private static string TypeNamePlainRecursive(Type type)
    {
      if (type.IsArray)
      {
        return TypeNamePlainRecursive(type.GetElementType()) + "[]";
      }

      if (s_PlainTypeNameCache.Find(type, out string result))
      {
        return result;
      }

      s_PlainTypeNameCache[type] =
        result = MakeTypeNamePlain(new StringBuilder(16), type);

      return result;
    }
    private static string MakeTypeNamePlain(StringBuilder bob, Type type)
    {
      if (type.DeclaringType != null)
      {
        if (s_PlainTypeNameCache.Find(type.DeclaringType, out string declarer))
        {
          bob.Append(declarer);
        }
        else
        {
          s_PlainTypeNameCache[type.DeclaringType] = MakeTypeNamePlain(bob, type.DeclaringType);
        }

        bob.Append('.');
      }

      if (type.IsGenericType || type.IsGenericTypeDefinition)
      {
        string name = type.Name;
        int    tick = name.LastIndexOf('`');

        if (tick < 0)
          tick = name.Length;

        bob.Append(name, 0, tick)
           .Append('<');

        if (type.IsGenericType)
        {
          var type_args = type.GetGenericArguments();
          int i = 0, len = type_args.Length;
          while (i < len)
          {
            bob.Append(TypeNamePlainRecursive(type_args[i]));

            if (++i < len)
              bob.Append(", ");
          }
        }

        return bob.Append('>').ToString();
      }
      else
      {
        return bob.Append(type.Name).ToString();
      }
    }


    public static string TypeName(Type type, Color32 color, Style style)
    {
      return Make(TypeName(type, color), style);
    }
    public static string TypeName(Type type, Color32 color)
    {
      if (color.IsDefault())
        return TypeNamePlain(type);

      if (type == null)
        return Color("null", color);

      return Color(TypeNamePlainRecursive(type), color);
    }


    public static StringBuilder AppendTypeNamePlain(this StringBuilder strb, object obj)
    {
      if (obj == null)
        return strb.Append("null");
      else
        return strb.Append(TypeNamePlainRecursive(obj.GetType()));
    }


    public static string Signature(MethodInfo method)
    {
      if (method == null)
        return "null";

      return AppendSignature(new StringBuilder(), method).ToString();
    }

    public static StringBuilder AppendSignature(this StringBuilder bob, MethodInfo method)
    {
      string name = method.Name;

      if (name[0] == '<') // anonymous function
      {
        var decltype = method.DeclaringType.DeclaringType ?? method.DeclaringType;

        bob.Append(decltype.GetRichLogName())
           .Append('.').Append(name, 1, name.IndexOf('>') - 1)
           .Append('.').Append("<Anonymous>(");
      }
      else
      {
        int dot = name.LastIndexOf('.') + 1;
        bob.Append(method.DeclaringType.GetRichLogName())
           .Append('.').Append(name, dot, name.Length - dot).Append('(');
      }

      var parms = method.GetParameters();

      int i = 0, ilen = parms.Length;
      while (i < ilen)
      {
        AppendParameterName(bob, parms[i], comma: ++i < ilen);
      }

      return bob.Append(')');
    }

    private static void AppendParameterName(StringBuilder bob, ParameterInfo parameter, bool comma)
    {
      // denote optional parameters by encasing with "[]":
      if (parameter.HasDefaultValue)
        bob.Append('[');

      // out/in/ref tags:
      if (parameter.ParameterType.IsByRef)
      {
        if (parameter.IsOut)
          bob.Append("out ");
        else if (parameter.IsIn)
          bob.Append("in ");
        else
          bob.Append("ref ");
      }

      // the actual name:
      string name = parameter.Name;
      if (Logging.Assert(!name.IsEmpty()))
        name = "_ ";

      bob.Append(name);

      // array marker
      if (parameter.ParameterType.IsArray)
      {
        bob.Append("[]");
      }

      // add in intermediate commas:
      if (comma)
      {
        if (parameter.HasDefaultValue)
          bob.Append(",] ");
        else
          bob.Append(", ");
      }
      else if (parameter.HasDefaultValue)
      {
        bob.Append(']');
      }

      // done
    }

  }

}