/**
@file   PyroDK/Game3D/ComponentTypes/RendererInterface.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-08-10

@brief
  Data type that stores and handles 1st-Person
  mouse looking details.
**/


using System.Collections;

using UnityEngine;


namespace PyroDK.Game3D
{
  
  [AddComponentMenu("PyroDK/Game3D/Renderer Interface")]
  public class RendererInterface : BaseInterface<Renderer>
  {
    public override bool IsInterfaceConnected
    {
      get => m_TargetComponent && m_MaterialIndex >= 0;
    }

    public float Blend
    {
      get => m_Blend;
      set
      {
        m_Blend = value.Clamp01();
        UpdateMaterialBlend();
      }
    }

    public Material BlendToMaterial
    {
      get => m_BlendToMaterial;
      set => m_BlendToMaterial = value;
    }


    [SerializeField]
    private int m_MaterialIndex;
    [SerializeField] [ReadOnly]
    private Material m_StartMaterial;
    [SerializeField]
    private Material m_BlendToMaterial;
    [SerializeField] [Range(0f, 1f)]
    private float    m_Blend;
    [SerializeField] [SignBitBool]
    private float    m_SmoothBlendRate = 1f;


    [System.NonSerialized]
    private Material m_CurrMaterial;
    [System.NonSerialized]
    private Coroutine m_AsyncBlend;


    public void SmoothBlendTo(float target_blend)
    {
      if (!m_CurrMaterial || !m_StartMaterial || !m_BlendToMaterial)
      {
        "Invalid materials for blend.".LogWarning(this);
        return;
      }

      if (m_SmoothBlendRate < Floats.EPSILON)
      {
        Blend = target_blend;
        return;
      }

      if (m_AsyncBlend != null)
        StopCoroutine(m_AsyncBlend);

      m_AsyncBlend = StartCoroutine(BlendMaterialAsync(target_blend));
    }

    public void SmoothBlendToFill(FillValue fill)
    {
      SmoothBlendTo(fill.Normalized);
    }


    protected override void OnValidate()
    {
      base.OnValidate();

      if (!m_TargetComponent)
      {
        m_MaterialIndex = -1;
        m_StartMaterial = null;
        return;
      }

      // fucks up if "Material Index" is changed in the editor at play time..
      m_MaterialIndex = m_MaterialIndex.ClampIndex(m_TargetComponent.sharedMaterials.Length);

      bool playing = Application.IsPlaying(this);
      if (playing)
      {
        if (!m_CurrMaterial)
          m_CurrMaterial = GetTargetMaterial();
        UpdateMaterialBlend();
      }
      else
      {
        m_StartMaterial = GetSharedMaterial();
      }
    }

    private void OnEnable()
    {
      if (m_CurrMaterial = GetTargetMaterial())
      {
        UpdateMaterialBlend();
      }
    }

    private void OnDisable()
    {
      m_CurrMaterial = null;
    }

    private Material GetTargetMaterial()
    {
      if (m_TargetComponent && m_MaterialIndex >= 0)
      {
        var mats = m_TargetComponent.materials;

        if (m_MaterialIndex < mats.Length)
          return mats[m_MaterialIndex];
      }

      return null;
    }

    private Material GetSharedMaterial()
    {
      if (m_TargetComponent && m_MaterialIndex >= 0)
      {
        var shared_mats = m_TargetComponent.sharedMaterials;

        if (m_MaterialIndex < shared_mats.Length)
          return shared_mats[m_MaterialIndex];
      }

      return null;
    }

    private bool UpdateMaterialBlend()
    {
      if (m_CurrMaterial && m_StartMaterial && m_BlendToMaterial)
      {
        m_CurrMaterial.Lerp(m_StartMaterial, m_BlendToMaterial, m_Blend);
        return true;
      }

      return false;
    }


    private IEnumerator BlendMaterialAsync(float target_blend)
    {
      target_blend = target_blend.Clamp01();

      if (m_Blend.Approximately(target_blend))
      {
        m_Blend = target_blend;
        m_CurrMaterial.Lerp(m_StartMaterial, m_BlendToMaterial, m_Blend);
        yield break;
      }

      float start = m_Blend;
      float time  = Time.unscaledTime;
      float t     = 0f;

      while (t < 1f)
      {
        yield return new WaitForEndOfFrame();

        t = (Time.unscaledTime - time) * m_SmoothBlendRate;
        m_Blend = start.SmoothSteppedTo(target_blend, t);
        m_CurrMaterial.Lerp(m_StartMaterial, m_BlendToMaterial, m_Blend);
      }

      m_Blend = target_blend;
      m_CurrMaterial.Lerp(m_StartMaterial, m_BlendToMaterial, m_Blend);
    }

  } // end class RendererInterface

}
