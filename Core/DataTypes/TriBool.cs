/**
@file   PyroDK/Core/DataTypes/TriBool.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-06

@brief
Defines the TriBool enumeration, as well as some utilities to go along
with it.
**/

using UnityEngine;


namespace PyroDK
{

  [System.Serializable]
  public /*readonly*/ struct TriBool :
    System.IComparable<TriBool>, System.IEquatable<TriBool>
  {

    #region PUBLIC CONSTANTS

    public const int POSITIVE_SIGN  = +1;
    public const int NEGATIVE_SIGN  = -1;
    public const int NULL_VALUE     =  0;

    public static readonly TriBool Positive   = new TriBool(POSITIVE_SIGN);
    public static readonly TriBool Negative   = new TriBool(NEGATIVE_SIGN);
    public static readonly TriBool Zero       = new TriBool(NULL_VALUE);

    public static readonly TriBool True       = new TriBool(POSITIVE_SIGN);
    public static readonly TriBool False      = new TriBool(NEGATIVE_SIGN);
    public static readonly TriBool Null       = new TriBool(NULL_VALUE);

    #endregion


    public int  Value       => m_Value;

    public bool IsPositive  => 0 < m_Value;
    public bool IsNegative  => m_Value < 0;
    public bool IsZero      => m_Value == 0;

    public bool IsSpecial   => m_Value < -1 || 1 < m_Value;


    [SerializeField]
    private /*readonly*/ int m_Value;


    private TriBool(int raw_value)
    {
      m_Value = raw_value;
    }


    public static TriBool FromSign(int sign)
    {
      if (sign < 0) return Negative;
                    return (0 < sign) ? Positive : Zero;
    }

    public static TriBool FromBool(bool? b)
    {
      if (b == null)  return Null;
                      return (bool)b ? True : False;
    }

    public static TriBool FromBool(bool b)
    {
      return b ? True : False;
    }

    public static TriBool FromEnum<T>(T enum_value) where T : System.Enum
    {
      return new TriBool(enum_value?.GetHashCode() ?? 0);
    }


    public static TriBool MakeSpecial(int special_value)
    {
      return new TriBool(special_value);
    }


    
    public bool IsSign(int sign)
    {
      if (sign < 0) return m_Value  < 0;
      if (sign > 0) return m_Value  > 0;
                    return m_Value == 0;
    }


    public override bool Equals(object other)
    {
      if (other == null)
        return m_Value == 0;

      return other.GetHashCode() == GetHashCode();
    }

    public override int GetHashCode()
    {
      return m_Value;
    }

    public int CompareTo(TriBool other)
    {
      return m_Value - other.m_Value;
    }

    public bool Equals(TriBool other)
    {
      return m_Value == other.m_Value;
    }



    #region OPERATORS

    public static implicit operator bool? (TriBool tribool)
    {
      if (tribool.m_Value == 0)
        return null;

      return tribool.m_Value > 0;
    }

    public static implicit operator TriBool (bool? b)
    {
      if (b == null)  return Null;
                      return (bool)b ? True : False;
    }


    public static implicit operator bool (TriBool tribool)
    {
      return 0 < tribool.m_Value;
    }

    public static implicit operator TriBool (bool b)
    {
      return b ? True : False;
    }



    public static implicit operator int (TriBool tribool)
    {
      return tribool.m_Value;
    }

    public static implicit operator TriBool (int value)
    {
      return new TriBool(value);
    }


    // make true if false, or false if true
    public static TriBool operator ! (TriBool tribool)
    {
      if (tribool.m_Value < 0)
        return True;

      if (tribool.m_Value > 0)
        return False;

      return tribool;
    }

    // make true if null, or null if not-null
    public static TriBool operator ~ (TriBool tribool)
    {
      if (tribool.m_Value == 0)
        return True;

      return Null;
    }

    // interpreted as "set-if-null"
    public static TriBool operator / (TriBool lhs, bool rhs)
    {
      if (lhs.m_Value != 0)
        return lhs;

      if (rhs)
        return True;

      return False;
    }

    // interpreted as "set-if-not-null"
    public static TriBool operator * (TriBool lhs, bool rhs)
    {
      if (lhs.m_Value == 0)
        return lhs;

      if (rhs)
        return True;

      return False;
    }


    public static bool operator == (TriBool lhs, TriBool rhs)
    {
      return lhs.m_Value == rhs.m_Value;
    }

    public static bool operator != (TriBool lhs, TriBool rhs)
    {
      return lhs.m_Value != rhs.m_Value;
    }

    #endregion

  }

}
