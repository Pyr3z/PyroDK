/**
@file   PyroDK/Core/AssetTypes/SpawnPool.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-11

@brief
  Implements object-pooling with GameObject Prefab assets!
**/

#pragma warning disable CS0649
#pragma warning disable CS0414

using UnityEngine;


namespace PyroDK
{

  [CreateAssetMenu(menuName = "PyroDK/Spawn Pool", order = -100)]
  public class SpawnPool : BaseAsset, ISceneAware
  {
    // TODO make BaseAssetSingleton!
    public static SpawnPool GlobalFallback => s_GlobalFallback;


    [SerializeStatic]
    private static int DEFAULT_INITIAL_POPULATION = 4;
    [SerializeStatic]
    private static int DEFAULT_MAX_POPULATION     = 32;

    [SerializeStatic]
    private static HideFlags DEFAULT_HIDE_FLAGS = HideFlags.HideInHierarchy;


    private static SpawnPool s_GlobalFallback = null; // TODO make global singleton asset types


    public GameObject Prefab
    {
      get => m_Prefab;
      set
      {
        Close();
        m_Prefab = value;
        OnValidate();
      }
    }

    public bool PoolingEnabled  => m_InitialPopulation >= 0;
    public int  TotalCount      => m_AllOwned.Count;
    public int  AliveCount      => m_Pool?.AliveCount ?? m_AllOwned.Count;
    public int  IdleCount       => m_Pool?.IdleCount  ?? 0;


    [SerializeField]
    private bool m_IsGlobalFallback = false;


  [Header("Pooling")]
    [SerializeField] [SignBitBool(boolean_label: "Pooling Enabled", toggles_value: true)]
    private int m_InitialPopulation = DEFAULT_INITIAL_POPULATION;
    [SerializeField]
    private int m_MaxPopulation     = DEFAULT_MAX_POPULATION;


  [Header("Instantiation Parameters")]
    [SerializeField] [RequiredReference]
    private GameObject  m_Prefab;
    [SerializeField]
    private HideFlags   m_HideFlags = DEFAULT_HIDE_FLAGS;
    [SerializeField]
    private SceneRef    m_PreferredTargetScene;
    [SerializeField]
    private SceneType   m_FallbackSceneType = SceneType.PlaySpace;
    [SerializeField] [Toggleable]
    private Pose        m_SpawnPose = Poses.Disabled;

  [Header("[Info]")]
    [SerializeField] [ReadOnly]
    private bool m_IsCustomSpawnable = false;



    [System.NonSerialized]
    private SceneRef m_OpenScene;

    [System.NonSerialized]
    private bool m_SceneCallbacksRegistered = false;

    [System.NonSerialized]
    private Pose m_RtSpawnPose;

    [System.NonSerialized]
    private GameObject m_CurrentSpawner;

    [System.NonSerialized]
    private ObjectPool<GameObject> m_Pool = null;

    [System.NonSerialized]
    private HashMap<GameObject, ISpawnable> m_AllOwned = new HashMap<GameObject, ISpawnable>();


    private void Awake()
    {
      if (!s_GlobalFallback ||
          (!s_GlobalFallback.m_IsGlobalFallback && m_IsGlobalFallback))
      {
        s_GlobalFallback = this;
      }
    }


    public GameObject Spawn(GameObject spawner)
    {
      if (!Open(force: false))
      {
        $"{TSpy<SpawnPool>.LogName} cannot spawn anything; Missing Prefab?"
          .LogError(this);
        return null;
      }

      m_CurrentSpawner = spawner;

      if (m_Pool == null)
      {
        if (m_IsCustomSpawnable)
        {
          var unpooled = MakeGameObjectCustom();
          OnBorrowCustom(unpooled);
          return unpooled;
        }
        else
        {
          var unpooled = MakeGameObjectBase();
          OnBorrowBase(unpooled);
          return unpooled;
        }
      }

      return m_Pool.Borrow();
    }

    public GameObject Spawn<T>(GameObject spawner, out T component)
    {
      var spawn = Spawn(spawner);
      component = spawn.GetComponent<T>();
      return spawn;
    }

    public GameObject SpawnAt(Vector3 position, GameObject spawner = null)
    {
      var spawn = Spawn(spawner);
      spawn.transform.position = position;
      return spawn;
    }

    public GameObject SpawnAt(Transform position, GameObject spawner = null)
    {
      var spawn = Spawn(spawner);

      if (position)
        spawn.transform.SetParentSpace(position);

      return spawn;
    }


    // for event callbacks:
    public void SetSpawner(GameObject spawner)
    {
      m_CurrentSpawner = spawner;
    }

    public void SetOpenScene(SceneRef sref)
    {
      m_OpenScene = sref;
    }

    public void SetOpenSceneFrom(GameObject obj)
    {
      if (SceneRef.Find(obj.scene, out SceneRef sref))
      {
        m_OpenScene = sref;
      }
      else
      {
        $"Unable to determine GameObject's SceneRef!"
          .LogWarning(obj);
      }
    }

    public void ClearSpawner()
    {
      m_CurrentSpawner = null;
    }

    public void Spawn()
    {
      _ = Spawn(m_CurrentSpawner);
    }

    public void SpawnAt(Transform position)
    {
      var spawn = Spawn(m_CurrentSpawner);

      if (position)
        spawn.transform.SetParentSpace(position);
    }

    public void SpawnOnCollider(Collider other)
    {
      var spawn = Spawn(m_CurrentSpawner);
      
      if (other)
        spawn.transform.SetParentSpace(other.transform);
    }

    public void SpawnOnSpawner(GameObject spawner)
    {
      var spawn = Spawn(spawner);

      if (spawner)
        spawn.transform.SetParentSpace(spawner.transform);
    }


    public bool Despawn(GameObject spawned)
    {
      if (!spawned)
      {
        $"\"{spawned}\" is destroyed; cannot return to {TSpy<SpawnPool>.LogName}."
          .LogError(this);
        return false;
      }

      if (m_Pool == null)
      {
        if (m_AllOwned.Unmap(spawned))
        {
          if (m_IsCustomSpawnable)
            OnReturnCustom(spawned);
          else
            OnReturnBase(spawned);

          Destroy(spawned);
          return true;
        }
      }
      else
      {
        if (m_AllOwned.Contains(spawned))
        {
          m_Pool.Return(spawned);
          return true;
        }
      }

      $"Cannot return \"{spawned}\" to {TSpy<SpawnPool>.LogName}; not owned."
        .LogWarning(this);
      return false;
    }


    public bool Owns(GameObject spawned)
    {
      return spawned && m_AllOwned.Contains(spawned);
    }


    public void Close()
    {
      foreach (var (obj, _ ) in m_AllOwned)
      {
        if (obj)
          Destroy(obj);
      }

      m_AllOwned.Clear();

      m_OpenScene = null;
      m_CurrentSpawner = null;
      m_Pool = null;
    }


    internal bool NotifyForcedSpawn(GameObject forced)
    {
      if (Logging.Assert(forced))
        return false;

      if (m_Pool == null)
      {
        var spawnable = forced.GetComponent<ISpawnable>();
        if (m_AllOwned.Map(forced, spawnable))
        {
          if (m_IsCustomSpawnable)
          {
            if (Logging.Assert(spawnable != null))
              return false;

            spawnable.OnPooled(this);
            OnBorrowCustom(forced);
          }
          else
          {
            OnBorrowBase(forced);
          }

          return true;
        }
      }
      else if (m_AllOwned.Contains(forced))
      {
        return m_Pool.NotifyExternalBorrow(forced);
      }

      return false;
    }


    private bool Open(bool force)
    {
      if (!m_Prefab || !Application.IsPlaying(this))
      {
        Close();
        return false;
      }
      else if (force || ( m_OpenScene && !m_OpenScene.IsLoaded ))
      {
        Close();
      }
      else if (m_Pool != null || m_InitialPopulation < 0)
      {
        return true;
      }

      SelectDefaultOpenScene();

      if (m_SpawnPose.IsEnabled())
        m_RtSpawnPose = m_SpawnPose.GetTransformedBy(m_Prefab.transform);
      else
        m_RtSpawnPose = m_Prefab.GetWorldPose();

      if (m_IsCustomSpawnable)
      {
        m_Pool = new ObjectPool<GameObject>(capacity:   m_MaxPopulation,
                                            start_with: m_InitialPopulation,
                                            make_obj:   MakeGameObjectCustom,
                                            destructor: Destroy,
                                            on_borrow:  OnBorrowCustom,
                                            on_return:  OnReturnCustom);
      }
      else
      {
        m_Pool = new ObjectPool<GameObject>(capacity:   m_MaxPopulation,
                                            start_with: m_InitialPopulation,
                                            make_obj:   MakeGameObjectBase,
                                            destructor: Destroy,
                                            on_borrow:  OnBorrowBase,
                                            on_return:  OnReturnBase);
      }

      if (!m_SceneCallbacksRegistered)
      {
        this.RegisterSceneCallbacks();
        m_SceneCallbacksRegistered = true;
      }

      return true;
    }


    private void SelectDefaultOpenScene()
    {
      if (m_OpenScene && m_OpenScene.IsLoaded)
        return;

      if (m_PreferredTargetScene && m_PreferredTargetScene.IsLoaded)
      {
        m_OpenScene = m_PreferredTargetScene;
      }
      else if (m_FallbackSceneType == 0)
      {
        m_OpenScene = null;
      }
      else
      {
        m_OpenScene = RootScene.CurrentSpace(m_FallbackSceneType);
      }
    }



    private void OnValidate()
    {
      m_IsCustomSpawnable = m_Prefab && ( m_Prefab.GetComponent<ISpawnable>() != null );

      m_MaxPopulation = Mathf.Max(m_MaxPopulation, m_InitialPopulation);

      if (m_IsGlobalFallback && s_GlobalFallback != this)
      {
        if (s_GlobalFallback)
          s_GlobalFallback.m_IsGlobalFallback = false;

        s_GlobalFallback = this;
      }
      else if (!m_IsGlobalFallback && s_GlobalFallback == this)
      {
        s_GlobalFallback = null;
      }
    }


    // callbacks for ObjectPool<GameObject>:
    
    private GameObject MakeGameObjectBase()
    {
      SceneRef prev_scene = SceneRef.GetActive();

      if (m_OpenScene && m_OpenScene != prev_scene)
      {
        m_OpenScene.IsPrimaryActive = true;
      }

      var obj = Instantiate(m_Prefab);

      obj.name = $"{m_Prefab.name}-{TotalCount:D2}";
      obj.hideFlags = m_HideFlags;

      if (!m_OpenScene)
      {
        DontDestroyOnLoad(obj);
      }
      else if (prev_scene && prev_scene != m_OpenScene)
      {
        prev_scene.IsPrimaryActive = true;
      }

      Logging.Assert(m_AllOwned.Map(obj, obj.GetComponent<ISpawnable>()));

      obj.SetActive(false);
      return obj;
    }

    private GameObject MakeGameObjectCustom()
    {
      var obj = MakeGameObjectBase();
      m_AllOwned[obj].OnPooled(this);
      return obj;
    }


    private void OnBorrowBase(GameObject obj)
    {
      obj.SetWorldPose(in m_RtSpawnPose);
      obj.SetActive(true);
    }

    private void OnBorrowCustom(GameObject obj)
    {
      OnBorrowBase(obj);
      m_AllOwned[obj].OnSpawned(m_CurrentSpawner);
    }


    private void OnReturnBase(GameObject obj)
    {
      obj.transform.SetParent(null);
      obj.SetActive(false);
    }

    private void OnReturnCustom(GameObject obj)
    {
      m_AllOwned[obj].OnDespawn();
      OnReturnBase(obj);
    }



    void ISceneAware.OnSceneLoaded(SceneRef sref) { }

    void ISceneAware.OnSceneUnloaded(SceneRef sref)
    {
      if (sref == m_OpenScene)
      {
        Close();
      }
    }

    private void OnDestroy()
    {
      this.DeregisterCallbacks();
    }
  }

}