/**
@file   PyroDK/Core/BaseTypes/IChoice.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-25

@brief
  An interface for identifying, weighting, and storing the
  states of a "Choice" concept.
**/


namespace PyroDK
{

  public interface IChoice<TSelf> :
    System.IEquatable<TSelf>,
    System.IComparable<TSelf>
  {
    int   Index         { get; }
    int   Hash          { get; }
    int   Weight        { get; }
    bool  IsEliminated  { get; }
  }

}