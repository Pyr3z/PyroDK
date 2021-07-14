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
      Disabled    = (0 << 0),
      Position    = (1 << 0),
      Rotation    = (1 << 1),

      UseOffset   = (1 << 3),

      Update      = (1 << 4),
      FixedUpdate = (1 << 5),
      LateUpdate  = (1 << 6),
    }


    public Transform Target
    {
      get => m_Target;
      set => m_Target = value;
    }


    [SerializeField] [RequiredReference(color_hex: "#EEA02011")]
    private Transform   m_Target;
    [SerializeField]
    private OptionFlags m_OptionFlags = OptionFlags.Position | OptionFlags.Update;
    [SerializeField]
    private Pose        m_OffsetPose = Poses.Disabled;


    [System.NonSerialized]
    private System.Action m_UpdateCallback, m_FixedUpdateCallback, m_LateUpdateCallback;


    
    public void UpdatePosition()
    {
      if (m_Target)
      {
        transform.position = m_Target.position + m_OffsetPose.position;
      }
    }

    public void UpdateRotation()
    {
      if (m_Target)
      {
        transform.rotation = m_Target.rotation * m_OffsetPose.rotation;
      }
    }

    public void UpdateFullPose()
    {
      if (m_Target)
      {
        transform.position = m_Target.position + m_OffsetPose.position;
        transform.rotation = m_Target.rotation * m_OffsetPose.rotation;
      }
    }


    public void UpdateCallbacks()
    {
      if (m_OptionFlags < OptionFlags.Update)
      {
        m_UpdateCallback      = NoOperation;
        m_FixedUpdateCallback = NoOperation;
        m_LateUpdateCallback  = NoOperation;
        return;
      }

      if (m_OptionFlags.HasFlag(OptionFlags.Update))
      {
        AssignCallback(ref m_UpdateCallback);
      }
      else
      {
        m_UpdateCallback = NoOperation;
      }

      if (m_OptionFlags.HasFlag(OptionFlags.FixedUpdate))
      {
        AssignCallback(ref m_FixedUpdateCallback);
      }
      else
      {
        m_FixedUpdateCallback = NoOperation;
      }

      if (m_OptionFlags.HasFlag(OptionFlags.LateUpdate))
      {
        AssignCallback(ref m_LateUpdateCallback);
      }
      else
      {
        m_LateUpdateCallback = NoOperation;
      }
    }


    private static void NoOperation()
    {
    }


    private void Update()
    {
      m_UpdateCallback();
    }

    private void FixedUpdate()
    {
      m_FixedUpdateCallback();
    }

    private void LateUpdate()
    {
      m_LateUpdateCallback();
    }


    private void Awake()
    {
      UpdateCallbacks();
    }


    private void AssignCallback(ref System.Action callback)
    {
      switch ((int)m_OptionFlags & 0x03)
      {
        case 0x01:
          callback = UpdatePosition;
          break;
        case 0x02:
          callback = UpdateRotation;
          break;
        case 0x03:
          callback = UpdateFullPose;
          break;
        default:
          callback = NoOperation;
          break;
      }
    }

    private void OnValidate()
    {
      if (!m_OptionFlags.HasFlag(OptionFlags.UseOffset))
      {
        if (m_Target)
        {
          if (m_OptionFlags.HasFlag(OptionFlags.Position))
            transform.position = m_Target.position;
          if (m_OptionFlags.HasFlag(OptionFlags.Rotation))
            transform.rotation = m_Target.rotation;
        }

        m_OffsetPose = Poses.Enabled;
      }
      else if (!m_OffsetPose.IsEnabled())
      {
        if (Application.IsPlaying(this))
        {
          m_OffsetPose.rotation.Normalize();
          return;
        }

        if (m_Target)
        {
          if (m_OptionFlags.HasFlag(OptionFlags.Position))
          {
            if (m_OffsetPose.position.IsZero())
            {
              m_OffsetPose.position = transform.position - m_Target.position;
            }
            else
            {
              m_OptionFlags &= ~OptionFlags.UseOffset;
              transform.position = m_Target.position;
              m_OffsetPose.position.Set(0f, 0f, 0f);
            }
          }
          else
          {
            m_OffsetPose.position.Set(0f, 0f, 0f);
          }

          if (m_OptionFlags.HasFlag(OptionFlags.Rotation))
          {
            if (m_OffsetPose.rotation.IsIdentity())
            {
              m_OffsetPose.rotation = transform.rotation * m_Target.rotation.Inverted();
            }
            else
            {
              transform.rotation = m_Target.rotation;
              m_OffsetPose.rotation.Set(0f, 0f, 0f, 1f);
            }  
          }
          else
          {
            m_OffsetPose.rotation.Set(0f, 0f, 0f, 1f);
          }
        }
      }
    }

    private void OnDrawGizmosSelected()
    {
      if (!m_Target)
        return;

      if (m_OptionFlags.HasFlag(OptionFlags.UseOffset) && !m_OffsetPose.position.IsZero())
      {
        Gizmos.color = Colors.Debug.GizmoBounds;
        Gizmos.DrawLine(transform.position, m_Target.position);
        Gizmos.DrawRay(m_Target.position, m_OffsetPose.position);
      }
    }

  }

}