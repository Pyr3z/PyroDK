/**
@file   PyroDK/Core/ComponentTypes/TransformTracker.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-12

@brief
  Simple Transform tracking, nothing fancy here.
**/

using UnityEngine;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Transform Tracker")]
  [DisallowMultipleComponent]
  public class TransformTracker : BaseComponent
  {
    [System.Flags]
    public enum OptionFlags
    {
      TrackPosition = (1 << 0),
      TrackRotation = (1 << 1),

      [System.Obsolete]
      UseOffset = (1 << 3),

      Update      = (1 << 4),
      FixedUpdate = (1 << 5),
      LateUpdate  = (1 << 6),
    }


    public Transform Target
    {
      get => m_Target;
      set => m_Target = value;
    }


    #if UNITY_EDITOR
    [SerializeField] [ButtonBool(text: "Set Transform to Offset")]
    private bool m_ApplyPoseButton;
    private void OnValidate()
    {
      if (m_ApplyPoseButton)
      {
        SelectCallback()?.Invoke();
        m_ApplyPoseButton = false;
      }
    }
    #endif


    [SerializeField] [RequiredReference]
    private Transform   m_Target;
    [SerializeField] [Toggleable]
    private Pose        m_OffsetPose = Poses.Disabled;
    [SerializeField] [ViewAsBools]
    private OptionFlags m_Options = OptionFlags.TrackPosition | OptionFlags.Update;

    [System.NonSerialized]
    private System.Action m_UpdateCallback, m_FixedUpdateCallback, m_LateUpdateCallback;


    
    public void UpdatePosition()
    {
      if (m_Target && m_OffsetPose.IsEnabled())
      {
        transform.position = m_Target.position + m_OffsetPose.position;
      }
    }

    public void UpdateRotation()
    {
      if (m_Target && m_OffsetPose.IsEnabled())
      {
        transform.rotation = m_Target.rotation * m_OffsetPose.rotation;
      }
    }

    public void UpdateFullPose()
    {
      if (m_Target && m_OffsetPose.IsEnabled())
      {
        transform.position = m_Target.position + m_OffsetPose.position;
        transform.rotation = m_Target.rotation * m_OffsetPose.rotation;
      }
    }


    private void Update()
    {
      m_UpdateCallback?.Invoke();
    }

    private void FixedUpdate()
    {
      m_FixedUpdateCallback?.Invoke();
    }

    private void LateUpdate()
    {
      m_LateUpdateCallback?.Invoke();
    }


    private void OnEnable()
    {
      UpdateCallbacks();
    }


    private void UpdateCallbacks()
    {
      m_UpdateCallback      =
      m_FixedUpdateCallback =
      m_LateUpdateCallback  = null;

      if (m_Options < OptionFlags.Update)
        return;

      if (m_Options.HasFlag(OptionFlags.Update))
        m_UpdateCallback = SelectCallback();

      if (m_Options.HasFlag(OptionFlags.FixedUpdate))
        m_FixedUpdateCallback = SelectCallback();

      if (m_Options.HasFlag(OptionFlags.LateUpdate))
        m_LateUpdateCallback = SelectCallback();
    }

    private System.Action SelectCallback()
    {
      switch ((int)m_Options & 0x03)
      {
        case 0x01:
          return UpdatePosition;
        case 0x02:
          return UpdateRotation;
        case 0x03:
          return UpdateFullPose;
        default:
          return null;
      }
    }

    private void OnDrawGizmosSelected()
    {
      if (!m_Target)
        return;

      if (m_OffsetPose.IsEnabled() && !m_OffsetPose.position.IsZero())
      {
        Gizmos.color = Colors.GUI.GizmoBounds;
        Gizmos.DrawLine(transform.position, m_Target.position);
        Gizmos.DrawRay(m_Target.position, m_OffsetPose.position);
      }
    }

  }

}