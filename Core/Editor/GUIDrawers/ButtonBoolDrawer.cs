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
      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        if (prop.propertyType != SerializedPropertyType.Boolean)
        {
          InvalidField(in total, label, "[ButtonBool] only valid on bool types!");
          return;
        }

        var attr = (ButtonBoolAttribute)attribute;

        string text = attr.CustomText ?? RichText.Color(label.text, Colors.Debug.String);

        var btn = PrefixLabelStrict(in total, label);
        prop.boolValue ^= GUI.Button(btn, text, Styles.Button);
      }
    }

  }
}