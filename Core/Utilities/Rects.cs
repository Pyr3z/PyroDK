/**
@file   PyroDK/Core/Utilities/Rects.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-06

@brief
  Provides utilities for the `UnityEngine.Rect` struct.
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{

  public static class Rects
  {

    public static readonly Rect Zero = new Rect(0.0f, 0.0f, 0.0f, 0.0f);


    public static void OrderMinMax(ref this Rect rect)
    {
      if (rect.xMin > rect.xMax)
      {
        float swap = rect.xMin;
        rect.xMin = rect.xMax;
        rect.xMax = swap;
      }
      if (rect.yMin > rect.yMax)
      {
        float swap = rect.yMin;
        rect.yMin = rect.yMax;
        rect.yMax = swap;
      }
    }
    public static Rect MinMaxOrdered(this Rect rect)
    {
      OrderMinMax(ref rect);
      return rect;
    }
    public static Rect MakeFromCorners(Vector2 pt_a, Vector2 pt_c)
    {
      if (pt_a.x > pt_c.x)
      {
        float swap = pt_a.x;
        pt_a.x = pt_c.x;
        pt_c.x = swap;
      }

      if (pt_a.y > pt_c.y)
      {
        float swap = pt_a.y;
        pt_a.y = pt_c.y;
        pt_c.y = swap;
      }

      return new Rect(pt_a, pt_c - pt_a);
    }


    public static Vector2 PickRandomPoint(in this Rect rect)
    {
      return new Vector2
      (
        Random.value * rect.width  + rect.xMin,
        Random.value * rect.height + rect.yMin
      );
    }

    
    public static bool IsZero(in this Rect r)
    {
      return r.position.IsZero() && r.size.IsZero();
    }

    public static bool Approximately(in this Rect a, in Rect b)
    {
      return a.position.Approximately(b.position)
          && a.size.Approximately(b.size);
    }

    public static void Expand(ref this Rect rect, float up, float down, float left, float right)
    {
      rect.Set(rect.x - left,
               rect.y - down,
               rect.width + left + right,
               rect.height + down + up);
    }

    public static void Expand(ref this Rect rect, float amount)
    {
      rect.Expand(amount, amount, amount, amount);
    }

    public static void Expand(ref this Rect rect, float dx, float dy)
    {
      rect.Expand(dy, dy, dx, dx);
    }

    public static void ExpandX(ref this Rect rect, float dx)
    {
      if (dx > 0f)
        rect.xMax += dx;
      else
        rect.xMin += dx;
    }

    public static void ExpandY(ref this Rect rect, float dy)
    {
      if (dy > 0f)
        rect.yMax += dy;
      else
        rect.yMin += dy;
    }


    public static Rect Expanded(this Rect rect, float up, float down, float left, float right)
    {
      rect.Expand(up, down, left, right);
      return rect;
    }

    public static Rect Expanded(this Rect rect, float amount)
    {
      rect.Expand(amount, amount, amount, amount);
      return rect;
    }

    public static Rect Expanded(this Rect rect, float dx, float dy)
    {
      rect.Expand(dy, dy, dx, dx);
      return rect;
    }

    public static Rect ExpandedX(this Rect rect, float dx)
    {
      if (dx > 0.0f)
        rect.xMax += dx;
      else
        rect.xMin += dx;

      return rect;
    }

    public static Rect ExpandedY(this Rect rect, float dy)
    {
      if (dy > 0.0f)
        rect.yMax += dy;
      else
        rect.yMin += dy;
      
      return rect;
    }


    public static Rect ExpandedWidth(this Rect rect, float dw)
    {
      if (!dw.IsZero())
      {
        rect.x -= dw * 0.5f;
        rect.width += dw;
      }

      return rect;
    }


    public static Rect Scaled(this Rect rect, float factor)
    {
      factor -= 1.0f;
      float dx = (rect.width  * factor) / 2.0f;
      float dy = (rect.height * factor) / 2.0f;

      rect.Expand(dx, dy);

      return rect;
    }

    public static void Encapsulate(this Rect rect, in Rect other)
    {
      rect.min = Vector2.Min(rect.min, other.min);
      rect.max = Vector2.Max(rect.max, other.max);
    }

    public static Rect Encapsulated(this Rect rect, in Rect other)
    {
      return Rect.MinMaxRect(Mathf.Min(rect.xMin, other.xMin),
                             Mathf.Min(rect.yMin, other.yMin),
                             Mathf.Max(rect.xMax, other.xMax),
                             Mathf.Max(rect.yMax, other.yMax));
    }

    public static Rect Moved(this Rect rect, in Vector2 dxdy)
    {
      rect.x += dxdy.x;
      rect.y += dxdy.y;
      return rect;
    }

    public static Rect MovedX(this Rect rect, float dx)
    {
      rect.x += dx;
      return rect;
    }

    public static Rect MovedY(this Rect rect, float dy)
    {
      rect.y += dy;
      return rect;
    }

    public static void Move(ref this Rect rect, in Vector2 dxdy)
    {
      rect.x += dxdy.x;
      rect.y += dxdy.y;
    }

    public static void MoveX(ref this Rect rect, float dx)
    {
      rect.x += dx;
    }

    public static void MoveY(ref this Rect rect, float dy)
    {
      rect.y += dy;
    }

    public static Rect CenteredAt(this Rect rect, Vector2 center)
    {
      rect.center = center;
      return rect;
    }

    public static Rect OriginedAt(this Rect rect, Vector2 origin)
    {
      rect.position = origin;
      return rect;
    }


    public static void SplitHorizontal(in this Rect rect, float left_width, out Rect left, out Rect right, float padding = 2.0f)
    {
      left = right = rect;

      left.width = left_width;
      right.xMin += left.width + padding;
    }



    public static void AdvanceLine(ref this Rect rect)
    {
      #if UNITY_EDITOR
      rect.y += UnityEditor.EditorGUIUtility.singleLineHeight + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
      #else
      rect.y += 20.0f;
      #endif
    }

    public static void AdvanceLines(ref this Rect rect, float lines)
    {
      #if UNITY_EDITOR
      rect.y += lines * (UnityEditor.EditorGUIUtility.singleLineHeight + UnityEditor.EditorGUIUtility.standardVerticalSpacing);
      #else
      rect.y += lines * 20.0f;
      #endif
    }


    public static Rect AdvancedLine(this Rect rect)
    {
      rect.AdvanceLine();
      return rect;
    }

    public static Rect AdvancedLines(this Rect rect, float lines)
    {
      rect.AdvanceLines(lines);
      return rect;
    }



    public static void Indent(ref this Rect rect)
    {
      #if UNITY_EDITOR
      rect.xMin += UnityEditor.EditorGUI.indentLevel * 15.0f;
      #endif
    }

    public static void AdvanceLabelWidth(ref this Rect rect)
    {
      #if UNITY_EDITOR
      rect.xMin += UnityEditor.EditorGUIUtility.labelWidth + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
      #endif
    }

  }


  public static class IntRects
  {

    public static bool IsZero(in this RectInt bounds)
    {
      return bounds.Equals(default);
    }

    public static Vector2Int CenterCell(in this RectInt bounds)
    {
      return new Vector2Int(bounds.x + bounds.width / 2,
                            bounds.y + bounds.height / 2);
    }

    public static Vector2Int RandomCell(in this RectInt bounds)
    {
      return new Vector2Int(bounds.x + (int)(bounds.width * Random.value),
                            bounds.y + (int)(bounds.height * Random.value));
    }

    public static float SqrMagnitude(in this RectInt bounds)
    {
      return bounds.size.sqrMagnitude;
    }

    public static float Magnitude(in this RectInt bounds)
    {
      return bounds.size.magnitude;
    }

    public static bool Contains(in this RectInt outer, in RectInt inner)
    {
      return  outer.Contains(inner.min) &&
              outer.Contains(inner.max - IntVec2D.One);
    }

    public static bool Contains(in this RectInt bounds, in CellLine line)
    {
      return bounds.Contains(line.A) && bounds.Contains(line.B);
    }

    public static bool Overlaps(in this RectInt self, int xmin, int xmax, int ymin, int ymax)
    {
      // semantically equivalent to the overload that takes a RectInt as parameter.
      return  xmin < self.xMax &&
              self.xMin < xmax &&
              ymin < self.yMax &&
              self.yMin < ymax;
    }
    public static bool Overlaps(in this RectInt self, Vector2Int center, int extent)
    {
      return  center.x - extent < self.xMax &&
              self.xMin < center.x + extent &&
              center.y - extent < self.yMax &&
              self.yMin < center.y + extent;
    }


    public static Vector2Int ClampCell(in this RectInt bounds, Vector2Int cell)
    {
      if (cell.x < bounds.xMin)
        cell.x = bounds.xMin;
      else if (cell.x >= bounds.xMax)
        cell.x = bounds.xMax - 1;

      if (cell.y < bounds.yMin)
        cell.y = bounds.yMin;
      else if (cell.y >= bounds.yMax)
        cell.y = bounds.yMax - 1;

      return cell;
    }

    public static void Encapsulate(ref this RectInt bounds, Vector2Int cell)
    {
      if (cell.x < bounds.xMin)
        bounds.xMin = cell.x;
      else if (cell.x >= bounds.xMax)
        bounds.xMax = cell.x + 1;

      if (cell.y < bounds.yMin)
        bounds.yMin = cell.y;
      else if (cell.y >= bounds.yMax)
        bounds.yMax = cell.y + 1;
    }

    public static void Encapsulate(ref this RectInt bounds, in RectInt other)
    {
      bounds.Encapsulate(other.min);
      bounds.Encapsulate(other.max - IntVec2D.One);
    }

    public static RectInt Contracted(in this RectInt rect, int amount)
    {
      if (amount == 0)
        return rect;

      return new RectInt( rect.x + amount,
                          rect.y + amount,
                          rect.width - 2 * amount,
                          rect.height - 2 * amount);
    }

    public static RectInt Scaled(in this RectInt rect, float scale)
    {
      // scales from center
      float magn = (rect.width + rect.height) / 2.0f;
      return Contracted(in rect, (int)(magn - magn * scale));
    }

    public static RectInt Intersected(this RectInt a, in RectInt b)
    {
      if (!a.Overlaps(b))
        return new RectInt();

      a.SetMinMax(new Vector2Int(Mathf.Max(a.xMin, b.xMin), Mathf.Max(a.yMin, b.yMin)),
                  new Vector2Int(Mathf.Min(a.xMax, b.xMax), Mathf.Min(a.yMax, b.yMax)));
      return a;
    }

    public static int CalcIndex(in this RectInt bounds, Vector2Int coord) // assumes non-inverted bounds
    {
      if (bounds.width <= 0 || bounds.width <= coord.x || bounds.height <= coord.y)
        return int.MaxValue; // ensures `if (idx < array.Length)` evaluates false

      //int idx = (coord.y - bounds.y) * bounds.width + (coord.x - bounds.x);
      int idx = coord.y * bounds.width + coord.x;

      if (idx < 0)
        return int.MaxValue;
      return idx;
    }

    public static Vector2Int CalcCell(in this RectInt bounds, int idx)
    {
      return IntVec2D.FromIndex(bounds.width, idx);
    }

    public static Vector2 NormalizeCoord(in this RectInt rect, Vector2Int coord)
    {
      return new Vector2( Mathf.InverseLerp(rect.xMin, rect.xMax, coord.x),
                          Mathf.InverseLerp(rect.yMin, rect.yMax, coord.y));
    }

    public static IEnumerable<Vector2Int> AllCells(this RectInt rect)
    {
      int x_max = rect.xMax;
      int y_max = rect.yMax;

      for (int x = rect.xMin; x < x_max; ++x)
      {
        for (int y = rect.yMin; y < y_max; ++y)
          yield return new Vector2Int(x, y);
      }
    }

    public static IEnumerable<Vector2Int> BorderCells(this RectInt rect, int thickness = 1)
    {
      if (thickness == 0)
        yield break;

      if (thickness < 0)
        rect = rect.Contracted(-1);

    Recurse:

      int x_min = rect.xMin;
      int y_min = rect.yMin;
      int x_max = rect.xMax;
      int y_max = rect.yMax;

      var curr1 = new Vector2Int(x_min, y_min);
      var curr2 = new Vector2Int(x_min, y_max - 1);

      for (; curr1.x < x_max; ++curr1.x, ++curr2.x)
      {
        yield return curr1; yield return curr2;
      }

      curr1.x = x_min;
      curr1.y = y_min + 1;

      curr2.x = x_max - 1;
      curr2.y = y_min + 1;

      --y_max;
      for (; curr1.y < y_max; ++curr1.y, ++curr2.y)
      {
        yield return curr1; yield return curr2;
      }

      if (thickness > 1)
      {
        rect = rect.Contracted(1);
        --thickness;
        goto Recurse;
      }
      else if (thickness < -1)
      {
        rect = rect.Contracted(-1);
        ++thickness;
        goto Recurse;
      }
    }

    public static IEnumerable<int> SubRectIndices(this RectInt bounds, RectInt subrect)
    {
      int w = bounds.width;

      if (w <= 0)
      {
        yield break;
      }
      else if (bounds.Contains(subrect))
      {
        foreach (var cell in AllCells(subrect))
        {
          yield return cell.y * w + cell.x;
        }
      }
      else
      {
        int x_min = Mathf.Max(subrect.xMin, bounds.xMin);
        int y_min = Mathf.Max(subrect.yMin, bounds.yMin);

        int x_max = Mathf.Min(subrect.xMax, bounds.xMax);
        int y_max = Mathf.Min(subrect.yMax, bounds.yMax);

        for (int x = x_min; x < x_max; ++x)
        {
          for (int y = y_min; y < y_max; ++y)
          {
            yield return y * w + x;
          }
        }
      }
    }

  }

}