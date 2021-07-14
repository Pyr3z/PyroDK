/**
@file   PyroDK/Core/Attributes/SignBitBoolAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-13

@brief
  Attribute applied to a signed integer (64-bit or 32-bit) telling
  the PropertyDrawer to treat the sign bit as a separate boolean.
**/

using UnityEngine;


namespace PyroDK
{

  public class SignBitBoolAttribute : PropertyAttribute
  {
    public readonly string LabelBoolean;
    public readonly bool   TogglesValue;

    public SignBitBoolAttribute(string boolean_label = null, bool toggles_value = true)
    {
      LabelBoolean  = boolean_label;
      TogglesValue  = toggles_value;
    }

  }

}