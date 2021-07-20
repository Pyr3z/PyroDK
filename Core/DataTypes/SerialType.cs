/**
@file   PyroDK/Core/DataTypes/SerialType.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-18

@brief
  A data type that stores a serializable reference to a runtime type.
**/

using UnityEngine;


namespace PyroDK
{
  using Type = System.Type;


  [System.Serializable]
  public struct SerialType : System.IEquatable<SerialType>, System.IEquatable<Type>
  {
    public static readonly SerialType Any     = TSpy<object>.SerialType;
    public static readonly SerialType Invalid = new SerialType()
    {
      m_TypeName = null,
      m_RtType   = Types.Missing
    };

    
    public static SerialType FromTypeName(string typename)
    {
      return new SerialType()
      {
        m_TypeName = typename,
        m_RtType   = null
      };
    }


    public bool IsMissing => m_TypeName.IsEmpty() || Type == Types.Missing;

    public string TypeName => m_TypeName;

    public Type Type
    {
      get => m_RtType ?? ReparseType();
      set
      {
        if (value == null)
        {
          m_TypeName  = null;
          m_RtType    = Types.Missing;
        }
        else
        {
          m_TypeName  = value.GetQualifiedName();
          m_RtType    = value;
        }
      }
    }


    [SerializeField]
    private string m_TypeName;

    private Type   m_RtType;


    internal const string FIELD_TYPE_NAME  = "m_TypeName";
    internal const int    MISSING_HASHCODE = 0; // int.MinValue;


    public SerialType(Type t)
    {
      if (t == null)
      {
        m_TypeName = null;
        m_RtType   = Types.Missing;
      }
      else
      {
        m_TypeName = t.GetQualifiedName();
        m_RtType   = t;
      }
    }


    public bool IsMissingOr<TOr>()
    {
      return IsMissing || Type == typeof(TOr);
    }


    public bool TryReparseType()
    {
      var prev_t = m_RtType;
      return ReparseType() != prev_t;
    }
    private Type ReparseType()
    {
      if (Assemblies.FindType(m_TypeName, out m_RtType))
        return m_RtType;

      return m_RtType = Types.Missing;
    }


    public string LogName()
    {
      return Type.GetRichLogName();
    }

    public override string ToString()
    {
      if (m_TypeName.IsEmpty())
        return "(Default Missing Type)";

      var t = Type;

      if (t == Types.Missing)
        return $"(MissingType:{m_TypeName})";

      return t.Name;
    }

    public override int GetHashCode()
    {
      var t = Type;

      if (t == Types.Missing)
        return MISSING_HASHCODE;

      return t.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return (obj is Type        t &&  t      == Type) ||
             (obj is SerialType st && st.Type == Type);
    }

    public bool Equals(SerialType other)
    {
      return Type == other.Type;
    }

    public bool Equals(Type other)
    {
      return Type == other;
    }



    public bool AssignableFrom<T>(T obj)
    {
      if (obj == null)
      {
        return ( Type.IsClass && TSpy<T>.Type == typeof(object) ) ||
                 TSpy<T>.IsCastableTo<T>();
      }

      return Type.IsAssignableFrom(obj.GetType());
    }

    public bool AssignableFrom<T>()
    {
      return Type.IsAssignableFrom(TSpy<T>.Type);
    }

    public bool AssignableFrom(Type t)
    {
      return t != null && Type.IsAssignableFrom(t);
    }


    public bool AssignableTo<T>(T obj)
    {
      if (obj == null)
        return TSpy<T>.IsAssignableFrom(Type);

      return obj.GetType().IsAssignableFrom(Type);
    }

    public bool AssignableTo<T>()
    {
      return TSpy<T>.IsAssignableFrom(Type);
    }

    public bool AssignableTo(Type t)
    {
      return t != null && t.IsAssignableFrom(Type);
    }


    public bool Matches<T>(T obj)
    {
      if (obj == null)
        return Type == TSpy<T>.Type;

      return Type == obj.GetType();
    }

    public bool Matches<T>()
    {
      return Type == TSpy<T>.Type;
    }

    public bool Matches(Type t)
    {
      if (t == null)
        return IsMissing;

      return Type == t;
    }



    public static implicit operator bool (SerialType st)
    {
      return !st.IsMissing;
    }

    public static implicit operator Type (SerialType st)
    {
      var t = st.Type;

      if (t == Types.Missing)
        return null;

      return t;
    }

    
    public static bool operator == (SerialType lhs, Type rhs)
    {
      if (rhs == null)
        return lhs.IsMissing;

      return lhs.Type == rhs;
    }

    public static bool operator != (SerialType lhs, Type rhs)
    {
      if (rhs == null)
        return !lhs.IsMissing;

      return lhs.Type != rhs;
    }

  }

}