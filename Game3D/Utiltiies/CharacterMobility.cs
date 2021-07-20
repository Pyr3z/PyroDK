/**
@file   PyroDK/Game3D/Utilities/CharacterMobility.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-08-10

@brief
  Utilities that make implementing various character motion controllers
  a relative breeze.
**/

using System.Collections;

using UnityEngine;


namespace PyroDK.Game3D
{

  public static class CharacterMobility
  {

    public static readonly MinMaxFloat DefaultTweakySpeed = MinMaxFloat.Make(0.0f).WithMin(0.1f);

    public static MinMaxFloat MakeTweakySpeed(float max_speed = 10.0f)
    {
      return DefaultTweakySpeed.WithMax(max_speed);
    }



    public static bool PrepLocalVelocity(ref Vector3 local_vel, ref MinMaxFloat speed, float delta)
    {
      float local_speed = local_vel.SqrMagnitudeXZ();

      if (speed.IsMinMaxLocked || speed.IsMin(local_speed))
      {
        speed.Value = local_vel.x = local_vel.z = 0f;
        return false;
      }

      if (speed.Value + delta > speed.Max)
      {
        float scalar = 1f / local_speed.Sqrt() * speed.Max;

        local_vel.x *= scalar;
        local_vel.z *= scalar;

        speed.Value = speed.Max;
      }
      else
      {
        float scalar = 1f / local_speed.Sqrt() * (speed.Value + delta);
        
        local_vel.x *= scalar;
        local_vel.z *= scalar;

        speed.Value += delta;
      }

      return true;
    }

    public static void ApplyLocalVelocity(this Rigidbody rb, ref Vector3 local_vel)
    {
      local_vel.y = rb.velocity.y;

      //var curr_vel = rb.velocity;
      var targ_vel = rb.transform.TransformVector(local_vel);
      //targ_vel.x = Mathf.LerpUnclamped(curr_vel.x, targ_vel.x, t);
      //targ_vel.z = Mathf.LerpUnclamped(curr_vel.z, targ_vel.z, t);

      rb.velocity = targ_vel;
    }



    public static bool GetNextRotationVelocity(this Rigidbody rb, out Quaternion  next_rot,
                                                                      float       max_delta)
    {
      next_rot = rb.rotation;

      if (!max_delta.IsPositive())
        return false;

      var look_dir = rb.velocity.FlattenedXZ();

      if (look_dir.IsZero())
        return false;

      var   look_rot  = Quaternion.LookRotation(look_dir);
      float delta     = Quaternion.Angle(next_rot, look_rot);

      if (delta == 0.0f)
        return false;

      if (delta < max_delta)
        next_rot = look_rot;
      else
        next_rot.SlerpTo(look_rot, max_delta / delta);

      return true;
    }


    public static bool GetNextRotationLookAt(this Rigidbody rb, out Quaternion  next_rot,
                                                                    float       max_delta,
                                                                    Vector3     look_at)
    {
      next_rot = rb.rotation;

      if (!max_delta.IsPositive())
        return false;

      look_at = (look_at - rb.transform.position);

      if (look_at.IsZero(0.3f))
        return GetNextRotationVelocity(rb, out next_rot, max_delta);

      var   look_rot  = Quaternion.LookRotation(look_at);
      float delta     = Quaternion.Angle(next_rot, look_rot);

      if (delta == 0.0f)
        return false;

      if (delta < max_delta)
        next_rot = look_rot;
      else
        next_rot.SlerpTo(look_rot, max_delta / delta);

      return true;
    }



    public static bool AdjustedRaycast(this Rigidbody rb, Collider    body,
                                                      out RaycastHit  hit,
                                                          Vector3     dir,
                                                          float       distance,
                                                          LayerMask   mask = default)
    {
      if (Physics.Raycast(rb.position, dir, out hit,
                          distance, mask,
                          QueryTriggerInteraction.Ignore))
      {
        // First calculate the vector from our "skin" to our "center":
        var to_center = rb.position - body.ClosestPoint(hit.point);

        // Then, simply add this vector to the hit point. This should clamp
        // the player's collider to prevent phasing through terrain.
        hit.point += to_center;
        return true;
      }

      return false;
    }


    public static IEnumerator GotoAsync(Rigidbody rb, Vector3 target, float in_seconds)
    {
      var start = rb.position;

      if (start.Approximately(target) || in_seconds < Floats.EPSILON)
      {
        rb.MovePosition(target);
        yield break;
      }

      float time = Time.time;
      float t = 0.0f;

      in_seconds = 1.0f / in_seconds; // for faster arithmetic

      while (t < 1.0f)
      {
        rb.MovePosition(Vector3.LerpUnclamped(start, target, t));

        yield return new WaitForFixedUpdate();

        t = (Time.time - time) * in_seconds;
      }

      rb.MovePosition(target);
    }

    public static IEnumerator TravelAsync(Rigidbody rb,
                                          CapsuleCollider caps,
                                          Vector3 direction,
                                          float max_distance,
                                          float speed)
    {
      var start = rb.position;

      // calculate the capsule sub-sphere centers:
      var (c1, c2) = CapsuleCentersWorld(caps);

      // determine if full travel, or if we are cut off short:
      Vector3 target;
      if (Physics.CapsuleCast(c1, c2, caps.radius, direction, out RaycastHit hit, max_distance))
      {
        target = hit.point + Vector3.Project((start - hit.point), hit.normal);
        max_distance = (target - start).magnitude;
      }
      else
      {
        target = start + direction * max_distance;
      }

      // similar to before:

      float duration = 1.0f / (max_distance / speed);
      float time = Time.time;
      float t = 0.0f;

      while (t < 1.0f)
      {
        rb.MovePosition(Vector3.LerpUnclamped(start, target, t));

        yield return new WaitForFixedUpdate();

        t = (Time.time - time) * duration;
      }

      rb.MovePosition(target);
    }

    public static (Vector3 c1, Vector3 c2) CapsuleCentersWorld(CapsuleCollider caps)
    {
      (Vector3 c1, Vector3 c2) result = (caps.center, caps.center);

      float offset = caps.height / 2f - caps.radius;

      result.c1[caps.direction] += offset;
      result.c2[caps.direction] -= offset;

      result.c1 = caps.transform.TransformPoint(result.c1);
      result.c2 = caps.transform.TransformPoint(result.c2);

      return result;
    }

    public static (Vector3 c1, Vector3 c2) CapsuleCentersLocal(CapsuleCollider caps)
    {
      (Vector3 c1, Vector3 c2) result = (caps.center, caps.center);

      float offset = caps.height / 2f - caps.radius;

      result.c1[caps.direction] += offset;
      result.c2[caps.direction] -= offset;

      return result;
    }

    // TODO move to Game2D
    public static int CapsuleCentersLocal(CapsuleCollider2D caps, out Vector3 c1, out Vector3 c2)
    {
      c1 = c2 = caps.offset;

      int dir = -1;
      if (caps.direction == CapsuleDirection2D.Vertical &&
          caps.size.x < caps.size.y)
      {
        dir = 1;
      }
      else if (caps.direction == CapsuleDirection2D.Horizontal &&
               caps.size.y < caps.size.x)
      {
        dir = 0;
      }
      else
      {
        // forms a circle.
        return -1;
      }

      float offset = ( caps.size[dir]       / 2f ) -
                     ( caps.size[dir.NOT()] / 2f );

      c1[dir] += offset;
      c2[dir] -= offset;

      return dir;
    }

  }

}