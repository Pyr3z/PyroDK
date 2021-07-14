/**
@file   PyroDK/Core/AssetTypes/SandboxAsset.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-18

@brief
  For testing purposes.
**/

#pragma warning disable CS0649

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  
  #if UNITY_EDITOR
  [CreateAssetMenu(menuName = "PyroDK/Sandbox Asset", order = -120)]
  public class SandboxingAsset : BaseAsset
  {
    //public string TestString;
    //public List<string> TestStrings = new List<string>();

    //public SerialType TestSerialType;
    //public List<SerialType> TestSerialTypes = new List<SerialType>();

    public MinMaxFloat TestFloat = MinMaxFloat.Make(3.14f).WithMinMax(0f, 10f);
    public List<MinMaxFloat> TestFloats = new List<MinMaxFloat>();

    public SerialDelegate TestMethodCall;
    public List<SerialDelegate> TestMethodCalls = new List<SerialDelegate>();

    public PyroEvent TestPyroEvent;
    public List<PyroEvent> TestPyroEvents = new List<PyroEvent>();
  }

  #else

  public class SandboxingAsset : BaseAsset
  {
  }

  #endif // UNITY_EDITOR

}