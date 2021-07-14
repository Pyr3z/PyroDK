/**
@file   PyroDK/Core/Editor/GUIDrawers/MinMaxFloatDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-20

@brief
  PropertyDrawer for `PyroDK.MinMaxFloat` fields.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{
  public static partial class GUIDrawers
  {
    [CustomPropertyDrawer(typeof(MinMaxFloat))]
    private sealed class MinMaxFloatDrawer : PropertyDrawer
    {
      private sealed class State : PropertyDrawerState
      {
        public string ValueString => m_MMF.Value.ToString();


        private MinMaxFloat          m_MMF;
        private SerializedProperty   m_ValueProp;
        private SerializedProperty[] m_SubProps;
        private float[]              m_SubValues;
        private int                  m_Index;


        protected sealed override void UpdateDetails()
        {
          m_ValueProp = m_RootProp.FindPropertyRelative("Value");

          m_SubProps = new SerializedProperty[]
          {
            m_RootProp.FindPropertyRelative("Origin"),
            m_RootProp.FindPropertyRelative("Stride")
          };

          m_SubValues = new float[]
          {
            m_SubProps[0].floatValue,
            m_SubProps[0].floatValue + m_SubProps[1].floatValue
          };

          m_MMF = MinMaxFloat.Make(m_ValueProp.floatValue)
                             .WithMinMax(m_SubValues[0], m_SubValues[1]);
          m_Index = -1;
        }

        private void CommitSubValue(int idx)
        {
          if (idx == 0)
          {
            m_MMF.Origin = m_SubProps[0].floatValue = m_SubValues[0];
          }
          else if (idx == 1)
          {
            m_MMF.Max = m_SubValues[1];
            m_SubProps[1].floatValue = m_MMF.Stride;
          }
          else
          {
            Logging.ShouldNotReach();
          }

          m_Index = -1;
        }


        public bool DrawValueLine(in Rect line, GUIContent label)
        {
          // returns true if should keep allowing commits

          EditorGUI.BeginChangeCheck();
          m_MMF.Value = m_MMF.HasMinMax ? EditorGUI.Slider(line, label, m_MMF.Value, m_MMF.Min, m_MMF.Max) :
                                          EditorGUI.FloatField(line, label, m_MMF.Value, Styles.NumberField);
          if (!EditorGUI.EndChangeCheck())
            return true;

          m_MMF.ClampValue();

          if (m_MMF.Equals(m_ValueProp.floatValue))
            return true;

          m_Index = -1;
          m_ValueProp.floatValue = m_MMF.Value;

          return false;
        }

        public void DrawSubLine(in Rect line, bool allow_commit)
        {
          EditorGUI.BeginDisabledGroup(!allow_commit);

          var pos = new Rect(line.x, line.y,
                             EditorGUIUtility.labelWidth,
                             line.height);

          // Styles.Label.CalcWidth("Has Slider") == 59f
          const float HAS_SLIDER_TOGGLE_W = 59f + STD_TOGGLE_W + STD_PAD;

          if (pos.width > HAS_SLIDER_TOGGLE_W)
          {
            pos.x += (pos.width - HAS_SLIDER_TOGGLE_W);
            pos.width = HAS_SLIDER_TOGGLE_W;
          }

          PushIndentLevel(0);

          bool had_range = m_MMF.HasMinMax;
          bool has_range = EditorGUI.ToggleLeft(pos, "Has Slider", had_range);

          PopIndentLevel();

          if (has_range != had_range)
          {
            if (!m_SubValues[0].IsFinite())
            {
              if (m_MMF.Value.IsFinite())
                m_SubValues[0] = m_MMF.Value;
              else
                m_SubValues[0] = 0f;
              CommitSubValue(0);
            }

            if (m_SubValues[1] == 0f)
            {
              float stride;
              if (m_MMF.Value > m_SubValues[0] && m_MMF.Value.IsFinite())
                stride = (m_MMF.Value - m_SubValues[0]) * 2f;
              else
                stride = 100f;

              m_SubValues[1] = m_SubValues[0] + stride;
            }
            else
            {
              m_SubValues[1] = -m_SubValues[1];
            }

            CommitSubValue(1);
          }

          pos.x += pos.width + STD_PAD;
          pos.xMax = line.xMax;

          bool commit = false;

          EditorGUI.BeginDisabledGroup(!has_range);
          using (var extra_labels = Labels.Borrow("min.", "max."))
          {
            m_Index = DelayedMultiFloatField(pos,
                                           extra_labels.Array,
                                           Styles.TextDetailLeft,
                                           m_SubValues,
                                           m_Index,
                                           out commit);
          }
          EditorGUI.EndDisabledGroup();

          if (commit)
          {
            CommitSubValue(m_Index);
          }

          EditorGUI.EndDisabledGroup();
        }

      } // end class State


      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        PropertyDrawerState.Restore(prop, out State state);

        if (prop.IsArrayElement())
        {
          label.text = state.ValueString;
        }

        var line = new Rect(total.x, total.y, total.width, STD_LINE_HEIGHT);

        bool expand = FoldoutPrefix(total, out _ , prop.isExpanded);

        bool allow_commit = state.DrawValueLine(in line, label);

        if (expand && prop.isExpanded)
        {
          line.y += STD_LINE_ADVANCE;
          state.DrawSubLine(line, allow_commit);
        }

        prop.isExpanded = expand;
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        return prop.isExpanded ? (STD_LINE_ADVANCE + STD_LINE_HEIGHT) : STD_LINE_HEIGHT;
      }

    }

  }

}