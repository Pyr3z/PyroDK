/**
@file   PyroDK/Core/EventTypes/InputSystemEvent.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2021-03-05

@brief
  Event types for responding to user input, supporting
  InputSystem as well as old school.
**/

using UnityEngine;

#if INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


namespace PyroDK
{

  #if INPUT_SYSTEM
  [System.Serializable]
  public sealed class InputSystemEvent :
    PyroEvent<InputAction.CallbackContext>,
    IHaveAName
  {
    public string Name => m_Name ?? RefreshName();


    [SerializeField] [RequiredReference]
    private InputActionReference m_Action;

    [SerializeField] [ViewAsEnum(typeof(InputActionPhase))]
    private int m_TriggerPhase;


    [System.NonSerialized]
    private InputAction m_ActionClone;

    [System.NonSerialized]
    private string m_Name;


    public void BeginListening(bool reinit)
    {
      var trigger = (InputActionPhase)m_TriggerPhase;

      if (!m_IsEnabled ||
          trigger == InputActionPhase.Disabled ||
          !m_Action ||
          m_Action.action == null)
      {
        StopListening();
        return;
      }

      if (m_ActionClone != null && !reinit)
        return;

      m_ActionClone = m_Action.action.Clone();

      switch (trigger)
      {
        case InputActionPhase.Started:
          m_ActionClone.started += Invoke;
          break;
        case InputActionPhase.Canceled:
          m_ActionClone.canceled += Invoke;
          break;
        case InputActionPhase.Performed:
          m_ActionClone.performed += Invoke;
          break;
      }

      m_ActionClone.Enable();
    }

    public void StopListening()
    {
      if (m_ActionClone != null)
      {
        m_ActionClone.Disable();
        m_ActionClone.Dispose();
        m_ActionClone = null;
      }
    }


    public void Bonk()
    {
      m_Name = null;
    }


    private string RefreshName()
    {
      if (m_Action && m_Action.action != null)
      {
        if (m_IsEnabled)
        {
          m_Name = RichText.String(m_Action.name) + " on " +
                   RichText.Value((object)(InputActionPhase)m_TriggerPhase);
        }
        else
        {
          m_Name = RichText.Color($"\"{m_Action.name}\" on {(InputActionPhase)m_TriggerPhase}", Colors.Gray);
        }
      }
      else
      {
        m_Name = RichText.Color("(no action)", Colors.Gray);
      }

      return m_Name;
    }

  }

  #else // define a data-safe dummy with no behavior:

  [System.Serializable]
  public sealed class InputSystemEvent : PyroEvent
  {
    [SerializeField] [ReadOnly]
    private Object m_Action;
    [SerializeField] [ReadOnly]
    private int m_TriggerPhase;


    public void BeginListening(bool reinit)
    {
      Logging.ShouldNotReach("Input System is not enabled!");
    }

    public void StopListening()
    {
    }

  }

  #endif // INPUT_SYSTEM

}