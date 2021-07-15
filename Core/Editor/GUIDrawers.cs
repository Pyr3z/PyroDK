/**
@file   PyroDK/Core/AssetTypes/GUIDrawers.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-20

@brief
  A static partial class containing all custom GUIDrawer types
  built-in to PyroDK.Core.
**/

using System.Collections.Generic;

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  // TODO move, ye lazy bum
  public class FoldoutHeader : GUI.Scope
  {
    public Rect Rect;
    public readonly bool IsOpen, IsVanilla, IsListElement;
    public readonly int  Indent;

    private FoldoutHeader(Rect pos, GUIContent content, bool is_open, bool is_list_el, int indent)
    {
      if (is_list_el)
      {
        pos.xMin += 5f;
        GUIDrawers.PushLabelWidth(EditorGUIUtility.labelWidth - 8f);
        indent -= 1;
      }

      if (indent > 0)
      {
        pos.xMin += indent * GUIDrawers.STD_INDENT;
        GUIDrawers.PushIndentLevel(indent - 1, fix_label_width: false);
      }

      Indent        = indent;
      Rect          = pos;
      IsOpen        = EditorGUI.BeginFoldoutHeaderGroup(pos, is_open, content);
      IsVanilla     = true;
      IsListElement = is_list_el;
    }

    private FoldoutHeader(GUIContent content, bool is_open, bool indent) // used by OpenLayout()
    {
      IsVanilla = false;

      GUILayout.BeginVertical(Styles.Section, GUILayout.MinHeight(GUIDrawers.STD_LINE_ADVANCE));

      Rect = GUILayoutUtility.GetRect(GUIDrawers.STD_LINE_HEIGHT,
                                      GUIDrawers.STD_LINE_HEIGHT,
                                      Styles.Defaults.FoldoutHeader,
                                      GUILayout.ExpandWidth(true));
      Rect.xMin = 5f; // from default inspector minimum margins

      IsOpen = GUI.Toggle(Rect, is_open, content, Styles.Defaults.FoldoutHeader);

      if (indent)
      {
        GUIDrawers.PushNextIndentLevel(fix_label_width: false);
        Indent = EditorGUI.indentLevel;
      }
      else
      {
        Indent = -1;
      }
    }



    protected override void CloseScope()
    {
      if (IsVanilla)
        EditorGUI.EndFoldoutHeaderGroup();
      else
        GUILayout.EndVertical();

      if (Indent > 0)
        GUIDrawers.PopIndentLevel(fix_label_width: false);

      if (IsListElement)
        GUIDrawers.PopLabelWidth();
    }


    public static bool Open(Rect total, GUIContent content, SerializedProperty prop, out FoldoutHeader header, int indent = -1)
    {
      return prop.isExpanded = header = new FoldoutHeader(total, content, prop.isExpanded, prop.IsArrayElement(), indent);
    }

    public static bool OpenLayout(GUIContent content, SerializedProperty prop, out FoldoutHeader header, bool indent)
    {
      return prop.isExpanded = header = new FoldoutHeader(content, prop.isExpanded, indent);
    }

    public static bool OpenLayout(string heading, SerializedProperty prop, out FoldoutHeader header, bool indent)
    {
      return OpenLayout(EditorGUIUtility.TrTempContent(heading), prop, out header, indent);
    }

    public static implicit operator bool (FoldoutHeader fh)
    {
      return fh != null && fh.IsOpen;
    }
  }


  public static partial class GUIDrawers
  {
    public static float Indent          => EditorGUI.indentLevel * STD_INDENT;

    // TODO this is defunct in reorderable lists!
    public static float LabelStartX     => STD_INDENT_0 + EditorGUI.indentLevel * STD_INDENT;
    public static float LabelEndX       => FieldStartX - STD_PAD;
    public static float LabelWidth      => EditorGUIUtility.labelWidth;
    public static float LabelWidthRaw   => s_LabelWidthStack.Front(fallback: EditorGUIUtility.labelWidth);
    public static float LabelWidthHalf  => LabelWidth * 0.45f;

    public static float FieldStartX     => FieldStartXRaw;
    public static float FieldStartXRaw  => FieldEndX * 0.45f - STD_INDENT_0;
    public static float FieldEndX       => EditorGUIUtility.currentViewWidth - STD_PAD_RIGHT;
    public static float FieldWidth      => Mathf.Max(FieldEndX - FieldStartXRaw, EditorGUIUtility.fieldWidth);

    public static float ViewWidth       => EditorGUIUtility.currentViewWidth;
    public static float ContentWidth    => FieldEndX - LabelStartX;



    public const float STD_LINE_HEIGHT  = 18f; // EditorGUIUtility.singleLineHeight
    public const float STD_LINE_HALF    = STD_LINE_HEIGHT / 2f;

    public const float STD_INDENT_0     = STD_LINE_HEIGHT;
    public const float STD_INDENT       = STD_LINE_HEIGHT - 3f;

    public const float STD_PAD          = 2f; // == EditorGUIUtility.standardVerticalSpacing
    public const float STD_PAD_HALF     = STD_PAD / 2f;
    public const float STD_PAD_RIGHT    = STD_PAD * 2f;

    public const float STD_LINE_ADVANCE = STD_LINE_HEIGHT + STD_PAD;

    public const float STD_TOGGLE_W     = STD_LINE_HEIGHT - STD_PAD_HALF;
    public const float STD_TOGGLE_H     = STD_LINE_HEIGHT + STD_PAD_HALF;

    public const float STD_BTN_W        = 14f;
    public const float STD_BTN_H        = 12f;

    public const float MIN_TOGGLE_W     = STD_TOGGLE_W - STD_PAD;
    public const float MIN_TOGGLE_H     = STD_TOGGLE_H - STD_PAD;


    public static readonly float ARRAY_INDEX_LABEL_WIDTH = Styles.NumberInfo.CalcWidth("[00]");


    internal static readonly int s_HashTextField = "EditorTextField".GetHashCode();
    internal static readonly int s_HashSelectableLabel = "s_SelectableLabel".GetHashCode();

    #if !NO_RICK_ROLL

    private static readonly string[] s_LoremIpsum =
    {
      "We're no strangers to love",
      "You know the rules and so do I",
      "A full commitment's what I'm thinking of",
      "You wouldn't get this from any other guy",

      "I just wanna tell you how I'm feeling",
      "Gotta make you understand",

      RichText.Emphasis("Never gonna give you up"),
      RichText.Emphasis("Never gonna let you down"),
      RichText.Emphasis("Never gonna run around and desert you"),
      RichText.Emphasis("Never gonna make you cry"),
      RichText.Emphasis("Never gonna say goodbye"),
      RichText.Emphasis("Never gonna tell a lie and hurt you"),

      "We've known each other for so long",
      "Your heart's been aching but you're too shy to say it",
      "Inside we both know what's been going on",
      "We know the game and we're gonna play it",

      "And if you ask me how I'm feeling",
      "Don't tell me you're too blind to see",

      RichText.Italics("Never gonna give you up"),
      RichText.Italics("Never gonna let you down"),
      RichText.Italics("Never gonna run around and desert you"),
      RichText.Italics("Never gonna make you cry"),
      RichText.Italics("Never gonna say goodbye"),
      RichText.Italics("Never gonna tell a ..."),

      RichText.Italics("..."),
      
      ".",
    };

    #else

    private static readonly string[] s_LoremIpsum =
    {
      "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
      "Etiam tincidunt urna in finibus tempor.",

      "In ac urna ac massa consequat convallis.",
      "Donec ac diam tempus, tincidunt libero non, volutpat sem.",
      
      "Vestibulum eu quam id enim finibus porttitor vitae ut odio.",
      
      "Proin sit amet sapien non ante lacinia euismod.",
      "Mauris molestie massa laoreet lacus feugiat, eget cursus velit venenatis.",
      
      "Pellentesque sollicitudin quam eu nisl laoreet, sit amet porta massa pharetra.",
      "Nunc ac nunc vitae felis mollis ultrices.",
      "Sed non neque bibendum, porta massa non, tincidunt sapien.",
    };

    #endif // NO_RICK_ROLL

    public static float CalcHeight(float count_lines)
    {
      if (count_lines > 1f)
        return ((int)count_lines - 1) * STD_LINE_ADVANCE + STD_LINE_HEIGHT;
      else
        return STD_LINE_HEIGHT;
    }


    public static void InvalidField(Rect pos, string message = null, GUIContent label = null, bool outline = true)
    {
      if (outline)
      {
        DrawRect(in pos, Colors.Debug.Attention, Colors.Debug.Boring);
      }

      using (Labels.Pool.MakePromiseIfNull(ref label))
      {
        label.text = RichText.Make($"{label.text} (!?)", RichText.Style.Bold, Colors.Debug.Attention);

        if (label.tooltip.IsEmpty())
        {
          label.tooltip = $"{label.text}\nA CustomPropertyDrawer or CustomEditor\nencountered an error trying to draw this field.";
        }
        else
        {
          label.tooltip += $"\n{label.text}\nA CustomPropertyDrawer or CustomEditor\nencountered an error trying to draw this field.";
        }

        if (message.IsEmpty())
        {
          message = "<< UNSPECIFIED ERROR >>";
        }

        pos.height = STD_LINE_HEIGHT;
        pos = PrefixLabelStrict(pos, label, Styles.Label);
        EditorGUI.SelectableLabel(pos, message, Styles.Label);
      }
    }

    public static void InvalidField(in Rect pos, GUIContent label, string message = null, bool outline = true)
    {
      InvalidField(pos, message, label, outline);
    }

    public static void InvalidFieldLayout(string label_str = null, string message = null)
    {
      using (Labels.Pool.MakePromise(out GUIContent label))
      {
        label.text = label_str ?? "INVALID";
        InvalidField(EditorGUILayout.GetControlRect(true, STD_LINE_HEIGHT, Styles.Label), message, label);
      }
    }

    public static void InvalidFieldLayout(GUIContent label, string message = null)
    {
      InvalidField(EditorGUILayout.GetControlRect(true, STD_LINE_HEIGHT, Styles.Label), message, label);
    }


    public static void InfoField(Rect pos, string text,
                                 GUIStyle   style       = null,
                                 GUIContent label       = null,
                                 GUIStyle   label_style = null)
    {
      if (!label.IsEmpty())
        pos = PrefixLabelLax(pos, label, label_style ?? Styles.Label, id: -1);

      bool enabled = GUI.enabled;
      GUI.enabled = true;

      PushIndentLevel(0);
      EditorGUI.LabelField(pos, text, style ?? Styles.TextInfo);
      PopIndentLevel();

      GUI.enabled = enabled;
    }

    public static void InfoFieldLayout(string   text,
                                       GUIStyle style       = null,
                                       string   label       = null,
                                       GUIStyle label_style = null)
    {
      bool use_label  = !label.IsEmpty();
      var  calc_style = style ?? Styles.TextInfo;

      using (Labels.Pool.MakePromise(out GUIContent content))
      {
        content.text = text;

        float height = (use_label) ? (FieldWidth) : (FieldWidth + LabelWidth + STD_PAD);

        height = Mathf.Ceil(calc_style.CalcHeight(content, height) / STD_LINE_ADVANCE) * STD_LINE_ADVANCE - STD_PAD;

        content.text = label;

        InfoField(pos:         GUILayoutUtility.GetRect(ContentWidth, height),
                  text:        text,
                  style:       calc_style,
                  label:       content,
                  label_style: label_style);
      }
    }


    private static List<float> s_LabelWidthStack = new List<float>();
    public static void PushLabelWidth(float width)
    {
      s_LabelWidthStack.PushBack(EditorGUIUtility.labelWidth);
      EditorGUIUtility.labelWidth = width;
    }
    public static void PopLabelWidth()
    {
      if (s_LabelWidthStack.Count > 0)
      {
        EditorGUIUtility.labelWidth = s_LabelWidthStack.PopBack();
      }
    }
    public static void ResetLabelWidth()
    {
      if (s_LabelWidthStack.Count > 0)
      {
        EditorGUIUtility.labelWidth = s_LabelWidthStack[0];
        s_LabelWidthStack.Clear();
      }
    }


    private static List<TextAnchor> s_LabelAlignmentStack = new List<TextAnchor>();
    public static void PushLabelAlign(TextAnchor align)
    {
      s_LabelAlignmentStack.PushBack(EditorStyles.label.alignment);
      EditorStyles.label.alignment = Styles.Label.alignment = align;
    }
    public static void PopLabelAlign()
    {
      if (s_LabelAlignmentStack.Count > 0)
      {
        EditorStyles.label.alignment = Styles.Label.alignment = s_LabelAlignmentStack.PopBack();
      }
      else
      {
        EditorStyles.label.alignment = Styles.Label.alignment = TextAnchor.MiddleLeft;
      }
    }


    //private static Stack<Color> s_TextFieldColorStack = new Stack<Color>();
    //private static void PushStylesColorInternal(GUIStyle textfield, Color color)
    //{
    //  s_TextFieldColorStack.Push(textfield.normal.textColor);
    //  s_TextFieldColorStack.Push(textfield.focused.textColor);
    //  s_TextFieldColorStack.Push(textfield.hover.textColor);
    //  textfield.normal.textColor  =
    //  textfield.hover.textColor   =
    //  textfield.focused.textColor = color;
    //}
    //private static void PopStyleColorInternal(GUIStyle textfield)
    //{
    //  if (s_TextFieldColorStack.Count > 2)
    //  {
    //    textfield.normal.textColor  = s_TextFieldColorStack.Pop();
    //    textfield.hover.textColor   = s_TextFieldColorStack.Pop();
    //    textfield.focused.textColor = s_TextFieldColorStack.Pop();
    //  }
    //  else
    //  {
    //    if (Logging.Assert(s_TextFieldColorStack.Count == 0, "Bad math. Trust lost."))
    //    {
    //      s_TextFieldColorStack.Clear();
    //    }
        
    //    textfield.normal.textColor  = Styles.Defaults.TextField.normal.textColor;
    //    textfield.hover.textColor   = Styles.Defaults.TextField.hover.textColor;
    //    textfield.focused.textColor = Styles.Defaults.TextField.focused.textColor;
    //  }
    //}
    
    //public static void PushTextFieldColor(Color32 color)
    //{
    //  PushStylesColorInternal(EditorStyles.textField, color);
    //}
    //public static void PopTextFieldColor()
    //{
    //  PopStyleColorInternal(EditorStyles.textField);
    //}


    private static List<int> s_IndentLvlStack = new List<int>();
    public static void PushIndentLevel(int lvl, bool fix_label_width = true)
    {
      if (lvl < 0)
        lvl = 0;

      s_IndentLvlStack.PushBack(EditorGUI.indentLevel);

      EditorGUI.indentLevel = lvl;

      if (fix_label_width)
        PushLabelWidth(LabelWidthRaw - STD_INDENT * lvl);
    }
    public static void PopIndentLevel(bool fix_label_width = true)
    {
      if (s_IndentLvlStack.Count > 0)
      {
        EditorGUI.indentLevel = s_IndentLvlStack.PopBack();

        if (fix_label_width)
          PopLabelWidth();
      }
    }
    public static void ResetIndentLevel()
    {
      s_IndentLvlStack.Clear();
      EditorGUI.indentLevel = 0;
    }
    public static void PushNextIndentLevel(bool fix_label_width = true, int delta = 1)
    {
      PushIndentLevel(EditorGUI.indentLevel + delta, fix_label_width);
    }


    public static bool IsClick(in Rect pos, int button = 0)
    {
      if (Event.current.type != EventType.MouseDown)
        return false;

      bool enabled = GUI.enabled;
      GUI.enabled = true;

      bool is_click = ( button == -1 || Event.current.button == button ) &&
                        pos.Contains(Event.current.mousePosition);

      GUI.enabled = enabled;

      return is_click;
    }

    public static bool IsClick(in Rect pos, Color32 outline, Color32 outline_hover = default, int button = 0)
    {
      if (Event.current.alt || Event.current.type != EventType.Repaint)
      {
        //
      }
      else if (!outline_hover.IsClear() && pos.Contains(Event.current.mousePosition))
      {
        DrawRect(in pos, outline_hover);
      }
      else if (!outline.IsClear())
      {
        DrawRect(in pos, outline);
      }

      return IsClick(in pos, button);
    }


    public static bool IsContextClick(in Rect pos)
    {
      return IsClick(in pos, button: 1);
    }

    public static bool IsContextClick(in Rect pos, Color32 outline, Color32 outline_hover = default)
    {
      return IsClick(in pos, outline, outline_hover, button: 1);
    }

    public static bool IsAnyClick(in Rect pos)
    {
      return IsClick(in pos, button: -1);
    }

    public static bool IsAnyClick(in Rect pos, Color32 outline, Color32 outline_hover = default)
    {
      return IsClick(in pos, outline, outline_hover, button: -1);
    }


    public static Rect MakePrefixRect(in Rect total)
    {
      return new Rect(x:      total.x + Indent,
                      y:      total.y,
                      width:  LabelWidth,
                      height: STD_LINE_HEIGHT);
    }


    public static Rect MakeFieldRectStrict(in Rect total)
    {
      return new Rect(x:      total.x + EditorGUIUtility.labelWidth + STD_PAD,
                      y:      total.y,
                      width:  1f,
                      height: total.height)
      {
        xMax = total.xMax
      };
    }

    public static Rect MakeFieldRectLax(in Rect total)
    {
      return new Rect(x:      total.x + LabelWidth + STD_PAD,
                      y:      total.y,
                      width:  total.width - LabelWidth - STD_PAD,
                      height: total.height);
    }


    public static Rect PrefixLabelStrict(in Rect total, GUIContent label, GUIStyle style,
                                            int  id = 0) // pass -1 if not intended for a control
    {
      Rect field = MakePrefixRect(in total);

      bool was_enabled = GUI.enabled;
      GUI.enabled = true;

      if (id < 0)
        GUI.Label(field, label, style);
      else
        EditorGUI.HandlePrefixLabel(total, field, label, id, style);

      GUI.enabled = was_enabled;

      field.x       = total.x + EditorGUIUtility.labelWidth + STD_PAD;
      field.xMax    = total.xMax;
      field.height  = total.height;

      return field;
    }

    public static Rect PrefixLabelStrict(in Rect total, GUIContent label,
                                            int  id = 0) // pass -1 if not intended for a control
    {
      return PrefixLabelStrict(in total, label, Styles.Label, id);
    }



    public static Rect PrefixLabelLax(in Rect total, GUIContent label, GUIStyle style,
                                         int  id = 0) // pass -1 if not intended for a control
    {
      Rect field = MakePrefixRect(in total);

      bool was_enabled = GUI.enabled;
      GUI.enabled = true;

      if (id < 0)
        GUI.Label(field, label, style);
      else
        EditorGUI.HandlePrefixLabel(total, field, label, id, style);

      GUI.enabled = was_enabled;

      field.x       = total.x + LabelWidth + STD_PAD;
      field.xMax    = total.xMax;
      field.height  = total.height;

      return field;
    }

    public static Rect PrefixLabelLax(in Rect total, GUIContent label,
                                         int  id = 0) // pass -1 if not intended for a control
    {
      return PrefixLabelLax(in total, label, Styles.Label, id);
    }


    public static bool FoldoutPrefixLabel(in Rect total, out Rect field, GUIContent label, bool is_expanded, GUIStyle style = null)
    {
      field = MakePrefixRect(in total);

      if (EditorGUI.indentLevel > 0)
      {
        field.xMin -= STD_PAD_HALF + STD_BTN_W;
      }
      else
      {
        field.xMin -= STD_PAD_HALF;
      }

      bool label_toggles = true;

      if (label == null)
      {
        label = GUIContent.none;
        field.width   = STD_BTN_W;
        label_toggles = false;
      }

      is_expanded = EditorGUI.Foldout(field,
                                      is_expanded,
                                      label,
                                      label_toggles,
                                      style ?? Styles.Foldout);

      field.x      = total.x + EditorGUIUtility.labelWidth + STD_PAD;
      field.xMax   = total.xMax;
      field.height = total.height;

      return is_expanded;
    }

    public static bool FoldoutPrefix(in Rect total, out Rect field, bool is_expanded, GUIStyle style = null)
    {
      return FoldoutPrefixLabel(in total, out field, label: null, is_expanded, style);
    }

    public static bool ToggledPrefixLabel(in Rect total, GUIContent label, bool on, out Rect field)
    {
      //total.xMin = LabelStartX;

      field = new Rect(x:      total.x - MIN_TOGGLE_W - STD_PAD + STD_INDENT * EditorGUI.indentLevel,
                       y:      total.y,
                       width:  MIN_TOGGLE_W,
                       height: MIN_TOGGLE_H);

      PushIndentLevel(0, fix_label_width: false);
      on = EditorGUI.Toggle(field, on);
      PopIndentLevel(fix_label_width: false);

      field.x      = field.xMax + STD_PAD;
      field.width  = EditorGUIUtility.labelWidth;
      field.height = STD_LINE_HEIGHT;

      EditorGUI.HandlePrefixLabel(total, field, label, id: 0, Styles.Label);

      //if (on)
      //{
      //  EditorGUI.HandlePrefixLabel(total, field, label, id: 0, Styles.LabelControl);
      //}
      //else
      //{
      //  field.xMin -= STD_INDENT * EditorGUI.indentLevel;
      //  EditorGUI.LabelField(field, label, Styles.LabelControl);
      //}

      field.x      = field.xMax + STD_PAD;
      field.xMax   = FieldEndX;
      field.height = total.height;
      return on;
    }

    public static bool ToggleField(Rect field, string label_str, bool on)
    {
      field.height = STD_LINE_HEIGHT;
      field.xMin -= STD_TOGGLE_W + STD_PAD;
      return EditorGUI.ToggleLeft(field, label_str, on);
    }

    // uses the very last bit in `current` as a flag for enabling/disabling the LayerMask.
    public static LayerMask ToggledLayerMaskField(Rect pos, GUIContent label, LayerMask current)
    {
      bool enabled = ToggledPrefixLabel(pos, label, current.IsEnabled(), out pos);

      if (enabled != current.IsEnabled())
      {
        current = current.Toggled();
      }

      int mask = current.ToLabelMask();

      EditorGUI.BeginDisabledGroup(!enabled);
      int edit = EditorGUI.MaskField(pos, mask, LayerMasks.Labels);
      EditorGUI.EndDisabledGroup();

      if (mask != edit)
      {
        current = LayerMasks.FromLabelMask(edit).SetEnabled(enabled);
      }

      return current;
    }

    public static string ToggledTagField(Rect pos, GUIContent label, string current)
    {
      bool enabled = ToggledPrefixLabel(pos, label, !current.IsEmpty(), out pos);

      if (enabled && current.IsEmpty())
      {
        current = "Untagged";
      }
      else if (!enabled)
      {
        current = string.Empty;
      }

      EditorGUI.BeginDisabledGroup(!enabled);
      current = EditorGUI.TagField(pos, current);
      EditorGUI.EndDisabledGroup();

      return current;
    }


    public static bool ButtonFieldLayout(GUIContent content, GUIStyle style = null)
    {
      if (style == null)
        style = GUI.skin.button;

      float height = style.CalcHeight(content, FieldWidth);

      var pos = GUILayoutUtility.GetRect(height, height, GUILayout.ExpandWidth(true));

      pos.xMin = FieldStartX;

      return GUI.Button(pos, content, style);
    }


    [System.Obsolete("This would would need revisiting. Consider removing entirely.")]
    public static bool ListField(in Rect total, SerializedProperty prop, GUIContent label)
    {
      DrawRect(in total, Colors.Debug.Pending);

      var label_keeper = Labels.Pool.MakePromiseIfNull(ref label);

      if (label.text.IsEmpty())
      {
        label.text = prop.displayName;
      }

      var pos = new Rect(total)
      {
        x       = LabelStartX,
        xMax    = LabelWidth,
        height  = STD_LINE_HEIGHT
      };

      int   count     = prop.arraySize;
      bool  changed   = GUI.changed;
      bool  is_ricky  = prop.arrayElementType == "string" &&
                        !label.text.Contains("No"); // always respect it when they say "No", hun.

      bool was_expanded = prop.isExpanded;

      if (count > 0)
      {
        if (label_keeper.IsStolen)
        {
          prop.isExpanded = EditorGUI.Foldout(pos, prop.isExpanded, label, toggleOnLabelClick: false);
        }
        else // UNTESTED CASE
        {
          prop.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(pos, prop.isExpanded, GUIContent.none);
        }
      }
      else
      {
        prop.isExpanded = false;

        if (label_keeper.IsStolen)
        {
          EditorGUI.LabelField(pos, label);
        }
        else // UNTESTED CASE
        {
          _ = EditorGUI.BeginFoldoutHeaderGroup(pos, false, GUIContent.none);
        }
      }

      //EditorGUI.BeginProperty(total, GUIContent.none, sprop);

      label.tooltip = null;
      label.text    = $"({count})";

      EditorGUI.LabelField(pos, label, Styles.TextDetail);

      pos.x     = FieldStartX;
      pos.xMax  = total.xMax;

      var btn = new Rect(pos);

      bool enabled = !prop.IsReadOnly();
      EditorGUI.BeginDisabledGroup(!enabled);

      if (enabled && (count == 0 || prop.isExpanded))
      {
        btn.width   = pos.width / 2.0f - 1.0f;
        btn.height  = STD_BTN_H;

        if (GUI.Button(btn, "Add Item", Styles.ButtonSmall))
        {
          prop.InsertArrayElementAtIndex(count);
          prop.isExpanded = changed = true;

          if (is_ricky)
          {
            var added = prop.GetArrayElementAtIndex(count);
            added.stringValue = RichText.Remove(s_LoremIpsum[count % s_LoremIpsum.Length]);
          }
        }

        btn.x += btn.width + STD_PAD;

        EditorGUI.BeginDisabledGroup(count == 0);

        if (GUI.Button(btn, "Clear List", Styles.ButtonSmall))
        {
          prop.ClearArray();
          changed = true;
          prop.isExpanded = false;
        }

        EditorGUI.EndDisabledGroup();
      }
      else if (!enabled)
      {
        EditorGUI.LabelField(pos, "[ReadOnly]", Styles.TextDetailCenter);
      }

      if (!prop.isExpanded)
      {
        EditorGUI.EndDisabledGroup();

        if (!label_keeper.IsStolen) // UNTESTED CASE
        {
          EditorGUI.EndFoldoutHeaderGroup();
        }

        label_keeper.Dispose();

        if (was_expanded)
        {
          for (int i = 0; i < count; ++i)
          {
            var child = prop.GetArrayElementAtIndex(i);
            child.isExpanded = false;
          }
        }
        
        return GUI.changed |= changed;
      }
      else if (!was_expanded)
      {
        for (int i = 0; i < count; ++i)
        {
          var child = prop.GetArrayElementAtIndex(i);
          child.isExpanded = true;
        }
      }

      pos.xMin -= ARRAY_INDEX_LABEL_WIDTH + STD_PAD;

      btn.width   = STD_BTN_W;
      btn.height  = STD_BTN_H;

      btn.x = pos.x - STD_BTN_W - STD_PAD;
      btn.y = pos.y += STD_LINE_ADVANCE;

      var die = new Rect(x: STD_INDENT_0, y: pos.y, width: 1, height: STD_LINE_HEIGHT)
      {
        xMax = btn.x - STD_PAD
      };

      PushLabelWidth(ARRAY_INDEX_LABEL_WIDTH);
      PushLabelAlign(TextAnchor.MiddleCenter);
      PushIndentLevel(0);

      EditorGUI.BeginChangeCheck();

      float reses_posx = pos.xMin;

      for (int i = 0; i < count; ++i)
      {
        var child = prop.GetArrayElementAtIndex(i);

        if (enabled)
        {
          if (GUI.Button(btn, "x", EditorStyles.miniButtonMid))
          {
            prop.DeleteArrayElementAtIndex(i);
            changed = true;
            break;
          }

          if (is_ricky && child.stringValue.Length == 0)
          {
            EditorGUI.SelectableLabel(die, s_LoremIpsum[i % s_LoremIpsum.Length], Styles.TextDetailLeft);
          }
        }

        label.text = $"[{i}]";

        if (child.hasVisibleChildren)
        {
          pos.height = STD_LINE_HEIGHT;
          //ReadOnlyTextField(pos, null, child.type);
          die.y = btn.y = pos.y = pos.y + pos.height + STD_PAD;

          if (EditorGUI.PropertyField(pos, child, label, includeChildren: false) &&
              child.NextVisible(true))
          {
            PopLabelWidth();
            PushLabelAlign(TextAnchor.MiddleRight);
            pos.xMin = LabelStartX;

            do
            {
              pos.height = EditorGUI.GetPropertyHeight(child, includeChildren: true);
              _ = EditorGUI.PropertyField(pos, child, includeChildren: true);
              die.y = btn.y = pos.y = pos.y + pos.height + STD_PAD;
            }
            while (child.NextVisible(false));

            pos.xMin = reses_posx;
            PopLabelAlign();
            PushLabelWidth(ARRAY_INDEX_LABEL_WIDTH);
          }
        }
        else
        {
          pos.height = EditorGUI.GetPropertyHeight(child, includeChildren: false);
          _ = EditorGUI.PropertyField(pos, child, label, includeChildren: false);

          die.y = btn.y = pos.y = pos.y + pos.height + STD_PAD;
        }
      }

      changed |= EditorGUI.EndChangeCheck();

      EditorGUI.EndDisabledGroup();

      //EditorGUI.EndProperty();

      if (!label_keeper.IsStolen) // UNTESTED CASE
      {
        EditorGUI.EndFoldoutHeaderGroup();
      }

      label_keeper.Dispose();

      PopLabelWidth();
      PopLabelAlign();
      PopIndentLevel();
      return GUI.changed |= changed;
    }
    [System.Obsolete("This would would need revisiting. Consider removing entirely.")]
    public static bool ListFieldLayout(SerializedProperty sprop)
    {
      using (Labels.Pool.MakePromise(out GUIContent label))
      {
        if (!sprop.isArray)
        {
          label.text = sprop.displayName;
          InvalidFieldLayout(label, "GUIDrawers.ListField requires array type.");
          return false;
        }

        //float height = STD_LINE_ADVANCE;
        //height *= sprop.isExpanded ? sprop.arraySize : 0.0f;
        //height += STD_LINE_HEIGHT;

        float height = STD_LINE_HEIGHT;

        // I know -- disgustingly inefficient to loop twice...
        // ...but this is the only way to support nested structs in lists.
        for (int i = 0; i < sprop.arraySize; ++i)
        {
          var element = sprop.GetArrayElementAtIndex(i);
          height += EditorGUI.GetPropertyHeight(element, true) + STD_PAD;
        }

        return ListField(EditorGUILayout.GetControlRect(hasLabel: true, height), sprop, label);
      }
    }


    public static bool DelayedStringField(in Rect pos, SerializedProperty prop, GUIStyle field_style = null)
    {
      EditorGUI.BeginChangeCheck();
      string edit = EditorGUI.DelayedTextField(pos, GUIContent.none, prop.stringValue, field_style ?? Styles.TextField);
      if (EditorGUI.EndChangeCheck())
      {
        prop.stringValue = edit;
        return true;
      }

      return false;
    }

    public static bool DelayedStringField(in Rect total, GUIContent label, ref string current, GUIStyle field_style = null)
    {
      EditorGUI.BeginChangeCheck();
      string edit = EditorGUI.DelayedTextField(total, label, current, field_style ?? Styles.TextField);
      if (EditorGUI.EndChangeCheck())
      {
        current = edit;
        return true;
      }

      return false;
    }

    public static int DelayedMultiFloatField(Rect pos, GUIContent[] labels, GUIStyle label_style, float[] curr_vals, int idx, out bool commit)
    {
      commit = false;

      if (curr_vals == null || curr_vals.Length == 0)
      {
        InvalidField(pos, "WARNING: null / empty value array!");
        return -1;
      }

      if (label_style == null)
        label_style = Styles.NumberInfo;

      PushLabelWidth(EditorGUIUtility.labelWidth);
      PushIndentLevel(0, fix_label_width: false);

      int curr_ctrl = GUIUtility.keyboardControl;
      if (curr_ctrl == 0) curr_ctrl = GUIUtility.hotControl;

      int prev_idx  = idx;
          idx       = -1;

      int len         = curr_vals.Length;
      int len_labels  = labels != null ? labels.Length : 0;

      float advance = pos.width = (pos.width - STD_PAD * (len - 1)) / len;
      advance += STD_PAD;

      bool ctx_click = Event.current.type == EventType.ContextClick;

      EditorGUI.BeginChangeCheck();

      int         this_ctrl;
      bool        is_bad_val;
      GUIContent  label;
      for (int i = 0; i < len; ++i, pos.x += advance)
      {
        if (i < len_labels)
        {
          label = labels[i];
          EditorGUIUtility.labelWidth = label_style.CalcWidth(label);
        }
        else
        {
          label = GUIContent.none;
          EditorGUIUtility.labelWidth = ARRAY_INDEX_LABEL_WIDTH + STD_PAD;
        }

        is_bad_val = curr_vals[i].IsNaN();

        if (is_bad_val && label.tooltip.IsEmpty())
        {
          label.tooltip = "[RMB] - Potential Fixes";
        }

        EditorGUI.BeginDisabledGroup(is_bad_val);

        this_ctrl     = GUIUtility.GetControlID(s_HashTextField, FocusType.Keyboard, pos) + 1;
        _             = PrefixLabelLax(in pos, label, label_style, this_ctrl);
        curr_vals[i]  = EditorGUI.FloatField(pos, Labels.NonEmpty, curr_vals[i], Styles.NumberField);

        EditorGUI.EndDisabledGroup();

        if (is_bad_val)
        {
          if (ctx_click && pos.Contains(Event.current.mousePosition))
          {
            SimpleFixContextMenu(i);
            ctx_click = false;
            Event.current.Use();

            if (-1 < prev_idx)
              idx = i;
            else
              commit = true;
          }
          else if ((prev_idx < 0 || prev_idx == i) && ConsumeSimpleFix(i))
          {
            curr_vals[i] = 0.0f;
            idx = i;
          }

          continue;
        }

        if (idx != -1)
          continue;

        if (this_ctrl == curr_ctrl)
        {
          if (prev_idx < 0 || i == prev_idx)
          {
            idx = i;
          }
          else
          {
            idx = prev_idx;
            commit = true;
          }
        }
      } // end loop

      // cleanup:
      PopIndentLevel(fix_label_width: false);
      PopLabelWidth();

      // fixup the logic:
      if (EditorGUI.EndChangeCheck() && !EditorGUIUtility.editingTextField)
      {
        commit = true;
      }
      else if (idx < 0)
      {
        if (idx < prev_idx)
        {
          idx = prev_idx;
          commit = true;
        }
      }
      else if (Event.current.isKey)
      {
        switch (Event.current.keyCode)
        {
          case KeyCode.Return:
          case KeyCode.KeypadEnter:
            commit = true;
            break;

          case KeyCode.Escape:
            idx     = -1;
            commit  = true;
            break;
        }
      }

      //$"commit? : {commit} ; idx? : {idx}".Log();

      GUI.changed |= commit;
      return idx;
    }

    private static int s_SimpleFixIdx = -1;
    public static void SimpleFixContextMenu(int idx)
    {
      var menu = new GenericMenu();

      using (Labels.Pool.MakePromise(out GUIContent label))
      {
        label.text = "Fix";

        menu.AddItem(content: label, on: false, func: () =>
        {
          s_SimpleFixIdx = idx;
        });

        menu.ShowAsContext();
      }
    }
    public static bool ConsumeSimpleFix(int idx)
    {
      if (s_SimpleFixIdx == idx)
      {
        s_SimpleFixIdx = ~idx;
        return true;
      }

      return false;
    }


    public static bool LongNumberField(in Rect pos, SerializedProperty sprop, bool is_bad = false)
    {
      GUIStyle style;
      if (is_bad)
      {
        style = Styles.NumberFieldBad;

        var rect = new Rect(pos);
        rect.Expand(STD_PAD);
        DrawRect(in rect, Colors.Debug.Attention, Colors.Debug.Warning.Alpha(0.35f));
      }
      else
      {
        style = Styles.NumberField;
      }

      EditorGUI.BeginChangeCheck();
      long edit = EditorGUI.LongField(pos, sprop.longValue, style);
      if (EditorGUI.EndChangeCheck())
      {
        sprop.longValue = edit;
        return true;
      }

      return false;
    }


    public static void DrawRect(in Rect pos, Color32 outline, Color32 fill = default)
    {
      //if (Event.current.type != EventType.Repaint)
      //  return;

      Handles.BeginGUI();

      var push_clr = Handles.color;
      Handles.color = Color.white;

      Handles.DrawSolidRectangleWithOutline(pos, fill, outline);

      Handles.color = push_clr;
      Handles.EndGUI();
    }

    public static void DrawFieldBackground(in Rect total)
    {
      //total.x -= STD_PAD;
      //total.width += STD_PAD_RIGHT;

      DrawRect(in total, Colors.GUI.FieldOutline, Colors.GUI.FieldBG);
    }

    public static void LayoutSeparator(float yoffset = STD_LINE_HALF)
    {
      // an ACTUAL separator that you can SEE...
      Rect pos = GUILayoutUtility.GetRect(ContentWidth, yoffset * 2f);
      
      float y = pos.y + yoffset;

      if (EditorGUI.indentLevel > 0)
      {
        DrawSingleLine(p0: new Vector2(pos.xMin, y),
                       p1: new Vector2(pos.xMax, y),
                       color: Colors.Grey);
      }
      else
      {
        DrawSingleLine(p0: new Vector2(pos.xMin - STD_INDENT_0 + STD_PAD, y),
                       p1: new Vector2(pos.xMax, y),
                       color: Colors.Grey);
      }
    }

    public static void DrawSingleLine(Vector2 p0, Vector2 p1, Color32 color)
    {
      if (Event.current.type != EventType.Repaint)
        return;

      Handles.BeginGUI();
      var push_clr = Handles.color;
      Handles.color = color;

      Handles.DrawLine(p0, p1);

      Handles.color = push_clr;
      Handles.EndGUI();
    }

    public static void DrawTopLine(in Rect pos, Color32 color)
    {
      DrawSingleLine(new Vector2(pos.x, pos.y), new Vector2(pos.xMax, pos.y), color);
    }

    public static void DrawBottomLine(in Rect pos, Color32 color)
    {
      DrawSingleLine(new Vector2(pos.x, pos.yMax), new Vector2(pos.xMax, pos.yMax), color);
    }

    public static void DrawFillBar(in Rect pos, Color32 fill, float t, Color32 text_color = default)
    {
      if (Event.current.type != EventType.Repaint)
        return;

      DrawRect(in pos, outline: Colors.Black);

      var fill_rect = new Rect(pos.x, pos.y, pos.width * t, pos.height);

      DrawRect(in fill_rect, outline: Colors.Black, fill.Alpha(0x88));

      if (text_color.a > 0x00)
      {
        string label_pct = Mathf.RoundToInt(t * 100).ToString() + '%';
        label_pct = RichText.Make(label_pct,
                                  RichText.Style.Large | RichText.Style.Bold,
                                  text_color);

        GUI.Label(pos, label_pct, Styles.LabelCenter);
      }
    }

  }

}