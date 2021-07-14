/**
@file   PyroDK/Core/AssetTypes/TypedLookupAsset.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-18

@brief
  A generic asset type that stores key-value pairs of semi-generic
  value types.
**/

#pragma warning disable CS0649

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  
  [CreateAssetMenu(menuName = "PyroDK/Data Lookup", order = -150)]
  public sealed class DataLookupAsset :
    BaseAsset,
    IMap<TypedStringKey, object>
  {
    public Type KeyType     => m_Lookup.KeyType;
    public Type ValueType   => m_Lookup.ValueType;


    public object this[TypedStringKey key]
    {
      get => m_Lookup[key];
      set => m_Lookup[key] = value;
    }


    public int Count => m_Lookup.Count;
    public int Capacity
    {
      get => m_Lookup.Capacity;
      set => m_Lookup.Capacity = value;
    }

    public bool IsFixedSize => m_Lookup.IsFixedSize;


    [SerializeField]
    private SerialValueMap m_Lookup = new SerialValueMap(HashMapParams.Default);


    public bool Clear()
    {
      return m_Lookup.Clear();
    }


    public bool Map(TypedStringKey key, object value)
    {
      return m_Lookup.Map(key, value);
    }

    public bool Remap(TypedStringKey key, object value)
    {
      return m_Lookup.Remap(key, value);
    }

    public bool Unmap(TypedStringKey key)
    {
      return m_Lookup.Unmap(key);
    }

    public bool Set(IReadOnlyCollection<TypedStringKey> keys, IReadOnlyCollection<object> values)
    {
      return m_Lookup.Set(keys, values);
    }

    public bool Contains(TypedStringKey key)
    {
      return m_Lookup.Contains(key);
    }

    public bool Find(TypedStringKey key, out object value)
    {
      return m_Lookup.Find(key, out value);
    }

    public IEnumerator<(TypedStringKey key, object value)> GetEnumerator()
    {
      return m_Lookup.GetEnumerator();
    }


    private void Awake()
    {
      if (m_Lookup.IncompleteDeserialize)
        m_Lookup.OnAfterDeserialize();
    }


    bool IReadOnlyMap.ContainsKey<TK>(TK key)               => ((IReadOnlyMap)m_Lookup).ContainsKey(key);
    bool IMap.Erase<TK>(TK key)                             => ((IMap)m_Lookup).Erase(key);
    bool IReadOnlyMap.Read<TK, TV>(TK key, out TV val)      => ((IReadOnlyMap) m_Lookup).Read(key, out val);
    bool IMap.Write<TK, TV>(TK key, TV val, bool overwrite) => ((IMap)m_Lookup).Write(key, val, overwrite);
    IEnumerator IEnumerable.GetEnumerator()                 => m_Lookup.GetEnumerator();

  }

}