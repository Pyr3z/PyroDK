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

    private static readonly string s_DisabledPoseString = $"(This {TSpy<Pose>.LogName} is functionally disabled)";

    public static bool PoseField(Rect total, ref Pose current, GUIContent label, bool toggleable = true)
    {
      bool toggled = current.IsEnabled();
      var  promise = Labels.Pool.MakePromiseIfNull(ref label);
      Rect field;

      EditorGUI.BeginDisabledGroup(Event.current.alt);
      try
      {
        if (toggleable)
        {
          EditorGUI.BeginChangeCheck();

          toggled = ToggledPrefixLabel(total, label, toggled, out field);

          if (EditorGUI.EndChangeCheck())
          {
            current.SetEnabled(toggled);
            return true;
          }

          if (!toggled && GUI.enabled)
          {
            InfoField(field, s_DisabledPoseString, Styles.TextDetailCenter);
            return false;
          }
        }
        else // not "Toggleable"
        {
          field = PrefixLabelStrict(in total, label);

          if (!toggled && GUI.enabled)
          {
            label.text = "Invalid Quaternion. Fix?";
            if (GUI.Button(field, label, Styles.ButtonSmall))
            {
              current.SetEnabled(true);
              return true;
            }

            return false;
          }
        }

        field.height = STD_LINE_HEIGHT;

        PushLabelWidth(Styles.Label.CalcWidth("(Position)"));

        label.text = "(Position)";

        EditorGUI.BeginChangeCheck();
        current.position = EditorGUI.Vector3Field(field, label, current.position);
        if (EditorGUI.EndChangeCheck())
          return true;

        field.y += STD_LINE_ADVANCE;

        label.text = "(Rotation)";

        if (GUI.enabled)
          return RotationField(in field, label, ref current.rotation);
        else
          return RawQuaternionField(in field, label, ref current.rotation);
      }
      finally
      {
        EditorGUI.EndDisabledGroup();
        promise.Dispose();
      }
    }

    public static bool RotationField(in Rect rect, GUIContent label, ref Quaternion rotation)
    {
      EditorGUI.BeginChangeCheck();

      Vector3 eulers = EditorGUI.Vector3Field(rect, label, rotation.ToEuler180().Squeezed());

      if (EditorGUI.EndChangeCheck())
      {
        rotation.SmoothSetEulers(eulers);
        return true;
      }

      return false;
    }

    public static bool RawQuaternionField(in Rect rect, GUIContent label, ref Quaternion quat)
    {
      label.text = RichText.Color(label.text, Colors.Yellow);

      EditorGUI.BeginChangeCheck();

      Vector4 edits = EditorGUI.Vector4Field(rect, label, quat.ToVec4());

      if (EditorGUI.EndChangeCheck())
      {
        quat = new Quaternion(edits.x, edits.y, edits.z, edits.w);
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
        var prop_position = prop.FindPropertyRelative("position");
        var prop_rotation = prop.FindPropertyRelative("rotation");

        m_Current = new Pose(prop_position.vector3Value, prop_rotation.quaternionValue);

        if (PoseField(total, ref m_Current, label, m_Toggleable))
        {
          prop_position.vector3Value    = m_Current.position;
          prop_rotation.quaternionValue = m_Current.rotation;
        }

        prop.isExpanded = m_Toggleable && m_Current.IsEnabled();
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        if (m_Toggleable = fieldInfo.IsDefined<ToggleableAttribute>())
        {
          if (!prop.isExpanded && !Event.current.alt)
          {
            return Styles.TextInfo.CalcFieldHeight(s_DisabledPoseString);
          }
        }

        return STD_LINE_ADVANCE + STD_LINE_HEIGHT;
      }

    }

  }

}