/**
@file   PyroDK/Core/BaseTypes/IHashSlot.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-15

@brief
Interface for the "slot" concept, for use mainly in data structures.
**/

#if false

namespace PyroDK
{

  public interface IHashSlot
  {
    object  Key       { get; }
    object  Value     { get; }
    int     Hash      { get; }
    bool    IsEmpty   { get; }
    bool    IsDirty   { get; }
    bool    IsSmeared { get; }

    bool MakeDirty();
    void Smear();
    void Clear();

    void Set(object key, object val, int hash31);
    IHashSlot RehashClone(int hash31);
  }

  public interface IHashSlot<TKey, TValue> : IHashSlot
  {
    new TKey    Key   { get; }
    new TValue  Value { get; set; }

    void Set(TKey key, TValue val, int hash31);

    new IHashSlot<TKey, TValue> RehashClone(int hash31);

    bool TryGetTuple(out (TKey key, TValue val) kvp);
  }

  public struct DefaultHashSlot<TKey, TValue> : IHashSlot<TKey, TValue>
  {
    public TKey Key => (TKey)m_Key;
    object IHashSlot.Key => m_Key;

    public TValue Value
    {
      get => m_Value;
      set => m_Value = value;
    }
    object IHashSlot.Value => m_Value;

    public int  Hash => m_DirtyHash & int.MaxValue;

    public bool IsEmpty   => m_Key == null;
    public bool IsDirty   => m_DirtyHash < 0;
    public bool IsSmeared => m_Key == null && m_DirtyHash < 0;


    private object  m_Key;
    private TValue  m_Value;
    private int     m_DirtyHash;


    public DefaultHashSlot(TKey key, TValue val, int hash31)
    {
      m_Key       = key;
      m_Value     = val;
      m_DirtyHash = hash31;
    }


    public bool TryGetTuple(out (TKey key, TValue val) kvp)
    {
      kvp = default;

      if (m_Key != null && m_Key is TKey key)
      {
        kvp.key = key;

        if (m_Value is TValue val)
          kvp.val = val;

        return true;
      }

      return false;
    }

    public void Set(TKey key, TValue val, int hash31)
    {
      m_Key       = key;
      m_Value     = val;
      m_DirtyHash = hash31 | (m_DirtyHash & Bitwise.SIGN_BIT_32);
    }
    void IHashSlot.Set(object key, object val, int hash31)
    {
      if (key is TKey k && val is TValue v)
      {
        Set(k, v, hash31);
      }
      else
      {
        throw new System.ArgumentException();
      }
    }

    public bool MakeDirty()
    {
      if (m_DirtyHash < 0)
        return false;

      m_DirtyHash |= Bitwise.SIGN_BIT_32;
      return true;
    }
    public void Smear()
    {
      m_Key       = null;
      m_Value     = default;
      m_DirtyHash = m_DirtyHash & Bitwise.SIGN_BIT_32;
    }
    public void Clear()
    {
      m_Key       = null;
      m_Value     = default;
      m_DirtyHash = 0;
    }

    public IHashSlot<TKey, TValue> RehashClone(int hash31)
    {
      return new DefaultHashSlot<TKey, TValue>(Key, m_Value, hash31);
    }
    IHashSlot IHashSlot.RehashClone(int hash31)
    {
      return new DefaultHashSlot<TKey, TValue>(Key, m_Value, hash31);
    }
  }

}

#endif