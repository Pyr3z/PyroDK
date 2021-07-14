/**
@file   PyroDK/Core/DataTypes/SerialChoiceList.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-25

@brief
  A container for Choices that can be sorted by some
  given weights, selected from at random, or selected
  from using given weights or conditional weights.
**/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{

  [System.Serializable]
  public class SerialChoiceList :
    IEnumerable<SerialChoice>,
    ISerializationCallbackReceiver
  {

    public int Count        => m_Choices.Count;
    public int TotalWeight  => m_TotalWeight;

    public int this[object choice] // gets/sets choice weights
    {
      get
      {
        _ = TryGetWeight(choice, out int weight);
        return weight;
      }
      set => SetWeightOrAdd(choice, value);
    }


    [SerializeField] //[ReadOnly]
    private int                 m_TotalWeight = 0;
    [SerializeField] //[ReadOnly]
    private List<SerialChoice>  m_Choices     = new List<SerialChoice>();

    [System.NonSerialized]
    private HashMap<int, int>   m_DupeMap     = new HashMap<int, int>();



    public bool TryChoose(out SerialChoice winner)
    {
      winner = SerialChoice.None;

      if (m_TotalWeight == 0) // also handles empty choice list
        return false;

      int raffle_choice = Random.Range(0, m_TotalWeight);
      int raffle_idx    = 0;

      int i = m_Choices.Count;
      while (i --> 0)
      {
        raffle_idx += m_Choices[i].Weight;

        if (raffle_choice < raffle_idx)
        {
          winner = m_Choices[i];
          return winner;
        }
      }

      return false;
    }

    public SerialChoice Choose()
    {
      _ = TryChoose(out SerialChoice choice);
      return choice;
    }


    public SerialChoice AddChoice(object choice_val, int weight = 1)
    {
      int hash = choice_val.GetHashCode();

      var trymap = m_DupeMap.TryMap(hash, m_Choices.Count, out int i);

      if (trymap == TriBool.True)
      {
        if (weight < 0)
          weight = 0;

        var choice = new SerialChoice(hash, weight, i);
        m_Choices.Add(choice);
        m_TotalWeight += weight;
        return choice;
      }
      else if (trymap == TriBool.False && i < m_Choices.Count)
      {
        var choice = m_Choices[i];
        m_TotalWeight -= choice.Weight;
        choice.AddWeight(weight);
        m_TotalWeight += choice.Weight;
        return choice;
      }

      $"SerialChoiceList: choice \"{choice_val}\" was not added; HashMap internal error.".LogError();
      return SerialChoice.None;
    }

    public void Add(object choice_val, int weight) // required for syntactic sugar
    {
      _ = AddChoice(choice_val, weight);
    }


    public void EliminateChoice(object choice_val)
    {
      if (TryFindIndex(choice_val.GetHashCode(), out int i))
      {
        m_TotalWeight -= m_Choices[i].Eliminate();
      }
    }

    public void RestoreChoice(object choice_val)
    {
      if (TryFindIndex(choice_val.GetHashCode(), out int i))
      {
        m_TotalWeight += m_Choices[i].Restore();
      }
    }

    public void RestoreAll()
    {
      int i = m_Choices.Count;
      while (i --> 0)
      {
        m_TotalWeight += m_Choices[i].Restore();
      }
    }

    
    public bool TryGetWeight(object choice, out int weight)
    {
      if (TryFindIndex(choice.GetHashCode(), out int i))
      {
        weight = m_Choices[i].Weight;
        return true;
      }

      weight = int.MinValue;
      return false;
    }

    public bool TrySetWeight(object choice_val, int weight)
    {
      if (TryFindIndex(choice_val.GetHashCode(), out int i))
      {
        var choice = m_Choices[i];

        m_TotalWeight -= choice.Weight;
        choice.SetWeight(weight);
        m_TotalWeight += choice.Weight;

        return true;
      }

      return false;
    }

    public void SetWeightOrAdd(object choice_val, int weight)
    {
      if (weight < 0)
        weight = 0;

      int hash = choice_val.GetHashCode();

      if (TryFindIndex(hash, out int i))
      {
        m_TotalWeight -= m_Choices[i].Weight;
        m_Choices[i].SetWeight(weight);
      }
      else
      {
        i = m_Choices.Count;
        m_DupeMap[hash] = i;
        m_Choices.Add(new SerialChoice(hash, weight, i));
      }

      m_TotalWeight += m_Choices[i].Weight;
    }


    private bool TryFindIndex(int hash, out int i)
    {
      return m_DupeMap.Find(hash, out i) && i < m_Choices.Count;
    }


    public IEnumerator<SerialChoice> GetEnumerator()
    {
      return m_Choices.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return m_Choices.GetEnumerator();
    }


    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }
    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      "SerialChoiceList: TODO: Remove the need for ISerializationCallbackReceiver".Log();

      int n;

    Recurse:

      m_TotalWeight = 0;

      n = m_Choices.Count;

      if (n == 0)
      {
        m_DupeMap.Clear();
        return;
      }

      m_DupeMap.Reinit(n);

      while (n --> 0)
      {
        var choice = m_Choices[n];

        if (!m_DupeMap.Map(choice.Hash, n))
        {
          $"SerialChoiceList: Error deserializing - found duplicate. {choice}".LogError();
          m_Choices.RemoveAt(n);
          goto Recurse;
        }

        choice.SetIndex(n);
        m_TotalWeight += choice.Weight;
      }

      Debug.Assert(m_DupeMap.Count == m_Choices.Count);
    }

  }

}