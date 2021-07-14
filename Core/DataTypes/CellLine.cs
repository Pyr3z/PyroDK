/**
@file   PyroDK/Core/DataTypes/CellLine.cs
@author Levi Perez (Pyr3z)
@author https://leviperez.dev
@date   2020-09-24

@brief
  Defines a 2D line for representation in integral grids.
**/

using System.Collections.Generic;
using System.Collections;

using UnityEngine;


namespace PyroDK
{

  [System.Serializable]
  public struct CellLine : IEnumerable<Vector2Int>, System.IEquatable<CellLine>
  {
    public bool   IsLine        => A != B;
    public int    SquareLength  => (B - A).sqrMagnitude;
    public float  Length        => (B - A).magnitude;

    public Vector2Int Vector    => (B - A);
    public Vector2Int Midpoint  => (A + B) / 2;


    public Vector2Int A, B;



    public CellLine(Vector2Int a, Vector2Int b)
    {
      A = a; B = b;
    }

    public CellLine(in CellLine other)
    {
      A = other.A;
      B = other.B;
    }



    public void Flip()
    {
      var swap = A;
      A = B;
      B = swap;
    }
    public CellLine Flipped()
    {
      return new CellLine(B, A);
    }


    public void Rotate(float degrees)
    {
      var dir = new Vector2(B.x - A.x, B.y - A.y);
      dir.Rotate(degrees);
      B = new Vector2Int(Mathf.RoundToInt((A.x + dir.x)), Mathf.RoundToInt(A.y + dir.y));
    }
    public CellLine Rotated(float degrees)
    {
      var rotated = new CellLine(in this);
      rotated.Rotate(degrees);
      return rotated;
    }


    public void Extend(float n = 1.0f)
    {
      var dir = new Vector2(B.x - A.x, B.y - A.y);
      dir.Normalize();
      int dx = Mathf.RoundToInt(dir.x * n);
      int dy = Mathf.RoundToInt(dir.y * n);
      A = new Vector2Int(A.x - dx, A.y - dy);
      B = new Vector2Int(B.x + dx, B.y + dy);
    }

    public CellLine Extended(float n = 1.0f)
    {
      var dir = new Vector2(B.x - A.x, B.y - A.y);
      dir.Normalize();
      int dx = Mathf.RoundToInt(dir.x * n);
      int dy = Mathf.RoundToInt(dir.y * n);
      return new CellLine(new Vector2Int(A.x - dx, A.y - dy),
                          new Vector2Int(B.x + dx, B.y + dy));
    }


    public void Clamp(in RectInt bounds)
    {
      A = bounds.ClampCell(A);
      B = bounds.ClampCell(B);
    }

    public CellLine Clamped(in RectInt bounds)
    {
      return new CellLine(bounds.ClampCell(A), bounds.ClampCell(B));
    }


    public CellLine Perpendicular()
    {
      var dir = (B - A);
      dir = new Vector2Int(dir.y / -2, dir.x / 2);
      var mid = (A + B) / 2;
      return new CellLine(mid - dir, mid + dir);
    }

    public CellLine SnappedHorizontal()
    {
      if (A.y == B.y) // already horizontal
        return new CellLine(in this);

      var dir = (B - A);
      dir.x = Mathf.RoundToInt(dir.x.SignNoZero() * dir.magnitude / 2);
      dir.y = 0;
      var mid = (A + B) / 2;
      return new CellLine(mid - dir, mid + dir);
    }

    public CellLine SnappedVertical()
    {
      if (A.x == B.x) // already vertical
        return new CellLine(in this);

      var dir = (B - A);
      dir.y = Mathf.RoundToInt(dir.y.SignNoZero() * dir.magnitude / 2);
      dir.x = 0;
      var mid = (A + B) / 2;
      return new CellLine(mid - dir, mid + dir);
    }

    public CellLine SnappedMajorAxis() // perfect diagonals are snapped to the horizontal
    {
      int dx = (B.x - A.x).Abs();
      int dy = (B.y - A.y).Abs();
      return (dx < dy) ? SnappedVertical() : SnappedHorizontal();
    }


    public void Append(Vector2Int v)
    {
      B += v;
    }
    public void Append(in CellLine other)
    {
      B += other.Vector;
    }

    public CellLine Appended(Vector2Int v)
    {
      return new CellLine(A, B + v);
    }
    public CellLine Appended(in CellLine other)
    {
      return new CellLine(A, B + other.Vector);
    }

    public void Prepend(Vector2Int v)
    {
      A -= v;
    }
    public void Prepend(in CellLine other)
    {
      A -= other.Vector;
    }

    public CellLine Prepended(Vector2Int v)
    {
      return new CellLine(A - v, B);
    }
    public CellLine Prepended(in CellLine other)
    {
      return new CellLine(A - other.Vector, B);
    }



    [System.Diagnostics.Conditional("DEBUG")]
    public void DebugDraw(Color color, float duration = 10f)
    {
      if (!IsLine) return;

      Debug.DrawLine(new Vector3(A.x, A.y, 0f), new Vector3(B.x, B.y, 0f), color, duration, false);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void GizmoDraw()
    {
      if (!IsLine) return;

      Gizmos.DrawLine(new Vector3(A.x, A.y, 0f), new Vector3(B.x, B.y, 0f));
    }


    public IEnumerator<Vector2Int> GetEnumerator() => IntVec2D.RasterLineTo(A, B).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()        => IntVec2D.RasterLineTo(A, B).GetEnumerator();


    public override int GetHashCode()
    {
      return Hashing.MakeHash(A, B);
    }
    public override bool Equals(object obj)
    {
      return (obj is CellLine cl) && Equals(in this, in cl);
    }
    public bool Equals(CellLine other)
    {
      return Equals(in this, in other);
    }
    public bool EqualsUnordered(in CellLine other)
    {
      return EqualsUnordered(in this, in other);
    }


    public static bool Equals(in CellLine a, in CellLine b)
    {
      return a.A == b.A && a.B == b.B;
    }
    public static bool EqualsUnordered(in CellLine a, in CellLine b)
    {
      return ( a.A == b.A && a.B == b.B ) ||
             ( a.A == b.B && a.B == b.A );
    }


    // operators:

    public static explicit operator bool (in CellLine self)
    {
      return self.IsLine;
    }

    public static bool operator == (in CellLine lhs, in CellLine rhs)
    {
      return EqualsUnordered(in lhs, in rhs);
    }
    public static bool operator != (in CellLine lhs, in CellLine rhs)
    {
      return !Equals(in lhs, in rhs);
    }


    // for delegate use:

    public static bool Longer(CellLine a, CellLine b)
    {
      return a.SquareLength > b.SquareLength;
    }
    public static bool Shorter(CellLine a, CellLine b)
    {
      return a.SquareLength < b.SquareLength;
    }

  } // end struct CellLine.


  //[System.Serializable]
  //public struct CellCircle : IEnumerable<Vector2Int>
  //{
  //  public Vector2Int  Center;
  //  public float       Radius;

  //  public CellCircle(Vector2Int center, float radius)
  //  {
  //    Center = center;
  //    Radius = radius;
  //  }

  //  public IEnumerator<Vector2Int> GetEnumerator() => IntVec2.RasterCircleFilled(Center, Radius).GetEnumerator();
  //  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  //}

}