// 
// EditorCoroutine.cs
// 
// Copyright (c) 2021 Lampert & Sons LLC
// 
// All rights reserved.
// 

using System.Collections;
using UnityEngine;

namespace WhatScript {
  internal static class EditorCoroutine {
    private class EditorCoroutineRunner : MonoBehaviour {
      private void Awake() {
        if (Application.isPlaying) {
          DontDestroyOnLoad(gameObject);
        }
      }

      public Coroutine Run(IEnumerator coroutine) {
        if (!Application.isPlaying) {
          UnityEditor.EditorApplication.update += Callback;

          return null;

          void Callback() {
            if (!coroutine.MoveNext()) {
              UnityEditor.EditorApplication.update -= Callback;
            }
          }
        }

        return StartCoroutine(coroutine);
      }
    }

    private static EditorCoroutineRunner _EditorCoroutineRunner;

    public static Coroutine StartCoroutine(IEnumerator coroutine) {
      EnsureCoroutineRunner();

      return _EditorCoroutineRunner.Run(coroutine);
    }

    private static void EnsureCoroutineRunner() {
      if (_EditorCoroutineRunner == null) {
        GameObject gameObject = new GameObject("EditorCoroutineRunner", typeof(EditorCoroutineRunner)) {
          hideFlags = HideFlags.HideAndDontSave
        };

        _EditorCoroutineRunner = gameObject.GetComponent<EditorCoroutineRunner>();

        if (Application.isPlaying) {
          Object.DontDestroyOnLoad(gameObject);
        }
      }
    }
  }
}