/**
@file   PyroDK/Core/Editor/GUIDrawers/RequireInterfaceDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-10

@brief
  Draws the PropertyAttribute [RequireInterface(type)], which allows
  serialized `UnityEngine.Object` fields to enforce only a particular
  interface type.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{
  using Type = System.Type;

  public static partial class GUIDrawers
  {

    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    private sealed class RequireInterfaceDrawer : PropertyDrawer
    {

      private bool m_HighlightMissing;


      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        //if (prop.propertyType == SerializedPropertyType.ManagedReference)
        //{
        //  InvalidField(total, "Managed references are not yet supported.", label);
        //  return;
        //}

        if (prop.propertyType != SerializedPropertyType.ObjectReference)
        {
          InvalidField(total, $"Unsupported field type: {prop.type}", label);
          return;
        }

        if (!TryGetInterfaceType(out Type iface))
        {
          var param = RichText.String(((RequireInterfaceAttribute)attribute).DeferredInterface);
          InvalidField(total, RichText.Attribute("EnforceInterface", param), label);
          return;
        }

        bool scene_objects = prop.serializedObject.targetObject is Component;

        if (m_HighlightMissing)
        {
          var suff = new Rect(total)
          {
            xMax = LabelEndX
          };

          if (!prop.objectReferenceValue)
          {
            DrawRect(total.Expanded(STD_PAD), Colors.Debug.Attention,
                                              Colors.Debug.Attention.Alpha(0.375f));
            GUI.Label(suff, s_RequiredSuffix, Styles.LabelDetail);
          }
          else
          {
            GUI.Label(suff, s_RequiredMetSuffix, Styles.LabelDetail);
          }
        }

        EditorGUI.BeginChangeCheck();

        var edit = EditorGUI.ObjectField(total, label, prop.objectReferenceValue, iface, scene_objects);

        if (!EditorGUI.EndChangeCheck())
          return;

        if (!edit)
        {
          prop.objectReferenceValue = null;
          prop.serializedObject.ApplyModifiedProperties();
        }
        else if (iface.IsAssignableFrom(edit.GetType()))
        {
          prop.objectReferenceValue = edit;
          prop.serializedObject.ApplyModifiedProperties();
        }
        else if (edit is GameObject obj && obj.TryGetComponent(iface, out Component comp))
        {
          prop.objectReferenceValue = comp;
          prop.serializedObject.ApplyModifiedProperties();
        }
      }


      private bool TryGetInterfaceType(out Type type)
      {
        var attr = (RequireInterfaceAttribute)attribute;

        m_HighlightMissing = attr.HighlightMissing;

        type = attr.Interface;
        
        if (type == null)
        {
          if (Assemblies.FindType(attr.DeferredInterface, out type))
          {
            attr.Interface = type;
          }
          else
          {
            // set to skip the erroneous parsing until editor reload:
            attr.Interface = type = Types.Missing;
            return false;
          }
        }

        return type != Types.Missing;
      }

    }

  }

}