/**
@file   PyroDK/Game3D/ComponentTypes/Button3D.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-12

@brief
  Uses the 3D Physics system to expose a collider as a
  clickable button.
**/

using UnityEngine;
using UnityEngine.EventSystems;


namespace PyroDK.Game3D
{

  public interface IPointerHandler :
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerUpHandler
  {
  }


  [AddComponentMenu("PyroDK/Game3D/Button Trigger (3D)")]
  [ScriptOrder(10, inherit: true)]
  public class Button3D : BaseComponent, IPointerHandler
  {

    [Space]
    [SerializeField]
    private DelayedEvent m_OnFocus      = new DelayedEvent();

    [Space]
    [SerializeField]
    private DelayedEvent m_OnUnfocus    = new DelayedEvent();

    [Space]
    [SerializeField]
    private DelayedEvent m_OnBeginPress = new DelayedEvent();

    [Space]
    [SerializeField]
    private DelayedEvent m_OnPressed    = new DelayedEvent();


    public void HelloWorld(string message = null)
    {
      if (message.IsEmpty())
      {
        $"Hello, world! It's me, \"{name}\"!"
          .LogSuccess();
      }
      else
      {
        $"Hello, world! It's me, \"{message}\"!"
          .LogSuccess();
      }
    }


    private void Start()
    {
      // so we can enable/disable in Inspector...
    }




    void IPointerClickHandler.OnPointerClick(PointerEventData ev)
    {
      if (!isActiveAndEnabled)
        return;

      m_OnPressed.TryInvokeOn(this);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData ev)
    {
      if (!isActiveAndEnabled)
        return;

      m_OnBeginPress.TryInvokeOn(this);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData ev)
    {
      if (!isActiveAndEnabled)
        return;

      m_OnFocus.TryInvokeOn(this);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData ev)
    {
      if (!isActiveAndEnabled)
        return;

      m_OnUnfocus.TryInvokeOn(this);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData ev)
    {
      // no-op
    }
  }

}