/**
@file   PyroDK/Core/DataTypes/SerialFieldInfo.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-23

@brief
  Serializable struct storing a reference to a particular FieldInfo.
**/

using UnityEngine;


namespace PyroDK
{
  using FieldInfo = System.Reflection.FieldInfo;


  [System.Serializable]
  public struct SerialFieldInfo :
    System.IEquatable<SerialFieldInfo>,
    System.IEquatable<FieldInfo>
  {
    public static SerialFieldInfo Temp(FieldInfo finfo)
    {
      return new SerialFieldInfo()
      {
        m_RtInfo = finfo
      };
    }


    public FieldInfo Info => m_RtInfo ?? ReparseInfo();
    public string Name => m_Name;

    public bool IsPersistent => !m_Name.IsEmpty() && m_Declarer;
    public bool IsMissing => m_Name.IsEmpty() || Info == TypeMembers.MissingField;



    [SerializeField]
    private SerialType m_Declarer;
    [SerializeField]
    private string     m_Name;


    [System.NonSerialized]
    private FieldInfo m_RtInfo;


    public SerialFieldInfo(FieldInfo finfo)
    {
      if (finfo == null || finfo.DeclaringType == null)
      {
        m_Declarer = SerialType.Invalid;
        m_Name     = null;
        m_RtInfo   = TypeMembers.MissingField;
      }
      else
      {
        m_Declarer = new SerialType(finfo.DeclaringType);
        m_Name     = finfo.Name;
        m_RtInfo   = finfo;
      }
    }


    public bool TryMakePersistent()
    {
      if (m_RtInfo == null)
        return false;

      m_Declarer = new SerialType(m_RtInfo.DeclaringType);
      m_Name     = m_RtInfo.Name;
      return true;
    }

    private FieldInfo ReparseInfo()
    {
      if (m_Declarer.IsMissing || !m_Declarer.Type.TryGetSerializableField(m_Name, out m_RtInfo))
        return m_RtInfo = TypeMembers.MissingField;

      return m_RtInfo;
    }


    public override string ToString()
    {
      var strb = new System.Text.StringBuilder(m_Declarer.ToString());

      strb.Append('.');
      strb.Append(m_Name ?? "(noname)");

      if (IsMissing)
        strb.Append(" [Missing]");

      return strb.ToString();
    }

    public override int GetHashCode()
    {
      return Info.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return ( (obj is SerialFieldInfo sfi) && Info == sfi.Info ) ||
             ( (obj is FieldInfo field)     && Info == field    );
    }

    public bool Equals(SerialFieldInfo other)
    {
      return Info == other.Info;
    }

    public bool Equals(FieldInfo other)
    {
      return Info == other;
    }


    public static bool IsMissingField(SerialFieldInfo sfi)
    {
      return sfi.IsMissing;
    }


    public static implicit operator bool (SerialFieldInfo sfi)
    {
      return !sfi.IsMissing;
    }

    public static implicit operator FieldInfo (SerialFieldInfo sfi)
    {
      var finfo = sfi.Info;

      if (finfo == TypeMembers.MissingField)
        return null;

      return finfo;
    }


    // too dangerous to be implicit:
    //public static implicit operator SerialFieldInfo (FieldInfo finfo)
    //{
    //  return Temp(finfo);
    //}


    public static bool operator == (SerialFieldInfo lhs, FieldInfo rhs)
    {
      if (rhs == null)
        return lhs.IsMissing;

      return lhs.Info == rhs;
    }

    public static bool operator != (SerialFieldInfo lhs, FieldInfo rhs)
    {
      if (rhs == null)
        return !lhs.IsMissing;

      return lhs.Info != rhs;
    }

  }

}