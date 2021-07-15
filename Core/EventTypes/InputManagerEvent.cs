/**
@file   PyroDK/Core/EventTypes/InputManagerEvent.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2021-03-05

@brief
  Event types for responding to user input, supporting
  InputSystem as well as old school.
**/

using UnityEngine;

namespace PyroDK
{

  [System.Serializable]
  public sealed class InputManagerEvent :
    PyroEvent,
    IHaveAName
  {
    public enum TriggerType
    {
      None  = 0,
      Down  = 1,
      Stay  = 2,
      Up    = 3
    }

    public delegate bool KeyQuery(KeyCode key);

    private static KeyQuery[] s_KeyQueries = new KeyQuery[]
    {
      (k) => false, // None
      Input.GetKeyDown,
      Input.GetKey,
      Input.GetKeyUp,
    };


    public string Name => m_Name ?? RefreshName();


    protected override bool ShouldInvoke => s_KeyQueries[(int)m_TriggerType](m_TriggerKey);


    [SerializeField]
    private KeyCode     m_TriggerKey;
    [SerializeField]
    private TriggerType m_TriggerType;


    [System.NonSerialized]
    private string m_Name;


    public void Bonk()
    {
      m_Name = null;
    }
    

    private string RefreshName()
    {
      if (m_IsEnabled)
        m_Name = $"{RichText.Value((object)m_TriggerKey)} on {RichText.String(m_TriggerType)}";
      else
        m_Name = RichText.Color($"{m_TriggerKey} on \"{m_TriggerType}\"", Colors.Gray);

      return m_Name;
    }
  }

}