/**
@file   PyroDK/Core/Utilities/Types.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-24

@brief
  Provides extensions and utilities for the `System.Type` type.
**/

using System.Collections.Generic;
using System.Reflection;

using UnityEngine;


namespace PyroDK
{
  using Type          = System.Type;
  using StringBuilder = System.Text.StringBuilder;


  public static class Types
  {
    public static readonly Type Missing = Type.Missing.GetType();



    public static Type AssertValid(this Type type, string name = "type")
    {
      #if DEBUG
      if (type == null)
        throw new System.ArgumentNullException(name);
      #endif

      return type;
    }


    public static string GetQualifiedName(this Type type)
    {
      if (Logging.AssertNonNull(type))
        return string.Empty;

      string aqname = type.AssemblyQualifiedName;
      int comma = aqname.IndexOf(',');

      if (comma < 0)
      {
        Logging.ShouldNotReach(blame: type);
        return aqname;
      }

      int split = aqname.IndexOf(',', comma + 1);

      if (split < 0)
      {
        Logging.TempReached(blame: type);
        return aqname;
      }

      return aqname.Remove(split);
    }


    public static string GetLogName(this Type type)
    {
      // won't actually have any rich text in it.
      // TODO Consider separating the name-building logic from RichText?
      return RichText.TypeNamePlain(type);
    }

    public static string GetLogName(object obj)
    {
      return RichText.TypeNamePlain(obj?.GetType());
    }
    

    public static string GetRichLogName(this Type type)
    {
      if (type == null)
        return RichText.Color("<null>", Colors.GUI.TypeByRef);

      if (type.IsValueType)
        return RichText.TypeName(type, Colors.GUI.TypeByVal);

      if (TSpy<System.Attribute>.IsBaseClassOf(type))
        return RichText.Attribute(type);

      return RichText.TypeName(type, Colors.GUI.TypeByRef);
    }

    public static string GetRichLogName(object obj)
    {
      if (obj == null)
        return RichText.Color("null", Colors.GUI.Keyword);
      else
        return GetRichLogName(obj.GetType());
    }
    public static string GetRichLogName(object obj, Color32 color)
    {
      var type = obj?.GetType() ?? Missing;

      if (TSpy<System.Attribute>.IsBaseClassOf(type))
        return RichText.Attribute(type, color);

      return RichText.TypeName(type, color);
    }


    public static bool IsAttribute(this Type type)
    {
      return type.IsSubclassOf(typeof(System.Attribute));
    }

    public static bool IsUnityObject(this Type type)
    {
      return type.IsSubclassOf(typeof(Object));
    }

    public static bool IsReferencable(this Type type)
    {
      // the `type.IsInterface` check is for inheritance strictness purposes
      return type.IsSubclassOf(typeof(Object)) ||
           ( type.IsInterface && type.IsSubclassOf(typeof(IObject)) );
    }

    public static bool IsStaticClass(this Type type)
    {
      return type.IsAbstract && type.IsSealed;
    }

    public static bool IsArrayOrList(this Type type)
    {
      return (type.IsArray)
          || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>));
    }


    public static bool HasDefaultConstructor(this Type type)
    {
      return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
    }


    public static Type GetListElementType(this Type type)
    {
      if (type.IsArray)
        return type.GetElementType();

      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        return type.GetGenericArguments()[0];

      return type;
    }

    public static object ConstructDefault(this Type type)
    {
      // returns non-null, unless the type is non-default constructible

      if (type == typeof(string))
        return string.Empty; // string has no default constructor lol

      if (TSpy<Object>.IsAssignableFrom(type))
      {
        Object none = null;
        return none;
      }
      else if (HasDefaultConstructor(type))
      {
        // this will fail for any type without a default constructor.
        return System.Activator.CreateInstance(type);
      }

      return null;
    }


    public static bool TryGetField(this Type type, string name, out FieldInfo field, BindingFlags blags = TypeMembers.INSTANCE)
    {
      field = type?.GetField(name, blags);
      return field != null;
    }
    
    public static bool TryGetSerializableField(this Type type, string name, out FieldInfo field)
    {
      return TryGetField(type, name, out field, TypeMembers.INSTANCE) &&
             ( field.IsPublic || field.IsDefined<SerializeField>() );
    }

    public static bool TryGetInternalField(this Type type, string name, out FieldInfo field)
    {
      return TryGetField(type, name, out field, TypeMembers.NONPUBLIC);
    }


    public static bool TryGetStaticValue<T>(this Type type, string name, out T value)
    {
      if (type.TryGetField(name, out FieldInfo field, TypeMembers.STATIC) &&
          TSpy<T>.IsAssignableFrom(field.FieldType))
      {
        value = (T)field.GetValue(null);
        return true;
      }

      value = default;
      return false;
    }



    public static bool TryGetProperty(this Type type, string name, out PropertyInfo prop, BindingFlags blags = TypeMembers.ALL)
    {
      prop = type?.GetProperty(name, blags);
      return prop != null;
    }



    public static bool TryGetMethod(this Type type, string name, out MethodInfo method, BindingFlags blags = TypeMembers.ALL)
    {
      method = type?.GetMethod(name, blags);
      return method != null;
    }

    public static bool TryGetMethod(this Type type, string name, Type[] paramtypes, out MethodInfo method, BindingFlags blags = TypeMembers.ALL)
    {
      method = type?.GetMethod(name, blags,
                               binder:    null,
                               types:     paramtypes,
                               modifiers: new ParameterModifier[0]);
      return method != null;
    }

    public static bool TryGetMethod<TParam0>(this Type type, string name, out MethodInfo method, BindingFlags blags = TypeMembers.ALL)
    {
      method = type?.GetMethod(name, blags,
                               binder:    null,
                               types:     new Type[] { typeof(TParam0) },
                               modifiers: new ParameterModifier[0]);
      return method != null;
    }

  } // end class Types

}