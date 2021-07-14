/**
@file   PyroDK/Core/Editor/GUIDrawers/EnumPairDrawer.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-20

@brief
  PropertyDrawer for [Serializable] `PyroDK.EnumPair`s.
**/

using UnityEngine;

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class GUIDrawers
  {

    [CustomPropertyDrawer(typeof(EnumPair))]
    private sealed class EnumPairDrawer : PropertyDrawer
    {

      public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
      {
        if (Logging.Assert(this.TryGetUnderlyingValue(prop, out EnumPair pair)))
        {
          InvalidField(in total, label);
          return;
        }
        if (pair.IsMissing)
        {
          InvalidField(total, label, "Invalid or missing enum types");
          return;
        }

        var pos = PrefixLabelStrict(in total, label, Styles.Label);
        var curr_prop = prop.FindPropertyRelative(EnumPair.VALUE_PROPERTY_START);

        PushIndentLevel(0, fix_label_width: false);

        pos.width = (pos.width - STD_PAD) / 2f;

        _ = EnumPopupField(in pos, curr_prop, pair.T0);

        curr_prop.NextVisible(false);

        pos.x += pos.width + STD_PAD;
        pos.xMax = total.xMax;

        _ = EnumPopupField(in pos, curr_prop, pair.T1);

        PopIndentLevel(fix_label_width: false);
      }

    }

  }

}