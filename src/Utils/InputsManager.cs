using Hexa.NET.ImGui;
using PlayerList.GUI.Tabs;
using Renderer = PlayerList.GUI.Renderer;
using System.Linq;
using UnityEngine;

namespace PlayerList.Utils;

class InputsManager : MonoBehaviour
{
  private static readonly ImGuiKey[] BlacklistedKeys =
[
  ImGuiKey.MouseLeft,
    ImGuiKey.MouseMiddle,
    ImGuiKey.MouseRight,
    ImGuiKey.Tab,
    ImGuiKey.MouseWheelX,
    ImGuiKey.MouseWheelY,
    ImGuiKey.MouseX1,
    ImGuiKey.MouseX2,

    // Disable those keys because once set, it's impossible to use them in Unity
    ImGuiKey.Enter,
    ImGuiKey.KeypadEnter,
    ImGuiKey.CapsLock,
];
  private static readonly ImGuiKey[] ModKeys =
  [
    ImGuiKey.ModCtrl,
    ImGuiKey.ReservedForModCtrl,
    ImGuiKey.ModShift,
    ImGuiKey.ReservedForModShift,
    ImGuiKey.ReservedForModAlt,
    ImGuiKey.ModAlt,
    ImGuiKey.ReservedForModSuper,
    ImGuiKey.ModSuper,
    ImGuiKey.LeftSuper,
    ImGuiKey.RightSuper,
    ImGuiKey.PrintScreen,
  ];

  private void Update()
  {
    if (Input.GetKeyDown(KeyMapper.ConvertImGuiToUnity(ConfigManager.EnableMenu.Keybind.Key)) && !ShouldCancel(ConfigManager.EnableMenu.Keybind))
      Renderer.ToggleMenu();

    if (Input.GetKeyDown(KeyMapper.ConvertImGuiToUnity(ConfigManager.DisplayUsernames.Keybind.Key)) && !ShouldCancel(ConfigManager.DisplayUsernames.Keybind))
      Renderer.ToggleUsernames();
  }

  private static bool ShouldCancel(Keybind hotkey, bool isImGui = false)
  {
    // If setting keybinds, just cancel directly no matter what
    if (ConfigTab.CurrentlySettingKeybind != null) return true;

    var controlPressed = isImGui ? ImGuiP.IsKeyPressed(ImGuiKey.ModCtrl) : Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    var shiftPressed = isImGui ? ImGuiP.IsKeyPressed(ImGuiKey.ModShift) : Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    var altPressed = isImGui ? ImGuiP.IsKeyPressed(ImGuiKey.ModAlt) : Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

    if (!controlPressed && hotkey.Control) return true;
    if (!shiftPressed && hotkey.Shift) return true;
    if (!altPressed && hotkey.Alt) return true;

    return false;
  }

  public static void DetectImGuiKeybinds()
  {
    if (ImGuiP.IsKeyPressed(ConfigManager.EnableMenu.Keybind.Key) && !ShouldCancel(ConfigManager.EnableMenu.Keybind, true))
    {
      if (ConfigManager.EnableMenu.Value) ProcessUtils.FocusGame();
      else ProcessUtils.FocusOverlay();

      Renderer.ToggleMenu();
    }

    if (ImGuiP.IsKeyPressed(ConfigManager.DisplayUsernames.Keybind.Key) && !ShouldCancel(ConfigManager.DisplayUsernames.Keybind, true))
    {
      if (ConfigManager.EnableMenu.Value) ProcessUtils.FocusGame();
      else ProcessUtils.FocusOverlay();

      ConfigManager.DisplayUsernames.Value = !ConfigManager.DisplayUsernames.Value;
    }
  }

  public static void GetKeybind(out bool control, out bool shift, out bool alt, out ImGuiKey key)
  {
    for (var eventKey = ImGuiKey.NamedKeyBegin; eventKey < ImGuiKey.NamedKeyEnd; eventKey++)
    {
      if (!ImGuiP.IsKeyDown(eventKey)) continue;
      if (eventKey == ImGuiKey.Escape) continue;
      if (BlacklistedKeys.Contains(eventKey) || ModKeys.Contains(eventKey)) continue;

      control = ImGuiP.IsKeyDown(ImGuiKey.ModCtrl);
      shift = ImGuiP.IsKeyDown(ImGuiKey.ModShift);
      alt = ImGuiP.IsKeyDown(ImGuiKey.ModAlt);
      key = eventKey;

      return;
    }

    control = false;
    shift = false;
    alt = false;
    key = ImGuiKey.None;
  }
}
