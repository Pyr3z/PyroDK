/**
@file   PyroDK/Core/Editor/GUIDrawers/Color32Drawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-16

@brief
  Overrides the default drawer for `UnityEngine.Color32` field types,
  adding the additional capability to specify the color in hexadecimal.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class GUIDrawers
  {
    private static readonly float COLOR32_STRING_FIELD_WIDTH = Styles.NumberField.CalcWidth("0xFFFFFFFF") + STD_PAD_RIGHT;

    public static Color32 Color32Field(Rect pos, Color32 curr_clr, ref string curr_str, ref GUIStyle curr_style)
    {
      PushIndentLevel(0, fix_label_width: false);

      if (curr_str == null)
      {
        curr_str = curr_clr.ToHex("#");
        curr_style = Styles.NumberField;
      }
      else if (curr_style == null)
      {
        if (Colors.TryReparseHex(curr_str, ref curr_clr))
          curr_style = Styles.NumberField;
        else
          curr_style = Styles.NumberFieldBad;
      }

      EditorGUI.BeginChangeCheck();

      float xmax = pos.xMax;
      pos.width = COLOR32_STRING_FIELD_WIDTH;
      
      curr_str = EditorGUI.TextField(pos, curr_str, curr_style);

      bool changed = EditorGUI.EndChangeCheck();
      if (changed)
      {
        if (Colors.TryReparseHex(curr_str, ref curr_clr))
          curr_style = Styles.NumberField;
        else
          curr_style = Styles.NumberFieldBad;
      }
      else
      {
        EditorGUI.BeginChangeCheck();
      }

      pos.x += pos.width;
      pos.xMax = xmax;

      curr_clr = EditorGUI.ColorField(pos, curr_clr);

      if (!changed && EditorGUI.EndChangeCheck())
      {
        curr_str    = curr_clr.ToHex(prev: curr_str);
        curr_style  = Styles.NumberField;
        changed     = true;
      }

      GUI.changed |= changed;

      PopIndentLevel(fix_label_width: false);
      return curr_clr;
    }


    [CustomPropertyDrawer(typeof(Color32))]
    private sealed class Color32Drawer : PropertyDrawer
    {

      private Color32   m_CurrClr   = Color.white;
      private string    m_CurrStr   = null;
      private GUIStyle  m_CurrStyle = null;

      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        var pos = PrefixLabelStrict(in total, label);

        var edit = Color32Field(pos, m_CurrClr, ref m_CurrStr, ref m_CurrStyle);

        if (!CheckRestringify(prop.colorValue) && CheckRestringify(edit))
        {
          prop.colorValue = edit;
        }
      }


      private bool CheckRestringify(Color32 next_clr)
      {
        if (!Colors.Comparator.Equals(m_CurrClr, next_clr))
        {
          m_CurrClr   = next_clr;
          m_CurrStr   = next_clr.ToHex(prev: "#");
          m_CurrStyle = Styles.NumberField;
          return true;
        }

        return false;
      }

    }

  }

}