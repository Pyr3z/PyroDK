/**
@file   PyroDK/Core/DataTypes/BufferList.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-30

@brief
  Provides a friendly temporary buffer class, for use in pretty much
  everything.
**/

using System.Collections;
using System.Collections.Generic;


namespace PyroDK
{

  public sealed class BufferList<T> :
    IList<T>,
    IReadOnlyCollection<T>,
    IPromiseKeeper
  {
    public bool IsDisposed => m_Buff == null;


    private List<T> m_Buff = null;

    // global pool of pre-allocated lists of this type
    private static readonly ObjectPool<List<T>> s_ListPool =
      new ObjectPool<List<T>>(on_return:  (list) => list.Clear(),
                              start_with: 2,
                              capacity:   4,
                              notify_exceed_capacity: true);

    private const int DEFAULT_CAPACITY = 8;
    public BufferList(int min_capacity = DEFAULT_CAPACITY)
    {
      Reserve(min_capacity);
    }

    public BufferList(IEnumerable<T> copy_from, int min_capacity = DEFAULT_CAPACITY)
    {
      Reserve(min_capacity);

      if (copy_from != null)
        m_Buff.AddRange(copy_from);
    }

    public BufferList(ICollection<T> copy_from)
    {
      if (copy_from == null || copy_from.Count == 0)
      {
        Reserve(DEFAULT_CAPACITY);
      }
      else
      {
        Reserve(copy_from.Count);
        m_Buff.AddRange(copy_from);
      }
    }


    public static IPromiseKeeper MakePromise(out List<T> borrowed_list, int min_capacity = DEFAULT_CAPACITY)
    {
      var keeper = new BufferList<T>(min_capacity);
      borrowed_list = keeper.m_Buff;
      return keeper;
    }


    public void Reserve(int min_capacity = DEFAULT_CAPACITY)
    {
      if (m_Buff == null)
      {
        m_Buff = s_ListPool.Borrow();
      }

      if (m_Buff.Capacity < min_capacity)
      {
        m_Buff.Capacity = min_capacity;
      }
    }

    public void Dispose()
    {
      if (m_Buff == null)
        "Tried to dispose of a BufferList more than once!".LogWarning(this);
      else
        s_ListPool.Return(m_Buff, out m_Buff);
    }


    public T[] ToArray()
    {
      return m_Buff.ToArray();
    }

    public T[] ToPackedArray()
    {
      _ = MakePackedArray(out T[] array);
      return array;
    }

    public void MakeArray(out T[] array)
    {
      array = m_Buff.ToArray();
    }

    // returns how many elements were cut out
    public int MakePackedArray(out T[] array) // guarantees no default/null elements in the out array
    {
      array = new T[m_Buff.Count];

      int i, j;

      for (i = 0, j = 0; i < m_Buff.Count; ++i)
      {
        var t = m_Buff[i];

        if (t == null || t.Equals(default))
          continue;

        array[j++] = t;
      }

      if (j < i)
      {
        var temp = new T[j];
        System.Array.Copy(array, temp, j);
        array = temp;
      }

      return i - j;
    }

    public void MakeTupleArrays<T0, T1>(out T0[]                  arr0,
                                        out T1[]                  arr1,
                                        System.Func<T, (T0, T1)>  tupler)
    {
      arr0 = new T0[m_Buff.Count];
      arr1 = new T1[m_Buff.Count];

      for (int i = 0; i < m_Buff.Count; ++i)
      {
        (arr0[i], arr1[i]) = tupler(m_Buff[i]);
      }
    }


    public bool EnsureEmptyIndex(int i)
    {
      if (i >= m_Buff.Count && i < m_Buff.Capacity)
      {
        while (m_Buff.Count <= i)
          m_Buff.Add(default);
        return true;
      }

      return i < m_Buff.Count && -1 < i;
    }


    public IEnumerable<T> ReverseEnumerate()
    {
      int i = m_Buff.Count;
      while (i --> 0)
      {
        yield return m_Buff[i];
      }
    }



    #region IList<T> interface implemented straight through BorrowedList:

    public static implicit operator List<T>(BufferList<T> buffer) => buffer.m_Buff;


    public T this[int i]
    {
      get => (i >= m_Buff.Count && i < m_Buff.Capacity) ? default : m_Buff[i];
      set
      {
        if (EnsureEmptyIndex(i))
          m_Buff[i] = value;
      }
    }

    public int  Count      => m_Buff.Count;
    public bool IsReadOnly => ((ICollection<T>)m_Buff).IsReadOnly;


    

    public bool Contains(T item)
    {
      return m_Buff.Contains(item);
    }

    public int IndexOf(T item)
    {
      return m_Buff.IndexOf(item);
    }


    public void Add(T item)
    {
      m_Buff.Add(item);
    }

    public void Insert(int i, T item)
    {
      m_Buff.Insert(i, item);
    }


    public bool Remove(T item)
    {
      return m_Buff.Remove(item);
    }

    public void RemoveAt(int i)
    {
      if (EnsureEmptyIndex(i))
        m_Buff.RemoveAt(i);
    }

    public int RemoveAll(System.Predicate<T> where)
    {
      return m_Buff.RemoveAll(where);
    }


    public void Clear()
    {
      m_Buff.Clear();
    }


    public void CopyTo(T[] array, int start_idx)
    {
      m_Buff.CopyTo(array, start_idx);
    }


    public IEnumerator<T> GetEnumerator()
    {
      return m_Buff.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion IList impl.

  }

}