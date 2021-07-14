/**
@file   PyroDK/Core/DataTypes/HashMapParams.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-06

@brief
  Parameter struct for configuring hashmaps.
**/

using UnityEngine;


namespace PyroDK
{

  [System.Serializable]
  public struct HashMapParams
  {
    #region Default Values

    public const int    DEFAULT_CAPACITY    = 10;
    public const int    MAXIMUM_CAPACITY    = Primes.LOOKUP10K_MAX;
    public const int    MINIMUM_CAPACITY    = Primes.USEFUL_MIN;

    public const float  DEFAULT_LOAD_FACTOR = 0.72f;
    public const float  MAXIMUM_LOAD_FACTOR = 1.0f;
    public const float  MINIMUM_LOAD_FACTOR = 0.1f;

    public const float  DEFAULT_GROW_FACTOR = 2.0f;
    public const float  MAXIMUM_GROW_FACTOR = 3.0f;
    public const float  MINIMUM_GROW_FACTOR = 1.0f;

    public const int    DEFAULT_HASH_PRIME  = 53; // System.Collections.HashHelpers uses 101

    public const bool   DEFAULT_IS_FIXED    = false;

    public static readonly HashMapParams Default = new HashMapParams(DEFAULT_CAPACITY);

    public static HashMapParams FixedCapacity(int user_capacity, float load_factor = DEFAULT_LOAD_FACTOR)
    {
      return new HashMapParams(starting_cap:  user_capacity,
                               is_fixed:      true,
                               load_factor:   load_factor,
                               hash_prime:    Primes.Next((int)(user_capacity / load_factor)));
    }

    #endregion Default Values


    public int  MinRealCapacity
    {
      get => m_MinCapacity;
      set => m_MinCapacity = value < MINIMUM_CAPACITY ? MINIMUM_CAPACITY : value;
    }
    public int  MinUserCapacity
    {
      get => CalcLoadLimit(m_MinCapacity);
      set => _ = SetMinUserCapacity(value);
    }

    public int    RehashThreshold => m_HashPrime - 1;
    
    public bool   IsFixedSize     => m_GrowFactor < MINIMUM_GROW_FACTOR;

    public float  LoadFactor      => m_LoadFactor;

    public float  GrowFactor      => m_GrowFactor;


    [SerializeField] [ReadOnly] // [Range(MINIMUM_CAPACITY, MAXIMUM_CAPACITY)]
    private int   m_MinCapacity;
    [SerializeField] [ReadOnly] // [Range(MINIMUM_LOAD_FACTOR, MAXIMUM_LOAD_FACTOR)]
    private float m_LoadFactor;
    [SerializeField] [ReadOnly] // [Range(0.0f, MAXIMUM_GROW_FACTOR)]
    private float m_GrowFactor;
    [SerializeField] [ReadOnly] // [Min(Primes.USEFUL_MIN)]
    private int   m_HashPrime;


    public HashMapParams(int   starting_cap,
                         bool  is_fixed    = DEFAULT_IS_FIXED,
                         float load_factor = DEFAULT_LOAD_FACTOR,
                         float grow_factor = DEFAULT_GROW_FACTOR,
                         int   hash_prime  = DEFAULT_HASH_PRIME)
    {
      m_MinCapacity = MINIMUM_CAPACITY;
      m_LoadFactor  = load_factor.Clamp(MINIMUM_LOAD_FACTOR,  MAXIMUM_LOAD_FACTOR);

      if (is_fixed)
        m_GrowFactor = 0.0f;
      else if (grow_factor < MINIMUM_GROW_FACTOR)
        m_GrowFactor = MINIMUM_GROW_FACTOR;
      else if (MAXIMUM_GROW_FACTOR < grow_factor)
        m_GrowFactor = MAXIMUM_GROW_FACTOR;
      else
        m_GrowFactor = grow_factor;

      m_HashPrime   = hash_prime;

      if (FixHashPrime())
      {
        $"({hash_prime}) was not a valid hash prime; fixed to ({m_HashPrime})"
          .LogWarning();
      }

      // deferred set, in case we had to fix m_HashPrime:
      m_MinCapacity = CalcRealCapacity(starting_cap);
    }

    private bool FixHashPrime() // returns true if m_HashPrime changed; false otherwise.
    {
      if (m_HashPrime == DEFAULT_HASH_PRIME)
        return false;

      if (m_HashPrime == int.MinValue)
      {
        m_HashPrime = DEFAULT_HASH_PRIME;
        return true;
      }

      bool changed = false;

      if (m_HashPrime < 0)
      {
        m_HashPrime &= int.MaxValue;
        changed = true;
      }

      if (!Primes.IsPrime(m_HashPrime))
      {
        m_HashPrime = Primes.Next(m_HashPrime);
        changed = true;
      }

      return changed;
    }


    public bool Check()
    {
      return  (MINIMUM_CAPACITY <= m_MinCapacity && m_MinCapacity <= MAXIMUM_CAPACITY)      &&
              (MINIMUM_LOAD_FACTOR <= m_LoadFactor && m_LoadFactor <= MAXIMUM_LOAD_FACTOR)  &&
              (m_GrowFactor <= MAXIMUM_GROW_FACTOR)                                         &&
              (m_HashPrime == DEFAULT_HASH_PRIME || Primes.IsPrime(m_HashPrime));
    }

    
    public int MakeBuckets<T>(int user_cap, out T[] buckets)
    {
      user_cap = CalcRealCapacity(user_cap);
      // "user_cap" is now actually the "real" capacity

      buckets = new T[user_cap];
      return CalcLoadLimit(user_cap);
    }


    public int CalcRealCapacity(int user_capacity)
    {
      return Primes.NextHashSafe(start:     (int)(user_capacity / m_LoadFactor),
                                 hashprime: m_HashPrime,
                                 min:       MINIMUM_CAPACITY,
                                 max:       MAXIMUM_CAPACITY);
    }

    public int CalcLoadLimit(int real_size)
    {
      return (int)(m_LoadFactor * real_size);
    }

    public int CalcJump(int hash31, int size)
    {
      return 1 + (hash31 * m_HashPrime).SetSignBit(false) % (size - 1);
    }

    public int CalcNextSize(int prev_size)
    {
      return Primes.GrowSize(prev_size:   prev_size,
                             hashprime:   m_HashPrime,
                             grow_factor: m_GrowFactor);
    }


    public int SetMinUserCapacity(int min_user_capacity)
    {
      return m_MinCapacity = CalcRealCapacity(min_user_capacity);
    }


    public override string ToString()
    {
      return $"( CAP: {m_MinCapacity}; LF: {m_LoadFactor}; GF: {m_GrowFactor}; HP: {m_HashPrime} )";
    }


    public static implicit operator HashMapParams (int min_cap)
    {
      return new HashMapParams(min_cap);
    }

  }

}