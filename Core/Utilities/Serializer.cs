/**
@file   PyroDK/Core/Utilities/Serializer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-24

@brief
  Defines the `Serializer<T>` static type that helps unify
  the interface for serializing data types.
**/

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  using Type          = System.Type;
  using FieldInfo     = System.Reflection.FieldInfo;

  #if UNITY_EDITOR
  using JsonUtility = UnityEditor.EditorJsonUtility;
  #else
  using JsonUtility = UnityEngine.JsonUtility;
  #endif



  public enum SerialTypeCode
  {
    Null = 0,

    [HideInInspector] // only hidden with PyroDK drawers.
    Unsupported,
    [HideInInspector]
    Generic,

    // value types:
    String,
    Integer,
    Float,
    Bool,

    // reference types:
    RefAssetObject,
    RefSceneObject,
  }


  
  internal interface IJsonWrapper
  {
    object Value { get; set; }
    string ToJson(object value);
    object FromJson(string json);
  }

  public static class WrappedSerializer<T>
  {
    [System.Serializable]
    public sealed class JsonWrapper : IJsonWrapper
    {
      [SerializeField]
      public T v;

      public object Value
      {
        get => v;
        set => v = (T)value;
      }

      public string ToJson(object value)
      {
        if (value == null)
          return ToJsonCastless(default(T));

        if (value is T tv)
          return ToJsonCastless(tv);

        $"\"value\" is not type {TSpy<T>.LogName}."
          .LogError();

        return string.Empty;
      }

      public string ToJsonCastless(T value)
      {
        v = value;

        try
        {
          return JsonUtility.ToJson(this, prettyPrint: false);
        }
        catch (UnityException e)
        {
          e.LogException();
        }

        return Serializer.NULL_REF_JSON_STRING;
      }

      public object FromJson(string json)
      {
        return FromJsonCastless(json);
      }

      public T FromJsonCastless(string json)
      {
        // do exception handling further up the stack, mk?
        JsonUtility.FromJsonOverwrite(json, this);
        return v;
      }
    }


    public static JsonWrapper Wrapper = new JsonWrapper();


    public static string ToJson(T value)
    {
      return Wrapper.ToJsonCastless(value);
    }

    public static T FromJson(string json)
    {
      return Wrapper.FromJsonCastless(json);
    }

    public static T DeepCopy(T value)
    {
      if (TSpy<T>.Type.IsValueType)
        return value;
      return FromJson(ToJson(value));
    }

  }


  #if UNITY_EDITOR
  [UnityEditor.InitializeOnLoad]
  #endif
  public static class Serializer
  {
    public const string NULL_REF_JSON_STRING = "{\"v\":{\"instanceID\":0}}";


    private readonly static Type[] s_RtTypeCodeLookup = new Type[]
    {
      typeof(object), // Null

      typeof(object), // Unsupported
      typeof(object), // Generic

      typeof(string), // String
      typeof(int),    // Integer
      typeof(float),  // Float
      typeof(bool),   // Bool

      typeof(Object), // RefAssetObject
      typeof(Object), // RefSceneObject
    };

    private readonly static string[] s_DefaultValueStringLookup = new string[]
    {
      null,               // Null

      null,               // Unsupported
      null,               // Generic

      string.Empty,       // String
      (0).ToString(),     // Integer
      (0f).ToString(),    // Float
      (false).ToString(), // Bool

      null,               // RefAssetObject
      null,               // RefSceneObject
    };

    private readonly static object[] s_DefaultValueObjectLookup = new object[]
    {
      null,         // Null

      null,         // Unsupported
      null,         // Generic

      string.Empty, // String
      0,            // Integer
      0f,           // Float
      false,        // Bool

      null,         // RefAssetObject
      null,         // RefSceneObject
    };

    private static readonly Dictionary<Type, IJsonWrapper> s_CachedWrappers = new Dictionary<Type, IJsonWrapper>();

    private const char    SPLIT_ENUM  = ':';
    private const string  FMT_ENUM    = "{0}:{1}";



    public static bool IsValidJson(string json)
    {
      return json != null && json.Length > 1 &&
             json.EndsWith("}") && json.StartsWith("{");
    }

    public static bool IsSerializable(Type type)
    {
      return false;
    }



    public static bool FallbackDefault(ref object value, Type type)
    {
      if (value == null)
      {
        value = type.ConstructDefault();
      }

      return value != null || TSpy<Object>.IsAssignableFrom(type);
    }


    public static string MakeNullRefString()
    {
      return WrappedSerializer<Object>.ToJson(null);
    }


    public static string MakeString(object value, SerialTypeCode code, ref SerialType ref_type)
    {
      if (value == null)
        return DefaultValueStringFor(code, ref ref_type);

      if (code > SerialTypeCode.Unsupported)
      {
        // value types
        if (code < SerialTypeCode.RefAssetObject)
        {
          return value.ToString();
        }
        // reference types
        else
        {
          Logging.Assert(TSpy<Object>.IsAssignableFrom(value));

          if (ref_type.IsMissingOr<Object>())
          {
            ref_type.Type = value.GetType();
          }

          return WrappedSerializer<Object>.ToJson(value as Object);
        }
      }

      return null;
    }


    public static string MakeString(object value, Type type)
    {
      if (!FallbackDefault(ref value, type))
      {
        return null;
      }

      if (type == typeof(string) || type.IsPrimitive)
      {
        return value.ToString();
      }

      if (type.IsEnum)
      {
        return string.Format(FMT_ENUM, System.Convert.ToUInt64(value), value.ToString());
      }

      if (value is Color32 c && TSpy<Color32>.IsCastableTo(type))
      {
        return '#' + c.ToHex();
      }

      return ToJsonWrapped(value, type);
    }


    public static object ParseString(string value_str, Type type)
    {
      _ = TryParseString(value_str, type, out object parsed);
      return parsed;
    }


    public static bool TryParseString(string          value_str,
                                      SerialTypeCode  code,
                                      out object      parsed)
    {
      if (value_str.IsEmpty() || code == SerialTypeCode.Null)
      {
        parsed = DefaultValueObjectFor(code);
        return true;
      }

      parsed = null;

      if (code >= SerialTypeCode.RefAssetObject)
      {
        if (Logging.Assert(IsValidJson(value_str), "IsValidJson(value_str)"))
          return false;

        if (FromJsonWrapped(value_str, out Object unity_obj))
        {
          parsed = unity_obj;
          return true;
        }
      }
      else if (TryParseString(value_str, CodeToRuntimeType(code), out object parsed_box))
      {
        parsed = parsed_box;
        return true;
      }

      return false;
    }


    public static bool TryParseString(string      value_str,
                                      Type        type,
                                      out object  parsed)
    {
      parsed = null;

      if (type == typeof(string))
      {
        parsed = value_str;
        return true;
      }
      else if (type == typeof(object))
      {
        return false;
      }
      else if (value_str.IsEmpty())
      {
        return FallbackDefault(ref parsed, type);
      }
      else if (type.IsPrimitive)
      {
        parsed = System.Convert.ChangeType(value_str, type);
        return parsed != null;
      }
      else if (type.IsEnum)
      {
        // prefer name over hash
        int split   = value_str.IndexOf(SPLIT_ENUM);
        string name = value_str.Substring(split + 1); // safe bc -1 + 1 = 0

        try
        {
          // should ignore case?
          parsed = System.Enum.Parse(type, name, ignoreCase: true);
        }
        catch (System.Exception e)
        {
          e.LogException();
          parsed = null;
        }

        if (parsed == null)
        {
          // fallbacks
          if (split < 1)
          {
            Logging.Reached();
            return FallbackDefault(ref parsed, type);
          }
          else if (ulong.TryParse(value_str.Remove(split), out ulong hash))
          {
            parsed = System.Enum.ToObject(type, hash);
          }
        }

        return parsed != null;
      }
      else if ( TSpy<Color32>.IsCastableTo(type) &&
                FallbackDefault(ref parsed, type)   &&
                parsed is Color32 c                 &&
                Colors.TryReparseHex(value_str, ref c))
      {
        parsed = c;
        return true;
      }
      else if (IsValidJson(value_str))
      {
        return FromJsonWrapped(value_str, type, out parsed);
      }

      return false;
    }



    public static string ToJsonWrapped(object value, Type type)
    {
      if (TSpy<Object>.IsAssignableFrom(type))
      {
        return WrappedSerializer<Object>.ToJson(value as Object);
      }

      return GetWrapper(type).ToJson(value);
    }

    public static bool FromJsonWrapped(string json, Type type, out object obj)
    {
      if (TSpy<Object>.IsAssignableFrom(type))
      {
        try
        {
          obj = WrappedSerializer<Object>.FromJson(json);
          return true;
        }
        catch (UnityException e)
        {
          e.LogDumbException();

          obj = null;
          return false;
        }
      }

      obj = GetWrapper(type).FromJson(json);
      return obj != null;
    }

    public static bool FromJsonWrapped(string json, out Object unity_object)
    {
      try
      {
        unity_object = WrappedSerializer<Object>.FromJson(json);
        return true;
      }
      catch (UnityException)
      {
        //e.LogDumbException();

        unity_object = null;
        return false;
      }
    }


    public static object DeepCopy(object value)
    {
      if (value == null)
        throw new System.ArgumentNullException("value");

      var type = value.GetType();

      if (type.IsValueType)
        return value;
      else if (TSpy<string>.Type == type)
        return string.Copy((string)value);

      return ParseString(MakeString(value, type), type);
    }


    public static bool SerializeField(FieldInfo field, object instance,
                                      out SerialFieldInfo key,
                                      out string          val)
    {
      key = new SerialFieldInfo(field);
      val = MakeString(field.GetValue(instance), field.FieldType);
      return key && val != null;
    }

    public static bool DeserializeField(string value, FieldInfo field, object instance)
    {
      if (field == null || value.IsEmpty())
        return false;

      if (TryParseString(value, field.FieldType, out object parsed))
      {
        object prev = field.GetValue(instance);
        if (prev != parsed)
        {
          field.SetValue(instance, parsed);
          return true;
        }
      }

      return false;
    }



    public static SerialTypeCode RuntimeTypeToCode(Type type)
    {
      if (type == null)
        return SerialTypeCode.Null;

      if (type == typeof(string))
        return SerialTypeCode.String;

      if (type.IsPrimitive)
      {
        if (type == typeof(float))
          return SerialTypeCode.Float;

        if (type == typeof(int))
          return SerialTypeCode.Integer;

        if (type == typeof(bool))
          return SerialTypeCode.Bool;
      }
      else if (TSpy<Object>.IsAssignableFrom(type))
      {
        if (type == typeof(GameObject) || TSpy<Component>.IsAssignableFrom(type))
          return SerialTypeCode.RefSceneObject;

        return SerialTypeCode.RefAssetObject;
      }
      else if (type.IsDefined<System.SerializableAttribute>())
      {
        return SerialTypeCode.Generic;
      }

      return SerialTypeCode.Unsupported;
    }

    public static Type CodeToRuntimeType(SerialTypeCode code)
    {
      return s_RtTypeCodeLookup[(int)code];
    }


    public static string DefaultValueStringFor(SerialTypeCode code, ref SerialType ref_type)
    {
      if (code >= SerialTypeCode.RefAssetObject && ref_type.IsMissingOr<Object>())
      {
        ref_type = TSpy<Object>.SerialType;
      }

      return s_DefaultValueStringLookup[(int)code];
    }

    public static string DefaultValueStringFor(SerialTypeCode code)
    {
      return s_DefaultValueStringLookup[(int)code];
    }


    public static object DefaultValueObjectFor(SerialTypeCode code)
    {
      return s_DefaultValueObjectLookup[(int)code];
    }



    private static IJsonWrapper GetWrapper(Type fortype)
    {
      if (s_CachedWrappers.TryGetValue(fortype, out IJsonWrapper wrapper))
        return wrapper;
      
      if (typeof(WrappedSerializer<>).MakeGenericType(fortype).TryGetStaticValue("Wrapper", out wrapper))
        return s_CachedWrappers[fortype] = wrapper;

      throw new System.Exception($"Reflection is whacky. type={fortype.GetLogName()}");
    }



#if UNITY_EDITOR
#pragma warning disable IDE0051

    [UnityEditor.MenuItem("PyroDK/Debug Info Loggers/Log Null Ref JSON", priority = -50)]
    private static void MenuLogNullRefJSON()
    {
      string json = MakeNullRefString();
      json.Log();

      if (!FromJsonWrapped(json, out _ ))
      {
        Logging.ShouldNotReach(blame: json);
      }
    }

#pragma warning restore IDE0051
#endif

  }

}