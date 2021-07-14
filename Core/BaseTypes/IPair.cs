/**
@file   PyroDK/Core/BaseTypes/IPair.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-12

@brief
  --
**/

using System.Collections;
using System.Collections.Generic;


namespace PyroDK
{
  public interface IPair<T0, T1> :
    System.Runtime.CompilerServices.ITuple
  {
    // Inherited:
    //
    // int    Length          { get; }
    // object this[int index] { get; }
  }


  public static class Pairs
  {

    public static T0 First<T0, T1>(this IPair<T0, T1> pair)
    {
      return (T0)pair[0];
    }

    public static T1 Second<T0, T1>(this IPair<T0, T1> pair)
    {
      return (T1)pair[1];
    }


    public static (T0, T1) ToValueTuple<T0, T1>(this IPair<T0, T1> pair)
    {
      return (First(pair), Second(pair));
    }


    public static PairKeyList<TPair, T0, T1> GetKeys<TPair, T0, T1>(this IReadOnlyList<TPair> pairs)
      where TPair : IPair<T0, T1>
    {
      return new PairKeyList<TPair, T0, T1>(pairs);
    }

    public static PairValueList<TPair, T0, T1> GetValues<TPair, T0, T1>(this IReadOnlyList<TPair> pairs)
      where TPair : IPair<T0, T1>
    {
      return new PairValueList<TPair, T0, T1>(pairs);
    }

  } // end static class Pairs


  public class PairKeyList<TPair, T0, T1> : IReadOnlyList<T0>
    where TPair : IPair<T0, T1>
  {
    public int  Count       => m_Pairs.Count;
    public T0   this[int i] => (T0)m_Pairs[i][0];


    private IReadOnlyList<TPair> m_Pairs;


    public PairKeyList(IReadOnlyList<TPair> pairs)
    {
      m_Pairs = pairs;
    }


    public IEnumerator<T0> GetEnumerator()
    {
      foreach (var pair in m_Pairs)
        yield return (T0)pair[0];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      foreach (var pair in m_Pairs)
        yield return pair[0];
    }
  }


  public class PairValueList<TPair, T0, T1> : IReadOnlyList<T1>
    where TPair : IPair<T0, T1>
  {
    public int  Count       => m_Pairs.Count;
    public T1   this[int i] => (T1)m_Pairs[i][1];


    private IReadOnlyList<TPair> m_Pairs;


    public PairValueList(IReadOnlyList<TPair> pairs)
    {
      m_Pairs = pairs;
    }


    public IEnumerator<T1> GetEnumerator()
    {
      foreach (var pair in m_Pairs)
        yield return (T1)pair[1];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      foreach (var pair in m_Pairs)
        yield return pair[1];
    }
  }

}