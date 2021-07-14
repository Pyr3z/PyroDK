/**
@file   PyroDK/Core/Editor/GUIDrawers/SceneDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-20

@brief
  PropertyDrawer for `UnityEngine.SceneManagement.Scene` fields.
**/

using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEditor;

namespace PyroDK.Editor
{
  public static partial class GUIDrawers
  {

    public static bool RuntimeSceneFacade(in Rect pos, GUIContent label, SceneAsset asset, ref Scene scene)
    {
      var edit = EditorGUI.ObjectField(pos, label, asset, typeof(SceneAsset), false) as SceneAsset;

      if (edit != asset)
      {
        if (!edit || !Scenes.EditorRuntimeScene(AssetDatabase.GetAssetPath(asset), out scene))
          scene = default;
        return true;
      }

      return false;
    }



    [CustomPropertyDrawer(typeof(Scene))]
    private sealed class SceneDrawer : PropertyDrawer
    {

      public override void OnGUI(Rect position, SerializedProperty sprop, GUIContent label)
      {
        if (!this.TryGetUnderlyingValue(sprop, out Scene scene))
        {
          InvalidField(position, "Could not get underlying field.", label);
          return;
        }

        label.tooltip = $"Current Handle: {scene.handle}";

        Scenes.TryGetSceneAsset(scene, out SceneAsset asset);

        if (RuntimeSceneFacade(position, label, asset, ref scene))
        {
          sprop.FindPropertyRelative("m_Handle").intValue = scene.handle;
          sprop.serializedObject.ApplyModifiedProperties();
        }
      }

    }

  }
}