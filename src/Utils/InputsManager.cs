using MelonLoader;
using PlayerList.GUI.Tabs;
using UnityEngine;
using Enum = System.Enum;
using Enumerable = System.Linq.Enumerable;
using Renderer = PlayerList.GUI.Renderer;

namespace PlayerList.Utils;

[RegisterTypeInIl2Cpp]
internal class InputsManager : MonoBehaviour
{
  private static readonly KeyCode[] BlacklistedKeys =
  [
    // Left click
    KeyCode.Mouse0,
    // Right click
    KeyCode.Mouse1,
    // Middle click
    KeyCode.Mouse2,
    // Fourth button
    KeyCode.Mouse3,
    // Fifth button
    KeyCode.Mouse4,
    KeyCode.Tab,

    // Disable those keys because once set, it's impossible to use them in Unity
    KeyCode.Return,
    KeyCode.KeypadEnter,
    KeyCode.CapsLock,
  ];

  private static readonly KeyCode[] ModKeys =
  [
    KeyCode.LeftControl,
    KeyCode.RightControl,
    KeyCode.LeftShift,
    KeyCode.RightShift,
    KeyCode.LeftAlt,
    KeyCode.RightAlt,
    KeyCode.AltGr,
    KeyCode.LeftWindows,
    KeyCode.RightWindows,
    KeyCode.Print,
  ];

  public void Update()
  {
    if (Input.GetKeyDown(ConfigManager.EnableMenu.Keybind.Key) &&
        !ShouldCancel(ConfigManager.EnableMenu.Keybind))
      Renderer.ToggleMenu();

    if (Input.GetKeyDown(ConfigManager.DisplayUsernames.Keybind.Key) &&
        !ShouldCancel(ConfigManager.DisplayUsernames.Keybind))
      Renderer.ToggleUsernames();
  }

  private static bool ShouldCancel(Keybind hotkey, bool isImGui = false)
  {
    // If setting keybinds, just cancel directly no matter what
    if (ConfigTab.CurrentlySettingKeybind is not null)
      return true;

    var controlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    var shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    var altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

    if (!controlPressed && hotkey.Control)
      return true;

    if (!shiftPressed && hotkey.Shift)
      return true;

    if (!altPressed && hotkey.Alt)
      return true;

    return false;
  }

  public static void GetKeybind(out bool control, out bool shift, out bool alt, out KeyCode key)
  {
    foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
    {
      if (!Input.GetKeyDown(keyCode)) continue;
      if (keyCode == KeyCode.Escape) continue;
      if (Enumerable.Contains(BlacklistedKeys, keyCode) || Enumerable.Contains(ModKeys, keyCode)) continue;

      control = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
      shift = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
      alt = Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
      key = keyCode;
    }

    control = false;
    shift = false;
    alt = false;
    key = KeyCode.None;
  }
}
