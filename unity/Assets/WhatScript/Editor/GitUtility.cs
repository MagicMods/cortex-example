// 
// GitUtility.cs
// 
// Copyright (c) 2023 Lampert & Sons LLC
// 
// All rights reserved.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace WhatScript {
  static internal class GitUtility {
    private class DeletedFile {
      public string Filename;
      public string Commit;
    }

    private static Process _GitProcess = null;

    private static string GitPath {
      get {
        string result = Preferences.OverrideGitPath;

        if (!string.IsNullOrEmpty(result)) {
          return result;
        }

        if (Application.platform == RuntimePlatform.WindowsEditor) {
          result = "git.exe";
        } else {
          result = "git";
        }

        return result;
      }
    }

    internal static void LoadDeletedScripts(Action<Dictionary<string, Script>> callback) {
      LoadDeletedScripts(null, callback);
    }

    internal static void LoadDeletedScripts(string targetGuid, Action<Dictionary<string, Script>> callback) {
      if (EnsureGitAvailable()) {
        EditorCoroutine.StartCoroutine(LoadDeletedScriptsAsync(targetGuid, callback));
      }
    }

    private static IEnumerator LoadDeletedScriptsAsync(string targetGuid, Action<Dictionary<string, Script>> callback) {
      ProgressTracker progressTracker = new ProgressTracker("Searching Git", "Loading Git History...");

      Dictionary<string, Script> result = new Dictionary<string, Script>();

      if (TryRunCommand(GitPath, "log --diff-filter=ADR --summary -- *.cs.meta", out string output)) {
        string[] lines = output.Split('\n');

        progressTracker.Start(lines.Length);

        List<DeletedFile> deletedFiles = new List<DeletedFile>();
        string currentCommit = "";

        foreach (string line in lines) {
          string invariant = line.Trim().ToLowerInvariant();

          if (progressTracker.Increment()) {
            yield return null;
          }

          if (progressTracker.Canceled) {
            break;
          }

          if (invariant.StartsWith("commit")) {
            string commit = line.Split(' ')[1];

            currentCommit = commit;
            progressTracker.Message = $"Searching commit: {commit.Substring(0, 7)}...";

            foreach (DeletedFile deletedFile in deletedFiles) {
              if (progressTracker.Tick()) {
                yield return null;
              }

              if (TryAddFile(deletedFile, commit, result, out Script script)) {
                if (targetGuid != null && targetGuid == script.Guid) {
                  progressTracker.Cancel();
                }
              }
            }

            deletedFiles.Clear();
          } else if (invariant.Contains(".cs.meta")) {
            if (ParseFilename(line, out string filename)) {
              if (invariant.StartsWith("delete") || invariant.StartsWith("rename")) {
                deletedFiles.Add(new DeletedFile {
                  Commit = currentCommit,
                  Filename = filename
                });
              } else if (TryAddFile(filename, currentCommit, result, out Script script)) {
                if (targetGuid != null && targetGuid == script.Guid) {
                  progressTracker.Cancel();
                }
              }
            }
          }
        }
      }

      EditorUtility.ClearProgressBar();

      callback.Invoke(result);
    }

    private static bool EnsureGitAvailable() {
      if (!RunCommand($"{GitPath}", "--version")) {
        bool result = EditorUtility.DisplayDialog("Git Not Found", "Git was not found on your system. Please install Git and try again, or select the location of the git executable.", "Find Git", "OK");

        if (result) {
          string git = EditorUtility.OpenFilePanel("Find Git", "", "");

          if (!string.IsNullOrEmpty(git)) {
            Preferences.OverrideGitPath = git;
            return EnsureGitAvailable();
          }
        }

        return false;
      }

      return true;
    }

    private static bool TryAddFile(DeletedFile deletedFile, string commit, Dictionary<string, Script> scriptLibrary, out Script script) {
      if (!TryGetFile(commit, deletedFile.Filename, out string contents)) {
        script = null;
        return false;
      }

      if (!TryMakeScript(deletedFile.Filename, deletedFile.Commit, contents, out script)) {
        return false;
      }

      if (scriptLibrary.ContainsKey(script.Guid)) {
        return false;
      }

      scriptLibrary.Add(script.Guid, script);
      return true;
    }

    private static bool TryAddFile(string filename, string commit, Dictionary<string, Script> scriptLibrary, out Script script) {
      if (!TryGetFile(commit, filename, out string contents)) {
        script = null;
        return false;
      }

      if (!TryMakeScript(filename, commit, contents, out script)) {
        return false;
      }

      if (scriptLibrary.ContainsKey(script.Guid)) {
        return false;
      }

      scriptLibrary.Add(script.Guid, script);
      return true;
    }

    private static bool TryMakeScript(string filename, string commit, string contents, out Script script) {
      int index = contents.IndexOf("guid: ", StringComparison.InvariantCulture);

      if (index == -1) {
        script = null;
        return false;
      }

      index += 6;

      int end = contents.IndexOf('\n', index);

      if (end == -1) {
        script = null;
        return false;
      }

      script = new Script {
        Guid = contents.Substring(index, end - index).Trim(),
        Path = filename.Replace(".meta", ""),
        FoundAt = commit
      };

      return true;
    }

    private static bool TryGetFile(string commit, string filename, out string contents) {
      if (TryRunCommand(GitPath, $"show {commit}:{filename}", out string output, out string error)) {
        if (string.IsNullOrEmpty(error)) {
          contents = output;
          return true;
        }
      }

      contents = null;
      return false;
    }

    private static bool RunCommand(string command, string arguments) {
      return TryRunCommand(command, arguments, out string output, out string error);
    }

    private static bool TryRunCommand(string command, string arguments, out string output) {
      bool result = TryRunCommand(command, arguments, out output, out string error);

      if (!result || !string.IsNullOrEmpty(error)) {
        return false;
      }

      return true;
    }

    private static bool TryRunCommand(string command, string arguments, out string output, out string error) {
      ProcessStartInfo processStartInfo = new ProcessStartInfo();
      processStartInfo.FileName = command;
      processStartInfo.Arguments = arguments;
      processStartInfo.RedirectStandardError = true;
      processStartInfo.RedirectStandardOutput = true;
      processStartInfo.CreateNoWindow = true;
      processStartInfo.UseShellExecute = false;

      if (_GitProcess == null) {
        _GitProcess = new Process();
      }

      try {
        if (_GitProcess == null) {
          output = "";
          error = "";
          return false;
        }

        _GitProcess.StartInfo = processStartInfo;
        _GitProcess.Start();

        output = _GitProcess.StandardOutput.ReadToEnd();
        error = _GitProcess.StandardError.ReadToEnd();
        return true;
      } catch (Exception) {
        output = "";
        error = "";
        return false;
      }
    }

    private static bool ParseFilename(string line, out string filename) {
      if (line.Contains("{")) {
        return ParseFilenameRename(line, out filename);
      }

      return ParseFilenameBasic(line, out filename);
    }

    private static bool ParseFilenameBasic(string line, out string filename) {
      int index = 0;

      for (int i = 0; i < 4; ++i) {
        index = line.IndexOf(' ', index);
        if (index == -1) {
          filename = null;
          return false;
        }

        index += 1;
      }

      filename = line.Substring(index).Trim();
      return true;
    }

    private static bool ParseFilenameRename(string line, out string filename) {
      string[] tokens = line.Split(new string[] {
        "rename",
        "{",
        "}",
        "=>"
      }, StringSplitOptions.None);

      if (tokens.Length == 5) {
        filename = $"{tokens[1].Trim()}{tokens[2].Trim()}";
        return true;
      }

      filename = null;
      return false;
    }
  }
}