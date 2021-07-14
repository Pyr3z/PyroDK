/**
@file   PyroDK/Core/DataTypes/CellRect.cs
@author Levi Perez (Pyr3z)
@author https://leviperez.dev
@date   2021-01-31

@brief
  Defines a 2D rectangle of cells for representation in integral grids.
**/

using System.Collections.Generic;
using System.Collections;

using UnityEngine;


namespace PyroDK
{

  using Cell2D = Vector2Int;


  [System.Serializable]
  public struct CellRect : IEnumerable<Cell2D>, System.IEquatable<CellRect>, System.IEquatable<RectInt>
  {
    public bool IsValid   => 0 < W && 0 < H;

    public int Width
    {
      get => W;
      set
      {
        if (value < 0)
        {
          X += value;
          W = -value;
        }
        else
        {
          W = value;
        }
      }
    }
    public int Height
    {
      get => H;
      set
      {
        if (value < 0)
        {
          Y += value;
          H = -value;
        }
        else
        {
          H = value;
        }
      }
    }

    public int MinX
    {
      get => X;
      set
      {
        int max = X + W;
        X = value;
        W = max - value;
      }
    }
    public int MaxX
    {
      get => X + W;
      set
      {
        W = value - X;
      }
    }


    public Cell2D Position
    {
      get => new Cell2D(X, Y);
      set
      {
        X = value.x;
        Y = value.y;
      }
    }

    public Vector2Int Size
    {
      get => new Vector2Int(W, H);
      set => SetSize(value);
    }

    public Cell2D Min
    {
      get => new Cell2D(X, Y);
      set
      {
        var max = Max;
        X = value.x;
        Y = value.y;
        max -= value;
        W = max.x;
        H = max.y;
      }
    }
    public Cell2D Max
    {
      get => new Cell2D(X + W, Y + H);
      set
      {
        W = value.x - X;
        H = value.y - Y;
      }
    }
    
    public Vector2 Center
    {
      get => new Vector2(X + W / 2f, Y + H / 2f);
    }


    [SerializeField]
    public int X, Y;
    
    [SerializeField] [Min(1)]
    public int W, H;
    


    public CellRect(Cell2D pos, Vector2Int size)
    {
      if (size.x < 0)
      {
        pos.x += size.x;
        size.x = -size.x;
      }
      if (size.y < 0)
      {
        pos.y += size.y;
        size.y = -size.y;
      }

      X = pos.x;
      Y = pos.y;
      W = size.x;
      H = size.y;
    }
    public CellRect(in CellRect other)
    {
      X = other.X;
      Y = other.Y;
      W = other.W;
      H = other.H;
      _ = Validate();
    }

    public static CellRect FromRectInt(in RectInt irect)
    {
      return new CellRect(irect.position, irect.size);
    }

    public static RectInt ToRectInt(in CellRect crect)
    {
      return new RectInt(crect.Position, crect.Size);
    }


    public void SetSize(Vector2Int size)
    {
      if (size.x < 0)
      {
        X += size.x;
        W = -size.x;
      }
      else
      {
        W = size.x;
      }

      if (size.y < 0)
      {
        Y += size.y;
        H = -size.y;
      }
      else
      {
        H = size.y;
      }
    }
    
    public void SetMinMax(Cell2D min, Cell2D max)
    {
      X = min.x;
      Y = min.y;
      SetSize(max - min);
    }

    public bool Validate()
    {
      if (W < 0)
      {
        X += W;
        W = -W;
      }

      if (H < 0)
      {
        Y += H;
        H = -H;
      }

      return IsValid;
    }



    public IEnumerator<Cell2D> GetEnumerator()
    {
      return new RectInt.PositionEnumerator(Min, Max);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return new RectInt.PositionEnumerator(Min, Max);
    }


    public override int GetHashCode()
    {
      return (int)Hashing.MixHashes((uint)X, (uint)Y, (uint)W, (uint)H);
    }
    public override bool Equals(object obj)
    {
      return obj is CellRect crect && Equals(in this, in crect);
    }
    public bool Equals(CellRect crect)
    {
      return Equals(in this, in crect);
    }
    public bool Equals(RectInt irect)
    {
      return EqualsRectInt(in this, in irect);
    }


    // static utilities:

    public static bool Equals(in CellRect a, in CellRect b)
    {
      return a.GetHashCode() == b.GetHashCode();
    }
    public static bool EqualsRectInt(in CellRect crect, in RectInt irect)
    {
      return crect.Position == irect.position && crect.Size == irect.size;
    }


    // operators: TODO: Reconsider `implicit operator`s?

    public static implicit operator RectInt (in CellRect crect)
    {
      return ToRectInt(in crect);
    }
    public static implicit operator CellRect (in RectInt irect)
    {
      return new CellRect(irect.position, irect.size);
    }
    
    public static explicit operator bool (in CellRect self)
    {
      return self.IsValid;
    }

    public static bool operator == (in CellRect lhs, in CellRect rhs)
    {
      return Equals(in lhs, in rhs);
    }
    public static bool operator != (in CellRect lhs, in CellRect rhs)
    {
      return !Equals(in lhs, in rhs);
    }

  }


#if false

  [System.Serializable]
  public struct Size : System.IEquatable<Size>, System.IEquatable<Vector2Int>
  {
    public bool IsZero => Width == 0 && Height == 0;

    [SerializeField]
    public int Width, Height;

    public Size(int w, int h)
    {
      Width  = w;
      Height = h;
    }
    public Size(Vector2Int v)
    {
      Width  = v.x;
      Height = v.y;
    }


    public override int GetHashCode()
    {
      // adapted to match Vector2Int's implementation:
      return Width ^ (Height << 2);
    }
    public override bool Equals(object obj)
    {
      return  (obj is Vector2Int v && Equals(v)) ||
              (obj is Size sz && Equals(sz));
    }
    public bool Equals(Size other)
    {
      return Equals(this, other);
    }
    public bool Equals(Vector2Int other)
    {
      return Equals(this, other);
    }


    public static bool Equals(Size lhs, Size rhs)
    {
      return lhs.Width == rhs.Width && lhs.Height == rhs.Height;
    }
    public static bool Equals(Size lhs, Vector2Int rhs)
    {
      return lhs.Width == rhs.x && lhs.Height == rhs.y;
    }


    public static implicit operator Vector2Int (Size self)
    {
      return new Vector2Int(self.Width, self.Height);
    }
    public static implicit operator Size (Vector2Int v)
    {
      return new Size(v);
    }

    public static bool operator == (Size lhs, Size rhs)
    {
      return Equals(lhs, rhs);
    }
    public static bool operator != (Size lhs, Size rhs)
    {
      return !Equals(lhs, rhs);
    }

    public static bool operator ==(Size lhs, Vector2Int rhs)
    {
      return Equals(lhs, rhs);
    }
    public static bool operator !=(Size lhs, Vector2Int rhs)
    {
      return !Equals(lhs, rhs);
    }
  }

#endif

}