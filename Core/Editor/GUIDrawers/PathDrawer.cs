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
        return edited && path.Set(edit);

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
            edit = EditorUtility.SaveFolderPanel( title:        label.text,
                                                  folder:       path.GetParentPath(),
                                                  defaultName:  path.GetFilename());
          }
          else if (path.IsAsset)
          {
            edit = EditorUtility.SaveFilePanelInProject(title:        label.text,
                                                        defaultName:  path.GetFilename(),
                                                        extension:    path.GetExtension(),
                                                        message:      "Select Asset Path",
                                                        path:         path);
          }
          else
          {
            edit = EditorUtility.SaveFilePanel( title:        label.text,
                                                directory:    path.GetParentPath(),
                                                defaultName:  path.GetFilename(),
                                                extension:    path.GetExtension());
          }

          return path.Set(edit);
        }
      }

      return edited && path.Set(edit);
    }



    [CustomPropertyDrawer(typeof(FilePath), true)]
    private sealed class PathDrawer : PropertyDrawer
    {

      private FilePath m_PathClone = null;


      public override void OnGUI(Rect pos, SerializedProperty sprop, GUIContent label)
      {
        if (m_PathClone == null)
        {
          InvalidField(pos, "Failed to get underlying Path field!", label);
          return;
        }
        
        label.tooltip = m_PathClone.RuntimeFlags.ToString();

        if (PathField(pos, label, m_PathClone))
        {
          sprop.FindPropertyRelative("m_Flags").intValue    = m_PathClone.RawFlags;
          sprop.FindPropertyRelative("m_Path").stringValue  = m_PathClone.RawString;
          sprop.serializedObject.ApplyModifiedProperties();
        }
      }

      public override float GetPropertyHeight(SerializedProperty sprop, GUIContent label)
      {
        if (this.TryGetUnderlyingValue(sprop, out FilePath path))
        {
          if (path.EditorFlags.HasFlag(FilePathFlags.ReadOnly))
          {
            m_PathClone = path;
          }
          else
          {
            path.CloneTo(ref m_PathClone);
          }
        }

        return STD_LINE_HEIGHT;
      }

    }

  }

}