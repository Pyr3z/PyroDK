/**
@file   PyroDK/Core/Utilities/Quaternions.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-06

@brief
  Provides utilities for the `UnityEngine.Quaternion` struct.
**/

using UnityEngine;


namespace PyroDK
{

  public static class Quaternions
  {

    public static readonly Quaternion Identity    = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

    public static readonly Quaternion Degrees45   = Quaternion.Euler(0.0f, 0.0f,  45.0f);
    public static readonly Quaternion Degrees90   = Quaternion.Euler(0.0f, 0.0f,  90.0f);
    public static readonly Quaternion Degrees135  = Quaternion.Euler(0.0f, 0.0f, 135.0f);
    public static readonly Quaternion Degrees180  = Quaternion.Euler(0.0f, 0.0f, 180.0f);
    public static readonly Quaternion Degrees225  = Quaternion.Euler(0.0f, 0.0f, 225.0f);
    public static readonly Quaternion Degrees270  = Quaternion.Euler(0.0f, 0.0f, 270.0f);
    public static readonly Quaternion Degrees315  = Quaternion.Euler(0.0f, 0.0f, 315.0f);


    public static bool Approximately(this Quaternion a, in Quaternion b)
    {
      return (a.w -= b.w) * a.w < Floats.EPSILON2
          && (a.x -= b.x) * a.x < Floats.EPSILON2
          && (a.y -= b.y) * a.y < Floats.EPSILON2
          && (a.z -= b.z) * a.z < Floats.EPSILON2;
    }

    public static bool Approximately(this Quaternion a, in Quaternion b, float epsilon)
    {
      epsilon *= epsilon;
      return (a.w -= b.w) * a.w < epsilon
          && (a.x -= b.x) * a.x < epsilon
          && (a.y -= b.y) * a.y < epsilon
          && (a.z -= b.z) * a.z < epsilon;
    }

    public static bool IsIdentity(in this Quaternion q)
    {
      return q.Approximately(Identity);
    }

    public static bool IsIdentity(in this Quaternion q, float epsilon)
    {
      return q.Approximately(Identity, epsilon);
    }


    public static bool IsRotation(in this Quaternion q)
    {
      return ( q.x * q.x +
               q.y * q.y +
               q.z * q.z +
               q.w * q.w ).Approximately(1f);
    }


    public static Vector4 ToVec4(in this Quaternion q)
    {
      return new Vector4(q.x, q.y, q.z, q.w);
    }

    public static Vector3 ToEuler180(in this Quaternion q)
    {
      var v = q.eulerAngles;

      if (v.x > 180f)
        v.x -= 360f;

      if (v.y > 180f)
        v.y -= 360f;

      if (v.z > 180f)
        v.z -= 360f;

      return v;
    }


    public static bool PitchClamped(ref this Quaternion q, float delta, float min, float max)
    {
      if (delta.IsZero())
        return false;

      q.Set(q.x / q.w,
            q.y / q.w,
            q.z / q.w,
            1f);
      float pitch = Mathf.Atan(q.x) * Mathf.Rad2Deg * 2f + delta;
      q.x = Mathf.Tan(pitch.Clamp(min, max) * Mathf.Deg2Rad / 2f);
      return true;
    }

    public static bool Yaw(ref this Quaternion q, float delta)
    {
      if (delta.IsZero())
        return false;
      q *= Quaternion.Euler(0f, delta, 0f);
      return true;
    }


    public static Quaternion Copy(in this Quaternion q)
    {
      return new Quaternion(q.x, q.y, q.z, q.w);
    }


    public static void Invert(ref this Quaternion q)
    {
      q = Quaternion.Inverse(q);
    }

    public static Quaternion Inverted(in this Quaternion q)
    {
      return Quaternion.Inverse(q);
    }


    public static void LerpTo(ref this Quaternion q, in Quaternion to, float t)
    {
      q = Quaternion.LerpUnclamped(q, to, t);
    }

    public static Quaternion LerpedTo(in this Quaternion q, in Quaternion to, float t)
    {
      return Quaternion.LerpUnclamped(q, to, t);
    }

    public static void SmoothSet(ref this Quaternion q, in Quaternion set)
    {
      q = Quaternion.RotateTowards(q, set, 180f);
    }

    public static void SmoothSetEulers(ref this Quaternion q, in Vector3 eulers)
    {
      q = Quaternion.RotateTowards(q, Quaternion.Euler(eulers), 180f);
    }


    public static void SlerpTo(ref this Quaternion q, in Quaternion to, float t)
    {
      q = Quaternion.SlerpUnclamped(q, to, t);
    }

    public static Quaternion SlerpedTo(in this Quaternion q, in Quaternion to, float t)
    {
      return Quaternion.Slerp(q, to, t);
    }

  }


  // TODO: move to separate file
  public static class Poses
  {

    public static readonly Pose Enabled = new Pose()
    {
      rotation = new Quaternion(0f, 0f, 0f, 1f)
    };

    public static readonly Pose Disabled = new Pose()
    {
      rotation = new Quaternion(0f, 0f, 0f, 2f)
    };



    public static bool IsEnabled(in this Pose pose)
    {
      return pose.rotation.IsRotation();
    }

    public static void SetEnabled(ref this Pose pose, bool set)
    {
      if (set)
      {
        if (!pose.rotation.IsRotation())
        {
          pose.rotation.Normalize();
        }
      }
      else if (pose.rotation.IsRotation())
      {
        // we can't cheaply know which are 0.0 and which aren't
        pose.rotation.x *= 2f;
        pose.rotation.y *= 2f;
        pose.rotation.z *= 2f;
        pose.rotation.w *= 2f;
      }
    }


    public static Pose Inverse(in this Pose pose)
    {
      return new Pose(-1 * pose.position,
                      Quaternion.Inverse(pose.rotation));
    }

  }

}