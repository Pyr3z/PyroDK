/**
@file   PyroDK/Core/BaseTypes/IDamageable.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-12

@brief
  Interface for objects that can be owned by some
  sort of object pool.
**/


namespace PyroDK
{

  public interface IDamageable : IComponent
  {
    FillValue Health { get; }
  }


  public static class Damageables
  {

    public static bool TryTakeDamage(this IDamageable dmgable, float amount, out bool lethal)
    {
      if (dmgable.Health.TryApplyDelta(-amount))
      {
        lethal = dmgable.Health.IsEmpty;
        return true;
      }

      lethal = false;
      return false;
    }

    public static bool TryHeal(this IDamageable dmgable, float amount)
    {
      return dmgable.Health.TryApplyDelta(amount);
    }

  }

}