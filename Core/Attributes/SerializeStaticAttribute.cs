/**
@file   PyroDK/Core/Attributes/SerializeStaticAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-21

@brief
  A `UnityEngine.PropertyAttribute` that allows a static field to
  be serialized using various forms of magic.
**/

#pragma warning disable IDE0051, CS0414

using System.Reflection;
using System.IO;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace PyroDK
{

  using Type = System.Type;


  [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
  public sealed class SerializeStaticAttribute : System.Attribute
  {
    public SerializeStaticAttribute()
    {
    }
  }


  // TODO switch over to a ScriptableObject for storage... you over-thinking idiot.

  public static class SerializeStaticFields
  {
    public static event System.Action OnNewFieldsApplied;
    public static string EditorFilePath => Application.dataPath + ASSET_PATH;


    private const string ASSET_PATH = "/Settings/[SerializeStatic].json";


    private static SerialFieldMap s_FieldMap = new SerialFieldMap(new HashMapParams(64));


    public static bool StoreField<T>(string field_name)
    {
      return StoreField(TSpy<T>.Type, field_name);
    }

    public static bool StoreField(Type declarer, string field_name)
    {
      if (!declarer.TryGetField(field_name, out FieldInfo field, TypeMembers.STATIC))
      {
        $"Field \"{declarer?.Name}.{field_name}\" was not found."
          .LogWarning();
        return false;
      }

      return s_FieldMap.StoreField(field, null, true, out _ );
    }


    public static bool LoadField<T>(string field_name)
    {
      return LoadField(TSpy<T>.Type, field_name);
    }

    public static bool LoadField(Type declarer, string field_name)
    {
      if (!declarer.TryGetField(field_name, out FieldInfo field, TypeMembers.STATIC))
      {
        $"Field \"{declarer?.Name}.{field_name}\" was not found."
          .LogWarning();
        return false;
      }

      return s_FieldMap.LoadField(field, null);
    }


    public static bool SaveAll(bool report)
    {
      int new_values = Collect(clear: !HasJson(), report: report);

      if (report)
      {
        $"Collected {new_values} new values for saving."
          .LogBoring();
      }

      if (!s_FieldMap.IsDirty && HasJson())
      {
        if (report)
        {
          $"Nothing new to serialize."
            .LogBoring();
        }

        return false;
      }

      if (s_FieldMap.TrySaveToDisk(EditorFilePath, pretty_print: true))
      {
        if (report)
        {
          $"Serialized {s_FieldMap.Count} fields to \"{EditorFilePath}\"."
            .LogBoring();
        }

        return s_FieldMap.Count > 0;
      }
      else
      {
        if (report)
        {
          $"Error serializing {s_FieldMap.Count} fields to \"{EditorFilePath}\"??"
            .LogError();
        }

        return false;
      }
    }

    public static int LoadAll(bool report)
    {
      int precount = s_FieldMap.Count;

      if (report)
      {
        $"Pre-count in field map: {s_FieldMap.Count}".LogBoring();
      }

      var temp = new SerialFieldMap(new HashMapParams(precount * 3));

      if (temp.TryLoadFromDisk(EditorFilePath))
      {
        s_FieldMap.Absorb(temp, string.Equals);

        int new_values = s_FieldMap.LoadAllStaticFields();
        
        if (report)
        {
          if (precount > s_FieldMap.Count)
          {
            $"Wiped {precount - s_FieldMap.Count} fields, applied {new_values}/{s_FieldMap.Count} new values."
              .Log();
          }
          else
          {
            $"{s_FieldMap.Count - precount} new fields discovered. {new_values} new values applied."
              .Log();
          }

          $"Current count in field map: {s_FieldMap.Count}"
            .LogBoring();
        }

        if (new_values > 0)
        {
          OnNewFieldsApplied?.Invoke();
        }

        return new_values;
      }
      else if (report)
      {
        $"Unexpected error occurred deserializing \"{EditorFilePath}\""
          .LogError();
      }

      return 0;
    }



    #region Editor Menu Items
    #if UNITY_EDITOR


    [MenuItem("PyroDK/[SerializeStatic]/Recollect Fields", priority = 0)]
    public static void RecollectFields()
    {
      int new_values = Collect(clear: true, report: true);
      if (new_values > 0)
      {
        $"Recollected {new_values} new field values.".LogSuccess();
      }
      else
      {
        "Recollected 0 new field values.".LogBoring();
      }
    }


    [MenuItem("PyroDK/[SerializeStatic]/Clear Missing Fields", priority = 0)]
    public static void ClearMissingFields()
    {
      int cleared = s_FieldMap.UnmapAll(SerialFieldInfo.IsMissingField);

      if (cleared > 0)
      {
        $"Cleared {cleared} missing fields.".Log();
      }
      else
      {
        $"Cleared {cleared} missing fields.".LogBoring();
      }
    }

    [MenuItem("PyroDK/[SerializeStatic]/Clear Missing Fields", validate = true)]
    public static bool HasFields()
    {
      return s_FieldMap.Count > 0;
    }


    [MenuItem("PyroDK/[SerializeStatic]/Save .json", priority = 1)]
    private static void SaveToJson()
    {
      SaveAll(report: true);
    }

    [MenuItem("PyroDK/[SerializeStatic]/Save .json", validate = true)]
    private static bool CanSaveJson()
    {
      return s_FieldMap.Count > 0 && (s_FieldMap.IsDirty || !HasJson());
    }


    [MenuItem("PyroDK/[SerializeStatic]/Reload .json", priority = 2)]
    private static void ReloadJson()
    {
      _ = LoadAll(report: true);
    }


    [MenuItem("PyroDK/[SerializeStatic]/Delete .json", priority = 3)]
    private static void DeleteJson()
    {
      Filesystem.DeleteAsset(EditorFilePath);
    }


    [MenuItem("PyroDK/[SerializeStatic]/Log All Fields", priority = 20)]
    private static void LogAllFields()
    {
      var strb = new System.Text.StringBuilder(RichText.Attribute("SerializeStatic"));

      _ = strb.Append(" Currently captured fields: (scroll)").AppendLine();

      int i = 0;
      foreach (var (field, value) in s_FieldMap)
      {
        _ = strb.Append($"{++i,2}  ").Append(field.ToString())
                .Append(" = ").Append(value).Append('\n');
      }

      strb.ToString().Log();
    }


    [InitializeOnLoadMethod]
    private static void OnEditorReload()
    {
      if (s_FieldMap.Clear())
      {
        $"s_FieldMap cleared.".LogBoring();
      }

      if (HasJson())
      {
        _ = LoadAll(report: false);
      }
      else
      {
        _ = Collect(clear: true, report: false);
      }
    }


    [MenuItem("PyroDK/[SerializeStatic]/Reload .json", validate = true)]
    [MenuItem("PyroDK/[SerializeStatic]/Delete .json", validate = true)]
    #endif // UNITY_EDITOR
    public static bool HasJson()
    {
      return File.Exists(EditorFilePath);
    }

    #endregion Editor Menu Items


    //#if !UNITY_EDITOR

    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    //private static void OnAfterAssembliesLoaded()
    //{
    //  LoadAll(report: false);
    //}

    //#endif


    private static int Collect(bool clear, bool report)
    {
      if (clear)
        _ = s_FieldMap.Clear();

      int new_values = 0;

      foreach (var type in Assemblies.GetAllUserTypes(where_type: (t) => !t.IsInterface && !t.IsGenericTypeDefinition))
      {
        foreach (var field in type.GetFields(TypeMembers.STATIC))
        {
          if (field.IsDefined<SerializeStaticAttribute>(false))
          {
            if (field.FieldType.IsGenericType)
            {
              $"{RichText.Attribute("SerializeStatic")} is unsupported on generic-typed fields! {field.GetLogName()}".LogWarning(typeof(SerializeStaticAttribute));
              continue;
            }

            if (s_FieldMap.StoreField(field, instance: null, overwrite: false, out string value))
            {
              ++new_values;

              if (report)
              {
                $"Collected field {field.GetLogName()}.\nvalue={value}".Log();
              }
            }
            else if (report)
            {
              if (value != null)
              {
                $"Skipped re-collecting field {field.GetLogName()}; values are identical".LogBoring();
              }
              else
              {
                $"Error collecting field {field.GetLogName()}!".LogWarning();
              }
            }
          }
        }
      }

      return new_values;
    }

  }

}