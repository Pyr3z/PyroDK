/**
@file   PyroDK/Core/Utilities/EnumInfo.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-09

@brief
  A data type that caches details about specific enum fields using reflection.
**/


namespace PyroDK
{
  using Type         = System.Type;
  using Enum         = System.Enum;
  using FieldInfo    = System.Reflection.FieldInfo;
  using BindingFlags = System.Reflection.BindingFlags;



  public sealed class EnumInfo
  {
    public readonly string    Name;
    public readonly long      Value64;
    public readonly FieldInfo Field;


    public Enum Value => (Enum)Enum.ToObject(Field.DeclaringType, Value64);

    public bool IsAnonymous => Field == null && !Name.IsEmpty();


    internal EnumInfo(Type enum_type, long value64)
    {
      Name    = Enum.GetName(enum_type, value64);
      Value64 = value64;

      if (Name.IsEmpty())
        Name = $"({enum_type.Name}){value64}";
      else
        Field = enum_type.GetField(Name, BindingFlags.Static | BindingFlags.Public);
    }
    internal EnumInfo(FieldInfo field)
    {
      Name    = field.Name;
      Value64 = field.GetValue<long>();
      Field   = field;
    }


    public T CastValue<T>()
      where T : struct, Enum
    {
      return (T)Enum.ToObject(TSpy<T>.Type, Value64);
    }


    public override string ToString()
    {
      return Field?.ToString() ?? "(null)";
    }

    public override int GetHashCode()
    {
      return Field?.GetHashCode() ?? int.MinValue;
    }

    public override bool Equals(object obj)
    {
      return (obj is EnumInfo efd) && efd.Field == Field;
    }


    public static implicit operator bool(EnumInfo info)
    {
      return info != null && info.Field != null;
    }

  }

}