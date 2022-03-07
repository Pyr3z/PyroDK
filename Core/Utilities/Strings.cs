/**
@file   PyroDK/Core/Utilities/Strings.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-19

@brief
  Provides utilities and extensions for the `System.String`
  type.
**/

using System.Collections.Generic;


namespace PyroDK
{
  using StringBuilder = System.Text.StringBuilder;
  using CompareType = System.StringComparison;


  public static class Strings
  {

    public static bool IsEmpty(this string str)
    {
      return str == null || str.Length == 0;
    }

    public static bool EndsWithAny(this string str, params char[] checks)
    {
      int end;
      if (str != null && (end = str.Length - 1) >= 0 && checks.Length > 0)
      {
        for (int i = 0; i < checks.Length; ++i)
        {
          if (str[end] == checks[i])
            return true;
        }
      }

      return false;
    }


    public static bool TryParseNextIndex(this string str, out int idx)
    {
      idx = -1;

      int idx_start = str.IndexOf('[') + 1;
      int idx_len = str.IndexOf(']', idx_start) - idx_start;

      return idx_len > 0 &&
              int.TryParse(str.Substring(idx_start, idx_len), out idx) &&
              idx >= 0;
    }

    public static bool TryParseNextIndex(this string str, int start, out int idx)
    {
      return TryParseNextIndex(str.Substring(start), out idx);
    }

    public static bool TryParseLastIndex(this string str, out int idx)
    {
      idx = -1;

      int idx_start = str.LastIndexOf('[') + 1;
      int idx_len = str.Length - 1 - idx_start;

      return idx_len > 0 &&
              int.TryParse(str.Substring(idx_start, idx_len), out idx) &&
              idx >= 0;
    }


    public static bool ContainsKeyword(this string self, IEnumerable<string> keywords, bool ignore_case = true)
    {
      if (string.IsNullOrEmpty(self) || keywords == null)
        return false;

      var selflen = self.Length;
      var cmptype = ignore_case ? CompareType.OrdinalIgnoreCase : CompareType.Ordinal;

      foreach (var kw in keywords)
      {
        int kwlen = kw.Length;
        if (kwlen > selflen)
          continue;

        if (kwlen == selflen)
        {
          if (string.Compare(self, kw, cmptype) == 0)
            return true;

          continue;
        }

        --kwlen;

        for (int i = 0; i < selflen - kwlen; ++i)
        {
          // Check if first letter compares equal before comparing the rest:
          if (string.Compare(self, i, kw, 0, 1, cmptype) == 0 &&
              string.Compare(self, i + 1, kw, 1, kwlen, cmptype) == 0)
          {
            return true;
          }
        }
      }

      return false;
    }

    public static int CompareOrdinalNoCase(string lhs, string rhs)
    {
      if (lhs.Length < rhs.Length)
        return 1;
      if (rhs.Length < lhs.Length)
        return -1;
      return string.Compare(lhs, rhs, CompareType.OrdinalIgnoreCase);
    }


    public static string MakeRandom()
    {
      // TODO pull from dictionary?
      return MakeGUID();
    }

    public static string MakeGUID()
    {
      #if UNITY_EDITOR
      return UnityEditor.GUID.Generate().ToString();
      #else
      return System.Guid.NewGuid().ToString("N");
      #endif

    }


    /// <summary>
    /// Splits `text` into component strings, separated by the given bracket chars,
    /// and places the splits into a promised List.
    /// </summary>
    public static IPromiseKeeper SplitBracketRegions(string           text,
                                                     char             brack0,
                                                     char             brack1,
                                                     bool             skip_angles,
                                                     out List<string> split_buffer)
    {
      if (text.IsEmpty())
      {
        split_buffer = null;
        return null;
      }

      var keeper = BufferList<string>.MakePromise(out split_buffer);

      char[] bracks = { brack0, brack1 };
      int side = 0, idx = 0, len = text.Length;

      //int sanity = text.Length + 1;
      while ((idx + side) < len)
      {
        int next;

        if (skip_angles)
        {
          next = idx + side;

          // TODO make separate function for this
          int in_angles = 0;
          while (next < len)
          {
            char curr = text[next];

            if (curr == '<')
            {
              ++in_angles;
            }
            else if (curr == '>')
            {
              if (in_angles > 0)
                --in_angles;
            }
            else if (in_angles == 0 && curr == bracks[side])
            {
              break;
            }

            ++next;
          } // end inner while loop

          if (next == len)
            next = -1;
        }
        else // !skip_angles
        {
          next = text.IndexOf(bracks[side], idx + side);
        }

        if (next < 0) // not found, append the remainder
        {
          split_buffer.Add(text.Substring(idx + side, len - idx - side));
          idx = len;
          break;
        }

        if (idx < next) // length of substring would be positive, non-zero
        {
          split_buffer.Add(text.Substring(idx, next - idx + side));
        }

        idx  = next + side;
        side = (side + 1) % 2;
      } // end while loop

      //Debug.Assert(sanity > 0, $"INFINITE LOOP! {text}");

      if (idx < len)
      {
        split_buffer.Add(text.Substring(idx, len - idx));
      }

      if (split_buffer.Count < 2)
      {
        keeper.Dispose();
        return null;
      }

      return keeper;
    }


    public static string ExpandCamelCase(string str)
    {
      if (IsEmpty(str) || str.Length == 1)
        return str;

      var bob = new StringBuilder(str.Length + 8);

      int i = 0, ilen = str.Length;

      bool in_word = false;

      if (str[1] == '_')
      {
        if (str.Length == 2)
          return str;
        else
          i = 2;
      }
      else if (str[0] == 'm' && char.IsUpper(str[1]))
      {
        i = 1;
      }

      char c = str[i];

      if (char.IsLower(c)) // adjusts for lower camel case
      {
        in_word = true;
        bob.Append(char.ToUpper(c));
        ++i;
      }
      
      while (i < ilen)
      {
        c = str[i];

        if (char.IsLower(c) || char.IsDigit(c))
        {
          in_word = true;
        }
        else if (in_word && ( char.IsUpper(c) || c == '_' ))
        {
          bob.Append(' ');
          in_word = false;
        }

        if (char.IsLetterOrDigit(c))
          bob.Append(c);

        ++i;
      }

      // TODO this is soooo hard coded man...
      bob.Replace("Dont", "Don't");

      return bob.ToString();
    }


    //[SanityTest]
    private static void TestCompare()
    {
      string[] kws =
      {
        "mscorlib",
        "Unity",
        "System",
        "Mono",
        "Samples",
        "FU",
        "Cinemachine",
        "nunit",
        "ICSharpCode",
        "fef",
      };

      System.Array.Sort(kws, CompareOrdinalNoCase);

      kws.MakeLogString("keywords").Log();
    }

  }


}