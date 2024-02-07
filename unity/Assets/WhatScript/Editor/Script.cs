// 
// Script.cs
// 
// Copyright (c) 2023 Lampert & Sons LLC
// 
// All rights reserved.
// 

namespace WhatScript {
  internal class Script {
    public string Guid;
    public string Path;
    public string FoundAt;

    public string Serialize() {
      return $"{Guid};{Path};{FoundAt}";
    }

    public void Merge(Script script) {
      if (string.IsNullOrEmpty(FoundAt)) {
        FoundAt = script.FoundAt;
      }
    }

    public static bool TryDeserialize(string line, out Script script) {
      string[] parts = line.Split(';');

      if (parts.Length < 3) {
        script = null;
        return false;
      }

      script = new Script {
        Guid = parts[0],
        Path = parts[1]
      };

      if (parts.Length >= 3) {
        script.FoundAt = parts[2];
      }

      return true;
    }

    public override string ToString() {
      if (!string.IsNullOrEmpty(FoundAt)) {
        return $"{Path}\nLast seen at {FoundAt.Substring(0, 7)}";
      }

      return Path;
    }
  }
}