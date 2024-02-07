// 
// ScriptLibrary.cs
// 
// Copyright (c) 2023 Lampert & Sons LLC
// 
// All rights reserved.
// 

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace WhatScript {
  internal static class ScriptLibrary {
    private class LibraryBuilder : AssetPostprocessor {
      private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        if (Preferences.AutoTrackScripts) {
          if (ContainsScripts(importedAssets) || ContainsScripts(movedAssets)) {
            RefreshScriptLibrary();
          }
        }
      }

      private static bool ContainsScripts(string[] filenames) {
        foreach (string filename in filenames) {
          if (filename.ToLowerInvariant().EndsWith(".cs")) {
            return true;
          }
        }

        return false;
      }
    }

    private static void RefreshScriptLibrary() {
      DateTime start = DateTime.Now;

      Dictionary<string, Script> scriptLibrary = BuildScriptLibrary();

      UpdateScriptLibrary(scriptLibrary);

      Log($"Updated script library in {(DateTime.Now - start).TotalMilliseconds}ms");
    }

    internal static void UpdateScriptLibrary(Dictionary<string, Script> scriptLibrary) {
      Dictionary<string, Script> previous = LoadScriptLibrary();

      // merge existing scripts into new scripts
      foreach (KeyValuePair<string, Script> keyValuePair in previous) {
        if (scriptLibrary.TryGetValue(keyValuePair.Key, out Script script)) {
          script.Merge(keyValuePair.Value);
        } else {
          scriptLibrary.Add(keyValuePair.Key, keyValuePair.Value);
        }
      }

      SaveScriptLibrary(scriptLibrary);
    }

    internal static void Log(string message) {
#if WHAT_SCRIPT_LOGGING
      Debug.Log($"{nameof(ScriptLibrary)}: {message}");
#endif
    }

    internal static void LogError(string message) {
      Debug.LogError($"{nameof(ScriptLibrary)}: {message}");
    }

    internal static Dictionary<string, Script> LoadScriptLibrary() {
      Dictionary<string, Script> result = new Dictionary<string, Script>();

      try {
        string scriptLibraryPath = ScriptLibraryUtility.GetCurrentScriptLibraryPath();

        if (File.Exists(scriptLibraryPath)) {
          foreach (string line in File.ReadAllLines(scriptLibraryPath)) {
            if (Script.TryDeserialize(line, out Script script)) {
              if (!result.ContainsKey(script.Guid)) {
                result.Add(script.Guid, script);
              }
            }
          }
        }
      } catch (Exception e) {
        LogError($"Unable to load script library: {e.Message}");
      }

      return result;
    }

    private static void SaveScriptLibrary(Dictionary<string, Script> scripts) {
      List<string> lines = new List<string>(scripts.Values.Count);

      foreach (Script script in scripts.Values) {
        lines.Add(script.Serialize());
      }

      lines.Sort(string.CompareOrdinal);

      File.WriteAllLines(ScriptLibraryUtility.GetCurrentScriptLibraryPath(), lines);
    }

    private static Dictionary<string, Script> BuildScriptLibrary() {
      string[] guids = AssetDatabase.FindAssets("t:script");

      Dictionary<string, Script> scripts = new Dictionary<string, Script>(guids.Length);

      foreach (string guid in guids) {
        if (scripts.ContainsKey(guid)) {
          continue;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guid);

        scripts.Add(guid, new Script {
          Guid = guid,
          Path = assetPath
        });
      }

      return scripts;
    }
  }
}