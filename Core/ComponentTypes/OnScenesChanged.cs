/**
@file   PyroDK/Core/ComponentTypes/OnScenesChanged.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-15

@brief
  Event interface that invokes events whenever certain Scenes
  are loaded/unloaded.
**/

#pragma warning disable CS0649

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Events/On Scenes Changed")]
  public sealed class OnScenesChanged : BaseComponent
  {

  [Header("Scene Filters")]
    [SerializeField]
    private SceneType       m_InvokesForType;
    [SerializeField] [RequiredReference]
    private List<SceneRef>  m_SceneWhitelist = new List<SceneRef>();

  [Header("Events")]
    [SerializeField]
    private DelayedEvent m_OnLoaded;
    [SerializeField]
    private DelayedEvent m_OnUnloaded;


    public bool InvokesFor(Scene scene)
    {
      if (!SceneRef.Find(scene, out SceneRef sref) || ( m_SceneWhitelist.Count > 0 &&
                                                       !m_SceneWhitelist.Contains(sref) ))
      {
        return false;
      }

      return sref.Type == m_InvokesForType;
    }


    private void InvokeOnLoad(Scene scene, LoadSceneMode mode)
    {
      if (mode == LoadSceneMode.Additive && InvokesFor(scene))
      {
        m_OnLoaded.TryInvokeOn(this);
      }
    }

    private void InvokeOnUnload(Scene scene)
    {
      if (InvokesFor(scene))
      {
        m_OnUnloaded.TryInvokeOn(this);
      }
    }


    private void OnEnable()
    {
      if (Application.IsPlaying(this) && m_SceneWhitelist.Count > 0)
      {
        int removed = m_SceneWhitelist.RemoveAll((s) => !s);

        if (removed > 0)
        {
          $"Removed {removed} null SceneRefs from \"{this}\"."
            .LogWarning(this);
        }
      }

      SceneManager.sceneLoaded    += InvokeOnLoad;
      SceneManager.sceneUnloaded  += InvokeOnUnload;
    }

    private void OnDisable()
    {
      SceneManager.sceneLoaded    -= InvokeOnLoad;
      SceneManager.sceneUnloaded  -= InvokeOnUnload;
    }

  }

}