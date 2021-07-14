/**
@file   PyroDK/Core/Attributes/ScriptOrderAttribute.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-09

@brief
  Defines the C# Attribute [ScriptOrder(int)], which should be applied to
  MonoBehaviour Component classes in order to define their execution order
  easily in code.
**/


namespace PyroDK
{

  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class ScriptOrderAttribute : System.Attribute
  {
    private readonly int  m_ExecutionOrder;
    private readonly bool m_Inherit;

    public ScriptOrderAttribute(int exec_order, bool inherit = false)
    {
      m_ExecutionOrder  = exec_order;
      m_Inherit         = inherit;
    }


    #if UNITY_EDITOR
    #pragma warning disable IDE0051 // `ApplyAll()` is called with reflection.

    [UnityEditor.InitializeOnLoadMethod]
    private static void ApplyAll()
    {
      foreach (var script in UnityEditor.MonoImporter.GetAllRuntimeMonoScripts())
      {
        var script_t = script.GetClass();
        if (script_t == null)
          continue;

        foreach (ScriptOrderAttribute attr in GetCustomAttributes(element:  script_t,
                                                                  type:     typeof(ScriptOrderAttribute),
                                                                  inherit:  true))
        {
          if (!attr.m_Inherit && script_t.IsDefined(typeof(ScriptOrderAttribute), false))
            continue;

          int curr_order = UnityEditor.MonoImporter.GetExecutionOrder(script);
          int next_order = attr.m_ExecutionOrder;

          if (curr_order != next_order)
          {
            try
            {
              UnityEditor.MonoImporter.SetExecutionOrder(script, next_order);
            }
            catch (System.Exception e)
            {
              e.LogException( $"Potentially failed to set [ScriptOrder] of \"{script}\"; check the editor.",
                              UnityEngine.LogType.Warning);
            }
          }
        }
      }
    }

    #endif // UNITY_EDITOR

  }

}
