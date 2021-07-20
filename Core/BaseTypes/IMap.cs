/**
@file   PyroDK/Core/BaseTypes/IMap.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-13

@brief
  Interface for a custom data structure that fulfills the "map" quality;
  that is, a container that maps one input value to some output value
  reliably.
**/

using System.Collections;
using System.Collections.Generic;


namespace PyroDK
{

  public interface IReadOnlyMap : IEnumerable
  {
    System.Type KeyType   { get; }
    System.Type ValueType { get; }

    int Count     { get; }
    int Capacity  { get; }

    bool ContainsKey<TK>(TK key);
    bool Read<TK, TV>(TK key, out TV val);
  }

  public interface IReadOnlyMap<TKey, TValue> :
    IReadOnlyMap,
    IReadOnlyCollection<(TKey key, TValue value)>
  {
    TValue this[TKey key] { get; }

    bool Contains(TKey key);
    bool Find(TKey key, out TValue value);
  }


  public interface IMap : IReadOnlyMap
  {
    new int Capacity    { get; set; }

    bool Clear();

    bool Write<TK, TV>(TK key, TV val, bool overwrite);
    bool Erase<TK>(TK key);
  }

  public interface IMap<TKey, TValue> :
    IReadOnlyMap<TKey, TValue>,
    IMap
  {
    new TValue this[TKey key] { get; set; }

    bool Map(TKey key, TValue value);
    bool Remap(TKey key, TValue value);
    bool Unmap(TKey key);

    bool Set(IReadOnlyCollection<TKey> keys, IReadOnlyCollection<TValue> values);
  }


  // the following specification is also called a "bimap", or "bidirectional map":

  public interface IReadOnlyReversableMap<T0, T1> :
    IReadOnlyMap<T0, T1>
  {
    IReadOnlyReversableMap<T1, T0> Flip { get; }
  }

  public interface IReversableMap<T0, T1> :
    IReadOnlyReversableMap<T0, T1>,
    IMap<T0, T1>
  {
    new IReversableMap<T1, T0> Flip { get; }
  }


  public static class Maps
  {

    public static bool Find<TKey, TValue, TOut>(this IReadOnlyMap<TKey, TValue> map, TKey key, out TOut result)
      where TOut : class, TValue
    {
      if (map.Find(key, out TValue val) && val is TOut casted)
      {
        result = casted;
        return true;
      }

      result = default;
      return false;
    }

    public static bool IsEmpty(this IReadOnlyMap map)
    {
      return map == null || map.Count == 0;
    }

  }


  public interface ILookup<T> : IMap<string, T>
  {
  }

  public abstract class Lookup<TPair, TValue> : SerialHashMap<TPair, string, TValue>, ILookup<TValue>
    where TPair : SerialKVP<string, TValue>, new()
  {
    protected Lookup(HashMapParams parms) : base(parms)
    {
      m_IsValidKey = IsValidString;
    }

    protected Lookup() : base()
    {
    }


    protected override bool Validate()
    {
      m_IsValidKey = IsValidString;
      return base.Validate();
    }

    private bool IsValidString(string str)
    {
      return str.Length > 0;
    }
  }

}