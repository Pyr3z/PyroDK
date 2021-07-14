/**
@file   PyroDK/Core/ComponentTypes/InputEventDispatcher.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-12

@brief
  A generic interface for setting up callback hooks that
  are triggered by global single-player input events.
**/

#pragma warning disable CS0649

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Events/Input Event Dispatcher")]
  public sealed class InputEventDispatcher : BaseComponent
  {
    [SerializeField]
    #if !ENABLE_INPUT_SYSTEM
    [HideInInspector]
    #endif
    private List<InputSystemEvent>  m_InputSystemEvents = new List<InputSystemEvent>();

    [SerializeField]
    #if !ENABLE_LEGACY_INPUT_MANAGER
    [HideInInspector]
    #endif
    private List<InputManagerEvent> m_InputManagerEvents = new List<InputManagerEvent>();


    #if INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
    private void OnEnable()
    {
      foreach (var ev in m_InputSystemEvents)
        ev.BeginListening(reinit: false);
    }
    private void OnDisable()
    {
      foreach (var ev in m_InputSystemEvents)
        ev.StopListening();
    }
    #endif

    #if ENABLE_LEGACY_INPUT_MANAGER
    private void Update()
    {
      foreach (var ev in m_InputManagerEvents)
        ev.CheckedInvoke();
    }
    #endif


    #if UNITY_EDITOR
    private void OnValidate()
    {
      #if INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
      if (Application.IsPlaying(this))
      {
        foreach (var ev in m_InputSystemEvents)
        {
          ev.Bonk();
          ev.BeginListening(reinit: true);
        }
      }
      else
      {
        foreach (var ev in m_InputSystemEvents)
          ev.Bonk();
      }
      #endif // INPUT_SYSTEM

      #if ENABLE_LEGACY_INPUT_MANAGER
      foreach (var ev in m_InputManagerEvents)
        ev.Bonk();
      #endif
    }
    #endif // UNITY_EDITOR

  }

}