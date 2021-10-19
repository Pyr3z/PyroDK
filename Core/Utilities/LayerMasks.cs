/**
@file   PyroDK/Core/Utilities/LayerMasks.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-06

@brief
  Provides utilities for the `UnityEngine.LayerMask` struct and concept.
**/

using System.Collections.Generic;
using System.Text;

using UnityEngine;


namespace PyroDK
{

  public static class LayerMasks
  {
    public static readonly LayerMask  Enabled   = (1 << 31);
    public static readonly LayerMask  Disabled  = (0 << 0);

    public static string[]  Labels      => RebuildLabelArray();


    private static string[] s_Labels = null;

    private static int[] s_IdxToLabelIdx, s_LabelIdxToIdx;



    public static string[] RebuildLabelArray()
    {
      if (s_Labels != null && s_Labels.Length > 0)
        return s_Labels;

      s_IdxToLabelIdx = new int[31];

      using (var buffer = new BufferList<string>(31))
      {
        for (int i = 0; i < 31; ++i)
        {
          var tempstr = LayerMask.LayerToName(i);

          if (tempstr.IsEmpty())
          {
            s_IdxToLabelIdx[i] = -1;
          }
          else
          {
            s_IdxToLabelIdx[i] = buffer.Count;
            buffer.Add($"[ {i,-2}] {tempstr}");
          }
        }

        buffer.MakeArray(out s_Labels);
      }

      s_LabelIdxToIdx = new int[s_Labels.Length];

      for (int i = 0; i < 31; ++i)
      {
        if (s_IdxToLabelIdx[i] > -1)
        {
          s_LabelIdxToIdx[s_IdxToLabelIdx[i]] = i;
        }
      }

      return s_Labels;
    }


    public static int ToFirstLayerIndex(this LayerMask mask)
    {
      return Bitwise.CTZ(mask & int.MaxValue);
    }

    public static LayerMask FromLayerIndex(int idx)
    {
      return 1 << idx;
    }

    public static string IndexToLayerName(int idx)
    {
      if (idx == 31)
        return "<enabled flag>";
      if (idx < 0 || idx > 31)
        throw new System.ArgumentException("a LayerMask idx should be in the range [0,31]");

      int label_idx = s_IdxToLabelIdx[idx];

      if (label_idx > -1)
        return Labels[label_idx];

      return $"<unused layer {idx}>";
    }

    public static string ToFirstLayerName(this LayerMask mask)
    {
      return IndexToLayerName(mask.ToFirstLayerIndex());
    }

    public static int ToLabelMask(this LayerMask mask)
    {
      if (s_IdxToLabelIdx == null)
        return 0;

      int result  = 0;
      int idx     = Bitwise.CTZ(mask.value);

      while (mask != 0 && idx < 31)
      {
        int lidx = s_IdxToLabelIdx[idx];

        if (lidx > -1)
          result |= (1 << lidx);

        mask = Bitwise.ShaveLSB(mask.value);
        idx  = Bitwise.CTZ(mask.value);
      }

      return result;
    }

    public static LayerMask FromLabelMask(int lmask)
    {
      int result  = 0;
      int lidx    = Bitwise.CTZ(lmask);
      int len     = s_LabelIdxToIdx.Length;

      while (lmask != 0 && lidx < len)
      {
        int idx = s_LabelIdxToIdx[lidx];

        if (idx > -1)
          result |= (1 << idx);

        lmask  = Bitwise.ShaveLSB(lmask);
        lidx   = Bitwise.CTZ(lmask);
      }

      return result;
    }


    public static string JoinLayerNames(this LayerMask mask)
    {
      const string SEPARATOR = ", ";

      var strb    = new StringBuilder();
      int idx     = Bitwise.CTZ(mask);

      while (mask != 0 && idx < 31)
      {
        strb.Append(Labels[s_IdxToLabelIdx[idx]]);

        mask = Bitwise.ShaveLSB(mask);
        idx  = Bitwise.CTZ(mask);

        if (mask != 0 && idx < 31)
        {
          strb.Append(SEPARATOR);
        }
      }

      return strb.ToString();
    }


    public static bool IsEnabled(this LayerMask mask)
    {
      return Bitwise.HasSignBit(mask);
    }

    public static LayerMask SetEnabled(this LayerMask mask, bool set = true)
    {
      return Bitwise.SetSignBit(mask, set);
    }

    public static LayerMask Toggled(this LayerMask mask)
    {
      return Bitwise.ToggleSignBit(mask);
    }



    public static bool Contains(this LayerMask mask, GameObject obj)
    {
      return (mask & (1 << obj.layer)) != 0;
    }

    public static bool ContainsAny(this LayerMask mask, GameObject obj1, params GameObject[] objs)
    {
      if ((mask & (1 << obj1.layer)) != 0)
        return true;

      foreach (var obj in objs)
        if ((mask & (1 << obj.layer)) != 0)
          return true;

      return false;
    }

    public static bool ContainsAll(this LayerMask mask, GameObject obj1, params GameObject[] objs)
    {
      if ((mask & (1 << obj1.layer)) == 0)
        return false;

      foreach (var obj in objs)
        if ((mask & (1 << obj.layer)) == 0)
          return false;

      return true;
    }


    public static bool ContainsAny(this LayerMask mask, IEnumerable<GameObject> objs)
    {
      foreach (var obj in objs)
        if ((mask & (1 << obj.layer)) != 0)
          return true;

      return false;
    }

    public static bool ContainsAll(this LayerMask mask, IEnumerable<GameObject> objs)
    {
      foreach (var obj in objs)
        if ((mask & (1 << obj.layer)) == 0)
          return false;

      return true;
    }


    //[SanityTest]
    private static void TestLayerMaskSanity()
    {
      LayerMask mask = LayerMask.GetMask("Default", "Ignore Raycast", "Water");
      mask.JoinLayerNames().Log();

      s_IdxToLabelIdx.MakeLogString("IdxToLabelIdx").Log();
      s_LabelIdxToIdx.MakeLogString("LabelIdxToIdx").Log();
    }

  }

}