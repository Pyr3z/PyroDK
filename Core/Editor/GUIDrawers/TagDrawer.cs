/**
@file   PyroDK/Core/Editor/GUIDrawers/TagDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  Restricts a string field to be represented as a GameObject tag
  in the Inspector.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class GUIDrawers
  {

    [CustomPropertyDrawer(typeof(GameObjectTagAttribute))]
    private sealed class TagDrawer : PropertyDrawer
    {

      public override void OnGUI(Rect pos, SerializedProperty sprop, GUIContent label)
      {
        if (sprop.propertyType != SerializedPropertyType.String)
        {
          InvalidField(pos, $"type <{sprop.type}> should be assignable from string or HashedString.", label);
          return;
        }

        EditorGUI.BeginChangeCheck();

        var edit = ToggledTagField(pos, label, sprop.stringValue);

        if (EditorGUI.EndChangeCheck())
        {
          sprop.stringValue = edit;
          sprop.serializedObject.ApplyModifiedProperties();
        }
      }

    }

  }

}