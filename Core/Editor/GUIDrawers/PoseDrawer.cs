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

    private static readonly string s_DisabledPoseMessage = $"(This {TSpy<Pose>.LogName} is functionally disabled.)";

    private static readonly string s_PositionLabel = RichText.Color("(position)", Colors.GUI.Value);
    private static readonly string s_RotationLabel = RichText.Color("(rotation)", Colors.GUI.Value);

    private static readonly float  s_PositionLabelWidth = Styles.Label.CalcWidth(s_PositionLabel);
    private static readonly float  s_VectorLabelWidth = Styles.Label.CalcWidth("X");

    public static bool PoseField(Rect total, ref Pose current, GUIContent label, bool toggleable)
    {
      using (Labels.Pool.MakePromiseIfNull(ref label))
      {
        if (label.text.IsEmpty())
          label.text = "Pose";

        bool toggled = current.IsEnabled();
        Rect field;

        if (toggleable)
        {
          EditorGUI.BeginChangeCheck();

          toggled = ToggledPrefixLabel(total, label, toggled, out field);

          if (EditorGUI.EndChangeCheck())
          {
            current.SetEnabled(toggled);
            return true;
          }

          if (!toggled)
          {
            label.text = s_DisabledPoseMessage;
            GUI.Label(field, label, Styles.TextInfoSmall);
            return false;
          }
        }
        else // not "Toggleable"
        {
          field = PrefixLabelStrict(in total, label);

          if (!toggled)
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

        bool do_labels =
          field.xMin >= (total.x + Styles.Label.CalcWidth(label) + s_PositionLabelWidth + s_VectorLabelWidth + 6f);

        if (do_labels)
        {
          field.xMin -= s_PositionLabelWidth + STD_PAD_RIGHT + s_VectorLabelWidth;
          label.text = s_PositionLabel;
          PushLabelWidth(s_PositionLabelWidth);
        }
        else
        {
          field.xMin -= s_VectorLabelWidth + STD_PAD;

          var ellipsis = new Rect(field.x - total.height - 2f,
                                  field.y,
                                  total.height - 6f,
                                  total.height);
          label.text = "◄◄";
          label.tooltip = "Make this Inspector window wider—some info may currently be squeezed off screen.";
          GUI.Label(ellipsis, label, Styles.TitleText);

          label.text = label.tooltip = string.Empty;
        }

        field.height = STD_LINE_HEIGHT;

        EditorGUI.BeginChangeCheck();
        current.position = EditorGUI.Vector3Field(field, label, current.position);
        if (EditorGUI.EndChangeCheck())
          return true;

        field.y += STD_LINE_ADVANCE;

        if (do_labels)
        {
          label.text = s_RotationLabel;

          bool changed = RotationField(in field, label, ref current.rotation);

          PopLabelWidth();
          return changed;
        }

        label.text = string.Empty;
        return RotationField(in field, label, ref current.rotation);
      }
    }

    public static bool RotationField(in Rect rect, GUIContent label, ref Quaternion rotation)
    {
      EditorGUI.BeginChangeCheck();

      var eulers = rotation.ToEuler180().Squeezed();
      var edits = EditorGUI.Vector3Field(rect, label, eulers);

      if (EditorGUI.EndChangeCheck() && !edits.Approximately(eulers))
      {
        rotation.SmoothSetEulers(edits);
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
        if ((m_Toggleable = fieldInfo.IsDefined<ToggleableAttribute>()) && !prop.isExpanded)
          return Styles.TextInfoSmall.CalcFieldHeight(s_DisabledPoseMessage).AtLeast(STD_LINE_HEIGHT);

        return STD_LINE_HEIGHT + STD_LINE_ADVANCE;
      }

    }

  }

}