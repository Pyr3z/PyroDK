/**
@file   PyroDK/Core/DataTypes/FillValue.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-24

@brief
  A POD-ish object for easy control of "fill" values.
**/

#pragma warning disable CS0649

using UnityEngine;
using UnityEngine.Events;


namespace PyroDK
{

  [System.Serializable]
  public sealed class FillValue
  {
    [System.Serializable]
    public sealed class Event : PyroEvent<FillValue>
    {
    }


    private const float DEFAULT_MAX = 1f;


    public bool IsFull  => m_Value.IsMaxValue;
    public bool IsEmpty => m_Value.IsMinValue;

    public float Value
    {
      get => m_Value.Value;
      set
      {
        value = m_Value.ClampInRange(value);

        if (m_Value.Approximately(value))
        {
          //m_Value.Value = value;
          return;
        }

        m_Previous = m_Value.Value;
        m_Value.Value = value;

        m_OnValueChanged.TryInvoke(this);
      }
    }
    public float Normalized
    {
      get => m_Value.Normalized;
      set => Value = m_Value.LerpInRange(value);
    }

    public float MaxValue
    {
      get => m_Value.Max;
      set
      {
        m_Value.Max = value;
        Value = m_Value.Value; // applies a clamp
      }
    }

    public float MinValue => m_Value.Min;

    public float PreviousValue => m_Previous;
    public float PreviousDelta => m_Value.Value - m_Previous;


    public Color32 FillColor
    {
      get => m_FillColor;
      set => m_FillColor = value;
    }


    public event UnityAction<FillValue> OnValueChanged
    {
      add     => m_OnValueChanged.AddListener(value);
      remove  => m_OnValueChanged.RemoveListener(value);
    }



    [SerializeField]
    private MinMaxFloat m_Value;
    [SerializeField]
    private Color32     m_FillColor = Colors.Grey;
    [SerializeField]
    private Event       m_OnValueChanged;


    [System.NonSerialized]
    private float m_Previous;


    public FillValue(Color32 color = default, float max = DEFAULT_MAX, float start_value = DEFAULT_MAX)
    {
      m_Value = MinMaxFloat.Make(start_value)
                           .WithMinMax(0f, max)
                           .Validated();
      m_Previous = m_Value;
      m_FillColor = color;
    }


    public void Reset()
    {
      Value = m_Value.Max;
    }


    public bool TryApplyDelta(float delta)
    {
      if (delta.IsZero())
        return false;

      float prev = m_Value.Value;
      m_Value.Value = prev + delta;
      m_Value.ClampValue();

      m_Previous = prev;

      m_OnValueChanged.TryInvoke(this);

      return true;
    }


    public static implicit operator float (FillValue fv)
    {
      return fv.m_Value.Value;
    }

  } // end class FillValue

}