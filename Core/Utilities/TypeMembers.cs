/**
@file   PyroDK/Core/Utilities/TypeMembers.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-24

@brief
  Provides extensions and utilities for the `System.Reflection.FieldInfo`
  type.
**/

using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


namespace PyroDK
{
  using Type   = System.Type;
  using Action = System.Action;

  using IncludeWhere    = System.Func<MethodInfo, bool>;
  using MethodProcessor = System.Action<MethodInfo>;


  public static class TypeMembers
  {
    public const BindingFlags       ALL = BindingFlags.Instance  |
                                          BindingFlags.Static    |
                                          BindingFlags.Public    |
                                          BindingFlags.NonPublic;

    public const BindingFlags  ALL_DECL = ALL                    |
                                          BindingFlags.DeclaredOnly;

    public const BindingFlags  INSTANCE = BindingFlags.Instance  |
                                          BindingFlags.Public    |
                                          BindingFlags.NonPublic;

    public const BindingFlags    STATIC = BindingFlags.Static    |
                                          BindingFlags.Public    |
                                          BindingFlags.NonPublic;

    public const BindingFlags    PUBLIC = BindingFlags.Public    |
                                          BindingFlags.Instance  |
                                          BindingFlags.Static;

    public const BindingFlags NONPUBLIC = BindingFlags.Instance  |
                                          BindingFlags.Static    |
                                          BindingFlags.NonPublic;

    public const BindingFlags     ENUMS = BindingFlags.Static    |
                                          BindingFlags.Public;



    public static readonly FieldInfo  MissingField  = Types.Missing.GetField("Value");
    public static readonly MethodInfo MissingMethod = typeof(TypeMembers).GetMethod("MissingMethodImpl");



    private static void MissingMethodImpl() => Logging.ShouldNotReach();


    public static string GetLogName(this FieldInfo field)
    {
      if (field == null)
        return "null";

      return $"\"{field.DeclaringType.Name}.{field.Name}\" ({field.FieldType.Name})";
    }

    public static string GetLogName(this MethodInfo m)
    {
      return $"{m.DeclaringType.FullName}.{m.Name}";
    }


    public static bool IsDefined<T>(this MemberInfo member, bool inherit = true)
      where T : System.Attribute
    {
      return member.IsDefined(typeof(T), inherit);
    }

    public static bool IsHidden(this FieldInfo field)
    {
      return field.IsDefined<HideInInspector>() ||
             field.IsDefined<System.ObsoleteAttribute>();
    }


    public static bool IsActionMethod(this MethodInfo m)
    {
      return m.GetParameters().Length == 0;
    }


    public static bool IsFieldIn(this FieldInfo field, object instance)
    {
      if (field == null)
        return false;
      if (instance == null)
        return field.IsStatic;

      var type = instance.GetType();
      return type == field.DeclaringType || type.IsSubclassOf(field.DeclaringType);
    }


    public static T GetValue<T>(this FieldInfo field, object instance = null)
    {
      Debug.Assert((instance == null) == (field.IsStatic));

      if (field.DeclaringType.IsEnum && TSpy<T>.IsIntegral)
      {
        return (T)System.Convert.ChangeType(field.GetValue(instance), TSpy<T>.Type);
      }

      Debug.Assert(TSpy<T>.IsAssignableFrom(field.FieldType));
      return (T)field.GetValue(instance);
    }

    public static bool TryGetValue<T>(this FieldInfo field, object instance, out T value)
    {
      value = default;
      if (!IsFieldIn(field, instance))
        return false;

      object box = field.GetValue(instance);

      if (box is T casted) // <-- critical difference
      {
        value = casted;
        return true;
      }

      return false;
    }


    public static void SetValue<TValue, TClass>(this FieldInfo field, TClass instance, TValue value)
      where TClass : class
    {
      Debug.Assert(TSpy<TValue>.IsCastableTo(field.FieldType));

      field.SetValue(instance, value);
    }

    public static bool TrySetValue<TValue, TClass>(this FieldInfo field, TClass instance, TValue value)
      where TClass : class
    {
      if (TSpy<TValue>.IsCastableTo(field?.FieldType))
      {
        field.SetValue(instance, value);
        return true;
      }

      return false;
    }


    public static void SetValue<TValue, TStruct>(this FieldInfo field, ref TStruct instance, TValue value)
      where TStruct : struct
    {
      Debug.Assert(TSpy<TValue>.IsCastableTo(field.FieldType));

      object boxed = instance;
      field.SetValue(boxed, value);
      instance = (TStruct)boxed;
    }

    public static bool TrySetValue<TValue, TStruct>(this FieldInfo field, ref TStruct instance, TValue value)
      where TStruct : struct
    {
      if (TSpy<TValue>.IsCastableTo(field?.FieldType))
      {
        object boxed = instance;
        field.SetValue(boxed, value);
        instance = (TStruct)boxed;

        return true;
      }

      return false;
    }


    public static bool TryGetStaticValue<T>(this FieldInfo field, out T value)
    {
      value = default;

      if (field == null)
        return false;

      object box = field.GetValue(null);

      if (box != null && TSpy<T>.Type == box.GetType())
      {
        value = (T)box;
        return true;
      }

      return false;
    }



    public static bool TryGetMethods(Type type, ref List<MethodInfo> output_list,
                                     IncludeWhere where = null,
                                     BindingFlags blags = ALL)
    {
      if (where == null)
        where = (m) => m != null;

      if (output_list == null)
      {
        output_list = type.AssertValid().GetMethods(blags)
                                        .Where(where)
                                        .ToList();
      }
      else
      {
        output_list.Clear();
        output_list.AddRange(type.AssertValid().GetMethods(blags)
                                               .Where(where));
      }

      return output_list.Count > 0;
    }

    public static bool TryGetMethodsWithAttribute<TAttr>(Type type, ref List<MethodInfo> output_list,
                                                         bool         inherit = false,
                                                         IncludeWhere where   = null,
                                                         BindingFlags blags   = ALL)
      where TAttr : System.Attribute
    {
      if (where == null)
        where = ((m) => m.IsDefined<TAttr>(inherit));
      else
        where = ((m) => m.IsDefined<TAttr>(inherit)) + where;

      return TryGetMethods(type, ref output_list, where, blags);
    }


    public static bool TryGetMethodsAsActions(Type type, ref List<Action> output_list,
                                              object          target    = null,
                                              IncludeWhere    where     = null,
                                              MethodProcessor processor = null,
                                              BindingFlags    blags     = ALL)
    {
      if (type == null)
        throw new System.ArgumentNullException("type");

      if (target == null)
      {
        blags &= ~BindingFlags.Instance;
        blags |= BindingFlags.Static;
      }

      if (where == null)
        where = IsActionMethod;
      else
        where = (m) => IsActionMethod(m) && where(m);

      if (processor != null)
      {
        where = (m) =>
        {
          if (where(m))
          {
            processor(m);
            return true;
          }

          return false;
        };
      }

      if (output_list == null)
      {
        output_list = type.GetMethods(blags)
                          .Where(where)
                          .Select(target.MethodToAction)
                          .ToList();

        return output_list.Count > 0;
      }

      int precount = output_list.Count;

      output_list.AddRange(type.GetMethods(blags)
                               .Where(where)
                               .Select(target.MethodToAction));

      return output_list.Count > precount;
    }
    private static Action MethodToAction(this object target, MethodInfo m)
    {
      if (target == null)
        return StaticMethodToAction(m);

      var result = m.CreateDelegate(typeof(Action), (m.IsStatic ? null : target)) as Action;

      if (result == null)
        result = $"Method \"{m.Name}\" was not convertible to a System.Action.".LogError;

      return result;
    }
    private static Action StaticMethodToAction(MethodInfo m)
    {
      Action result = null;

      if (m.IsStatic)
        result = m.CreateDelegate(typeof(Action)) as Action;
      else
        result = $"Method \"{m.Name}\" was incorrectly assumed static.".LogError;

      if (result == null)
        result = $"Method \"{m.Name}\" was not convertible to a System.Action.".LogError;

      return result;
    }

  }

}