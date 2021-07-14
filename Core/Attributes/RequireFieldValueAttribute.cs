/**
@file   PyroDK/Core/Attributes/RequireFieldValueAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2021-04-30

@brief
  Inspector property attribute that warns the user if a certain field's
  value is not equal to the one specified in the constructor.
**/

using UnityEngine;


namespace PyroDK
{

  [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
  public sealed class RequireFieldValueAttribute : PropertyAttribute
  {

    public readonly string FieldName;
    public readonly object RequiredValue;

    public Color32 WarningColor = RequiredReferenceAttribute.DEFAULT_COLOR;


    public RequireFieldValueAttribute(string field_name, object required_value)
    {
      FieldName = field_name;
      RequiredValue = required_value;
    }

  }

}