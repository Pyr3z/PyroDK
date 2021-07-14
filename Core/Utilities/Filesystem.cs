/**
@file   PyroDK/Core/Utilities/Filesystem.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-10

@brief
  Provides utilities and extensions for working with the local filesystem.
**/

using UnityEngine;


namespace PyroDK
{

  using Directory = System.IO.Directory;
  using File      = System.IO.File;
  using SysPath   = System.IO.Path;


  public static partial class Filesystem
  {
    public const int MAX_PATH = 260;

    public const char DIRECTORY_SEPARATOR     = '/';
    public const char BAD_DIRECTORY_SEPARATOR = '\\';
    public const char VOLUME_SEPARATOR        = ':';


    public const string EXPLICIT_RELATIVE_ROOT   = "./";
    public const string EXPLICIT_RELATIVE_PARENT = "../";

    public static string ProjectDrive
    {
      get
      {
        if (s_ProjectDrive == null)
        {
          s_ProjectDrive = SysPath.GetPathRoot(System.Environment.CurrentDirectory);
          s_ProjectDrive = FixDirectorySeparators(s_ProjectDrive);
          Debug.Assert(!s_ProjectDrive.IsEmpty());
        }

        return s_ProjectDrive;
      }
    }
    private static string s_ProjectDrive = null;

    public static string ProjectRoot
    {
      get
      {
        if (s_ProjectRoot == null)
        {
          s_ProjectRoot = FixDirectorySeparators(System.Environment.CurrentDirectory);
          Debug.Assert(!s_ProjectRoot.IsEmpty());
        }

        return s_ProjectRoot;
      }
    }
    private static string s_ProjectRoot = null;

    public static string AssetsRoot
    {
      get
      {
        if (s_AssetsRoot == null)
        {
          s_AssetsRoot = ProjectRoot + "/Assets";
          Debug.Assert(!s_AssetsRoot.IsEmpty());
        }

        return s_AssetsRoot;
      }
    }
    private static string s_AssetsRoot = null;


    public static readonly char[] DirectorySeparators =
    {
      DIRECTORY_SEPARATOR,      // '/'
      BAD_DIRECTORY_SEPARATOR,  // '\\'
    };


    public static readonly char[] InvalidFileNameChars  = SysPath.GetInvalidFileNameChars();
    public static readonly char[] InvalidPathChars      = SysPath.GetInvalidPathChars();

    public static readonly System.StringComparer PathStringComparer
      = System.StringComparer.CurrentCulture;



    public static bool IsValidFileName(string name)
    {
      return name != null && name.Length > 1 && name.IndexOfAny(InvalidFileNameChars) < 0;
    }

    public static bool IsValidPath(string path)
    {
      return  path != null && path.Length > 1 && path.Length < MAX_PATH &&
              path.IndexOfAny(InvalidPathChars) < 0;
    }

    public static bool IsValidPath(string path, out bool is_directory, out bool is_relative)
    {
      is_directory = is_relative = false;

      if (path == null || path.Length == 0 || path.Length >= MAX_PATH)
        return false;

      is_directory = IsDirectorySeparator(path[path.Length - 1]);
      is_relative  = path[0] == '.';
      return path.IndexOfAny(InvalidPathChars) < 0;
    }

    public static bool IsValidFilePath(string path)
    {
      if (path == null || path.Length < 2 || path.Length >= MAX_PATH ||
          !TryDecomposePath(path, out string dirs, out string file))
      {
        return false;
      }

      return  dirs.IndexOfAny(InvalidPathChars) < 0 &&
              file.IndexOfAny(InvalidFileNameChars) < 0;
    }



    public static bool IsAbsolutePath(string path)
    {
      // Does fewer checks than the standard defines;
      // who the feck wants to deal with the weird-ass Path specs that are
      // *technically* allowable, but that nobody would even recognize? o.O
      return path.Length > 1 && path[1] == VOLUME_SEPARATOR;
    }

    public static bool IsAssetPath(string path)
    {
      if (IsAbsolutePath(path))
        return path.StartsWith(Application.dataPath);

      if (IsExplicitRelativePath(path))
        return path.Substring(2).StartsWith("Assets");

      return path.StartsWith("Assets");
    }

    public static bool IsExplicitRelativePath(string path)
    {
      return path.Length > 1 && path[0] == '.' && path[1] == '/';
    }


    public static bool IsDirectorySeparator(char c)
    {
      return c == DIRECTORY_SEPARATOR || c == BAD_DIRECTORY_SEPARATOR;
    }

    public static bool IsExplicitDirectoryPath(string path)
    {
      return IsDirectorySeparator(path[path.Length - 1]);
    }


    public static bool IsVersionControlLocked(string filepath, string assetpath)
    {
      #if UNITY_EDITOR
      return  File.Exists(filepath)                   &&
              !UnityEditor.AssetDatabase.IsOpenForEdit(assetpath) &&
              !UnityEditor.AssetDatabase.MakeEditable(assetpath);
      #else
      return false;
      #endif
    }


    public static string ExtractFileExtension(string path)
    {
      if (path == null || path.Length < 3 || IsExplicitDirectoryPath(path))
        return string.Empty;

      int dot = path.LastIndexOf('.');

      if (dot < 0 || dot >= path.Length - 1 || dot < path.LastIndexOfAny(DirectorySeparators))
        return string.Empty;

      return path.Substring(dot + 1);
    }


    public static bool PathExists(string path) // assumes you've already validated the path
    {
      if (IsExplicitDirectoryPath(path))
        return Directory.Exists(path.TrimEnd(DirectorySeparators));

      return (path.Length > 1 && path[0] == '.' && path[1] == '/') || File.Exists(path);
    }


    public static bool TryGuaranteePathFor(string assetpath, out string filepath)
    {
      if (!TryMakeAbsolutePath(assetpath, out filepath) || !TryExtractPathHead(filepath, out string filedir))
      {
        return false;
      }

      if (!Directory.Exists(filedir))
      {
        return Directory.CreateDirectory(filedir).Exists;
      }

      return !IsVersionControlLocked(filepath, assetpath);
    }

    
    public static void ImportAsset(string filepath)
    {
      #if UNITY_EDITOR

      if (TryMakeAssetPath(filepath, out filepath) && File.Exists(filepath))
      {
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.ImportAsset(filepath);
        UnityEditor.AssetDatabase.SaveAssets();
      }

      #endif
    }

    public static void DeleteAsset(string filepath)
    {
      #if UNITY_EDITOR
      if (TryMakeAssetPath(filepath, out filepath) && UnityEditor.AssetDatabase.DeleteAsset(filepath))
      {
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();
      }
      #endif
    }


    public static string TrimPath(string path)
    {
      _ = TrimPath(path, out path);
      return path;
    }

    // returns how many chars were trimmed from the beginning only
    public static int TrimPath(string path, out string result)
    {
      result = path;

      if (path == null || path.Length == 0)
        return -1;

      int i   = 0;
      int len = path.Length;

      if (len > 5 && IsDirectorySeparator(path[len - 1]))
      {
        // the value 5 comes from the maximum root length for a path
        --len;
      }

      if (( path[1] == VOLUME_SEPARATOR ) ||
          ( path[0] == '.' && ( path[1] == '.' || IsDirectorySeparator(path[1] ) ) ))
      {
        i = 2;
      }

      while (i < len)
      {
        if (!IsDirectorySeparator(path[i]))
        {
          result = path.Substring(i, len - i);
          break;
        }

        ++i;
      }

      return i;
    }


    public static string FixDirectorySeparators(string path)
    {
      if (path == null)
        return string.Empty;

      return path.Replace(BAD_DIRECTORY_SEPARATOR, DIRECTORY_SEPARATOR);
    }


    public static string MakeAssetPath(string path)
    {
      _ = TryMakeAssetPath(ref path);
      return path;
    }

    public static bool TryMakeAssetPath(string path, out string result)
    {
      if (path == null || path.Length < 2)
      {
        result = path;
        return false;
      }

      string assets_path = AssetsRoot;

      if (path.Length > assets_path.Length && path.StartsWith(assets_path))
      {
        result = path.Substring(assets_path.Length - 6);
        return (assets_path.Length - 6 + result.Length) < MAX_PATH;
      }

      if (TrimPath(path, out result) < 0)
        return false;

      if (!result.StartsWith("Assets/"))
        result = "Assets/" + result;

      return (assets_path.Length - 6 + result.Length) < MAX_PATH;
    }

    public static bool TryMakeAssetPath(ref string path)
    {
      if (path == null || path.Length < 2)
        return false;

      string assets_path = AssetsRoot;

      string result;

      if (path.Length > assets_path.Length && path.StartsWith(assets_path))
      {
        result = path.Substring(assets_path.Length - 6);

        if ((assets_path.Length - 6 + result.Length) < MAX_PATH)
        {
          path = result;
          return true;
        }

        return false;
      }

      if (TrimPath(path, out result) < 0)
        return false;

      if (!result.StartsWith("Assets/"))
        result = "Assets/" + result;

      if ((assets_path.Length - 6 + result.Length) < MAX_PATH)
      {
        path = result;
        return true;
      }

      return false;
    }


    public static bool TryMakeAbsolutePath(string path, out string result)
    {
      if (IsValidPath(path))
      {
        bool is_explicit_dir = IsExplicitDirectoryPath(path);

        result = SysPath.GetFullPath(path);

        if (result.IsEmpty())
        {
          result = path;
          return false;
        }

        result = result.Replace(BAD_DIRECTORY_SEPARATOR, DIRECTORY_SEPARATOR);

        if (is_explicit_dir && !IsExplicitDirectoryPath(result))
          result += DIRECTORY_SEPARATOR;

        return result.Length > 1;
      }

      result = path;
      return false;
    }

    public static bool TryMakeRelativePath(string path, string root, out string result, bool explicit_relative = false)
    {
      // setup to do the thing

      result = null;

      if (!IsValidPath(path) || !IsValidPath(root))
      {
        return false;
      }

      var path_splits = path.Split(DirectorySeparators);
      var root_splits = root.Split(DirectorySeparators);

      int path_depth  = path_splits.Length;
      int root_depth  = root_splits.Length;

      if (path_depth < 2 || root_depth < 2)
      {
        return false;
      }

      if (path_depth < root_depth)
      {
        $"\"{path}\" ({path_depth}) should be a deeper path than \"{root}\" ({root_depth}).".LogError();
        return false;
      }

      // begin doing the thing

      int i = 0;
      
      // walk until all the way through, or until we find a directory that doesn't match
      while (i < root_depth && PathStringComparer.Equals(path_splits[i], root_splits[i]))
      {
        ++i;
      }

      var strb = new System.Text.StringBuilder(path.Length - root.Length);

      // if the user wants an explicitly relative path, give them one.
      if (explicit_relative)
      {
        _ = strb.Append(EXPLICIT_RELATIVE_ROOT);
      }

      // if we broke early in the last loop, prepend the correct number of parent slugs
      for (int j = root_depth; j > i; --j)
      {
        _ = strb.Append(EXPLICIT_RELATIVE_PARENT);
      }

      // finally, append the remainder of the path
      while (i < path_depth - 1)
      {
        _ = strb.Append(path_splits[i++]).Append(DIRECTORY_SEPARATOR);
      }

      result = strb.Append(path_splits[path_depth - 1]).ToString();

      return result.Length > 0;
    }



    public static bool TryStripNonExistentDirectories(string path, out string result)
    {
      if (!TryMakeAbsolutePath(path, out result))
        return false;

      if (Directory.Exists(path))
        return true;

      while (TryExtractPathHead(result, out result))
      {
        if (Directory.Exists(result))
          return true;
      }

      return false;
    }


    public static bool TryExtractPathHead(string path, out string head, bool keep_trailing_slash = true)
    {
      int trim = TrimPath(path, out head);
      if (trim < 0)
        return false;

      int slash = trim + head.LastIndexOfAny(DirectorySeparators);

      if (keep_trailing_slash)
        head = path.Remove(slash + 1);
      else if (slash < trim)
        head = path.TrimEnd(DirectorySeparators);
      else
        head = path.Remove(slash);

      return head.Length > 1;
    }

    public static bool TryExtractPathTail(string path, out string tail, bool keep_extension = true)
    {
      tail = string.Empty;

      if (path == null || path.Length < 2 || IsExplicitDirectoryPath(path))
        return false;

      int slash = 1 + path.LastIndexOfAny(DirectorySeparators);
      int dot   = path.Length;

      if (!keep_extension)
      {
        dot = path.LastIndexOf('.');

        if (dot < slash || dot - slash < 2)
          dot = path.Length;
      }

      if (dot - slash < 2) // space between dot/end and slash is too small.
        return false;

      tail = path.Substring(slash, dot - slash);
      return true;
    }


    public static bool TryDecomposePath(string path, out string head, out string tail, bool explicit_directory = true)
    {
      tail = string.Empty;

      int trim = TrimPath(path, out head);
      if (trim < 0)
        return false;

      int slash = trim + head.LastIndexOfAny(DirectorySeparators);

      if (slash > trim)
        tail = path.Substring(slash + 1);
      else
        tail = head;

      if (explicit_directory)
        head = path.Remove(slash + 1);
      else if (slash < trim)
        head = path.TrimEnd(DirectorySeparators);
      else
        head = path.Remove(slash);

      return head.Length > 1;
    }



    public static string ParseFileName(this string path, bool keep_extension = false)
    {
      if (TryExtractPathTail(path, out string result, keep_extension))
      {
        return result;
      }

      return path;
    }

    public static string ParseParentDirectory(this string path, bool keep_trailing_slash = false)
    {
      if (TryExtractPathHead(path, out string result, keep_trailing_slash))
      {
        return result;
      }

      return string.Empty;
    }


  }

}