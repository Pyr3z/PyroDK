/**
@file   PyroDK/Core/BaseTypes/ISingleton.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-07

@brief
  Defines the ISingleton interface.
**/


namespace PyroDK
{

  public interface ISingleton<TSelf> // : IObject
    where TSelf : ISingleton<TSelf>
  {
    TSelf Instance { get; }
    bool IsCurrentInstance { get; }
    bool IsReplaceableInstance { get; }
  }

}