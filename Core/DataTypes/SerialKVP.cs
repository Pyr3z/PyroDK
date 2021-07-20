/**
@file   PyroDK/Core/DataTypes/SerialKVP.cs
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
  public abstract class SerialKVP<TKey, TValue> : IPair<TKey, TValue>
  {
    // Dummy safeguard:
    static SerialKVP()
    {
      // called for each instantiation of this base class
      Debug.Assert(  TSpy<TKey>.SerialCode > SerialTypeCode.Unsupported, $"Key type is not serializable! {TSpy<TKey>.LogName}");
      Debug.Assert(TSpy<TValue>.SerialCode > SerialTypeCode.Unsupported, $"Value type is not serializable! {TSpy<TValue>.LogName}");
    }


    #region IPair impl.
    public int Length => 2;
    public object this[int i] => (i == 1) ? (object)Value : Key;

    #endregion IPair impl.


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