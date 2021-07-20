/**
@file   PyroDK/Core/Editor/GUIDrawers/ReadOnlyDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-22

@brief
  PropertyDrawer for fields having the [ReadOnly] attribute.
**/

using System.Reflection;

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{
  using Type = System.Type;


  public static partial class GUIDrawers
  {
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    private sealed class ReadOnlyDrawer : PropertyDrawer
    {
      private const string TOOLTIP_DEFAULT = "[ReadOnly]";


      private GUIStyle        m_StringStyle   = null;
      private PropertyDrawer  m_CustomDrawer  = null;


      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        var attr = (ReadOnlyAttribute)attribute;
        
        if (!attr.Label.IsEmpty())
          label.text = attr.Label;

        if (attr.Tooltip.IsEmpty())
          label.tooltip = TOOLTIP_DEFAULT;
        else
          label.tooltip = attr.Tooltip;

        if (HasCustomDrawer())
        {
          EditorGUI.BeginDisabledGroup(true);
          m_CustomDrawer.OnGUI(total, prop, label);
          EditorGUI.EndDisabledGroup();
        }
        else if (m_StringStyle != null)
        {
          InfoField(total, text:  Labels.Scratch.text,
                           style: m_StringStyle,
                           label: label);
        }
        else // default
        {
          EditorGUI.BeginDisabledGroup(true);
          _ = EditorGUI.PropertyField(total, prop, label, includeChildren: true);
          EditorGUI.EndDisabledGroup();
        }
      }


      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        if (HasCustomDrawer())
          return m_CustomDrawer.GetPropertyHeight(prop, label);

        switch (prop.propertyType)
        {
          case SerializedPropertyType.String:
            Labels.Scratch.text = prop.stringValue;
            break;
          case SerializedPropertyType.Enum:
            Labels.Scratch.text = prop.enumDisplayNames[prop.enumValueIndex];
            m_StringStyle = Styles.NumberInfo;
            break;
          case SerializedPropertyType.Integer:
            Labels.Scratch.text = prop.longValue.ToString();
            m_StringStyle = Styles.NumberInfo;
            return STD_LINE_HEIGHT;
          case SerializedPropertyType.Float:
            Labels.Scratch.text = $"{prop.doubleValue}f";
            m_StringStyle = Styles.NumberInfo;
            return STD_LINE_HEIGHT;
          case SerializedPropertyType.Boolean:
            Labels.Scratch.text = prop.boolValue ? "true" : "false";
            m_StringStyle = Styles.NumberInfo;
            return STD_LINE_HEIGHT;
          default:
            return EditorGUI.GetPropertyHeight(prop.propertyType, label);
        }

        if (Labels.Scratch.text.IsEmpty())
          return STD_LINE_HEIGHT;

        m_StringStyle = Styles.TextInfo;
        
        float height = m_StringStyle.CalcHeight(Labels.Scratch, FieldWidth);

        if (height > STD_LINE_ADVANCE)
          height = Mathf.Ceil(height / STD_LINE_ADVANCE) * STD_LINE_ADVANCE - STD_PAD;
        return height;
      }


      private bool HasCustomDrawer()
      {
        if (m_CustomDrawer != null)
          return m_CustomDrawer != this;

        var attr = (ReadOnlyAttribute)attribute;

        if (attr.CustomDrawerProvided)
        {
          var type = attr.CustomDrawerType;

          if (type == null &&
              !Assemblies.FindSubType("PyroDK.Editor.GUIDrawers+" + attr.CustomDrawerString, typeof(PropertyDrawer), out type) &&
              !Assemblies.FindSubType(attr.CustomDrawerString, typeof(PropertyDrawer), out type))
          {
            $"Could not find type from string \"{attr.CustomDrawerString}\"."
              .LogWarning();
            m_CustomDrawer = this;
            return false;
          }

          if (!type.TryGetInternalField("m_FieldInfo", out FieldInfo finfo))
          {
            $"Could not get FieldInfo for \"{type.Name}.m_FieldInfo\"."
              .LogError();
            m_CustomDrawer = this;
            return false;
          }

          m_CustomDrawer = System.Activator.CreateInstance(type) as PropertyDrawer;

          if (!finfo.TrySetValue(m_CustomDrawer, fieldInfo))
          {
            $"Could not set the fieldInfo property on \"{type.Name}\"."
              .LogError();
            m_CustomDrawer = this;
          }
        }
        else if (fieldInfo.FieldType == TSpy<FilePath>.Type)
        {
          m_CustomDrawer = new PathDrawer();
          
          Debug.Assert(TSpy<PathDrawer>.Type.TryGetInternalField("m_FieldInfo", out FieldInfo finfo));
          finfo.SetValue(m_CustomDrawer, fieldInfo);
        }

        return m_CustomDrawer != null && m_CustomDrawer != this;
      }

    }

  }

}