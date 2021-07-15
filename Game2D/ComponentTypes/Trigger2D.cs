/**
@file   PyroDK/Game2D/ComponentTypes/Trigger2D.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-21

@brief
  Invokes Collider2D-triggered events; highly Editor-friendly.
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK.Game2D
{

  [AddComponentMenu("PyroDK/Game2D/Collider Trigger (2D)")]
  [RequireComponent(typeof(Collider2D))]
  public sealed class Trigger2D : BaseComponent
  {

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


    #if DEBUG

  [Header("[DebugOnly]")]
    [SerializeField]
    private Color32 m_GizmoColor = Colors.GUI.GizmoTrigger;
    [SerializeField] [ReadOnly]
    private List<Collider2D> m_Triggers = new List<Collider2D>();


    private void OnValidate()
    {
      GetComponentsInChildren(includeInactive: true, m_Triggers);
      m_Triggers.RemoveAll((collider) => !collider.isTrigger);
    }

    private void OnDrawGizmos()
    {
      if (m_Triggers.IsEmpty() || m_GizmoColor.IsClear())
        return;

      bool is_enabled = isActiveAndEnabled;

      foreach (var trig in m_Triggers)
      {
        if (!trig)
          continue;

        if (is_enabled && trig.enabled)
          Gizmos.color = m_GizmoColor;
        else
          Gizmos.color = m_GizmoColor.ToGrayscale().AlphaWash();

        if (trig is CircleCollider2D circ)
        {
          Gizmos.matrix = transform.localToWorldMatrix;
          Gizmos.DrawWireSphere(circ.offset, circ.radius);
        }
        else if (trig is CapsuleCollider2D cap)
        {
          var (c1, c2) = Game3D.CharacterMobility.CapsuleCentersLocal(cap);
          float radius = cap.size[((int)cap.direction).NOT()] / 2f + 0.025f;

          Gizmos.matrix = trig.transform.localToWorldMatrix;
          Gizmos.DrawWireSphere(c1, radius);
          Gizmos.DrawWireSphere(c2, radius);
        }
        else
        {
          var bounds = trig.bounds;
          Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
      }
    }
    #endif // DEBUG

  }

}
