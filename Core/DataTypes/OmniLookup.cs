/**
@file   PyroDK/Core/DataTypes/OmniLookup.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-10

@brief
  A generic map type that stores key-value pairs of semi-generic
  value types (those that are easy to serialize in Unity).
**/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{

  using Type = System.Type;


  [System.Serializable]
  public sealed class OmniLookup :
    IMap<TypedKey, object>,
    ISerializationCallbackReceiver
  {
    [System.Serializable]
    private sealed class KVP : SerialKVP<TypedKey, string>
    {
      [SerializeField]
      public SerialType StrictRefType = SerialType.Invalid;


      public KVP(TypedKey key, object val)
      {
        Key = key;

        if (key.Type <= SerialTypeCode.Unsupported)
          return;

        Value = Serializer.MakeString(val, key.Type, ref StrictRefType);
      }
    }


    public Type KeyType   => typeof(TypedKey);
    public Type ValueType => typeof(object);


    public object this[TypedKey key]
    {
      get
      {
        if (Find(key, out object result))
          return result;
        return null;
      }
      set
      {
        _ = Remap(key, value);
      }
    }


    public int Capacity
    {
      get => m_HashMap.Capacity;
      set
      {
        Debug.Assert(value >= m_Pairs.Count);
        m_HashMap.Capacity = value;
      }
    }

    public int  Count       => m_Pairs.Count;
    public bool IsFixedSize => m_SerialParams.IsFixedSize;

    public bool IncompleteDeserialize => ( m_HashMap == null ) ||
                                         ( m_Pairs.Count != m_HashMap.Count ); // bad condition?


    [SerializeField]
    private HashMapParams m_SerialParams;
    [SerializeField]
    private int           m_SerialVersion;

    [SerializeField]
    private List<KVP>     m_Pairs = new List<KVP>();


    [System.NonSerialized]
    private HashMap<TypedKey, object> m_HashMap;


    public OmniLookup(HashMapParams parms)
    {
      if (parms.Check())
      {
        m_SerialParams = parms;
      }
      else
      {
        $"Given HashMapParams contained invalid values:\n{parms}"
          .LogWarning();
        m_SerialParams = HashMapParams.Default;
      }

      m_HashMap = new HashMap<TypedKey, object>(m_SerialParams);
    }

    public OmniLookup() :
      this(HashMapParams.Default)
    {
    }

    public OmniLookup(int user_capacity) :
      this(new HashMapParams(user_capacity))
    {
    }


    public bool Find(TypedKey key, out object value)
    {
      if (!key)
      {
        value = null;
        return false;
      }
      else if (key.Type >= SerialTypeCode.RefAssetObject)
      {
        if (!m_HashMap.Find(key, out value))
          return false;

        if (!(value is string deferred_json))
          return true; // already deserialized

        // ref types may need to be deserialized from a JSON string:
        if (Serializer.FromJsonWrapped(deferred_json, out Object unity_obj) &&
            m_HashMap.Remap(key, unity_obj))
        {
          value = unity_obj;
          return SetDirty();
        }

        return false;
      }
      else
      {
        return m_HashMap.Find(key, out value) && value != null;
      }
    }
    public bool Find(SerialTypeCode type, string key, out object value)
    {
      return Find(TypedKey.Make(type, key), out value);
    }
    public bool Find<TValue>(string key, out TValue value)
    {
      value = default;

      if (Find(TypedKey.Make<TValue>(key), out object boxed) &&
          boxed is TValue unboxed)
      {
        value = unboxed;
        return true;
      }

      return false;
    }


    public bool Contains(TypedKey key)
    {
      return key && m_HashMap.Contains(key);
    }
    public bool Contains(SerialTypeCode type, string key)
    {
      return Contains(TypedKey.Make(type, key));
    }
    public bool Contains<TValue>(string key)
    {
      return Contains(TypedKey.Make<TValue>(key));
    }


    public bool Map(TypedKey key, object value)
    {
      return key && m_HashMap.Map(key, value) && SetDirty();
    }
    public bool Map(SerialTypeCode type, string key, object value)
    {
      return Map(TypedKey.Make(type, key), value);
    }
    public bool Map<TValue>(string key, TValue value)
    {
      return Map(TypedKey.Make<TValue>(key), value);
    }


    public bool Remap(TypedKey key, object value)
    {
      return key && m_HashMap.Remap(key, value) && SetDirty();
    }
    public bool Remap(SerialTypeCode type, string key, object value)
    {
      return Remap(TypedKey.Make(type, key), value);
    }
    public bool Remap<TValue>(string key, TValue value)
    {
      return Remap(TypedKey.Make<TValue>(key), value);
    }


    public bool Unmap(TypedKey key)
    {
      return key && m_HashMap.Unmap(key) && SetDirty();
    }
    public bool Unmap(SerialTypeCode type, string key)
    {
      return Unmap(TypedKey.Make(type, key));
    }
    public bool Unmap<TValue>(string key)
    {
      return Unmap(TypedKey.Make<TValue>(key));
    }


    public bool Clear()
    {
      m_Pairs.Clear();
      m_SerialParams.MinRealCapacity = 0;
      return m_HashMap.Clear() && SetDirty();
    }


    public bool Set(IReadOnlyCollection<TypedKey> keys, IReadOnlyCollection<object> values)
    {
      return m_HashMap.Set(keys, values) && SetDirty();
    }


    public void OnBeforeSerialize()
    {
      if (m_HashMap == null || m_HashMap.Version == m_SerialVersion)
        return;

      int count = m_HashMap.Count;

      m_Pairs.Clear();

      if (count > 0)
      {
        m_Pairs.Capacity = count;

        foreach (var (key, val) in m_HashMap)
        {
          m_Pairs.Add(new KVP(key, val));
        }
      }

      m_SerialParams  = m_HashMap.Parameters;
      m_SerialVersion = m_HashMap.Version;
    }

    public void OnAfterDeserialize()
    {
      if (!m_SerialParams.Check())
      {
        "\"m_SerialParams\" were invalid; resetting to default params."
          .LogWarning();

        m_HashMap = new HashMap<TypedKey, object>(m_SerialParams = HashMapParams.Default);
      }
      else if (m_HashMap == null)
      {
        m_HashMap = new HashMap<TypedKey, object>(m_SerialParams);
      }

      int n = m_Pairs.Count;
      if (n == 0)
      {
        _ = Clear();
        m_SerialVersion = m_HashMap.Version;
        return;
      }

      using (var boxed_vals = new BufferList<object>(n))
      {
        for (int i = n - 1; i >= 0; --i)
        {
          var pair = m_Pairs[i];

          if (pair.Key.Type <= SerialTypeCode.Unsupported)
          {
            boxed_vals[i] = null;
          }
          else if (Serializer.TryParseString(pair.Value, pair.Key.Type, out object boxed))
          {
            if (pair.StrictRefType && !pair.StrictRefType.AssignableFrom(boxed))
            {
              boxed_vals[i] = null;
            }
            else
            {
              boxed_vals[i] = boxed;
            }
          }
          else
          {
            // Reaching here is only an error if the value is not a wrapped serial type:
            if (pair.Key.Type < SerialTypeCode.RefAssetObject)
            {
              $"Invalid key/value: {pair}, StrictRefType={pair.StrictRefType.LogName()}"
                .LogError(this);
            }
            else
            {
              // deferring deserialization of this value string for later
              boxed_vals[i] = pair.Value;
            }
          }
        }

        if (m_HashMap.GrowTo(user_cap: n, rehash: false) &&
            m_HashMap.Set(m_Pairs.GetKeys<KVP, TypedKey, string>(), boxed_vals))
        {
          m_SerialParams.MinUserCapacity = m_HashMap.Capacity;
        }
      } // end using BufferList

      m_SerialVersion = m_HashMap.Version;
    }



    public IEnumerator<(TypedKey key, object value)> GetEnumerator()
    {
      return m_HashMap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return m_HashMap.GetEnumerator();
    }


    bool IReadOnlyMap.ContainsKey<TK>(TK key)
    {
      return ((IReadOnlyMap)m_HashMap).ContainsKey(key);
    }

    bool IMap.Erase<TK>(TK key)
    {
      return ((IMap)m_HashMap).Erase(key) && SetDirty();
    }
    

    bool IReadOnlyMap.Read<TK, TV>(TK key, out TV val)
    {
      return ((IReadOnlyMap)m_HashMap).Read(key, out val);
    }

    bool IMap.Write<TK, TV>(TK key, TV val, bool overwrite)
    {
      return ((IMap)m_HashMap).Write(key, val, overwrite) && SetDirty();
    }



    private bool SetDirty()
    {
      m_SerialVersion = -1;
      return true;
    }

  }
}