/**
@file   PyroDK/Core/DataTypes/ObjectLookup.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-18

@brief
  A [Serializable] Lookup data structure that stores
  string-Object pairs for later lookup.
**/

#pragma warning disable CS0649

using UnityEngine;


namespace PyroDK
{
  using Type = System.Type;


  [System.Serializable]
  public sealed class ObjectLookup : Lookup<ObjectLookup.KVP, Object>
  {
    #region Public Static Section

    [System.Serializable]
    public sealed class KVP : SerialKVP<string, Object>
    {
    }


    public static ObjectLookup MakeStrict<T>(HashMapParams parms = default)
    {
      if (Logging.Assert(TSpy<T>.IsUnityObject, $"{TSpy<T>.LogName} is not a \"UnityObject\" type!"))
        return new ObjectLookup(parms);

      return new ObjectLookup(parms)
      {
        m_StrictType = TSpy<T>.SerialType
      };
    }

    #endregion Public Static Section


    public override Type ValueType => m_StrictType;

    [SerializeField]
    private SerialType m_StrictType = TSpy<Object>.SerialType;


    public ObjectLookup(HashMapParams parms = default) : base(parms)
    {
      _ = Validate();
    }

    protected override bool Validate()
    {
      if (!m_StrictType.AssignableTo<Object>())
      {
        $"ObjectLookup strict value type {m_StrictType} is not assignable to Object! Defaulting."
          .LogWarning(this);
        m_StrictType = TSpy<Object>.SerialType;
      }

      m_IsValidValue = ObeysStrictValueType;
      m_HashMap.SetValueEquals(ObjectsEqual);

      return base.Validate();
    }


    #region Private Section

    private bool ObeysStrictValueType(Object obj)
    {
      return !obj || m_StrictType.AssignableFrom(obj);
    }


    internal static bool ObjectsEqual(Object a, Object b)
    {
      return a == b;
    }

    internal static bool NotReferencable<T>()
    {
      #if DEBUG
      if (!TSpy<T>.IsReferencable)
      {
        $"{TSpy<T>.LogName} is not a \"Referencable\" type!"
          .LogWarning();
        return true;
      }

      return false;
      #else
      return !TSpy<T>.IsReferencable;
      #endif
    }

    #endregion Private Section

  } // end class ObjectLookup

}