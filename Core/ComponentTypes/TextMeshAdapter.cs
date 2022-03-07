/**
@file   PyroDK/Core/ComponentTypes/TextMeshAdapter.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  TextMesh Pro!!
**/

#if TMPRO

using System.Collections.Generic;

using UnityEngine;

using TMPro;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/TextMesh Pro Adapter")]
  public class TextMeshAdapter : BaseAdapter<TMP_Text>
  {

    [SerializeField] // [ReadOnly]
    private Color m_InitialColor;


    [System.NonSerialized]
    private List<float> m_AlphaStack = new List<float>(capacity: 5);



    public void PushAlpha(float alpha)
    {
      m_AlphaStack.PushBack(m_TargetComponent.alpha);
      m_TargetComponent.alpha = alpha;
    }

    public void PopAlpha()
    {
      if (m_AlphaStack.Count > 0)
        m_TargetComponent.alpha = m_AlphaStack.PopBack();
      else
        m_TargetComponent.alpha = m_InitialColor.a;
    }

    public void ResetAlpha()
    {
      if (m_AlphaStack.Count > 0)
      {
        m_TargetComponent.alpha = m_AlphaStack[0];
        m_AlphaStack.Clear();
      }
      else
      {
        m_TargetComponent.alpha = m_InitialColor.a;
      }
    }


    #if UNITY_EDITOR
    protected override void OnValidate()
    {
      base.OnValidate();

      if (m_TargetComponent)
      {
        m_InitialColor = m_TargetComponent.color;
      }
    }
    #endif

  }

}

#endif // TMPRO