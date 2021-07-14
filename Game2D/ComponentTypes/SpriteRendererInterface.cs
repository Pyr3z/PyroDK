/**
@file   PyroDK/Game2D/ComponentTypes/SpriteRendererInterface.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-09-25
**/

#pragma warning disable CS0649

using System.Collections;

using UnityEngine;
using UnityEngine.Events;


namespace PyroDK.Game2D
{

  [AddComponentMenu("PyroDK/Game2D/SpriteRenderer Interface")]
  [DisallowMultipleComponent]
  public sealed class SpriteRendererInterface : BaseComponent
  {

    public float Alpha
    {
      get => m_Renderer.color.a;
      set
      {
        var c = m_Renderer.color;
        c.a = value.Clamp01();
        m_Renderer.color = c;
      }
    }

    public float AlphaFloor
    {
      get => m_AlphaFloor;
      set => m_AlphaFloor = value.Clamp01();
    }

    public float AlphaCeiling
    {
      get => m_AlphaCeiling;
      set => m_AlphaCeiling = value.Clamp01();
    }


  [Header("Ranges")]
    [SerializeField]
    private float m_AlphaFloor = 0.0f;
    [SerializeField]
    private float m_AlphaCeiling = 1.0f;

  [Header("Events")]
    [SerializeField]
    private UnityEvent m_OnFadeFinished;

  [Header("References")]
    [SerializeField] [RequiredReference]
    private SpriteRenderer m_Renderer;



    public void FadeIn(float seconds)
    {
      if (!m_Renderer)
      {
        "No SpriteRenderer assigned!".LogWarning(this);
        return;
      }

      StopAllCoroutines();

      if (seconds.IsZero())
      {
        Alpha = m_AlphaCeiling;
      }
      else
      {
        _ = StartCoroutine(FadeAlphaAsync(m_Renderer, m_AlphaCeiling, seconds));
      }
    }

    public void FadeOut(float seconds)
    {
      if (!m_Renderer)
      {
        "No SpriteRenderer assigned!".LogWarning(this);
        return;
      }

      StopAllCoroutines();

      if (seconds.IsZero())
      {
        Alpha = m_AlphaFloor;
      }
      else
      {
        _ = StartCoroutine(FadeAlphaAsync(m_Renderer, m_AlphaFloor, seconds));
      }
    }

    public void StartOscillateAlpha(float rate)
    {
      if (!m_Renderer)
      {
        $"No SpriteRendered assigned!".LogWarning(this);
        return;
      }

      StopAllCoroutines();

      if (!rate.IsZero())
      {
        _ = StartCoroutine(OscillateAlphaAsync(m_Renderer, m_AlphaFloor, m_AlphaCeiling, rate));
      }
    }



    private void OnValidate()
    {
      if (!Application.IsPlaying(this))
      {
        if (!m_Renderer)
        {
          m_Renderer = GetComponentInChildren<SpriteRenderer>();
        }
      }

      m_AlphaFloor    = m_AlphaFloor.Clamp(0.0f, m_AlphaCeiling.AtMost(1.0f));
      m_AlphaCeiling  = m_AlphaCeiling.Clamp(m_AlphaFloor.AtLeast(0.0f), 1.0f);
    }


    private IEnumerator FadeAlphaAsync(SpriteRenderer sprite, float target, float s = 1.0f)
    {
      var   c     = sprite.color;
      float start = c.a;

      if (start.Approximately(target))
      {
        c.a = target;
        sprite.color = c;
        yield break;
      }

      float time  = Time.time;
      float t     = 0.0f;

      while (t < 1.0f)
      {
        c.a = start.SmoothSteppedTo(target, t);
        sprite.color = c;

        t = (Time.time - time) / s;

        yield return new WaitForEndOfFrame();
      }

      c.a = target;
      sprite.color = c;

      m_OnFadeFinished.Invoke();
    }

    private IEnumerator OscillateAlphaAsync(SpriteRenderer sprite, float min, float max, float rate)
    {
      var   c = sprite.color;
      float t = Floats.InverseSinusoidalParameter(c.a);

      while (sprite) // quasi-infinite loop
      {
        yield return new WaitForEndOfFrame();

        c.a = Floats.SinusoidalLoop(min, max, t);
        sprite.color = c;

        t += Time.smoothDeltaTime * rate;
      }
    }

  }

}