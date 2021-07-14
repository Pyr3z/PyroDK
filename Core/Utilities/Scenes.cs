/**
@file   PyroDK/Core/Utilities/Scenes.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-14

@brief
  Provides extensions to the `UnityEngine.SceneManagement` and
  `UnityEditor.SceneManagement` namespaces.
**/

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


namespace PyroDK
{

  public static class Scenes
  {

    public static bool TryLoadSingle(string path_or_name)
    {
      #if UNITY_EDITOR
      if (!Application.isPlaying &&
          EditorSceneManager.OpenScene(path_or_name, OpenSceneMode.Single).IsValid())
      {
        return true;
      }
      #endif

      path_or_name = path_or_name.ParseFileName(keep_extension: false);

      if (path_or_name.IsEmpty())
        return false;

      var parms = new LoadSceneParameters(LoadSceneMode.Single);
      return SceneManager.LoadScene(path_or_name, parms).IsValid();
    }


    public static bool RuntimeSceneFromIndex(int build_idx, out Scene scene)
    {
      if (build_idx >= 0 && build_idx < SceneManager.sceneCountInBuildSettings)
      {
        scene = SceneManager.GetSceneByBuildIndex(build_idx);
        return scene.IsValid();
      }

      scene = default;
      return false;
    }

    public static bool RuntimeSceneFromPath(string asset_path, out Scene scene)
    {
      scene = default;

      if (asset_path.IsEmpty())
        return false;

      if (Application.isPlaying)
      {
        // scene must be loaded for this to actually work
        scene = SceneManager.GetSceneByName(asset_path.ParseFileName(keep_extension: false));

        if (scene.IsValid())
          return true;
      }

      return RuntimeSceneFromIndex(SceneUtility.GetBuildIndexByScenePath(asset_path), out scene);
    }

    public static bool EditorRuntimeScene(string path_or_name, out Scene scene, int build_idx = -1, bool peek = true)
    {
      #if UNITY_EDITOR
      if (build_idx < 0 && build_idx > int.MinValue)
        build_idx = ~build_idx;
      #endif

      if (RuntimeSceneFromIndex(build_idx, out scene))
      {
        return true;
      }

      if (path_or_name.IsEmpty())
      {
        scene = default;
        return false;
      }

      // check for loaded scenes
      scene = SceneManager.GetSceneByName(path_or_name.ParseFileName(keep_extension: false));
      if (scene.IsValid())
        return true;

      #if UNITY_EDITOR
      if (peek && !Application.isPlaying)
      {
        // if in Editor, open the scene without loading it.
        try // I really didn't want to have to use a try-catch block, but no choice.
        {
          scene = EditorSceneManager.OpenScene(path_or_name, OpenSceneMode.AdditiveWithoutLoading);
          return scene.IsValid();
        }
        catch (System.InvalidOperationException) // occurs during assembly reload
        {
        }
      }
      #endif

      return false;
    }


    public static IEnumerable<Scene> GetValidScenes()
    {
      Scene current;
      int   count = SceneManager.sceneCount;

      while (count --> 0)
      {
        current = SceneManager.GetSceneAt(count);

        if (current.IsValid())
        {
          yield return current;
        }
      }
    }

    public static IEnumerable<Scene> GetLoadedScenes()
    {
      Scene current;
      int   count = SceneManager.sceneCount;

      while (count --> 0)
      {
        current = SceneManager.GetSceneAt(count);

        if (current.IsValid() && current.isLoaded)
        {
          yield return current;
        }
      }
    }

    public static IEnumerable<Scene> GetBuiltScenes()
    {
      Scene current;
      int   count = SceneManager.sceneCountInBuildSettings;

      while (count --> 0)
      {
        current = SceneManager.GetSceneByBuildIndex(count);

        if (current.IsValid())
        {
          yield return current;
        }
      }
    }



    #region Editor-Only Functionality
    #if UNITY_EDITOR

    public static bool TryGetSceneAsset(Scene scene, out SceneAsset asset)
    {
      if (scene.IsValid())
      {
        asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
        return asset;
      }

      asset = null;
      return false;
    }


    // BUILD SETTINGS FUNCTIONS:


    public static bool AddToBuildList(string asset_path, out int build_idx)
    {
      TriBool is_enabled = IsEnabledInBuildList(asset_path, out int real_idx, out build_idx);

      if (is_enabled == TriBool.Null) // not already in list
      {
        var appendix = new EditorBuildSettingsScene(asset_path, enabled: true);
        EditorBuildSettings.scenes = EditorBuildSettings.scenes.Append(appendix).ToArray();
        return true;
      }
      else if (is_enabled) // already in list and enabled
      {
        return false;
      }
      else // in list, but not enabled
      {
        var settings = EditorBuildSettings.scenes;
        settings[real_idx].enabled = true;
        EditorBuildSettings.scenes = settings;
        return true;
      }
    }


    public static bool RemoveFromBuildList(string asset_path)
    {
      using (var list = new BufferList<EditorBuildSettingsScene>(EditorBuildSettings.scenes))
      {
        for (int i = 0; i < list.Count; ++i)
        {
          if (list[i].path == asset_path)
          {
            list[i] = null;
          }
        }

        if (list.MakePackedArray(out EditorBuildSettingsScene[] settings) != 0)
        {
          EditorBuildSettings.scenes = settings;
          return true;
        }

        return false;
      }
    }


    public static TriBool IsEnabledInBuildList(string asset_path, out int real_idx, out int build_idx)
    {
      var settings  = EditorBuildSettings.scenes;
      int len_real  = settings.Length;

      real_idx = build_idx = -1;

      while (++real_idx < len_real)
      {
        if (settings[real_idx].enabled)
          ++build_idx;

        if (settings[real_idx].path == asset_path)
          return settings[real_idx].enabled;
      }

      return TriBool.Null;
    }


    public static int SetEnabledInBuildList(string asset_path, bool enabled)
    {
      TriBool was_enabled = IsEnabledInBuildList(asset_path, out int real_idx, out int build_idx);

      if (was_enabled == TriBool.Null)
        return ~real_idx;

      // in list, may or may not be enabled

      if (was_enabled ^ enabled)
      {
        var settings = EditorBuildSettings.scenes;
        
        settings[real_idx].enabled = enabled;
        
        EditorBuildSettings.scenes = settings;
      }

      if (enabled)
        return build_idx;
      else
        return ~real_idx;
    }

    #endif // UNITY_EDITOR
    #endregion // Editor-Only Functionality

  }

}
