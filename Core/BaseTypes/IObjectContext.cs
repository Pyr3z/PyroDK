/**
@file   PyroDK/Core/BaseTypes/IObjectContext.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-14

@brief
The interface that defines objects who can provide context for
a given DataElement instance.
**/

using UnityEngine;


namespace PyroDK
{
  [System.Obsolete("not useful.")]
  public interface IObjectContext<TObject>
  {
    bool HasObject(TObject obj);
    bool TryResolveID(int id, out TObject obj);
    void RegisterReference(TObject obj);
  }
}