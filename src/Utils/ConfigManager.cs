using MelonLoader;

using Hexa.NET.ImGui;

namespace PlayerList.Utils;

public enum PositionEnum
{
  TopLeft = 0,
  TopRight = 1,
  BottomLeft = 2,
  BottomRight = 3,
}

internal static class ConfigManager
{
  /// <summary>
  /// Categories for settings
  /// </summary>
  public static MelonPreferences_Category GeneralCategory { get; private set; }
  public static MelonPreferences_Category KeybindCategory { get; private set; }

  /// <summary>
  /// General settings entries
  /// </summary>
  public static MelonPreferences_Entry<PositionEnum> Position { get; private set; }
  public static MelonPreferences_Entry<int> FontSize { get; private set; }
  public static MelonPreferences_Entry<float> Opacity { get; private set; }

  /// <summary>
  /// Settings with keybind support
  /// </summary>
  public static ConfigWithKeybind<bool> EnableMenu { get; private set; }
  public static ConfigWithKeybind<bool> DisplayUsernames { get; private set; }

  public static void Setup()
  {
    // Create preferences categories.
    // MelonPreferences automatically saves to the mod's config file in the user data folder.
    GeneralCategory = MelonPreferences.CreateCategory("General", "General settings", false, true);
    KeybindCategory = MelonPreferences.CreateCategory("Keybinds", "Keybind settings", false, true);

    // Create entries in the General category.
    Position = GeneralCategory.CreateEntry("Position", PositionEnum.TopLeft, "Where to place the menu on the screen");
    // Assumes FontsManager.DefaultFontSize is defined elsewhere in your project.
    FontSize = GeneralCategory.CreateEntry("FontSize", FontsManager.DefaultFontSize, "Menu font size");
    Opacity = GeneralCategory.CreateEntry("Opacity", 0.8f, "Menu opacity");

    // Create keybind-enabled settings.
    EnableMenu = new ConfigWithKeybind<bool>("EnableMenu", true, false, false, false, ImGuiKey.F9);
    DisplayUsernames = new ConfigWithKeybind<bool>("DisplayUsernames", false, false, false, false, ImGuiKey.F10);

    // Always set the menu to on by default.
    EnableMenu.Value = true;
    // GeneralCategory.SaveToFile();
    // KeybindCategory.SaveToFile();
  }

  public static void ResetSettings()
  {
    EnableMenu.Value = EnableMenu.DefaultValue;
    DisplayUsernames.Value = DisplayUsernames.DefaultValue;
    Position.Value = Position.DefaultValue;
    FontSize.Value = FontSize.DefaultValue;
    Opacity.Value = Opacity.DefaultValue;

    // Update the window's transparency.
    ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg].W = Opacity.Value;
  }

  public static void ResetKeybinds()
  {
    EnableMenu.Keybind.Reset();
    DisplayUsernames.Keybind.Reset();
  }
}

internal class ConfigWithKeybind<T>
{
  private readonly MelonPreferences_Entry<T> _value;

  public string Name { get; }
  public T DefaultValue => _value.DefaultValue;
  public T Value
  {
    get => _value.Value;
    set => _value.Value = value;
  }
  public Keybind Keybind { get; }

  public ConfigWithKeybind(string name, T defaultValue, bool control, bool shift, bool alt, ImGuiKey key)
  {
    Name = name;
    // Create the general setting entry.
    _value = ConfigManager.GeneralCategory.CreateEntry(name, defaultValue);
    // Initialize the keybind using the Keybind category.
    Keybind = new Keybind(name, control, shift, alt, key);
  }
}

/// <summary>
/// Represents a keybind configuration using MelonLoader's preferences.
/// </summary>
internal class Keybind
{
  private readonly MelonPreferences_Entry<bool> control;
  private readonly MelonPreferences_Entry<bool> shift;
  private readonly MelonPreferences_Entry<bool> alt;
  private readonly MelonPreferences_Entry<ImGuiKey> key;
  public readonly string Name;

  public Keybind(string name, bool controlDefault, bool shiftDefault, bool altDefault, ImGuiKey keyDefault)
  {
    Name = name;

    // Create entries in the Keybinds category.
    control = ConfigManager.KeybindCategory.CreateEntry($"{name}_Ctrl", controlDefault);
    shift = ConfigManager.KeybindCategory.CreateEntry($"{name}_Shift", shiftDefault);
    alt = ConfigManager.KeybindCategory.CreateEntry($"{name}_Alt", altDefault);
    key = ConfigManager.KeybindCategory.CreateEntry($"{name}_Key", keyDefault);
  }

  public bool Control
  {
    get => control.Value;
    set => control.Value = value;
  }
  public bool Shift
  {
    get => shift.Value;
    set => shift.Value = value;
  }
  public bool Alt
  {
    get => alt.Value;
    set => alt.Value = value;
  }
  public ImGuiKey Key
  {
    get => key.Value;
    set => key.Value = value;
  }

  public void Reset()
  {
    control.Value = control.DefaultValue;
    shift.Value = shift.DefaultValue;
    alt.Value = alt.DefaultValue;
    key.Value = key.DefaultValue;
  }
}
