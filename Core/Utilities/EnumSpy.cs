/**
@file   PyroDK/Core/Utilities/EnumSpy.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-09

@brief
  Provides a "spy" helper that caches commonly queried properties
  of specific Enum fields.
**/


namespace PyroDK
{
  using Debug = UnityEngine.Debug;

  using Type         = System.Type;
  using Enum         = System.Enum;
  using IConvertible = System.IConvertible;

  public static class EnumSpy
  {
    private static readonly HashMap<(Type t, long v), EnumInfo>
      s_InfoMap = new HashMap<(Type t, long v), EnumInfo>();


    public static EnumInfo FindInfo(Type enum_type, IConvertible value)
    {
      long value64 = value.ToInt64(null);

      (Type t, long v) key = (enum_type, value64);

      if (s_InfoMap.Find(key, out EnumInfo result))
        return result;

      result = null;

      foreach (var field in enum_type.GetFields(TypeMembers.ENUMS))
      {
        var data = new EnumInfo(field);

        key.v = data.Value64;

        var try_map = s_InfoMap.TryMap(key, data, out EnumInfo prev_data);

        Debug.Assert(try_map != TriBool.Null);

        if (try_map == TriBool.False)
        {
          if (!prev_data || data == prev_data)
          {
            // we've already been here?
            Logging.ShouldNotReach(data);
            break;
          }

          data = prev_data;
        }

        if (!result && value64 == data.Value64)
        {
          result = data;
        }
      }

      if (!result)
      {
        result = new EnumInfo(enum_type, value64);

        // possibly a combined enum flag value?
        // Either way, dis fucky, so

        key.v = value64;

        Logging.TempReached(key);

        if (Logging.Assert(s_InfoMap.Map(key, result)))
          return null;
      }

      return result;
    }


    public static EnumInfo GetInfo(Enum value)
    {
      return FindInfo(value.GetType(), value);
    }


    public static bool TryGetDefaultValue<TValue>(Type enum_type, ref TValue value)
      where TValue : unmanaged, IConvertible
    {
      var values = Enum.GetValues(enum_type);

      if (values.Length > 0)
      {
        value = (TValue)((IConvertible)values.GetValue(0)).ToType(typeof(TValue), null);
        return true;
      }

      return false;
    }


    public static Enum ConvertFrom(Type enum_type, long value)
    {
      return (Enum)Enum.ToObject(enum_type, value);
    }

    public static Enum ConvertFrom(Type enum_type, int value)
    {
      return (Enum)Enum.ToObject(enum_type, value);
    }


    public static long ToInt64(this Enum value)
    {
      return ((IConvertible)value).ToInt64(null);
    }


    public static bool IsNamedValue(Enum value)
    {
      return GetInfo(value); // implicit operator bool
    }

    public static bool IsNamedValue(Type enum_type, long value)
    {
      return FindInfo(enum_type, value); // implicit operator bool
    }


    public static bool HasAttribute<TAttr>(Enum value)
      where TAttr : System.Attribute
    {
      var data = GetInfo(value);
      return data && TSpy<TAttr>.IsAttributeOn(data.Field);
    }

  } // end class EnumSpy


  public static class EnumSpy<TEnum>
    where TEnum : unmanaged, Enum, IConvertible
  {
    public static TEnum ConvertFrom(long value)
    {
      return (TEnum)Enum.ToObject(TSpy<TEnum>.Type, value);
    }
    public static TEnum ConvertFrom(ulong value)
    {
      return (TEnum)Enum.ToObject(TSpy<TEnum>.Type, value);
    }
    public static TEnum ConvertFrom(int value)
    {
      return (TEnum)Enum.ToObject(TSpy<TEnum>.Type, value);
    }
    public static TEnum ConvertFrom(uint value)
    {
      return (TEnum)Enum.ToObject(TSpy<TEnum>.Type, value);
    }


    public static EnumInfo FindData(TEnum value)
    {
      return EnumSpy.FindInfo(TSpy<TEnum>.Type, value);
    }


    public static bool HasAttribute<TAttr>(TEnum value)
    {
      var data = EnumSpy.FindInfo(TSpy<TEnum>.Type, value);
      return data && TSpy<TAttr>.IsAttributeOn(data.Field);
    }
  } // end class EnumSpy<T>

}