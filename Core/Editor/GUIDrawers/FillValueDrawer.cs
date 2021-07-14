/**
@file   PyroDK/Core/Editor/GUIDrawers/FillValueDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-20

@brief
  PropertyDrawer for `PyroDK.FillValue` fields.
**/

using System.Collections.Generic;

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class GUIDrawers
  {

    [CustomPropertyDrawer(typeof(FillValue), useForChildren: true)]
    private sealed class FillValueDrawer : PropertyDrawer
    {
      private FillValue   m_Target;
      private List<float> m_ChildHeights = new List<float>();

      private bool m_IsReadOnly;


      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        if (m_ChildHeights == null || !this.TryGetUnderlyingValue(prop, out m_Target))
        {
          m_ChildHeights = null;
          InvalidField(total, label);
          return;
        }

        total.height = STD_LINE_HEIGHT;

        if (m_IsReadOnly)
          GUI.enabled = true;

        bool expand = FoldoutPrefixLabel(total, out Rect field,
                                         label,
                                         prop.isExpanded);

        var color_bar_text = m_Target.FillColor.Inverted().AlphaBump();

        DrawFillBar(in field, m_Target.FillColor,
                              m_Target.Normalized,
                              color_bar_text);

        if (m_IsReadOnly)
        {
          var style = (m_Target.Normalized < 0.5f) ? Styles.TextDetail : Styles.TextDetailLeft;
          InfoField(field.ExpandedWidth(-STD_PAD_RIGHT), TSpy<ReadOnlyAttribute>.LogName, style);
        }

        if (prop.isExpanded)
        {
          PushNextIndentLevel(fix_label_width: false);

          var line = new Rect(total);

          int i = 0;
          foreach (var child in prop.VisibleChildren())
          {
            if (m_IsReadOnly && child.name == "m_Value")
              continue;

            line.y      += line.height + STD_PAD;
            line.height  = m_ChildHeights[i++];

            EditorGUI.PropertyField(line, child, includeChildren: true);
          }

          PopIndentLevel(fix_label_width: false);
        }

        prop.isExpanded = expand;

        if (m_IsReadOnly)
          GUI.enabled = false;
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        m_IsReadOnly = fieldInfo.IsDefined<ReadOnlyAttribute>();

        if (!prop.isExpanded || m_ChildHeights == null)
        {
          prop.isExpanded = false;
          return STD_LINE_ADVANCE;
        }

        float total = STD_LINE_ADVANCE;

        m_ChildHeights.Clear();
        foreach (var child in prop.VisibleChildren())
        {
          if (m_IsReadOnly && child.name == "m_Value")
            continue;

          float height = EditorGUI.GetPropertyHeight(child, child.isExpanded) + STD_PAD;
          m_ChildHeights.Add(height);
          total += height + STD_PAD;
        }

        return total;
      }

    }

  }

}