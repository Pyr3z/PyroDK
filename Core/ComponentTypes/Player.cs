/**
@file   PyroDK/Core/ComponentTypes/Player.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-15

@brief
  Scene-global Singleton script, for referencing the Player's
  GameObject (and associated Components) from anywhere.
**/

#pragma warning disable UNT0008 // null propogation is fine.

using UnityEngine;

#if INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Singletons/Player (Core)")]
  public class Player : BaseSingleton<Player>
  {
    public static Camera WorldCamera  => s_Instance?.m_WorldCamera;
    public static Camera UICamera     => PyroDK.UICamera.Camera;


    #if INPUT_SYSTEM
    public static PlayerInput Input => s_Instance?.m_Input;
    #else
    public static Object Input => s_Instance?.m_Input;
    #endif
    


  [Header("Scene References")]
    [SerializeField] [RequiredReference]
    protected Camera m_WorldCamera;

    [SerializeField]
    #if INPUT_SYSTEM
    [RequiredReference(color_hex: "#EEA02011")]
    protected PlayerInput m_Input;
    #else
    [ReadOnly]
    protected Object m_Input;
    #endif



    protected virtual void Start()
    {
      if (!m_WorldCamera)
      {
        if (Camera.main)
          m_WorldCamera = Camera.main;
        else
          $"{TSpy<Player>.LogName} has no valid World Camera attached!"
            .LogWarning(this);
      }
    }

    protected override void OnValidate()
    {
      base.OnValidate();

      if (!m_WorldCamera)
      {
        m_WorldCamera = GetComponentInChildren<Camera>();
      }

      #if INPUT_SYSTEM
      if (!m_Input)
      {
        m_Input = GetComponent<PlayerInput>();
      }
      #endif
    }

  }

}