/**
@file   PyroDK/Core/ComponentTypes/UICamera.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-15

@brief
  A Singleton-patterned controller for your current UI Camera.
**/

#pragma warning disable UNT0008 // null propogation with Unity objects is fine.

using UnityEngine;

#if SRP_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Singletons/Camera (UI)")]
  [RequireComponent(typeof(Camera))]
  #if SRP_UNIVERSAL
  [RequireComponent(typeof(UniversalAdditionalCameraData))]
  #endif
  public class UICamera : BaseSingleton<UICamera>
  {
    public static Camera Camera => s_Instance?.m_Camera;

    #if SRP_UNIVERSAL
    public static UniversalAdditionalCameraData CameraData => s_Instance?.m_URPData;
    #endif


  [Header("[ReadOnly]")]
    [SerializeField] [ReadOnly]
    private Camera m_Camera;

    #if SRP_UNIVERSAL
    [SerializeField] [ReadOnly]
    private UniversalAdditionalCameraData m_URPData;
    #endif


    public void AddToRenderStack()
    {
      if (!m_Camera)
        return;

      #if SRP_UNIVERSAL // must put itself onto the main camera's stack
      if (!Camera.main || Camera.main == m_Camera)
      {
        m_URPData.renderType = CameraRenderType.Base;
      }
      else if (Camera.main.TryGetComponent(out UniversalAdditionalCameraData ucam) &&
               ucam.renderType == CameraRenderType.Base)
      {
        m_URPData.renderType = CameraRenderType.Overlay;
        ucam.cameraStack.RemoveAll((c) => c.gameObject == gameObject);
        ucam.cameraStack.Add(m_Camera);
      }
      #endif
    }

    public void RemoveFromRenderStack()
    {
      #if SRP_UNIVERSAL // must put itself onto the main camera's stack
      if (Camera.main && m_URPData.renderType == CameraRenderType.Overlay &&
          Camera.main.TryGetComponent(out UniversalAdditionalCameraData ucam))
      {
        ucam.cameraStack.RemoveAll((c) => c.gameObject == gameObject);
      }

      m_URPData.renderType = CameraRenderType.Base;
      #endif
    }


    protected override void OnEnable()
    {
      base.OnEnable();
      AddToRenderStack();
    }

    protected override void OnDisable()
    {
      base.OnDisable();
      RemoveFromRenderStack();
    }

    protected override void OnValidate()
    {
      base.OnValidate();

      Logging.Assert(m_Camera || TryGetComponent(out m_Camera), "m_Camera");

      #if SRP_UNIVERSAL
      Logging.Assert(m_URPData || TryGetComponent(out m_URPData), "m_URPData");
      #endif
    }

  }

}