/**
@file   PyroDK/Core/AssetTypes/DictionaryAsset.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-18

@brief
  A generic asset type that stores key-value pairs, both of type `string`.
**/

#pragma warning disable CS0649

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  using Type = System.Type;

  
  [CreateAssetMenu(menuName = "PyroDK/Dictionary Asset", order = -150)]
  public sealed class DictionaryAsset : BaseAsset, ILookup<string>
  {
    public Type KeyType   => typeof(string);
    public Type ValueType => typeof(string);


    public string this[string key]
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
    

    [SerializeField]
    private StringLookup m_Lookup = new StringLookup(HashMapParams.Default);


    public bool Contains(string key)               => m_Lookup.Contains(key);
    public bool Find(string key, out string value) => m_Lookup.Find(key, out value);


    public bool Map(string key, string value)   => m_Lookup.Map(key, value);
    public bool Remap(string key, string value) => m_Lookup.Remap(key, value);
    public bool Unmap(string key)               => m_Lookup.Unmap(key);
    public bool Clear()                         => m_Lookup.Clear();

    public bool Set(IReadOnlyCollection<string> keys, IReadOnlyCollection<string> values)
    {
      return m_Lookup.Set(keys, values);
    }


    public IEnumerator<(string key, string value)> GetEnumerator() => m_Lookup.GetEnumerator();


    IEnumerator IEnumerable.GetEnumerator()                 => m_Lookup.GetEnumerator();
    bool IReadOnlyMap.ContainsKey<TK>(TK key)               => ((IReadOnlyMap)m_Lookup).ContainsKey(key);
    bool IReadOnlyMap.Read<TK, TV>(TK key, out TV val)      => ((IReadOnlyMap)m_Lookup).Read(key, out val);
    bool IMap.Write<TK, TV>(TK key, TV val, bool overwrite) => ((IMap)m_Lookup).Write(key, val, overwrite);
    bool IMap.Erase<TK>(TK key)                             => ((IMap)m_Lookup).Erase(key);
  }

}