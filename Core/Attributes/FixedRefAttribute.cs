/**
@file   PyroDK/Core/Attributes/FixedRefAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-10

@brief
  --
**/

using UnityEngine;


namespace PyroDK
{

  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public sealed class FixedRefAttribute : PropertyAttribute
  {
    public System.Type  Interface         = null;
    public string       DeferredInterface = null;


    public bool HighlightMissing = false;


    public FixedRefAttribute(System.Type iface)
    {
      Interface = iface;
    }

    public FixedRefAttribute(string iface_full_name)
    {
      if (!Assemblies.FindType(iface_full_name, out Interface))
      {
        DeferredInterface = iface_full_name;
      }
    }

  }

}