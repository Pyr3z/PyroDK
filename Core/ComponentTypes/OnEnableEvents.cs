/**
@file   PyroDK/Core/ComponentTypes/OnEnableEvents.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-21

@brief
  Event interface that invokes events whenever it is
  enabled or disabled.
**/

#pragma warning disable CS0649

using UnityEngine;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Events/On Enable, On Disable")]
  public sealed class OnEnableEvents : BaseComponent
  {

    [SerializeField]
    private DelayedEvent m_OnEnable;

    [Space]

    [SerializeField]
    private DelayedEvent m_OnDisable;



    private void OnEnable()
    {
      m_OnEnable.TryInvokeOn(this);
    }


    private void OnDisable()
    {
      m_OnDisable.TryInvokeOn(this);
    }

  }

}