/**
@file   PyroDK/Core/AssetTypes/ValueLookupAsset.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-18

@brief
  An asset that stores key-value pairs of serializable data types,
  where each pair's key contains a type code corresponding to the type of the
  mapped value.

@remark
  Because the type code portions (`SerialTypeCode`) contribute to each and
  every key's uniqueness, two or more keys in a given lookup may have IDENTICAL
  string portions, as long as these keys have different type codes.
**/

#pragma warning disable CS0649, IDE0051

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  using Type = System.Type;

  
  [CreateAssetMenu(menuName = "PyroDK/Omni Lookup Asset", order = -150)]
  public sealed class OmniLookupAsset : BaseAsset, IMap<TypedKey, object>
  {
    public OmniLookup Lookup => m_Lookup;


    [SerializeField]
    private OmniLookup m_Lookup = new OmniLookup();


    private void Awake()
    {
      if (m_Lookup.IncompleteDeserialize)
        m_Lookup.OnAfterDeserialize();
    }


    #region IMap<TypedKey, object> impl.

    public Type KeyType => m_Lookup.KeyType;
    public Type ValueType => m_Lookup.ValueType;
    public bool IsFixedSize => m_Lookup.IsFixedSize;
    public int Count => m_Lookup.Count;
    public int Capacity
    {
      get => m_Lookup.Capacity;
      set => m_Lookup.Capacity = value;
    }
    public object this[TypedKey key]
    {
      get => m_Lookup[key];
      set => m_Lookup[key] = value;
    }


    public bool Find(TypedKey key, out object value)
    {
      return m_Lookup.Find(key, out value);
    }

    public bool Contains(TypedKey key)
    {
      return m_Lookup.Contains(key);
    }

    public bool Map(TypedKey key, object value)
    {
      return m_Lookup.Map(key, value);
    }

    public bool Remap(TypedKey key, object value)
    {
      return m_Lookup.Remap(key, value);
    }

    public bool Unmap(TypedKey key)
    {
      return m_Lookup.Unmap(key);
    }

    public bool Clear()
    {
      return m_Lookup.Clear();
    }

    public bool Set(IReadOnlyCollection<TypedKey> keys, IReadOnlyCollection<object> values)
    {
      return m_Lookup.Set(keys, values);
    }

    public IEnumerator<(TypedKey key, object value)> GetEnumerator()
    {
      return m_Lookup.GetEnumerator();
    }


    bool IReadOnlyMap.ContainsKey<TK>(TK key)               => ((IReadOnlyMap)m_Lookup).ContainsKey(key);
    bool IMap.Erase<TK>(TK key)                             => ((IMap)m_Lookup).Erase(key);
    bool IReadOnlyMap.Read<TK, TV>(TK key, out TV val)      => ((IReadOnlyMap) m_Lookup).Read(key, out val);
    bool IMap.Write<TK, TV>(TK key, TV val, bool overwrite) => ((IMap)m_Lookup).Write(key, val, overwrite);
    IEnumerator IEnumerable.GetEnumerator()                 => m_Lookup.GetEnumerator();

    #endregion IMap<TypedKey, object> impl.

  } // end class OmniLookupAsset

}