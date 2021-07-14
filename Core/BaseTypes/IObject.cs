/**
@file   PyroDK/Core/BaseTypes/IObject.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-07

@brief
  Defines the IObject interface.
**/

using UnityEngine;


namespace PyroDK
{

  public interface IObject
  {
    #if USE_DEPRECATED_ADDRESSES
    string Address { get; } // TODO consider replacing with Addressables?
    internal bool UpdateAddress(string addr);
    #endif // USE_DEPRECATED_ADDRESSES

    // UnityEngine.Object facade:
    string      name      { get; set; }
    HideFlags   hideFlags { get; set; }
    int         GetInstanceID();
  }

}