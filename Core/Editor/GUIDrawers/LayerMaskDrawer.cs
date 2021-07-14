/**
@file   PyroDK/Core/Editor/GUIDrawers/LayerMaskDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  Custom LayerMask property drawer.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class GUIDrawers
  {
    [CustomPropertyDrawer(typeof(LayerMask))]
    private sealed class LayerMaskDrawer : PropertyDrawer
    {
      public override void OnGUI(Rect pos, SerializedProperty sprop, GUIContent label)
      {
        EditorGUI.BeginChangeCheck();

        int edit = ToggledLayerMaskField(pos, label, sprop.intValue);

        if (EditorGUI.EndChangeCheck())
        {
          sprop.intValue = edit;
          sprop.serializedObject.ApplyModifiedProperties();
        }
      }

    }

  }

}