/**
@file   PyroDK/Core/BaseTypes/BaseComponent.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-07

@brief
  Defines the BaseComponent quasi-abstract class that all PyroDK "components"
  share in their inheritance hierarchy.
**/

#pragma warning disable CS0649, CS0414

using System.Collections;

using UnityEngine;
using UnityEngine.Events;


namespace PyroDK
{
  using Condition = System.Func<bool>;



  public abstract class BaseComponent : MonoBehaviour, IObject, IComponent
  {
    #region Static Utilities

    [System.Diagnostics.Conditional("DEBUG")]
    public static void DebugLogStatic(string message)
    {
      message.Log();
    }

    public static IEnumerator InvokeNextFrame(UnityAction action)
    {
      yield return new WaitForEndOfFrame();
      action();
    }

    public static IEnumerator InvokeNextFrameIf(UnityAction action, Condition condition)
    {
      yield return new WaitForEndOfFrame();

      if (condition())
        action();
    }

    public static IEnumerator InvokeNextFrameIf(UnityAction action, Condition condition, UnityAction else_action)
    {
      yield return new WaitForEndOfFrame();

      if (condition())
        action();
      else
        else_action();
    }

    public static IEnumerator InvokeInSeconds(UnityAction action, float s)
    {
      if (s < Floats.EPSILON)
      {
        action();
      }
      else
      {
        yield return new WaitForSeconds(s);
        action();
      }
    }

    #endregion Static Utilities


    #region Event Callback Methods

    [System.Diagnostics.Conditional("DEBUG")]
    public void DebugLog(string message)
    {
      message.Log(ctx: this);
    }


    public void SpawnLocal(SpawnPool pool)
    {
      if (pool)
        pool.SpawnAt(transform);
      else
        TSpy<SpawnPool>.LogMissingReference(this);
    }

    public void SpawnWorld(SpawnPool pool)
    {
      if (pool)
        pool.Spawn(gameObject);
      else
        TSpy<SpawnPool>.LogMissingReference(this);
    }



    public void ToggleSelf()
    {
      enabled = !enabled;
    }

    public void ToggleGameObject()
    {
      gameObject.SetActive(!gameObject.activeSelf);
    }



    public void DestroySelf(float in_seconds = 0f)
    {
      Destroy(this, in_seconds);
    }
    public void DestroyGameObject(float in_seconds = 0f)
    {
      Destroy(gameObject, in_seconds);
    }

    #endregion Event Callback Methods


    #if USE_DEPRECATED_ADDRESSES

    public string Address => m_ObjectAddress;

    [SerializeField] [ReadOnly]
    private string m_ObjectAddress;

    bool IObject.UpdateAddress(string addr)
    {
      if (addr.IsEmpty())
        addr = $"{base.ToString()} ({GetInstanceID()})";

      if (m_ObjectAddress.IsEmpty() || m_ObjectAddress != addr)
      {
        m_ObjectAddress = addr;
        return true;
      }

      return false;
    }

    public override string ToString()
    {
      return m_ObjectAddress;
    }

    #endif // USE_DEPRECATED_ADDRESSES

  } // end class BaseComponent

}
