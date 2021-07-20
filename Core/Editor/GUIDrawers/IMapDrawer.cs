/**
@file   PyroDK/Core/Editor/GUIDrawers/IMapDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-06

@brief
  Draws ILookups (hashtables where all keys are strings).
**/

using System.Collections.Generic;

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class GUIDrawers
  {

    [CustomPropertyDrawer(typeof(IMap), useForChildren: true)]
    private sealed class IMapDrawer : PropertyDrawer
    {
      private const float N_LINES_STRING_VALUE  = 2.85f;
      private const int   CACHE_GUI_THRESHOLD   = 32;


      private IMap                m_Map         = null;
      private SerializedProperty  m_Pairs       = null;
      private SerialType          m_StrictRefType  = SerialType.Invalid;

      private string  m_MapTypeName = null;
      private float   m_LabelWidth  = 22f;
      private float   m_ValueHeight = STD_LINE_HEIGHT;

      private HashSet<object> m_DupeSet   = new HashSet<object>();
      private HashSet<object> m_BadKeySet = new HashSet<object>();


      public override bool CanCacheInspectorGUI(SerializedProperty prop)
      {
        return prop.propertyType == SerializedPropertyType.ObjectReference ||
             ( m_Pairs != null && m_Pairs.arraySize > CACHE_GUI_THRESHOLD );
      }

      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        if (prop.propertyType == SerializedPropertyType.ObjectReference)
        {
          // potentially temporary case here
          EditorGUI.ObjectField(total, prop, label);
          return;
        }

        if (m_Map == null || m_Pairs == null)
        {
          InvalidField(in total, label, "Failed to get underlying IMap or its pairs.");
          return;
        }

        total.xMin -= STD_INDENT;

        DrawRect(pos:     new Rect(total.x - 2f, total.y, total.width + 6f, total.height),
                 outline: Colors.GUI.HashMapOutline,
                 fill:    Colors.GUI.HashMapBG);

        var rect_key = new Rect(total.x, total.y, total.width, STD_LINE_HEIGHT);

        GUI.Label(rect_key, $"count:{m_Map.Count} | capacity:{m_Map.Capacity}", Styles.TextDetail);

        label.text += m_MapTypeName;
        prop.isExpanded = FoldoutPrefixLabel(in rect_key, out Rect rect_val, label, prop.isExpanded);

        if (!prop.isExpanded)
          return;

        int ilen = m_Pairs.arraySize;

        // advance row
        rect_val.y = rect_key.y += STD_LINE_ADVANCE;

        // key header column
        rect_key.x = total.x + STD_PAD;
        rect_key.width = 0.45f * (total.width - s_TypeDropdownWidth);

        if (m_Map.KeyType == typeof(TypedKey))
        {
          TypedKeyHeader(rect_key);
        }
        else
        {
          KeyHeader(rect_key, "Key");
        }

        // map-wide button column
        rect_val.xMin = rect_key.xMax + STD_PAD;
        rect_val.xMax = total.xMax;
        rect_val.width = (rect_val.width - STD_PAD) / 2f;

        // add entry button:
        if (GUI.Button(rect_val, "Add Entry", Styles.ButtonSmall))
        {
          m_Pairs.InsertArrayElementAtIndex(ilen);

          var new_pair = m_Pairs.GetArrayElementAtIndex(ilen);

          if (m_Map is OmniLookup)
          {
            OmniLookupNewStringEntry(new_pair, ilen);
            return;
          }

          if (m_Map.KeyType == typeof(string))
          {
            var new_key = new_pair.FindPropertyRelative("Key");
            new_key.stringValue = Strings.MakeRandom();
          }

          if (m_Map.ValueType == typeof(string))
          {
            var new_val = new_pair.FindPropertyRelative("Value");
            new_val.stringValue = s_LoremIpsum[ilen % s_LoremIpsum.Length];
          }

          prop.serializedObject.ApplyModifiedProperties();
          return;
        }

        rect_val.x += rect_val.width + STD_PAD;

        // clear map button:
        if (GUI.Button(rect_val, "Clear All", Styles.ButtonSmall))
        {
          m_Pairs.ClearArray();
          prop.serializedObject.ApplyModifiedProperties();
          return;
        }

        // advance to start entry rows, reset val column
        rect_val.y = rect_key.y += STD_LINE_ADVANCE; // + VERTICAL_SPACE;

        rect_val.xMin = rect_key.xMax + STD_PAD;

        // make rect for opening an extended options menu per entry:
        var rect_opt_btn  = new Rect(rect_val);
        rect_opt_btn.xMin = rect_opt_btn.xMax - STD_TOGGLE_W;

        rect_val.xMax = rect_opt_btn.xMin - STD_PAD;

        bool was_enabled  = GUI.enabled;
        bool was_changed  = GUI.changed;
        GUI.changed       = false;

        if (m_Map.ValueType == typeof(string))
        {
          rect_val.height = STD_LINE_HEIGHT * N_LINES_STRING_VALUE;
        }
        else
        {
          rect_val.height = STD_LINE_HEIGHT;
        }

        m_DupeSet.Clear();

        // handle custom drawers
        System.Action<SerializedProperty, Rect, Rect> entry_drawer;

        if (m_Map is OmniLookup)
          entry_drawer = OmniLookupEntryDrawer;
        else if (m_StrictRefType)
          entry_drawer = ObjectLookupEntryDrawer;
        else
          entry_drawer = DefaultEntryDrawer;

        int i = 0;
        while (i < ilen)
        {
          if (GUI.changed)
            break;

          EditorGUI.BeginChangeCheck();

          entry_drawer(m_Pairs.GetArrayElementAtIndex(i), rect_key, rect_val);

          if (EditorGUI.EndChangeCheck())
          {
            GUI.changed = true;
          }

          if (Event.current.control)
          {
            if (GUI.Button(rect_opt_btn, "X", Styles.ButtonSmall))
            {
              m_Pairs.DeleteArrayElementAtIndex(i);
              GUI.changed = true;
            }
          }
          else if (GUI.Button(rect_opt_btn, ">", Styles.ButtonSmall))
          {
            var menu = MakeEntryOptionMenu(i, ilen, out IPromiseKeeper promise);
            menu.DropDown(new Rect( x:      rect_opt_btn.xMax - 22f,
                                    y:      rect_opt_btn.y,
                                    width:  22f,
                                    height: STD_LINE_HEIGHT));
            promise.Dispose();
          }

          rect_opt_btn.y = rect_val.y = ( rect_key.y += rect_val.height + STD_PAD );
          ++i;
        } // end while loop

        GUI.enabled = was_enabled;

        if (GUI.changed && prop.serializedObject.ApplyModifiedProperties())
        {
          m_BadKeySet.Clear();
        }

        GUI.changed = was_changed;
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        if (prop.propertyType == SerializedPropertyType.ObjectReference)
        {
          // TODO perhaps draw a proxy view in addition to this simple Object ref?
          return STD_LINE_HEIGHT;
        }

        if (m_Map == null && !this.TryGetUnderlyingValue(prop, out m_Map))
        {
          return STD_LINE_HEIGHT;
        }

        m_Pairs = prop.FindPropertyRelative("m_Pairs");

        if (m_MapTypeName == null)
        {
          if (m_Map.ValueType == typeof(string))
          {
            m_ValueHeight = STD_LINE_HEIGHT * N_LINES_STRING_VALUE;
          }
          else if (TSpy<Object>.IsAssignableFrom(m_Map.ValueType) || m_Map.ValueType.IsInterface)
          {
            if (!prop.TryGetUnderlyingValue("m_StrictType", out m_StrictRefType))
            {
              m_StrictRefType = new SerialType(m_Map.ValueType);
            }

            if (m_StrictRefType && Logging.Assert(m_StrictRefType.Type.IsReferencable()))
            {
              m_StrictRefType = TSpy<Object>.SerialType;
            }
          }

          if (m_StrictRefType)
            m_MapTypeName = $" <{Types.GetRichLogName(m_Map.KeyType)}, {m_StrictRefType.LogName()}>";
          else if (m_Map.ValueType == typeof(object))
            m_MapTypeName = RichText.Comment(Types.GetLogName(m_Map));
          else
            m_MapTypeName = $" <{Types.GetRichLogName(m_Map.KeyType)}, {Types.GetRichLogName(m_Map.ValueType)}>";

          m_LabelWidth = Styles.Foldout.CalcSize(label).x + STD_PAD;
        }

        if (!prop.isExpanded)
        {
          return STD_LINE_HEIGHT + STD_PAD;
        }

        return  // VERTICAL_SPACE +           // initial vertical spacing
                STD_LINE_ADVANCE * 2 +        // header + button row
                // VERTICAL_SPACE +           // vertical spacing
                m_Map.Count * m_ValueHeight + // entries
                m_Map.Count * STD_PAD +       // entry padding
                STD_PAD;                      // terminal vertical spacing
      }


      private GenericMenu MakeEntryOptionMenu(int i, int ilen, out IPromiseKeeper promise)
      {
        Logging.Assert(i < ilen, "i < ilen");

        var menu = new GenericMenu();

        var labels = Labels.Pool.MakePromises(count: 3);
        promise = labels;

        labels[2].text = "Delete Element";
        menu.AddItem(labels[2], on: false, () =>
        {
          m_Pairs.DeleteArrayElementAtIndex(i);
          m_Pairs.serializedObject.ApplyModifiedProperties();
          GUI.changed = true;
        });

        menu.AddSeparator("");

        labels[0].text = "▲ Shift Element Up 1";
        if (i > 0)
        {
          menu.AddItem(labels[0], on: false, () =>
          {
            if (m_Pairs.MoveArrayElement(i, i - 1))
            {
              m_Pairs.serializedObject.ApplyModifiedProperties();
              GUI.changed = true;
            }
          });
        }

        labels[1].text = "▼ Shift Element Down 1";
        if (i < ilen - 1)
        {
          menu.AddItem(labels[1], on: false, () =>
          {
            if (m_Pairs.MoveArrayElement(i, i + 1))
            {
              m_Pairs.serializedObject.ApplyModifiedProperties();
              GUI.changed = true;
            }
          });
        }
        
        return menu;
      }



      private bool DefaultKeyDrawer(SerializedProperty prop_key, in Rect rect_key)
      {
        bool valid = m_BadKeySet.Add(prop_key) &&
                     m_DupeSet.Add(prop_key)   &&
                     m_BadKeySet.Remove(prop_key);

        if (prop_key.propertyType == SerializedPropertyType.String)
        {
          if (valid)
            _ = DelayedStringField(in rect_key, prop_key, Styles.TextFieldGood);
          else
            _ = DelayedStringField(in rect_key, prop_key, Styles.TextFieldBad);
        }
        else
        {
          _ = EditorGUI.PropertyField(rect_key, prop_key, GUIContent.none, false);
        }

        return valid;
      }

      private void DefaultEntryDrawer(SerializedProperty prop_pair, Rect rect_key, Rect rect_val)
      {
        bool valid_key = DefaultKeyDrawer(prop_pair.FindPropertyRelative("Key"), in rect_key);

        var prop_val = prop_pair.FindPropertyRelative("Value");

        EditorGUI.BeginDisabledGroup(!valid_key);
        if (prop_val.propertyType == SerializedPropertyType.String)
        {
          _ = DelayedStringField(in rect_val, prop_val);
        }
        else
        {
          _ = EditorGUI.PropertyField(rect_val, prop_val, GUIContent.none, false);
        }
        EditorGUI.EndDisabledGroup();
      }


      private void ObjectLookupEntryDrawer(SerializedProperty prop_pair, Rect rect_key, Rect rect_val)
      {
        bool valid_key = DefaultKeyDrawer(prop_pair.FindPropertyRelative("Key"), in rect_key);

        var prop_val = prop_pair.FindPropertyRelative("Value");
        Logging.Assert(prop_val.propertyType == SerializedPropertyType.ObjectReference);

        EditorGUI.BeginDisabledGroup(!valid_key);
        EditorGUI.ObjectField(rect_val, prop_val, m_StrictRefType, GUIContent.none);
        EditorGUI.EndDisabledGroup();
      }


      private void OmniLookupEntryDrawer(SerializedProperty prop_pair, Rect rect_key, Rect rect_val)
      {
        var prop_typedkey = prop_pair.FindPropertyRelative("Key");
        var prop_valuestr = prop_pair.FindPropertyRelative("Value");

        Logging.Assert(prop_typedkey.TryGetUnderlyingValue(out TypedKey tkey));
        
        bool valid =  m_BadKeySet.Add(tkey) &&
                      m_DupeSet.Add(tkey)   &&
                      m_BadKeySet.Remove(tkey);

        if (TypedKeyField(rect_key, prop_typedkey, valid, out SerializedProperty prop_typecode))
        {
          var type_code     = (SerialTypeCode)prop_typecode.enumValueIndex;
          var prop_typename = prop_pair.FindPropertyRelative("StrictRefType.m_TypeName");

          if (type_code >= SerialTypeCode.RefAssetObject)
          {
            prop_typename.stringValue = TSpy<Object>.AQName;
          }
          else
          {
            prop_typename.stringValue = string.Empty;
          }

          prop_valuestr.stringValue = Serializer.DefaultValueStringFor(type_code);
          return;
        }

        switch ((SerialTypeCode)prop_typecode.enumValueIndex)
        {
          case SerialTypeCode.String:
          {
            _ = DelayedStringField(in rect_val, prop_valuestr);
          } break;

          case SerialTypeCode.Integer:
          {
            if (!int.TryParse(prop_valuestr.stringValue, out int current))
            {
              prop_valuestr.stringValue = "0";
              current = 0;
            }

            EditorGUI.BeginChangeCheck();
            int edit = EditorGUI.DelayedIntField(rect_val, GUIContent.none, current, Styles.NumberField);
            if (EditorGUI.EndChangeCheck())
            {
              prop_valuestr.stringValue = edit.ToString();
            }
          } break;

          case SerialTypeCode.Float:
          {
            if (!float.TryParse(prop_valuestr.stringValue, out float current))
            {
              prop_valuestr.stringValue = "0";
              current = 0f;
            }

            EditorGUI.BeginChangeCheck();
            float edit = EditorGUI.DelayedFloatField(rect_val, GUIContent.none, current, Styles.NumberField);
            if (EditorGUI.EndChangeCheck())
            {
              prop_valuestr.stringValue = edit.ToString();
            }
          } break;

          case SerialTypeCode.Bool:
          {
            if (!bool.TryParse(prop_valuestr.stringValue, out bool current))
            {
              prop_valuestr.stringValue = "False";
              current = false;
            }

            EditorGUI.BeginChangeCheck();
            bool edit = EditorGUI.Toggle(rect_val, GUIContent.none, current);
            if (EditorGUI.EndChangeCheck())
            {
              prop_valuestr.stringValue = edit.ToString();
            }
          } break;

          case SerialTypeCode.RefAssetObject:
          {
            var prop_strict_type = prop_pair.FindPropertyRelative("StrictRefType");

            if (!prop_strict_type.TryGetUnderlyingValue(out SerialType stype))
            {
              Logging.ShouldNotReach(blame: stype);
              stype = TSpy<Object>.SerialType;
            }
            else if (stype.IsMissing)
            {
              stype = TSpy<Object>.SerialType;
            }

            Object curr_obj;
            if (Serializer.TryParseString(prop_valuestr.stringValue,
                                          SerialTypeCode.RefAssetObject,
                                          out object boxed))
            {
              curr_obj = boxed as Object;
            }
            else
            {
              Logging.ShouldNotReach(blame: prop_valuestr.stringValue);
              break;
            }

            EditorGUI.BeginChangeCheck();
            curr_obj = EditorGUI.ObjectField(rect_val, GUIContent.none, curr_obj, stype.Type,
                                             allowSceneObjects: false);
            if (EditorGUI.EndChangeCheck())
            {
              var prop_typename = prop_strict_type.FindPropertyRelative("m_TypeName");

              if (curr_obj && stype.IsMissingOr<Object>())
              {
                if (curr_obj is BaseComponent)
                {
                  prop_typename.stringValue = TSpy<BaseComponent>.AQName;
                }
                else if (curr_obj is BaseAsset)
                {
                  prop_typename.stringValue = TSpy<BaseAsset>.AQName;
                }
                else
                {
                  prop_typename.stringValue = curr_obj.GetType().GetQualifiedName();
                }

                prop_valuestr.stringValue = WrappedSerializer<Object>.ToJson(curr_obj);
              }
              else
              {
                prop_typename.stringValue = TSpy<Object>.AQName;
                prop_valuestr.stringValue = Serializer.NULL_REF_JSON_STRING;
              }
            }
          } break;

          case SerialTypeCode.Null:
          {
            InfoField(rect_val, text: "(null)", style: Styles.TextDetailCenter);
          } break;

          case SerialTypeCode.RefSceneObject:
          default:
          {
            InvalidField(rect_val, "Unimplemented!");
          } break;
        }
      }

      private static void OmniLookupNewStringEntry(SerializedProperty new_pair, int i)
      {
        var typed_key = new_pair.FindPropertyRelative("Key");

        var new_keystr = typed_key.FindPropertyRelative("String");
        new_keystr.stringValue = Strings.MakeRandom();

        var type_code = typed_key.FindPropertyRelative("Type");
        type_code.enumValueIndex = (int)SerialTypeCode.String;

        var new_val = new_pair.FindPropertyRelative("Value");
        new_val.stringValue = s_LoremIpsum[i % s_LoremIpsum.Length];

        new_pair.serializedObject.ApplyModifiedProperties();
      }


      private static void KeyHeader(Rect pos, string key_label)
      {
        InfoField(pos, key_label, Styles.TextDetailCenter);
      }

      private static void TypedKeyHeader(Rect rect)
      {
        float x_max = rect.xMax;
        rect.width = rect.width / 2f - STD_LINE_HEIGHT;

        InfoField(rect, "Type", Styles.TextDetailCenter);

        rect.x += rect.width + STD_PAD;
        rect.xMax = x_max;

        InfoField(rect, "Key", Styles.TextDetailCenter);
      }


      private enum ViewableTypeCode
      {
        Null   = SerialTypeCode.Null,
        String = SerialTypeCode.String,
        Int    = SerialTypeCode.Integer,
        Float  = SerialTypeCode.Float,
        Bool   = SerialTypeCode.Bool,
        Asset  = SerialTypeCode.RefAssetObject,

        [HideInInspector]
        Unsupported = SerialTypeCode.Unsupported
      }

      private static readonly float s_TypeDropdownWidth = Styles.Popup.CalcWidth("string");
      private static bool TypedKeyField(Rect rect, SerializedProperty prop, bool valid, out SerializedProperty prop_typecode)
      {
        prop_typecode = prop.FindPropertyRelative("Type");

        Logging.Assert(prop_typecode != null, "prop_typecode != null");

        float x_max = rect.xMax;
        //rect.width = rect.width / 2f - STD_LINE_HEIGHT;
        rect.width = s_TypeDropdownWidth;

        if (EnumPopupField(rect, prop_typecode, typeof(ViewableTypeCode),
                           default_value: (long)SerialTypeCode.Unsupported))
        {
          return true;
        }

        rect.x += rect.width + STD_PAD;
        rect.xMax = x_max;

        if (valid)
          DelayedStringField(rect, prop.FindPropertyRelative("String"), Styles.TextFieldGood);
        else
          DelayedStringField(rect, prop.FindPropertyRelative("String"), Styles.TextFieldBad);

        return false;
      }

    } // end class IMapDrawer

  } // end partial class GUIDrawers

}