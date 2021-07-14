/**
@file   PyroDK/Core/DataTypes/ObjectPool.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-18

@brief
  Defines the `ObjectPool<T>` generic type that gives you memory-friendly
  access to object instances.
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{

  public interface IObjectPool
  {
  }

  public interface IPromiseKeeper : System.IDisposable
  {
    int Count { get; }
  }


  public static class PromiseKeepers
  {

    public static bool IsVoid(this IPromiseKeeper keeper)
    {
      return keeper == null || keeper.Count == 0;
    }

    public static void SafeDispose(this System.IDisposable keeper)
    {
      if (keeper != null)
        keeper.Dispose();
    }

  }


  public sealed class ObjectPool<T>
    where T : class, new()
  {
    public sealed class PromiseKeeper : IPromiseKeeper
    {
      public bool IsDisposed  => m_Promises == null;
      public bool IsBorrowed  => Count > 0 && m_Pool != null;
      public bool IsStolen    => Count > 0 && m_Pool == null;

      public T    this[int i] => m_Promises[i];
      public int  Count       => m_Promises?.Length ?? 0;

      public T    Object  => m_Promises?[0];
      public T[]  Array   => m_Promises;


      private ObjectPool<T> m_Pool;
      private T[]           m_Promises;


      internal PromiseKeeper(ObjectPool<T> pool)
      {
        m_Pool     = pool;
        m_Promises = new T[] { pool.Borrow() };
      }
      internal PromiseKeeper(ObjectPool<T> pool, int count)
      {
        m_Pool     = pool;
        m_Promises = pool.BorrowBulk(count);
      }
      internal PromiseKeeper(ObjectPool<T> pool, ref T stolen)
      {
        if (stolen == null)
        {
          m_Pool = pool;
          stolen = pool.Borrow();
        }
        else
        {
          m_Pool = null;
        }

        m_Promises = new T[] { stolen };
      }


      public void Dispose()
      {
        if (m_Pool != null)
        {
          int i = Count;
          while (i --> 0)
          {
            if (m_Promises[i] != null)
            {
              m_Pool.Return(m_Promises[i]);
            }
          }
        }

        m_Pool = null;
        m_Promises = null;
      }

    }


    public delegate void ObjectCallback(T obj);
    public delegate T    ObjectMaker();


    public int TotalCount       => m_TotalCount;
    public int AliveCount       => m_TotalCount - m_Pool.Count;
    public int IdleCount        => m_Pool.Count;
    public int StartingCapacity => m_StartCap;
    public int CurrentCapacity  => m_Pool.Capacity;


    private readonly List<T> m_Pool;


    private ObjectMaker    m_MakeObject;
    private ObjectCallback m_Destructor;
    private ObjectCallback m_OnBorrow;
    private ObjectCallback m_OnReturn;

    private int  m_TotalCount, m_StartCap;
    private bool m_NotifyBeyondStartCap;


    public ObjectPool(ObjectMaker     make_obj    = null,
                      ObjectCallback  destructor  = null,
                      ObjectCallback  on_borrow   = null,
                      ObjectCallback  on_return   = null,
                      int             start_with  = 4,
                      int             capacity    = 8,
                      bool notify_exceed_capacity = true)
    {
      m_MakeObject = make_obj ?? MakeDefaultObject;
      m_Destructor = destructor;
      m_OnBorrow   = on_borrow;
      m_OnReturn   = on_return;
      m_StartCap   = capacity;

      m_NotifyBeyondStartCap = notify_exceed_capacity;

      //if (capacity < 1)
      //  capacity = 1;

      //while (start_with >= capacity)
      //  capacity *= 2;

      m_Pool = new List<T>(capacity);

      while (start_with --> 0)
        m_Pool.Add(MakeObject());
    }

    ~ObjectPool()
    {
      if (m_Destructor != null)
      {
        foreach (var obj in m_Pool)
        {
          if (obj != null)
            m_Destructor(obj);
        }
      }
    }


    public T Borrow()
    {
      T result;

      if (m_Pool.Count == 0)
        result = MakeObject();
      else
        result = m_Pool.PopBack();

      m_OnBorrow?.Invoke(result);

      return result;
    }

    public T[] BorrowBulk(int count)
    {
      if (Logging.Assert(count > 0))
        return null;

      var result = new T[count];

      if (m_Pool.Count == 0)
      {
        while (count --> 0)
          result[count] = MakeObject();
      }
      else if (m_Pool.Count > count)
      {
        int i = m_Pool.Count - count;
        m_Pool.CopyTo(i, result, 0, count);
        m_Pool.RemoveRange(i, count);
      }
      else
      {
        int i = m_Pool.Count;
        m_Pool.CopyTo(result);
        m_Pool.Clear();
        while (count-- > i)
          result[count] = MakeObject();
      }

      if (m_OnBorrow != null)
      {
        foreach (var obj in result)
          m_OnBorrow(obj);
      }

      return result;
    }

    public bool NotifyExternalBorrow(T external)
    {
      for (int i = m_Pool.Count - 1; i >= 0; --i)
      {
        if (m_Pool[i] == external)
        {
          m_Pool.RemoveAt(i);

          m_OnBorrow?.Invoke(external);

          return true;
        }
      }

      return false;
    }

    public void Return(T borrowed, out T old_reference)
    {
      old_reference = null;

      if (Logging.AssertNonNull(borrowed))
        return;

      m_OnReturn?.Invoke(borrowed);

      m_Pool.PushBack(borrowed);
    }

    public void Return(T borrowed)
    {
      if (Logging.AssertNonNull(borrowed))
        return;

      m_OnReturn?.Invoke(borrowed);

      m_Pool.PushBack(borrowed);
    }


    public PromiseKeeper MakePromise()
    {
      return new PromiseKeeper(this);
    }

    public PromiseKeeper MakePromise(out T borrowed)
    {
      var keeper = new PromiseKeeper(this);
      borrowed = keeper[0];
      return keeper;
    }

    public PromiseKeeper MakePromises(int count)
    {
      return new PromiseKeeper(this, count);
    }

    public PromiseKeeper MakePromiseIfNull(ref T maybe_null)
    {
      return new PromiseKeeper(this, ref maybe_null);
    }



    private T MakeObject()
    {
      ++m_TotalCount;

      #if DEBUG
      if (m_NotifyBeyondStartCap && m_TotalCount >= m_StartCap)
      {
        $"{TSpy<ObjectPool<T>>.LogName} has exceeded its soft capacity of {m_StartCap}. (count={m_TotalCount})"
          .LogWarning(this);
      }
      #endif

      return m_MakeObject();
    }

    private T MakeDefaultObject() => new T();

  }

}