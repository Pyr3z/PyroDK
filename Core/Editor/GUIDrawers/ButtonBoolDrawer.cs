/**
@file   PyroDK/Core/Editor/GUIDrawers/ButtonBoolDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-20

@brief
  PropertyDrawer for fields decorated with `[ButtonBool]`.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{
  public static partial class GUIDrawers
  {

    [CustomPropertyDrawer(typeof(ButtonBoolAttribute))]
    private sealed class ButtonBoolDrawer : PropertyDrawer
    {
      public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
      {
        if (prop.propertyType != SerializedPropertyType.Boolean)
        {
          InvalidField(in pos, label, "[ButtonBool] only valid on bool types!");
          return;
        }


        var attr = (ButtonBoolAttribute)attribute;

        if (!attr.CustomText.IsEmpty())
        {
          label.text = attr.CustomText;
        }

        float width = STD_PAD_RIGHT + Styles.Button.CalcWidth(label) + STD_PAD_RIGHT;
        width = pos.width - width;

        pos.xMax -= width;
        pos.y += 1f;
        pos.height -= 2f;

        prop.boolValue ^= GUI.Button(pos, label, Styles.Button);
      }

      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
        return Styles.Button.fixedHeight + 2f;
      }

    } // end class ButtonBoolDrawer

  }
}