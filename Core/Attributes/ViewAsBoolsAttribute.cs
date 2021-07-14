/**
@file   PyroDK/Core/Attributes/ViewAsBoolsAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-12

@brief
  A `PropertyAttribute` that interprets a [System.Flags]
  enum as an array of boolean values.
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{

  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class ViewAsBoolsAttribute : PropertyAttribute
  {
    internal System.Type EnumType = null;

    internal List<(string label, long value)> VisibleValues = null;


    public ViewAsBoolsAttribute()
    {
      // can deduce the enum type in the PropertyDrawer.
    }

    public ViewAsBoolsAttribute(System.Type enum_type)
    {
      Debug.Assert(enum_type.IsEnum, "Must pass an enum type to [ViewAsBools]!");
      EnumType = enum_type;
    }

  }

}