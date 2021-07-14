/**
@file   PyroDK/Core/Utilities/Lists.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-18

@brief
  Defines me.
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  using IConvertible = System.IConvertible;


  public static class Lists
  {

    public static T At<T, TIdx>(this IReadOnlyList<T> list, TIdx idx)
      where TIdx : IConvertible
    {
      // useful to use enums as indices
      return list[idx.ToInt32(null)];
    }


    /// <summary>
    /// DOES NOT CHECK FOR NULL OR EMPTY
    /// </summary>
    public static T Random<T>(this IReadOnlyList<T> list)
    {
      return list[Integers.RandomIndex(list.Count)];
    }

    public static bool IsEmpty<T>(this IReadOnlyCollection<T> collection)
    {
      return collection == null || collection.Count == 0;
    }



    public static int MakePackedArray<T>(this T[] orig, out T[] array)
    {
      int len = orig.Length, i = 0, j = 0;
      array = new T[len];

      while (i < len)
      {
        var t = orig[i++];

        if (t == null || t.Equals(default))
          continue;

        array[j++] = t;
      }

      if (j < i)
      {
        var temp = new T[j];
        System.Array.Copy(array, temp, j);
        array = temp;
      }

      return i - j;
    }

    public static int MakePackedArray<T>(this IReadOnlyCollection<T> orig, out T[] array)
    {
      int len = orig.Count, i = 0, j = 0;
      array = new T[len];

      foreach (var t in orig)
      {
        ++i;

        if (t == null || t.Equals(default))
          continue;

        array[j++] = t;
      }

      if (j < i)
      {
        var temp = new T[j];
        System.Array.Copy(array, temp, j);
        array = temp;
      }

      return i - j;
    }


    public static IList<T> Fill<T>(this IList<T> list, System.Func<T> ctor)
    {
      Debug.Assert(ctor != null);

      for (int count = list.Count, i = 0; i < count; ++i)
      {
        list[i] = ctor();
      }

      return list;
    }


    public static void PushFront<T>(this IList<T> list, T item)
    {
      list.Insert(0, item);
    }

    public static void PushBack<T>(this IList<T> list, T item)
    {
      list.Insert(list.Count, item);
    }


    public static T PopFront<T>(this IList<T> list) // V BAD
    {
      T result = list[0];
      list.RemoveAt(0);
      return result;
    }

    public static T PopBack<T>(this IList<T> list)
    {
      T result = list[list.Count - 1];
      list.RemoveAt(list.Count - 1);
      return result;
    }

    public static T[] PopBack<T>(this IList<T> list, int count)
    {
      T[] result = new T[count];

      int i = 0, j = list.Count;
      while (count --> 0 && j > 0)
      {
        result[i++] = list[--j];
        list.RemoveAt(j);
      }

      return result;
    }


    public static T Front<T>(this IReadOnlyList<T> list, T fallback = default)
    {
      if (list == null || list.Count == 0)
        return fallback;

      return list[0];
    }

    public static T Back<T>(this IReadOnlyList<T> list, T fallback = default)
    {
      if (list == null || list.Count == 0)
        return fallback;

      return list[list.Count - 1];
    }


    public static bool TryBinarySearch<T>(this IReadOnlyList<T> list, out T found, System.Func<T, int> where)
    {
      found = default;

      int left  = 0;
      int right = list.Count - 1;

      while (left <= right)
      {
        int center = (left + right) / 2;
        found = list[center];

        int cmp = where(found);

        if (cmp < 0)
          left = center + 1;
        else if (0 < cmp)
          right = center - 1;
        else
          return true;
      }

      return false;
    }


    /// <summary>
    /// DOES NOT CHECK FOR NULL OR VALID INDICES
    /// </summary>
    public static void Swap<T>(this IList<T> list, int idx1, int idx2)
    {
      T temp      = list[idx1];
      list[idx1]  = list[idx2];
      list[idx2]  = temp;
    }


    public static void Shuffle<T>(this IList<T> list)
    {
      if (list == null || list.Count < 2)
        return;

      int len = list.Count;
      for (int i = 0; i < len; ++i)
      {
        Swap(list, i, Integers.RandomIndex(len));
      }
    }


    public static IList<T> Reversed<T>(this IList<T> list)
    {
      int left = 0, right = list.Count - 1;

      while (left < right)
      {
        Swap(list, left, right);
        ++left;
        --right;
      }

      return list;
    }


    public static List<T> Sorted<T>(this List<T> list)
    {
      list?.Sort();
      return list;
    }

    public static List<T> Sorted<T>(this List<T> list, System.Comparison<T> cmp)
    {
      list?.Sort(cmp);
      return list;
    }


    public static void MakeHeap<T>(this IList<T> list, System.Comparison<T> cmp)
    {
      // `node` is the index of the last non-leaf node.
      // We start there and iterate backwards because any leaf nodes can be skipped.
      for (int node = (list.Count - 1 - 1) / 2; node >= 0; --node)
      {
        HeapifyDown(list, node, cmp);
      }
    }

    public static void PushHeap<T>(this IList<T> list, T push, System.Comparison<T> cmp)
    {
      list.Add(push);

      int child   = list.Count - 1;
      int parent  = (child - 1) / 2;

      // heapify up
      while (child > 0 && cmp(list[child], list[parent]) > 0)
      {
        Swap(list, child, parent);

        child   = parent;
        parent  = (parent - 1) / 2;
      }
    }

    public static T PopHeap<T>(this IList<T> list, System.Comparison<T> cmp)
    {
      int last = list.Count - 1;
      if (last < 0)
        return default;

      var item = list[0];

      if (last > 1)
      {
        Swap(list, 0, last);
        list.RemoveAt(last);
        HeapifyDown(list, 0, cmp);
      }

      return item;
    }

    internal static void HeapifyDown<T>(IList<T> list, int node, System.Comparison<T> cmp)
    {
      // This is way faster than the recursive version!

      int count = list.Count;
      int last  = (count - 1 - 1) / 2;
      int max   = node;

      while (node <= last)
      {
        int lhs = 2 * node + 1;
        int rhs = 2 * node + 2;

        if (lhs < count && cmp(list[lhs], list[max]) > 0)
          max = lhs;

        if (rhs < count && cmp(list[rhs], list[max]) > 0)
          max = rhs;

        if (max == node)
          return;

        Swap(list, node, max);

        node = max;
      }
    }

    internal static void HeapifyDownRecursive<T>(IList<T> list, int node, System.Comparison<T> cmp)
    {
      // eww, recursion!

      int max = node;
      int lhs = 2 * node + 1;
      int rhs = 2 * node + 2;

      if (lhs < list.Count && cmp(list[lhs], list[max]) > 0)
        max = lhs;

      if (rhs < list.Count && cmp(list[rhs], list[max]) > 0)
        max = rhs;

      if (max == node)
        return;

      Swap(list, node, max);

      if (max <= (list.Count - 1 - 1) / 2)
        HeapifyDownRecursive(list, max, cmp); // <--
    }



    public static string JoinString(this IReadOnlyList<string> list)
    {
      if (list.IsEmpty())
        return string.Empty;

      var strb = new System.Text.StringBuilder(list.Count * 12);

      for (int i = 0, len = list.Count; i < len; ++i)
      {
        _ = strb.Append(list[i]);
      }
      
      return strb.ToString();
    }


    public static string MakeLogString<T>(this IReadOnlyList<T> list,
                                               string name = null,
                                               System.Func<T, string> translator = null)
    {
      if (list == null)
        throw new System.ArgumentNullException("list");

      if (translator == null)
        translator = (element) => element?.ToString() ?? "(null)";

      var strb = new System.Text.StringBuilder(Types.GetRichLogName(list));

      if (name == null)
        _ = strb.Append($" (n={list.Count}): (Click for full view)\n");
      else
        _ = strb.Append($" \"{name}\" (n={list.Count}): (Click for full view)\n");

      int i = 0, ilen = list.Count;

      string fmt_index_log = Integers.MakeIndexPreformattedString(ilen) + ": {1}\n";
        
      while (i < ilen)
      {
        _ = strb.AppendFormat(fmt_index_log, i, translator(list[i]));
        ++i;
      }

      return strb.ToString();
    }


    #if false
    public static T[] ConstructNulls<T>(this T[] array)
      where T : class, new()
    {
      int i = array.Length;
      while (i --> 0)
      {
        if (array[i] == null)
          array[i] = new T();
      }

      return array;
    }

    public static IT[] ConstructInterface<IT, T>(this IT[] array, T empty)
      where T : IT
    {
      int i = array.Length;
      while (i --> 0)
      {
        array[i] = empty;
      }

      return array;
    }
    #endif

  } // end static class Lists

}