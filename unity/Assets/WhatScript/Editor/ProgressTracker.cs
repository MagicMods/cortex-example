// 
// ProgressTracker.cs
// 
// Copyright (c) 2023 Lampert & Sons LLC
// 
// All rights reserved.
// 

using UnityEditor;

namespace WhatScript {
  internal class ProgressTracker {
    private readonly string _Title;
    private int _Target;
    private int _Ticks;
    private int _Counter;
    private bool _Canceled;

    public string Message { get; set; }
    public bool Canceled => _Canceled;

    public ProgressTracker(string title, string initialMessage) {
      _Title = title;
      Message = initialMessage;
      _Target = 0;
      _Counter = 0;
      _Ticks = 0;

      EditorUtility.DisplayProgressBar(_Title, initialMessage, 0.0f);
    }

    public void Start(int target) {
      _Target = target;
    }

    public bool Increment() {
      _Counter += 1;

      return Tick();
    }

    public bool Tick() {
      _Ticks += 1;

      if ((_Ticks % 10) == 0) {
        if (EditorUtility.DisplayCancelableProgressBar(_Title, Message, (float) (_Counter / (double) _Target))) {
          _Canceled = true;
        }

        return true;
      }

      return false;
    }

    public void Cancel() {
      _Canceled = true;
    }
  }
}