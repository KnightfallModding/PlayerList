using BepInEx;
using BepInEx.Configuration;
using Hexa.NET.ImGui;
using System.IO;

namespace PlayerList.Utils;

public enum PositionEnum
{
  TopLeft,
  TopRight,
  BottomLeft,
  BottomRight
};

public static class ConfigManager
{
  public static ConfigFile File { get; private set; }

  public static ConfigEntry<PositionEnum> Position { get; private set; }
  public static ConfigEntry<int> FontSize { get; private set; }
  public static ConfigEntry<float> Opacity { get; private set; }
  public static ConfigWithKeybind<bool> EnableMenu { get; private set; }
  public static ConfigWithKeybind<bool> DisplayUsernames { get; private set; }

  public static void Setup()
  {
    File = new ConfigFile(Path.Join(Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.cfg"), true);

    EnableMenu = new ConfigWithKeybind<bool>(File, "EnableMenu", true, false, false, false, ImGuiKey.F9);
    DisplayUsernames = new ConfigWithKeybind<bool>(File, "DisplayUsernames", false, false, false, false, ImGuiKey.F10);
    Position = File.Bind("General", "Position", PositionEnum.TopLeft, "Where to place the menu on the screen");
    FontSize = File.Bind("General", "FontSize", FontsManager.DefaultFontSize);
    Opacity = File.Bind("General", "Opacity", .8f);

    // Always set the menu to on by default
    EnableMenu.Value = true;
  }

  public static void ResetSettings()
  {
    EnableMenu.Value = EnableMenu.DefaultValue;
    DisplayUsernames.Value = DisplayUsernames.DefaultValue;
    Position.Value = (PositionEnum)Position.DefaultValue;
    FontSize.Value = (int)FontSize.DefaultValue;
    Opacity.Value = (float)Opacity.DefaultValue;

    // Update the window's transparency
    ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg].W = Opacity.Value;
  }

  public static void ResetKeybinds()
  {
    EnableMenu.Keybind.Reset();
    DisplayUsernames.Keybind.Reset();
  }
}

public class ConfigWithKeybind<T>(ConfigFile config, string name, T value = default, bool control = false, bool shift = false, bool alt = false, ImGuiKey key = ImGuiKey.None)
{
  private readonly ConfigEntry<T> _value = config.Bind("General", name, value);

  public string Name { get => name; }
  public T DefaultValue { get => (T)_value.DefaultValue; }
  public T Value { get => _value.Value; set => _value.Value = value; }
  public Keybind Keybind { get; } = new Keybind(config, name, control, shift, alt, key);
}

/// <summary>
/// Creates a new keybind configuration.
/// </summary>
/// <param name="file">Your BepInEx config file.</param>
/// <param name="name">The base name for this keybind (e.g. "ToggleDisplay").</param>
/// <param name="section">The config section to use (default is "Keybinds").</param>
public class Keybind(ConfigFile file, string name, bool control, bool shift, bool alt, ImGuiKey key, string section = "Keybinds")
{
  // Private config entries for each property.
  private readonly ConfigEntry<bool> control = file.Bind(section, $"{name}.Ctrl", control, $"Ctrl modifier for {name}");
  private readonly ConfigEntry<bool> shift = file.Bind(section, $"{name}.Shift", shift, $"Shift modifier for {name}");
  private readonly ConfigEntry<bool> alt = file.Bind(section, $"{name}.Alt", alt, $"Alt modifier for {name}");
  private readonly ConfigEntry<ImGuiKey> key = file.Bind(section, $"{name}.Key", key, $"Key for {name}");

  // Public properties exposing the values.
  public bool Control { get => control.Value; set => control.Value = value; }
  public bool Shift { get => shift.Value; set => shift.Value = value; }
  public bool Alt { get => alt.Value; set => alt.Value = value; }
  public ImGuiKey Key { get => key.Value; set => key.Value = value; }

  public void Reset()
  {
    control.Value = (bool)control.DefaultValue;
    shift.Value = (bool)shift.DefaultValue;
    alt.Value = (bool)alt.DefaultValue;
    key.Value = (ImGuiKey)key.DefaultValue;
  }
}
