/**
@file   PyroDK/Game2D/ComponentTypes/Trigger2D.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-21

@brief
  Invokes Collider2D-triggered events; highly Editor-friendly.
**/

using UnityEngine;


namespace PyroDK.Game2D
{

  [AddComponentMenu("PyroDK/Game2D/Collider Trigger (2D)")]
  [RequireComponent(typeof(Collider2D))]
  public sealed class Trigger2D : BaseComponent
  {
    [SerializeStatic]
    private static bool DEFAULT_DRAW_GIZMOS = true;



  [Header("Target Requirements")]
    [SerializeField]
    private LayerMask m_ValidLayers = LayerMasks.Disabled;
    [SerializeField] [GameObjectTag]
    private string    m_ValidTag;


  [Header("Events")]
    [SerializeField]
    private GameObjectEvent m_OnTargetEnter;
    [SerializeField]
    private GameObjectEvent m_OnTargetStay;
    [SerializeField]
    private GameObjectEvent m_OnTargetExit;


  [Header("Triggers")]
    [SerializeField] [RequiredReference]
    private Collider2D m_Trigger;

    #if DEBUG
  [Header("[DebugOnly]")]
    [SerializeField]
    private bool m_DrawGizmos = DEFAULT_DRAW_GIZMOS;
    #endif


    [System.NonSerialized]
    private HashMap<GameObject, Collider2D> m_UniqueTargets = new HashMap<GameObject, Collider2D>(5);


    public bool IsValidTarget(GameObject target)
    {
      return target &&
             (!m_ValidLayers.IsEnabled() || m_ValidLayers.Contains(target)) &&
             (m_ValidTag.IsEmpty() || target.CompareTag(m_ValidTag));
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
      var root = other.attachedRigidbody.gameObject;

      if (IsValidTarget(root) && m_UniqueTargets.Map(root, other))
      {
        m_OnTargetEnter.Invoke(root);
      }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
      var root = other.attachedRigidbody.gameObject;

      if (m_UniqueTargets.Unmap(root))
      {
        m_OnTargetExit.Invoke(root);
      }
    }


    private bool ProcessTargetOnStay(GameObject target)
    {
      // TODO change the check to also include the mapped Collider2D,
      // which may have been deactivated even if its root GameObject is
      // still kicking.

      if (target && target.activeInHierarchy)
      {
        m_OnTargetStay.Invoke(target);
        return false;
      }

      // tell the HashMap to remove this bucket

      m_OnTargetExit.Invoke(target);
      return true;
    }


    private void FixedUpdate()
    {
      if (m_OnTargetStay.IsEnabled)
      {
        m_UniqueTargets.ClearSelective(ProcessTargetOnStay);
      }
    }



    private void OnValidate()
    {
      if (enabled)
      {
        if (m_Trigger)
        {
          if (!m_Trigger.isTrigger)
            m_Trigger = null;
        }
        else
        {
          if (TryGetComponent(out Collider2D coll) && coll.isTrigger)
            m_Trigger = coll;
        }
      }
    }


    #if DEBUG
    private void OnDrawGizmos()
    {
      if (!m_DrawGizmos || !m_Trigger)
        return;

      Gizmos.color  = m_Trigger.enabled ? Colors.Debug.GizmoTrigger : Colors.Debug.GizmoTriggerDisabled;

      if (m_Trigger is CircleCollider2D circ)
      {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(circ.offset, circ.radius);
      }
      else
      {
        var bounds = m_Trigger.bounds;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
      }
    }
    #endif

  }

}
