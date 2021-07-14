/**
@file   PyroDK/Core/Editor/AssetObjects.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-04

@brief
  Extensioned functions for `UnityEngine.Object`s which are assets.
**/

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{
  using StringBuilder = System.Text.StringBuilder;
  using DirectoryInfo = System.IO.DirectoryInfo;


  public static class AssetObjects
  {
    #pragma warning disable IDE0051 // unused private members OK - called by Unity w/ reflection

    #if USE_DEPRECATED_ADDRESSES
    [InitializeOnLoadMethod]
    private static void OnEditorLoad()
    {
      PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdate;
    }

    private static void OnPrefabInstanceUpdate(GameObject root)
    {
      var iobjs = root.GetComponentsInChildren<IComponent>();
      foreach (var icomp in iobjs)
      {
        if (!(icomp is Component comp))
        {
          Logging.ShouldNotReach(blame: icomp);
          continue;
        }

        string path = MakeGenericPath(comp);
        if (icomp.UpdateAddress(path))
        {
        }
      }
    }
    #endif // USE_DEPRECATED_ADDRESSES


    [MenuItem("Assets/PyroDK/Delete Sub-Assets", priority = -100)]
    private static void Menu_DeleteSubAssets()
    {
      int removed  = 0;
      var filtered = Selection.GetFiltered<Object>(SelectionMode.Assets | SelectionMode.Editable);

      string selection_string;
      if (filtered.Length == 1)
        selection_string = $"\"{filtered[0].name}\"";
      else
        selection_string = $"selection ({filtered.Length})";

      foreach (var asset in filtered)
      {
        if (IsMainAsset(asset))
          removed += DeleteAllSubAssets(asset, commit_asset: false);
        else if (DeleteSubAsset(asset, commit_asset: false))
          ++removed;
      }

      if (removed < 0)
      {
        $"Error trying to remove sub-assets from {selection_string}."
          .LogError();
      }
      else if (0 < removed)
      {
        $"Removed {removed} sub-assets from {selection_string}."
          .LogSuccess();

        CommitAllChanges(reimport: true);
      }
      else
      {
        $"Didn't find any sub-assets to remove from {selection_string}... potential bug?"
          .LogBoring();
      }
    }


    [MenuItem("Assets/PyroDK/Make SpawnPool Standalone", priority = -80)]
    private static void Menu_MakeSpawnPoolStandalone()
    {
      var pool = CreateAsset<SpawnPool>(file_name_stem: $"[Pool]-{Selection.activeGameObject.name}",
                                        commit_asset:   true,
                                        path:           out _ );
      pool.Prefab = Selection.activeGameObject;
    }

    [MenuItem("Assets/PyroDK/Make SpawnPool Sub-Asset", priority = -80)]
    private static void Menu_MakeSpawnPoolSubAsset()
    {
      var prefab = Selection.activeGameObject;
      int count_existing = CountSubAssetsOfType<SpawnPool>(prefab);

      var pool = CreateSubAsset<SpawnPool>( main_asset:   prefab,
                                            name:                $"{TSpy<SpawnPool>.Name}-{count_existing}",
                                            commit_asset: true);
      pool.Prefab = prefab;
    }

    [MenuItem("Assets/PyroDK/Set Editor Label Font", priority = -60)]
    private static void Menu_SetEditorLabelFont()
    {
      Logging.ShouldNotReach("This menu item is due to be removed.");

      //var fonts = Selection.GetFiltered<Font>(SelectionMode.Assets);
      //if (Logging.Assert(fonts.Length > 0 && fonts[0], "No FontAsset ?!"))
      //  return;

      //var skin = Styles.Defaults.Skin;
      //if (Logging.Assert(skin, "No Styles.Default.Skin ?!"))
      //  return;

      //skin.font = fonts[0];

      //$"Set the default Editor label font (for skin \"{skin.name}\") to: \"{skin.font.name}\""
      //  .LogImportant();
    }


    // validators:

    [MenuItem("Assets/PyroDK/Delete Sub-Assets", isValidateFunction: true)]
    private static bool MenuValidate_DeleteSubAssets()
    {
      var filtered = Selection.GetFiltered<Object>(SelectionMode.Assets | SelectionMode.Editable);
      return filtered.Length > 0 && filtered.Any((obj) => IsSubAsset(obj) || HasSubAssets(obj));
    }

    [MenuItem("Assets/PyroDK/Make SpawnPool Standalone", isValidateFunction: true)]
    [MenuItem("Assets/PyroDK/Make SpawnPool Sub-Asset",  isValidateFunction: true)]
    private static bool MenuValidate_MakeSpawnPool()
    {
      return IsPrefabAssetRoot(Selection.activeGameObject);
    }

    [MenuItem("Assets/PyroDK/Set Editor Label Font", isValidateFunction: true)]
    private static bool MenuValidate_SetEditorLabelFont()
    {
      var fonts = Selection.GetFiltered<Font>(SelectionMode.Assets);
      return fonts.Length > 0 && fonts[0];
    }

    #pragma warning restore IDE0051



    public static bool IsAsset(Object obj)
    {
      return obj && AssetDatabase.Contains(obj);
    }

    public static bool IsMainAsset(Object obj)
    {
      return obj && AssetDatabase.IsMainAsset(obj);
    }

    public static bool IsSubAsset(Object obj)
    {
      return obj && AssetDatabase.IsSubAsset(obj);
    }

    public static bool IsPrefabAssetRoot(GameObject gobj)
    {
      return gobj && PrefabUtility.IsPartOfPrefabAsset(gobj) && !gobj.transform.parent;
    }

    public static bool IsNonPrefabMainAsset(Object obj)
    {
      return IsMainAsset(obj) && !PrefabUtility.IsPartOfAnyPrefab(obj);
    }

    public static bool IsInPrefabAsset(Object obj)
    {
      return obj && PrefabUtility.IsPartOfPrefabAsset(obj);
    }

    public static bool IsInInstantiatedPrefab(Object obj)
    {
      return obj && PrefabUtility.IsPartOfNonAssetPrefabInstance(obj);
    }

    public static bool IsPreloadedAsset(Object asset)
    {
      if (!IsAsset(asset))
        return false;

      return PlayerSettings.GetPreloadedAssets().Contains(asset);
    }


    public static bool TryLoad<T>(string path, out T asset)
      where T : Object
    {
      asset = AssetDatabase.LoadAssetAtPath<T>(path);
      return asset != null;
    }

    public static bool TryLoadWhere<T>(string dir_path, System.Func<T, bool> where, out T asset)
      where T : Object
    {
      asset = null;

      var dir = new DirectoryInfo(dir_path);
      if (!dir.Exists)
        return false;

      foreach (var file in dir.EnumerateFiles("*.*", System.IO.SearchOption.TopDirectoryOnly))
      {
        asset = AssetDatabase.LoadAssetAtPath<T>($"{dir_path}/{file.Name}");
        if (asset != null && ( where?.Invoke(asset) ?? true ))
          return true;
      }

      return false;
    }


    public static string MakeGenericPath(Object obj)
    {
      return AppendGenericPath(new StringBuilder(capacity: 32), obj).ToString();
    }

    public static StringBuilder AppendGenericPath(this StringBuilder strb, Object obj)
    {
      if (Logging.Assert(obj, "obj does not exist!"))
        return strb.Append("null");

      if (AssetDatabase.Contains(obj))
      {
        string path = AssetDatabase.GetAssetPath(obj);
        int cut = path.LastIndexOf('.');

        // begin asset path
        _ = strb.Append('/');

        if (cut < 1)
          _ = strb.Append(path);
        else // cut the unreliable extension
          _ = strb.Append(path, 0, cut);

        if (!(obj is Component) && !(obj is GameObject))
        {
          if (IsMainAsset(obj))
          {
            return strb.Append('<').AppendTypeNamePlain(obj).Append('>');
          }

          return strb.Append(':') // sub asset path
                     .Append(obj.name)
                     .Append('<').AppendTypeNamePlain(obj).Append('>');
        }

        _ = strb.Append('|'); // prefab asset
      }
      //else
      //{
      //  // start with global scope indicator
      //  _ = strb.Append("::");
      //}

      if (obj is Component comp)
      {
        return strb.AppendHierarchyPath(comp.transform, scene_ctx: true).Append(':')
                   .Append('<').AppendTypeNamePlain(comp).Append('>');
      }
      else if (obj is GameObject go)
      {
        return strb.AppendHierarchyPath(go.transform, scene_ctx: true);
      }

      return strb.Append(obj.name)
                 .Append('<').AppendTypeNamePlain(obj).Append('>');
    }


    public static string GetAssetPath(Object asset)
    {
      if (!IsAsset(asset))
        return string.Empty;

      return AssetDatabase.GetAssetPath(asset);
    }


    public static Object GetParentAsset(Object subasset)
    {
      if (!IsSubAsset(subasset))
        return null;

      return AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(subasset));
    }


    public static GameObject GetNearestPrefabAsset(Object prefab_obj)
    {
      if (Logging.Assert(prefab_obj && (TSpy<GameObject>.IsInstance(prefab_obj) ||
                                        TSpy<Component>.IsAssignableFrom(prefab_obj))))
      {
        return null;
      }

      if (PrefabUtility.IsPartOfPrefabInstance(prefab_obj))
      {
        prefab_obj = PrefabUtility.GetCorrespondingObjectFromSource(prefab_obj);

        if (Logging.Assert(prefab_obj, $"No corresponding Prefab found! \"{prefab_obj.name}\""))
          return null;
      }

      if (!(prefab_obj is GameObject gobj))
        gobj = ((Component)prefab_obj).gameObject;

      while (!IsPrefabAssetRoot(gobj))
      {
        if (!gobj || !gobj.transform.parent)
          break;

        // Note: "Original" source means we find a nested Prefab's original asset.
        gobj = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gobj.transform.parent.gameObject);
      }

      return gobj;
    }

    public static GameObject GetFurthestPrefabAsset(Object prefab_obj)
    {
      if (Logging.Assert(prefab_obj && (TSpy<GameObject>.IsInstance(prefab_obj) ||
                                      TSpy<Component>.IsAssignableFrom(prefab_obj))))
      {
        return null;
      }

      if (PrefabUtility.IsPartOfPrefabInstance(prefab_obj))
      {
        prefab_obj = PrefabUtility.GetCorrespondingObjectFromSource(prefab_obj);

        if (Logging.Assert(prefab_obj, $"No corresponding Prefab found! \"{prefab_obj.name}\""))
          return null;
      }

      if (!(prefab_obj is GameObject gobj))
        gobj = ((Component)prefab_obj).gameObject;

      return gobj.transform.root.gameObject;
    }

    public static Object[] GetSubAssets(Object asset)
    {
      if (!IsMainAsset(asset))
        return new Object[0];

      return AssetDatabase.LoadAllAssetRepresentationsAtPath(GetAssetPath(asset));
    }

    public static bool HasSubAssets(Object asset)
    {
      return GetSubAssets(asset).Length > 0;
    }


    public static IEnumerable<T> GetSubAssetsOfType<T>(Object asset)
      where T : class
    {
      var subassets = GetSubAssets(asset);

      int i = subassets.Length;
      while (i --> 0)
      {
        if (subassets[i] is T valid)
          yield return valid;
      }
    }

    public static bool AnySubAssetsOfType<T>(Object asset)
      where T : class // permits interfaces
    {
      var subassets = GetSubAssets(asset);

      int i = subassets.Length;
      while (i --> 0)
      {
        if (subassets[i] is T)
          return true;
      }

      return false;
    }

    public static int CountSubAssets(Object obj)
    {
      return GetSubAssets(obj).Length;
    }

    public static int CountSubAssetsOfType<T>(Object asset)
      where T : class
    {
      int count = 0;

      var subassets = GetSubAssets(asset);

      int i = subassets.Length;
      while (i --> 0)
      {
        if (subassets[i] is T)
          ++count;
      }

      return count;
    }


    public static void CommitAllChanges(bool reimport = true)
    {
      AssetDatabase.SaveAssets();
      
      if (reimport)
        AssetDatabase.Refresh(ImportAssetOptions.Default);
    }

    public static bool CommitChangedAsset(string asset_path)
    {
      if (asset_path.IsEmpty())
        return false;

      AssetDatabase.SaveAssets();
      AssetDatabase.ImportAsset(asset_path, ImportAssetOptions.ForceUpdate);
      return true;
    }

    public static bool CommitChangedAsset(Object asset)
    {
      string path = GetAssetPath(asset);
      if (Logging.Assert(path.Length > 0))
        return false;

      return CommitChangedAsset(path);
    }


    public static T CreateSubAsset<T>(Object main_asset, string name = null, bool commit_asset = true)
      where T : ScriptableObject
    {
      if (!main_asset)
        return null;

      if (!AssetDatabase.IsMainAsset(main_asset))
      {
        main_asset = GetParentAsset(main_asset);
        if (!main_asset)
          return null;
      }

      var instance = ScriptableObject.CreateInstance<T>();

      instance.name = name ?? TSpy<T>.Name;

      AssetDatabase.AddObjectToAsset(instance, main_asset);

      Logging.Assert(!commit_asset || CommitChangedAsset(main_asset));

      return instance;
    }

    public static bool DeleteSubAsset(Object subasset, bool commit_asset)
    {
      if (!IsSubAsset(subasset))
        return false;

      if (commit_asset)
      {
        string path = GetAssetPath(subasset);
        if (Logging.Assert(path.Length > 0))
          return false;

        AssetDatabase.RemoveObjectFromAsset(subasset);
        Object.DestroyImmediate(subasset, allowDestroyingAssets: true);

        return !Logging.Assert(!subasset && CommitChangedAsset(path));
      }
      else
      {
        AssetDatabase.RemoveObjectFromAsset(subasset);
        Object.DestroyImmediate(subasset, allowDestroyingAssets: true);

        return !Logging.Assert(!subasset);
      }
    }

    public static int DeleteAllSubAssets(Object main_asset, bool commit_asset)
    {
      if (!IsMainAsset(main_asset))
        return -1;

      int removed = 0;

      var subassets = GetSubAssets(main_asset);

      int i = subassets.Length;
      while (i --> 0)
      {
        if (DeleteSubAsset(subassets[i], commit_asset: false))
          ++removed;
      }

      Logging.Assert(!commit_asset || removed == 0 || CommitChangedAsset(main_asset));

      return removed;
    }



    public static T CreateAsset<T>(string file_name_stem, bool commit_asset, out string path)
      where T : ScriptableObject
    {
      path = null;

      foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
      {
        // sigh... wish there was a better way.
        path = AssetDatabase.GetAssetPath(obj);

        if (!string.IsNullOrEmpty(path))
          break;
      }

      if (!Filesystem.TryExtractPathHead(path, out path, keep_trailing_slash: false))
      {
        $"Could not create path for \"{file_name_stem}\"; likely, no valid selection."
          .LogError();
        return null;
      }

      return CreateAssetAtPath<T>(path, file_name_stem, commit_asset, out path);
    }

    public static T CreateAssetAtPath<T>(string path_head, string filename, bool commit_asset, out string assetpath)
      where T : ScriptableObject
    {
      if (!Filesystem.IsAssetPath(path_head))
      {
        $"\"{path_head}\" is not a valid Asset path. Defaulting to the root directory.".LogWarning(typeof(AssetObjects));
        path_head = "Assets";
      }
      else
      {
        path_head = Filesystem.TrimPath(path_head);
      }

      // By this point, assume `path` is relative to the Project's directory
      // and leads up to the target directory to place the new asset in.

      if (!string.IsNullOrWhiteSpace(filename))
      {
        assetpath = AssetDatabase.GenerateUniqueAssetPath($"{path_head}/{filename}.asset");
      }
      else
      {
        assetpath = AssetDatabase.GenerateUniqueAssetPath($"{path_head}/{TSpy<T>.Name}.asset");
      }

      if (!Filesystem.TryGuaranteePathFor(assetpath, out _))
      {
        $"Could not create the appropriate directory structure for \"{assetpath}\"".LogError(typeof(AssetObjects));
        return null;
      }

      var instance = ScriptableObject.CreateInstance<T>();

      AssetDatabase.CreateAsset(instance, assetpath);

      Logging.Assert(!commit_asset || CommitChangedAsset(assetpath));

      return instance;
    }


    // Does NOT create Undo states:
    public static void MakeDirtyNoUndo(this Object obj)
    {
      //if (Logging.Assert(IsAsset(obj), "IsAsset(obj) returned false."))
      //  return;
      
      EditorUtility.SetDirty(obj);
    }

    // DOES create Undo states:
    public static void RecordUndo<TObject>(this TObject obj, string why = null)
      where TObject : Object
    {
      if (why.IsEmpty())
      {
        why = Logging.GetCallingMethodName();
      }

      Undo.RecordObject(obj, why);
    }

    public static void RecordBatchUndo<TObject>(this TObject[] objs, string why = null)
      where TObject : Object
    {
      if (objs != null && objs.Length > 0)
      {
        if (why.IsEmpty())
        {
          why = Logging.GetCallingMethodName();
        }

        Undo.RecordObjects(objs, why);
      }
    }


    public static bool TrySetPreloadedAsset(Object asset, bool set)
    {
      if (Logging.Assert(IsMainAsset(asset), $"\"{asset}\" is not a main asset. It will not be preloaded."))
      {
        return false;
      }

      bool changed = set;

      using (var preloaded = new BufferList<Object>(PlayerSettings.GetPreloadedAssets()))
      {
        if (set)
        {
          for (int i = 0; i < preloaded.Count && preloaded[i] != asset; ++i)
          {
            if (preloaded[i] == asset)
              return false;
          }

          preloaded.Add(asset);
        }
        else
        {
          for (int i = 0; i < preloaded.Count; ++i)
          {
            if (preloaded[i] == asset)
            {
              preloaded[i] = null;
              changed = true;
            }
          }
        }

        if (changed)
        {
          PlayerSettings.SetPreloadedAssets(preloaded.ToPackedArray());
        }
      } // end using BufferList<Object>

      return true;
    }

    // yields changed Objects
    public static IEnumerable<Object> SetPreloadedAssets(IEnumerable<Object> assets, bool set)
    {
      var preloaded = PlayerSettings.GetPreloadedAssets();
      var preloaded_set = new HashSet<Object>(preloaded.Where((o) => o));

      int changed = preloaded.Length - preloaded_set.Count;

      if (set)
      {
        foreach (var asset in assets)
        {
          if (asset)
          {
            if (preloaded_set.Add(asset))
            {
              ++changed;
              yield return asset;
            }
          }
        }
      }
      else
      {
        foreach (var asset in assets)
        {
          if (asset)
          {
            if (preloaded_set.Remove(asset))
            {
              ++changed;
              yield return asset;
            }
          }
        }
      }

      if (changed > 0)
      {
        PlayerSettings.SetPreloadedAssets(preloaded_set.ToArray());
      }
    }

  }

}