/**
@file   PyroDK/Core/ComponentTypes/PerformanceSnitch.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-20

@brief
  The `PerformanceSnitch` type rats to you basic performance stats
  at runtime, and is noted for its simplicity to use.
**/

using UnityEngine;
using UnityEngine.UI;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/[Debug]/[Performance Snitch]")]
  public sealed class PerformanceSnitch : BaseSingleton<PerformanceSnitch>
  {
    public enum GUIType
    {
      IMGUIBottomLeft,
      IMGUIBottomRight,
      IMGUITopLeft,
      IMGUITopRight,
      CanvasText,
      WorldText
    }


    [Header("Configure Snitching GUI")]
    [SerializeField]
    private GUIType  m_GUIType = GUIType.IMGUIBottomLeft;
    [SerializeField]
    private Vector2  m_IMGUIOffsetFromCorner = new Vector2(35.0f, 35.0f);
    [SerializeField]
    private Color32  m_TextColor = Color.green;

    [Header("Configure Sampling")]
    [SerializeField]
    private float    m_SampleRate = 0.25f;


    private int       m_PrevFrameCount  = 0;
    private float     m_CurrTime        = 0.0f;
    private float     m_CurrFPS         = 60.0f;
    private string    m_CurrFPSString   = "<NO DATA>";
    private Rect      m_IMGUIPos        = new Rect();

    private const int IMGUI_WIDTH   = 50;
    private const int IMGUI_HEIGHT  = 22;

    
    [SerializeField] [HideInInspector]
    private Text      m_Text;
    [SerializeField] [HideInInspector]
    private TextMesh  m_TextMesh;



    public void DevConsoleVisible(bool visible)
    {
      #if DEBUG
      Debug.developerConsoleVisible = visible;

      if (visible)
      {
        $"Wowzers!".LogError(this);
      }
      #endif
    }

    public void ClearDevConsole()
    {
      #if DEBUG
      Debug.ClearDeveloperConsole();
      Debug.developerConsoleVisible = false;
      #endif
    }



    protected override void OnValidate()
    {
      base.OnValidate();

      if (m_GUIType == GUIType.CanvasText)
      {
        m_Text = GetComponent<Text>();
      }
      else if (m_GUIType == GUIType.WorldText)
      {
        m_TextMesh = GetComponent<TextMesh>();
      }
    }


    private void LateUpdate()
    {
      m_CurrTime += Time.unscaledDeltaTime;

      if (m_CurrTime >= m_SampleRate)
      {
        m_CurrFPS = (Time.frameCount - m_PrevFrameCount) / m_CurrTime;
        m_CurrFPSString = m_CurrFPS.ToString("N1");

        m_CurrTime = 0.0f;
        m_PrevFrameCount = Time.frameCount;

        if (m_Text)
        {
          m_Text.color = m_TextColor;
          m_Text.text  = m_CurrFPSString;
        }
        if (m_TextMesh)
        {
          m_TextMesh.color = m_TextColor;
          m_TextMesh.text = m_CurrFPSString;
        }
      }
    }


    private void OnGUI()
    {
      switch (m_GUIType)
      {
        case GUIType.IMGUIBottomLeft:
          m_IMGUIPos.Set(m_IMGUIOffsetFromCorner.x, Screen.height - IMGUI_HEIGHT - m_IMGUIOffsetFromCorner.y, IMGUI_WIDTH, IMGUI_HEIGHT);
          break;
        case GUIType.IMGUIBottomRight:
          m_IMGUIPos.Set(Screen.width - IMGUI_WIDTH - m_IMGUIOffsetFromCorner.x, Screen.height - IMGUI_HEIGHT - m_IMGUIOffsetFromCorner.y, IMGUI_WIDTH, IMGUI_HEIGHT);
          break;
        case GUIType.IMGUITopLeft:
          m_IMGUIPos.Set(m_IMGUIOffsetFromCorner.x, m_IMGUIOffsetFromCorner.y, IMGUI_WIDTH, IMGUI_HEIGHT);
          break;
        case GUIType.IMGUITopRight:
          m_IMGUIPos.Set(Screen.width - IMGUI_WIDTH - m_IMGUIOffsetFromCorner.x, m_IMGUIOffsetFromCorner.y, IMGUI_WIDTH, IMGUI_HEIGHT);
          break;
        default:
          return;
      }

      GUI.color = m_TextColor;
      GUI.Box(m_IMGUIPos, m_CurrFPSString);
    }

  }

}