/**
@file   PyroDK/Core/Editor/Inspectors.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-05

@brief
  Override the default inspector editor behaviour.
**/

using System.Collections;

using UnityEngine;

using UnityEditor;
using UnityEditorInternal;


namespace PyroDK.Editor
{

  using Type      = System.Type;
  using FieldInfo = System.Reflection.FieldInfo;


  public static partial class Inspectors
  {
    [SerializeStatic]
    public static bool ENABLE_EXPERIMENTAL_DRAWERS = false;

    // InjectedDrawer functions should return true if they should replace
    // instead of precede the default drawing behaviour.
    public delegate bool InjectedDrawer(SerializedProperty prop_it);


    private static readonly string PATH_UNAVAILABLE = RichText.Color("——/", Colors.Debug.Warning);

    private static Object s_PathCheckedObject = null;
    private static string s_CurrentObjectPath = null;
    private static string s_CurrentObjectName = null;


    public abstract class BaseInspector : UnityEditor.Editor
    {

      protected override void OnHeaderGUI() // only shows for Assets
      {
        EditorGUILayout.BeginHorizontal(Styles.TitleBox);
        EditorGUILayout.BeginVertical(Styles.Section);

        string prepath = PATH_UNAVAILABLE;
        string type    = $"<{target.GetType().GetRichLogName()}>";

        if (!s_CurrentObjectPath.IsEmpty())
        {
          int slash = s_CurrentObjectPath.LastIndexOf('/');

          if (slash > 0)
            prepath = s_CurrentObjectPath.Remove(++slash);
          else
            prepath = s_CurrentObjectPath;
        }

        float type_w = Styles.TitleTextSmall.CalcWidth(type);
        float line   = Styles.TitleTextSmall.lineHeight;

        var total = GUILayoutUtility.GetRect(width: line,
                                            height: line + Styles.TitleText.lineHeight + GUIDrawers.STD_LINE_HEIGHT + 6f,
                                                    GUILayout.ExpandWidth(true));

        // Upper-left: Parent directory path
        var rect = new Rect(total.x, total.y, total.width, line);
        GUI.Label(rect, prepath, Styles.TitleTextSmall);

        // Middle-center: Asset's identifier
        rect.y += rect.height + 2f;
        rect.height = Styles.TitleText.lineHeight;
        //rect.width = name_w;
        //rect.x = total.x + (total.width - name_w) / 2f;
        GUI.Label(rect, s_CurrentObjectName, Styles.TitleText);

        // Bottom-right: Asset's type
        rect.y += rect.height + 2f;
        rect.height = GUIDrawers.STD_LINE_HEIGHT;
        rect.xMin = total.xMax - type_w;
        rect.xMax = total.xMax;
        GUI.Label(rect, type, Styles.TitleTextSmall);

        // Bottom-left: Preloaded asset toggle
        rect.x = total.xMin;
        rect.xMax = total.xMax - type_w;
        PreloadedAssetToggle(rect);

        // Any click opens the context menu:
        if (GUIDrawers.IsAnyClick(total.Expanded(3f), Colors.Clear, Colors.GUI.Comment))
        {
          HeaderContextMenu()
            .ShowAsContext();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
      }

      private GenericMenu HeaderContextMenu()
      {
        using (var labels = Labels.Borrow("Copy Identifier",
                                          "Show Asset in Project Window",
                                          "Goto Script Asset"))
        {
          var menu = new GenericMenu();

          menu.AddItem(labels[0], on: false, () =>
          {
            GUIUtility.systemCopyBuffer = s_CurrentObjectPath;
          });

          menu.AddItem(labels[1], on: false, () =>
          {
            //ProjectWindowUtil.ShowCreatedAsset(target);
            EditorGUIUtility.PingObject(target);
          });

          menu.AddItem(labels[2], on: false, () =>
          {
            var script = serializedObject.FindProperty("m_Script").objectReferenceValue;
            //ProjectWindowUtil.ShowCreatedAsset(script);
            EditorGUIUtility.PingObject(script);
          });

          return menu;
        }
      }

      private void PreloadedAssetToggle(Rect rect)
      {
        const string LABEL = "Is Pre-Loaded Asset";

        if (!AssetObjects.IsMainAsset(target))
          return;

        rect.width = Styles.Label.CalcWidth(LABEL) + 2f + GUIDrawers.STD_TOGGLE_W;

        bool is_preloaded = AssetObjects.IsPreloadedAsset(target);
        bool edit = EditorGUI.ToggleLeft(rect, LABEL, is_preloaded, Styles.Label);

        if (is_preloaded != edit)
        {
          Logging.Assert(AssetObjects.TrySetPreloadedAsset(target, edit));
        }
      }

    } // end class BaseInspector


    public static void DrawPyroInspector(SerializedObject sobj, InjectedDrawer inj = null)
    {
      sobj.Update();

      var curr_prop = sobj.GetIterator();

      // handle implied "m_Script" that we KNOW is the first serialized property:
      if (Logging.Assert(curr_prop.NextVisible(true) && curr_prop.propertyPath == "m_Script"))
        return;

      var script = curr_prop.objectReferenceValue;

      #if USE_DEPRECATED_ADDRESSES
      // handle "m_ObjectAddress" that all IObjects will have:
      if (Logging.Assert(curr_prop.NextVisible(false) && curr_prop.propertyPath == "m_ObjectAddress"))
        return;
      #endif // USE_DEPRECATED_ADDRESSES

      CheckUpdatedObjectPaths(curr_prop);

      var type = sobj.targetObject.GetType();

      // For Components, display the path as a sub-header
      if (sobj.targetObject is BaseComponent)
      {
        var style_path = Styles.TextInfoSmall;
        var style_namespace = Styles.TextDetail;

        using (var labels = Labels.Borrow(s_CurrentObjectPath, type.Namespace))
        using (new EditorGUILayout.VerticalScope(Styles.Section))
        {
          var path_size = new Vector2(GUIDrawers.FieldEndX - 4f, 0f);
          path_size.y = style_path.CalcHeight(labels[0], path_size.x);

          if (path_size.y > style_path.lineHeight)
          {
            // handle tiny tiny inspectors
            string short_path = labels[0].text;

            int bar = 1 + short_path.IndexOf('|');
            if (bar > 0 && bar < short_path.Length - 1)
              short_path = short_path.Substring(bar);

            if (short_path.StartsWith("</"))
            {
              bar = 1 + short_path.IndexOf('>');
              if (bar > 0 && bar < short_path.Length - 1)
                short_path = short_path.Substring(bar);
            }

            labels[0].text = short_path;
            path_size.y = style_path.CalcHeight(labels[0], path_size.x);
          }

          path_size.x = style_path.CalcWidth(labels[0]);

          float total_height = 4f + path_size.y;

          // adjust the height we ask for, to account for the namespace label
          bool draw_namespace = !labels[1].text.IsEmpty();
          if (draw_namespace && path_size.x + style_namespace.CalcWidth(type.Namespace) > GUIDrawers.ContentWidth)
            total_height += style_namespace.lineHeight;
          else
            total_height += 5f; // kinda magic

          var total = GUILayoutUtility.GetRect(total_height, total_height, GUILayout.ExpandWidth(true));
          total.xMin = 3f;

          var rect = new Rect(total.x,
                              total.y,
                              total.width,
                              path_size.y);
          GUI.Label(rect, labels[0], style_path);

          if (draw_namespace)
          {
            if (total_height > 4f + path_size.y)
            {
              // two lines
              rect.yMax = total.yMax;
              rect.yMin = total.yMax - style_namespace.lineHeight - 4f;
              GUI.Label(rect, labels[1], style_namespace);
            }
            else
            {
              // one line
              rect.xMin += path_size.x + 2f;
              GUI.Label(rect, labels[1], style_namespace);
            }
          }

          if (GUIDrawers.IsAnyClick(total.Expanded(2.5f), Colors.Clear, Colors.GUI.Comment))
          {
            labels[0].text = "Copy Identifier";
            labels[1].text = "Goto Script Asset";

            var menu = new GenericMenu();

            menu.AddItem(labels[0], on: false, () =>
            {
              GUIUtility.systemCopyBuffer = RichText.RemoveSoberly(s_CurrentObjectPath);
            });

            menu.AddItem(labels[1], on: false, () =>
            {
              //ProjectWindowUtil.ShowCreatedAsset(script);
              EditorGUIUtility.PingObject(script);
            });

            menu.ShowAsContext();
          }
        } // end using scope
      }

      // move on to the rest of the properties:
      using (new LocalizationGroup(type))
      {
        if (inj == null || !inj(curr_prop))
        {
          PropertyIterator(curr_prop);
        }
      }

      _ = sobj.ApplyModifiedProperties();
    }

    private static void CheckUpdatedObjectPaths(SerializedProperty prop)
    {
      var curr_obj = prop.serializedObject.targetObject;

      if (curr_obj == s_PathCheckedObject && curr_obj.name == s_CurrentObjectName)
        return;

      s_PathCheckedObject = curr_obj;
      s_CurrentObjectName = curr_obj.name;
      s_CurrentObjectPath = PATH_UNAVAILABLE;

      if (s_CurrentObjectName.IsEmpty())
        return;

      string obj_path = AssetObjects.MakeGenericPath(curr_obj);

      #if USE_DEPRECATED_ADDRESSES
      if (!(curr_obj is IObject iobj))
      {
        Logging.ShouldNotReach(blame: curr_obj);
        return;
      }

      if (iobj.UpdateAddress(obj_path))
      {
        curr_obj.MakeDirtyNoUndo();
      }
      #endif // USE_DEPRECATED_ADDRESSES

      if (curr_obj is BaseComponent) // Color it with rich text here
      {
        int bar = obj_path.IndexOf('|') + 1;
        if (bar > 0)
        {
          string scene = RichText.Color(obj_path.Remove(bar), Colors.GUI.TypeByVal);
          obj_path = scene + obj_path.Substring(bar);
        }

        int brack = obj_path.LastIndexOf('<');
        if (brack > 0)
        {
          string type = RichText.Color(obj_path.Substring(brack), Colors.GUI.TypeByRef);
          obj_path = obj_path.Remove(brack) + type;
        }
      }

      s_CurrentObjectPath = obj_path;
    }

    // returns true if there are no more properties left to iterate over.
    private static bool PropertyIterator(SerializedProperty curr_prop, string terminal_prop = null)
    {
      try
      {
        int start_indent = EditorGUI.indentLevel;
        bool drill = false;
        while (curr_prop.NextVisible(drill))
        {
          EditorGUI.indentLevel = start_indent + curr_prop.depth;

          #if UNITY_2020_2_OR_NEWER  // fix reorderable lists!

          if (curr_prop.propertyType != SerializedPropertyType.String && curr_prop.isArray)
          {
            if (curr_prop.IsReorderableList())
            {
              drill = EditorGUILayout.PropertyField(curr_prop, includeChildren: true);

              if (curr_prop.IsReadOnly())
              {
                SetReorderableListFrozen(curr_prop, true);
              }
            }
            else
            {
              drill = EditorGUILayout.PropertyField(curr_prop, includeChildren: false);

              if (drill)
              {
                var prop_array = curr_prop.Copy();
                curr_prop.NextVisible(true);
                DoArrayHeader(curr_prop, prop_array);
                drill = false;
              }
              else
              {
                // still draw when closed
                DoArrayHeader(curr_prop.FindPropertyRelative("Array.size"), curr_prop);
              }
            }
          }
          else
          {
            drill = EditorGUILayout.PropertyField(curr_prop, includeChildren: false);
          }

          #else  // DEPRECATED in Unity 2020: (meh.)
          if (curr_prop.isArray && curr_prop.propertyType != SerializedPropertyType.String)
          {
            if (ENABLE_EXPERIMENTAL_DRAWERS)
            {
              _ = GUIDrawers.ListFieldLayout(curr_prop);
            }
            else
            {
              drill = EditorGUILayout.PropertyField(curr_prop, includeChildren: false,
                                                    GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
              if (drill)
              {
                var prop_array = curr_prop.Copy();
                curr_prop.NextVisible(true);
                DoArrayHeader(curr_prop, prop_array);
                drill = false;
              }
              else
              {
                // still draw when closed
                DoArrayHeader(curr_prop.FindPropertyRelative("Array.size"), curr_prop);
              }
            }
          }
          else
          {
            // default case
            drill = EditorGUILayout.PropertyField(curr_prop, includeChildren: false);
          }
          #endif // UNITY_2020_2_OR_NEWER

          if (curr_prop.name == terminal_prop)
            return false;
        }

        return true;
      }
      finally
      {
        GUIDrawers.ResetLabelWidth();
        GUIDrawers.ResetIndentLevel();
      }
    }


    // Underlying key/value : <string, UnityEditorInternal.ReorderableListWrapper>
    private static IDictionary s_ReorderableListLookup;

    private static FieldInfo   s_ReorderableListWrapper_Reorderable;
    private static FieldInfo   s_ReorderableListWrapper_ReorderableList;
    
    private static bool FetchReorderableListLookup(out IDictionary lookup)
    {
      lookup = s_ReorderableListLookup;
      
      if (lookup != null)
        return true;

      if (Assemblies.FindType("UnityEditor.PropertyHandler", out Type t_prophand) &&
          t_prophand.TryGetInternalField("s_reorderableLists", out FieldInfo f_reolists) &&
          Assemblies.FindType("UnityEditorInternal.ReorderableListWrapper", out Type t_reolistwrapper) &&
          t_reolistwrapper.TryGetInternalField("m_Reorderable", out s_ReorderableListWrapper_Reorderable) &&
          t_reolistwrapper.TryGetInternalField("m_ReorderableList", out s_ReorderableListWrapper_ReorderableList))
      {
        lookup = s_ReorderableListLookup = (IDictionary)f_reolists.GetValue(null);
      }

      return lookup != null;
    }

    private static void SetReorderableListFrozen(SerializedProperty prop_list, bool freeze)
    {
      if (!FetchReorderableListLookup(out IDictionary lookup))
        return;

      string prop_id = prop_list.GetPropertyUID();
      object reolistwrapper = lookup[prop_id];

      if (reolistwrapper == null)
        return;

      s_ReorderableListWrapper_Reorderable.SetValue(reolistwrapper, !freeze);

      var list = (ReorderableList)s_ReorderableListWrapper_ReorderableList.GetValue(reolistwrapper);
      list.displayAdd = list.displayRemove = !freeze;
    }


    // returns true if there are NO more properties to draw after this.
    private static bool DoFoldoutSection(SerializedProperty curr_prop, string heading, string terminal_prop)
    {
      FoldoutHeader.OpenLayout(heading, curr_prop, out FoldoutHeader header, indent: true);
      try
      {
        if (header)
        {
          if (PropertyIterator(curr_prop, terminal_prop))
            curr_prop = null;
        }
        else if (!curr_prop.NextVisible(false))
        {
          curr_prop = null;
        }

        return curr_prop == null || !curr_prop.Copy().NextVisible(false);
      }
      finally
      {
        var last = GUILayoutUtility.GetLastRect();
        header.Rect.yMax = last.yMax;
        header.Dispose();
      }
    }


    private static void DoArrayHeader(SerializedProperty prop_count, SerializedProperty prop_array)
    {
      var pos  = GUILayoutUtility.GetLastRect();
      pos.yMin = pos.yMax - GUIDrawers.STD_LINE_HEIGHT;

      if (prop_array.IsReadOnly())
      {
        pos.x += GUIDrawers.LabelWidthHalf;
        pos.xMax = GUIDrawers.LabelEndX;
        GUIDrawers.InfoField(pos:   pos,
                                     text:  RichText.Color("[ReadOnly]", Colors.Gray),
                                     style: Styles.TextDetail);

        pos.x    = GUIDrawers.FieldStartX;
        pos.xMax = GUIDrawers.FieldEndX;
        GUIDrawers.InfoField(pos:   pos,
                                     text:  RichText.Value("Count: ", prop_count.intValue),
                                     style: Styles.TextDetailLeft);
      }
      else
      {
        pos.x     = GUIDrawers.FieldStartX;
        pos.width = GUIDrawers.FieldWidth * 0.45f;

        if (prop_array.isExpanded)
        {
          Labels.Button.text = "Add";
          if (GUI.Button(pos, Labels.Button))
          {
            prop_array.InsertArrayElementAtIndex(prop_array.arraySize);
          }
        }

        pos.x += pos.width + GUIDrawers.STD_PAD;
        pos.xMax = GUIDrawers.FieldEndX;

        Labels.Scratch.text = "Count: ";
        
        GUIDrawers.PushIndentLevel(0, fix_label_width: false);
        GUIDrawers.PushLabelWidth(EditorStyles.label.CalcWidth(Labels.Scratch) + GUIDrawers.STD_PAD_RIGHT);

        _ = EditorGUI.PropertyField(pos, prop_count, Labels.Scratch, includeChildren: false);

        GUIDrawers.PopLabelWidth();
        GUIDrawers.PopIndentLevel(fix_label_width: false);
      }
    }

  }

}