/**
@file   PyroDK/Core/Attributes/RequireInterfaceAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-10

@brief
  Defines the PropertyAttribute [RequireInterface(type)], which allows
  serialized `UnityEngine.Object` fields to enforce only a particular C#
  interface type.
**/

using UnityEngine;


namespace PyroDK
{

  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public sealed class RequireInterfaceAttribute : PropertyAttribute
  {
    public System.Type  Interface         = null;
    public string       DeferredInterface = null;


    public bool HighlightMissing = false;


    public RequireInterfaceAttribute(System.Type iface)
    {
      Interface = iface;
    }

    public RequireInterfaceAttribute(string iface_full_name)
    {
      if (!Assemblies.FindType(iface_full_name, out Interface))
      {
        DeferredInterface = iface_full_name;
      }
    }

  }

}