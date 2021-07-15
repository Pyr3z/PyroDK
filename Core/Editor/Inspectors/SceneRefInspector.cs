/**
@file   PyroDK/Core/Editor/Inspectors/SceneRefInspector.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-04

@brief
  Overrides the AssetInspector editor behaviour.
**/

using UnityEngine;

using UnityEditor;
using System.Reflection;

namespace PyroDK.Editor
{

  public static partial class Inspectors
  {

    [CustomEditor(typeof(SceneRef), editorForChildClasses: false)]
    [CanEditMultipleObjects]
    private sealed class SceneRefInspector : BaseInspector
    {

      public override void OnInspectorGUI()
      {
        DrawPyroInspector(serializedObject, InjectedReplacementDrawer);
      }


      // returns true always, to tell `DrawPyroInspector()` to not continue drawing past what's here.
      private bool InjectedReplacementDrawer(SerializedProperty curr_prop)
      {
        var sref = target as SceneRef;
        var scenetype = sref.Type;

        if (sref.IsLoaded)
        {
          Labels.Button.text = "Unload from Scene Spaces";
          if (GUILayout.Button(Labels.Button, Styles.ButtonBig))
          {
            sref.Unload();
          }
        }
        else
        {
          Labels.Button.text = RichText.Bold(RichText.Value("Open Scene as ", sref.Type));
          if (GUILayout.Button(Labels.Button, Styles.ButtonBig))
          {
            sref.Load();
          }
        }

        while (curr_prop.NextVisible(enterChildren: false))
        {
          switch (curr_prop.name)
          {
            case "m_SceneAsset":
              EditorGUI.BeginChangeCheck();

              EditorGUILayout.PropertyField(curr_prop);

              if (EditorGUI.EndChangeCheck())
              {
                var asset = curr_prop.objectReferenceValue as SceneAsset;
                sref.SetAssetReference(asset, AssetObjects.GetAssetPath(asset));
              }
              break;

            case "m_BuildIndex":
              {
                string info;

                if (curr_prop.intValue == int.MinValue)
                {
                  info = RichText.False + RichText.Comment("Not listed.");
                }
                else if (curr_prop.intValue >= 0)
                {
                  info = RichText.True + RichText.Comment($"Build index {curr_prop.intValue}");
                }
                else
                {
                  info = RichText.False + RichText.Comment($"Build list position {~curr_prop.intValue}");
                }

                GUIDrawers.InfoFieldLayout(info, Styles.TextInfo, "Enabled in Build List?");
              }
              break;

            case "m_IncludeInBuild":
              EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
              {
                if (curr_prop.boolValue)
                  Labels.Button.text = "Remove from Build List";
                else
                  Labels.Button.text = "Add to Build List";

                if (GUIDrawers.ButtonFieldLayout(Labels.Button, Styles.ButtonSmall))
                {
                  curr_prop.boolValue = !curr_prop.boolValue;
                }
              }
              EditorGUI.EndDisabledGroup();
              break;

            case "m_LocalPhysicsMode":
            case "m_ShowLoadingBar":
              if (scenetype != SceneType.Root)
                goto default;
              break;

            case "m_FadeInTime":
            case "m_FadeOutTime":
              if (scenetype == SceneType.PlaySpace || scenetype == SceneType.None)
                goto default;
              break;

            case "m_SubScenes":
              if (scenetype != SceneType.SubSpace)
                goto default;
              break;

            default:
              _ = EditorGUILayout.PropertyField(curr_prop, includeChildren: true);
              break;

          } // end switch

        } // end while loop

        return true;
      }

    }

  }

}