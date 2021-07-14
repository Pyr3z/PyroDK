/**
@file   PyroDK/Core/Editor/Inspectors/SceneAssetInspector.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-14

@brief
  Provides a custom inspector for SceneAssets that can also store additional
  metadata about Scene.
**/

using System.Linq;

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class Inspectors
  {

    [CustomEditor(typeof(SceneAsset))]
    [CanEditMultipleObjects]
    public class SceneAssetInspector : UnityEditor.Editor
    {
      [SerializeStatic]
      private static string FMT_NEW_SCENEREF = "[Ref]-{0}";


      private bool m_LoadAssetFailed = false;

      private SceneRef m_SceneRef = null;
      private string   m_CurrPath;


      private static bool SceneIsMissingRef(Object scene_asset)
      {
        return !SceneRef.Find(scene_asset.name, out _ );
      }

      private static bool TryMakeSceneRef(Object scene_asset, string path, bool commit_asset, out SceneRef sref)
      {
        var ref_name = string.Format(FMT_NEW_SCENEREF, scene_asset.name);

        if (ref_name.IsEmpty())
        {
          ref_name = "[Ref]-" + scene_asset.name;
        }

        sref = AssetObjects.CreateAsset<SceneRef>(ref_name, commit_asset, out _ );

        if (sref)
        {
          sref.ReadableName = Strings.ExpandCamelCase(scene_asset.name);
          sref.SetAssetReference(scene_asset as SceneAsset, path);
          return true;
        }

        return false;
      }


      public override void OnInspectorGUI()
      {
        bool gui_was_enabled = GUI.enabled;
        GUI.enabled = true;

        m_CurrPath = AssetObjects.GetAssetPath(target);

        if (targets.Length > 1)
        {
          var lonelies = targets.Where(SceneIsMissingRef).ToList();

          GUI.enabled = lonelies.Count > 0;

          if (GUILayout.Button($"Batch Generate Refs ({lonelies.Count})", Styles.ButtonBig))
          {
            int count = 0;
            for (int i = lonelies.Count - 1; i >= 0; --i)
            {
              var lonely = lonelies[i];

              if (TryMakeSceneRef(lonely, AssetObjects.GetAssetPath(lonely),
                                  commit_asset: false, out SceneRef sref))
              {
                if (lonely == target)
                  m_SceneRef = sref;

                ++count;
              }
              else
              {
                $"Unable to generate a SceneRef asset for Scene \"{lonely.name}\"."
                  .LogWarning(this);
              }
            }

            if (count > 0)
            {
              m_LoadAssetFailed = false;
              AssetObjects.CommitAllChanges();
            }
          }

          GUI.enabled = gui_was_enabled;
          return;
        }

        if (m_SceneRef || SceneRef.Find(m_CurrPath, out m_SceneRef))
        {
          if (!m_SceneRef)
          {
            SceneRef.Deregister(m_SceneRef, m_CurrPath);
            m_SceneRef = null;
            GUI.enabled = gui_was_enabled;
            return;
          }

          Labels.Button.text = $"Goto {TSpy<SceneRef>.LogName} Asset";

          if (GUILayout.Button(Labels.Button, Styles.ButtonBig))
          {
            Selection.activeObject = m_SceneRef;
          }
        }
        else if (!m_LoadAssetFailed &&
                  AssetObjects.TryLoadWhere(dir_path: m_CurrPath.ParseParentDirectory(true),
                                            where:    SceneRefIsTarget,
                                            asset:    out m_SceneRef))
        {
          if (!SceneRef.Register(m_SceneRef))
          {
            m_LoadAssetFailed = true;
            m_SceneRef = null;
            GUI.enabled = gui_was_enabled;
            return;
          }
        }
        else
        {
          Labels.Button.text = $"Generate new {TSpy<SceneRef>.LogName} {RichText.String(string.Format(FMT_NEW_SCENEREF, target.name))}";

          if (GUILayout.Button(Labels.Button, Styles.ButtonBig))
          {
            Logging.Assert(TryMakeSceneRef(target, m_CurrPath, commit_asset: true, out m_SceneRef));
          }
        }

        if (m_SceneRef && m_SceneRef.IsLoaded)
        {
          GUI.enabled = false;
        }

        if (m_SceneRef)
          Labels.Button.text = $"Load Scene as a SceneType.{RichText.Value(m_SceneRef.Type)}";
        else
          Labels.Button.text = $"Load Scene as a Ref-less  {RichText.Value("kitchen sink")}";

        if (GUILayout.Button(Labels.Button, Styles.ButtonBig))
        {
          if (m_SceneRef)
          {
            m_SceneRef.Load();
          }
          else if (!Scenes.TryLoadSingle(m_CurrPath))
          {
            $"Failed to load SceneAsset \"{target.name}\".".LogWarning(this);
          }
        }

        GUI.enabled = gui_was_enabled;
      } // end OnInspectorGUI()


      private bool SceneRefIsTarget(SceneRef candidate)
      {
        return candidate && candidate.ScenePath == m_CurrPath;
      }


    } // end class

  }

}
