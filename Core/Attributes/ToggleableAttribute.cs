/**
@file   PyroDK/Core/Attributes/ToggleableAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  Marks a serialized field to be toggleable.

  Currently supports:
    - UnityEngine.Pose
**/


namespace PyroDK
{

  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class ToggleableAttribute : System.Attribute
  {
  }

}