/**
@file   PyroDK/Core/Editor/GUIDrawers/PoseDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-16

@brief
  Overrides the default drawer for `UnityEngine.Pose` field types.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class GUIDrawers
  {

    private static readonly string s_DisabledPoseString = $"This {TSpy<Pose>.LogName} is disabled.";

    public static bool PoseField(Rect total, ref Pose current, GUIContent label, bool toggleable = false)
    {
      bool enabled = current.IsEnabled();

      Rect field;

      if (toggleable)
      {
        EditorGUI.BeginChangeCheck();

        enabled = ToggledPrefixLabel(total, label, enabled, out field);

        if (EditorGUI.EndChangeCheck())
        {
          current.SetEnabled(enabled);
          return true;
        }

        if (!enabled)
        {
          InfoField(field, s_DisabledPoseString);
          return false;
        }
      }
      else
      {
        if (!enabled)
        {
          current.SetEnabled(true);
          return true;
        }

        field = PrefixLabelStrict(in total, label);
      }

      var promise = Labels.Pool.MakePromiseIfNull(ref label);

      field.height = STD_LINE_HEIGHT;

      PushLabelWidth(Styles.Label.CalcWidth("(Position)"));

      label.text = "(Position)";

      EditorGUI.BeginChangeCheck();
      current.position = EditorGUI.Vector3Field(field, label, current.position);
      if (EditorGUI.EndChangeCheck())
        return true;
      
      field.y += STD_LINE_ADVANCE;

      label.text = "(Rotation)";

      EditorGUI.BeginChangeCheck();
      var eulers = EditorGUI.Vector3Field(field, label, current.rotation.ToEuler180());
      
      PopLabelWidth();
      promise.Dispose();

      if (EditorGUI.EndChangeCheck())
      {
        current.rotation.SmoothSetEulers(in eulers);
        return true;
      }

      return false;
    }


    [CustomPropertyDrawer(typeof(Pose))]
    private sealed class PoseDrawer : PropertyDrawer
    {

      private Pose m_Current;
      private bool m_Toggleable;


      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        if (!this.TryGetUnderlyingValue(prop, out m_Current))
        {
          InvalidField(total, label);
          return;
        }

        if (PoseField(total, ref m_Current, label, m_Toggleable))
        {
          prop.FindPropertyRelative("position").vector3Value     = m_Current.position;
          prop.FindPropertyRelative("rotation").quaternionValue  = m_Current.rotation;
          prop.isExpanded = !m_Toggleable || m_Current.IsEnabled();
        }
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        m_Toggleable = fieldInfo.IsDefined<ToggleableAttribute>();

        if (!prop.isExpanded)
        {
          if (m_Current.IsEnabled())
            return STD_LINE_HEIGHT;

          return Styles.TextInfo.CalcFieldHeight(s_DisabledPoseString);
        }

        return STD_LINE_ADVANCE + STD_LINE_HEIGHT;
      }

    }

  }

}