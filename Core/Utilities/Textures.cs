/**
@file   PyroDK/Core/Utilities/Textures.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-05

@brief
  Texture predefinitions and utilities.
**/

using UnityEngine;


namespace PyroDK
{

  public static class Textures
  {

    public static readonly Texture2D Clear = new Texture2D(1, 1, TextureFormat.Alpha8, mipChain: false)
    {
      name       = "PyroDK.Textures.Clear",
      filterMode = FilterMode.Point,
      wrapMode   = TextureWrapMode.Clamp,
      wrapModeU  = TextureWrapMode.Clamp,
      wrapModeV  = TextureWrapMode.Clamp,
      wrapModeW  = TextureWrapMode.Clamp
    };

    public static readonly Texture2D FieldBG = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false)
    {
      name       = "PyroDK.Textures.FieldBG",
      filterMode = FilterMode.Point,
      wrapMode   = TextureWrapMode.Clamp,
      wrapModeU  = TextureWrapMode.Clamp,
      wrapModeV  = TextureWrapMode.Clamp,
      wrapModeW  = TextureWrapMode.Clamp
    };

    public static readonly Texture2D SectionBG = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false)
    {
      name       = "PyroDK.Textures.SectionBG",
      filterMode = FilterMode.Point,
      wrapMode   = TextureWrapMode.Clamp,
      wrapModeU  = TextureWrapMode.Clamp,
      wrapModeV  = TextureWrapMode.Clamp,
      wrapModeW  = TextureWrapMode.Clamp
    };


    private static HashMap<string, Texture2D> s_EditorMap = new HashMap<string, Texture2D>()
    {
      { Clear.name,     Clear     },
      { FieldBG.name,   FieldBG   },
      { SectionBG.name, SectionBG },
    };

    static Textures()
    {
      Clear.SetPixels32(new Color32[] { Colors.Clear });
      Clear.Apply(updateMipmaps: true, makeNoLongerReadable: true);

      FieldBG.SetPixels32(new Color32[] { Colors.GUI.FieldBG });
      FieldBG.Apply(updateMipmaps: true, makeNoLongerReadable: true);

      SectionBG.SetPixels32(new Color32[] { Colors.GUI.SectionBG });
      SectionBG.Apply(updateMipmaps: true, makeNoLongerReadable: true);
    }


    public static Texture2D FindForEditor(string name)
    {
      if (s_EditorMap.Find(name, out Texture2D tex))
        return tex;

      #if UNITY_EDITOR
      return UnityEditor.EditorGUIUtility.FindTexture(name);
      #else
      return Clear;
      #endif
    }


    public static void Fill(this Texture2D tex, Color32 c)
    {
      if (!tex)
        return;

      int count = tex.width * tex.height;
      var pixels = new Color32[count];

      for (int i = 0; i < count; ++i)
        pixels[i] = c;

      tex.SetPixels32(pixels);
      tex.Apply(updateMipmaps: true);
    }

  } // end class Textures

}