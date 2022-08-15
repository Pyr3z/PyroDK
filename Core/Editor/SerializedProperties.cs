/**
@file   PyroDK/Core/Editor/SerializedProperties.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-09

@brief
  Provides utilities and extensions for Unity-Serialization-related
  operations, such as with editor inspectors and serialized properties.
**/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace PyroDK.Editor
{
  using StringBuilder = System.Text.StringBuilder;
  using FieldInfo     = System.Reflection.FieldInfo;
  using Type          = System.Type;


  [InitializeOnLoad]
  public static class SerializedProperties
  {

    private static readonly HashMap<string, FieldInfo> s_PropertyFieldLookup;


    static SerializedProperties()
    {
      s_PropertyFieldLookup = new HashMap<string, FieldInfo>(new HashMapParams(71));
    }


    public static bool FindFieldInfo(this SerializedProperty prop,
                                      out FieldInfo field)
    {
      string apath = prop.MakeAnonymousPath();

      if (s_PropertyFieldLookup.Find(apath, out field) && field != null)
        return true;

      string[] path_splits = prop.propertyPath.Split('.');
      Type curr_type = prop.serializedObject.targetObject.GetType();

      for (int i = 0, ilen = path_splits.Length; i < ilen; ++i)
      {
        if (curr_type.TryGetSerializableField(path_splits[i], out field))
        {
          curr_type = field.FieldType;

          if (i == ilen - 1)
            break;
          else if (curr_type.IsArray)
            curr_type = curr_type.GetElementType();
          else if (curr_type.IsGenericType && curr_type.GetGenericTypeDefinition() == typeof(List<>))
            curr_type = curr_type.GetGenericArguments()[0];
          else // (skip the remaining block if it's just a normal field)
            continue;

          // Ignore "Array.data[idx]" slugs:
          i += 2;
        }
        else
        {
          Logging.ShouldNotReach(blame: prop);
          return false;
        }
      }

      if (field != null)
      {
        return s_PropertyFieldLookup.Map(apath, field);
      }

      return false;
    }


    public static bool IsDisposed(this SerializedProperty prop)
    {
      // JESUS CHRIST WAS IT SO HARD TO PROVIDE A METHOD LIKE THIS, UNITY ?!

      // I HAD TO SCOUR THE SOURCE CODE IN ORDER TO FIGURE OUT THAT
      // I COULD CHECK FOR DISPOSAL THIS WAY:

      return SerializedProperty.EqualContents(null, prop);
    }


    public static bool IsArrayElement(this SerializedProperty prop)
    {
      return prop.propertyPath.EndsWith("]");
    }

    public static bool IsArrayElement(this SerializedProperty prop, out int idx)
    {
      string path = prop.propertyPath;
      idx = -1;
      return path[path.Length - 1] == ']' && path.TryParseLastIndex(out idx);
    }


    public static bool IsReorderableList(this SerializedProperty prop)
    {
      return  FindFieldInfo(prop, out FieldInfo field) &&
              !field.IsDefined<NonReorderableAttribute>(inherit: true);
    }

    public static bool IsReadOnly(this SerializedProperty prop)
    {
      return  FindFieldInfo(prop, out FieldInfo field) &&
              field.IsDefined<ReadOnlyAttribute>(inherit: true);
    }


    public static string GetPropertyUID(this SerializedProperty prop)
    {
      return prop.propertyPath + prop.serializedObject.targetObject.GetInstanceID();
    }

    public static uint GetPropertyHash(this SerializedProperty prop)
    {
      return Hashing.MixHashes((uint)prop.propertyPath.GetHashCode(),
                               (uint)prop.serializedObject.targetObject.GetInstanceID());
    }


    public static string GetArrayPropertyPath(this SerializedProperty prop)
    {
      const string ARRAY_ELEMENT_HINT = ".Array.data[";

      string path = prop.propertyPath;
      int len = path.Length;

      if (path[len - 1] != ']')
        return null;

      int cut = 6 + path.LastIndexOf(ARRAY_ELEMENT_HINT);
        // (6 is the length of ".Array")

      if (cut < 6)
        return null;

      return path.Remove(cut);
    }

    public static bool TryGetArrayProperty(this SerializedProperty elem, out SerializedProperty array)
    {
      string path = GetArrayPropertyPath(elem);

      if (path == null)
      {
        array = null;
        return false;
      }

      array = elem.serializedObject.FindProperty(path);

      return array != null;
    }


    public static IEnumerable<SerializedProperty> VisibleChildren(this SerializedProperty prop)
    {
      if (!prop.hasVisibleChildren)
        yield break;

      var child_it = prop.Copy();
      int depth    = prop.depth + 1;

      bool drill = true;
      while (child_it.NextVisible(drill) &&
             child_it.depth == depth &&
             child_it.propertyPath.StartsWith(prop.propertyPath))
      {
        drill = false;
        yield return child_it;
      }
    }


    public static string MakeNamedPath(this SerializedProperty prop)
    {
      return new StringBuilder()
              .AppendGenericPath(prop.serializedObject.targetObject)
              .Append('.').Append(prop.propertyPath)
              .ToString();
    }


    public static string MakeAnonymousPath(this SerializedProperty prop)
    {
      var bob    = new StringBuilder();
      var splits = prop.propertyPath.Split('.');

      bob.AppendTypeNamePlain(prop.serializedObject.targetObject)
         .Append('.');

      int i = 0, ilen = splits.Length;
      for (; i < ilen; ++i)
      {
        if (splits[i] == "Array")
        {
          if (i < ilen - 1 && (splits[i + 1].StartsWith("data[") || splits[i + 1] == "size"))
            ++i; // skip
        }
        else if (i < ilen - 1)
        {
          bob.Append(splits[i])
             .Append('.');
        }
        else
        {
          bob.Append(splits[i]);
        }
      }

      return bob.ToString();
    }



    public static bool TryGetUnderlyingValue<T>(this PropertyDrawer drawer,
                                                SerializedProperty prop,
                                                out T field)
    {
      var target = prop.serializedObject.targetObject;

      if (drawer.fieldInfo.FieldType.IsArrayOrList())
      {
        field = default;

        if (Logging.Assert(drawer.fieldInfo.TryGetValue(target, out IList list)))
          return false;

        int dot = prop.propertyPath.LastIndexOf('.');
        if (dot < 0 || !prop.propertyPath.TryParseNextIndex(dot, out int idx))
        {
          if (TSpy<T>.IsArrayOrList)
          {
            field = (T)list;
            return true;
          }

          Logging.ShouldNotReach(blame: prop);
          return false;
        }

        if (idx < list.Count && list[idx] is T casted)
        {
          field = casted;
          return true;
        }

        return false;
      }
      else if (drawer.fieldInfo.IsFieldIn(target))
      {
        return drawer.fieldInfo.TryGetValue(target, out field);
      }
      else
      {
        return TryGetUnderlyingValue(prop, out field);
      }
    }

    public static bool TryGetUnderlyingValue<T>(this SerializedProperty prop,
                                                out T field_value)
    {
      if (TryGetUnderlyingBoxedValue(prop, out object boxed))
      {
        if (boxed is T valid)
        {
          field_value = valid;
          return true;
        }
        else
        {
          $"Cannot cast found field to type {TSpy<T>.LogName}!"
            .LogError();
        }
      }

      field_value = default;
      return false;
    }

    public static bool TryGetUnderlyingValue<T>(this SerializedProperty prop,
                                                string child_prop_path,
                                                out T field_value)
    {
      if (TryGetUnderlyingBoxedValue(prop, child_prop_path, out object boxed))
      {
        if (boxed is T valid)
        {
          field_value = valid;
          return true;
        }
        else
        {
          $"Cannot cast found field to type {TSpy<T>.LogName}!"
            .LogError();
        }
      }

      field_value = default;
      return false;
    }


    public static bool TryGetUnderlyingBoxedValue(SerializedProperty prop,
                                                  out object boxed_value)
    {
      boxed_value = prop.serializedObject.targetObject;
      return TryGetUnderlyingBoxedValue(prop.propertyPath, ref boxed_value);
    }

    public static bool TryGetUnderlyingBoxedValue(SerializedProperty prop,
                                                  string child_prop_path,
                                                  out object boxed_value)
    {
      string path = prop.propertyPath;

      if (!child_prop_path.IsEmpty())
      {
        if (child_prop_path.StartsWith("."))
          path += child_prop_path;
        else
          path += '.' + child_prop_path;
      }

      boxed_value = prop.serializedObject.targetObject;

      return TryGetUnderlyingBoxedValue(path, ref boxed_value);
    }

    private static bool TryGetUnderlyingBoxedValue(string prop_path, ref object boxed_value)
    {
      if (boxed_value == null)
        return false;

      // Strategy: use Reflection to follow the fully-qualified property path
      //           and obtain an exact reference to the underlying object.

      string[] path_splits = prop_path.Split('.');

      for (int i = 0; i < path_splits.Length; ++i)
      {
        // Look at the next segment of the property's path:
        var field_name = path_splits[i];

        // Special handling to support Array/List nested types:
        if (field_name == "Array" && boxed_value is IList boxed_list)
        {
          // increment past the injected array sub-properties:
          // parse for an integer index:
          if (path_splits[++i].TryParseNextIndex(out int idx) && idx < boxed_list.Count)
          {
            boxed_value = boxed_list[idx];
            continue;
          }
          else
          {
            //Logging.ShouldNotReach(blame: prop_path);
            return false;
          }
        }

        // Find the FieldInfo of the target we are currently looking at:
        if (!boxed_value.GetType().TryGetSerializableField(field_name, out FieldInfo field))
        {
          // Early-out opportunity:
          return false;
        }

        // Step finished.
        boxed_value = field.GetValue(boxed_value);

      } // end for-loop

      // We've walked to the end of the property path.
      // Returns true if success:
      return boxed_value != null;
    }

  }

}