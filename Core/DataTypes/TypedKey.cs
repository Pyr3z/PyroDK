/**
@file   PyroDK/Core/DataTypes/TypedKey.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-22

@brief
  A [System.Serializable] string key that is also made unique
  with a specific SerialTypeCode associated with it.
**/

using UnityEngine;


namespace PyroDK
{

  [System.Serializable]
  public struct TypedKey : System.IEquatable<TypedKey>
  {
    //public static TypedKey Scratch = new TypedKey()
    //{
    //  Type = SerialTypeCode.Null,
    //  String = string.Empty
    //};

    public static TypedKey Make(SerialTypeCode type, string key)
    {
      return new TypedKey()
      {
        Type   = type,
        String = key
      };
    }

    public static TypedKey Make<T>(string key)
    {
      return new TypedKey()
      {
        Type   = TSpy<T>.SerialCode,
        String = key
      };
    }


    [SerializeField]
    public SerialTypeCode Type;
    [SerializeField]
    public string         String;


    public bool CheckedSet<TValue>(string key)
    {
      if (TSpy<TValue>.SerialCode > SerialTypeCode.Unsupported)
      {
        String = key ?? string.Empty;
        Type   = TSpy<TValue>.SerialCode;
        return true;
      }

      return false;
    }

    public override int GetHashCode()
    {
      return Hashing.MakeHash(Type, String);
    }

    public override string ToString()
    {
      return $"({Type},{String})";
    }

    public override bool Equals(object obj)
    {
      return (obj is TypedKey other && Equals(other));
    }

    public bool Equals(TypedKey other)
    {
      return  Type == other.Type &&
              string.Equals(String, other.String);
    }


    public static implicit operator bool (TypedKey tkey)
    {
      return tkey.Type > SerialTypeCode.Unsupported && tkey.String != null;
    }

  }

}