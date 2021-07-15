/**
@file   PyroDK/Core/Editor/Inspectors/ComponentInspector.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-09

@brief
  Override the default inspector editor behaviour.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class Inspectors
  {

    [CustomEditor(typeof(BaseComponent), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    private class ComponentInspector : BaseInspector
    {

      [System.NonSerialized]
      private Spawnable m_Spawnable = null;


      public override bool RequiresConstantRepaint()
      {
        // TODO determine if this is OK to use en masse
        return true;
      }

      public override void OnInspectorGUI()
      {
        m_Spawnable = target as Spawnable;

        if (m_Spawnable == null)
          DrawPyroInspector(serializedObject);
        else
          DrawPyroInspector(serializedObject, InjectSpawnableButtons);
      }


      private bool InjectSpawnableButtons(SerializedProperty prop_it)
      {
        var rect = GUILayoutUtility.GetRect(GUIDrawers.STD_LINE_HEIGHT,
                                            GUIDrawers.STD_LINE_HEIGHT,
                                            GUILayout.ExpandWidth(true));
        if (m_Spawnable.Owner)
        {
          EditorGUI.ObjectField(rect, "Owned By:", m_Spawnable.Owner, typeof(SpawnPool), allowSceneObjects: false);
        }
        else
        {
          GUI.Label(rect, $"(Not owned by any {TSpy<SpawnPool>.LogName})", Styles.LabelDetail);
        }

        rect = GUILayoutUtility.GetRect(GUIDrawers.STD_LINE_HEIGHT,
                                        GUIDrawers.STD_LINE_HEIGHT,
                                        GUILayout.ExpandWidth(true));
        if (m_Spawnable.Spawner)
        {
          EditorGUI.ObjectField(rect, "Spawned By:", m_Spawnable.Spawner, typeof(GameObject), allowSceneObjects: true);
        }
        else
        {
          GUI.Label(rect, "(No Spawner — manually instanced)", Styles.LabelDetail);
        }

        rect = GUILayoutUtility.GetRect(GUIDrawers.STD_LINE_HEIGHT,
                                        GUIDrawers.STD_LINE_HEIGHT,
                                        GUILayout.ExpandWidth(true));

        float xmax = rect.xMax;
        rect.width = ( rect.width - GUIDrawers.STD_PAD ) / 2f;

        bool is_spawned = m_Spawnable.gameObject.activeSelf;

        Labels.Button.text = "Force SPAWN";

        EditorGUI.BeginDisabledGroup(is_spawned || !EditorApplication.isPlaying);

        if (GUI.Button(rect, Labels.Button, Styles.Button) &&
            m_Spawnable.TryForceSpawnState(force_state: true))
        {
          is_spawned = true;

          $"Force Spawning \"{target.name}\"."
            .LogBoring();
        }

        EditorGUI.EndDisabledGroup();

        rect.x = rect.xMax + GUIDrawers.STD_PAD;
        rect.xMax = xmax;

        Labels.Button.text = "Force DESPAWN";

        EditorGUI.BeginDisabledGroup(!is_spawned || !EditorApplication.isPlaying);
          
        if (GUI.Button(rect, Labels.Button, Styles.Button) &&
            m_Spawnable.TryForceSpawnState(force_state: false))
        {
          $"Force Despawning \"{target.name}\"."
            .LogBoring();
        }
          
        EditorGUI.EndDisabledGroup();

        GUIDrawers.LayoutSeparator();

        return false;
      }

    }

  }

}