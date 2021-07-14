/**
@file   PyroDK/Core/Utilities/Enumerables.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-07

@brief
  Provides iterators (IEnumerables) for common use cases.
**/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{
  public enum TreeTraversal
  {
    BreadthFirst,
    DepthFirst
  }

  public static class Enumerables
  {
    private delegate void Push<T>(T obj);
    private delegate T    Pop<T>();

    public static IEnumerable<Transform> TraverseChildren(this Transform node, bool include_root = true, TreeTraversal traversal = TreeTraversal.BreadthFirst)
    {
      ICollection     stackqueue;
      Push<Transform> push;
      Pop<Transform>  pop;

      if (traversal == TreeTraversal.BreadthFirst)
      {
        var stack = new Stack<Transform>();
        push = stack.Push;
        pop = stack.Pop;
        stackqueue = stack;
      }
      else
      {
        var queue = new Queue<Transform>();
        push = queue.Enqueue;
        pop = queue.Dequeue;
        stackqueue = queue;
      }

      if (include_root) yield return node;

      foreach (Transform child in node)
      {
        push(child);
      }

      while (stackqueue.Count > 0)
      {
        node = pop();
        
        foreach (Transform child in node)
        {
          push(child);
        }

        yield return node;
      }

      yield break;
    }

    public static IEnumerable<Transform> TraverseUpwards(this Transform node)
    {
      while (node)
      {
        yield return node;
        node = node.parent;
      }

      yield break;
    }


    public static IEnumerable<TResult> SelectType<TResult>(this IEnumerable source)
      where TResult : class
    {
      foreach (object obj in source)
      {
        if (obj is TResult casted)
          yield return casted;
      }
    }

  }

}