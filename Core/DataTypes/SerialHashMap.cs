/**
@file   PyroDK/Core/DataTypes/SerialHashMap.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-17

@brief
  Base class for creating serializable hashmaps for Unity.
**/

using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;


namespace PyroDK
{
  using Type = System.Type;

  #if UNITY_EDITOR
  using JsonUtility = UnityEditor.EditorJsonUtility;
  #else
  using JsonUtility = UnityEngine.JsonUtility;
  #endif


  [System.Serializable]
  public abstract class SerialHashMap<TPair, TKey, TValue> : IMap<TKey, TValue>, ISerializationCallbackReceiver
    where TPair : SerialKVP<TKey, TValue>, new()
  {

    public virtual Type KeyType   => typeof(TKey);
    public virtual Type ValueType => typeof(TValue);


    public int Count => m_HashMap.Count;
    public int Capacity
    {
      get => m_HashMap.Capacity;
      set => m_HashMap.Capacity = value;
    }


    public TValue this[TKey key]
    {
      get => m_HashMap[key];
      set => _ = Remap(key, value);
    }


    public bool IsDirty => m_HashMap != null && m_HashMap.Version != m_SerialVersion;


    bool ICollection<(TKey key, TValue value)>.IsReadOnly => false;
    bool ICollection.IsSynchronized => false; // TODO implement async hashmap
    object ICollection.SyncRoot => this;


    [SerializeField]
    protected HashMapParams m_SerialParams;
    [SerializeField]
    protected int           m_SerialVersion;

    [SerializeField]
    protected List<TPair>   m_Pairs = new List<TPair>();
    

    [System.NonSerialized]
    protected HashMap<TKey, TValue> m_HashMap = new HashMap<TKey, TValue>();

    [System.NonSerialized]
    protected System.Func<TKey, bool>   m_IsValidKey   = null;
    [System.NonSerialized]
    protected System.Func<TValue, bool> m_IsValidValue = null;
    

    public SerialHashMap(HashMapParams parms)
    {
      Debug.Assert(parms.Check());
      m_HashMap = new HashMap<TKey, TValue>(m_SerialParams = parms);
    }

    public SerialHashMap() : this(HashMapParams.Default)
    {
    }



    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
      if (!IsDirty)
        return;

      int count = m_HashMap.Count;
      
      m_Pairs.Clear();

      if (count > 0)
      {
        m_Pairs.Capacity = count;

        foreach (var (key, value) in m_HashMap)
        {
          m_Pairs.Add(new TPair()
          {
            Key   = key,
            Value = value
          });
        }
      }

      m_SerialParams  = m_HashMap.Parameters;
      m_SerialVersion = m_HashMap.Version;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      if (Validate())
      {
        Set(m_Pairs.GetKeys<TPair, TKey, TValue>(),
            m_Pairs.GetValues<TPair, TKey, TValue>());
      }

      m_SerialVersion = m_HashMap.Version;
    }

    protected virtual bool Validate()
    {
      if (!m_SerialParams.Check())
      {
        "\"m_SerialParams\" were invalid; resetting to default params.".LogWarning(this);
        m_SerialParams = HashMapParams.Default;
      }

      if (m_HashMap == null)
        m_HashMap = new HashMap<TKey, TValue>(m_SerialParams);
      else
        m_HashMap.Reinit(m_SerialParams);

      return true;
    }


    protected bool SetDirty()
    {
      m_SerialVersion = -1;
      return true;
    }


    protected bool IsValidKey(TKey key)
    {
      return key != null && (m_IsValidKey == null || m_IsValidKey(key));
    }

    protected bool IsValidValue(TValue val)
    {
      return m_IsValidValue == null || m_IsValidValue(val);
    }


    public bool Contains(TKey key)
    {
      return m_HashMap.Contains(key);
    }
    public bool Find(TKey key, out TValue val)
    {
      return m_HashMap.Find(key, out val);
    }


    public bool Map(TKey key, TValue val)
    {
      return  IsValidKey(key) && IsValidValue(val) &&
              m_HashMap.Map(key, val) && SetDirty();
    }

    public bool Remap(TKey key, TValue val)
    {
      return  IsValidKey(key) && IsValidValue(val) &&
              m_HashMap.Remap(key, val) && SetDirty();
    }

    protected bool CheckedRemap(TKey key, TValue val, HashMap<TKey, TValue>.ValueEqualityProvider value_equals)
    {
      if (!IsValidKey(key) || !IsValidValue(val))
        return false;

      m_HashMap.PushValueEquals(value_equals);
      bool result = m_HashMap.Remap(key, val);
      m_HashMap.PopValueEquals();
      return result && SetDirty();
    }


    public bool Unmap(TKey key)
    {
      return m_HashMap.Unmap(key) && SetDirty();
    }


    public int UnmapAll(System.Func<TKey, bool> where)
    {
      int removed = m_HashMap.ClearSelective(where);

      if (removed > 0)
        _ = SetDirty();

      return removed;
    }

    public int UnmapAll(System.Func<TKey, TValue, bool> where)
    {
      int removed = m_HashMap.ClearSelective(where);

      if (removed > 0)
        _ = SetDirty();

      return removed;
    }


    public bool Clear()
    {
      m_SerialParams.MinRealCapacity = 0;

      m_Pairs.Clear();

      return m_HashMap.Clear() && SetDirty();
    }

    public bool Set(IReadOnlyCollection<TKey> keys, IReadOnlyCollection<TValue> vals)
    {
      int n = keys?.Count ?? 0;

      if (n == 0)
        return Clear();

      var pairs = new ValidatedCollection<TKey, TValue>(keys, vals, IsValidKey, IsValidValue);

      n = pairs.Count;
      if (n == 0)
        return false; // don't actually clear in this case (?)

      if (!Logging.Assert(m_HashMap.GrowTo(user_cap: n, rehash: false)) &&
          m_HashMap.Set(pairs.GetTCollection(), pairs.GetUColllection()))
      {
        m_SerialParams.MinUserCapacity = m_HashMap.Capacity;
        return SetDirty();
      }

      return false;
    }

    
    public int Absorb(IMap<TKey, TValue> other, HashMap<TKey, TValue>.ValueEqualityProvider value_equals)
    {
      int absorbed = 0;

      m_HashMap.PushValueEquals(value_equals);

      foreach (var (key, val) in other)
      {
        if (m_HashMap.Remap(key, val))
        {
          ++absorbed;
        }
      }

      m_HashMap.PopValueEquals();

      if (absorbed > 0)
      {
        _ = SetDirty();
      }  

      return absorbed;
    }



    public bool TrySaveToDisk(string path, bool pretty_print = true)
    {
      if (!Filesystem.TryGuaranteePathFor(path, out path))
      {
        $"Could not save SerialHashMap to \"{path}\" ; likely restricted by version control.".LogWarning();
        return false;
      }

      File.WriteAllText(path, JsonUtility.ToJson(this, pretty_print));
      Filesystem.ImportAsset(path);
      return true;
    }

    public bool TryLoadFromDisk(string path)
    {
      if (!Filesystem.TryMakeAbsolutePath(path, out path) || !File.Exists(path))
        return false;

      string json = File.ReadAllText(path);

      if (json.IsEmpty())
        return false;

      JsonUtility.FromJsonOverwrite(json, this);
      return true;
    }



    public IEnumerator<(TKey key, TValue value)> GetEnumerator() => m_HashMap.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_HashMap.GetEnumerator();


    bool IMap.Write<TK, TV>(TK key, TV val, bool overwrite)
    {
      return ((IMap)m_HashMap).Write(key, val, overwrite) && SetDirty();
    }

    bool IMap.Erase<TK>(TK key)
    {
      return ((IMap)m_HashMap).Erase(key) && SetDirty();
    }

    bool IReadOnlyMap.ContainsKey<TK>(TK key)
    {
      return ((IMap)m_HashMap).ContainsKey(key);
    }

    bool IReadOnlyMap.Read<TK, TV>(TK key, out TV val)
    {
      return ((IMap)m_HashMap).Read(key, out val);
    }

    void ICollection<(TKey key, TValue value)>.Add((TKey key, TValue value) item)
    {
      ((ICollection<(TKey key, TValue value)>)m_HashMap).Add(item);
    }

    void ICollection<(TKey key, TValue value)>.Clear()
    {
      ((ICollection<(TKey key, TValue value)>)m_HashMap).Clear();
    }

    bool ICollection<(TKey key, TValue value)>.Contains((TKey key, TValue value) item)
    {
      return ((ICollection<(TKey key, TValue value)>)m_HashMap).Contains(item);
    }

    void ICollection<(TKey key, TValue value)>.CopyTo((TKey key, TValue value)[] array, int arrayIndex)
    {
      ((ICollection<(TKey key, TValue value)>)m_HashMap).CopyTo(array, arrayIndex);
    }

    bool ICollection<(TKey key, TValue value)>.Remove((TKey key, TValue value) item)
    {
      return ((ICollection<(TKey key, TValue value)>)m_HashMap).Remove(item);
    }

    void ICollection.CopyTo(System.Array array, int index)
    {
      ((ICollection)m_HashMap).CopyTo(array, index);
    }
  }

}