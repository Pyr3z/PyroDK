/**
@file   PyroDK/Core/ComponentTypes/UICanvas.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-15

@brief
  A Singleton-patterned controller for your current UI Canvas.
**/

using UnityEngine;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Singletons/Global UI Canvas")]
  [RequireComponent(typeof(Canvas))]
  public class UICanvas : BaseSingleton<UICanvas>
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

    public static CanvasGroup MainCanvasGroup
    {
      get
      {
        if (s_Instance && s_Instance.TryFindGroup(s_Instance.name, out CanvasGroup group))
          return group;
        return null;
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



    public bool TryFindGroup(string name, out CanvasGroup group)
    {
      return m_AllGroups.Find(name, out group);
    }

    public void ToggleGroup(string name)
    {
      if (m_AllGroups.Find(name, out CanvasGroup group))
      {
        group.gameObject.SetActive(!group.gameObject.activeSelf);

        $"Toggled {group.gameObject} {(group.gameObject.activeSelf ? "ON" : "OFF")}"
          .LogSuccess();
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
    }

  }

}