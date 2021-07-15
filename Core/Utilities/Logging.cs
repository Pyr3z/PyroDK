/**
@file   PyroDK/Core/Utilities/Logging.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-16

@brief
  Logging qualities of life.
**/

#pragma warning disable UNT0008 // null propogation is FINE.

using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  using Type        = System.Type;
  using StackFrame  = System.Diagnostics.StackFrame;
  using MethodInfo  = System.Reflection.MethodBase;

  using StringBuilder = System.Text.StringBuilder;


  public static class Logging
  {

    /// <summary>
    /// Only prints in Debug, but still follows through with
    /// potential fix-up logic.
    /// </summary>
    /// <param name="assertion">The condition to assert is true.</param>
    /// <returns>True if the assertion fails (`condition` is false).</returns>
    public static bool Assert(bool assertion)
    {
      #if DEBUG
      if (assertion)
        return false;

      LogError<object>(DefaultAssertMessage, ctx: null);
      return true;
      #else
      return !condition;
      #endif
    }

    /// <summary>
    /// Only prints in Debug, but still follows through with
    /// potential fix-up logic.
    /// </summary>
    /// <param name="assertion">The condition to assert is true.</param>
    /// <param name="message">An additional message to print with the error trace.</param>
    /// <returns>True if the assertion fails (`condition` is false).</returns>
    public static bool Assert(bool assertion, string message)
    {
      #if DEBUG
      if (assertion)
        return false;

      LogError<object>($"{DefaultAssertMessage} \"{message}\"", ctx: null);
      return true;
      #else
      return !condition;
      #endif
    }


    public static bool AssertNonNull<T>(T obj) where T : class
    {
      #if DEBUG
      if (obj != null)
        return false;

      LogError(DefaultAssertNonNullMessage, ctx: obj);
      return true;
      #else
      return obj == null;
      #endif
    }

    public static T WarnNull<T>(T obj) where T : class
    {
      #if DEBUG
      if (obj == null)
      {
        LogWarning(DefaultAssertNonNullMessage, ctx: obj);
      }
      #endif

      return obj;
    }


    public static void Log(this string message)
    {
      Log<object>(message, ctx: null);
    }

    public static void LogSuccess(this string message)
    {
      LogSuccess<object>(message, ctx: null);
    }

    public static void LogImportant(this string message)
    {
      LogImportant<object>(message, ctx: null);
    }

    public static void LogBoring(this string message)
    {
      LogBoring<object>(message, ctx: null);
    }

    public static void LogWarning(this string message)
    {
      LogWarning<object>(message, ctx: null);
    }

    public static void LogError(this string message)
    {
      LogError<object>(message, ctx: null);
    }


    public static void LogException(this System.Exception e)
    {
      LogException<object>(e, ctx: null);
    }

    public static void LogDumbException(this System.Exception e)
    {
      LogException<object>(e, ctx: null, log_level: LogType.Log);
    }

    public static void LogDumbException<T>(this System.Exception e, T ctx)
    {
      LogException(e, ctx, log_level: LogType.Log);
    }



    private const string FMT_LOG            = "{0} {1}\n(in type {2})\n\nStackTrace:\n{3}";
    private const string FMT_LOG_NAMED_CTX  = "{0} {1}\n(\"{3}\", type {2})\n\nStackTrace:\n{4}";

    private const LogOption USE_PYRODK_STACKTRACE = LogOption.NoStacktrace;



    public static void Reached(bool is_error = false, object blame = null)
    {
      const string REACHED_MESSAGE = "<b><i>Reached!</i></b>";

      if (is_error)
      {
        LogError(REACHED_MESSAGE, ctx: blame, full_stack: false);
      }
      else
      {
        LogImportant(REACHED_MESSAGE, ctx: blame, full_stack: false);
      }
    }


    public static void ShouldNotReach(object blame = null)
    {
      LogError(ShouldNotReachMessage, ctx: blame, full_stack: false);
    }

    public static void TempReached(object blame = null)
    {
      LogImportant(TempReachedMessage, ctx: blame, full_stack: true);
    }

    public static void TempReachedGood(object blame = null)
    {
      LogSuccess(TempReachedMessage, ctx: blame, full_stack: true);
    }


    public static void WarnReached(object blame = null)
    {
      LogWarning(WarnReachedMessage, ctx: blame, full_stack: true);
    }



    public static void Log<T>(this string message,  T       ctx,
                                                    bool    full_stack  = false,
                                                    LogType logtype     = LogType.Log,
                                                    string  logtag      = null,
                                                    Color32 type_color  = default)
    {
      if (type_color.IsClear())
        type_color = Colors.GUI.TypeByRef;

      string stacktrace_str;
      if (ctx is Type type_ctx)
      {
               _ = GetFormattedStackTrace(full_stack, type_color, typeof(Logging), out stacktrace_str);
      }
      else
      {
        type_ctx = GetFormattedStackTrace(full_stack, type_color, typeof(Logging), out stacktrace_str);
      }

      string name;
      if (ctx is Object obj_ctx)
      {
        if (obj_ctx)
          name = obj_ctx.name;
        else
          name = "(Destroyed)";
      }
      else
      {
        obj_ctx = null;
        name    = ctx?.ToString();
      }

      if (name.IsEmpty())
      {
        Debug.LogFormat(logtype, USE_PYRODK_STACKTRACE, obj_ctx, FMT_LOG,
                        logtag ?? LogTag,
                        RichText.ColorStringLiterals(message),
                        RichText.TypeName(type_ctx, type_color),
                        stacktrace_str);
      }
      else
      {
        Debug.LogFormat(logtype, USE_PYRODK_STACKTRACE, obj_ctx, FMT_LOG_NAMED_CTX,
                        logtag ?? LogTag,
                        RichText.ColorStringLiterals(message),
                        RichText.TypeName(type_ctx, type_color),
                        name,
                        stacktrace_str);
      }
    }

    public static void LogSuccess<T>(this string message, T ctx, bool full_stack = false)
    {
      Log<T>(message, ctx, full_stack, logtag: LogSuccessTag);
    }

    public static void LogImportant<T>(this string message, T ctx, bool full_stack = false)
    {
      Log<T>(message, ctx, full_stack, LogType.Warning, logtag: LogImportantTag);
    }

    public static void LogBoring<T>(this string message, T ctx, bool full_stack = false)
    {
      Log<T>( RichText.Make(message, LOG_TAG_BORING_STYLE, Colors.Debug.Boring),
              ctx, full_stack,
              logtag:     LogBoringTag,
              type_color: Colors.GUI.TypeByRef.Alpha(0x88));
    }

    public static void LogWarning<T>(this string message, T ctx, bool full_stack = true)
    {
      Log<T>( message, ctx, full_stack,
              logtype:  LogType.Warning,
              logtag:   LogWarningTag);
    }

    public static void LogError<T>(this string message, T ctx, bool full_stack = true)
    {
      Log<T>( message, ctx,
              full_stack: full_stack,
              logtype:    LogType.Error,
              logtag:     LogErrorTag,
              type_color: Colors.Debug.Attention);
    }


    public static void LogException<T>(this System.Exception e, T ctx, LogType log_level = LogType.Exception)
    {
      var    type_color     = Colors.Debug.Error;
      string stacktrace_str = string.Empty;

      if (!(ctx is Type type_ctx))
      {
        if (typeof(T) == typeof(object))
          type_ctx = GetFormattedStackTrace(true, type_color, typeof(Logging), out stacktrace_str);
        else
          type_ctx = typeof(T);
      }

      if (stacktrace_str.IsEmpty())
      {
        _ = GetFormattedStackTrace(true, type_color, typeof(Logging), out stacktrace_str);
      }

      string name = null;

      if (ctx is Object obj_ctx)
      {
        if (obj_ctx)
          name = obj_ctx.name;
        else
          name = "(Missing)";
      }
      else
      {
        obj_ctx = null;

        if (ctx is string str_ctx)
          name = str_ctx;
      }

      string tag;
      switch (log_level)
      {
        case LogType.Log:
          tag = LogTag;
          break;

        case LogType.Assert:
        case LogType.Warning:
          tag = LogWarningTag;
          break;

        default:
          tag = LogErrorTag;
          break;
      }

      if (name.IsEmpty())
      {
        Debug.LogFormat(log_level, USE_PYRODK_STACKTRACE, obj_ctx, FMT_LOG,
                        tag,
                        GetFormattedExceptionMessage(e, type_color),
                        RichText.TypeName(type_ctx, type_color),
                        stacktrace_str);
      }
      else
      {
        Debug.LogFormat(log_level, USE_PYRODK_STACKTRACE, obj_ctx, FMT_LOG_NAMED_CTX,
                        tag,
                        GetFormattedExceptionMessage(e, type_color),
                        RichText.TypeName(type_ctx, type_color),
                        name,
                        stacktrace_str);
      }
    }


    public static StackFrame GetCallingStackFrame(bool get_file_info, Type skip_top)
    {
      if (skip_top == null)
      {
        return new StackFrame(skipFrames: 2, fNeedFileInfo: get_file_info);
      }

      var stacktrace = new System.Diagnostics.StackTrace(skipFrames: 2, fNeedFileInfo: get_file_info);

      int i = 0, ilen = stacktrace.FrameCount;
      if (MAX_STACKTRACE_DEPTH < ilen)
        ilen = MAX_STACKTRACE_DEPTH;

      while (i < ilen)
      {
        var frame  = stacktrace.GetFrame(i++);
        var method = frame.GetMethod();

        if (method == null || method.DeclaringType == null || method.DeclaringType == skip_top)
          continue;

        return frame;
      }

      return null;
    }


    public static string GetCallingMethodName()
    {
      var frame = GetCallingStackFrame(get_file_info: false, skip_top: typeof(Logging));
      if (frame == null)
        return "null";
      return RichText.Signature(frame.GetMethod());
    }


    private static string MakeStackFrameLink(StackFrame frame)
    {
      if (frame == null || !Filesystem.TryMakeRelativePath(frame.GetFileName(),
                                                           Filesystem.ProjectRoot,
                                                           out string file))
      {
        return string.Empty;
      }

      return $"(at {file}:{frame.GetFileLineNumber()})";
    }



    [SerializeStatic]
    private static string LOG_TAG_INFIX = "PyroDK";

    [SerializeStatic]
    private static int MAX_STACKTRACE_DEPTH = 16;

    [SerializeStatic]
    private static RichText.Style LOG_TAG_STYLE         = RichText.Style.BraceHard;
    [SerializeStatic]
    private static RichText.Style LOG_TAG_BORING_STYLE  = RichText.Style.BraceSoft | RichText.Style.Italic;


    private static string s_LogTagRT = null;
    private static string LogTag
    {
      get
      {
        if (s_LogTagRT == null)
          return s_LogTagRT = RichText.Make(LOG_TAG_INFIX, LOG_TAG_STYLE, Colors.Debug.Log);
        return s_LogTagRT;
      }
    }

    private static string s_LogSuccessTagRT = null;
    private static string LogSuccessTag
    {
      get
      {
        if (s_LogSuccessTagRT == null)
          return s_LogSuccessTagRT = $"{LogTag} {RichText.Color("(^_^)", Colors.Debug.Success)}";
        return s_LogSuccessTagRT;
      }
    }

    private static string s_LogImportantTagRT = null;
    private static string LogImportantTag
    {
      get
      {
        if (s_LogImportantTagRT == null)
          return s_LogImportantTagRT = RichText.Make(LOG_TAG_INFIX, LOG_TAG_STYLE, Colors.Debug.Important);
        return s_LogImportantTagRT;
      }
    }

    private static string s_LogBoringTagRT = null;
    private static string LogBoringTag
    {
      get
      {
        if (s_LogBoringTagRT == null)
          return s_LogBoringTagRT = RichText.Make(LOG_TAG_INFIX, LOG_TAG_BORING_STYLE, Colors.Debug.Boring);
        return s_LogBoringTagRT;
      }
    }

    private static string s_LogWarningTagRT = null;
    private static string LogWarningTag
    {
      get
      {
        if (s_LogWarningTagRT == null)
          return s_LogWarningTagRT = RichText.Make(LOG_TAG_INFIX, LOG_TAG_STYLE, Colors.Debug.Warning);
        return s_LogWarningTagRT;
      }
    }

    private static string s_LogErrorTagRT = null;
    private static string LogErrorTag
    {
      get
      {
        if (s_LogErrorTagRT == null)
          return s_LogErrorTagRT = RichText.Make(LOG_TAG_INFIX, LOG_TAG_STYLE, Colors.Debug.Error);
        return s_LogErrorTagRT;
      }
    }


    private static string s_DefaultAssertMessageRT = null;
    private static string DefaultAssertMessage
    {
      get
      {
        if (s_DefaultAssertMessageRT == null)
          return s_DefaultAssertMessageRT = RichText.Make("Assertion failed!",
                                                          RichText.Style.Bold,
                                                          Colors.Debug.Attention);
        return s_DefaultAssertMessageRT;
      }
    }

    private static string s_DefaultAssertNonNullMessageRT = null;
    private static string DefaultAssertNonNullMessage
    {
      get
      {
        if (s_DefaultAssertNonNullMessageRT == null)
          return s_DefaultAssertNonNullMessageRT = RichText.Make("Non-null assertion failed!",
                                                                 RichText.Style.Bold,
                                                                 Colors.Debug.Attention);
        return s_DefaultAssertNonNullMessageRT;
      }
    }


    private static string s_ShouldNotReachMessageRT = null;
    private static string ShouldNotReachMessage
    {
      get
      {
        if (s_ShouldNotReachMessageRT == null)
          return s_ShouldNotReachMessageRT = RichText.Make( "SHOULD NOT HAVE REACHED HERE!",
                                                            RichText.Style.Bold,
                                                            Colors.Debug.Error);
        return s_ShouldNotReachMessageRT;
      }
    }


    private static string s_TempReachedMessageRT = null;
    private static string TempReachedMessage
    {
      get
      {
        if (s_TempReachedMessageRT == null)
          return s_TempReachedMessageRT = RichText.Make("TEMPORARY REACHED MARKER",
                                                        RichText.Style.Italic | RichText.Style.BraceSoft,
                                                        Colors.Debug.Pending);
        return s_TempReachedMessageRT;
      }
    }


    private static string s_WarnReachedMessageRT = null;
    private static string WarnReachedMessage
    {
      get
      {
        if (s_WarnReachedMessageRT == null)
          return s_WarnReachedMessageRT = RichText.Make("Unwanted code reached!",
                                                        RichText.Style.Bold,
                                                        Colors.Debug.Warning);
        return s_WarnReachedMessageRT;
      }
    }



    static Logging()
    {
      SerializeStaticFields.OnNewFieldsApplied += RecacheLogStringTags;
    }
    private static void RecacheLogStringTags()
    {
      s_LogTagRT          =
      s_LogImportantTagRT =
      s_LogSuccessTagRT   =
      s_LogBoringTagRT    =
      s_LogWarningTagRT   =
      s_LogErrorTagRT     = null;
    }


    private static Type GetFormattedStackTrace(bool full, Color32 type_color, Type skip_top, out string trace_str)
    {
      Type top_type   = skip_top;
      var  stacktrace = new System.Diagnostics.StackTrace(skipFrames: 2, fNeedFileInfo: true);
      var  bob        = new StringBuilder(255);

      int i = 0, ilen = stacktrace.FrameCount;

      if (MAX_STACKTRACE_DEPTH <= 0)
        full = false;
      else if (MAX_STACKTRACE_DEPTH < ilen)
        ilen = MAX_STACKTRACE_DEPTH;

      string fmt_index_log = Integers.MakeIndexPreformattedString(ilen) + ": ";

      var was_user_ass = TriBool.Null;
      for (; i < ilen; ++i)
      {
        var frame  = stacktrace.GetFrame(i);
        var method = frame.GetMethod();

        if (method == null || method.DeclaringType == null || method.DeclaringType == skip_top)
          continue;

        if (top_type == skip_top)
        {
          top_type = method.DeclaringType;
          skip_top = null;
        }

        // differentiate between User vs. Non-User Assemblies:
        bool is_user_ass = method.DeclaringType.Assembly.IsUserAssembly();
        
        if (was_user_ass == TriBool.False && is_user_ass)
        {
          bob.CloseRichText(RichText.Style.Italic, Colors.Debug.Boring)
             .AppendEmphasis(" <-- RESUME USER CODE")
             .Append('\n');
        }
        else if (was_user_ass == TriBool.True && !is_user_ass)
        {
          bob.AppendEmphasis("NON-USER CODE -->")
             .OpenRichText(RichText.Style.Italic, Colors.Debug.Boring)
             .Append('\n');
        }

        was_user_ass = is_user_ass;

        // print the index bullet:
        bob.AppendFormat(fmt_index_log, i);

        //bob.OpenRichText(type_color);

        // and now the method's signature, if it's different from top:
        if (!method.DeclaringType.Namespace.IsEmpty() &&
            method.DeclaringType.Namespace != top_type.Namespace)
        {
          bob.Append(method.DeclaringType.Namespace)
             .Append('.');
        }

        bob.AppendSignature(method);

        //bob.CloseRichText(type_color);

        // create file and lineno link: (this format very specific must be like this...)
        bob.Append(' ').Append(MakeStackFrameLink(frame));

        // only do one stack frame if not full
        if (!full)
          break;

        bob.Append('\n');
      }

      // backup RichText close:
      if (was_user_ass == TriBool.False)
      {
        bob.CloseRichText(RichText.Style.Italic, Colors.Debug.Boring);
      }

      trace_str = bob.ToString();
      return top_type;
    }

    private static string GetFormattedExceptionMessage(System.Exception e, Color32 type_color)
    {
      var bob             = new StringBuilder(capacity: 255);
      var nested_messages = new List<string>(4);

      while (e != null)
      {
        if (e.InnerException != null)
        {
          bob.Append("└─> Rethrow as ");
        }

        bob.Append(RichText.TypeName(e.GetType(), type_color))
           .Append(": ");

        if (e.Message.IsEmpty())
        {
          bob.Append("(no message)");
        }
        else
        {
          bob.AppendColoredStringLiterals(e.Message);
        }

        if (e.InnerException == null && !e.StackTrace.IsEmpty())
        {
          bob.Append("\nException StackTrace:\n").Append(e.StackTrace);
        }

        nested_messages.Add(bob.ToString());
        bob.Clear();

        e = e.InnerException;
      }

      for (int i = nested_messages.Count - 1; i >= 0; --i)
      {
        bob.Append(nested_messages[i]);

        if (i > 0)
          bob.Append('\n');
      }

      return bob.ToString();
    }



    #if UNITY_EDITOR
    [UnityEditor.MenuItem("PyroDK/Debug Info Loggers/Test \"Logging.Reached()\"", priority = 50)]
    private static void MenuTestReached()
    {
      Reached();
    }
    #endif

  }

}