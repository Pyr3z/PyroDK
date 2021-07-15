/**
@file   PyroDK/Core/Editor/GUIDrawers/PyroEventDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-10-30

@brief
  Drawer(s) for "PyroEvent"s.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class GUIDrawers
  {

    [CustomPropertyDrawer(typeof(PyroEvent),    useForChildren: true)]
    [CustomPropertyDrawer(typeof(PyroEvent<>),  useForChildren: true)]
    [CustomPropertyDrawer(typeof(DelayedEvent), useForChildren: true)]
    private class PyroEventDrawer : UnityEditorInternal.UnityEventDrawer
    {
      private const string UNITYEVENT_LAST_PROPERTY = "m_PersistentCalls";
      private const float  UNEXPANDED_HEIGHT = STD_LINE_HEIGHT + STD_PAD;

      private new sealed class State : PropertyDrawerState
      {
        public IPyroEvent Event;
        public IHaveAName NameHaver;
        public int        ChildCount;
        public float      ExtraHeight;
        public string     EventLabel;


        protected sealed override void UpdateDetails()
        {
          m_RootProp.TryGetUnderlyingValue(out Event);

          NameHaver   = Event as IHaveAName;
          ChildCount  = 0;
          ExtraHeight = UNEXPANDED_HEIGHT;

          var child_it = m_RootProp.FindPropertyRelative(UNITYEVENT_LAST_PROPERTY);
          
          while (child_it.NextVisible(false) &&
                 child_it.depth == m_RootProp.depth + 1 &&
                 child_it.propertyPath.StartsWith(m_RootProp.propertyPath))
          {
            ExtraHeight += EditorGUI.GetPropertyHeight(child_it, child_it.isExpanded) + STD_PAD;
            ++ChildCount;
          }

          EventLabel = RichText.TypeNamePlain(Event.GetType());
        }

        public void UpdateExtraHeight()
        {
          IsStale = false;

          if (ChildCount == 0)
            return;

          ExtraHeight = UNEXPANDED_HEIGHT;

          var child_it = m_RootProp.FindPropertyRelative(UNITYEVENT_LAST_PROPERTY);

          int i = 0;
          while (i++ < ChildCount && child_it.NextVisible(false))
          {
            ExtraHeight += EditorGUI.GetPropertyHeight(child_it, child_it.isExpanded) + STD_PAD;
          }
        }

        public SerializedProperty GetChildIterator(SerializedProperty prop)
        {
          if (ChildCount <= 0)
            return null;

          return prop.FindPropertyRelative(UNITYEVENT_LAST_PROPERTY);
        }
      } // end class State


      private static readonly string s_LabelSuffixDisabled = RichText.Color(" (event disabled)", Colors.Gray);


      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        PropertyDrawerState.Restore(prop, out State state);

        if (Logging.Assert(!state.NeedsUpdate))
          return;

        // enable/disable button:
        float btn_begin = FieldStartX + FieldWidth * 0.45f;
        var pos = new Rect(btn_begin, total.y + STD_PAD_HALF, total.xMax - btn_begin, STD_LINE_HEIGHT);

        if (state.Event.IsEnabled)
          Labels.Button.text = "Disable Event";
        else
          Labels.Button.text = "Enable Event";

        if (GUI.Button(pos, Labels.Button, Styles.Button))
        {
          prop.serializedObject.targetObject.RecordUndo(Labels.Button.text);

          state.Event.IsEnabled = !state.Event.IsEnabled;

          state.NameHaver?.Bonk();
          return;
        }

        // get the custom display string, if it is available:
        if (!(state.NameHaver?.Name).IsEmpty())
          label.text = state.NameHaver.Name;
        else if (!state.Event.IsEnabled)
          label.text += s_LabelSuffixDisabled;

        // draw foldout header next:

        pos.x = total.x;
        pos.xMax = btn_begin - STD_PAD_RIGHT;

        if (FoldoutHeader.Open(pos, label, prop, out FoldoutHeader header, prop.depth))
        {
          pos.x = header.Rect.x;
          pos.xMax = total.xMax;
          pos.y += pos.height + STD_PAD;

          EditorGUI.BeginDisabledGroup(!state.Event.IsEnabled);

          // get the property iterator for our extra members:
          var child_prop = state.GetChildIterator(prop);
          if (child_prop != null)
          {
            int i = 0;

            // iterate:
            while (i++ < state.ChildCount && child_prop.NextVisible(false))
            {
              label.text = child_prop.displayName;
              pos.height = EditorGUI.GetPropertyHeight(child_prop, label, includeChildren: true);

              _ = EditorGUI.PropertyField(pos, child_prop, label, includeChildren: true);

              pos.y += pos.height + STD_PAD;
            }

            child_prop.Dispose();
          }

          // finally, draw the vanilla event interface:
          pos.xMin += Indent;
          pos.yMax = total.yMax;

          if (state.Event.IsEnabled)
            label.text = RichText.Color(state.EventLabel, Colors.GUI.Keyword);
          else
            label.text = state.EventLabel;

          base.OnGUI(pos, prop, label);

          EditorGUI.EndDisabledGroup();
        }

        header.Rect.yMax = pos.yMax + STD_PAD;
        header.Dispose();
      }

      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
        if (!prop.isExpanded)
          return UNEXPANDED_HEIGHT;

        PropertyDrawerState.Restore(prop, out State state);

        if (state.IsStale)
          state.UpdateExtraHeight();

        return state.ExtraHeight + base.GetPropertyHeight(prop, label) + STD_PAD_HALF;
      }


      //protected override void DrawEventHeader(Rect rect)
      //{
      //  rect.height = STD_LINE_HEIGHT;
      //  GUI.Label(rect, m_State.EventLabel);
      //}

    } // end class PyroEventDrawer

  } // end partial static class GUIDrawers

}