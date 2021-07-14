/**
@file   PyroDK/Core/DataTypes/SerialMethodInfo.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2021-05-12

@brief
  Serializable runtime method info.
**/

using UnityEngine;


namespace PyroDK
{
  using Type = System.Type;
  using MethodInfo = System.Reflection.MethodInfo;


  [System.Serializable]
  public sealed class SerialMethodInfo :
    System.IEquatable<SerialMethodInfo>,
    System.IEquatable<MethodInfo>,
    ISerializationCallbackReceiver
  {
    public MethodInfo Info => m_RtMethodInfo ?? DeferredDeserialize();
    public Type[] ParameterTypes
    {
      get
      {
        if (Logging.AssertNonNull(m_RtParamTypes))
          DeferredDeserialize();

        return m_RtParamTypes;
      }
    }

    public bool IsMissing => m_Declarer.IsEmpty() || Info == TypeMembers.MissingMethod;


    [SerializeField] [TypeName]
    private string m_Declarer;
    [SerializeField]
    private string m_Name;
    [SerializeField] [TypeName]
    private string[] m_ParamTypes;


    [System.NonSerialized]
    private MethodInfo m_RtMethodInfo = null;
    [System.NonSerialized]
    private Type[]     m_RtParamTypes = null;


    public SerialMethodInfo(MethodInfo method)
    {
      if (method == null || Logging.Assert(method.DeclaringType != null))
      {
        m_RtMethodInfo = TypeMembers.MissingMethod;
      }
      else
      {
        m_Declarer = method.DeclaringType.GetQualifiedName();
        m_Name     = method.Name;
        
        // Avoid deferred deserialization if this instance is
        // used soon after being created with code by immediately
        // assigning the runtime reflection infos:

        m_RtMethodInfo = method;

        var parms = method.GetParameters();
        m_ParamTypes   = new string[parms.Length];
        m_RtParamTypes = new Type[parms.Length];

        for (int i = 0; i < parms.Length; ++i)
        {
          m_ParamTypes[i] = ( m_RtParamTypes[i] = parms[i].ParameterType ).GetQualifiedName();
        }
      }
    }

    private SerialMethodInfo()
    {
      m_RtMethodInfo = TypeMembers.MissingMethod;
    }


    private MethodInfo DeferredDeserialize()
    {
      if (Assemblies.FindType(m_Declarer, out Type declarer) &&
          TryParseParameterTypes(out m_RtParamTypes)         &&
          declarer.TryGetMethod(m_Name, m_RtParamTypes, out m_RtMethodInfo, blags: TypeMembers.ALL))
      {
        return m_RtMethodInfo;
      }

      m_RtParamTypes = new Type[0];
      return m_RtMethodInfo = TypeMembers.MissingMethod;
    }
    private bool TryParseParameterTypes(out Type[] types)
    {
      bool all_valid = true;

      int pcount = m_ParamTypes?.Length ?? 0;
      types = new Type[pcount];

      for (int i = 0; i < pcount; ++i)
      {
        if (!Assemblies.FindType(m_ParamTypes[i], out types[i]))
        {
          $"Unable to find type \"{m_ParamTypes[i]}\""
            .LogWarning();

          all_valid = false;
        }
      }

      return all_valid;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      m_RtMethodInfo = null;
      m_RtParamTypes = null;
    }
    void ISerializationCallbackReceiver.OnBeforeSerialize() { }


    public override int GetHashCode()
    {
      return Info.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return ( obj is SerialMethodInfo smi && Info == smi.Info ) ||
             ( obj is MethodInfo method && m_RtMethodInfo == method );
    }
    public bool Equals(SerialMethodInfo other)
    {
      return Info == other.Info;
    }
    public bool Equals(MethodInfo other)
    {
      return Info == other;
    }

    public override string ToString()
    {
      if (m_Name.IsEmpty())
        return "(missing method)";

      return $"{m_Declarer}.{m_Name}"; // TODO tack on other qualifiers
    }


    public static implicit operator bool (SerialMethodInfo smi)
    {
      return smi != null && !smi.IsMissing;
    }

    public static implicit operator MethodInfo (SerialMethodInfo smi)
    {
      if (smi == null)
        return null;

      var method = smi.Info;

      if (method == TypeMembers.MissingMethod)
        return null;

      return method;
    }

  } // end class SerialMethodInfo



  //public interface IDelegate
  //{
  //  MethodInfo Method         { get; }
  //  object     Target         { get; }
  //  Type[]     ParameterTypes { get; }

  //  bool IsValid { get; }

  //  bool ValidateInvoke(object[] parameters);
  //  object Invoke(object[] parameters);
  //}


  [System.Serializable]
  public sealed class SerialDelegate
  {
    internal const int MAX_PARAMETER_COUNT = 3;
    public static bool IsValidMethod(MethodInfo method)
    {
      if (method.IsSpecialName ||
          method.IsGenericMethodDefinition ||
          method.IsDefined<System.ObsoleteAttribute>())
      {
        return false;
      }

      var parameters = method.GetParameters();

      if (parameters.Length > MAX_PARAMETER_COUNT)
        return false;

      foreach (var parameter in parameters)
      {
        if (Serializer.RuntimeTypeToCode(parameter.ParameterType) <= SerialTypeCode.Unsupported)
          return false;
      }

      return true;
    }



    public SerialMethodInfo Method => m_Method;

    public object Target => m_Target;

    public bool IsValid => m_Method && ( m_Target || m_Method.Info.IsStatic );


    [SerializeField]
    private SerialMethodInfo m_Method;
    [SerializeField]
    private Object m_Target;
    [SerializeField]
    private bool m_Static;


    private SerialDelegate()
    {
    }


    public bool ValidateInvoke(object[] parameters)
    {
      if (!m_Method)
        return false;

      var pts  = m_Method.ParameterTypes;
      int plen = parameters?.Length ?? 0;

      if (plen != pts.Length)
        return false;

      for (int i = 0; i < plen; ++i)
      {
        if (parameters[i] == null)
        {
          if (!pts[i].IsClass && pts[i] != typeof(string))
            return false;
        }
        else if (!pts[i].IsAssignableFrom(parameters[i].GetType()))
        {
          return false;
        }
      }

      return true;
    }

    public object Invoke(object[] parameters)
    {
      // Should be pre-validated in bulk
      return m_Method.Info.Invoke(m_Target, parameters);
    }

  } // end class SerialDelegate

}