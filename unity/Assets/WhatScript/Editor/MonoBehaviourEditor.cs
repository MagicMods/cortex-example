// 
// MonoBehaviourEditor.cs
// 
// Copyright (c) 2023 Lampert & Sons LLC
// 
// All rights reserved.
// 

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WhatScript {
  [CustomEditor(typeof(MonoBehaviour))]
  public class MonoBehaviourEditor : Editor {
    private static Dictionary<string, Script> _SharedScriptLibrary = null;
    private static bool _SharedGitSearchInProgress = false;

    private string _Guid = null;
    private GUIContent _IconGUIContent = null;

    private static void EnsureSharedScriptLibrary() {
      if (_SharedScriptLibrary == null) {
        DateTime start = DateTime.Now;

        _SharedScriptLibrary = ScriptLibrary.LoadScriptLibrary();

        ScriptLibrary.Log($"Loaded script library in {(DateTime.Now - start).TotalMilliseconds}ms");
      }
    }

    private void Awake() {
      _IconGUIContent = new GUIContent(Resources.Load<Texture>("WhatScriptIcon"));
    }

    public override void OnInspectorGUI() {
      if (_Guid == null) {
        _Guid = GetGuid(serializedObject);
      }

      base.OnInspectorGUI();

      // the script isn't fully formed yet on the Unity side
      if (string.IsNullOrEmpty(_Guid)) {
        return;
      }

      // usually Unity won't call into our GUI if the script isn't missing, but if they do, ensure we don't report a false positive
      if (!IsMissing(_Guid)) {
        return;
      }

      EnsureSharedScriptLibrary();

      Script script = LocateScript(_Guid);

      bool previous = GUI.enabled;
      GUI.enabled = true;

      try {
        if (script == null) {
          HelpBox("Unable to find previous record for script. Search source control?");

          GUILayout.BeginHorizontal();

          GUI.enabled = !_SharedGitSearchInProgress;
          if (GUILayout.Button("Search Git")) {
            if (!_SharedGitSearchInProgress) {
              GitUtility.LoadDeletedScripts(_Guid, scriptLibrary => {
                _SharedScriptLibrary = null;
                _SharedGitSearchInProgress = false;

                ScriptLibrary.UpdateScriptLibrary(scriptLibrary);
                Repaint();

                if (!scriptLibrary.ContainsKey(_Guid)) {
                  EditorUtility.DisplayDialog("Unable to locate script", "Please ensure Git is installed has been in use for source control. Scripts removed before installing WhatScript and that are not tracked by source control cannot be identified.", "OK");
                }
              });
            }
          }

          GUI.enabled = true;

          GUILayout.FlexibleSpace();
          GUILayout.EndHorizontal();
        } else {
          HelpBox($"Was previously {script}");

          if (!string.IsNullOrEmpty(script.FoundAt)) {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Copy Git Commit Hash")) {
              EditorGUIUtility.systemCopyBuffer = script.FoundAt;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
          } else {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Find in Git History")) {
              if (!_SharedGitSearchInProgress) {
                GitUtility.LoadDeletedScripts(_Guid, scriptLibrary => {
                  _SharedScriptLibrary = null;
                  _SharedGitSearchInProgress = false;

                  ScriptLibrary.UpdateScriptLibrary(scriptLibrary);
                  Repaint();

                  if (!scriptLibrary.ContainsKey(_Guid)) {
                    EditorUtility.DisplayDialog("Unable to locate script", "Please ensure Git is installed has been in use for source control. Scripts that haven't been committed to source control cannot be found using Git search.", "OK");
                  }
                });
              }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
          }
        }
      } finally {
        GUI.enabled = previous;
      }
    }

    private void HelpBox(string message) {
      _IconGUIContent.text = message;
      EditorGUILayout.LabelField(GUIContent.none, _IconGUIContent, EditorStyles.helpBox);
    }

    private static Script LocateScript(string guid) {
      if (_SharedScriptLibrary.TryGetValue(guid, out Script script)) {
        return script;
      }

      return null;
    }

    private static string GetGuid(SerializedObject serializedObject) {
      SerializedProperty script = serializedObject.FindProperty("m_Script");

      AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script.objectReferenceInstanceIDValue, out string guid, out long _);

      return guid;
    }

    private static bool IsMissing(string guid) {
      return AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid)) == null;
    }
  }
}