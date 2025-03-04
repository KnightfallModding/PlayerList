using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerList.Utils;

public static class KeyMapper
{
  // Mapping from ImGui key names to Unity KeyCodes.
  // Note: Some names differ slightly (e.g. "Enter" in ImGui vs. "Return" in Unity).
  private static readonly Dictionary<string, KeyCode> ImGuiToUnity = new(StringComparer.OrdinalIgnoreCase)
  {
  // Navigation keys
  { "Tab", KeyCode.Tab },
  { "LeftArrow", KeyCode.LeftArrow },
  { "RightArrow",KeyCode.RightArrow },
  { "UpArrow",  KeyCode.UpArrow },
  { "DownArrow", KeyCode.DownArrow },
  { "PageUp",  KeyCode.PageUp },
  { "PageDown", KeyCode.PageDown },
  { "Home", KeyCode.Home },
  { "End",  KeyCode.End },
  { "Insert",  KeyCode.Insert },
  { "Delete",  KeyCode.Delete },

  // Uncategorized keys
  { "Backslash",  KeyCode.Backslash },
  { "GraveAccent",  KeyCode.BackQuote },
  { "Comma",  KeyCode.Comma },
  { "Period",  KeyCode.Period },
  { "Slash",  KeyCode.Slash },
  { "Apostrophe",  KeyCode.Quote },
  { "Semicolon",  KeyCode.Semicolon },
  { "ScrollLock",  KeyCode.ScrollLock },
  { "Pause",  KeyCode.Pause },

  // Editing keys
  { "Backspace", KeyCode.Backspace },
  { "Space", KeyCode.Space },
  { "Enter", KeyCode.Return }, // ImGui uses "Enter"; Unity uses "Return"
  { "Escape",  KeyCode.Escape },
  // Modifier keys
  { "LeftCtrl", KeyCode.LeftControl },
  { "RightCtrl", KeyCode.RightControl },
  { "LeftShift", KeyCode.LeftShift },
  { "RightShift",KeyCode.RightShift },
  { "LeftAlt",  KeyCode.LeftAlt },
  { "RightAlt", KeyCode.RightAlt },
  { "LeftSuper", KeyCode.LeftCommand }, // or KeyCode.LeftWindows as appropriate
  { "RightSuper",KeyCode.RightCommand },
  // Letter keys (A-Z)
  { "A", KeyCode.A }, { "B", KeyCode.B }, { "C", KeyCode.C }, { "D", KeyCode.D },
  { "E", KeyCode.E }, { "F", KeyCode.F }, { "G", KeyCode.G }, { "H", KeyCode.H },
  { "I", KeyCode.I }, { "J", KeyCode.J }, { "K", KeyCode.K }, { "L", KeyCode.L },
  { "M", KeyCode.M }, { "N", KeyCode.N }, { "O", KeyCode.O }, { "P", KeyCode.P },
  { "Q", KeyCode.Q }, { "R", KeyCode.R }, { "S", KeyCode.S }, { "T", KeyCode.T },
  { "U", KeyCode.U }, { "V", KeyCode.V }, { "W", KeyCode.W }, { "X", KeyCode.X },
  { "Y", KeyCode.Y }, { "Z", KeyCode.Z },
  // Digit keys (ImGui uses "Key0"-"Key9")
  { "Keypad0", KeyCode.Keypad0 },
  { "Keypad1", KeyCode.Keypad1 },
  { "Keypad2", KeyCode.Keypad2 },
  { "Keypad3", KeyCode.Keypad3 },
  { "Keypad4", KeyCode.Keypad4 },
  { "Keypad5", KeyCode.Keypad5 },
  { "Keypad6", KeyCode.Keypad6 },
  { "Keypad7", KeyCode.Keypad7 },
  { "Keypad8", KeyCode.Keypad8 },
  { "Keypad9", KeyCode.Keypad9 },
  // Function keys
  { "F1", KeyCode.F1 },
  { "F2", KeyCode.F2 },
  { "F3", KeyCode.F3 },
  { "F4", KeyCode.F4 },
  { "F5", KeyCode.F5 },
  { "F6", KeyCode.F6 },
  { "F7", KeyCode.F7 },
  { "F8", KeyCode.F8 },
  { "F9", KeyCode.F9 },
  { "F10", KeyCode.F10 },
  { "F11", KeyCode.F11 },
  { "F12", KeyCode.F12 },
  // Mouse buttons
  { "MouseLeft",  KeyCode.Mouse0 },
  { "MouseRight", KeyCode.Mouse1 },
  { "MouseMiddle", KeyCode.Mouse2}
  };

  // Reverse mapping from Unity KeyCodes to ImGui key names.
  private static readonly Dictionary<KeyCode, string> UnityToImGui = [];

  // Build the reverse mapping based on the ImGuiToUnity dictionary.
  static KeyMapper()
  {
    foreach (var pair in ImGuiToUnity)
    {
      if (!UnityToImGui.ContainsKey(pair.Value)) UnityToImGui.Add(pair.Value, pair.Key);
    }
  }

  /// <summary>
  /// Converts an ImGui key (given as a string) to a Unity KeyCode.
  /// Returns KeyCode.None if the key is not mapped.
  /// </summary>
  public static KeyCode ConvertImGuiToUnity(ImGuiKey imguiKey) => ImGuiToUnity.TryGetValue(imguiKey.ToString(), out var unityKey) ? unityKey : KeyCode.None;

  /// <summary>
  /// Converts a Unity KeyCode to its corresponding ImGui key (as a string).
  /// Returns "None" if the key is not mapped.
  /// </summary>
  public static string ConvertUnityToImGui(KeyCode unityKey) => UnityToImGui.TryGetValue(unityKey, out string imguiKey) ? imguiKey : "None";
}

