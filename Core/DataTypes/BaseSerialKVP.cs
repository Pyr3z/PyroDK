/**
@file   PyroDK/Core/DataTypes/BaseSerialKVP.cs
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


  [System.Serializable]
  public abstract class BaseSerialKVP<TKey, TValue> : IPair<TKey, TValue>
  {
    // TODO separate file?

    public int Length => 2;
    public object this[int i] => (i == 1) ? (object)Value : Key;


    public Type KeyType => TSpy<TKey>.SerialType;
    public Type ValueType
    {
      get
      {
        if (TSpy<TValue>.IsUnityObject)
        {
          if (Value == null)
            return TSpy<Object>.SerialType;
          return new SerialType(Value.GetType());
        }

        return TSpy<TValue>.SerialType;
      }
    }


    [SerializeField]
    public TKey Key;
    [SerializeField]
    public TValue Value;


    public override string ToString()
    {
      return $"{Key} => {Value}";
    }
  }

}