/**
@file   PyroDK/Core/DataTypes/ValidatedCollection.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-20

@brief
  A generic, read-only runtime collection type that
  validates contained elements before iterate time.
**/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  public class ValidatedCollection<T> : IReadOnlyCollection<T>
  {
    public delegate bool Validator(T element);


    public int Count => m_Count;


    private IReadOnlyCollection<T> m_Collection;

    private bool[] m_IsInvalidIndex; // "is invalid" so the default value "is valid" is true
    private int    m_Count;


    public ValidatedCollection(IReadOnlyCollection<T> collection, Validator validator)
    {
      Debug.Assert(collection != null);

      m_Collection = collection;

      m_Count = collection.Count;

      m_IsInvalidIndex = new bool[m_Count];

      if (validator == null)
        return;

      int i = 0;
      foreach (var element in collection)
      {
        if (( m_IsInvalidIndex[i++] = !validator(element) ))
          --m_Count;
      }
    }


    public IEnumerator<T> GetEnumerator()
    {
      int i = 0;
      foreach (var element in m_Collection)
      {
        if (i >= m_Count)
          yield break;

        if (m_IsInvalidIndex[i++])
          continue;

        yield return element;
      }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  }


  public class ValidatedCollection<T, U>
  {
    public sealed class TCollection : IReadOnlyCollection<T>
    {
      public int Count => m_Parent.Count;


      [System.NonSerialized]
      private ValidatedCollection<T, U> m_Parent;


      internal TCollection(ValidatedCollection<T, U> parent)
      {
        m_Parent = parent;
      }


      public IEnumerator<T> GetEnumerator()
      {
        int i = 0, ilen = m_Parent.m_Count;
        foreach (var element in m_Parent.m_TCollection)
        {
          if (i >= ilen)
            yield break;

          if (m_Parent.m_IsInvalidIndex[i++])
            continue;

          yield return element;
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        int i = 0, ilen = m_Parent.m_Count;
        foreach (var element in m_Parent.m_TCollection)
        {
          if (i >= ilen)
            yield break;

          if (m_Parent.m_IsInvalidIndex[i++])
            continue;

          yield return element;
        }
      }
    }
    public sealed class UCollection : IReadOnlyCollection<U>
    {
      public int Count => m_Parent.Count;


      [System.NonSerialized]
      private ValidatedCollection<T, U> m_Parent;


      internal UCollection(ValidatedCollection<T, U> parent)
      {
        m_Parent = parent;
      }


      public IEnumerator<U> GetEnumerator()
      {
        int i = 0, ilen = m_Parent.m_Count;
        foreach (var element in m_Parent.m_UCollection)
        {
          if (i >= ilen)
            yield break;

          if (m_Parent.m_IsInvalidIndex[i++])
            continue;

          yield return element;
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        int i = 0, ilen = m_Parent.m_Count;
        foreach (var element in m_Parent.m_UCollection)
        {
          if (i >= ilen)
            yield break;

          if (m_Parent.m_IsInvalidIndex[i++])
            continue;

          yield return element;
        }
      }
    }


    public delegate bool TValidator(T element);
    public delegate bool UValidator(U element);


    public int Count => m_Count;


    private IReadOnlyCollection<T> m_TCollection;
    private IReadOnlyCollection<U> m_UCollection;

    private bool[] m_IsInvalidIndex; // "is invalid" so the default value "is valid" is true
    private int    m_Count;


    public ValidatedCollection(IReadOnlyCollection<T> ts,
                               IReadOnlyCollection<U> us,
                               TValidator tvalid,
                               UValidator uvalid)
    {
      Debug.Assert(ts != null && us != null);

      m_TCollection = ts;
      m_UCollection = us;

      m_Count = Mathf.Min(ts.Count, us.Count);

      m_IsInvalidIndex = new bool[m_Count];

      if (tvalid == null)
      {
        if (uvalid == null)
          return;

        tvalid = (t) => true;
      }
      else if (uvalid == null)
      {
        uvalid = (u) => true;
      }

      var uiter = us.GetEnumerator();
      int i = 0;
      foreach (var t in ts)
      {
        _ = uiter.MoveNext();

        if (( m_IsInvalidIndex[i++] = !( tvalid(t) && uvalid(uiter.Current) ) ))
          --m_Count;
      }
    }


    public TCollection GetTCollection()
    {
      return new TCollection(this);
    }

    public UCollection GetUColllection()
    {
      return new UCollection(this);
    }

  }

}