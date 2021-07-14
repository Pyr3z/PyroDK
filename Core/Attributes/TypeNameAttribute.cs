/**
@file   PyroDK/Core/Attributes/TypeNameAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-18

@brief
  A PropertyAttribute for string fields that validates the string
  as the name of a runtime type.
**/

using UnityEngine;


namespace PyroDK
{

  public sealed class TypeNameAttribute : PropertyAttribute
  {
  }

}