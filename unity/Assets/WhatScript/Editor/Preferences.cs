// 
// Preferences.cs
// 
// Copyright (c) 2023 Lampert & Sons LLC
// 
// All rights reserved.
// 

using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace WhatScript {
  internal static class Preferences {
    private const string DontTrackScriptsPath = "ProjectSettings/WhatScript.donttrack";

    internal static string OverrideGitPath {
      get { return EditorPrefs.GetString("WhatScript.OverrideGitPath", ""); }

      set { EditorPrefs.SetString("WhatScript.OverrideGitPath", value); }
    }

    private static ScriptLibraryLocation ScriptLibraryLocation {
      get { return ScriptLibraryUtility.DetectScriptLibraryLocation(); }

      set {
        ScriptLibraryLocation scriptLibraryLocation = ScriptLibraryUtility.DetectScriptLibraryLocation();

        if (scriptLibraryLocation != value) {
          string source = ScriptLibraryUtility.GetScriptLibraryPath(scriptLibraryLocation);
          string destination = ScriptLibraryUtility.GetScriptLibraryPath(value);

          File.Move(source, destination);
        }
      }
    }

    internal static bool AutoTrackScripts {
      get { return !File.Exists(DontTrackScriptsPath); }

      set {
        if (value) {
          File.Delete(DontTrackScriptsPath);
        } else {
          File.Create(DontTrackScriptsPath);
        }
      }
    }

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider() {
      SettingsProvider provider = new SettingsProvider("Project/WhatScript", SettingsScope.Project) {
        guiHandler = searchContext => {
          // respect search context
          if (!string.IsNullOrEmpty(searchContext)) {
            searchContext = searchContext.ToLowerInvariant();
            if (!searchContext.Contains("whatscript") && !searchContext.Contains("git")) {
              return;
            }
          }

          OverrideGitPath = EditorGUILayout.TextField("Override Git Path", OverrideGitPath);
          ScriptLibraryLocation = (ScriptLibraryLocation) EditorGUILayout.EnumPopup("Library Location", ScriptLibraryLocation);

          //
          // auto track scripts
          // 
          bool autoTrackScripts0 = AutoTrackScripts;
          bool autoTrackScripts = EditorGUILayout.Toggle("Auto Track Scripts", autoTrackScripts0);

          if (autoTrackScripts != autoTrackScripts0) {
            AutoTrackScripts = autoTrackScripts;
          }
          
          if (!autoTrackScripts) {
            EditorGUILayout.HelpBox("Scripts will not be auto tracked. You can still use Git search to identify scripts if they go missing.", MessageType.Warning);
          }
        },
        keywords = new HashSet<string>(new[] {"WhatScript", "Git"})
      };

      return provider;
    }
  }
}