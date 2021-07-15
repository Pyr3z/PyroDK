/**
@file   PyroDK/Core/Editor/GUIDrawers/RequiredReferenceDrawer.cs
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

  public static partial class GUIDrawers
  {

    private static readonly string s_RequiredSuffix    = RichText.Color("(required)", Colors.Yellow);
    private static readonly string s_RequiredMetSuffix = RichText.Color("(required)", Colors.Gray.Alpha(0x90));


    [CustomPropertyDrawer(typeof(RequiredReferenceAttribute))]
    private sealed class RequiredReferenceDrawer : PropertyDrawer
    {

      private TriBool m_Highlight; // satisfied if false, disable if null


      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        if (prop.propertyType != SerializedPropertyType.ObjectReference)
        {
          InvalidField(in total, label, $"{RichText.Attribute("RequiredReference")} requires an Object type!");
          return;
        }

        var suff = new Rect(total)
        {
          xMax = LabelEndX
        };

        if (m_Highlight)
        {
          var attr = (RequiredReferenceAttribute)attribute;

          var high = new Rect(total.x - STD_INDENT,
                              total.y,
                              total.width + STD_INDENT + 3f,
                              total.height);

          if (high.Contains(Event.current.mousePosition))
            DrawRect(high, Colors.Clear, attr.Highlight.Alpha(0x0C));
          else
            DrawRect(high, Colors.Clear, attr.Highlight);

          label.text = RichText.Bold(label.text);

          GUI.Label(suff, s_RequiredSuffix, Styles.LabelDetail);
        }
        else
        {
          GUI.Label(suff, s_RequiredMetSuffix, Styles.LabelDetail);
        }

        EditorGUI.BeginDisabledGroup(m_Highlight == TriBool.Null);

        total.y += 3f;
        total.height = STD_LINE_HEIGHT;
        _ = EditorGUI.PropertyField(total, prop, label);

        EditorGUI.EndDisabledGroup();
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        CheckConditions(prop);

        return STD_LINE_HEIGHT + 6f;
      }


      private void CheckConditions(SerializedProperty prop)
      {
        var attr = (RequiredReferenceAttribute)attribute;
        m_Highlight = !prop.objectReferenceValue;

        if (attr.DisableIfPrefab && AssetObjects.IsInPrefabAsset(prop.serializedObject.targetObject))
        {
          m_Highlight = TriBool.Null;
        }
        else if (!attr.IgnoreIfProperty.IsEmpty())
        {
          var ifprop = prop.serializedObject.FindProperty(attr.IgnoreIfProperty);

          if (ifprop != null)
          {
            switch (ifprop.propertyType)
            {
              case SerializedPropertyType.Boolean:
                if (!ifprop.boolValue)
                  m_Highlight = TriBool.False;
                break;
              case SerializedPropertyType.ObjectReference:
                if (!ifprop.objectReferenceValue)
                  m_Highlight = TriBool.False;
                break;
            }
          }
          else
          {
            $"Could not locate IgnoreIfProperty \"{attr.IgnoreIfProperty}\""
              .LogWarning(this);
          }
        }
      }


    } // end class RequiredReferenceDrawer

  }

}