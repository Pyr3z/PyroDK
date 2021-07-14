/**
@file   PyroDK/Core/Editor/GUIDrawers/ViewAsEnumDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-26

@brief
  Attempts to draw fields marked with `[EnumField(typeof(TEnum))]`
  like they were that enum of type TEnum.
**/

using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{
  using Type = System.Type;
  using Enum = System.Enum;


  public static partial class GUIDrawers
  {

    public static bool EnumPopupField(in Rect pos, SerializedProperty prop, Type enum_type,
                                      System.Func<Enum, bool> filter = null,
                                      long default_value = 0)
    {
      if (Logging.Assert(enum_type != null, "m_EnumType != null"))
      {
        InvalidField(pos, "Null Enum Type!");
        return false;
      }

      if (!enum_type.IsEnum)
      {
        return LongNumberField(in pos, prop, is_bad: true);
      }

      if (filter == null)
        filter = (e) => !EnumSpy.HasAttribute<HideInInspector>(e);

      if (!EnumSpy.IsNamedValue(enum_type, prop.longValue) &&
           EnumSpy.IsNamedValue(enum_type, default_value))
      {
        prop.longValue = default_value;
        return true;
      }

      var current = (Enum)Enum.ToObject(enum_type, prop.longValue);

      PushIndentLevel(0, fix_label_width: false);

      EditorGUI.BeginChangeCheck();
      
      current = EditorGUI.EnumPopup(pos, GUIContent.none, current, filter,
                                    includeObsolete: false);
      PopIndentLevel(fix_label_width: false);

      if (EditorGUI.EndChangeCheck())
      {
        prop.longValue = System.Convert.ToInt64(current);
        return true;
      }

      return false;
    }

    public static bool EnumFlagsPopupField( in Rect rect, SerializedProperty prop, string[] display_names,
                                            int start_bit = 0, long all_mask = ~0)
    {
      if (start_bit < 0 || start_bit >= sizeof(long) * 8)
      {
        InvalidField(rect, "Null Enum Type!");
        return false;
      }

      EditorGUI.BeginChangeCheck();

      long current = prop.longValue >> start_bit;

      int edit = EditorGUI.MaskField(rect, (int)current, display_names);

      if (EditorGUI.EndChangeCheck())
      {
        current = (long)edit << start_bit;

        if (current.HasAllBits(all_mask))
          prop.longValue = all_mask;
        else
          prop.longValue = current;

        return true;
      }

      return false;
    }


    [CustomPropertyDrawer(typeof(ViewAsEnumAttribute))]
    private sealed class ViewAsEnumDrawer : PropertyDrawer
    {
      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        if (prop.propertyType != SerializedPropertyType.Integer &&
            prop.propertyType != SerializedPropertyType.Enum)
        {
          InvalidField(in total, label, "[ViewAsEnum] is for integer or enum types!");
          return;
        }

        var attr = (ViewAsEnumAttribute)attribute;
        
        var field = PrefixLabelStrict(total, label);

        if (ShouldDrawFlags(attr))
        {
          EnumFlagsPopupField(in field, prop, attr.Labels, attr.StartBit, attr.AllMask);
        }
        else
        {
          EnumPopupField(in field, prop, attr.EnumType, filter: attr.VisibleEnums.Contains);
        }
      }


      private static bool ShouldDrawFlags(ViewAsEnumAttribute attr)
      {
        if (attr.EnumType != null)
        {
          if (attr.Labels != null)
            return true;
          if (attr.VisibleEnums != null)
            return false;
        }

        if (attr.EnumType == null)
        {
          if (Logging.Assert(attr.DeferredEnumType != null) ||
              !Assemblies.FindSubType(attr.DeferredEnumType, typeof(Enum), out attr.EnumType))
          {
            $"Could not find deferred Enum Type \"{attr.DeferredEnumType}\""
              .LogError(attr);
            return false;
          }
        }

          // notice special operator / used with TriBool:
        attr.UseBitFlags /= TSpy<System.FlagsAttribute>.IsAttributeOn(attr.EnumType);

        var fields = attr.EnumType.GetFields(TypeMembers.ENUMS);

        // attempt to cache bit flag names:
        if (attr.UseBitFlags && fields.Length > 0)
        {
          var buffer = new List<string>(fields.Length);
          attr.AllMask = 0;

          foreach (var field in fields)
          {
            if (field.IsHidden())
              continue;

            long value = ((System.IConvertible)field.GetValue(null)).ToInt64(null);

            // don't draw empty or combined bit flags
            if (value != 0 && (attr.IncludeBitCombos || Bitwise.IsOneBit(value)))
            {
              if (attr.AllMask == 0)
                attr.StartBit = Bitwise.CTZ(value);

              attr.AllMask |= value;
              buffer.Add(field.Name);
            }
          }

          if (attr.AllMask != 0)
          {
            attr.Labels = buffer.ToArray();
            return true;
          }

          // fall through
        }

        // if here, flags will not be drawn:

        attr.VisibleEnums = new HashSet<Enum>();

        foreach (var field in fields)
        {
          if (!field.IsHidden())
            attr.VisibleEnums.Add((Enum)field.GetValue(null));
        }

        return false;
      }

    }

  }

}