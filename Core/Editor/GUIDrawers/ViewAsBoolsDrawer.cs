/**
@file   PyroDK/Core/Editor/GUIDrawers/ViewAsBoolsDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-20

@brief
  PropertyDrawer for fields decorated with `[ButtonBool]`.
**/

using System.Collections.Generic;

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{
  using Enum = System.Enum;


  public static partial class GUIDrawers
  {

    [CustomPropertyDrawer(typeof(ViewAsBoolsAttribute))]
    private sealed class ViewAsBoolsDrawer : PropertyDrawer
    {
      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        var attr = (ViewAsBoolsAttribute)attribute;

        label = EditorGUI.BeginProperty(total, label, prop);

        bool expand = FoldoutPrefixLabel(total, out Rect field, label, prop.isExpanded);
        field.height = STD_LINE_HEIGHT;

        if (prop.isExpanded)
        {
          if (attr.VisibleValues == null)
          {
            InvalidField(field, message: "[ViewAsBools] is for enum bitflag types!");
            EditorGUI.EndProperty();
            return;
          }

          foreach (var (text, value) in attr.VisibleValues)
          {
            label.text = text;

            EditorGUI.BeginChangeCheck();
            _ = EditorGUI.ToggleLeft(field, label, prop.longValue.HasAllBits(value));
            if (EditorGUI.EndChangeCheck())
              prop.longValue ^= value;

            field.y += STD_LINE_ADVANCE;
          }
        }
        else if (attr.EnumType != null)
        {
          string value_label = Enum.ToObject(attr.EnumType, prop.longValue).ToString();
          InfoField(field, value_label, Styles.NumberInfo);
        }

        EditorGUI.EndProperty();

        prop.isExpanded = expand;
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        var attr = (ViewAsBoolsAttribute)attribute;

        if (attr.EnumType == null)
        {
          if (Logging.Assert(fieldInfo.FieldType.IsEnum))
            return STD_LINE_HEIGHT;

          attr.EnumType = fieldInfo.FieldType;
        }

        if (!prop.isExpanded)
          return STD_LINE_HEIGHT;

        if (attr.VisibleValues != null)
        {
          return CalcHeight(attr.VisibleValues.Count);
        }

        if (prop.propertyType != SerializedPropertyType.Enum)
        {
          // TODO support SerializedPropertyType.Integer
          attr.EnumType = null;
          return STD_LINE_HEIGHT;
        }

        if (!TSpy<System.FlagsAttribute>.IsAttributeOn(attr.EnumType))
        {
          $"{TSpy<ViewAsBoolsAttribute>.LogName} is currently only valid for [System.Flags] enums."
            .LogError(attr);
          return STD_LINE_HEIGHT;
        }

        // gather and cache the enum field data:
        
        var fields = attr.EnumType.GetFields(TypeMembers.ENUMS);

        attr.VisibleValues = new List<(string label, long value)>(fields.Length);

        if (fields.Length == 0)
          return STD_LINE_HEIGHT;

        foreach (var field in fields)
        {
          if (field.IsHidden())
            continue;

          long value = ((System.IConvertible)field.GetValue(null)).ToInt64(null);

          // don't draw empty or combined bit flags
          if (value != 0 && Bitwise.IsOneBit(value))
          {
            attr.VisibleValues.Add((Strings.ExpandCamelCase(field.Name), value));
          }
        }

        return CalcHeight(attr.VisibleValues.Count);
      }

    }

  }
}