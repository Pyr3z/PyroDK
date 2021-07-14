/**
@file   PyroDK/Core/EventTypes/DelayedEvent.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-25

@brief
  An IPyroEvent that supports a time delay before invoking.
**/

using System.Collections;

using UnityEngine;
using UnityEngine.Events;


namespace PyroDK
{

  [System.Serializable]
  public class DelayedEvent : UnityEvent, IPyroEvent
  {
    public bool IsEnabled
    {
      get => m_IsEnabled;
      set => m_IsEnabled = value;
    }

    public bool HasInvocation => m_Invocation != null;

    public float Delay
    {
      get => m_DelaySeconds.AtLeast(0f);
      set => m_DelaySeconds = value.AtLeast(0f);
    }


    [SerializeField] [HideInInspector]
    protected bool m_IsEnabled;

    [SerializeField] [SignBitBool]
    protected float m_DelaySeconds = -1f;


    [System.NonSerialized]
    private Coroutine m_Invocation = null;


    new public void Invoke()
    {
      Debug.Assert(RootScene.IsActive);
      _ = TryInvokeOn(RootScene.Current);
    }


    public bool TryInvokeOn(MonoBehaviour component)
    {
      if (!m_IsEnabled ||
          Logging.Assert(m_Invocation == null, "DelayedEvent could not invoke - there is already an invocation queued!"))
      {
        return false;
      }

      if (m_DelaySeconds < Floats.EPSILON)
        base.Invoke();
      else
        m_Invocation = component.StartCoroutine(DelayedInvokeCoroutine());

      return true;
    }


    private IEnumerator DelayedInvokeCoroutine()
    {
      yield return new WaitForSeconds(m_DelaySeconds);
      base.Invoke();
      m_Invocation = null;
    }

  }

}