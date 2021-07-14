/**
@file   PyroDK/Core/Editor/GUIDrawers/TypeNameDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-19

@brief
  A data type that stores a serializable reference to a runtime type.
**/

using System.Collections.Generic;

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{
  using Type = System.Type;
  using MethodInfo = System.Reflection.MethodInfo;

  public static partial class GUIDrawers
  {
    [CustomPropertyDrawer(typeof(SerialDelegate))]
    private sealed class SerialDelegateDrawer : PropertyDrawer
    {
      private readonly struct DelegateChoice
      {
        public readonly SerializedProperty RootProp;
        public readonly MethodInfo Method;
        public readonly Object Target;
        public readonly string Signature;
        public readonly bool IsCurrent;


        public DelegateChoice(SerializedProperty root_prop, MethodInfo method, Object target, bool is_current)
        {
          RootProp  = root_prop;
          Method    = method;
          Target    = target;
          Signature = MakeSignature(method);
          IsCurrent = is_current;
        }


        public void Choose()
        {
          if (IsCurrent)
            return;

          RootProp.FindPropertyRelative("m_Method.m_Declarer").stringValue = Method.DeclaringType.GetQualifiedName();
          RootProp.FindPropertyRelative("m_Method.m_Name").stringValue     = Method.Name;
          RootProp.FindPropertyRelative("m_Target").objectReferenceValue   = Target;

          var prop_params = RootProp.FindPropertyRelative("m_Method.m_ParamTypes");
          var param_infos = Method.GetParameters();

          prop_params.arraySize = param_infos.Length;

          for (int i = 0; i < param_infos.Length; ++i)
          {
            var prop_param = prop_params.GetArrayElementAtIndex(i);
            prop_param.stringValue = param_infos[i].ParameterType.GetQualifiedName();
          }

          RootProp.serializedObject.ApplyModifiedProperties();
        }
      }


      private const float EXTRA_VERTICAL_SPACE = 6f;

      private static readonly GUIContent LABEL_NO_METHOD = new GUIContent("(No Method)");


      private static string MakeSignature(MethodInfo method)
      {
        // heavily modified version of RichText.AppendSignature()

        if (method == null)
          return "<null>";

        var bob = new System.Text.StringBuilder(method.Name).Append('(');

        var parameters = method.GetParameters();
        int i = 0, ilen = parameters.Length;
        while (i < ilen)
        {
          bob.Append(parameters[i].ParameterType.GetRichLogName());

          if (++i < ilen)
            bob.Append(", ");
        }

        bob.Append(')');

        return bob.ToString();
      }

      private static List<DelegateChoice> FindMethodChoices(SerializedProperty prop, Object target, Type target_type, MethodInfo current)
      {
        var choices = new List<DelegateChoice>();

        if (target_type == null)
        {
          if (target)
            target_type = target.GetType();
          else
            return choices;
        }

        foreach (var method in target_type.GetMethods(TypeMembers.PUBLIC))
        {
          if (SerialDelegate.IsValidMethod(method) &&
             ( method.IsStatic || target ))
          {
            choices.Add(new DelegateChoice(prop, method, target, method == current));
          }
        }

        return choices;
      }

      private static GenericMenu BuildMethodPopup(SerializedProperty prop, Object target, Type target_type, MethodInfo current)
      {
        var prop_method = prop.FindPropertyRelative("m_Method.m_Name");

        var menu = new GenericMenu();

        menu.AddItem(LABEL_NO_METHOD, on: current == null, () =>
        {
          prop_method.stringValue = null;
          prop_method.serializedObject.ApplyModifiedProperties();
        });

        var choices = FindMethodChoices(prop, target, target_type, current);

        if (choices.Count > 0)
        {
          int count  = choices.Count;
          var labels = Labels.Borrow(count);

          for (int i = 0; i < count; ++i)
          {
            var label  = labels[i];
            var choice = choices[i];
            label.text = choice.Signature;
            menu.AddItem(label, on: choice.IsCurrent, choice.Choose);
          }

          labels.SafeDispose();
        }
        else
        {
          menu.AddDisabledItem(LABEL_NO_METHOD, true);
        }

        return menu;
      }



      private SerialDelegate m_Delegate;
      private float m_ExtraLines;


      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        if (prop.isExpanded)
          DrawFieldBackground(in total);

        total.yMin += EXTRA_VERTICAL_SPACE / 2f;

        bool expand = FoldoutPrefixLabel(total, out Rect field, label, prop.isExpanded);

        if (prop.isExpanded)
        {
          SerializedProperty prop_target = null;

          label.tooltip = string.Empty;

          var prop_declarer = prop.FindPropertyRelative("m_Method.m_Declarer");
          var prop_static   = prop.FindPropertyRelative("m_Static");

          bool  is_static = prop_static.boolValue;
          float field_beg = field.x;

          field.height = STD_LINE_HEIGHT;

          if (is_static)
          {
            EditorGUI.PropertyField(field, prop_declarer, GUIContent.none);
          }
          else
          {
            prop_target = prop.FindPropertyRelative("m_Target");

            EditorGUI.BeginChangeCheck();
            EditorGUI.ObjectField(field, prop_target, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
              prop_declarer.stringValue = prop_target.objectReferenceValue?.GetType().GetQualifiedName();
            }
          }

          field.y += STD_LINE_ADVANCE;

          field.x = total.x + STD_PAD_RIGHT;
          field.xMax = field_beg - STD_PAD_RIGHT;

          if (is_static)
            label.text = "Switch to Non-Static";
          else
            label.text = "Switch to Static";

          if (GUI.Button(field, label, Styles.ButtonSmall))
          {
            prop_static.boolValue = !is_static;
          }

          field.x = field_beg;
          field.xMax = total.xMax;

          if (m_Delegate.Method.IsMissing)
            label.text = LABEL_NO_METHOD.text;
          else
            label.text = MakeSignature(m_Delegate.Method); // <--

          EditorGUI.BeginDisabledGroup(!Assemblies.FindType(prop_declarer.stringValue, out Type declarer));
          if (GUI.Button(field, label, Styles.Defaults.Popup))
          {
            if (is_static)
            {
              BuildMethodPopup(prop, null, declarer, m_Delegate.Method)
                .DropDown(field);
            }
            else
            {
              BuildMethodPopup(prop, prop_target.objectReferenceValue, null, m_Delegate.Method)
                .DropDown(field);
            }
          }
          EditorGUI.EndDisabledGroup();

          // Additional line(s):

          if (m_ExtraLines > 1f)
          {
            field.y += STD_LINE_ADVANCE;
            //field.xMin = total.x + STD_PAD;

            InvalidField(field, "[placeholder]");
          }
        }

        prop.isExpanded = expand;
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        if (!prop.isExpanded)
          return STD_LINE_HEIGHT + EXTRA_VERTICAL_SPACE;

        Debug.Assert(this.TryGetUnderlyingValue(prop, out m_Delegate));

        if (m_Delegate.IsValid && m_Delegate.Method.ParameterTypes.Length > 0)
          m_ExtraLines = 2f;
        else
          m_ExtraLines = 1f;

        return STD_LINE_HEIGHT +
               STD_LINE_ADVANCE * m_ExtraLines +
               EXTRA_VERTICAL_SPACE;
      }

    } // end class SerialDelegateDrawer

  } // end static partial class GUIDrawers
}