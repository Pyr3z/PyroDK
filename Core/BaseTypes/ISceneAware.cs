/**
@file   PyroDK/Core/BaseTypes/ISceneAware.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-07

@brief
  Defines the ISceneAware interface.
**/

using System.Collections.Generic;

using UnityEngine.SceneManagement;


namespace PyroDK
{

  public interface ISceneAware : IObject
  {
    void OnSceneLoaded(SceneRef sref);
    void OnSceneUnloaded(SceneRef sref);
  }


  public static class ISceneAwareExtensions
  {

    private static HashSet<ISceneAware> s_Registered = new HashSet<ISceneAware>();


    public static void RegisterSceneCallbacks(this ISceneAware self)
    {
      if (self != null && s_Registered.Add(self))
      {
        SceneManager.sceneLoaded   += self.OnSceneLoadedDelegate;
        SceneManager.sceneUnloaded += self.OnSceneUnloadedDelegate;
      }
    }
    public static void DeregisterCallbacks(this ISceneAware self)
    {
      if (self != null && s_Registered.Remove(self))
      {
        SceneManager.sceneLoaded   -= self.OnSceneLoadedDelegate;
        SceneManager.sceneUnloaded -= self.OnSceneUnloadedDelegate;
      }
    }


    public static void AttachCallbacksTo(this ISceneAware self, SceneRef sref)
    {
      if (sref)
      {
        sref.OnLoaded       += self.OnSceneLoaded;
        sref.OnAfterUnload  += self.OnSceneUnloaded;
      }
    }
    public static void DetachCallbacksTo(this ISceneAware self, SceneRef sref)
    {
      if (sref)
      {
        sref.OnLoaded       -= self.OnSceneLoaded;
        sref.OnAfterUnload  -= self.OnSceneUnloaded;
      }
    }


    private static void OnSceneLoadedDelegate(this ISceneAware self, Scene scene, LoadSceneMode _ )
    {
      if (SceneRef.Find(scene, out SceneRef sref))
      {
        self.OnSceneLoaded(sref);
      }
    }

    private static void OnSceneUnloadedDelegate(this ISceneAware self, Scene scene)
    {
      if (SceneRef.Find(scene, out SceneRef sref))
      {
        self.OnSceneUnloaded(sref);
      }
    }

  }

}