/**
@file   PyroDK/Core/Utilities/GameObjects.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-20

@brief
  Provides extensions and utilities for the `UnityEngine.GameObject` type.
**/

using UnityEngine;
using UnityEngine.SceneManagement;


namespace PyroDK
{

  public static class GameObjects
  {
    public static bool InScenelessAsset(this GameObject obj)
    {
      // or, is the GameObject contained in a Prefab asset?
      return obj && obj.scene.IsValid();
    }


    public static string GetHierarchyPath(this GameObject obj, bool scene_ctx = false)
    {
      if (!obj)
        return null;

      return obj.transform.GetHierarchyPath(scene_ctx);
    }


    public static bool TryGetComponentInParent<T>(this GameObject obj, out T comp)
    {
      comp = obj.GetComponentInParent<T>();
      return comp != null;
    }


    public static Pose GetWorldPose(this GameObject obj)
    {
      return obj.transform.GetWorldPose();
    }

    public static void SetWorldPose(this GameObject obj, in Pose pose)
    {
      obj.transform.SetWorldPose(in pose);
    }


    public static Pose GetLocalPose(this GameObject obj)
    {
      return obj.transform.GetLocalPose();
    }

    public static void SetLocalPose(this GameObject obj, in Pose world, in Pose local)
    {
      obj.transform.SetLocalPose(in world, in local);
    }



    public static bool IsLayerMask(this GameObject obj, LayerMask mask)
    {
      return (mask & (1 << obj.layer)) != 0;
    }


    public static bool TryGetArbitrarySceneObject(out GameObject obj)
    {
      var gos = SceneManager.GetActiveScene().GetRootGameObjects();

      if (gos.Length > 0)
      {
        obj = gos[0];
        return obj;
      }

      obj = null;
      return false;
    }

  }

}