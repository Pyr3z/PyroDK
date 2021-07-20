/**
@file   PyroDK/Core/DataTypes/SerialFieldMap.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-22

@brief
  Serializable HashMap lookup for static field values.
**/

using System.Reflection;

using UnityEngine;


namespace PyroDK
{
  
  [System.Serializable]
  public sealed class SerialFieldMap : SerialHashMap<SerialFieldMap.KVP, SerialFieldInfo, string>
  {
    [System.Serializable]
    public sealed class KVP : SerialKVP<SerialFieldInfo, string>
    {
    }



    public SerialFieldMap(in HashMapParams parms = default) : base(parms)
    {
      m_IsValidKey = IsValidField;
    }


    public bool StoreField(FieldInfo field, object instance, bool overwrite, out string value)
    {
      value = null;

      if (instance == null && !field.IsStatic)
      {
        $"\"{field.GetLogName()}\" is not a static field; provide an instance object.".LogError(this);
        return false;
      }

      if (!Serializer.SerializeField(field, instance, out SerialFieldInfo key, out value))
      {
        $"Failed to serialize a field to a string value (\"{field.GetLogName()}\").".LogError(this);
        return false;
      }

      if (overwrite)
      {
        return CheckedRemap(key, value, string.Equals);
      }

      return Map(key, value);
    }

    public bool LoadField(FieldInfo field, object instance)
    {
      if (instance == null && !field.IsStatic)
      {
        $"\"{field.GetLogName()}\" is not a static field; provide an instance object.".LogError(this);
        return false;
      }

      if (!Find(SerialFieldInfo.Temp(field), out string value))
      {
        $"Could not find field {field.GetLogName()}.".LogWarning(this);
        return false;
      }

      return Serializer.DeserializeField(value, field, instance);
    }

    public int LoadAllStaticFields()
    {
      if (Count == 0)
        return 0;

      int count = 0;

      foreach (var (field, value) in this)
      {
        if (field.IsMissing)
        {
          $"Unexpectedly skipping field \"{field}\".".LogWarning(this);
          continue;
        }

        if (field.Info.IsStatic && Serializer.DeserializeField(value, field, null))
        {
          ++count;
        }
      }

      return count;
    }


    protected sealed override bool Validate()
    {
      m_IsValidKey = IsValidField;
      return base.Validate();
    }



    //private static bool KeysEqual(SerialFieldInfo lhs, SerialFieldInfo rhs)
    //{
    //  return true;
    //}

    private static bool IsValidField(SerialFieldInfo sfi)
    {
      return ( sfi.IsPersistent || sfi.TryMakePersistent() ) &&
              !sfi.IsMissing;
    }

  }

}