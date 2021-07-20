/**
@file   PyroDK/Core/AssetTypes/GUIDrawers/PathDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-08-20

@brief
  Draws `PyroDK.Filesystem.Path` and derived properties.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class GUIDrawers
  {


    private static readonly string s_PathExistsSuffix  = RichText.Small("(exists)");
    private static readonly string s_PathEmptySuffix   = RichText.Small("(empty)");
    private static readonly string s_PathInvalidSuffix = RichText.Color("(INVALID)", Colors.Debug.Attention);

    public static bool PathField(Rect pos, GUIContent label, FilePath path)
    {
      bool read_only = path.EditorFlags.HasFlag(FilePathFlags.ReadOnly);

      if (read_only)
        Labels.Button.text = "View..";
      else
        Labels.Button.text = "Edit..";

      float btn_width = Styles.ButtonSmall.CalcWidth(Labels.Button) + STD_PAD_RIGHT;

      bool   exists = path.RecheckExists();
      string edit   = path;

      if (Styles.Label.CalcWidth(label) + Styles.LabelDetail.CalcWidth(s_PathInvalidSuffix) < LabelWidth + STD_INDENT_0)
      {
        var suff = new Rect(pos)
        {
          xMax = LabelEndX
        };

        if (exists)
          GUI.Label(suff, s_PathExistsSuffix, Styles.LabelDetail);
        else if (path.IsEmpty())
          GUI.Label(suff, s_PathEmptySuffix, Styles.LabelDetail);
        else if (!path.IsValid)
          GUI.Label(suff, s_PathInvalidSuffix, Styles.LabelDetail);
      }

      pos = PrefixLabelLax(pos, label);

      pos.width -= btn_width + STD_PAD;

      if (read_only)
      {
        GUIStyle style;
        if (exists)
          style = Styles.PathInfoExists;
        else if (path.IsValid)
          style = Styles.PathInfo;
        else
          style = Styles.PathInfoInvalid;

        InfoField(pos, edit, style);
      }
      else
      {
        GUIStyle style;
        if (exists)
          style = Styles.PathFieldExists;
        else if (path.IsValid)
          style = Styles.PathField;
        else
          style = Styles.PathFieldInvalid;

        EditorGUI.BeginChangeCheck();

        edit = EditorGUI.DelayedTextField(pos, edit, style);
      }
      
      bool edited = !read_only && EditorGUI.EndChangeCheck();

      if (btn_width < 1f)
        return edited && path.SetLax(edit);

      pos.x     = pos.xMax + STD_PAD;
      pos.width = btn_width;

      if (!edited && GUI.Button(pos, Labels.Button, Styles.ButtonSmall))
      {
        if (read_only)
        {
          if (exists)
            EditorUtility.RevealInFinder(path);
          else
            EditorUtility.RevealInFinder(path.GetParentPath());
        }
        else
        {
          if (path.EditorFlags.HasFlag(FilePathFlags.DirsOnly))
          {
            edit = EditorUtility.SaveFolderPanel(title:       label.text,
                                                 folder:      path.GetParentPath(),
                                                 defaultName: path.GetFilename());
          }
          else if (path.IsAsset)
          {
            edit = EditorUtility.SaveFilePanelInProject(title:       label.text,
                                                        defaultName: path.GetFilename(),
                                                        extension:   path.GetExtension(),
                                                        message:     "Select Asset Path",
                                                        path:        path);
          }
          else
          {
            edit = EditorUtility.SaveFilePanel(title:       label.text,
                                               directory:   path.GetParentPath(),
                                               defaultName: path.GetFilename(),
                                               extension:   path.GetExtension());
          }

          return path.SetLax(edit);
        }
      }

      return edited && path.SetLax(edit);
    }



    [CustomPropertyDrawer(typeof(FilePath), true)]
    private sealed class PathDrawer : PropertyDrawer
    {
      private FilePath m_PathClone = new FilePath();


      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        var prop_flags = prop.Copy();
        prop_flags.NextVisible(true);

        var prop_path = prop_flags.Copy();
        prop_path.NextVisible(false);

        bool was_enabled = GUI.enabled;
        if (was_enabled)
          m_PathClone.SetRaw((FilePathFlags)prop_flags.intValue, prop_path.stringValue);
        else
          m_PathClone.SetRaw((FilePathFlags)prop_flags.intValue | FilePathFlags.ReadOnly, prop_path.stringValue);

        label.tooltip = $"{m_PathClone.RuntimeFlags}\n[{m_PathClone.EditorFlags}]";

        GUI.enabled = true;
        if (PathField(total, label, m_PathClone))
        {
          prop_flags.intValue   = m_PathClone.RawFlags;
          prop_path.stringValue = m_PathClone.RawString;
          prop.serializedObject.ApplyModifiedProperties();
        }
        GUI.enabled = was_enabled;
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        return STD_LINE_HEIGHT;
      }

    }

  }

}