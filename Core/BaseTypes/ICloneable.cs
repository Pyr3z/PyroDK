/**
@file   PyroDK/Core/BaseTypes/ICloneable.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-25

@brief
  An interface for objects that can produce clones of themselves.
**/


namespace PyroDK
{

  public interface ICloneable<T> : System.ICloneable
  {
    new T Clone();
    void CloneTo(ref T other);
  }

}