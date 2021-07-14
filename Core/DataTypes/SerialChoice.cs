/**
@file   PyroDK/Core/DataTypes/SerialChoice.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-25

@brief
  A structure for generically AND efficiently identifying,
  weighting, and storing the states of a "Choice" concept.
**/

using UnityEngine;


namespace PyroDK
{

  [System.Serializable]
  public class SerialChoice : IChoice<SerialChoice>
  {

    public static readonly SerialChoice None = new SerialChoice(int.MinValue, int.MinValue, -1);


    public int  Hash          => m_Hash;
    public int  Weight        => Mathf.Max(m_Weight, 0);
    public bool IsEliminated  => m_Weight < 0;
    public int  Index         => m_Index;


    [SerializeField]
    private int m_Hash;
    [SerializeField]
    private int m_Weight;
    [SerializeField]
    private int m_Index;


    public SerialChoice(int choice_hash, int weight, int idx)
    {
      m_Hash    = choice_hash;
      m_Weight  = weight;
      m_Index   = idx;
    }


    public void SetIndex(int idx)
    {
      m_Index = idx;
    }

    public void SetWeight(int w)
    {
      m_Weight = Mathf.Max(w, 0).SetSignBit(IsEliminated);
    }

    public void AddWeight(int w)
    {
      if (IsEliminated)
        m_Weight = Mathf.Max(m_Weight.SetSignBit(false) + w, 0).SetSignBit(true);
      else
        m_Weight = Mathf.Max(m_Weight + w, 0);
    }

    public int Eliminate()
    {
      if (m_Weight < 0)
        return 0;

      int old_weight = m_Weight;
      m_Weight = m_Weight.SetSignBit(true);
      return old_weight;
    }

    public int Restore()
    {
      if (m_Weight < 0)
        return m_Weight = m_Weight.SetSignBit(false);

      return 0;
    }


    public T As<T>() where T : System.Enum
    {
      return (T)System.Enum.ToObject(TSpy<T>.Type, m_Hash);
    }

    public bool TryCast<T>(out T enumval) where T : System.Enum
    {
      if (!this || !System.Enum.IsDefined(TSpy<T>.Type, m_Hash))
      {
        enumval = default;
        return false;
      }

      enumval = As<T>();
      return true;
    }


    public override string ToString()
    {
      return $"#{m_Hash}, ${m_Weight}";
    }

    public override int GetHashCode()
    {
      return m_Hash;
    }

    public override bool Equals(object obj)
    {
      if (obj == null || IsEliminated)
        return false;

      return m_Hash == obj.GetHashCode();
    }


    public bool Equals(SerialChoice other)
    {
      return m_Hash == other.m_Hash;
    }


    public int CompareTo(SerialChoice rhs)
    {
      return Weight - rhs.Weight;
    }



    public static implicit operator bool (SerialChoice choice)
    {
      return !(choice is null) && choice.m_Weight >= 0;
    }


    public static bool operator == (SerialChoice lhs, object rhs)
    {
      return !(lhs is null) && lhs.Equals(rhs);
    }

    public static bool operator != (SerialChoice lhs, object rhs)
    {
      return !(lhs == rhs);
    }

  }

}