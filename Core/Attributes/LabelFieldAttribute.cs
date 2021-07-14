/**
@file   PyroDK/Core/Attributes/LabelFieldAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-11

@brief
  Defines the PropertyAttribute [Label(string)], which allows the user to
  provide a custom label for any Inspector-visible (serialized) class fields.
**/

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace PyroDK
{

  [System.Obsolete("Needs migrating")] // TODO
  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class LabelFieldAttribute : PropertyAttribute
  {
    protected readonly string Label, Tooltip;
    
    public LabelFieldAttribute(string label)
    {
      Label = label;
    }

    public LabelFieldAttribute(string label, string tooltip)
    {
      Label = label;
      Tooltip = tooltip;
    }


    #if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(LabelFieldAttribute))]
    private class PropertyDrawer : UnityEditor.PropertyDrawer
    {

      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
        var attr = (LabelFieldAttribute)attribute;

        if (!string.IsNullOrEmpty(attr.Label))
        {
          label.text = attr.Label;
        }

        if (!string.IsNullOrEmpty(attr.Tooltip))
        {
          label.tooltip = attr.Tooltip;
        }

        EditorGUI.PropertyField(position, property, label, true);
      }

    }

    #endif // UNITY_EDITOR

  }

}
