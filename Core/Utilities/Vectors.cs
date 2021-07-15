/**
@file   PyroDK/Core/Utilities/Vectors.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-06

@brief
  Provides utilities for the `UnityEngine.Vector2` and `UnityEngine.Vector3`
  structs.
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  public static class Axis2D
  {
    public static readonly Vector2 Origin = new Vector2(0f, 0f);

    public static readonly Vector2 X = new Vector2(1f, 0f);
    public static readonly Vector2 Y = new Vector2(0f, 1f);
  }


  public static class Axis3D
  {
    public static readonly Vector3 Origin = new Vector3(0f, 0f, 0f);

    public static readonly Vector3 X = new Vector3(1f, 0f, 0f);
    public static readonly Vector3 Y = new Vector3(0f, 1f, 0f);
    public static readonly Vector3 Z = new Vector3(0f, 0f, 1f);

    public static readonly Vector3 XY = X + Y;
    public static readonly Vector3 XZ = X + Z;
    public static readonly Vector3 ZY = Y + Z;
  }


  public static class Cardinals
  {
    public static readonly Vector2Int E   = new Vector2Int( 1,  0);
    public static readonly Vector2Int N   = new Vector2Int( 0,  1);
    public static readonly Vector2Int W   = new Vector2Int(-1,  0);
    public static readonly Vector2Int S   = new Vector2Int( 0, -1);

    public static readonly Vector2Int NE  = new Vector2Int( 1,  1);
    public static readonly Vector2Int NW  = new Vector2Int(-1,  1);
    public static readonly Vector2Int SW  = new Vector2Int(-1, -1);
    public static readonly Vector2Int SE  = new Vector2Int( 1, -1);

    public static readonly Vector2Int[] Orthogonals = new Vector2Int[] { E, N, W, S };
    public static readonly Vector2Int[] Diagonals   = new Vector2Int[] { NE, NW, SW, SE };
    public static readonly Vector2Int[] Hexagonals  = new Vector2Int[] { E, NE, NW, W, SW, SE };
    public static readonly Vector2Int[] All         = new Vector2Int[] { E, NE, N, NW, W, SW, S, SE };
  }


  public static class IntVec2D
  {

    public static readonly Vector2Int Zero  = new Vector2Int(0, 0);
    public static readonly Vector2Int One   = new Vector2Int(1, 1);

    public static readonly Vector2Int Max = new Vector2Int(int.MaxValue, int.MaxValue);
    public static readonly Vector2Int Min = new Vector2Int(int.MinValue, int.MinValue);

    public static readonly Vector2Int Right = new Vector2Int(1, 0);
    public static readonly Vector2Int Up    = new Vector2Int(0, 1);

    public static readonly Vector2Int Left  = new Vector2Int(-1,  0);
    public static readonly Vector2Int Down  = new Vector2Int( 0, -1);


    public static bool IsZero(this Vector2Int v)
    {
      return v.x == 0 && v.y == 0;
    }


    public static int SquareDistanceTo(this Vector2Int a, Vector2Int b)
    {
      return (b - a).sqrMagnitude;
    }


    public static Vector3Int ToVec3(this Vector2Int vec2)
    {
      return new Vector3Int(vec2.x, vec2.y, 0);
    }


    public static Vector2Int FromIndex(int xmax, int idx)
    {
      if (xmax <= 0)
        return Max;
      return new Vector2Int(idx % xmax, idx / xmax);
    }

    public static int CalcIndex(this Vector2Int coord, int xmax)
    {
      xmax = coord.y * xmax + coord.x;

      if (xmax < 0)
        return int.MaxValue; // ensures `if (idx < array.Length)` evaluates false
      return xmax;
    }


    public static Vector2Int Clamped(this Vector2Int cell, Vector2Int min, Vector2Int max)
    {
      if (cell.x < min.x)
        cell.x = min.x;
      else if (cell.x >= max.x)
        cell.x = max.x - 1;

      if (cell.y < min.y)
        cell.y = min.y;
      else if (cell.y >= max.y)
        cell.y = max.y - 1;

      return cell;
    }

    public static Vector2Int Clamped(this Vector2Int cell, in RectInt bounds)
    {
      return Clamped(cell, bounds.min, bounds.max);
    }


    public static IEnumerable<Vector2Int> WithOffsets(this Vector2Int cell, params Vector2Int[] offsets)
    {
      foreach (var offset in offsets)
      {
        yield return cell + offset;
      }
    }
    public static IEnumerable<Vector2Int> WithOffsets(this Vector2Int cell, RectInt bounds, params Vector2Int[] offsets)
    {
      Vector2Int current;
      foreach (var offset in offsets)
      {
        current = cell + offset;

        if (bounds.Contains(current))
          yield return current;
      }
    }


    public static IEnumerable<Vector2Int> Orthogonals(this Vector2Int cell)
    {
      return cell.WithOffsets(Cardinals.Orthogonals);
    }
    public static IEnumerable<Vector2Int> Orthogonals(this Vector2Int cell, in RectInt bounds)
    {
      return cell.WithOffsets(bounds, Cardinals.Orthogonals);
    }

    public static IEnumerable<Vector2Int> Diagonals(this Vector2Int cell)
    {
      return cell.WithOffsets(Cardinals.Diagonals);
    }
    public static IEnumerable<Vector2Int> Diagonals(this Vector2Int cell, in RectInt bounds)
    {
      return cell.WithOffsets(bounds, Cardinals.Diagonals);
    }

    public static IEnumerable<Vector2Int> Adjacent(this Vector2Int cell)
    {
      return cell.WithOffsets(Cardinals.All);
    }
    public static IEnumerable<Vector2Int> Adjacent(this Vector2Int cell, in RectInt bounds)
    {
      return cell.WithOffsets(bounds, Cardinals.All);
    }

    public static RectInt.PositionEnumerator Neighborhood(this Vector2Int cell, int size)
    {
      // NOW INCLUDES THE PASSED-IN CELL BY DEFAULT!
      var ext = One * Integers.CalcExtent(size);
      return new RectInt.PositionEnumerator(cell - ext, cell + ext);
    }

    public static RectInt.PositionEnumerator Neighborhood(this Vector2Int cell, int size, RectInt bounds)
    {
      // NOW INCLUDES THE PASSED-IN CELL BY DEFAULT!
      var ext = One * Integers.CalcExtent(size);

      if (bounds.Overlaps(cell, ext.x))
      {
        return new RectInt.PositionEnumerator(bounds.ClampCell(cell - ext), bounds.ClampCell(cell + ext));
      }
     
      return default;
    }


    public static IEnumerable<Vector2Int> RasterLineTo(this Vector2Int a, Vector2Int b)
    {
      int dx = Mathf.Abs(b.x - a.x);
      int dy = Mathf.Abs(b.y - a.y);
      int x_inc = (b.x < a.x) ? -1 : 1;
      int y_inc = (b.y < a.y) ? -1 : 1;

      bool side_equal;
      if (a.x == b.x)
        side_equal = (a.y < b.y);
      else
        side_equal = (a.x < b.x);

      int i = dx + dy;
      int error = dx - dy;
      dx *= 2;
      dy *= 2;

      while (i-- > 0)
      {
        yield return a;

        if (error > 0 || (side_equal && error == 0))
        {
          a.x += x_inc;
          error -= dy;
        }
        else
        {
          a.y += y_inc;
          error += dx;
        }
      }

      yield return b;
    }

    public static bool RasterCircleContains(this Vector2Int center, Vector2Int cell, float radius)
    {
      Vector2 line = cell - center;
      return line.sqrMagnitude < (radius * radius + Floats.EPSILON2);
    }

    public static IEnumerable<Vector2Int> RasterCircleFilled(this Vector2Int center, float radius)
    {
      Vector2 los;
      float   radius_sqr = radius * radius + Floats.EPSILON2;

      foreach (var cell in center.Neighborhood(Mathf.CeilToInt(radius * 2)))
      {
        los = cell - center;
        
        if (los.sqrMagnitude < radius_sqr)
          yield return cell;
      }
    }
    public static IEnumerable<Vector2Int> RasterCircleFilled(this Vector2Int center, float radius, RectInt bounds)
    {
      Vector2 los;
      float   radius_sqr = radius * radius + Floats.EPSILON2;

      foreach (var cell in center.Neighborhood(Mathf.CeilToInt(radius * 2), bounds))
      {
        los = cell - center;

        if (los.sqrMagnitude < radius_sqr)
          yield return cell;
      }
    }

  }


  public static class IntVec3D
  {

    public static readonly Vector3Int Zero = new Vector3Int(0, 0, 0);
    public static readonly Vector3Int One  = new Vector3Int(1, 1, 1);

    public static readonly Vector3Int Max = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
    public static readonly Vector3Int Min = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    public static readonly Vector3Int Right   = new Vector3Int(1, 0, 0);
    public static readonly Vector3Int Up      = new Vector3Int(0, 1, 0);
    public static readonly Vector3Int Forward = new Vector3Int(0, 0, 1);

    public static readonly Vector3Int Left      = new Vector3Int(-1,  0,  0);
    public static readonly Vector3Int Down      = new Vector3Int( 0, -1,  0);
    public static readonly Vector3Int Backward  = new Vector3Int( 0,  0, -1);


    public static Vector2Int ToVec2(this Vector3Int vec3)
    {
      return new Vector2Int(vec3.x, vec3.y);
    }


    public static Vector3Int FromFlatIndex(int xmax, int ymax, int idx)
    {
      int frame_sz = xmax * ymax;
      return new Vector3Int
      {
        x = (idx % frame_sz) % xmax,
        y = (idx % frame_sz) / xmax,
        z = idx / frame_sz
      };
    }

    public static int ToFlatIndex(this Vector3Int coord, int xmax, int ymax)
    {
      return coord.x + coord.y * xmax + coord.z * xmax * ymax;
    }


    public static bool IsNonNegative(this Vector3Int v)
    {
      return v.x >= 0 && v.y >= 0 && v.z >= 0;
    }

    public static bool IsInExtent(this Vector3Int coord, Vector3Int ext)
    {
      return coord.IsNonNegative()
          && coord.x < ext.x
          && coord.y < ext.y
          && coord.z < ext.z;
    }


    // TODO : IntVec3.RasterLineTo()

  }


  public static class Planes
  {
    public static readonly Plane XY = new Plane(Axis3D.Z, 0f);
    public static readonly Plane XZ = new Plane(Axis3D.Y, 0f);
    public static readonly Plane ZY = new Plane(Axis3D.X, 0f);
  }


  public static class Vec2D
  {

    public static readonly Vector2 Zero     = Axis2D.Origin;
    public static readonly Vector2 One      = new Vector2(1f, 1f);

    public static readonly Vector2 Half     = new Vector2(0.5f, 0.5f);
    public static readonly Vector2 Infinity = new Vector2(Mathf.Infinity, Mathf.Infinity);
    public static readonly Vector2 NaN      = new Vector2(float.NaN, float.NaN);

    public static readonly Vector2 Right    = new Vector2(1f, 0f);
    public static readonly Vector2 Up       = new Vector2(0f, 1f);

    public static readonly Vector2 Left     = new Vector2(-1f, 0f);
    public static readonly Vector2 Down     = new Vector2(0f, -1f);



    public static Vector2 RandomOnUnitCircle()
    {
      // `UnityEngine.Random.insideUnitCircle` is NOT the same thing as this:
      float rads = Random.value * 2.0f * Mathf.PI;
      return new Vector2(Mathf.Cos(rads), Mathf.Sin(rads));
    }


    public static Vector2 SinusoidalLoop(Vector2 a, Vector2 b, float t)
    {
      return Vector2.LerpUnclamped(a, b, Floats.SinusoidalParameter(t));
    }


    public static Vector2 SmoothStepLerp(Vector2 a, Vector2 b, float t)
    {
      return Vector2.LerpUnclamped(a, b, Floats.SmoothStepParameter(t));
    }


    public static Vector2Int North(this Vector2Int cell)
    {
      return new Vector2Int(cell.x, cell.y + 1);
    }

    public static Vector2Int East(this Vector2Int cell)
    {
      return new Vector2Int(cell.x + 1, cell.y);
    }

    public static Vector2Int South(this Vector2Int cell)
    {
      return new Vector2Int(cell.x, cell.y - 1);
    }

    public static Vector2Int West(this Vector2Int cell)
    {
      return new Vector2Int(cell.x - 1, cell.y);
    }


    public static bool Approximately(this Vector2 a, Vector2 b)
    {
      return (a.x -= b.x) * a.x < Floats.EPSILON2
          && (a.y -= b.y) * a.y < Floats.EPSILON2;
    }

    public static bool Approximately(this Vector2 a, Vector2 b, float epsilon)
    {
      return (a.x -= b.x) * a.x < epsilon * epsilon
          && (a.y -= b.y) * a.y < epsilon * epsilon;
    }

    public static bool IsZero(this Vector2 v)
    {
      return v.x * v.x < Floats.EPSILON2
          && v.y * v.y < Floats.EPSILON2;
    }


    public static bool ComponentSignsEqual(this Vector2 v)
    {
      return (v.x * v.y).IsPositive();
    }

    public static bool IsClockwiseOf(this Vector2 v_ccw, Vector2 v_cw)
    {
      return CrossProduct(v_cw, v_ccw).IsNegative();
    }


    public static Vector2 Clamped01(this Vector2 v)
    {
      float len = v.x * v.x + v.y * v.y;

      if (len > 1f)
      {
        len = 1f / Mathf.Sqrt(len);
        return v * len;
      }

      return v;
    }


    public static void Clamp(ref this Vector2 v, Vector2 min, Vector2 max)
    {
      v.x = Mathf.Clamp(v.x, min.x, max.x);
      v.y = Mathf.Clamp(v.y, min.y, max.y);
    }

    public static void Clamp(ref this Vector2 v, in Rect bounds)
    {
      v.Clamp(bounds.min, bounds.max);
    }

    public static void Clamp(ref this Vector2 v, float max)
    {
      float magn = v.sqrMagnitude;
      if (magn > max * max)
      {
        magn = 1f / Mathf.Sqrt(magn) * max;
        v.x *= magn;
        v.y *= magn;
      }
    }


    public static Vector2 Clamped(this Vector2 v, Vector2 min, Vector2 max)
    {
      v.Clamp(min, max);
      return v;
    }

    public static Vector2 Clamped(this Vector2 v, in Rect bounds)
    {
      v.Clamp(bounds.min, bounds.max);
      return v;
    }

    public static Vector2 Clamped(this Vector2 v, float max)
    {
      v.Clamp(max);
      return v;
    }


    public static Vector2 Abs(this Vector2 v)
    {
      return new Vector2(Mathf.Abs(v.x),
                          Mathf.Abs(v.y));
    }


    public static float CrossProduct(this Vector2 lhs, Vector2 rhs)
    {
      return lhs.x * rhs.y - lhs.y * rhs.x;
    }


    public static float NormalAngleRads(this Vector2 n) // SIGNED ANGLE! Assumes n is normal.
    {
      // assumes `n` is already normalized!
      float rads = (float)System.Math.Acos(n.x);

      if (n.y < 0.0f)
        return -1 * rads;

      return rads;
    }

    public static float NormalAngle(this Vector2 n) // SIGNED ANGLE! Assumes n is normal.
    {
      // assumes `n` is already normalized!
      float rads = (float)System.Math.Acos(n.x);

      if (n.y < 0.0f)
        return -1 * rads * Mathf.Rad2Deg;

      return rads * Mathf.Rad2Deg;
    }

    public static void RotateRadians(ref this Vector2 v, float rads)
    {
      double sin = -rads; // negate direction so rotation is more user-intuitive

      double cos = System.Math.Cos(sin);
      sin = System.Math.Sin(sin);

      double x = v.x; // necessary to save original x value
      v.x = (float)(x * cos + v.y * sin);
      v.y = (float)(x * -sin + v.y * cos);
    }

    public static Vector2 RotatedRadians(this Vector2 v, float rads)
    {
      // rotate the local variable v in-place, then return a copy:
      v.RotateRadians(rads);
      return v;
    }

    public static void Rotate(ref this Vector2 v, float degrees)
    {
      v.RotateRadians(degrees * Mathf.Deg2Rad);
    }

    public static Vector2 Rotated(this Vector2 v, float degrees)
    {
      v.RotateRadians(degrees * Mathf.Deg2Rad);
      return v;
    }


    public static void LerpTo(ref this Vector2 from, Vector2 to, float t)
    {
      from = Vector2.LerpUnclamped(from, to, t);
    }

    public static Vector3 LerpedTo(this Vector2 from, Vector2 to, float t)
    {
      return Vector2.LerpUnclamped(from, to, t);
    }


    public static Vector2 MidpointTo(this Vector2 from, Vector2 to)
    {
      return (from + to) / 2.0f;
    }

    public static Vector2 AveragePoint(this IEnumerable<Vector2> vs)
    {
      var v0 = Zero;
      int count = 0;

      foreach (var v in vs)
      {
        v0 += v;
        ++count;
      }

      if (count > 0)
        return v0 / count;

      return v0;
    }

  }


  public static class Vec3D
  {

    public static readonly Vector3 Zero     = Axis3D.Origin;
    public static readonly Vector3 One      = new Vector3(1f, 1f, 1f);

    public static readonly Vector3 Half     = new Vector3(0.5f, 0.5f, 0.5f);
    public static readonly Vector3 Infinity = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
    public static readonly Vector3 NaN      = new Vector3(float.NaN, float.NaN, float.NaN);

    public static readonly Vector3 Right    = new Vector3(1f, 0f, 0f);
    public static readonly Vector3 Up       = new Vector3(0f, 1f, 0f);
    public static readonly Vector3 Forward  = new Vector3(0f, 0f, 1f);

    public static readonly Vector3 Left     = new Vector3(-1f, 0f, 0f);
    public static readonly Vector3 Down     = new Vector3(0f, -1f, 0f);
    public static readonly Vector3 Backward = new Vector3(0f, 0f, -1f);


    public static Vector3 RandomDirectionInPlane(Plane plane)
    {
      throw new System.NotImplementedException();
    }


    public static Vector3 SinusoidalLoop(in Vector3 a, in Vector3 b, float t)
    {
      return Vector3.LerpUnclamped(a, b, Floats.SinusoidalParameter(t));
    }


    public static Vector3 SmoothStepLerp(in Vector3 a, in Vector3 b, float t)
    {
      return Vector3.LerpUnclamped(a, b, Floats.SmoothStepParameter(t));
    }


    public static bool Approximately(this Vector3 a, in Vector3 b)
    {
      return (a.x -= b.x) * a.x < Floats.EPSILON2
          && (a.y -= b.y) * a.y < Floats.EPSILON2
          && (a.z -= b.z) * a.z < Floats.EPSILON2;
    }

    public static bool Approximately(this Vector3 a, in Vector3 b, float epsilon)
    {
      epsilon *= epsilon;

      return (a.x -= b.x) * a.x < epsilon
          && (a.y -= b.y) * a.y < epsilon
          && (a.z -= b.z) * a.z < epsilon;
    }

    public static bool IsZero(in this Vector3 v)
    {
      return v.x * v.x < Floats.EPSILON2
          && v.y * v.y < Floats.EPSILON2
          && v.z * v.z < Floats.EPSILON2;
    }

    public static bool IsZero(in this Vector3 v, float epsilon)
    {
      epsilon *= epsilon;

      return v.x * v.x < epsilon
          && v.y * v.y < epsilon
          && v.z * v.z < epsilon;
    }


    public static void Squeeze(ref this Vector3 v)
    {
      v = new Vector3(Floats.Squeezed(v.x),
                      Floats.Squeezed(v.y),
                      Floats.Squeezed(v.z));
    }

    public static Vector3 Squeezed(in this Vector3 v)
    {
      return new Vector3(Floats.Squeezed(v.x),
                         Floats.Squeezed(v.y),
                         Floats.Squeezed(v.z));
    }


    public static void Clamp(ref this Vector3 v, float min, float max)
    {
      v.x = v.x.Clamp(min, max);
      v.y = v.y.Clamp(min, max);
      v.z = v.z.Clamp(min, max);
    }

    public static void Clamp(ref this Vector3 v, in Vector3 min, in Vector3 max)
    {
      v.x = v.x.Clamp(min.x, max.x);
      v.y = v.y.Clamp(min.y, max.y);
      v.z = v.z.Clamp(min.z, max.z);
    }

    public static void Clamp(ref this Vector3 v, in Rect bounds)
    {
      v.x = v.x.Clamp(bounds.xMin, bounds.xMax);
      v.y = v.y.Clamp(bounds.yMin, bounds.yMax);
      // intentionally do not touch the Z-coordinate;
    }

    public static void Clamp(ref this Vector3 v, in Bounds bounds)
    {
      v.Clamp(bounds.min, bounds.max);
    }


    public static Vector3 Clamped(this Vector3 v, float min, float max)
    {
      v.Clamp(min, max);
      return v;
    }

    public static Vector3 Clamped(this Vector3 v, in Vector3 min, in Vector3 max)
    {
      v.Clamp(min, max);
      return v;
    }

    public static Vector3 Clamped(this Vector3 v, in Rect bounds)
    {
      v.Clamp(bounds);
      return v;
    }

    public static Vector3 Clamped(this Vector3 v, in Bounds bounds)
    {
      v.Clamp(bounds.min, bounds.max);
      return v;
    }


    public static void FlattenXZ(ref this Vector3 v)
    {
      v.y = 0f;
    }

    public static Vector3 FlattenedXZ(in this Vector3 v)
    {
      return new Vector3(v.x, 0f, v.z);
    }


    public static void Rotate(ref this Vector3 v, in Quaternion rot, in Quaternion inv)
    {
      var vq = rot * new Quaternion(v.x, v.y, v.z, 0f) * inv;
      v.x = vq.x;
      v.y = vq.y;
      v.z = vq.z;
    }

    public static void Rotate(ref this Vector3 v, in Quaternion rot)
    {
      Rotate(ref v, in rot, Quaternion.Inverse(rot));
    }


    public static void Rotate(ref this Vector3 v, in Vector3 axis, float degrees)
    {
      var q = Quaternion.AngleAxis(degrees, axis);
      Rotate(ref v, in q, Quaternion.Inverse(q));
    }

    public static Vector3 Rotated(this Vector3 v, in Quaternion rot)
    {
      Rotate(ref v, in rot);
      return v;
    }

    public static Vector3 Rotated(this Vector3 v, in Vector3 axis, float degrees)
    {
      Rotate(ref v, in axis, degrees);
      return v;
    }


    public static float AngleBetweenRads(in this Vector3 from, in Vector3 to)
    {
      float cosine = Mathf.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
      if (cosine < Vector3.kEpsilon)
        return 0.0f;

      cosine = Mathf.Clamp(Vector3.Dot(from, to) / cosine, -1.0f, 1.0f);

      return Mathf.Acos(cosine);
    }

    public static float AngleBetween(in this Vector3 from, in Vector3 to)
    {
      return AngleBetweenRads(in from, in to) * Mathf.Rad2Deg;
    }


    public static Vector3 CrossedWith(in this Vector3 lhs, in Vector3 rhs)
    {
      return new Vector3(lhs.y * rhs.z - lhs.z * rhs.y,
                          lhs.z * rhs.x - lhs.x * rhs.z,
                          lhs.x * rhs.y - lhs.y * rhs.x);
    }


    public static bool TryGetSignedAngleTo(in this Vector3 from, in Vector3 to,
                                                                out Vector3 axis,
                                                                out float degrees)
    {
      axis = Vector3.Cross(from, to);
      degrees = 0.0f;

      if (axis.IsZero())
        return false;

      degrees = Mathf.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
      degrees = Mathf.Acos(Mathf.Clamp(Vector3.Dot(from, to) / degrees, -1.0f, 1.0f)) * Mathf.Rad2Deg;

      return !degrees.IsZero();
    }


    public static void LerpTo(ref this Vector3 from, in Vector3 to, float t)
    {
      from = Vector3.LerpUnclamped(from, to, t);
    }

    public static Vector3 LerpedTo(in this Vector3 from, in Vector3 to, float t)
    {
      return Vector3.LerpUnclamped(from, to, t);
    }



    public static Vector3 MidpointTo(in this Vector3 a, in Vector3 b)
    {
      return (a + b) / 2.0f;
    }

    public static Vector3 AveragePoint(IEnumerable<Vector3> vs)
    {
      var v0 = Zero;
      int count = 0;

      foreach (var v in vs)
      {
        v0 += v;
        ++count;
      }

      if (count > 0)
        return v0 / count;

      return v0;
    }

    public static Vector3 AveragePoint(params Vector3[] vs)
    {
      return AveragePoint((IEnumerable<Vector3>)vs);
    }


    public static float MagnitudeClamped(ref this Vector3 v, float max)
    {
      float magn = v.magnitude;

      if (magn > max)
      {
        magn = 1.0f / magn * max;
        v.Set(v.x * magn,
              v.y * magn,
              v.z * magn);
        return max;
      }

      return magn;
    }


    public static float SqrMagnitudeXZ(in this Vector3 v)
    {
      return v.x * v.x + v.z * v.z;
    }

    public static float MagnitudeXZ(in this Vector3 v)
    {
      return Mathf.Sqrt(v.x * v.x + v.z * v.z);
    }

  }


}