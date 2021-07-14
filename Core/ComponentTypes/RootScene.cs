/**
@file   PyroDK/Core/ComponentTypes/RootScene.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-12

@brief
  Provides hooks into the [ROOT] Scene, if that scheme is used.
**/

#pragma warning disable CS0612 // SceneType.COUNT is obsolete
#pragma warning disable CS0649 // yeah yeah yeah

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Singletons/Root Scene (Core)")]
  public class RootScene : BaseSingleton<RootScene>
  {

    public static bool LoadSpace(SceneRef space)
    {
      if (s_Instance)
      {
        s_Instance.LoadSpaceInstance(space);
        return space;
      }

      return false;
    }

    public static bool UnloadSpace(SceneRef space)
    {
      if (s_Instance)
      {
        s_Instance.UnloadSpaceInstance(space);
        return space;
      }

      return false;
    }

    public static bool ClearSpaces()
    {
      if (s_Instance)
      {
        s_Instance.ClearSpacesInstance();
        return true;
      }

      return false;
    }

    public static SceneRef CurrentSpace(SceneType type)
    {
      if (s_Instance)
      {
        var result = s_Instance.CurrRef(type);
        if (result && result.IsLoaded)
          return result;
      }

      return Scene;
    }

    public static void GlobalQuitGameNow()
    {
      if (Application.isEditor)
      {
        "The Application would have closed now if this were a build!"
          .LogImportant();
      }

      Application.Quit();
    }




  [Header("Optional References")]
    #if UNITY_UI
    [SerializeField]
    private Image m_LoadingFill;
    #endif

    [SerializeField]
    #if !SRP_CORE
    [ReadOnly]
    #endif
    private Game3D.PostProcessInterface m_PostProcessFading;

    #if DEBUG
  [Header("[Debug]")]
    [SerializeField]
    private SceneType m_ShowReadableNameFor = SceneType.PlaySpace;
    #endif


    public void LoadSpaceInitial(SceneRef space)
    {
      var prev = SceneRef.GetActive();
      if (prev && prev.Type == space.Type)
      {
        NextRef(space.Type) = prev;
        prev.InvokeOnLoaded();
        
        if (prev.ShouldFadeIn && m_PostProcessFading.IsValid())
          m_PostProcessFading.FadeIn(prev.FadeInTime);
      }
      else
      {
        LoadSpaceInstance(space);
      }
    }

    public void LoadSpaceInstance(SceneRef space)
    {
      if (!space || !space.HasReference)
      {
        $"SceneRef \"{space}\" was found to be invalid. No-op.".LogWarning(this);
      }
      else if (space.IsLoaded && !CurrRef(space.Type))
      {
        NextRef(space.Type) = space;

        if (space.Type == SceneType.PlaySpace)
          space.IsPrimaryActive = true;

        space.InvokeOnLoaded();
      }
      else
      {
        System.Action<float> do_fillbar = null;
        IEnumerator do_fadein = null, do_fadeout = null;

        if (m_LoadingFill && space.ShowLoadingBar)
          do_fillbar = DoFillBar;

        if (m_PostProcessFading.IsValid())
        {
          if (space.ShouldFadeIn)
            do_fadein = m_PostProcessFading.FadeInAsync(space.FadeInTime);

          if (space.ShouldFadeOut)
            do_fadeout = m_PostProcessFading.FadeOutAsync(space.FadeOutTime);
        }

        _ = StartCoroutine(LoadSpaceAsync(space, do_fadein, do_fadeout, do_fillbar));
      }
    }

    public void UnloadSpaceInstance(SceneRef space)
    {
      if (space && space.IsLoaded)
      {
        IEnumerator do_fadeout = null;

        if (space.Type == SceneType.PlaySpace && space.ShouldFadeOut &&
            m_PostProcessFading.IsValid())
        {
          do_fadeout = m_PostProcessFading.FadeOutAsync(space.FadeOutTime);
        }

        _ = StartCoroutine(UnloadSpaceAsync(space, do_fadeout));
      }
    }

    public void ReloadPlaySpace()
    {
      var space = CurrRef(SceneType.PlaySpace) ?? PrevRef(SceneType.PlaySpace);
      if (!space)
        return;

      _ = StartCoroutine(LoadSpaceAsync(space));
    }

    public void ClearSpacesInstance()
    {
      if (EverLoadedAny(SceneType.TestSpace))
      {
        var test = CurrRef(SceneType.TestSpace);
        if (test && test.IsLoaded)
        {
          _ = StartCoroutine(UnloadSpaceAsync(test));
        }
      }

      if (EverLoadedAny(SceneType.LayerSpace))
      {
        foreach (var layer in LoadedLayers())
        {
          _ = StartCoroutine(UnloadSpaceAsync(layer));
        }
      }

      var play = CurrRef(SceneType.PlaySpace);
      if (!IsClearing(SceneType.PlaySpace) && play && play.IsLoaded)
      {
        IEnumerator do_fadeout = null;

        if (play.ShouldFadeOut && m_PostProcessFading.IsValid())
        {
          do_fadeout = m_PostProcessFading.FadeOutAsync(play.FadeOutTime);
        }

        _ = StartCoroutine(UnloadSpaceAsync(play, do_fadeout));
      }
    }

    public void QuitGameWithFade(float fade_time)
    {
      SetLoading(SceneType.PlaySpace, true);
      SetClearing(SceneType.PlaySpace, true);

      StopAllCoroutines();

      if (fade_time <= 0f || !m_PostProcessFading.IsValid())
      {
        GlobalQuitGameNow();
      }
      else
      {
        _ = StartCoroutine(QuitGameFadeAsync(fade_time));
      }
    }


    // private section

    [System.Flags]
    private enum LoadState : int
    {
      None     = (0 <<  0),
      Loading  = (1 <<  0),
      Clearing = (1 << 31),
    }

    private const int COUNT_TYPES    = (int)SceneType.COUNT;
    private const int SZ_RING_BUFFER = 4;


    [System.NonSerialized]
    private SceneRef[] m_SpaceRefs = new SceneRef[COUNT_TYPES * SZ_RING_BUFFER];

    [System.NonSerialized]
    private uint[] m_SpaceIndices = new uint[COUNT_TYPES];

    [System.NonSerialized]
    private int[] m_LoadStates = new int[COUNT_TYPES];


    private SceneRef PrevRef(SceneType type)
    {
      return m_SpaceRefs[(uint)type + (m_SpaceIndices[(int)type] - 1) % SZ_RING_BUFFER];
    }

    private SceneRef CurrRef(SceneType type)
    {
      return m_SpaceRefs[(uint)type + m_SpaceIndices[(int)type] % SZ_RING_BUFFER];
    }

    private ref SceneRef NextRef(SceneType type)
    {
      return ref m_SpaceRefs[(uint)type + (++m_SpaceIndices[(int)type]) % SZ_RING_BUFFER];
    }

    private bool EverLoadedAny(SceneType type)
    {
      return m_SpaceIndices[(int)type] > 0;
    }

    private IEnumerable<SceneRef> LoadedLayers()
    {
      const uint LAYER_IDX = (uint)SceneType.LayerSpace;

      uint idx = m_SpaceIndices[LAYER_IDX];

      for (int i = 0; i < SZ_RING_BUFFER && idx > 0u; ++i)
      {
        var layer = m_SpaceRefs[LAYER_IDX + idx-- % SZ_RING_BUFFER];
        
        if (layer && layer.IsLoaded)
          yield return layer;
      }
    }

    private bool IsClearing(SceneType type)
    {
      return m_LoadStates[(int)type] < 0;
    }
    private bool SetClearing(SceneType type, bool set)
    {
      if (type == SceneType.LayerSpace)
      {
        if (IsLoading(type))
          return false;
        else if (set)
          --m_LoadStates[(int)SceneType.LayerSpace];
        else if (IsClearing(type))
          ++m_LoadStates[(int)SceneType.LayerSpace];

        return true;
      }
      else if (set == IsClearing(type))
      {
        return false;
      }

      m_LoadStates[(int)type].ToggleFlag(LoadState.Clearing);
      return true;
    }

    private bool IsLoading(SceneType type)
    {
      return m_LoadStates[(int)type] > 0;
    }
    private bool SetLoading(SceneType type, bool set)
    {
      if (type == SceneType.LayerSpace)
      {
        if (IsClearing(type))
          return false;
        else if (set)
          ++m_LoadStates[(int)SceneType.LayerSpace];
        else if (IsLoading(type))
          --m_LoadStates[(int)SceneType.LayerSpace];

        return true;
      }
      else if (set == IsLoading(type))
      {
        return false;
      }

      m_LoadStates[(int)type].ToggleFlag(LoadState.Loading);
      return true;
    }


    private void DoFillBar(float t)
    {
      m_LoadingFill.fillAmount = t;

      if (t < 1f)
        m_LoadingFill.transform.parent.gameObject.SetActive(true);
      else
        m_LoadingFill.transform.parent.gameObject.SetActive(false);
    }



    private IEnumerator LoadSpaceAsync(SceneRef space,  IEnumerator           fadein  = null,
                                                        IEnumerator           fadeout = null,
                                                        System.Action<float>  loadbar = null)
    {
      var type = space.Type;

      while (IsClearing(type))
      {
        yield return new WaitForEndOfFrame();
      }

      if (!SetLoading(type, true))
      {
        $"Failed to start loading {space}: something is already loading."
          .LogWarning(this);
        yield break;
      }

      if (type != SceneType.LayerSpace && CurrRef(type) && CurrRef(type).IsLoaded)
      {
        yield return UnloadSpaceAsync(CurrRef(type),  fadeout: fadeout,
                                                      loadbar: loadbar);
      }

      NextRef(type) = space;

      if (loadbar == null)
      {
        var load_progress = space.LoadAdditiveRoutine(min_time: 0f);
        yield return new WaitWhile(load_progress.MoveNext);
      }
      else
      {
        var load_progress = space.LoadAdditiveRoutine(min_time: 2f);

        while (load_progress.MoveNext())
        {
          loadbar.Invoke(load_progress.Current);
          yield return new WaitForEndOfFrame();
        }
      }

      yield return fadein;

      _ = SetLoading(type, false);
    }

    private IEnumerator UnloadSpaceAsync(SceneRef space,  IEnumerator           fadeout = null,
                                                          System.Action<float>  loadbar = null)
    {
      var type = space.Type;

      if (!SetClearing(type, true))
        yield break;

      space.InvokeOnBeforeUnload();

      yield return fadeout;

      var unload_progress = space.UnloadRoutine(unload_embedded_objs: true);
      
      if (loadbar == null)
      {
        yield return new WaitWhile(unload_progress.MoveNext);
      }
      else
      {
        while (unload_progress.MoveNext())
        {
          loadbar.Invoke(unload_progress.Current);
          yield return new WaitForEndOfFrame();
        }
      }
      
      SetClearing(type, false);
    }

    private IEnumerator QuitGameFadeAsync(float fade_time)
    {
      yield return m_PostProcessFading.FadeOutAsync(fade_time);
      GlobalQuitGameNow();
    }


    protected override void OnEnable()
    {
      if (TryInitialize(this))
      {
        NextRef(SceneType.Root) = m_OwningScene;
      }
      else
      {
        "Failed to load RootScene Singleton."
          .LogError(this);
      }
    }

    protected override void OnDestroy()
    {
      if (s_Instance == this)
      {
        s_Instance = null;
        NextRef(SceneType.Root) = null;
      }
    }



    #if DEBUG
    private void OnGUI()
    {
      if (m_ShowReadableNameFor == SceneType.None)
        return;

      var sref = CurrRef(m_ShowReadableNameFor);

      string text;
      if (sref)
        text = $"{sref.Type}: {sref.ReadableName}";
      else if (m_OwningScene && m_OwningScene.Type == m_ShowReadableNameFor)
        text = m_OwningScene.ReadableName;
      else
        return;

      const float HEIGHT = 22f;
      const float OFFSET = 90f;

      var rect = new Rect(x:      OFFSET,
                          y:      Screen.height - HEIGHT - OFFSET,
                          width:  Screen.width - 2 * OFFSET,
                          height: HEIGHT);

      var scale = 1.5f * Vector2.one;
      var pivot = new Vector2(rect.x + rect.width / 2, rect.y + rect.height);

      GUIUtility.ScaleAroundPivot(scale, pivot);

      GUI.color = Colors.Debug.String;
      GUI.Box(rect, text);
    }
    #endif

  }

}