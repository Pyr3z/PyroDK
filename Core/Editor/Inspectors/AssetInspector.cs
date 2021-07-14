/**
@file   PyroDK/Core/Editor/Inspectors/AssetInspector.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-09

@brief
  Override the default inspector editor behaviour.
**/

using UnityEditor;


namespace PyroDK.Editor
{

  public static partial class Inspectors
  {

    [CustomEditor(typeof(BaseAsset), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    private class AssetInspector : BaseInspector
    {

      public override void OnInspectorGUI()
      {
        DrawPyroInspector(serializedObject);
      }

    }

  }

}