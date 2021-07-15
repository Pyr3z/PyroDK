/**
@file   PyroDK/Core/Editor/GUIDrawers/SignBitBoolDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-06

@brief
  Draws integers marked with [SignBitBool].
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class GUIDrawers
  {
    [CustomPropertyDrawer(typeof(SignBitBoolAttribute))]
    private sealed class SignBitBoolDrawer : PropertyDrawer
    {

      private GUIContent m_BoolLabel        = null;
      private bool       m_BoolTogglesValue = false;

      public override void OnGUI(Rect pos, SerializedProperty sprop, GUIContent label)
      {
        switch (sprop.propertyType)
        {
          case SerializedPropertyType.Integer:
            {
              int current = sprop.intValue;

              EditorGUI.BeginChangeCheck();

              if (ShouldDrawInline())
                current = DrawInline(pos, current, label);
              else
                current = DrawSeparate(pos, current, m_BoolLabel, label);

              if (EditorGUI.EndChangeCheck())
              {
                sprop.intValue = current;
                sprop.serializedObject.ApplyModifiedProperties();
              }
            }
            return;

          case SerializedPropertyType.Float:
            {
              float current = sprop.floatValue;

              EditorGUI.BeginChangeCheck();

              if (ShouldDrawInline())
                current = DrawInline(pos, current, label);
              else
                current = DrawSeparate(pos, current, m_BoolLabel, label);

              if (EditorGUI.EndChangeCheck())
              {
                sprop.floatValue = current;
                sprop.serializedObject.ApplyModifiedProperties();
              }
            }
            break;

          default:
            InvalidField(pos, $"{RichText.Attribute("SignBitBool")} needs int / float!", label);
            return;
        }
      }
      public override float GetPropertyHeight(SerializedProperty sprop, GUIContent label)
      {
        if (ShouldDrawInline())
          return STD_LINE_ADVANCE;

        return STD_LINE_ADVANCE * 2.0f;
      }


      private bool ShouldDrawInline()
      {
        if (m_BoolLabel == null)
        {
          var attr = (SignBitBoolAttribute)attribute;
          m_BoolTogglesValue = attr.TogglesValue;
          m_BoolLabel = new GUIContent(attr.LabelBoolean);
        }

        return m_BoolLabel.text.IsEmpty();
      }


      private int DrawInline(Rect pos, int current, GUIContent label_int)
      {
        bool boolval = ToggledPrefixLabel(pos, label_int, !current.HasSignBit(), out pos);

        EditorGUI.BeginDisabledGroup(m_BoolTogglesValue && !boolval);
        current = EditorGUI.IntField(pos, current.SetSignBit(false), Styles.NumberField);
        EditorGUI.EndDisabledGroup();

        if (current < 0)
          current = 0;

        return current.SetSignBit(!boolval);
      }

      private int DrawSeparate(Rect pos, int current, GUIContent label_bool, GUIContent label_int)
      {
        pos.height = STD_LINE_HEIGHT;

        bool boolval = EditorGUI.Toggle(pos, label_bool, !current.HasSignBit());

        pos.y += STD_LINE_ADVANCE;

        EditorGUI.BeginDisabledGroup(m_BoolTogglesValue && !boolval);
        current = EditorGUI.IntField(pos, label_int, current.SetSignBit(false), Styles.NumberField);
        EditorGUI.EndDisabledGroup();

        if (current < 0)
          current = 0;

        return current.SetSignBit(!boolval);
      }


      private float DrawInline(Rect pos, float current, GUIContent label)
      {
        bool boolval = ToggledPrefixLabel(pos, label, current >= Floats.EPSILON, out pos);

        EditorGUI.BeginDisabledGroup(m_BoolTogglesValue && !boolval);

        if (current < Floats.EPSILON)
        {
          if (boolval) // switch on
          {
            if (current < -Floats.EPSILON)
              current = EditorGUI.FloatField(pos, -current, Styles.NumberField);
            else
              current = EditorGUI.FloatField(pos, Floats.EPSILON, Styles.NumberField);
          }
          else
          {
            float flipped = Floats.SqueezedNaN(-current - 1f - Floats.EPSILON) + 1f;
            float edit    = Mathf.Max(EditorGUI.FloatField(pos, flipped, Styles.NumberField), 0f);
           
            if (!edit.Approximately(flipped))
            {
              current = -(edit + Floats.EPSILON);
            }
            else
            {
              GUI.changed = false;
            }
          }
        }
        else
        {
          if (boolval)
          {
            float adjusted = Floats.SqueezedNaN(current - 1f - Floats.EPSILON) + 1f;
            float edit = Mathf.Max(EditorGUI.FloatField(pos, adjusted, Styles.NumberField), 0f);

            if (!edit.Approximately(adjusted))
            {
              current = edit + Floats.EPSILON;
            }
            else
            {
              GUI.changed = false;
            }
          }
          else // switch off
          {
            current = -EditorGUI.FloatField(pos, current, Styles.NumberField);
          }
        }

        EditorGUI.EndDisabledGroup();

        return current;
      }

      private float DrawSeparate(Rect pos, float current, GUIContent label_bool, GUIContent label_float)
      {
        pos.height = STD_LINE_HEIGHT;

        bool boolval = EditorGUI.Toggle(pos, label_bool, current >= Floats.EPSILON);

        pos.y += STD_LINE_ADVANCE;

        EditorGUI.BeginDisabledGroup(m_BoolTogglesValue && !boolval);

        if (current < Floats.EPSILON)
        {
          if (boolval) // switch on
          {
            if (current < 0f)
              current = EditorGUI.FloatField(pos, label_float, -current, Styles.NumberField);
            else
              current = EditorGUI.FloatField(pos, label_float, 0f, Styles.NumberField);
          }
          else
          {
            float flipped = Floats.SqueezedNaN(-current - 1f - Floats.EPSILON) + 1f;
            float edit = Mathf.Max(EditorGUI.FloatField(pos, label_float, flipped, Styles.NumberField), 0f);

            if (!edit.Approximately(flipped))
            {
              current = -(edit + Floats.EPSILON);
            }
            else
            {
              GUI.changed = false;
            }
          }
        }
        else
        {
          if (boolval)
          {
            float adjusted = Floats.SqueezedNaN(current - 1f - Floats.EPSILON) + 1f;
            float edit = Mathf.Max(EditorGUI.FloatField(pos, label_float, adjusted, Styles.NumberField), 0f);

            if (!edit.Approximately(adjusted))
            {
              current = edit + Floats.EPSILON;
            }
            else
            {
              GUI.changed = false;
            }
          }
          else // switch off
          {
            current = -EditorGUI.FloatField(pos, label_float, current, Styles.NumberField);
          }
        }

        EditorGUI.EndDisabledGroup();

        return current;
      }

    }
  }
}