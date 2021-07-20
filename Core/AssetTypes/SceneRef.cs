/**
@file   PyroDK/Core/AssetTypes/SceneData.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-14

@brief
  Defines the `PyroDK.Core.SceneData` type that stores generic forms of data
  pertaining to a specific `UnityEditor.SceneAsset`, and also provides some
  useful methods to do things with this scene.
**/

#pragma warning disable CS0414 // Unused private members.
#pragma warning disable CS0649 // Unassigned private members.
#pragma warning disable CS0660 // Defining "operator ==", but not "Equals(object)"
#pragma warning disable CS0661 // Defining "operator ==", but not "GetHashCode()"

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


namespace PyroDK
{

  public enum SceneType : int
  {
    None,      // Very greedy. Unloads everything and takes up ALL the space.
    Root,      // Only 1 Root may be, and is typically required to be, active at a time.
    PlaySpace, // Only 1 PlaySpace may be active at a time.
    SubSpace,  // Any number of SubSpaces may be active at a time.

    TestSpace, // Only 1 TestSpace may be active at a time.

    [System.Obsolete(null, false)]
    COUNT
  }


  [System.Serializable]
  public class SceneRefEvent : PyroEvent<SceneRef>
  {
  }


  [CreateAssetMenu(menuName = "PyroDK/SceneRef", order = -120)]
  public sealed class SceneRef : BaseAsset
  {
    #region Static Section

    public static bool Find(string path, out SceneRef result)
    {
      result = null;

      if (path.IsEmpty())
        return false;

      if (s_RtPathLookup.Find(path, out result))
      {
        if (!result)
        {
          Deregister(result, path);
          return false;
        }

        return true;
      }
      #if UNITY_EDITOR
      else
      {
        foreach (var obj in PlayerSettings.GetPreloadedAssets())
        {
          if (obj is SceneRef sref && sref && sref.ScenePath == path)
          {
            result = sref;
            Register(result);
            return true;
          }
        }
      }
      #endif

      return false;
    }

    public static bool Find(Scene scene, out SceneRef sref)
    {
      if (!scene.IsValid())
      {
        sref = null;
        return false;
      }

      return Find(scene.path, out sref);
    }


    public static bool Dummy<T>(string path, SceneRef result)
    {
      return false;
    }


    public static SceneRef GetActive()
    {
      var scene = SceneManager.GetActiveScene();

      if (Find(scene.path, out SceneRef sref))
        return sref;

      #if DEBUG
      if (scene.IsValid())
      {
        $"No SceneRef asset exists for otherwise valid Scene \"{scene.path}\"!"
          .LogWarning();
      }
      #endif

      return null;
    }

    public static IEnumerable<SceneRef> GetAllRegistered()
    {
      var set = new HashSet<SceneRef>();

      foreach (var ( _ , sref ) in s_RtPathLookup)
      {
        if (sref && set.Add(sref))
        {
          //yield return sref;
        }
      }

      return set;

      #if UNITY_EDITOR && false
      foreach (var obj in PlayerSettings.GetPreloadedAssets())
      {
        if (obj is SceneRef sref && set.Add(sref))
        {
          Register(sref);
          yield return sref;
        }
      }
      #endif
    }


    public static bool operator == (SceneRef lhs, Scene rhs) => lhs.Scene == rhs;
    public static bool operator != (SceneRef lhs, Scene rhs) => lhs.Scene != rhs;
    public static bool operator == (Scene lhs, SceneRef rhs) => lhs == rhs.Scene;
    public static bool operator != (Scene lhs, SceneRef rhs) => lhs != rhs.Scene;


    // PRIVATES

    private static readonly HashMap<string, SceneRef> s_RtPathLookup = new HashMap<string, SceneRef>();

    internal static bool Register(SceneRef sref)
    {
      if (!sref || !sref.HasReference)
        return false;

      var tribool = s_RtPathLookup.TryMap(sref.ScenePath, sref, out SceneRef prev);

      if (tribool == TriBool.False)
      {
        if (!prev || !prev.HasReference || !prev.ScenePath.EqualsUnfixed(sref.ScenePath))
        {
          tribool = s_RtPathLookup.Remap(sref.ScenePath, sref);
        }
      }

      return tribool;
    }

    internal static bool Deregister(SceneRef sref, string path = null)
    {
      if (path == null && (object)sref != null) // non null but also a destroyed Unity Object
        path = sref.ScenePath;

      if (!path.IsEmpty())
        return s_RtPathLookup.Unmap(path);
      return false;
    }


    #if UNITY_EDITOR

    static SceneRef()
    {
      EditorBuildSettings.sceneListChanged += BuildListStatusUpdated;
    }

    [InitializeOnLoadMethod]
    private static void BuildListStatusUpdated()
    {
      foreach (var (path, sref) in s_RtPathLookup.GetPairList())
      {
        if (!sref)
        {
          s_RtPathLookup.Remove(path);
          continue;
        }

        int  prev_idx     = sref.m_BuildIndex;
        bool prev_include = sref.m_IncludeInBuild;

        if (sref.HasReference)
        {
          if (sref.ScenePath != path)
          {
            s_RtPathLookup.Remove(path);

            if (sref.ScenePath.IsEmpty())
              continue;
            else
              s_RtPathLookup.Add(sref.ScenePath, sref);
          }

          var tribool = Scenes.IsEnabledInBuildList(sref.ScenePath, out int real_idx, out int build_idx);

          sref.m_IncludeInBuild = tribool;

          if (tribool) // enabled in list
          {
            sref.m_BuildIndex = build_idx;
          }
          else if (tribool == TriBool.Null) // not in list
          {
            sref.m_BuildIndex = int.MinValue;
          }
          else // disabled in list -- flip to mark this as NOT a build index
          {
            sref.m_BuildIndex = ~real_idx;
          }
        }
        else // this shouldn't be registered, so remove it
        {
          sref.m_BuildIndex     = int.MinValue;
          sref.m_IncludeInBuild = false;

          s_RtPathLookup.Remove(path);
        }

        if (sref.m_BuildIndex != prev_idx || sref.m_IncludeInBuild != prev_include)
        {
          EditorUtility.SetDirty(sref);
        }
      }
    }

    #else

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void RegisterPreloaded()
    {
    }

    #endif // UNITY_EDITOR


    #endregion Static Section



    // NON-STATIC SECTION:

    public SceneType Type => m_SceneType;

    public string ReadableName
    {
      get => m_ReadableName.IsEmpty() ? Strings.ExpandCamelCase(name) : m_ReadableName;
      set => m_ReadableName = value ?? string.Empty;
    }

    public string SceneName
    {
      get
      {
        #if UNITY_EDITOR
        if (m_SceneAsset)
          return m_SceneAsset.name;
        #endif

        if (m_Instance.IsValid())
          return m_Instance.name;

        if (m_ScenePath.IsValid)
          return m_ScenePath.GetFilename(include_ext: false);

        return string.Empty;
      }
    }

    public FilePath ScenePath => m_ScenePath;

    public Scene Scene
    {
      get
      {
        #if UNITY_EDITOR
        if (Logging.Assert(m_SceneAsset, "Required field \"Scene Asset\" is unassigned!"))
          return default;
        #endif

        if (!m_Instance.IsValid() &&
            !Scenes.EditorRuntimeScene(m_ScenePath, out m_Instance, m_BuildIndex, peek: true))
        {
          "No valid runtime Scene...".LogError(this);
        }
        
        return m_Instance;
      }
    }

    public int SceneHandle => HasReference ? m_Instance.handle : 0;
    public int BuildIndex  => m_BuildIndex;


    #if UNITY_EDITOR
    public bool HasReference => m_SceneAsset && !m_ScenePath.IsEmpty();
    #else
    public bool HasReference => m_BuildIndex >= 0 && !m_ScenePath.IsEmpty();
    #endif

    public bool IsRegistered => s_RtPathLookup.Find(ScenePath, out SceneRef sref) &&
                                sref == this;

    public bool IsLoaded  => (m_Instance.IsValid() || Scenes.EditorRuntimeScene(m_ScenePath,
                                                                            out m_Instance,
                                                                                m_BuildIndex,
                                                                                peek: false))
                              && m_Instance.isLoaded;
    
    public bool IsPrimaryActive
    {
      get => IsLoaded && SceneManager.GetActiveScene() == m_Instance;
      set
      {
        if (IsLoaded)
        {
          _ = SceneManager.SetActiveScene(Scene);
        }
      }
    }


    public bool ShowLoadingBar => m_ShowLoadingBar;
    public bool ShouldFadeIn   => m_FadeInTime >= 0f;
    public bool ShouldFadeOut  => m_FadeOutTime >= 0f;

    public float FadeInTime  => m_FadeInTime.AtLeast(0f);
    public float FadeOutTime => m_FadeOutTime.AtLeast(0f);


    public DataLookupAsset UserDataLookup
    {
      get => m_DataLookupAsset;
      set => m_DataLookupAsset = value;
    }


    public event UnityAction<SceneRef> OnLoaded
    {
      add     => m_OnLoaded.AddListener(value);
      remove  => m_OnLoaded.RemoveListener(value);
    }

    public event UnityAction<SceneRef> OnAfterUnload
    {
      add     => m_OnAfterUnload.AddListener(value);
      remove  => m_OnAfterUnload.RemoveListener(value);
    }



  [Header("Meta & Reference Data")]
    [SerializeField]
    private SceneType m_SceneType = SceneType.PlaySpace;

    [SerializeField]
    private string m_ReadableName;

    [SerializeField] [RequireInterface("UnityEditor.SceneAsset, UnityEditor", HighlightMissing = true)]
    private Object m_SceneAsset;

    [SerializeField] [ReadOnly]
    private FilePath m_ScenePath = new FilePath();

    [SerializeField] [ReadOnly]
    private int m_BuildIndex = int.MinValue;

    [SerializeField]
    private bool m_IncludeInBuild = false;


  [Header("Scene Type Properties")] [Space(22f)]
    [SerializeField]
    private LocalPhysicsMode m_LocalPhysicsMode = LocalPhysicsMode.None;

    [SerializeField]
    private bool m_ShowLoadingBar = false;

    [SerializeField] [SignBitBool]
    private float m_FadeInTime = -1f;

    [SerializeField] [SignBitBool]
    private float m_FadeOutTime = -1f;

    [SerializeField] [RequireFieldValue("m_SceneType", SceneType.SubSpace)]
    private SceneRef[] m_SubSpaces = new SceneRef[0]; // TODO implement with RootScene loading


  [Header("Event Callback Hooks")] [Space(22f)]
    [SerializeField]
    private SceneRefEvent m_OnLoaded = new SceneRefEvent();
    
    [Space]
    
    [SerializeField]
    private SceneRefEvent m_OnBeforeUnload = new SceneRefEvent();
    
    [Space]

    [SerializeField]
    private SceneRefEvent m_OnAfterUnload = new SceneRefEvent();


  [Header("Additional User Data")] [Space(22f)]
    [SerializeField]
    [Tooltip("An external asset is used here in case the user wishes to more efficiently share data between Scenes.")]
    private DataLookupAsset m_DataLookupAsset;
    
    #if UNITY_EDITOR

    [Space(22f)]

    [SerializeField] [TextArea(minLines: 3, maxLines: 10)]
    private string m_EditorNotes;

    #endif


    [System.NonSerialized]
    private Scene m_Instance; // TODO consider listifying this field


    public int CountLoadedInstances()
    {
      if (!m_ScenePath.RecheckExists())
        return -1;

      int count = 0;

      foreach (var scene in Scenes.GetLoadedScenes())
      {
        if (m_ScenePath.EqualsUnfixed(scene.path))
          ++count;
      }

      return count;
    }


    public void Load() // for easy event callbacks, etc
    {
      if (!HasReference)
      {
        $"{this} is not fully configured to be loaded (no Scene reference)."
          .LogError(this);
      }
      else if (RootScene.IsActive)
      {
        if (RootScene.LoadSpace(this))
          LoadSubSpaces();
      }
      #if UNITY_EDITOR // fallback in Editor
      else if (!EditorApplication.isPlaying)
      {
        if (m_SceneType == SceneType.None)
          m_Instance = EditorSceneManager.OpenScene(m_ScenePath, OpenSceneMode.Single);
        else
          m_Instance = EditorSceneManager.OpenScene(m_ScenePath, OpenSceneMode.Additive);

        if (m_SceneType == SceneType.PlaySpace)
        {
          var prev = GetActive();
          if (prev && prev.Type != SceneType.Root)
            prev.Unload();

          int sanity = 120;
          while (!SceneManager.SetActiveScene(m_Instance) && sanity --> 0)
          {
            System.Threading.Thread.Sleep(500);
          }

          if (sanity == 0)
            Logging.ShouldNotReach();
          else
            LoadSubSpaces();
        }
      }
      #endif
      else
      {
        $"{this} failed to load."
          .LogError(this);
      }
    }

    public void Unload() // likewise as Load();
    {
      if (!IsLoaded)
        return;

      if (RootScene.IsActive)
      {
        _ = RootScene.UnloadSpace(this);
      }
      #if UNITY_EDITOR
      else if (!EditorApplication.isPlaying)
      {
        EditorSceneManager.CloseScene(m_Instance, removeScene: true);
      }
      #endif
      else
      {
        _ = UnloadRoutine();
      }

      UnloadSubSpaces();
    }

    public void LoadSubSpaces()
    {
      if (m_SceneType == SceneType.SubSpace)
        return;

      foreach (var subspace in m_SubSpaces)
      {
        if (subspace && subspace != this) // no infinite recursion today =3
          subspace.Load();
      }
    }

    public void UnloadSubSpaces()
    {
      if (m_SceneType == SceneType.SubSpace)
        return;

      foreach (var subspace in m_SubSpaces)
      {
        if (subspace && subspace != this)
          subspace.Unload();
      }
    }


    public void QuitGameNow()
    {
      RootScene.GlobalQuitGameNow();
    }

    // uses the curren't PlaySpace's settings
    public void QuitGameWithFade()
    {
      if (!RootScene.IsActive)
      {
        RootScene.GlobalQuitGameNow();
        return;
      }

      var curr_scene = RootScene.CurrentSpace(SceneType.PlaySpace);

      if (!curr_scene)
      {
        RootScene.GlobalQuitGameNow();
        return;
      }

      RootScene.Current.QuitGameWithFade(curr_scene.FadeOutTime);
    }


    public void ToggleCanvasGroup(string name)
    {
      var group = CanvasUI.FindGroup(name);
      if (group)
      {
        group.gameObject.SetActive(!group.gameObject.activeSelf);
      }
    }


    public IEnumerator<float> LoadAdditiveRoutine(float min_time)
    {
      if (m_Instance.isLoaded)
      {
        yield return 1f;
        yield break;
      }

      var load_params = new LoadSceneParameters(LoadSceneMode.Additive, m_LocalPhysicsMode);

      AsyncOperation loader;

      if (m_BuildIndex >= 0 && m_BuildIndex < SceneManager.sceneCountInBuildSettings)
      {
        loader = SceneManager.LoadSceneAsync(m_BuildIndex, load_params);
      }
      #if UNITY_EDITOR // can only load Scenes not in the Build List in the editor.
      else if (m_ScenePath.RecheckExists())
      {
        loader = EditorSceneManager.LoadSceneAsyncInPlayMode(m_ScenePath, load_params);
      }
      #endif
      else
      {
        loader = SceneManager.LoadSceneAsync(SceneName, load_params);
      }

      if (loader == null)
      {
        yield return 1f;
        yield break;
      }

      loader.completed += InvokeOnLoaded;

      float pct = 0f;
      if (min_time > 0.5f)
      {
        loader.allowSceneActivation = false;

        min_time = 1f / min_time; // for fast arithmetic

        float time  = Time.unscaledTime;
        float t     = 0f;
        while (t < 1f || loader.progress < 0.89f)
        {
          yield return pct;

          t   = Floats.SmoothStepParameter((Time.unscaledTime - time) * min_time);
          pct = t * loader.progress;
        }
      }

      loader.allowSceneActivation = true;

      while (!loader.isDone)
      {
        yield return Floats.SmoothStepParameter(loader.progress);
      }

      yield return 1f;
    }

    public IEnumerator<float> UnloadRoutine(bool unload_embedded_objs = true)
    {
      if (!IsLoaded)
      {
        yield return 1.0f;
        yield break;
      }

      //InvokeOnBeforeUnload();

      UnloadSceneOptions opts = UnloadSceneOptions.None;
      if (unload_embedded_objs)
        opts = UnloadSceneOptions.UnloadAllEmbeddedSceneObjects;

      AsyncOperation unloader;
      if (m_Instance.IsValid())
        unloader = SceneManager.UnloadSceneAsync(m_Instance, opts);
      else
        unloader = SceneManager.UnloadSceneAsync(SceneName, opts);

      if (unloader == null)
      {
        $"Failed to async unload {SceneName}."
          .LogError(this);
        yield break;
      }

      unloader.completed += InvokeOnAfterUnload;

      while (!unloader.isDone)
      {
        yield return unloader.progress;
      }
    }


    public void InvokeOnLoaded(AsyncOperation _ = null)
    {
      Scene last_scene = default;

      for (int i = SceneManager.sceneCount - 1; i >= 0; --i)
      {
        last_scene = SceneManager.GetSceneAt(i);
        if (m_ScenePath.EqualsUnfixed(last_scene.path))
          break;
      }

      if (!last_scene.IsValid())
      {
        $"Failed to detect last loaded Scene \"{SceneName}\"."
          .LogWarning(this);
        return;
      }

      m_Instance = last_scene;
      Register(this);

      if (m_SceneType == SceneType.PlaySpace ||
          m_SceneType == SceneType.None)
      {
        SceneManager.SetActiveScene(m_Instance);
      }

      m_OnLoaded.TryInvoke(this);
    }

    public void InvokeOnBeforeUnload()
    {
      m_OnBeforeUnload.TryInvoke(this);
    }

    public void InvokeOnAfterUnload(AsyncOperation _ )
    {
      m_OnAfterUnload.TryInvoke(this);
    }


    private void Awake()
    {
      OnValidate();
    }

    private void OnValidate()
    {
      #if UNITY_EDITOR
      Deregister(this);

      if (HasReference)
      {
        if (m_ScenePath.RecheckExists())
        {
          Register(this);

          if (m_IncludeInBuild)
          {
            if (Scenes.AddToBuildList(m_ScenePath, out m_BuildIndex))
              EditorUtility.SetDirty(this);
          }
          else
          {
            int next_idx = Scenes.SetEnabledInBuildList(m_ScenePath, false);
            if (next_idx != m_BuildIndex)
            {
              m_BuildIndex = next_idx;
              EditorUtility.SetDirty(this);
            }
          }
        }
        else
        {
          Logging.TempReached();
          Scenes.RemoveFromBuildList(m_ScenePath);
          m_IncludeInBuild = false;
          m_BuildIndex = int.MinValue;
          EditorUtility.SetDirty(this);
        }
      }

      #else
      Register(this);
      #endif
    }



    #if UNITY_EDITOR

    internal SceneAsset Asset => m_SceneAsset as SceneAsset;

    internal void SetAssetReference(SceneAsset asset, string path)
    {
      if (Application.isPlaying)
        return;

      Deregister(this, m_ScenePath);

      if (!asset)
      {
        m_SceneAsset = null;
        m_ScenePath.Clear();

        m_BuildIndex     = int.MinValue;
        m_IncludeInBuild = false;
      }
      else
      {
        m_SceneAsset = asset;
        m_ScenePath.SetLax(path);

        var tribool = Scenes.IsEnabledInBuildList(path, out int real_idx, out int build_idx);

        m_IncludeInBuild = tribool;

        if (tribool) // in list and enabled
        {
          m_BuildIndex = build_idx;
        }
        else if (tribool == TriBool.Null) // not in list
        {
          m_BuildIndex = int.MinValue;
        }
        else // disabled in list -- flip to mark this as NOT a build index
        {
          m_BuildIndex = ~real_idx;
        }

        Register(this);
      }

      EditorUtility.SetDirty(this);
    }

    #endif // UNITY_EDITOR
  }

}