/**
@file   PyroDK/Core/Attributes/GameObjectTagAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  Restricts a string field to be represented as a GameObject tag
  in the Inspector.
**/


namespace PyroDK
{

  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class GameObjectTagAttribute : UnityEngine.PropertyAttribute
  {
  }

}