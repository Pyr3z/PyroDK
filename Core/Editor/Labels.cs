/**
@file   PyroDK/Core/Editor/Labels.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-07-09

@brief
  Cache of some custom styles for re-use.
**/

using UnityEngine;


namespace PyroDK.Editor
{

  using BorrowedLabels = ObjectPool<GUIContent>.PromiseKeeper;


  public static class Labels
  {

    public static readonly GUIContent NonEmpty  = new GUIContent(" ");
    public static readonly GUIContent Scratch   = new GUIContent();
    public static readonly GUIContent Button    = new GUIContent();


    public static readonly ObjectPool<GUIContent> Pool
      = new ObjectPool<GUIContent>(notify_exceed_capacity: true,
                                                on_borrow: Clear,
                                                 capacity: 32);

    
    public static void Clear(this GUIContent content)
    {
      content.text = content.tooltip = string.Empty;
      content.image = null;
    }

    public static bool IsEmpty(this GUIContent cont)
    {
      return cont == null || (cont.text.IsEmpty() && cont.image == null);
    }


    public static BorrowedLabels Borrow(params string[] labels)
    {
      if (labels.Length == 0)
      {
        return Pool.MakePromise();
      }
      else if (labels.Length == 1)
      {
        var result = Pool.MakePromise();
        result.Object.text = labels[0];
        return result;
      }
      else
      {
        var result = Pool.MakePromises(labels.Length);

        for (int i = 0; i < labels.Length; ++i)
          result[i].text = labels[i];

        return result;
      }
    }

    public static BorrowedLabels Borrow(int count)
    {
      if (count < 1)
        return null;

      return Pool.MakePromises(count);
    }

  }

}