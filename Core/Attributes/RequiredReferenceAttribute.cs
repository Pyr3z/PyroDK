/**
@file   PyroDK/Core/Attributes/RequiredReferenceAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-26

@brief
  Outlines an Object reference field if it seems to be missing.

  Currently only valid on `UnityEngine.Object` references.
**/

using UnityEngine;


namespace PyroDK
{

  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class RequiredReferenceAttribute : PropertyAttribute
  {
    public static readonly Color32 DEFAULT_COLOR = Colors.Debug.Attention.Alpha(0.375f);


    public bool   DisableIfPrefab = false;
    public string IgnoreIfProperty;

    public Color32 Highlight = DEFAULT_COLOR;


    public RequiredReferenceAttribute()
    {
    }

    public RequiredReferenceAttribute(string color_hex)
    {
      Highlight = Colors.FromHex(color_hex);
    }

  }

}