/**
@file   PyroDK/Core/DataTypes/SerialValue.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-10

@brief
  A generic value that stores its own type info and
  srting/JSON value representation.
**/

using UnityEngine;


namespace PyroDK
{
  using Type   = System.Type;
  using ITuple = System.Runtime.CompilerServices.ITuple;


  [System.Serializable]
  public sealed class SerialValue :
    IPair<SerialType, string>,
    ISerializationCallbackReceiver
  {
    public static bool IsValidType(Type type)
    {
      return type != null && ( type == typeof(string) ||
                               type.IsPrimitive       ||
                               TSpy<Object>.IsAssignableFrom(type) );
    }


    public Type Type => m_Type;

    public bool IsValid => !m_Type.IsMissing;

    public object Value => m_RtValue;


    int    ITuple.Length => 2;
    object ITuple.this[int idx] => idx == 0 ? (object)m_Type.Type : m_Value;


    [SerializeField]
    private SerialType  m_Type;
    [SerializeField]
    private string      m_Value;


    [System.NonSerialized]
    private object m_RtValue;


    public SerialValue()
    {
      m_Type    = SerialType.Invalid;
      m_Value   = string.Empty;
      m_RtValue = Type.Missing;
    }

    public SerialValue(Type type, object value)
    {
      if (Logging.Assert(IsValidType(type), "Invalid Type for SerialValue!"))
      {
        m_Type    = SerialType.Invalid;
        m_Value   = string.Empty;
        m_RtValue = Type.Missing;
      }
      else
      {
        m_Type    = new SerialType(type);
        m_Value   = Serializer.MakeString(value, type);
        m_RtValue = value;
      }
    }


    public void Set<T>(T value)
    {
      if (value == null)
      {
        m_Type    = SerialType.Invalid;
        m_Value   = string.Empty;
        m_RtValue = value;
        return;
      }

      var type = value.GetType();

      if (IsValidType(type))
      {
        m_Type.Type = type;
        m_Value     = Serializer.MakeString(value, type);
        m_RtValue   = value;
      }
      else
      {
        $"Type \"{type.GetQualifiedName()}\" is not valid for SerialValue."
          .LogError();
      }
    }

    public T Get<T>()
    {
      if (m_RtValue == null && !Serializer.TryParseString(m_Value, m_Type, out m_RtValue))
      {
        $"Could not parse {m_Type} value string into {TSpy<T>.LogName}."
          .LogError();
        return default;
      }

      if (TSpy<T>.IsAssignableFrom(m_RtValue))
      {
        return (T)m_RtValue;
      }

      $"\"{m_Value}\" value string could not be cast to {TSpy<T>.LogName}."
        .LogError();

      return default;
    }



    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
      if (!IsValid)
        return;

      m_Value = Serializer.MakeString(m_RtValue, m_Type);
    }
    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      if (!IsValid)
        return;

      m_RtValue = Serializer.ParseString(m_Value, m_Type);
    }

  } // end class SerialValue

}