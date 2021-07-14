/**
@file   PyroDK/Core/BaseTypes/IPyroEvent.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-25

@brief
  An improved interface to replace UnityEvents.
**/


namespace PyroDK
{

  public interface IPyroEvent
  {
    bool IsEnabled { get; set; }
  }

  public interface IHaveAName // lol TODO : lol?
  {
    string Name { get; }
    void Bonk();
  }

}