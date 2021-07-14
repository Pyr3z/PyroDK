/**
@file   PyroDK/Core/BaseTypes/IComponent.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-07

@brief
  Defines the IComponent interface for component types that are meant to be
  "attached" to IObjects (GameObjects).

@remark
  Formerly known as "Lovo3D.Core.IMonoBehaviour".
**/

#pragma warning disable IDE1006 // naming convention violations

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace PyroDK
{

  public interface IComponent : IObject
  {



    // `UnityEngine.MonoBehaviour` facade members:
    GameObject  gameObject         { get; }
    Transform   transform          { get; }
    string      tag                { get; set;  }
    bool        enabled            { get; set; }
    bool        isActiveAndEnabled { get; }

    bool        CompareTag(string tag);

    bool        TryGetComponent<T>(out T component);

    T           GetComponent<T>();
    T[]         GetComponents<T>();
    void        GetComponents<T>(List<T> results);
    T           GetComponentInChildren<T>();
    T           GetComponentInChildren<T>(bool include_inactive);
    T[]         GetComponentsInChildren<T>();
    T[]         GetComponentsInChildren<T>(bool include_inactive);
    void        GetComponentsInChildren<T>(List<T> results);
    void        GetComponentsInChildren<T>(bool include_inactive, List<T> results);
    T           GetComponentInParent<T>();
    T[]         GetComponentsInParent<T>();
    T[]         GetComponentsInParent<T>(bool include_inactive);
    void        GetComponentsInParent<T>(bool include_inactive, List<T> results);

    Coroutine   StartCoroutine(IEnumerator routine);

    void        StopCoroutine(IEnumerator routine);
    void        StopCoroutine(Coroutine routine);
    void        StopCoroutine(string name);

    void        StopAllCoroutines();

  }

}