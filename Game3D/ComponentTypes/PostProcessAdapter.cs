/**
@file   PyroDK/Game3D/ComponentTypes/PostProcessAdapter.cs
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

  #if SRP_CORE

  using Volume = UnityEngine.Rendering.Volume;

  
  [AddComponentMenu("PyroDK/Game3D/Post-Processing Volume Adapter")]
  public class PostProcessAdapter : BaseAdapter<Volume>
  {

    public float Weight
    {
      get => m_TargetComponent ? m_TargetComponent.weight : 0f;
      set
      {
        if (m_TargetComponent)
          m_TargetComponent.weight = value.Clamp01();
      }
    }

    public float TargetWeightFadeIn  => m_FlipFadeSemantics ? 0f : 1f;
    public float TargetWeightFadeOut => m_FlipFadeSemantics ? 1f : 0f;


    [SerializeField]
    private bool m_FlipFadeSemantics = false;


    public void FadeIn(float duration)
    {
      StopAllCoroutines();
      _ = StartCoroutine(FadeAsync(TargetWeightFadeIn, duration));
    }

    public void FadeOut(float duration)
    {
      StopAllCoroutines();
      _ = StartCoroutine(FadeAsync(TargetWeightFadeOut, duration));
    }


    public IEnumerator FadeInAsync(float duration)
    {
      return FadeAsync(TargetWeightFadeIn, duration);
    }

    public IEnumerator FadeOutAsync(float duration)
    {
      return FadeAsync(TargetWeightFadeOut, duration);
    }

    public IEnumerator FadeAsync(float target_weight, float duration)
    {
      #if DEBUG
      if (Logging.Assert(m_TargetComponent, "Target Volume is unassigned!"))
        yield break;
      #endif

      target_weight = target_weight.Clamp01();

      if (m_TargetComponent.weight.Approximately(target_weight))
      {
        // early out if we're close enough to the target weight
        m_TargetComponent.weight = target_weight;
        yield break;
      }

      float start = m_TargetComponent.weight;
      float time  = Time.unscaledTime;
      float t     = 0f;

      duration = 1f / duration;

      while (t < 1f)
      {
        yield return new WaitForEndOfFrame();

        t = (Time.unscaledTime - time) * duration;
        m_TargetComponent.weight = start.SmoothSteppedTo(target_weight, t);
      }

      m_TargetComponent.weight = target_weight;
    }

  } // end class PostProcessAdapter


  #else // !SRP_CORE

  public class PostProcessAdapter : BaseAdapter<Object>
  {

    // TODO : fill with dummy code

  }

  #endif

}
