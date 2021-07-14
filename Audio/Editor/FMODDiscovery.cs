/**
@file   PyroDK/Audio/Editor/FMODDiscovery.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-11-11

@brief
  Detects if FMODUnity is included in the project correctly,
  and adds "FMOD" as a preprocessor define accordingly.
**/

#if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;

using UnityEditor;


namespace PyroDK.Audio
{

  internal static class FMODDiscovery
  {

    public static bool Found => s_Found;


    private static bool s_Found = false;


    [InitializeOnLoadMethod]
    private static void OnEditorReload()
    {
      var build_targ = EditorUserBuildSettings.selectedBuildTargetGroup;
      var curr_defs  = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(build_targ).Split(';'));

      s_Found = Assemblies.Find("FMODUnity", out _ );

      if (curr_defs.Contains("FMOD"))
      {
        if (!s_Found)
        {
          "\"FMOD\" was defined, but no Assembly w/ \"FMODUnity\" in its name could be found."
            .Log();

          curr_defs.Remove("FMOD");
          PlayerSettings.SetScriptingDefineSymbolsForGroup(build_targ, string.Join(";", curr_defs));
        }
      }
      else if (s_Found)
      {
        "An Assembly w/ \"FMODUnity\" in its name was found; adding \"FMOD\" to the preprocessor defs."
          .Log();

        curr_defs.Add("FMOD");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(build_targ, string.Join(";", curr_defs));
      }
    }

  }

}

#endif