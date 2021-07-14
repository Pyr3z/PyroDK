/**
@file   PyroDK/Core/BaseTypes/BaseAsset.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-07

@brief
  Defines the BaseAsset abstract class that all PyroDK "asset container"
  types share in their inheritance hierarchy.
**/

using UnityEngine;


namespace PyroDK
{

  public abstract class BaseAsset : ScriptableObject, IObject
  {
    #region Event Callback Methods

    [System.Diagnostics.Conditional("DEBUG")]
    public void DebugLog(string log)
    {
      Logging.Log(log, ctx: this);
    }

    #endregion Event Callback Methods


    #if USE_DEPRECATED_ADDRESSES

    public string Address => m_ObjectAddress;

    [SerializeField] [ReadOnly]
    private string m_ObjectAddress;

    bool IObject.UpdateAddress(string path)
    {
      if (path.IsEmpty())
        path = $"{base.ToString()} ({GetInstanceID()})";

      if (m_ObjectAddress.IsEmpty() || m_ObjectAddress != path)
      {
        m_ObjectAddress = path;
        return true;
      }

      return false;
    }

    public override string ToString()
    {
      return m_ObjectAddress;
    }

    #endif // USE_DEPRECATED_ADDRESSES

  } // end class BaseAsset

}