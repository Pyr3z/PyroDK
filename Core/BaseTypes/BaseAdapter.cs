/**
@file   PyroDK/Core/BaseTypes/BaseAdapter.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-08-10

@brief
  Base class for Adapter component types.
**/

using UnityEngine;


namespace PyroDK
{

  public abstract class BaseAdapter<T> : BaseComponent
    where T : Component
  {

    public T Target => m_TargetComponent;

    public virtual bool IsConnected => m_TargetComponent;


    [SerializeField] [RequiredReference]
    protected T m_TargetComponent;


    protected virtual void OnValidate()
    {
      if (!m_TargetComponent && !TryGetComponent(out m_TargetComponent) &&
          Application.isPlaying)
      {
        $"No valid target of type {TSpy<T>.LogName} assigned to Interface!"
          .LogWarning(this);
      }
    }

  }


  public static class Adapters
  {
    public static bool IsValid<T>(this BaseAdapter<T> self)
      where T : Component
    {
      return self && self.IsConnected;
    }
  }

}