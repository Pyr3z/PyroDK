/**
@file   PyroDK/Core/EventTypes/PyroEvent.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-25

@brief
  Basic IPyroEvent implementations.
**/

using UnityEngine;
using UnityEngine.Events;


namespace PyroDK
{

  [System.Serializable]
  public /*abstract*/ class PyroEvent :
    UnityEvent,
    IPyroEvent
  {
    [SerializeField] [HideInInspector]
    protected bool m_IsEnabled;

    public bool IsEnabled
    {
      get => m_IsEnabled;
      set => m_IsEnabled = value;
    }

    protected virtual bool ShouldInvoke => true;


    new public void Invoke()
    {
      if (m_IsEnabled && ShouldInvoke)
        base.Invoke();
    }

    public bool TryInvoke()
    {
      if (m_IsEnabled && ShouldInvoke)
      {
        base.Invoke();
        return true;
      }

      return false;
    }
  }


  [System.Serializable]
  public abstract class PyroEvent<T> :
    UnityEvent<T>,
    IPyroEvent
  {
    [SerializeField] [HideInInspector]
    protected bool m_IsEnabled;

    public bool IsEnabled
    {
      get => m_IsEnabled;
      set => m_IsEnabled = value;
    }

    protected virtual bool ShouldInvoke => true;


    new public void Invoke(T arg)
    {
      if (m_IsEnabled && ShouldInvoke)
        base.Invoke(arg);
    }

    public bool TryInvoke(T arg)
    {
      if (m_IsEnabled && ShouldInvoke)
      {
        base.Invoke(arg);
        return true;
      }

      return false;
    }
  }


  [System.Serializable]
  public sealed class GameObjectEvent : PyroEvent<GameObject>
  {
  }

}