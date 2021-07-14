/**
@file   PyroDK/Core/Editor/GUIDrawers/RequireFieldValueDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  Highlights an Object reference field if it is a missing reference.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  using FieldInfo = System.Reflection.FieldInfo;


  public static partial class GUIDrawers
  {

    private static readonly string s_WarningLabelSuffix = RichText.Make("( ! )", RichText.Style.Bold, Colors.Yellow);


    [CustomPropertyDrawer(typeof(RequireFieldValueAttribute))]
    private sealed class RequireFieldValueDrawer : PropertyDrawer
    {
      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        if (prop.propertyType != SerializedPropertyType.ObjectReference)
        {
          InvalidField(in total, label, $"{RichText.Attribute("RequireFieldValue")} requires an Object reference field!");
          return;
        }

        var color = ((RequireFieldValueAttribute)attribute).WarningColor;

        if (!label.tooltip.IsEmpty() && !label.tooltip.EndsWith("\n"))
        {
          label.tooltip += '\n';
        }

        bool warn = false;

        if (!prop.objectReferenceValue)
        {
          warn = true;
          label.tooltip += $"Null object cannot be checked for valid field(s)!";
        }
        else
        {
          var type = prop.objectReferenceValue.GetType();
          var attrs = fieldInfo.GetCustomAttributes(typeof(RequireFieldValueAttribute), inherit: true)
                               .SelectType<RequireFieldValueAttribute>();
          foreach (var attr in attrs)
          {
            if (!type.TryGetSerializableField(attr.FieldName, out FieldInfo field))
            {
              $"Could not locate field \"{attr.FieldName}\" for validation attribute {RichText.Attribute("RequireFieldValue")}."
                .LogWarning();
              continue;
            }

            var field_value = field.GetValue(prop.objectReferenceValue);

            if (!(field_value?.Equals(attr.RequiredValue) ?? attr.RequiredValue == null))
            {
              warn = true;
              label.tooltip += $"Invalid value in field {RichText.String(attr.FieldName)};\n";
              label.tooltip += $"      value should be: {RichText.Value(attr.RequiredValue)}\n";
            }
          }
        }

        if (!label.tooltip.IsEmpty())
        {
          label.tooltip = label.tooltip.TrimEnd('\n', ' ');
        }

        if (warn)
        {
          DrawRect(total, Colors.Debug.Attention, color);

          var suff = new Rect(total)
          {
            xMax = LabelEndX
          };

          GUI.Label(suff, s_WarningLabelSuffix, Styles.LabelDetail);
        }

        total.y += STD_PAD;
        total.height -= STD_PAD_RIGHT;

        _ = EditorGUI.PropertyField(total, prop, label);
      }


      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
        return STD_LINE_HEIGHT + STD_PAD_RIGHT;
      }

    }

  }

}