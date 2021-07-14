/**
@file   PyroDK/Core/DataTypes/TypedStringKey.cs
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
  public struct TypedStringKey : System.IEquatable<TypedStringKey>
  {

    public static TypedStringKey ScratchKey = new TypedStringKey()
    {
      Type = SerialTypeCode.Null,
      String = string.Empty
    };


    [SerializeField]
    public SerialTypeCode Type;

    [SerializeField]
    public string String;


    public bool Set<TValue>(string key)
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
      return (obj is TypedStringKey other && Equals(other));
    }

    public bool Equals(TypedStringKey other)
    {
      return  Type == other.Type &&
              string.Equals(String, other.String);
    }
  }

}