/**
@file   PyroDK/Core/ComponentTypes/CanvasUI.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-15

@brief
  A Singleton-patterned controller for your current UI Canvas.
**/

using UnityEngine;
using UnityEngine.EventSystems;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Singletons/Canvas UI Root Singleton")]
  [RequireComponent(typeof(Canvas))]
  public class CanvasUI : BaseSingleton<CanvasUI>
  {

    public static Canvas Canvas
    {
      get
      {
        if (s_Instance)
          return s_Instance.m_Canvas;
        return null;
      }
    }

    public static CanvasGroup RootGroup
    {
      get
      {
        CanvasGroup group = null;
        
        if (s_Instance)
        {
          if (s_Instance.m_MainGroup)
            group = s_Instance.m_MainGroup;
          else if (s_Instance.TryFindGroup(s_Instance.name, out group))
            s_Instance.m_MainGroup = group;
        }

        return group;
      }
    }

    public static bool IsAcceptingInput
    {
      get =>  s_Instance &&
              s_Instance.m_EventSystem &&
              EventSystem.current &&
              EventSystem.current.IsActive() &&
              EventSystem.current == s_Instance.m_EventSystem;
      set
      {
        if (s_Instance)
          s_Instance.EnableEventSystem(value);
      }
    }


    public static CanvasGroup FindGroup(string name)
    {
      if (s_Instance && s_Instance.TryFindGroup(name, out CanvasGroup group))
        return group;
      return null;
    }



  [Header("Control Groups")]
    [SerializeField]
    private ObjectLookup m_AllGroups = ObjectLookup.MakeStrict<CanvasGroup>();

  [Header("[ReadOnly]")]
    [SerializeField] [ReadOnly]
    private Canvas      m_Canvas;
    [SerializeField] [ReadOnly]
    private CanvasGroup m_MainGroup;
    [SerializeField] [ReadOnly]
    private EventSystem m_EventSystem;


    public void EnableEventSystem(bool enable)
    {
      if (m_EventSystem && enable != ( EventSystem.current == m_EventSystem && m_EventSystem.IsActive() ))
      {
        if (enable)
        {
          if (EventSystem.current)
            EventSystem.current.gameObject.SetActive(false);
          
          m_EventSystem.gameObject.SetActive(true);

          EventSystem.current = m_EventSystem;
        }
        else
        {
          m_EventSystem.gameObject.SetActive(false);
        }
      }
    }


    public bool TryFindGroup(string name, out CanvasGroup group)
    {
      return m_AllGroups.Find(name, out group);
    }

    public void ToggleGroup(string name)
    {
      if (m_AllGroups.Find(name, out CanvasGroup group))
      {
        group.gameObject.SetActive(!group.gameObject.activeSelf);

        //$"Toggled {group.gameObject} {(group.gameObject.activeSelf ? "ON" : "OFF")}"
        //  .LogSuccess();
      }
      else
      {
        $"Failed to find Control Group {name}."
          .LogWarning();
      }
    }



    protected override void OnValidate()
    {
      base.OnValidate();

      Logging.Assert(m_Canvas || TryGetComponent(out m_Canvas), "m_Canvas");

      if (m_AllGroups.Count == 0 || !m_AllGroups.Find(name, out m_MainGroup))
      {
        m_AllGroups.Clear();

        var groups = GetComponentsInChildren<CanvasGroup>(includeInactive: true);

        foreach (var group in groups)
        {
          if (group.gameObject == gameObject)
          {
            m_MainGroup = group;
          }

          if (!m_AllGroups.Map(group.name, group))
          {
            Logging.ShouldNotReach(blame: group.name);
          }
        }
      }

      m_EventSystem = GetComponentInChildren<EventSystem>(includeInactive: true);
    }

  }

}