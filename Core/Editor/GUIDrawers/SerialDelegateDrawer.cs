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
      private const float EXTRA_VERTICAL_SPACE = 6f;


      private static string MakeSignature(MethodInfo method, bool rich)
      {
        // heavily stripped version of RichText.AppendSignature()

        if (method == null)
          return "<null>";

        System.Func<Type, string> get_typename;
        if (rich)
          get_typename = Types.GetRichLogName;
        else
          get_typename = Types.GetLogName;

        var bob = new System.Text.StringBuilder(method.Name).Append('(');

        var parameters = method.GetParameters();
        int i = 0, ilen = parameters.Length;
        while (i < ilen && !parameters[i].HasDefaultValue)
        {
          if (i > 0)
            bob.Append(", ");

          bob.Append(get_typename(parameters[i].ParameterType));

          ++i;
        }

        bob.Append(')');

        return bob.ToString();
      }


      private readonly struct MethodChoice
      {
        public readonly SerializedProperty RootProp;
        public readonly MethodInfo Method;
        public readonly Object Target;
        public readonly string Signature;
        public readonly bool IsCurrent;


        public MethodChoice(SerializedProperty root_prop, MethodInfo method, Object target, bool is_current)
        {
          RootProp  = root_prop;
          Method    = method;
          Target    = target;
          Signature = MakeSignature(method, rich: false);
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
      } // end struct MethodChoice


      private static List<MethodChoice> FindMethodChoices(SerializedProperty prop, Object target, Type target_type, MethodInfo current)
      {
        var choices = new List<MethodChoice>();

        bool has_target = !!target;

        if (target_type == null)
        {
          if (has_target)
            target_type = target.GetType();
          else
            return choices;
        }

        foreach (var method in target_type.GetMethods(TypeMembers.PUBLIC))
        {
          if (SerialDelegate.IsValidMethod(method) && ( method.IsStatic || has_target ))
          {
            choices.Add(new MethodChoice(prop, method, target, method == current));
          }
        }

        return choices;
      }

      private static GenericMenu BuildMethodPopup(SerializedProperty prop, Object target, Type target_type, MethodInfo current)
      {
        using (var default_labels = Labels.Borrow("(No Method)"))
        {
          var prop_method = prop.FindPropertyRelative("m_Method.m_Name");

          var menu = new GenericMenu();

          menu.AddItem(default_labels[0], on: current == null, () =>
          {
            prop_method.stringValue = null;
            prop_method.serializedObject.ApplyModifiedProperties();
          });

          var choices = FindMethodChoices(prop, target, target_type, current);

          if (choices.Count > 0)
          {
            var labels = Labels.Borrow(choices.Count);

            for (int i = 0; i < choices.Count; ++i)
            {
              labels[i].text = choices[i].Signature;
              menu.AddItem(labels[i], on: choices[i].IsCurrent, choices[i].Choose);
            }

            labels.SafeDispose();
          }
          else
          {
            menu.AddDisabledItem(default_labels[0], true);
          }

          return menu;
        }
      }



      private SerialDelegate m_Delegate;
      private float m_ExtraLines;


      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        DrawFieldBackground(in total);

        total.y += EXTRA_VERTICAL_SPACE / 2f;
        total.height -= EXTRA_VERTICAL_SPACE;

        bool expand = FoldoutPrefixLabel(total, out Rect field, label, prop.isExpanded);

        if (prop.isExpanded)
        {
          label.tooltip = string.Empty;

          var prop_declarer = prop.FindPropertyRelative("m_Method.m_Declarer");
          var prop_target   = prop.FindPropertyRelative("m_Target");

          bool  is_static = !prop_target.objectReferenceValue;

          field.height = STD_LINE_HEIGHT;

          if (is_static)
          {
            EditorGUI.PropertyField(field, prop_declarer, GUIContent.none);
          }
          else
          {
            EditorGUI.BeginChangeCheck();
            EditorGUI.ObjectField(field, prop_target, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
              prop_declarer.stringValue = prop_target.objectReferenceValue?.GetType().GetQualifiedName();
            }
          }

          field.y += STD_LINE_ADVANCE;

          var left = new Rect(total.x + STD_PAD_RIGHT, field.y, 1f, STD_LINE_HEIGHT)
          {
            xMax = field.x - STD_PAD_RIGHT
          };

          if (is_static)
          {
            label.text = "Switch: Instance";
            if (GUI.Button(left, label, Styles.ButtonSmall))
            {
              prop_declarer.stringValue = (prop_target.objectReferenceValue = prop.serializedObject.targetObject)
                                            .GetType().GetQualifiedName();
            }
          }
          else
          {
            label.text = "Switch: Static";
            if (GUI.Button(left, label, Styles.ButtonSmall))
            {
              prop_target.objectReferenceValue = null;
            }
          }

          if (m_Delegate.Method.IsMissing)
            label.text = "(Missing!)";
          else
            label.text = MakeSignature(m_Delegate.Method, rich: true);

          EditorGUI.BeginDisabledGroup(!Assemblies.FindTypeQuiet(prop_declarer.stringValue, out Type declarer));
          if (GUI.Button(field, label, Styles.Popup))
          {
            BuildMethodPopup(prop, prop_target.objectReferenceValue, declarer, m_Delegate.Method)
              .DropDown(field);
          }
          EditorGUI.EndDisabledGroup();

          // Additional line(s):
          if (m_ExtraLines > 1f)
          {
            EditorGUI.BeginDisabledGroup(!m_Delegate.IsValid);
            left.y += STD_LINE_ADVANCE;
            label.text = "INVOKE";
            if (GUI.Button(left, label, Styles.Button))
            {
              m_Delegate.Invoke(new object[m_Delegate.Method.ParameterCount]);
            }
            EditorGUI.EndDisabledGroup();

            field.y += STD_LINE_ADVANCE;
            DrawRect(field, Colors.Debug.Important);
          }
        }

        prop.isExpanded = expand;
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        if (!prop.isExpanded)
          return STD_LINE_HEIGHT + EXTRA_VERTICAL_SPACE;

        Debug.Assert(this.TryGetUnderlyingValue(prop, out m_Delegate));

        m_ExtraLines = 1f;

        if (m_Delegate.IsValid)
        {
          // TODO this is currently giving falsely good results due to default parameters
          m_ExtraLines += m_Delegate.Method.ParameterCount;
        }

        return STD_LINE_HEIGHT +
               STD_LINE_ADVANCE * m_ExtraLines +
               EXTRA_VERTICAL_SPACE;
      }

    } // end class SerialDelegateDrawer

  } // end static partial class GUIDrawers
}