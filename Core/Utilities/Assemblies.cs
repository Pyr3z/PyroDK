/**
@file   PyroDK/Core/Utilities/Assemblies.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-14

@brief
  Utilities for `System.Reflection.Assembly`s.
**/

using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEngine;


namespace PyroDK
{
  using Type = System.Type;


  public static class Assemblies
  {
    private static readonly HashMap<string, Assembly> s_AssemblyMap = new HashMap<string, Assembly>(150);

    private static readonly HashSet<Assembly> s_UserAssemblies = new HashSet<Assembly>();

    private static readonly string[] s_NonUserAssemblyKeywords = new List<string>()
    {
      "Unity",
      "Cinemachine",
      "mscorlib",
      "System",
      "Mono",
      "Samples",
      "nunit",
      "ICSharpCode"
    } .Sorted(Strings.CompareOrdinalNoCase)
      .ToArray();



    public static bool FindType(string typename, out Type type)
    {
      if (typename.IsEmpty() || typename.EndsWithAny(' ', ','))
      {
        type = null;
        return false;
      }

      type = Type.GetType(typename, AssemblyResolver, TypeResolver);
      return type != null;
    }

    public static bool FindSubType(string typename, Type base_type, out Type type)
    {
      if (typename.IsEmpty() || typename.EndsWithAny(' ', ','))
      {
        type = null;
        return false;
      }

      type = Type.GetType(typename, AssemblyResolver, TypeResolver);

      if (type == null)
      {
        foreach (var t in GetUserTypesDerivedFrom(base_type))
        {
          if (typename.StartsWith(t.FullName))
          {
            type = t;
            return true;
          }
        }
      }

      return type != null && type.IsSubclassOf(base_type);
    }


    public static bool Find(string name, out Assembly ass)
    {
      if (s_AssemblyMap.Count == 0)
        ReloadAssemblyCaches();
      return s_AssemblyMap.Find(name, out ass);
    }


    public static IEnumerable<Assembly> GetUserAssemblies(bool reload_cache = false)
    {
      if (s_UserAssemblies.Count == 0 || reload_cache)
        ReloadAssemblyCaches();
      return s_UserAssemblies;
    }

    public static void ReloadAssemblyCaches()
    {
      s_AssemblyMap.Clear();
      s_UserAssemblies.Clear();

      foreach (var ass in System.AppDomain.CurrentDomain.GetAssemblies())
      {
        if (!ass.IsDynamic)
        {
          string name = ass.GetSimpleName();

          if (s_AssemblyMap.Map(name, ass) && !name.ContainsKeyword(s_NonUserAssemblyKeywords))
            s_UserAssemblies.Add(ass);
        }
      }
    }

    public static IEnumerable<Type> GetAllUserTypes(System.Func<Type, bool>     where_type  = null,
                                                    System.Func<Assembly, bool> where_ass   = null)
    {
      if (where_type == null)
        where_type = (t) => true;

      if (where_ass == null)
        where_ass = (a) => true;

      foreach (var user_ass in GetUserAssemblies().Where(where_ass))
      {
        foreach (var type in user_ass.GetTypes().Where(where_type))
        {
          yield return type;
        }
      }

      yield break;
    }

    public static IEnumerable<Type> GetUserTypesDerivedFrom(Type basetype)
    {
      return GetAllUserTypes(where_type: (t) => t.IsSubclassOf(basetype));
    }

    public static IEnumerable<Type> GetAllScriptedUserTypes()
    {
      return GetAllUserTypes(where_type: (t) => t.IsSubclassOf(TSpy<MonoBehaviour>.Type) ||
                                                t.IsSubclassOf(TSpy<ScriptableObject>.Type));
    }



    public static string GetSimpleName(this Assembly ass)
    {
      return ass.GetName().Name;
    }

    public static bool IsUserAssembly(this Assembly ass)
    {
      return s_UserAssemblies.Contains(ass);
    }


    public static int Compare(Assembly ass1, Assembly ass2)
    {
      return string.Compare(ass1.GetName().Name, ass2.GetName().Name);
    }



    private static Assembly AssemblyResolver(AssemblyName assname)
    {
      if (s_AssemblyMap.Find(assname.Name, out Assembly found))
      {
        return found;
      }

      return null;
    }


    private static Type TypeResolver(Assembly ass, string typename, bool ignore_case)
    {
      if (ass == null)
      {
        if (typename.StartsWith("System."))
        {
          return Type.GetType(typename, false, ignore_case);
        }

        int dot = typename.LastIndexOf('.');
        if (dot < 0 || !s_AssemblyMap.Find(typename.Remove(dot), out ass))
        {
          return Type.GetType(typename, false, ignore_case);
        }
      }

      return ass.GetType(typename, false, ignore_case);
    }


    #if UNITY_EDITOR
    [UnityEditor.MenuItem("PyroDK/Debug Info Loggers/Log Loaded Assemblies")]
    private static void LogLoaded()
    {
      var asses             = System.AppDomain.CurrentDomain.GetAssemblies();
      var reflective_asses  = System.AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();

      asses.MakeLogString("Loaded Assemblies", GetSimpleName)
           .Log();
      reflective_asses.MakeLogString("Loaded Reflection-Only Assemblies", GetSimpleName)
           .Log();
    }

    [UnityEditor.MenuItem("PyroDK/Debug Info Loggers/Log Assembly Caches")]
    private static void LogCache()
    {
      s_UserAssemblies.ToList()
                      .MakeLogString("User Assemblies", translator: GetSimpleName)
                      .Log();

      s_AssemblyMap.GetKeyList()
                   .MakeLogString("Cached Assemblies")
                   .Log();
    }
    #endif

  } // end static class Assemblies

}
