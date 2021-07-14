/**
@file   PyroDK/Game3D/ComponentTypes/PoseSnapshotAnimator.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  Holds and allows lerping between snapshots of a Transform component
  (basically, a cheaper Animator).
**/

#pragma warning disable CS0649, CS0414

using System.Collections.Generic;
using System.Collections;

using UnityEngine;


namespace PyroDK.Game3D
{

  [AddComponentMenu("PyroDK/Game3D/Pose Snapshot Animator")]
  public class PoseSnapshotAnimator : BaseComponent
  {

    private const float GOTO_DURATION_MIN = 0.1f;


    public float GotoDuration
    {
      get => m_GotoDuration;
      set => m_GotoDuration = value.AtLeast(GOTO_DURATION_MIN);
    }


  [Header("Default Movement Behavior")]
    [SerializeField] [Min(GOTO_DURATION_MIN)]
    private float m_GotoDuration = 5f;
    [SerializeField]
    private AnimationCurve m_PositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField]
    private AnimationCurve m_RotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

  [Header("Snapshots")]
    [SerializeField] [Min(0)]
    private int m_InitialSnapshotIndex = 0;
    [SerializeField] [ButtonBool]
    private bool m_TakeSnapshotNow;
    [SerializeField]
    private List<Pose> m_Snapshots = new List<Pose>();


    [System.NonSerialized]
    private Coroutine m_CurrentGoto = null;



    public void TakeSnapshot()
    {
      m_TakeSnapshotNow = false;
      m_Snapshots.Add(transform.GetLocalPose());
    }

    public void StopCurrentGoto()
    {
      if (m_CurrentGoto != null)
      {
        StopCoroutine(m_CurrentGoto);
        m_CurrentGoto = null;
      }
    }

    public void SetSnapshot(int idx)
    {
      if (!Logging.Assert(idx >= 0 && idx < m_Snapshots.Count, "Bad index given to GotoSnapshotImmediate(int)!"))
      {
        StopCurrentGoto();
        transform.SetLocalPose(m_Snapshots[idx]);
      }
    }

    public void GotoSnapshot(int idx)
    {
      if (!Logging.Assert(idx >= 0 && idx < m_Snapshots.Count, "Bad index given to GotoSnapshotImmediate(int)!"))
      {
        StopCurrentGoto();
        m_CurrentGoto = StartCoroutine(GotoSnapshotAsync(m_Snapshots[idx], m_GotoDuration));
      }
    }


    private void OnEnable()
    {
      if (m_Snapshots.Count > 0 &&
          m_InitialSnapshotIndex >= 0 &&
          m_InitialSnapshotIndex < m_Snapshots.Count)
      {
        SetSnapshot(m_InitialSnapshotIndex);
      }
    }

    private void OnValidate()
    {
      if (m_TakeSnapshotNow || m_Snapshots.Count == 0)
      {
        TakeSnapshot();
      }

      if (m_InitialSnapshotIndex >= m_Snapshots.Count)
      {
        m_InitialSnapshotIndex = m_Snapshots.Count - 1;
      }

      if (m_PositionCurve.length == 0)
      {
        m_PositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
      }

      if (m_RotationCurve.length == 0)
      {
        m_RotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
      }
    }


    private IEnumerator GotoSnapshotAsync(Pose snap, float duration)
    {
      var   start = transform.GetLocalPose();
      float time  = Time.time;
      float tpos  = m_PositionCurve.Evaluate(0f);
      float trot  = m_RotationCurve.Evaluate(0f);

      if (start.position.Approximately(snap.position))
      {
        tpos = 2f;
      }

      if (start.rotation.Approximately(snap.rotation))
      {
        trot = 2f;
      }

      duration = 1f / duration; // minor optimization

      while (tpos < 1f || trot < 1f)
      {
        yield return new WaitForEndOfFrame();

        float t = (Time.time - time) * duration;

        if (tpos < 1f)
        {
          tpos = m_PositionCurve.Evaluate(t);
          transform.localPosition = start.position.LerpedTo(snap.position, tpos);
        }

        if (trot < 1f)
        {
          trot = m_RotationCurve.Evaluate(t);
          transform.localRotation = start.rotation.SlerpedTo(snap.rotation, trot);
        }
      }

      transform.SetLocalPose(snap);

      m_CurrentGoto = null;
    }

  }

}