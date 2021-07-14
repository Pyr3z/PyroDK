/**
@file   PyroDK/Core/Utilities/Components.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-20

@brief
  Provides extensions and utilities for the `UnityEngine.Component`
  type and all its derivatives.
**/

#pragma warning disable UNT0008 // wrong. is fine.

using System.Linq;

using UnityEngine;


namespace PyroDK
{
  using StringBuilder = System.Text.StringBuilder;


  [System.Serializable]
  public class ComponentEvent : PyroEvent<Component>
  {
  }

  public static class Components
  {

    public static bool InScenelessAsset(this Component comp)
    {
      return comp && comp.gameObject.InScenelessAsset();
    }


    public static string GetComponentPath(this Component comp, bool scene_ctx = false)
    {
      return  Transforms.AppendHierarchyPath(new StringBuilder(), comp.transform, scene_ctx)
                        .Append(".<").Append(Types.GetLogName(comp)).Append('>')
                        .ToString();
    }

  }


  // TODO new file!!!

  [System.Serializable]
  public class TransformEvent : PyroEvent<Transform>
  {
  }

  public static class Transforms
  {

    public static string GetHierarchyPath(this Transform trans, bool scene_ctx = false)
    {
      return AppendHierarchyPath(new StringBuilder(), trans, scene_ctx).ToString();
    }

    public static StringBuilder AppendHierarchyPath(this StringBuilder strb, Transform trans, bool scene_ctx = false)
    {
      if (scene_ctx && !trans.gameObject.scene.name.IsEmpty())
      {
        _ = strb.Append(trans.gameObject.scene.name)
                .Append('|');
      }

      int start = strb.Length;

      foreach (var node in trans.TraverseUpwards().Reverse())
      {
        if (strb.Length > start)
          _ = strb.Append('/');

        _ = strb.Append(node.name);
      }

      return strb;
    }


    public static int GetHierarchyDepth(this Transform trans)
    {
      int depth = -1;

      while (trans)
      {
        trans = trans.parent;
        ++depth;
      }

      return depth;
    }


    public static void Rotate2D(this Transform trans, float degrees)
    {
      var eulers = trans.localEulerAngles;
      trans.localEulerAngles = new Vector3(eulers.x, eulers.y, eulers.z + degrees);
    }


    public static Pose GetWorldPose(this Transform trans)
    {
      return new Pose(trans.position, trans.rotation);
    }

    public static void SetWorldPose(this Transform trans, in Pose pose)
    {
      trans.SetPositionAndRotation(pose.position, pose.rotation);
    }


    public static Pose GetLocalPose(this Transform trans)
    {
      return new Pose(trans.localPosition, trans.localRotation);
    }

    public static void SetLocalPose(this Transform trans, in Pose world, in Pose local)
    {
      if (world == Pose.identity)
        SetWorldPose(trans, in local);
      else
        SetWorldPose(trans, local.GetTransformedBy(world));
    }

    public static void SetLocalPose(this Transform trans, in Pose local)
    {
      trans.localPosition = local.position;
      trans.localRotation = local.rotation;
    }


    public static void SetParentSpace(this Transform child, Transform parent)
    {
      SetWorldPose(child, GetWorldPose(child).GetTransformedBy(parent));
    }
  }

}