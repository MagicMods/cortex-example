// 
// ScriptLibraryUtility.cs
// 
// Copyright (c) 2023 Lampert & Sons LLC
// 
// All rights reserved.
// 

using System.IO;

namespace WhatScript {
  internal static class ScriptLibraryUtility {
    private const string ScriptLibraryFilename = "script_library.txt";

    internal static string GetCurrentScriptLibraryPath() {
      return GetScriptLibraryPath(DetectScriptLibraryLocation());
    }

    internal static string ScriptLibraryPathProjectRoot {
      get { return ScriptLibraryFilename; }
    }

    internal static string ScriptLibraryPathProjectSettings {
      get { return "ProjectSettings/" + ScriptLibraryFilename; }
    }

    internal static string ScriptLibraryPathLibrary {
      get { return "Library/" + ScriptLibraryFilename; }
    }

    internal static string GetScriptLibraryPath(ScriptLibraryLocation scriptLibraryLocation) {
      switch (scriptLibraryLocation) {
        case ScriptLibraryLocation.Library:
          return ScriptLibraryPathLibrary;
        
        case ScriptLibraryLocation.ProjectSettings:
          return ScriptLibraryPathProjectSettings;
        
        case ScriptLibraryLocation.ProjectRoot:
          return ScriptLibraryPathProjectRoot;
        
        default:
          return ScriptLibraryPathProjectSettings;
      }
    }

    internal static ScriptLibraryLocation DetectScriptLibraryLocation() {
      if (File.Exists(ScriptLibraryPathProjectSettings)) {
        return ScriptLibraryLocation.ProjectSettings;
      }

      if (File.Exists(ScriptLibraryPathLibrary)) {
        return ScriptLibraryLocation.Library;
      }

      if (File.Exists(ScriptLibraryPathProjectRoot)) {
        return ScriptLibraryLocation.ProjectRoot;
      }

      return ScriptLibraryLocation.ProjectSettings;
    }
  }
}