/**
@file   PyroDK/Core/ComponentTypes/Spawnable.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-12

@brief
  Core Component for objects that need additional behaviour
  when they are spawned/despawned by a SpawnPool.
**/

using UnityEngine;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Spawnable")]
  [DisallowMultipleComponent]
  public class Spawnable : BaseComponent, ISpawnable
  {
    public SpawnPool  Owner   => m_RuntimeOwner;
    public GameObject Spawner => m_RuntimeSpawner;


  [Header("Base Spawnable Options")]
    [SerializeField] [SignBitBool]
    protected float m_TimeToLive = -1f;

    [SerializeField] [Space]
    protected DelayedEvent m_OnSpawned    = new DelayedEvent();
    [SerializeField] [Space]
    protected DelayedEvent m_OnDespawned  = new DelayedEvent();


    [System.NonSerialized]
    protected SpawnPool   m_RuntimeOwner;

    [System.NonSerialized]
    protected GameObject  m_RuntimeSpawner;

    [System.NonSerialized]
    protected Coroutine   m_AsyncDespawn;


    public void DespawnNow()
    {
      CancelDespawn();

      if (m_RuntimeOwner)
      {
        m_RuntimeOwner.Despawn(gameObject);
      }
      else
      {
        ((ISpawnable)this).OnDespawn();
        Destroy(gameObject);
      }
    }


    public void DespawnInSeconds(float s)
    {
      if (m_AsyncDespawn == null)
      {
        m_AsyncDespawn = StartCoroutine(InvokeInSeconds(DespawnNow, s));
      }
      else
      {
        "Called DespawnInSeconds(float) with one already ticking; intentional?"
          .LogImportant(this);
      }
    }

    public void CancelDespawn()
    {
      if (m_AsyncDespawn != null)
      {
        StopCoroutine(m_AsyncDespawn);
        m_AsyncDespawn = null;
      }
    }


    public bool TryForceSpawnState(bool force_state)
    {
      if (gameObject.activeSelf == force_state)
        return false;

      gameObject.SetActive(force_state);
      
      if (!m_RuntimeOwner || !m_RuntimeOwner.NotifyForcedSpawn(gameObject))
      {
        if (force_state)
          ((ISpawnable)this).OnSpawned(null);
        else
          ((ISpawnable)this).OnDespawn();
      }
      
      return true;
    }


    protected virtual void Start()
    {
      if (!m_RuntimeOwner)
      {
        ((ISpawnable)this).OnSpawned(null);
      }
    }


    protected virtual void PostSpawn()
    {
    }

    protected virtual void PreDespawn()
    {
    }


    void ISpawnable.OnPooled(SpawnPool pool)
    {
      m_RuntimeOwner = pool;
    }

    void ISpawnable.OnSpawned(GameObject spawner)
    {
      m_RuntimeSpawner = spawner;
      m_OnSpawned.TryInvokeOn(this);

      if (m_TimeToLive > 0.0f)
      {
        DespawnInSeconds(m_TimeToLive);
      }

      PostSpawn();
    }

    void ISpawnable.OnDespawn()
    {
      PreDespawn();
      
      m_OnDespawned.TryInvokeOn(this);
      
      StopAllCoroutines();

      m_AsyncDespawn    = null;
      m_RuntimeSpawner  = null;
    }

  }

}