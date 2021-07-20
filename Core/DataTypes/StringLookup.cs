/**
@file   PyroDK/Core/DataTypes/StringLookup.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-18

@brief
  A [Serializable] Lookup data structure that stores
  string-string pairs for later lookup.
**/


namespace PyroDK
{
  
  [System.Serializable]
  public sealed class StringLookup : Lookup<StringLookup.KVP, string>
  {
    [System.Serializable]
    public sealed class KVP : SerialKVP<string, string>
    {
    }


    public StringLookup(HashMapParams parms) : base(parms)
    {
      m_HashMap.SetValueEquals(string.Equals);
    }

    public StringLookup() : base()
    {
      m_HashMap.SetValueEquals(string.Equals);
    }


    protected sealed override bool Validate()
    {
      m_HashMap.SetValueEquals(string.Equals);
      return base.Validate();
    }

  }

}