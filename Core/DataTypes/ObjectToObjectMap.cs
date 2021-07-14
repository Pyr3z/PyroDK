/**
@file   PyroDK/Core/DataTypes/ObjectToObjectMap.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-18

@brief
  A generic asset type that stores string-Object pairs for later lookup.
**/

using UnityEngine;


namespace PyroDK
{

  [System.Serializable]
  public sealed class ObjectToObjectMap : SerialHashMap<ObjectToObjectMap.KVP, Object, Object>
  {
    #region Public Static Section

    [System.Serializable]
    public sealed class KVP : BaseSerialKVP<Object, Object>
    {
    }


    public static ObjectToObjectMap MakeStrictKeyType<T>(HashMapParams parms = default)
    {
      if (ObjectLookup.NotReferencable<T>())
        return new ObjectToObjectMap(parms);

      return new ObjectToObjectMap(parms)
      {
        m_KeyStrictType = TSpy<T>.SerialType
      };
    }

    public static ObjectToObjectMap MakeStrictValueType<T>(HashMapParams parms = default)
    {
      if (ObjectLookup.NotReferencable<T>())
        return new ObjectToObjectMap(parms);

      return new ObjectToObjectMap(parms)
      {
        m_ValueStrictType = TSpy<T>.SerialType
      };
    }

    public static ObjectToObjectMap MakeStrict<TKey, TValue>(HashMapParams parms = default)
    {
      SerialType keytype, valtype;

      if (ObjectLookup.NotReferencable<TKey>())
        keytype = TSpy<Object>.SerialType;
      else
        keytype = TSpy<TKey>.SerialType;

      if (ObjectLookup.NotReferencable<TValue>())
        valtype = TSpy<Object>.SerialType;
      else
        valtype = TSpy<TValue>.SerialType;

      return new ObjectToObjectMap(parms)
      {
        m_KeyStrictType   = keytype,
        m_ValueStrictType = valtype
      };
    }

    #endregion Public Static Section


    public override System.Type KeyType   => m_KeyStrictType;
    public override System.Type ValueType => m_ValueStrictType;


    [SerializeField]
    private SerialType m_KeyStrictType = TSpy<Object>.SerialType;
    [SerializeField]
    private SerialType m_ValueStrictType = TSpy<Object>.SerialType;
    

    public ObjectToObjectMap(in HashMapParams parms = default) : base(parms)
    {
      _ = Validate();
    }

    protected override bool Validate()
    {
      if (!m_KeyStrictType)
      {
        $"ObjectToObjectMap strict key type was deserialized as Missing! Defaulting."
          .LogWarning(this);
        m_KeyStrictType = TSpy<Object>.SerialType;
      }

      if (!m_ValueStrictType)
      {
        $"ObjectToObjectMap strict value type was deserialized as Missing! Defaulting."
          .LogWarning(this);
        m_ValueStrictType = TSpy<Object>.SerialType;
      }

      m_IsValidKey = ObeysStrictKeyType;
      m_IsValidValue = ObeysStrictValueType;
      m_HashMap.SetKeyEquals(ObjectLookup.ObjectsEqual);
      m_HashMap.SetValueEquals(ObjectLookup.ObjectsEqual);

      return base.Validate();
    }


    #region Private Section

    private bool ObeysStrictKeyType(Object key)
    {
      return key && m_KeyStrictType.AssignableFrom(key);
    }

    private bool ObeysStrictValueType(Object val)
    {
      // slightly different semantics for values
      return !val || m_ValueStrictType.AssignableFrom(val);
    }

    #endregion Private Section

  } // end class ObjectToObjectMap

}