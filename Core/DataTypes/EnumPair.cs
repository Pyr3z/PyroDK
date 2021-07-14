/**
@file   PyroDK/Core/DataTypes/EnumPair.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-12

@brief
  --
**/

using UnityEngine;


namespace PyroDK
{
  using Type = System.Type;
  using Enum = System.Enum;


  [System.Serializable]
  public sealed class EnumPair : IPair<Enum, Enum>
  {
    //public static EnumPair Make<T0, T1>()
    //  where T0 : unmanaged, Enum
    //  where T1 : unmanaged, Enum
    //{
    //  return new EnumPair()
    //  {
    //    m_T0 = TSpy<T0>.SerialType,
    //    m_T1 = TSpy<T1>.SerialType
    //  };
    //}


    public int Length => 2;
    public object this[int i] => (i == 1) ? (object)V1 : V0;


    public bool IsMissing => !m_T0.Type.IsEnum || !m_T1.Type.IsEnum;

    public Type T0 => m_T0;
    public Type T1 => m_T1;


    [SerializeField]
    public long V0;
    [SerializeField]
    public long V1;


    [SerializeField]
    private SerialType m_T0;
    [SerializeField]
    private SerialType m_T1;


    internal const string VALUE_PROPERTY_START = "V0";


    private EnumPair()
    {
    }

    public EnumPair(Type t0, Type t1)
    {
      Debug.Assert(t0.IsEnum);
      Debug.Assert(t1.IsEnum);

      m_T0 = new SerialType(t0);
      m_T1 = new SerialType(t1);

      EnumSpy.TryGetDefaultValue(t0, ref V0);
      EnumSpy.TryGetDefaultValue(t1, ref V1);
    }

    public EnumPair(Enum e0, Enum e1)
    {
      V0   = e0.ToInt64();
      V1   = e1.ToInt64();
      m_T0 = new SerialType(e0.GetType());
      m_T1 = new SerialType(e1.GetType());
    }


    public void FixValues()
    {
      if (!EnumSpy.IsNamedValue(m_T0, V0))
      {
        V0 = 0;
      }
      if (!EnumSpy.IsNamedValue(m_T1, V1))
      {
        V1 = 0;
      }
    }


    public T0 Get0<T0>() where T0 : unmanaged, Enum
    {
      return EnumSpy<T0>.ConvertFrom(V0);
    }

    public void Set0<T0>(T0 value)
      where T0 : unmanaged, System.IConvertible
    {
      V0 = value.ToInt64(null);
    }


    public T1 Get1<T1>() where T1 : unmanaged, Enum
    {
      return EnumSpy<T1>.ConvertFrom(V1);
    }

    public void Set1<T1>(T1 value)
      where T1 : unmanaged, System.IConvertible
    {
      V1 = value.ToInt64(null);
    }


    public override string ToString()
    {
      return $"<{m_T0}, {m_T1}> : ({V0}) / ({V1})";
    }
  } // end struct EnumPair

}