/**
@file   PyroDK/Core/BaseTypes/ISpawnable.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-12

@brief
  Interface for objects that can be spawned and owned
  by a SpawnPool.
**/

using UnityEngine;


namespace PyroDK
{

  public interface ISpawnable : IComponent
  {
    SpawnPool   Owner   { get; }
    GameObject  Spawner { get; }

    void DespawnNow();

    // these should be implemented explicitly (hidden):
    void OnPooled(SpawnPool pool);
    void OnSpawned(GameObject spawner);
    void OnDespawn();

  }

}