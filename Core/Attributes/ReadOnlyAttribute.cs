/**
@file   PyroDK/Core/Attributes/ReadOnlyAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-11

@brief
  Defines the [ReadOnly] C# Attribute that greys out serialized fields of
  custom `UnityEngine.MonoBehaviour` or `UnityEngine.ScriptableObject` classes
  that are being viewed in the Inspector -- rendering the marked field(s) as
  read-only.
**/

using UnityEngine;


namespace PyroDK
{

  using Type = System.Type;


  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class ReadOnlyAttribute : PropertyAttribute
  {
    public string Label   { get; set; }
    public string Tooltip { get; set; }

    public Type   CustomDrawerType    { get; set; }
    public string CustomDrawerString  { get; set; }
    public bool   CustomDrawerProvided => CustomDrawerType != null || CustomDrawerString != null;



    public ReadOnlyAttribute(System.Type custom_drawer = null)
    {
      CustomDrawerType    = custom_drawer;
      CustomDrawerString  = null;
    }

    public ReadOnlyAttribute(string custom_drawer)
    {
      CustomDrawerType    = null;
      CustomDrawerString  = custom_drawer;
    }

  }

}