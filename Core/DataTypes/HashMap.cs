/**
@file   PyroDK/Core/DataTypes/HashMap.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-06

@brief
  Versatile, type-safe, and lightning-fast hashmapping, for your pleasure.
**/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  using Type = System.Type;


  public class HashMap<TKey, TValue> : IMap<TKey, TValue>
  {
    protected struct Bucket
    {
      private object m_Key;

      public TValue Value;

      private int m_DirtyHash;


      public TKey Key => (TKey)m_Key;

      public int  Hash => m_DirtyHash & int.MaxValue;
      public bool IsEmpty => m_Key == null;
      public bool IsDefault => m_Key == null && m_DirtyHash == 0;
      public bool IsDirty => m_DirtyHash < 0;
      public bool IsSmeared => m_Key == null && m_DirtyHash < 0;


      public Bucket(TKey key, TValue val, int hash31)
      {
        m_Key = key;
        Value = val;
        m_DirtyHash = hash31;
      }


      public bool TryGetTuple(out (TKey key, TValue val) tuple)
      {
        tuple = default;

        if (m_Key == null)
          return false;

        tuple.key = Key;
        tuple.val = Value;
        return true;
      }

      public void Set(TKey key, TValue val, int hash31)
      {
        m_Key = key;
        Value = val;
        m_DirtyHash = hash31 | ( m_DirtyHash & Bitwise.SIGN_BIT_32 );
      }

      public bool MakeDirty()
      {
        if (m_DirtyHash < 0)
          return false;

        m_DirtyHash |= Bitwise.SIGN_BIT_32;
        return true;
      }

      public void Clear()
      {
        m_Key = null;
        Value = default;
        m_DirtyHash = 0;
      }

      public void Smear()
      {
        m_Key = null;
        Value = default;
        m_DirtyHash = m_DirtyHash & Bitwise.SIGN_BIT_32;
      }

      public Bucket RehashClone(int hash31)
      {
        return new Bucket(Key, Value, hash31);
      }

      public int PlaceIn(Bucket[] buckets, int jump, KeyEqualityProvider key_eq)
      {
        int collisions = 0;
        int i = (m_DirtyHash & int.MaxValue) % buckets.Length;

        while (!buckets[i].IsEmpty)
        {
          if (Hash == buckets[i].Hash && key_eq(Key, buckets[i].Key))
            return collisions | Bitwise.SIGN_BIT_32;

          if (buckets[i].MakeDirty())
            ++collisions;

          i = (i + jump) % buckets.Length;
        }

        buckets[i] = this;
        return collisions;
      }

      public int PlaceIn(Bucket[] buckets, int i, int jump)
      {
        int collisions = 0;

        while (!buckets[i].IsEmpty)
        {
          if (buckets[i].MakeDirty())
            ++collisions;

          i = (i + jump) % buckets.Length;
        }

        buckets[i] = this;
        return collisions;
      }

    } // end struct Bucket


    public delegate int KeyHashProvider(TKey key);
    public delegate bool KeyEqualityProvider(TKey k0, TKey k1);
    public delegate bool ValueEqualityProvider(TValue v0, TValue v1);



    public Type KeyType   => typeof(TKey);
    public Type ValueType => typeof(TValue);

    public HashMapParams Parameters => m_Params;

    public int Version => m_Version; // value that increments every time the data structure changes.
    public int Count   => m_Count;
    public int Capacity // interpreted here as "user capacity" or "load limit"
    {
      get => m_LoadLimit;
      set => EnsureMinUserCapacity(value);
    }

    public TValue this[TKey key]
    {
      get
      {
        _ = Find(key, out TValue result);
        return result;
      }
      set => TryInsert(key, value, true, out _ );
    }


    protected HashMapParams m_Params;

    protected int m_Count, m_Collisions, m_LoadLimit;
    protected int m_Version;

    protected Bucket[] m_Buckets;

    protected KeyHashProvider       m_KeyHasher   = DefaultKeyHash31;
    protected KeyEqualityProvider   m_KeyEquals   = DefaultKeyEquals;
    protected ValueEqualityProvider m_ValueEquals = null;

    private Stack<ValueEqualityProvider> m_ValueEqualsStack = null;


    public HashMap() :
      this(HashMapParams.Default)
    {
    }
    public HashMap(int start_capacity) :
      this(new HashMapParams(start_capacity))
    {
    }
    public HashMap(HashMapParams parms)
    {
      if (parms.Check())
      {
        m_Params = parms;
      }
      else
      {
        $"HashMapParameters == default in {GetType().Name} constructor."
          .LogWarning(this);
        m_Params = HashMapParams.Default;
      }

      InitRuntimeBuckets();
    }
    
    protected void InitRuntimeBuckets(int user_cap = 0)
    {
      m_Count = m_Collisions = 0;
      m_LoadLimit = m_Params.MakeBuckets(user_cap, out m_Buckets);
    }


    public void Reinit(HashMapParams parms)
    {
      if (parms.Check())
        m_Params = parms;
      else
        m_Params = HashMapParams.Default;

      InitRuntimeBuckets();
    }

    public void Reinit(int user_cap)
    {
      m_Params.MinUserCapacity = user_cap;
      InitRuntimeBuckets(user_cap);
    }

    public void SetKeyHash31(KeyHashProvider hasher)
    {
      if (hasher == null)
        m_KeyHasher = DefaultKeyHash31;
      else
        m_KeyHasher = hasher;
    }

    public void SetKeyEquals(KeyEqualityProvider key_equals)
    {
      if (key_equals == null)
        m_KeyEquals = DefaultKeyEquals;
      else
        m_KeyEquals = key_equals;
    }

    public void SetValueEquals(ValueEqualityProvider value_equals)
    {
      m_ValueEquals = value_equals;
    }

    public void PushValueEquals(ValueEqualityProvider value_equals)
    {
      if (m_ValueEqualsStack == null)
        m_ValueEqualsStack = new Stack<ValueEqualityProvider>();

      m_ValueEqualsStack.Push(m_ValueEquals);
      m_ValueEquals = value_equals;
    }

    public void PopValueEquals()
    {
      if (m_ValueEqualsStack != null && m_ValueEqualsStack.Count > 0)
        m_ValueEquals = m_ValueEqualsStack.Pop();
      else
        m_ValueEquals = null;
    }


    public IEnumerator<(TKey key, TValue value)> GetEnumerator()
    {
      if (m_Buckets == null || m_Count == 0)
        yield break;

      int i       = m_Buckets.Length;
      int count   = m_Count;
      int version = m_Version;
      
      (TKey key, TValue value) next = default;

      while (i --> 0)
      {
        if (m_Buckets[i].TryGetTuple(out next))
        {
          yield return next;

          if (--count == 0)
            yield break;

          if (m_Version != version)
            throw new System.InvalidOperationException("HashMap was modified while iterating through it.");
        }
      }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public List<(TKey key, TValue value)> GetPairList()
    {
      var list = new List<(TKey key, TValue value)>(m_Count);

      foreach (var kvp in this)
      {
        list.Add(kvp);
      }

      return list;
    }

    public List<TKey> GetKeyList()
    {
      var list = new List<TKey>(m_Count);

      foreach (var (key, _ ) in this)
      {
        list.Add(key);
      }

      return list;
    }

    public List<TValue> GetValueList()
    {
      var list = new List<TValue>(m_Count);

      foreach (var ( _ , value) in this)
      {
        list.Add(value);
      }

      return list;
    }


    public bool Contains(TKey key)
    {
      if (key == null || m_Count == 0)
        return false;

      var (hash31, jump) = CalcHashJump(key, m_Buckets.Length);

      int i = hash31 % m_Buckets.Length;
      int jumps = 0;

      TriBool found;

      do
      {
        found = BucketEquals(i, hash31, key);

        if (found.IsZero)
          return false;

        if (found.IsPositive)
          return true;

        i = (i + jump) % m_Buckets.Length;
      }
      while (++jumps < m_Count);

      return false;
    }

    public bool Find(TKey key, out TValue result)
    {
      result = default;
      if (key == null || m_Count == 0)
        return false;

      var (hash31, jump) = CalcHashJump(key, m_Buckets.Length);

      int i = hash31 % m_Buckets.Length;
      int jumps = 0;

      TriBool found;

      do
      {
        found = BucketEquals(i, hash31, key);

        if (found.IsZero)
          return false;

        if (found.IsPositive)
        {
          result = m_Buckets[i].Value;
          return true;
        }

        i = (i + jump) % m_Buckets.Length;
      }
      while (++jumps < m_Count);

      return false;
    }



    /// <summary>
    /// Registers a new key-value mapping in the HashMap iff there isn't already
    /// a mapping at the given key.
    /// </summary>
    /// <returns>
    /// true   if the value was successfully mapped to the given key,
    /// false  if there was already a value mapped to this key,
    ///        or there was an error.
    /// </returns>
    public bool Map(TKey key, TValue val)
    {
      return TryInsert(key, val, overwrite: false, out _ );
    }

    /// <summary>
    /// For syntactic sugar and familiarity, however no different from Map(),
    /// aside from the void return.
    /// </summary>
    public void Add(TKey key, TValue val)
    {
      _ = TryInsert(key, val, overwrite: false, out _ );
    }

    /// <summary>
    /// Like Map(), but allows the user to overwrite preexisting values.
    /// </summary>
    /// <returns>
    /// true   if the value is successfully mapped,
    /// false  if the value is identical to a preexisting mapping,
    ///        or there was an error.
    /// </returns>
    public bool Remap(TKey key, TValue val)
    {
      return TryInsert(key, val, overwrite: true, out _ );
    }

    /// <summary>
    /// Used in case you care what happens to previously mapped values at certain keys.
    /// </summary>
    /// <param name="key">The key to map the new value to.</param>
    /// <param name="val">The value to be mapped.</param>
    /// <param name="preexisting">Situational output value, which is only valid if TriBool.False is returned.</param>
    /// <returns>
    /// true   if new value was mapped successfully,
    /// false  if new value was NOT mapped because there is a preexisting value,
    /// null   if error.
    /// </returns>
    public TriBool TryMap(TKey key, TValue val, out TValue preexisting)
    {
      if (TryInsert(key, val, overwrite: false, out int i))
      {
        preexisting = val;
        return TriBool.True;
      }
      else if (-1 < i && !m_Buckets[i].IsEmpty)
      {
        preexisting = m_Buckets[i].Value;
        return TriBool.False;
      }

      preexisting = default;
      return TriBool.Null;
    }


    public bool Set(IReadOnlyCollection<TKey> keys, IReadOnlyCollection<TValue> vals)
    {
      int n = keys?.Count ?? 0;
      if (n == 0)
        return Clear();

      if (vals == null || vals.Count > n)
      {
        "Mismatched keys/vals array sizes."
          .LogError(this);
        return false;
      }

      if (n < m_LoadLimit)
        n = m_LoadLimit;

      Debug.Assert(m_Params.Check());

      m_Collisions  = m_Count = 0;
      m_LoadLimit   = m_Params.MakeBuckets(n, out Bucket[] buckets);

      var vals_iter = vals.GetEnumerator();

      Bucket bucket;
      int hash31, jump, collisions;

      foreach (var key in keys)
      {
        vals_iter.MoveNext();

        if (key == null)
        {
          "A key array with a null key was provided."
            .LogWarning(this);
          continue;
        }

        (hash31, jump) = CalcHashJump(key, buckets.Length);

        bucket     = new Bucket(key, vals_iter.Current, hash31);
        collisions = bucket.PlaceIn(buckets, jump, m_KeyEquals.Invoke);
        
        if (collisions < 0)
        {
          $"A key array was given with duplicate keys. key: \"{bucket.Key}\""
            .LogWarning(this);
        }
        else
        {
          m_Collisions += collisions;
          ++m_Count;
        }
      }

      Debug.Assert(m_Count <= Capacity);

      m_Buckets = buckets;
      ++m_Version;
      return true;
    }


    public bool Unmap(TKey key)
    {
      if (key == null)
        return false;

      var (hash31, jump) = CalcHashJump(key, m_Buckets.Length);

      int i = hash31 % m_Buckets.Length;
      int jumps = 0;

      TriBool found;

      do
      {
        found = BucketEquals(i, hash31, key);

        if (found.IsZero)
          return false;

        if (found.IsPositive)
        {
          m_Buckets[i].Smear();
          --m_Count;
          ++m_Version;
          return true;
        }

        i = (i + jump) % m_Buckets.Length;
      }
      while (++jumps < m_Count);

      return false;
    }
    public void Remove(TKey key)
    {
      _ = Unmap(key);
    }


    public int ClearSelective(System.Func<TKey, bool> select)
    {
      if (m_Count == 0 || m_Buckets == null || select == null)
        return 0;

      int removed = 0;

      int i       = m_Buckets.Length;
      int count   = m_Count;

      while (i --> 0)
      {
        if (!m_Buckets[i].IsEmpty)
        {
          if (select(m_Buckets[i].Key))
          {
            m_Buckets[i].Smear();
            --m_Count;
            ++removed;
          }

          if (--count == 0)
            break;
        }
      }

      if (removed > 0)
      {
        if (m_Count == 0)
          InitRuntimeBuckets(m_Params.CalcLoadLimit(m_Buckets.Length));
        ++m_Version;
      }

      return removed;
    }

    public int ClearSelective(System.Func<TKey, TValue, bool> select)
    {
      if (m_Count == 0 || m_Buckets == null || select == null)
        return 0;

      int removed = 0;

      int i = m_Buckets.Length;
      int count = m_Count;

      while (i --> 0)
      {
        if (!m_Buckets[i].IsEmpty)
        {
          if (select(m_Buckets[i].Key, m_Buckets[i].Value))
          {
            m_Buckets[i].Smear();
            --m_Count;
            ++removed;
          }

          if (--count == 0)
            break;
        }
      }

      if (removed > 0)
      {
        if (m_Count == 0)
          InitRuntimeBuckets(m_Params.CalcLoadLimit(m_Buckets.Length));
        ++m_Version;
      }

      return removed;
    }

    public bool Clear()
    {
      bool already_clear = m_Count == 0;

      if (m_Buckets == null)
      {
        InitRuntimeBuckets();
      }
      else
      {
        InitRuntimeBuckets(m_LoadLimit);
        ++m_Version;
      }

      return !already_clear;
    }


    public void EnsureMinUserCapacity(int min_user_cap)
    {
      if (m_Params.IsFixedSize)
        return;

      if (min_user_cap > m_LoadLimit)
      {
        Rehash(m_Params.SetMinUserCapacity(min_user_cap));
      }
    }


    public bool GrowTo(int user_cap, bool rehash)
    {
      if (m_LoadLimit >= user_cap)
        return true;

      if (m_Params.IsFixedSize)
        return false;

      user_cap = m_Params.CalcRealCapacity(user_cap);

      // "new_limit" is the next REAL capacity we are trying to grow
      int new_limit = m_Params.CalcNextSize(m_Params.CalcRealCapacity(m_LoadLimit));

      int sanity = 31; // should mathematically NEVER exceed this many loops

      while (new_limit < user_cap && sanity --> 0)
      {
        new_limit = m_Params.CalcNextSize(new_limit);
      }

      if (sanity <= 0)
      {
        "Lost all sanity. Saved our asses from an infinite while loop tho."
          .LogError(this);
        return false;
      }

      if (rehash)
      {
        Rehash(new_limit);
      }
      else
      {
        m_LoadLimit = m_Params.CalcLoadLimit(new_limit);
      }

      return true;
    }


    #region Explicit Interface Members

    bool IReadOnlyMap.ContainsKey<TK>(TK key)
    {
      return (key is TKey tkey) && Contains(tkey);
    }

    bool IReadOnlyMap.Read<TK, TV>(TK key, out TV val)
    {
      val = default;

      if (key is TKey tkey && TSpy<TValue>.IsCastableTo<TV>() &&
          Find(tkey, out TValue tval))
      {
        if (tval is TV casted)
          val = casted;
        return true;
      }

      return false;
    }

    bool IMap.Write<TK, TV>(TK key, TV val, bool overwrite)
    {
      return (key is TKey tkey && val is TValue tval) && TryInsert(tkey, tval, overwrite, out _ );
    }

    bool IMap.Erase<TK>(TK key)
    {
      return (key is TKey tkey) && Unmap(tkey);
    }

    #endregion Explicit Interface Members


    protected int GrowCapacity()
    {
      if (m_Params.IsFixedSize)
        return -1;

      int new_limit = m_Params.CalcNextSize(m_Buckets.Length);

      if (m_Buckets.Length < new_limit)
      {
        Rehash(new_limit);
      }

      return m_LoadLimit;
    }

    protected (int hash31, int jump) CalcHashJump(TKey key, int size)
    {
      int hash31 = m_KeyHasher(key);
      return (hash31, m_Params.CalcJump(hash31, size));
    }

    protected TriBool BucketEquals(int i, int hash31, TKey key)
    {
      if (Logging.Assert(i < m_Buckets.Length)) // $"m_Buckets.Length: {m_Buckets.Length} ; m_Count: {m_Count}"
        return TriBool.Null;

      var bucket = m_Buckets[i];

      if (bucket.IsEmpty)
      {
        if (bucket.IsDirty)
          return TriBool.False;
        else
          return TriBool.Null;
      }

      if (bucket.Hash == hash31 && m_KeyEquals(key, bucket.Key))
        return TriBool.True;

      return TriBool.False;
    }
    
    protected bool BucketValueEquals(int i, TValue val)
    {
      return  m_ValueEquals != null &&
              m_ValueEquals(m_Buckets[i].Value, val);
    }

    protected bool TryInsert(TKey key, TValue val, bool overwrite, out int i)
    {
      if (key == null)
      {
        i = -1;
        return false;
      }

      if (m_Buckets == null)
      {
        InitRuntimeBuckets();
      }
      else if (m_Count >= m_LoadLimit)
      {
        if (GrowCapacity() <= m_Count)
        {
          i = -1;
          return false;
        }
      }
      else if (m_Collisions > m_LoadLimit && m_Count > m_Params.RehashThreshold)
      {
        Rehash();
      }

      var (hash31, jump) = CalcHashJump(key, m_Buckets.Length);

      i = hash31 % m_Buckets.Length;

      int fallback  = -1;
      int jumps     = 0;

      do
      {
        if (m_Buckets[i].IsDefault)
        {
          // immediately at an empty bucket
          if (fallback != -1)
            i = fallback;

          m_Buckets[i] = new Bucket(key, val, hash31);
          ++m_Count;
          ++m_Version;
          return true;
        }

        if (fallback == -1 && m_Buckets[i].IsSmeared)
        {
          // cache this dirty slot as the fallback option to fill:
          fallback = i;
        }
        else if (m_Buckets[i].IsEmpty)
        {
          // end of a smear chain
          if (fallback != -1)
            i = fallback;

          m_Buckets[i].Set(key, val, hash31);
          ++m_Count;
          ++m_Version;

          if (jumps > m_Params.RehashThreshold)
          {
            Rehash();
          }

          return true;
        }
        else if (BucketEquals(i, hash31, key))
        {
          // equivalent bucket found
          if (!overwrite || BucketValueEquals(i, val))
            return false;

          m_Buckets[i].Value = val;
          ++m_Version;

          if (jumps > m_Params.RehashThreshold)
          {
            Rehash();
          }

          return true;
        }

        // Mark collision if not already marked:
        if (fallback == -1 && m_Buckets[i].MakeDirty())
        {
          ++m_Collisions;
        }

        // Increment jump:
        i = (i + jump) % m_Buckets.Length;
      }
      while (++jumps < m_Buckets.Length);

      // VERY bad if we made it here...
      Logging.ShouldNotReach(key);
      if (fallback != -1)
      {
        $"Jumped {jumps} times before considering the fallback..."
          .LogWarning(this);

        m_Buckets[fallback].Set(key, val, hash31);
        ++m_Count;
        ++m_Version;

        if (jumps > m_Params.RehashThreshold)
        {
          Rehash();
        }

        return true;
      }

      return false;
    }


    protected void Rehash()
    {
      Rehash(m_Buckets.Length);
    }

    private void Rehash(int real_size) // new_size is assumed prime
    {
      if (m_Params.IsFixedSize && m_Buckets.Length != real_size)
      {
        $"Tried to resize a supposedly fixed-size array. old: {m_Buckets.Length} ; new: {real_size}"
          .LogError(this);
        return;
      }

      m_LoadLimit = m_Params.CalcLoadLimit(real_size);

      if (m_Count == 0 && m_Buckets.Length != real_size)
      {
        InitRuntimeBuckets(m_LoadLimit);
        return;
      }

      var new_buckets = new Bucket[real_size];
      
      m_Collisions = 0;

      int count = 0;
      int hash31, jump;
      foreach (var bucket in m_Buckets)
      {
        if (!bucket.IsEmpty)
        {
          hash31 = bucket.Hash;
          jump   = m_Params.CalcJump(hash31, real_size);

          m_Collisions += bucket.RehashClone(hash31).PlaceIn(new_buckets, hash31 % real_size, jump);

          ++count;
        }
      }

      Debug.Assert(m_Count == count, "HashMap.Rehash should not change the count of entries.");
      m_Count   = count;
      m_Buckets = new_buckets;
    }



    protected static int DefaultKeyHash31(TKey key)
    {
      return key.GetHashCode() & int.MaxValue;
    }

    protected static bool DefaultKeyEquals(TKey a, TKey b)
    {
      return DefaultEquals(a, b);
    }

    protected static bool DefaultEquals(object a, object b)
    {
      return (a != null && b != null) &&
              (a == b || a.Equals(b));
    }

  } // end class HashMap

}