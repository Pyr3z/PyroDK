/**
@file   PyroDK/Game3D/ComponentTypes/Trigger3D.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  Invokes Collider-triggered events; highly Editor-friendly.
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK.Game3D
{

  [System.Serializable]
  public sealed class ColliderEvent : PyroEvent<Collider>
  {
  }


  [RequireComponent(typeof(Collider))]
  [AddComponentMenu("PyroDK/Game3D/Collider Trigger (3D)")]
  public class Trigger3D : BaseComponent
  {

  [Header("Target Requirements")]
    [SerializeField]
    protected LayerMask m_ValidLayers = LayerMasks.Disabled;
    [SerializeField] [GameObjectTag]
    protected string    m_ValidTag;
    [SerializeField]
    protected bool      m_SeekInactiveTriggers = false;

  [Header("Callback Hooks")]
    [SerializeField]
    protected ColliderEvent m_OnTriggerEnter;
    [Space]
    [SerializeField]
    protected ColliderEvent m_OnTriggerExit;


  [Header("[DebugOnly]")]
    [SerializeField]
    protected Color32 m_GizmoColor = Colors.Debug.GizmoTrigger;
    [SerializeField] [ReadOnly]
    protected List<Collider> m_Triggers = new List<Collider>();


    [System.NonSerialized]
    protected HashSet<GameObject> m_UniqueTargets = new HashSet<GameObject>();


    public bool IsValidTarget(GameObject target)
    {
      return (!m_ValidLayers.IsEnabled() || m_ValidLayers.Contains(target)) &&
             (m_ValidTag.IsEmpty()       || target.CompareTag(m_ValidTag));
    }

    public bool Contains(GameObject obj)
    {
      return m_UniqueTargets.Contains(obj.transform.root.gameObject);
    }



    protected void OnTriggerEnter(Collider other)
    {
      if (TryAddTarget(other))
      {
        m_OnTriggerEnter.TryInvoke(other);
      }
    }

    protected void OnTriggerExit(Collider other)
    {
      if (TryRemoveTarget(other))
      {
        m_OnTriggerExit.TryInvoke(other);
      }
    }


    protected void OnDisable()
    {
      m_UniqueTargets.Clear();
    }


    protected bool TryAddTarget(Collider target)
    {
      var root = target.transform.root.gameObject;
      return isActiveAndEnabled && IsValidTarget(root) && m_UniqueTargets.Add(root);
    }

    protected bool TryRemoveTarget(Collider target)
    {
      var root = target.transform.root.gameObject;
      return m_UniqueTargets.Remove(root) && isActiveAndEnabled;
    }


    protected void FlushDeadTargets()
    {
      if (m_UniqueTargets.Count == 0)
        return;

      m_UniqueTargets.RemoveWhere(ShouldRemoveTarget);
    }

    protected bool ShouldRemoveTarget(GameObject obj)
    {
      return !obj || !IsValidTarget(obj);
    }


    protected void PopulateTriggers()
    {
      GetComponentsInChildren(true, m_Triggers);

      for (int i = m_Triggers.Count - 1; i >= 0; --i)
      {
        if (!m_Triggers[i].isTrigger || (!m_SeekInactiveTriggers && !m_Triggers[i].enabled))
        {
          m_Triggers.RemoveAt(i);
        }
      }
    }



    protected void OnDrawGizmos()
    {
      if (m_GizmoColor.a == 0x00)
        return;

      if (m_Triggers.Count == 0)
      {
        PopulateTriggers();
      }

      bool is_enabled = isActiveAndEnabled;

      foreach (var coll in m_Triggers)
      {
        Gizmos.color = ( is_enabled && coll.enabled ) ? m_GizmoColor : m_GizmoColor.ToGrayscale().AlphaWash();

        if (coll is SphereCollider sphere)
        {
          Gizmos.matrix = coll.transform.localToWorldMatrix;
          Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
        else if (coll is CapsuleCollider cap)
        {
          var (c1, c2) = CharacterMobility.CapsuleCentersLocal(cap);

          Gizmos.matrix = coll.transform.localToWorldMatrix;

          Gizmos.DrawWireSphere(c1, cap.radius + 0.025f);
          Gizmos.DrawWireSphere(c2, cap.radius + 0.025f);
        }
        else
        {
          var bounds = coll.bounds;
          Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
      }
    }

  }

}
