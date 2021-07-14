/**
@file   PyroDK/Core/Utilities/Bounds3D.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-06

@brief
  Provides utilities for the `UnityEngine.Bounds` struct.
**/

using UnityEngine;


namespace PyroDK
{

  public static class Bounds3D
  {
    public static readonly Bounds XYPlane = new Bounds(Axis3D.Origin, Axis3D.XY * Mathf.Infinity);
    public static readonly Bounds XZPlane = new Bounds(Axis3D.Origin, Axis3D.XZ * Mathf.Infinity);
    public static readonly Bounds YZPlane = new Bounds(Axis3D.Origin, Axis3D.ZY * Mathf.Infinity);
  }

}
