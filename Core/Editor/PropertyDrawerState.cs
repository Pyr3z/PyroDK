/**
@file   PyroDK/Core/Editor/PropertyDrawerState.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2021-05-14

@brief
  Base and static utility class for managing state data
  between PropertyDrawer instances.
**/

using UnityEditor;


namespace PyroDK.Editor
{

  public abstract class PropertyDrawerState
  {

  #region STATIC SECTION

    public static void Restore<TState>(SerializedProperty prop_root, out TState state)
      where TState : PropertyDrawerState, new()
    {
      uint id = prop_root.GetPropertyHash();

      if (s_StateMap.Find(id, out state) && state != null)
      {
        if (state.NeedsUpdate)
        {
          state.Update(prop_root);
          state.IsStale = false;
        }
        else
        {
          state.RenewLifeSpan();
          state.IsStale = true;
        }
      }
      else // emplace a newly constructed state
      {
        s_StateMap[id] = state = new TState();
        state.Update(prop_root);

        if (s_InspectorTracker == null)
        {
          EditorApplication.update += TickStaleStates;
          s_InspectorTracker = ActiveEditorTracker.sharedTracker;
        }
      }
    }


    private static readonly HashMap<uint, PropertyDrawerState> s_StateMap = new HashMap<uint, PropertyDrawerState>();
    private static ActiveEditorTracker s_InspectorTracker = null;

    private static void TickStaleStates()
    {
      _ = s_StateMap.ClearSelective(TickedStateIsExpired);

      if (s_StateMap.Count == 0)
      {
        EditorApplication.update -= TickStaleStates;
        s_InspectorTracker = null;
      }
    }

    private static bool TickedStateIsExpired(uint _ , PropertyDrawerState state)
    {
      return --state.m_LifeSpan <= 0 &&
             ( state.NeedsUpdate || s_InspectorTracker.activeEditors.Length == 0 );
    }

  #endregion STATIC SECTION


    public bool NeedsUpdate => m_RootProp.IsDisposed();
    public bool IsStale { get; protected set; }


    protected SerializedProperty m_RootProp;

    private int m_LifeSpan;


    protected abstract void UpdateDetails();

    public void Update(SerializedProperty prop_root)
    {
      m_RootProp = prop_root;
      UpdateDetails();
      RenewLifeSpan();
    }

    public void RenewLifeSpan() => m_LifeSpan = 100;

  } // end abstract class PropertyDrawerState

}