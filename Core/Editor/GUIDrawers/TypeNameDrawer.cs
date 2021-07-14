/**
@file   PyroDK/Core/Editor/GUIDrawers/TypeNameDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-19

@brief
  A data type that stores a serializable reference to a runtime type.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{
  public static partial class GUIDrawers
  {
    public static class NextTypeNameField
    {
      public static bool HasValidType => Assemblies.FindType(s_CurrName, out _ );

      public static GUIStyle TextStyle;


      private static string s_CurrName;


      public static void Begin(SerializedProperty prop_typename)
      {
        // setting TextStyle to null would be effectively the same,
        // however this is more readable:
        TextStyle = Styles.TextField;
        s_CurrName  = prop_typename.stringValue;
      }

      public static void Apply(SerializedProperty prop_typename)
      {
        prop_typename.stringValue = s_CurrName;
        s_CurrName = null;
      }

      public static void PrepareTextStyle()
      {
        if (HasValidType)
          TextStyle = Styles.TextFieldType;
        else
          TextStyle = Styles.TextFieldBad;
      }

      public static void PrepareTextStyle(GUIStyle valid, GUIStyle invalid)
      {
        if (HasValidType)
          TextStyle = valid;
        else
          TextStyle = invalid;
      }

      public static bool DoTextField(in Rect pos, GUIContent label, bool delayed = true)
      {
        if (delayed)
        {
          return DelayedStringField(in pos, label, ref s_CurrName, TextStyle);
        }
        else
        {
          EditorGUI.BeginChangeCheck();
          s_CurrName = EditorGUI.TextField(pos, label, s_CurrName, TextStyle);
          return EditorGUI.EndChangeCheck();
        }
      }
    }


    [CustomPropertyDrawer(typeof(TypeNameAttribute))]
    [CustomPropertyDrawer(typeof(SerialType))]
    private sealed class TypeNameDrawer : PropertyDrawer
    {
      public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
      {
        if (attribute == null)
        {
          prop = prop.FindPropertyRelative(SerialType.FIELD_TYPE_NAME);
        }

        if (prop.propertyType != SerializedPropertyType.String)
        {
          InvalidField(pos, label, "Only valid for string fields!");
          return;
        }

        NextTypeNameField.Begin(prop);

        NextTypeNameField.PrepareTextStyle();

        if (NextTypeNameField.DoTextField(pos, label, delayed: false))
        {
          NextTypeNameField.Apply(prop);
        }
      }
    } // end class TypeNameDrawer

  }

}