/**
@file   PyroDK/Core/Editor/Styles.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-09

@brief
  Cache of some custom styles for re-use.
**/

using System.Collections.Generic;

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{
  using MethodInfo = System.Reflection.MethodInfo;


  [InitializeOnLoad]
  public static class Styles
  {
    public static class Defaults
    {
      public static GUISkin Skin => s_Skin;

      public static GUIStyle Label           => s_Label           ?? s_SkinFallback.GetStyle("ControlLabel");
      public static GUIStyle Tooltip         => s_Tooltip         ?? s_SkinFallback.GetStyle("Tooltip");
      public static GUIStyle TextField       => s_TextField       ?? s_SkinFallback.GetStyle("TextField");
      public static GUIStyle NumberField     => TextField;
      public static GUIStyle TitleBox        => s_TitleBox        ?? s_SkinFallback.GetStyle("IN BigTitle");
      public static GUIStyle TitleText       => s_TitleText       ?? s_SkinFallback.GetStyle("IN TitleText");
      public static GUIStyle Button          => s_Button          ?? s_SkinFallback.GetStyle("miniButton");
      public static GUIStyle Popup           => s_Popup           ?? s_SkinFallback.GetStyle("MiniPopup");
      public static GUIStyle Foldout         => s_Foldout         ?? s_SkinFallback.GetStyle("Foldout");
      public static GUIStyle FoldoutHeader   => s_FoldoutHeader   ?? s_SkinFallback.GetStyle("FoldoutHeader");


      private static GUISkin s_Skin;

      private static readonly GUISkin s_SkinFallback
        = EditorGUIUtility.LoadRequired("builtin skins/generated/skins/darkskin.guiskin") as GUISkin;


      private static GUIStyle s_Label;
      private static GUIStyle s_Tooltip;
      private static GUIStyle s_TextField;
      private static GUIStyle s_TitleBox;
      private static GUIStyle s_TitleText;
      private static GUIStyle s_Button;
      private static GUIStyle s_Popup;
      private static GUIStyle s_Foldout;
      private static GUIStyle s_FoldoutHeader;


      internal static bool PopulateAndTweak()
      {
        if (!typeof(GUIUtility).TryGetMethod<int>("GetDefaultSkin", out MethodInfo getskin, TypeMembers.NONPUBLIC))
        {
          Logging.ShouldNotReach();
          return false;
        }

        s_Skin = getskin.Invoke(null, new object[] { (int)EditorSkin.Inspector }) as GUISkin;

        if (!s_Skin)
          return false;

        // These style strings were ripped from the UnityEditor DLLs
        var ctrl_label   = s_Skin.GetStyle("ControlLabel");
        var tooltip      = s_Skin.GetStyle("Tooltip");
        var textfield    = s_Skin.GetStyle("TextField");
        var title_box    = s_Skin.GetStyle("IN BigTitle");
        var title        = s_Skin.GetStyle("IN TitleText"); // used namely for component titles
        var button       = s_Skin.GetStyle("miniButton");
        var popup        = s_Skin.GetStyle("MiniPopup");
        var foldout      = s_Skin.GetStyle("Foldout");
        var foldout_head = s_Skin.GetStyle("FoldoutHeader");

        // Store copies here in Defaults:
        s_Label           = new GUIStyle(ctrl_label);
        s_Tooltip         = new GUIStyle(tooltip);
        s_TextField       = new GUIStyle(textfield);
        s_TitleBox        = new GUIStyle(title_box);
        s_TitleText       = new GUIStyle(title);
        s_Button          = new GUIStyle(button);
        s_Popup           = new GUIStyle(popup);
        s_Foldout         = new GUIStyle(foldout);
        s_FoldoutHeader   = new GUIStyle(foldout_head);

        // Tweak the vanilla styles to mesh better with Pyro styles:
        s_Skin.font =
          ctrl_label.font =
          tooltip.font =
          foldout.font =
          foldout_head.font =
          PyroLabelFont;

        textfield.font =
          popup.font =
          title.font =
          PyroMonoFont;

        s_Skin.label.richText =
          ctrl_label.richText =
          tooltip.richText =
          foldout.richText =
          foldout_head.richText =
          s_Skin.button.richText =
          title.richText =
          true;

        s_Skin.label.wordWrap =
          s_Skin.button.wordWrap =
          ctrl_label.wordWrap =
          foldout.wordWrap =
          foldout_head.wordWrap =
          title.wordWrap =
          false;

        s_Skin.label.fontSize =
          ctrl_label.fontSize =
          foldout.fontSize =
          foldout_head.fontSize =
          s_Skin.button.fontSize =
          button.fontSize =
          12;

        textfield.fontSize =
          popup.fontSize =
          tooltip.fontSize =
          title.fontSize =
          11;

        title.fontStyle =
          FontStyle.Bold;

        textfield.SetTextColor(Colors.Debug.Value);
        popup.SetTextColor(Colors.Debug.Value);
        title.SetTextColor(Colors.Grey, StyleState.Normal);

        return true;
      }
    } // end class Defaults


    // TODO make this more portable and/or use Addressables. This is currently shit and I admit it.
    private const string PATH_FONT_LABEL = "Assets/PyroDK/Core/Assets/Fonts/IBMPlex/IBMPlexSans-Medium.ttf";
    private const string PATH_FONT_MONO  = "Assets/PyroDK/Core/Assets/Fonts/IBMPlex/IBMPlexMono-Medium.ttf";
    private const string PATH_FONT_TITLE = "Assets/PyroDK/Core/Assets/Fonts/IBMPlex/IBMPlexSerif-Medium.ttf";
    //


    public static readonly Font PyroLabelFont   = EditorGUIUtility.LoadRequired(PATH_FONT_LABEL) as Font;
    public static readonly Font PyroMonoFont    = EditorGUIUtility.LoadRequired(PATH_FONT_MONO)  as Font;
    public static readonly Font PyroTitleFont   = EditorGUIUtility.LoadRequired(PATH_FONT_TITLE) as Font;


    public static GUIStyle Label            => s_Label ?? Defaults.Label;
    public static GUIStyle LabelCenter      => s_LabelCenter ?? Defaults.Label;
    public static GUIStyle LabelRight       => s_LabelRight ?? Defaults.Label;
    public static GUIStyle LabelDetail      => s_LabelDetail ?? Defaults.Label;
    public static GUIStyle TextInfo         => s_TextInfo ?? Defaults.Label;
    public static GUIStyle TextInfoSmall    => s_TextInfoSmall ?? Defaults.Label;
    public static GUIStyle TextDetail       => s_TextDetail ?? Defaults.Label;
    public static GUIStyle TextDetailCenter => s_TextDetailCenter ?? Defaults.Tooltip;
    public static GUIStyle TextDetailLeft   => s_TextDetailLeft ?? Defaults.Tooltip;
    public static GUIStyle Foldout          => s_Foldout ?? Defaults.Foldout;
    public static GUIStyle TextField        => s_TextField ?? Defaults.TextField;
    public static GUIStyle TextFieldMulti   => s_TextFieldMulti ?? Defaults.TextField;
    public static GUIStyle TextFieldBad     => s_TextFieldBad ?? Defaults.TextField;
    public static GUIStyle TextFieldGood    => s_TextFieldGood ?? Defaults.TextField;
    public static GUIStyle TextFieldType    => s_TextFieldType ?? Defaults.TextField;
    public static GUIStyle NumberField      => s_NumberField ?? Defaults.NumberField;
    public static GUIStyle NumberFieldBad   => s_NumberFieldBad ?? Defaults.NumberField;
    public static GUIStyle NumberInfo       => s_NumberInfo ?? Defaults.Label;
    public static GUIStyle NumberInfoBad    => s_NumberInfoBad ?? Defaults.Label;
    public static GUIStyle PathField        => s_PathField ?? Defaults.TextField;
    public static GUIStyle PathFieldExists  => s_PathFieldExists ?? Defaults.TextField;
    public static GUIStyle PathFieldInvalid => s_PathFieldInvalid ?? Defaults.TextField;
    public static GUIStyle PathInfo         => s_PathInfo ?? Defaults.Label;
    public static GUIStyle PathInfoExists   => s_PathInfoExists ?? Defaults.Label;
    public static GUIStyle PathInfoInvalid  => s_PathInfoInvalid ?? Defaults.Label;
    public static GUIStyle Button           => s_Button ?? Defaults.Button;
    public static GUIStyle ButtonSmall      => s_ButtonSmall ?? Defaults.Button;
    public static GUIStyle ButtonBig        => s_ButtonBig ?? Defaults.Button;
    public static GUIStyle TitleBox         => s_TitleBox ?? Defaults.TitleBox;
    public static GUIStyle TitleText        => s_TitleText ?? Defaults.Label;
    public static GUIStyle TitleTextSmall   => s_TitleTextSmall ?? Defaults.Label;


    public static readonly GUIStyle Section = new GUIStyle()
    {
      name = "PyroSection",
      overflow = new RectOffset(18, 4, 4, 0),
    } .SetBackgroundTexture(Textures.SectionBG);

    public static readonly GUIStyle Box = new GUIStyle()
    {
      name = "PyroBox",
      overflow = new RectOffset(4, 2, 2, 2)
    } .SetBackgroundTexture(Textures.FieldBG);


    private static GUIStyle s_Label;
    private static GUIStyle s_LabelCenter;
    private static GUIStyle s_LabelRight;
    private static GUIStyle s_LabelDetail;
    private static GUIStyle s_TextInfo;
    private static GUIStyle s_TextInfoSmall;
    private static GUIStyle s_TextDetail;
    private static GUIStyle s_TextDetailCenter;
    private static GUIStyle s_TextDetailLeft;
    private static GUIStyle s_Foldout;
    private static GUIStyle s_TextField;
    private static GUIStyle s_TextFieldMulti;
    private static GUIStyle s_TextFieldBad;
    private static GUIStyle s_TextFieldGood;
    private static GUIStyle s_TextFieldType;
    private static GUIStyle s_NumberField;
    private static GUIStyle s_NumberFieldBad;
    private static GUIStyle s_NumberInfo;
    private static GUIStyle s_NumberInfoBad;
    private static GUIStyle s_PathField;
    private static GUIStyle s_PathFieldExists;
    private static GUIStyle s_PathFieldInvalid;
    private static GUIStyle s_PathInfo;
    private static GUIStyle s_PathInfoExists;
    private static GUIStyle s_PathInfoInvalid;
    private static GUIStyle s_Button;
    private static GUIStyle s_ButtonSmall;
    private static GUIStyle s_ButtonBig;
    private static GUIStyle s_TitleBox;
    private static GUIStyle s_TitleText;
    private static GUIStyle s_TitleTextSmall;


    static Styles()
    {
      EditorApplication.delayCall += Initialize;
    }

    [MenuItem("PyroDK/Debug Shims/Reinit Styles")]
    private static void Initialize()
    {
      if (!Defaults.PopulateAndTweak())
      {
        "Failed to populate PyroDK.Editor.Styles.Defaults!"
          .LogError();
        return;
      }

      PyroLabelFont.name = "PyroLabelFont";
      PyroMonoFont.name  = "PyroMonoFont";

      s_Label = new GUIStyle(Defaults.Label)
      {
        alignment = TextAnchor.MiddleLeft,
        richText = true,
        wordWrap = false,
        font = PyroLabelFont,
        fontSize = 12,
      };

      s_LabelCenter = new GUIStyle(s_Label)
      {
        alignment = TextAnchor.MiddleCenter,
      };

      s_LabelRight = new GUIStyle(s_Label)
      {
        alignment = TextAnchor.MiddleRight,
      };

      s_LabelDetail = new GUIStyle(s_LabelRight)
      {
        fontSize = 10,
      };

      s_TextInfo = new GUIStyle(s_Label)
      {
        fixedHeight = 0f,
        stretchHeight = true,
        clipping = TextClipping.Overflow,
        wordWrap = true,
        font = PyroMonoFont,
      };

      s_TextInfoSmall = new GUIStyle(s_TextInfo)
      {
        fontSize = 10,
      };

      s_TextDetail = new GUIStyle(Defaults.Label)
      {
        alignment = TextAnchor.MiddleRight,
        fontStyle = FontStyle.Normal,
        richText = true,
        wordWrap = false,
        font = PyroMonoFont,
        fontSize = 10,
      };

      s_TextDetailCenter = new GUIStyle(s_TextDetail)
      {
        alignment = TextAnchor.MiddleCenter
      };

      s_TextDetailLeft = new GUIStyle(s_TextDetail)
      {
        alignment = TextAnchor.MiddleLeft
      };

      s_TextField = new GUIStyle(Defaults.TextField)
      {
        fontSize = 11,
        font = PyroMonoFont,
      } .SetTextColor(Colors.Debug.String);

      s_TextFieldMulti = new GUIStyle(s_TextField)
      {
        wordWrap = true,
      };

      s_TextFieldBad = new GUIStyle(s_TextField)
        .SetTextColor(Colors.Debug.Attention);

      s_TextFieldGood = new GUIStyle(s_TextField)
        .SetTextColor(Colors.Debug.StringGood);

      s_TextFieldType = new GUIStyle(s_TextField)
        .SetTextColor(Colors.Debug.TypeByRef);

      s_NumberField = new GUIStyle(Defaults.NumberField)
      {
        fontSize = 11,
        font = PyroMonoFont,
      } .SetTextColor(Colors.Debug.Value);

      s_NumberFieldBad = new GUIStyle(s_NumberField)
        .SetTextColor(Colors.Debug.Warning);

      s_NumberInfo = new GUIStyle(s_Label)
      {
        fontSize = 11,
        font = PyroMonoFont,
      } .SetTextColor(Colors.Debug.Value);

      s_NumberInfoBad = new GUIStyle(s_NumberInfo)
      {
        fontStyle = FontStyle.Bold,
      } .SetTextColor(Colors.Debug.String);

      s_PathField = new GUIStyle(s_TextField)
      {
        fontSize = 10,
      } .SetTextColor(Colors.Debug.StringGood);

      s_PathFieldExists = new GUIStyle(s_PathField)
        .SetTextColor(Colors.Debug.Value);

      s_PathFieldInvalid = new GUIStyle(s_PathField)
        .SetTextColor(Colors.Debug.Error);

      s_PathInfo = new GUIStyle(Defaults.Label)
      {
        richText = false,
        font = PyroMonoFont,
        fontSize = 10,
      } .SetTextColor(Colors.Debug.Warning);

      s_PathInfoExists = new GUIStyle(s_PathInfo)
        .SetTextColor(Colors.Debug.Success);

      s_PathInfoInvalid = new GUIStyle(s_PathInfo)
        .SetTextColor(Colors.Debug.Error);

      s_Button = new GUIStyle(Defaults.Button)
      {
        richText = true,
        clipping = TextClipping.Clip,
        fixedHeight = GUIDrawers.STD_LINE_HEIGHT,
        padding = new RectOffset(4, 4, 1, 1),
        font = PyroLabelFont,
        fontStyle = FontStyle.Normal,
        fontSize = 12,
        alignment = TextAnchor.MiddleCenter,
      };

      s_ButtonSmall = new GUIStyle(s_Button)
      {
        fixedHeight = s_Button.fixedHeight - 2f,
        fontSize = 10,
      };

      s_ButtonBig = new GUIStyle(s_Button)
      {
        fixedHeight = s_Button.fixedHeight * 2f - 2f,
        fontStyle = FontStyle.Bold,
        fontSize = 14,
      };

      s_TitleBox = new GUIStyle(Defaults.TitleBox)
      {
        padding = new RectOffset(7, 0, 2, 4), // original: 4, 4, 7, 8
      };

      s_TitleText = new GUIStyle(Defaults.TitleText)
      {
        richText = true,
        wordWrap = false,
        clipping = TextClipping.Clip,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        fontSize = 14,
        font = PyroMonoFont,
        fixedHeight = 0f,
      };

      s_TitleTextSmall = new GUIStyle(s_TitleText)
      {
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleLeft,
        fontSize = 11,
      } .SetTextColor(Colors.Debug.Value);

      s_Foldout = new GUIStyle(Defaults.Foldout)
      {
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleLeft,
        richText  = true,
        wordWrap  = false,
        fontSize  = 12,
        font      = PyroLabelFont,
        padding   = new RectOffset(16, 0, 0, 0), // default: 14, 0, 0, 0
      //margin    = new RectOffset(),            // default: 0, 4, 0, 0
      //overflow  = new RectOffset(),            // default: 0, 0, -2, 0
      };
    }


    [System.Flags]
    public enum StyleState
    {
      None      = (0 << 0),

      Normal    = (1 << 0),
      Hover     = (1 << 1),
      Focused   = (1 << 2),

      Pressed   = (1 << 3), // onActive

      NormalOn  = (1 << 4),
      HoverOn   = (1 << 5),
      FocusedOn = (1 << 6),

      Display = Normal | Hover | Focused,

      All = Normal | NormalOn | Hover | HoverOn | Focused | FocusedOn
    }

    public static IEnumerable<GUIStyleState> GetStates(this GUIStyle style, StyleState states)
    {
      while (states != StyleState.None)
      {
        StyleState curr = Bitwise.LSB(states);

        switch (curr)
        {
          case StyleState.Normal:
            yield return style.normal;
            break;
          case StyleState.Hover:
            yield return style.hover;
            break;
          case StyleState.Focused:
            yield return style.focused;
            break;
          case StyleState.Pressed:
            yield return style.onActive;
            break;
          case StyleState.NormalOn:
            yield return style.onNormal;
            break;
          case StyleState.HoverOn:
            yield return style.onHover;
            break;
          case StyleState.FocusedOn:
            yield return style.onFocused;
            break;
        }

        states &= ~curr;
      }
    }

    public static GUIStyle SetTextColor(this GUIStyle style, Color color, StyleState states = StyleState.Display)
    {
      foreach (var state in GetStates(style, states))
      {
        state.textColor = color;
      }

      return style;
    }

    public static GUIStyle SetBackgroundTexture(this GUIStyle style, Texture2D tex, StyleState states = StyleState.Normal)
    {
      foreach (var state in GetStates(style, states))
      {
        state.background = tex;
      }

      return style;
    }


    public static float CalcWidth(this GUIStyle style, string text)
    {
      Labels.Scratch.text = text;
      return style.CalcSize(Labels.Scratch).x;
    }

    public static float CalcWidth(this GUIStyle style, GUIContent content)
    {
      if (content == null || content == GUIContent.none)
        return 0f;
      return style.CalcSize(content).x;
    }


    public static float CalcHeight(this GUIStyle style, string text)
    {
      Labels.Scratch.text = text;
      return style.CalcHeight(Labels.Scratch, GUIDrawers.ContentWidth);
    }

    public static float CalcHeight(this GUIStyle style, string text, float width)
    {
      Labels.Scratch.text = text;
      return style.CalcHeight(Labels.Scratch, width);
    }

    public static float CalcFieldHeight(this GUIStyle style, string text)
    {
      Labels.Scratch.text = text;
      return style.CalcHeight(Labels.Scratch, GUIDrawers.FieldWidth);
    }


    public static bool CalcFit(this GUIStyle style, string text, Rect rect)
    {
      Labels.Scratch.text = text;
      return style.CalcHeight(Labels.Scratch, rect.width) <= rect.height;
    }

    public static bool CalcFit(this GUIStyle style, string text)
    {
      return CalcFit(style, text, new Rect(0f, 0f, GUIDrawers.FieldWidth, GUIDrawers.STD_LINE_HEIGHT));
    }



    [MenuItem("PyroDK/Debug Info Loggers/Log OS Fonts")]
    private static void LogOSFonts()
    {
      Font.GetPathsToOSFonts().MakeLogString("Paths to OS Fonts")
        .Log();
      
      Font.GetOSInstalledFontNames().MakeLogString("All Installed OS Fonts")
        .Log();
    }

    [MenuItem("PyroDK/Debug Info Loggers/Log Current \"Pyro\" Fonts")]
    private static void LogCurrentPyroFonts()
    {
      PyroMonoFont.ToString().Log();

      PyroLabelFont.ToString().Log();

      $"skin.font: \"{Defaults.Skin.font.name}\""
        .Log(Defaults.Skin);
      $"skin.label.font: \"{Defaults.Skin.GetStyle("ControlLabel").font.name}\""
        .Log(Defaults.Skin);
    }

  }

}