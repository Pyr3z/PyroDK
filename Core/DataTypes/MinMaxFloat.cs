/**
@file   PyroDK/Core/DataTypes/MinMaxFloat.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-24

@brief
  Defines the `MinMaxFloat` type.
**/

using UnityEngine;


namespace PyroDK
{

  [System.Serializable]
  public struct MinMaxFloat :
    System.IComparable<MinMaxFloat>,
    System.IEquatable<MinMaxFloat>,
    System.IComparable<float>,
    System.IEquatable<float>
  {
    public static readonly MinMaxFloat Zero = MakeConstant(0f);
    public static readonly MinMaxFloat One  = MakeConstant(1f);


    public float Normalized
    {
      get => (Stride == 0f) ? 1f : (Value - Origin) / Stride;
      set => Value = Mathf.LerpUnclamped(Origin, Origin + Stride, value);
    }

    public float Min
    {
      get => Origin;
      set => Origin = value;
    }
    public float Max
    {
      get => Origin + Stride;
      set => Stride = value - Origin;
    }



    public bool HasMinMax => Stride > 0f && (Origin + Stride).IsFinite();

    public bool IsMinValue => Value == Origin;
    public bool IsMaxValue => Value == Origin + Stride;

    public bool IsConstantValue => Stride == 0f;

    public bool IsMinMaxLocked => Stride < 0f;


    public float Value;
    public float Origin;
    public float Stride;


    private MinMaxFloat(float value, float min = 0f, float max = 1f)
    {
      Value  = value;
      Origin = min;
      Stride = max - min;
    }



    public static MinMaxFloat Make(float value = 0f)
    {
      return new MinMaxFloat(value);
    }
    public static MinMaxFloat MakeConstant(float value)
    {
      return new MinMaxFloat(value, value, value);
    }


    public MinMaxFloat WithValue(float value)
    {
      Value = value;
      return this;
    }
    public MinMaxFloat WithMin(float min)
    {
      Origin = min.AtMost(Stride);

      //Debug.Assert(m_Min == min, $"MinMaxFloat.WithMin() clamped the value ({min}) to ({m_Min}).");

      return this;
    }
    public MinMaxFloat WithMax(float max)
    {
      Stride = max.AtLeast(Origin);

      //Debug.Assert(m_Max == max, $"MinMaxFloat.WithMax() clamped the value ({max}) to ({m_Max}).");

      return this;
    }
    public MinMaxFloat WithMinMax(float min, float max)
    {
      SetMinMax(min, max);
      return this;
    }
    public MinMaxFloat Validated()
    {
      ValidateRange();
      ClampValue();
      return this;
    }


    public void ValidateRange()
    {
      if (Stride < 0f && Stride.IsSquarable())
      {
        Origin += Stride;
        Stride = -Stride;
      }
    }

    public void ClampValue()
    {
      if (Stride.IsFinite())
      {
        if (Stride == 0f || Value < Origin)
          Value = Origin;
        else if (Origin + Stride < Value)
          Value = Origin + Stride;
      }
    }


    public void SetValueInRange(float value)
    {
      if (Stride == 0f)
        Value = Origin;
      else
        Value = value.Clamp(Origin, Origin + Stride);
    }

    public void SetMinMax(float min, float max)
    {
      Origin = min;
      Stride = max - min;
      ValidateRange();
    }

    public void SetRange(float origin, float stride)
    {
      Origin = origin;
      Stride = stride;
      ValidateRange();
    }


    public override int GetHashCode()
    {
      return Hashing.MakeHash(Origin, Stride);
    }

    public override string ToString()
    {
      return $"{Value}f : [{Min},{Max}]";
    }

    public override bool Equals(object obj)
    {
      if (obj is MinMaxFloat tf)
        return Equals(tf);

      return Value.Equals(obj);
    }



    public int CompareTo(MinMaxFloat mmf)
    {
      return Value.CompareTo(mmf.Value);
    }
    public bool Equals(MinMaxFloat mmf)
    {
      return Value == mmf.Value &&
             Origin == mmf.Origin &&
             Stride == mmf.Stride;
    }


    public int CompareTo(float f)
    {
      return Value.CompareTo(f);
    }
    public bool Equals(float f)
    {
      return Value.Equals(f);
    }


    public float ClampInRange(float f)
    {
      return f.Clamp(Origin, Origin + Stride);
    }

    public float LerpInRange(float t)
    {
      return Mathf.LerpUnclamped(Origin, Origin + Stride, t);
    }

    public bool Approximately(float f)
    {
      return Value.Approximately(f);
    }
    public bool IsMin(float f)
    {
      return f < Origin + Floats.EPSILON;
    }



    public static implicit operator float (MinMaxFloat mmf)
    {
      return mmf.Value;
    }


    public static MinMaxFloat operator + (MinMaxFloat lhs, float rhs)
    {
      lhs.Value += rhs;
      return lhs;
    }

    public static MinMaxFloat operator - (MinMaxFloat lhs, float rhs)
    {
      lhs.Value -= rhs;
      return lhs;
    }

    public static MinMaxFloat operator / (MinMaxFloat lhs, float rhs)
    {
      lhs.Value /= rhs;
      return lhs;
    }

    public static MinMaxFloat operator * (MinMaxFloat lhs, float rhs)
    {
      lhs.Value *= rhs;
      return lhs;
    }


  }

}