/**
@file   PyroDK/Game2D/DataTypes/HullEdge2D.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-24

@brief
  Defines a 2D line for representation in integral grids.
**/

using UnityEngine;


namespace PyroDK.Game2D
{

  [System.Serializable]
  public class HullEdge2D
  {

    public static HullEdge2D MakeFromEdgeVector(Vector2 pt_start, Vector2 v_edge)
    {
      return new HullEdge2D(pt_start, v_edge);
    }
    public static HullEdge2D MakeFromEndPoints(Vector2 pt_start, Vector2 pt_end)
    {
      return new HullEdge2D(pt_start, pt_end - pt_start);
    }


    public static bool StrictlyEqual(HullEdge2D lhs, HullEdge2D rhs)
    {
      return lhs.m_StartPt == rhs.m_StartPt && lhs.m_EdgeV == rhs.m_EdgeV;
    }
    public static bool BoundsEqual(HullEdge2D lhs, HullEdge2D rhs)
    {
      return lhs.m_Bounds == rhs.m_Bounds;
    }



    public Vector2  Start   => m_StartPt;
    public Vector2  End     => m_StartPt + m_EdgeV;
    public Vector2  Edge    => m_EdgeV;
    public Rect     Bounds  => m_Bounds;


    [SerializeField] [ReadOnly]
    private Vector2 m_StartPt, m_EdgeV;
    [SerializeField] [ReadOnly]
    private Rect    m_Bounds;



    private HullEdge2D(Vector2 pt_start, Vector2 v_edge)
    {
      m_StartPt = pt_start;
      m_EdgeV   = v_edge;
      m_Bounds  = Rects.MakeFromCorners(pt_start, pt_start + v_edge);

      if (v_edge.ComponentSignsEqual()) // Quadrants I and III
      {
        // Note: By correctly utilizing IEEE negative 0.0f,
        //       Q-III is equally as inclusive as Q-I.
        //m_ConvexMaximum = new Vector2(m_StartPt.x, m_StartPt.y + m_EdgeV.y);
      }
      else // Quadrants II and IV
      {
        //m_ConvexMaximum = new Vector2(m_StartPt.x + m_EdgeV.x, m_StartPt.y);
      }
    }



    public bool HasArea()
    {
      return !m_EdgeV.IsZero();
    }

    public bool IsConvexCandidate(Vector2 p)
    {
      var pv = p - m_StartPt;
      return m_EdgeV.IsClockwiseOf(pv) && m_Bounds.Contains(p);
    }



    [System.Diagnostics.Conditional("DEBUG")]
    public void DebugDraw(Color color, float duration = 10.0f)
    {
      Debug.DrawRay(m_StartPt, Edge, color, duration, false);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void GizmoDraw()
    {
      Gizmos.DrawRay(m_StartPt, Edge);
    }

  } // end class HullEdge2D.

}