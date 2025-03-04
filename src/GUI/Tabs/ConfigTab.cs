using Hexa.NET.ImGui;
using PlayerList.Utils;
using System.Numerics;

namespace PlayerList.GUI.Tabs;

public static class ConfigTab
{
#nullable enable
  public static Keybind? CurrentlySettingKeybind { get; set; } = null;
#nullable disable

  public static void Render()
  {
    if (ImGui.BeginTabItem("Config"))
    {
      GeneralCategory();
      KeybindsCategory();
      DangerZoneCategory();
      AdvancedCategory();
      ImGui.EndTabItem();
    }
  }

  private static void GeneralCategory()
  {
    ToggleMenuCheckbox();
    ImGui.SameLine();
    ToggleUsernamesCheckbox();
    ChangeOpacitySlider();
  }

  private static void ToggleMenuCheckbox()
  {
    var isMenuEnabled = Renderer.IsVisible;

    ImGui.Checkbox("Enable menu", ref isMenuEnabled);
    if (isMenuEnabled != Renderer.IsVisible) Renderer.ToggleMenu();
  }

  private static void ToggleUsernamesCheckbox()
  {
    var areUsernamesDisplayed = ConfigManager.DisplayUsernames.Value;

    ImGui.Checkbox("Display usernames", ref areUsernamesDisplayed);
    if (areUsernamesDisplayed != ConfigManager.DisplayUsernames.Value) Renderer.ToggleUsernames();
  }

  private static void ChangeOpacitySlider()
  {
    var windowBg = ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg];
    var currentAlpha = (int)(windowBg.W * 100);

    if (ImGui.SliderInt("Opacity", ref currentAlpha, 0, 100) && currentAlpha / 100f != windowBg.W)
      ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg].W = currentAlpha / 100f;
    else if (ImGui.IsItemDeactivated())
      ConfigManager.Opacity.Value = currentAlpha / 100f;
  }

  private static void KeybindsCategory()
  {
    SetKeybind();

    ImGui.NewLine();
    ImGui.SeparatorText("⌨️Keybinds");
    KeybindGroup("Toggle menu", ConfigManager.EnableMenu);
    KeybindGroup("Toggle usernames", ConfigManager.DisplayUsernames);
  }

  private static void SetKeybind()
  {
    if (CurrentlySettingKeybind == null) return;

    InputsManager.GetKeybind(out var control, out var shift, out var alt, out var key);
    if (key == ImGuiKey.None) return;

    CurrentlySettingKeybind.Control = control;
    CurrentlySettingKeybind.Shift = shift;
    CurrentlySettingKeybind.Alt = alt;
    CurrentlySettingKeybind.Key = key;

    CurrentlySettingKeybind = null;
  }

  private static void KeybindGroup<T>(string name, ConfigWithKeybind<T> config)
  {
    var currentKey = "";
    var keybind = config.Keybind;
    if (keybind.Control) currentKey += "Ctrl +";
    if (keybind.Shift) currentKey += "Shift +";
    if (keybind.Alt) currentKey += "Alt +";
    currentKey += keybind.Key;

    ImGui.BeginGroup();
    ImGui.AlignTextToFramePadding();
    ImGui.Text(name);
    ImGui.SameLine();
    if (CurrentlySettingKeybind == null)
    {
      if (ImGui.Button($"{currentKey}###{config.Name}")) CurrentlySettingKeybind = config.Keybind;
    }
    else
    {
      if (CurrentlySettingKeybind == config.Keybind) ImGui.Button($"...###{config.Name}");
      else ImGui.Button($"{currentKey}###{config.Name}");
    }
    ImGui.EndGroup();
  }

  private static void DangerZoneCategory()
  {
    ImGui.NewLine();
    ImGui.SeparatorText("⚠️DANGER ZONE");
    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(.4f, 0, 0, 1f));
    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(.8f, 0, 0, 1));
    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(.8f, 0, 0, 1));

    if (ImGui.Button("Reset settings")) ConfigManager.ResetSettings();
    ImGui.SameLine();
    if (ImGui.Button("Reset keybinds")) ConfigManager.ResetKeybinds();

    ImGui.PopStyleColor(3);
  }

  private static void AdvancedCategory()
  {
    ImGui.NewLine();
    if (ImGui.CollapsingHeader("Advanced"))
    {
      FontSizeSlider();
    }
  }

  private static void FontSizeSlider()
  {
    var currentFontSize = ConfigManager.FontSize.Value;

    if (ImGui.SliderInt("FontSize", ref currentFontSize, FontsManager.MinFontSize, FontsManager.MaxFontSize) && currentFontSize != ConfigManager.FontSize.Value)
    {
      ConfigManager.FontSize.Value = currentFontSize;
    }
    else if (ImGui.IsItemDeactivated())
    {
      ImGui.GetFont().Scale = currentFontSize / (float)FontsManager.DefaultFontSize;
      ConfigManager.FontSize.Value = currentFontSize;
    }
  }
}
