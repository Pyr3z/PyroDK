/**
@file   PyroDK/Core/DataTypes/FilePath.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-07

@brief
  Defines the type `FilePath`, which is used for dealing with paths on the
  user's filesystem, and for storing these paths as serializable and validated
  fields.
**/

using UnityEngine;


namespace PyroDK
{

  using Directory = System.IO.Directory;
  using File      = System.IO.File;


  [System.Flags]
  public enum FilePathFlags : int
  {
    Invalid     = (1 << 31),
    None        = (0 <<  0),

    File        = (1 <<  0), // If has both File and Directory flags,
    Directory   = (1 <<  1), // it's a valid but otherwise unused path.

    Exists      = (1 <<  2),

    Relative    = (1 <<  3),
    Extension   = (1 <<  4),
    
    Asset       = (1 <<  5),
    Hierarchy   = (1 <<  6), // Unused

    ReadOnly    = (1 <<  7), // Editor
    DirsOnly    = (1 <<  8), // Editor
    FileOnly    = (1 <<  9), // Editor

    EditorMask  = ( ReadOnly | DirsOnly | FileOnly ),
    RuntimeMask = ~EditorMask,
  }


  [System.Serializable]
  public sealed class FilePath :
    System.IEquatable<FilePath>,
    System.IEquatable<string>,
    ICloneable<FilePath>,
    ISerializationCallbackReceiver
  {

    public static readonly FilePath Empty = new FilePath(trash: true)
    {
      m_Path  = string.Empty,
      m_Flags = FilePathFlags.Invalid
    };


    public FilePathFlags RuntimeFlags
    {
              get => m_Flags & FilePathFlags.RuntimeMask;
      private set => m_Flags = EditorFlags | ( value & FilePathFlags.RuntimeMask );
    }
    public FilePathFlags EditorFlags
    {
      get => m_Flags & FilePathFlags.EditorMask;
      set => m_Flags = RuntimeFlags | ( value & FilePathFlags.EditorMask );
    }

    public int    RawFlags  => (int)m_Flags;
    public string RawString => m_Path;


    public bool IsValid
    {
      get => !m_Flags.HasFlag(FilePathFlags.Invalid);
      set => m_Flags = m_Flags.SetBits(FilePathFlags.Invalid, !value);
    }

    public bool IsFile        => m_Flags.HasFlag(FilePathFlags.File);
    public bool IsDirectory   => m_Flags.HasFlag(FilePathFlags.Directory);

    public bool IsAsset       => m_Flags.HasFlag(FilePathFlags.Asset);

    public bool Exists        => m_Flags.HasFlag(FilePathFlags.Exists);

    public bool HasExtension  => m_Flags.HasFlag(FilePathFlags.Extension);


    [SerializeField]
    private FilePathFlags m_Flags;

    [SerializeField]
    private string        m_Path;


    public static FilePath MakeAssetPath(string filename)
    {
      return new FilePath($"Assets/{filename}");
    }



    public FilePath()
    {
      m_Flags = FilePathFlags.None;
      m_Path  = "";
    }
    public FilePath(string path)
    {
      m_Flags = Filesystem.FixUpRawPath(ref path);
      m_Path  = path;
    }
    private FilePath(bool trash)
    {
    }


    public FilePath MakeReadOnly()
    {
      m_Flags |= FilePathFlags.ReadOnly;
      return this;
    }

    public FilePath RestrictToDirectories()
    {
      if (m_Flags.HasFlag(FilePathFlags.File))
        Clear();

      m_Flags |= FilePathFlags.DirsOnly;

      return this;
    }

    public FilePath RestrictToFiles()
    {
      if (m_Flags.HasFlag(FilePathFlags.Directory))
        Clear();

      m_Flags |= FilePathFlags.FileOnly;

      return this;
    }



    public string GetFilename(bool include_ext = true)
    {
      if (Filesystem.TryExtractPathTail(m_Path, out string result, include_ext))
        return result;
      return string.Empty;
    }

    public string GetExtension()
    {
      return HasExtension ? m_Path.Substring(m_Path.LastIndexOf('.') + 1) : string.Empty;
    }

    public FilePath GetParentPath(bool leave_trailing_slash = false)
    {
      if (Filesystem.TryExtractPathHead(m_Path, out string parent_dir, leave_trailing_slash))
      {
        var result = new FilePath(trash: true)
        {
          m_Path  = parent_dir,
          m_Flags = m_Flags.ClearBits(FilePathFlags.File).SetBits(FilePathFlags.Directory)
        };

        _ = result.RecheckExists();
        return result;
      }

      return Empty;
    }



    public bool Set(string path)
    {
      var flags = Filesystem.FixUpRawPath(ref path, EditorFlags);

      if (flags.HasFlag(FilePathFlags.Invalid) || (flags == RuntimeFlags && path == m_Path))
        return false;
      
      m_Path  = path;
      m_Flags = flags | EditorFlags;
      return true;
    }

    public bool SetLax(string path) // subtle difference: this one allows invalid paths to be set
    {
      var flags = Filesystem.FixUpRawPath(ref path, EditorFlags);

      if (flags == RuntimeFlags && path == m_Path)
        return false;

      m_Path  = path;
      m_Flags = flags | EditorFlags;
      return true;
    }

    public void SetRaw(FilePathFlags flags, string path)
    {
      m_Flags = flags;
      m_Path  = path;
    }


    public void Clear()
    {
      m_Path  = string.Empty;
      m_Flags = EditorFlags;
    }

      

    public bool RecheckExists()
    {
      if (!IsValid)
        return false;

      if (m_Flags.HasFlag(FilePathFlags.File) && File.Exists(m_Path))
      {
        m_Flags = m_Flags.SetBits(FilePathFlags.Exists).ClearBits(FilePathFlags.Directory);
        return true;
      }

      if (m_Flags.HasFlag(FilePathFlags.Directory) && Directory.Exists(m_Path))
      {
        m_Flags = m_Flags.SetBits(FilePathFlags.Exists).ClearBits(FilePathFlags.File);
        return true;
      }

      m_Flags = m_Flags.ClearBits(FilePathFlags.Exists).SetBits(FilePathFlags.File | FilePathFlags.Directory);
      return false;
    }



    public override string ToString()
    {
      if (IsValid)
        return $"\"{m_Path}\"";

      return $"BAD PATH: \"{m_Path}\"";
    }

    public override int GetHashCode()
    {
      return Hashing.MakeHash(m_Path, m_Flags);
    }

    public override bool Equals(object obj)
    {
      return obj != null && Equals(obj.ToString());
    }

    public bool Equals(FilePath other)
    {
      return other != null && ( this == other || (m_Flags == other.m_Flags && m_Path == other.m_Path) );
    }

    public bool Equals(string path)
    {
      if (this.IsEmpty())
        return path.IsEmpty();

      if (path.IsEmpty())
        return false;

      // TODO test this for odd cases and efficiency.
      return GetHashCode() == Hashing.MakeHash(path, Filesystem.FixUpRawPath(ref path));
    }

    public bool EqualsUnfixed(string other)
    {
      return m_Path == other;
    }


    public FilePath Clone()
    {
      return new FilePath(trash: true)
      {
        m_Path  = this.m_Path,
        m_Flags = this.m_Flags
      };
    }

    public void CloneTo(ref FilePath other)
    {
      if (Equals(other, null))
      {
        other = Clone();
      }
      else
      {
        other.m_Path  = m_Path;
        other.m_Flags = m_Flags;
      }
    }


    object System.ICloneable.Clone()
    {
      return Clone();
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
      RuntimeFlags = Filesystem.GetPathFlags(m_Path);
      
      if (IsValid)
      {
        m_Path = m_Path.Replace(Filesystem.BAD_DIRECTORY_SEPARATOR, Filesystem.DIRECTORY_SEPARATOR);
      }
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
    }



    public static implicit operator string (FilePath path)
    {
      return path?.m_Path ?? string.Empty;
    }

    public static implicit operator FilePath (string path)
    {
      return path.IsEmpty() ? null : new FilePath(path);
    }

  } // end class FilePath


  public static partial class Filesystem
  {

    public static bool IsEmpty(this FilePath path)
    {
      return path == null || path.RawString.IsEmpty();
    }


    public static FilePathFlags GetPathFlags(string path)
    {
      if (!IsValidPath(path))
        return FilePathFlags.Invalid;

      var result = FilePathFlags.Directory;

      if (IsExplicitDirectoryPath(path))
      {
        if (Directory.Exists(path))
          result |= FilePathFlags.Exists;
      }
      else if (TryExtractPathTail(path, out string file))
      {
        if (File.Exists(path))
          result = FilePathFlags.Exists | FilePathFlags.File;
        else if (Directory.Exists(path))
          result = FilePathFlags.Exists | FilePathFlags.Directory;
        else if (IsValidFileName(file))
          result = FilePathFlags.File | FilePathFlags.Directory;

        if (result.HasFlag(FilePathFlags.File) && ExtractFileExtension(file).Length > 0)
          result = (result & ~FilePathFlags.Directory) | FilePathFlags.Extension;
      }

      if (IsAssetPath(path))
        result |= FilePathFlags.Asset;

      if (!IsAbsolutePath(path))
        result |= FilePathFlags.Relative;

      return result;
    }

    public static FilePathFlags FixUpRawPath(ref string path, FilePathFlags preflags = FilePathFlags.None)
    {
      var result = GetPathFlags(path);

      if (!result.HasFlag(FilePathFlags.Invalid))
      {
        if (result.HasFlag(FilePathFlags.Asset) && !TryMakeAssetPath(ref path))
        {
          result &= ~FilePathFlags.Asset;
        }

        if (preflags.HasFlag(FilePathFlags.DirsOnly))
        {
          if (!result.HasFlag(FilePathFlags.Directory))
            result |= FilePathFlags.Invalid;
        }
        else if (preflags.HasFlag(FilePathFlags.FileOnly))
        {
          if (!result.HasFlag(FilePathFlags.File))
            result |= FilePathFlags.Invalid;
        }
      }

      return result | preflags;
    }

  } // end static partial class Filesystem

}