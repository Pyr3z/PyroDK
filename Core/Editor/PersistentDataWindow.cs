/**
@file   PyroDK/Core/Editor/PersistentDataWindow.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-26

@brief
  Draws the Editor window for viewing, editing, and otherwise
  dealing with custom persistent data structures provided by
  PyroDK.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public class PersistentDataWindow : EditorWindow
  {

    public static PersistentDataWindow Instance
    {
      get
      {
        if (s_Instance == null)
          s_Instance = GetWindow<PersistentDataWindow>();

        return s_Instance;
      }
    }
    private static PersistentDataWindow s_Instance = null;
    

    [MenuItem("PyroDK/Window/Persistent Data Inspector")]
    private static void MenuItemShow()
    {
      s_Instance = GetWindow<PersistentDataWindow>();
      s_Instance.titleContent = new GUIContent("Persistent Data");
      s_Instance.Show();
    }

  }

}