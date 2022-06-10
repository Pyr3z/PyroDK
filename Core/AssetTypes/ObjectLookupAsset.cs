/**
@file   PyroDK/Core/AssetTypes/ObjectLookupAsset.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-18

@brief
  A generic asset type that stores string-Object pairs for later lookup.
**/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  using Type = System.Type;


  [CreateAssetMenu(menuName = "PyroDK/Object Lookup", order = -150)]
  public sealed class ObjectLookupAsset : BaseAsset, ILookup<Object>
  {
    public Type KeyType   => m_Lookup.KeyType;
    public Type ValueType => m_Lookup.ValueType;


    public Object this[string key]
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


    bool ICollection<(string key, Object value)>.IsReadOnly => false;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this;


    [SerializeField]
    private ObjectLookup m_Lookup;


    public bool Contains(string key)                => m_Lookup.Contains(key);
    public bool Find(string key, out Object value)  => m_Lookup.Find(key, out value);


    public bool Map(string key, Object value)   => m_Lookup.Map(key, value);
    public bool Remap(string key, Object value) => m_Lookup.Remap(key, value);


    public bool Unmap(string key) => m_Lookup.Unmap(key);
    public bool Clear()           => m_Lookup.Clear();


    public bool Set(IReadOnlyCollection<string> keys, IReadOnlyCollection<Object> values)
    {
      return m_Lookup.Set(keys, values);
    }


    public IEnumerator<(string key, Object value)> GetEnumerator() => m_Lookup.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()                 => m_Lookup.GetEnumerator();
    bool IReadOnlyMap.ContainsKey<TK>(TK key)               => ((IReadOnlyMap)m_Lookup).ContainsKey(key);
    bool IReadOnlyMap.Read<TK, TV>(TK key, out TV val)      => ((IReadOnlyMap)m_Lookup).Read(key, out val);
    bool IMap.Write<TK, TV>(TK key, TV val, bool overwrite) => ((IMap)m_Lookup).Write(key, val, overwrite);
    bool IMap.Erase<TK>(TK key)                             => ((IMap)m_Lookup).Erase(key);

    void ICollection<(string key, Object value)>.Add((string key, Object value) item)
    {
      ((ICollection<(string key, Object value)>)m_Lookup).Add(item);
    }

    void ICollection<(string key, Object value)>.Clear()
    {
      m_Lookup.Clear();
    }

    bool ICollection<(string key, Object value)>.Contains((string key, Object value) item)
    {
      return ((ICollection<(string key, Object value)>)m_Lookup).Contains(item);
    }

    void ICollection<(string key, Object value)>.CopyTo((string key, Object value)[] array, int idx)
    {
      ((ICollection<(string key, Object value)>)m_Lookup).CopyTo(array, idx);
    }

    bool ICollection<(string key, Object value)>.Remove((string key, Object value) item)
    {
      return ((ICollection<(string key, Object value)>)m_Lookup).Remove(item);
    }

    void ICollection.CopyTo(System.Array array, int idx)
    {
      ((ICollection)m_Lookup).CopyTo(array, idx);
    }
  }

}