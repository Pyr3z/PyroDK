/**
@file   PyroDK/Core/Attributes/SanityTestAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-30

@brief
  Attribute for marking a method as a [SanityTest].
**/

using System.Collections.Generic;
using System.Reflection;


namespace PyroDK
{
  using Action      = System.Action;
  using TimeSpan    = System.TimeSpan;
  using Stopwatch   = System.Diagnostics.Stopwatch;
  using ResultList  = List<(string name, System.TimeSpan time)>;


  [System.Obsolete("Use Unity's Test Framework instead.")]
  [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public class SanityTestAttribute : System.Attribute
  {
    public string   Name  { get; set; }
    public int      Order { get; set; }

    public SanityTestAttribute(int order = 0, string name = null)
    {
      Order = order;
      Name  = name;
    }
  }

  #if UNITY_EDITOR && false


  public static class SanityTests
  {

    public static bool CheckMethod(MethodInfo minfo)
    {
      return minfo.IsDefined<SanityTestAttribute>(true);
    }


    
    private static readonly List<Action> s_AllTests = new List<Action>();

    static SanityTests()
    {
      foreach (var type in Assemblies.GetAllUserTypes())
      {
        if (Methods.TryGetMethodsAsActions(type, ref s_AllTests, where: CheckMethod))
        {
        }
      }  
    }



    //[UnityEditor.MenuItem("PyroDK/Tests/Sanity", false)]
    public static void Run()
    {
      var duration = RunSpeedTestsQuiet(s_AllTests, 1, out ResultList results);

      var strb = new System.Text.StringBuilder($"Ran a total of {results.Count} [SanityTest]s in {duration.Ticks} ticks.\n");

      foreach (var (test, time) in results)
      {
        strb.AppendLine($"{test}(); <color=green>// {time.Ticks} ticks</color>");
      }

      if (duration == TimeSpan.Zero)
        strb.ToString().Log();
      else
        strb.ToString().LogSuccess();
    }

    //[UnityEditor.MenuItem("PyroDK/Tests/Sanity", true)]
    public static bool CanRun()
    {
      return s_AllTests.Count > 0;
    }


    public static TimeSpan RunSpeedTestsQuiet(IList<Action> tests,
                                              int           runs,
                                          out ResultList    sorted_results,
                                              bool          average = true)
    {
      if (tests == null || tests.Count < 1 || runs < 1)
      {
        sorted_results = null;
        return TimeSpan.Zero;
      }

      int count = tests.Count;
      var watch = new Stopwatch();
      var duration = TimeSpan.Zero;

      sorted_results = new ResultList(count);

      foreach (var test in tests)
      {
        int i = runs;

        watch.Restart();

        while (i-- > 0)
          test.Invoke();

        watch.Stop();

        if (average)
          sorted_results.Add((test.Method.GetLogName(), new TimeSpan(watch.ElapsedTicks / runs)));
        else
          sorted_results.Add((test.Method.GetLogName(), watch.Elapsed));

        duration += watch.Elapsed;
      }

      sorted_results.Sort((lhs, rhs) => lhs.time.CompareTo(rhs.time));

      return duration;
    }

  }

  #endif // UNITY_EDITOR

}