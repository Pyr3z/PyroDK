/**
@file   PyroDK/Core/BaseTypes/BaseSingleton.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-12

@brief
  Abstract base class for all singleton-patterned GameObject components.
**/

using UnityEngine;
using UnityEngine.Events;


namespace PyroDK
{

  [System.Flags]
  public enum SingletonTraits
  {
    None                  = (0 << 0),
    InstanceIsReplaceable = (1 << 0),
    HideInstanceOnDisable = (1 << 1),
    DontDestroyOnLoad     = (1 << 2),
  }


  [ScriptOrder(-200, inherit: true)]
  public abstract class BaseSingleton<TSelf> : BaseComponent, ISingleton<TSelf>
    where TSelf : BaseSingleton<TSelf> // CRTP (Curiously Recurring Template Pattern)
  {
    public static TSelf      Current    => s_Instance;
    public static bool       IsActive   => s_Instance && s_Instance.isActiveAndEnabled;
    public static GameObject GameObject => s_Instance ? s_Instance.gameObject : null;
    public static SceneRef   Scene      => s_Instance ? s_Instance.m_OwningScene : null;


    protected static TSelf s_Instance;


    // Implements PyroDK.ISingleton:
    public TSelf Instance => s_Instance;
    public bool IsCurrentInstance => s_Instance == this && isActiveAndEnabled;
    public bool IsReplaceableInstance
    {
      get => m_SingletonTraits.HasFlag(SingletonTraits.InstanceIsReplaceable);
      set => m_SingletonTraits = m_SingletonTraits.SetBits(SingletonTraits.InstanceIsReplaceable, value);
    }
    //

  [Header("Singleton GameObject")]
    [SerializeField] [RequiredReference(DisableIfPrefab = true)]
    protected SceneRef m_OwningScene;

    [SerializeField] [ViewAsBools]
    protected SingletonTraits m_SingletonTraits = SingletonTraits.None;

    [SerializeField]
    protected DelayedEvent m_OnAfterInitialized = new DelayedEvent();



    protected virtual void OnEnable()
    {
      if (TryInitialize((TSelf)this))
      {
        if (this is ISceneAware isa)
          isa.RegisterSceneCallbacks();
      }
      else
      {
        $"{TSpy<TSelf>.LogName} failed to initialize."
          .LogError(this);
      }
    }

    protected virtual void OnDisable()
    {
      if (m_SingletonTraits.HasFlag(SingletonTraits.HideInstanceOnDisable) &&
          s_Instance == this)
      {
        if (this is ISceneAware isa)
          isa.DeregisterCallbacks();
        
        s_Instance = null;
      }
    }

    protected virtual void OnDestroy()
    {
      if (s_Instance == this)
      {
        if (this is ISceneAware isa)
          isa.DeregisterCallbacks();

        s_Instance = null;
      }
    }


    protected bool TryInitialize(TSelf self)
    {
      Debug.Assert(this == self);

      if (s_Instance)
      {
        if (s_Instance == self)
        {
          return true;
        }
        else
        {
          if (s_Instance.IsReplaceableInstance)
          {
            Destroy(s_Instance.gameObject);
            // intentional continue
          }
          else
          {
            Destroy(gameObject);
            return false;
          }
        }
      }

      s_Instance = self;

      if (m_SingletonTraits.HasFlag(SingletonTraits.DontDestroyOnLoad))
      {
        m_OwningScene = null;
        DontDestroyOnLoad(gameObject);
      }

      if (m_OnAfterInitialized.IsEnabled)
      {
        System.Func<bool> invoke_condition = () =>
        {
          return s_Instance == self && s_Instance.isActiveAndEnabled;
        };

        UnityAction else_action = () =>
        {
          $"Singleton {TSpy<TSelf>.LogName} lived for less than 1 frame, and thus could not post-initialize."
            .LogWarning(this);
          enabled = false;
        };

        _ = StartCoroutine(InvokeNextFrameIf(m_OnAfterInitialized.Invoke, invoke_condition, else_action));
      }

      return true;
    }


    protected virtual void OnValidate()
    {
      if (!m_SingletonTraits.HasFlag(SingletonTraits.DontDestroyOnLoad))
      {
        if (SceneRef.Find(gameObject.scene, out SceneRef sref))
        {
          m_OwningScene = sref;
        }
        else if (!m_OwningScene && RootScene.Scene)
        {
          m_OwningScene = RootScene.Scene;
        }
      }
      else
      {
        m_OwningScene = null;
      }
    }

  }

}