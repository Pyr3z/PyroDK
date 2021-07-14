/**
@file   PyroDK/Core/Utilities/TSpy.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-09

@brief
  Provides a "spy" helper that caches commonly queried properties
  of `System.Type`s.
**/

using UnityEngine;


namespace PyroDK
{
  using Type       = System.Type;
  using TypeCode   = System.TypeCode;
  using MemberInfo = System.Reflection.MemberInfo;


  public static class TSpy<T>
  {

    public static readonly Type     Type  = typeof(T);
    public static readonly int      Hash  = Type.GetHashCode();
    public static readonly TypeCode Code  = Type.GetTypeCode(Type);

    public static readonly Type ElementType = Type.GetListElementType();

    public static readonly SerialType     SerialType  = new SerialType(Type);
    public static readonly SerialTypeCode SerialCode  = Serializer.RuntimeTypeToCode(Type);

    public static readonly string Name      = Type.Name;
    public static readonly string FullName  = Type.FullName;
    public static readonly string AQName    = Type.GetQualifiedName();
    public static readonly string LogName   = Type.GetRichLogName();

    public static readonly bool IsClass        = Type.IsClass || Type.IsInterface;
    public static readonly bool IsAttribute    = Type.IsAttribute();
    public static readonly bool IsUnityObject  = Type.IsUnityObject();
    public static readonly bool IsReferencable = Type.IsReferencable();
    public static readonly bool IsArrayOrList  = Type.IsArrayOrList();
    public static readonly bool IsIntegral     = Type.IsPrimitive && TypeCode.Boolean < Code &&
                                                                     Code < TypeCode.Single;



    public static bool Equals<TOther>()
    {
      return Type == typeof(TOther);
    }

    public static bool Equals(Type type)
    {
      return Type == type;
    }

    public static bool IsCastableTo<U>(U other = default)
    {
      if (TSpy<U>.Type == TSpy<object>.Type && other != null)
        return other.GetType().IsAssignableFrom(Type);

      return TSpy<U>.Type.IsAssignableFrom(Type);
    }

    public static bool IsCastableTo(Type u)
    {
      return u.IsAssignableFrom(Type);
    }


    public static bool IsAssignableFrom<U>(U other)
    {
      if (other == null)
      {
        return ( IsClass && TSpy<U>.Type == typeof(object) ) ||
                 Type.IsAssignableFrom(TSpy<U>.Type);
      }

      return Type.IsAssignableFrom(other.GetType());
    }

    public static bool IsAssignableFrom(Type u)
    {
      return Type.IsAssignableFrom(u);
    }


    public static bool IsInstance<U>(U instance)
    {
      return Type == typeof(U) || ( instance != null && Type == instance.GetType() );
    }


    public static bool IsBaseClassOf<U>(U other)
    {
      if (other == null)
      {
        return IsClass && TSpy<U>.Type.IsSubclassOf(Type);
      }

      return IsClass && other.GetType().IsSubclassOf(Type);
    }

    public static bool IsBaseClassOf(Type u)
    {
      return u.IsSubclassOf(Type);
    }


    public static bool IsSubClassOf<U>()
    {
      return Type.IsSubclassOf(TSpy<U>.Type);
    }

    public static bool IsSubClassOf(Type u)
    {
      return Type.IsSubclassOf(u);
    }


    public static bool IsAttributeOn(MemberInfo finfo)
    {
      return System.Attribute.IsDefined(finfo, Type);
    }

    public static bool IsAttributeOn(MemberInfo finfo, bool inherit)
    {
      if (!IsAttribute)
      {
        $"The type argument \"{Name}\" is not an Attribute!".LogError();
        return false;
      }

      return System.Attribute.IsDefined(finfo, Type, inherit);
    }


    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogMissingReference(Object ctx)
    {
      $"Missing reference to {LogName}."
        .LogError(ctx);
    }

  }

}