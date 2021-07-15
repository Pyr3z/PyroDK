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

  [RequireComponent(typeof(Collider))]
  [AddComponentMenu("PyroDK/Game3D/Collider Trigger (3D)")]
  public sealed class Trigger3D : BaseComponent
  {

  [Header("Target Requirements")]
    [SerializeField]
    private LayerMask m_ValidLayers = LayerMasks.Disabled;
    [SerializeField] [GameObjectTag]
    private string m_ValidTag;

  [Header("Callback Hooks")]
    [SerializeField]
    private GameObjectEvent m_OnTriggerEnter;
    [SerializeField]
    private GameObjectEvent m_OnTriggerExit;


    [System.NonSerialized]
    private HashSet<GameObject> m_UniqueTargets = new HashSet<GameObject>();


    public bool IsValidTarget(GameObject root)
    {
      return root && (!m_ValidLayers.IsEnabled() || m_ValidLayers.Contains(root)) &&
                     ( m_ValidTag.IsEmpty()      || root.CompareTag(m_ValidTag));
    }



    private void OnTriggerEnter(Collider other)
    {
      var root = other.transform.root.gameObject;

      if (isActiveAndEnabled && IsValidTarget(root) && m_UniqueTargets.Add(root))
      {
        m_OnTriggerEnter.Invoke(root);
      }
    }

    private void OnTriggerExit(Collider other)
    {
      var root = other.transform.root.gameObject;

      if (m_UniqueTargets.Remove(root) && isActiveAndEnabled)
      {
        m_OnTriggerExit.Invoke(root);
      }
    }

    private void FixedUpdate()
    {
      if (m_UniqueTargets.Count == 0)
        return;

      m_UniqueTargets.RemoveWhere(ShouldRemoveTarget);
    }
    private bool ShouldRemoveTarget(GameObject root)
    {
      if (!root || !IsValidTarget(root))
      {
        m_OnTriggerExit.Invoke(root);
        return true;
      }

      return false;
    }

    private void OnDisable()
    {
      m_UniqueTargets.Clear();
    }


    #if DEBUG

  [Header("[DebugOnly]")]
    [SerializeField]
    private Color32 m_GizmoColor = Colors.GUI.GizmoTrigger;
    [SerializeField] [ReadOnly]
    private List<Collider> m_Triggers = new List<Collider>();


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

        if (trig is SphereCollider sphere)
        {
          Gizmos.matrix = trig.transform.localToWorldMatrix;
          Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
        else if (trig is CapsuleCollider cap)
        {
          var (c1, c2) = CharacterMobility.CapsuleCentersLocal(cap);
          float radius = cap.radius + 0.025f;

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

  } // end class Trigger3D

}
