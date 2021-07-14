/**
@file   PyroDK/Core/BaseTypes/BaseInterface.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-08-10

@brief
  Base class for ____Interface component types.
**/

using UnityEngine;


namespace PyroDK
{

  public abstract class BaseInterface<T> : BaseComponent
    where T : Component
  {

    public T Target => m_TargetComponent;

    public virtual bool IsInterfaceConnected => m_TargetComponent;


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


  public static class Interfaces
  {
    public static bool IsValid<T>(this BaseInterface<T> self)
      where T : Component
    {
      return self && self.IsInterfaceConnected;
    }
  }

}