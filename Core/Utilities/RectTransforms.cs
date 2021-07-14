/**
@file   PyroDK/Core/Utilities/RectTransforms.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-06

@brief
  Provides utilities for the `UnityEngine.RectTransform` GameObject Component.
**/

using UnityEngine;


namespace PyroDK
{

  public static class RectTransforms
  {

    public static Rect GetWorldRectXY(this RectTransform rtrans)
    {
      return rtrans.rect.CenteredAt(rtrans.position);
    }

  }

}