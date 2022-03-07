/**
@file   PyroDK/Core/BaseTypes/IPromiseKeeper.cs
@author Levi Perez (Pyr3z)
@date   2021-06-06
**/


namespace PyroDK
{

  public interface IPromiseKeeper : System.IDisposable
  {
    bool IsDisposed { get; }
  }


  public static class PromiseKeepers
  {

    public static bool IsVoid(this IPromiseKeeper keeper)
    {
      return keeper == null || keeper.IsDisposed;
    }

    public static void SafeDispose(this System.IDisposable keeper)
    {
      if (keeper != null)
        keeper.Dispose();
    }

  }

}