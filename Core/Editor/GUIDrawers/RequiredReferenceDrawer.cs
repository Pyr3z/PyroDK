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
    private static readonly string s_RequiredMetSuffix = RichText.Color("(required)", Colors.Grey.Alpha(0x90));


    [CustomPropertyDrawer(typeof(RequiredReferenceAttribute))]
    private sealed class RequiredReferenceDrawer : PropertyDrawer
    {

      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        if (prop.propertyType != SerializedPropertyType.ObjectReference)
        {
          InvalidField(in total, label, $"{RichText.Attribute("RequiredReference")} requires an Object type!");
          return;
        }

        var attr = (RequiredReferenceAttribute)attribute;

        var highlight = TriBool.True; // ignore if null, disable if false

        if (attr.DisableIfPrefab && AssetObjects.IsInPrefabAsset(prop.serializedObject.targetObject))
        {
          highlight = TriBool.False;
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
                  highlight = TriBool.Null;
                break;
              case SerializedPropertyType.ObjectReference:
                if (!ifprop.objectReferenceValue)
                  highlight = TriBool.Null;
                break;
            }
          }
          else
          {
            $"Could not locate IgnoreIfProperty \"{attr.IgnoreIfProperty}\""
              .LogWarning(this);
          }
        }

        if (highlight)
        {
          var suff = new Rect(total)
          {
            xMax = LabelEndX
          };

          if (!prop.objectReferenceValue)
          {
            DrawRect(total.Expanded(STD_PAD), Colors.Debug.Attention, attr.Highlight);
            GUI.Label(suff, s_RequiredSuffix, Styles.LabelDetail);
          }
          else
          {
            GUI.Label(suff, s_RequiredMetSuffix, Styles.LabelDetail);
          }
        }

        EditorGUI.BeginDisabledGroup(highlight == TriBool.False);
        _ = EditorGUI.PropertyField(total, prop, label);
        EditorGUI.EndDisabledGroup();
      }

    }

  }

}