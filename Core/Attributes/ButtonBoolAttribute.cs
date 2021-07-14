
/**
@file   PyroDK/Core/Attributes/ButtonBoolAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-26

@brief
  Renders a bool as a button in the Inspector.
**/

using UnityEngine;


namespace PyroDK
{

  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class ButtonBoolAttribute : PropertyAttribute
  {
    public readonly string CustomText;


    public ButtonBoolAttribute(string text = null)
    {
      CustomText = text;
    }

  }

}