/**
@file   PyroDK/Core/Attributes/ViewAsEnumAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-26

@brief
  A `PropertyAttribute` that tries its best to interpret the fields
  it is applied to as an enum of the given type (and draws it so).
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{

  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class ViewAsEnumAttribute : PropertyAttribute
  {
    public TriBool UseBitFlags      = TriBool.Null;
    public bool    IncludeBitCombos = true;


    internal System.Type EnumType         = null;
    internal string      DeferredEnumType = null;

    internal string[] Labels   = null;
    internal int      StartBit = int.MaxValue;
    internal long     AllMask  = ~0;

    internal HashSet<System.Enum> VisibleEnums = null;

    public ViewAsEnumAttribute(System.Type enum_type)
    {
      Debug.Assert(enum_type.IsEnum, "Must pass an enum type to [ViewAsEnum]!");
      EnumType = enum_type;
    }

    public ViewAsEnumAttribute(string deferred_enum_type)
    {
      if (!Assemblies.FindSubType(deferred_enum_type, typeof(System.Enum), out EnumType))
      {
        DeferredEnumType = deferred_enum_type;
      }
    }

  }

}