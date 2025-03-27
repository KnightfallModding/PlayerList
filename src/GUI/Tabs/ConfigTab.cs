using System;
using System.Drawing;
using System.Numerics;

using Hexa.NET.ImGui;

using PlayerList.Utils;

namespace PlayerList.GUI.Tabs;

internal static class ConfigTab
{
  private static string UUID = "";
  private static string prefix = "";
  private static string suffix = "";

#nullable enable
  public static Keybind? CurrentlySettingKeybind { get; set; }
#nullable disable

  public static void Render()
  {
    if (ImGui.BeginTabItem("Config"))
    {
      GeneralCategory();
      KeybindsCategory();
      DangerZoneCategory();
      AdvancedCategory();
      AdminCategory();
      ImGui.EndTabItem();
    }
  }

  private static void GeneralCategory()
  {
    ChangePositionPicker();
    ToggleMenuCheckbox();
    ImGui.SameLine();
    ToggleUsernamesCheckbox();
    ChangeOpacitySlider();
  }

  private static void ChangePositionPicker()
  {
    PositionEnum currentPosition = ConfigManager.Position.Value;

    if (ImGui.BeginCombo("Position", currentPosition.ToString(), ImGuiComboFlags.None))
    {
      foreach (string position in Enum.GetNames(typeof(PositionEnum)))
      {
        bool isSelected = currentPosition.ToString() == position;

        if (ImGui.Selectable(position, isSelected))
          ConfigManager.Position.Value = (PositionEnum)Enum.Parse(typeof(PositionEnum), position);
      }

      ImGui.EndCombo();
    }
  }

  private static void ToggleMenuCheckbox()
  {
    bool isMenuEnabled = Renderer.IsVisible;

    _ = ImGui.Checkbox("Enable menu", ref isMenuEnabled);
    if (isMenuEnabled != Renderer.IsVisible)
      Renderer.ToggleMenu();
  }

  private static void ToggleUsernamesCheckbox()
  {
    bool areUsernamesDisplayed = ConfigManager.DisplayUsernames.Value;

    _ = ImGui.Checkbox("Display usernames", ref areUsernamesDisplayed);
    if (areUsernamesDisplayed != ConfigManager.DisplayUsernames.Value)
      Renderer.ToggleUsernames();
  }

  private static void ChangeOpacitySlider()
  {
    Vector4 windowBg = ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg];
    var currentAlpha = (int)(windowBg.W * 100);

    if (ImGui.SliderInt("Opacity", ref currentAlpha, 0, 100) && currentAlpha / 100f != windowBg.W)
    {
      ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg].W = currentAlpha / 100f;
    }
    else if (ImGui.IsItemDeactivated())
    {
      ConfigManager.Opacity.Value = currentAlpha / 100f;
    }
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
    if (CurrentlySettingKeybind is null)
      return;

    InputsManager.GetKeybind(out bool control, out bool shift, out bool alt, out ImGuiKey key);
    if (key == ImGuiKey.None)
      return;

    CurrentlySettingKeybind.Control = control;
    CurrentlySettingKeybind.Shift = shift;
    CurrentlySettingKeybind.Alt = alt;
    CurrentlySettingKeybind.Key = key;

    CurrentlySettingKeybind = null;
  }

  private static void KeybindGroup<T>(string name, ConfigWithKeybind<T> config)
  {
    var currentKey = "";
    Keybind keybind = config.Keybind;
    if (keybind.Control)
      currentKey += "Ctrl + ";

    if (keybind.Shift)
      currentKey += "Shift + ";

    if (keybind.Alt)
      currentKey += "Alt + ";

    currentKey += keybind.Key;

    ImGui.BeginGroup();
    ImGui.AlignTextToFramePadding();
    ImGui.Text(name);
    ImGui.SameLine();
    if (CurrentlySettingKeybind is null)
    {
      if (ImGui.Button($"{currentKey}###{config.Name}"))
        CurrentlySettingKeybind = config.Keybind;
    }
    else
    {
      _ = (CurrentlySettingKeybind == config.Keybind)
        ? ImGui.Button($"...###{config.Name}")
        : ImGui.Button($"{currentKey}###{config.Name}");
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

    if (ImGui.Button("Reset settings"))
      ConfigManager.ResetSettings();

    ImGui.SameLine();
    if (ImGui.Button("Reset keybinds"))
      ConfigManager.ResetKeybinds();

    ImGui.PopStyleColor(3);
  }

  private static void AdvancedCategory()
  {
    ImGui.NewLine();
    if (ImGui.CollapsingHeader("⚙️Advanced"))
      FontSizeSlider();
  }

  private static void FontSizeSlider()
  {
    int currentFontSize = ConfigManager.FontSize.Value;

    if (ImGui.SliderInt("FontSize", ref currentFontSize, FontsManager.MinFontSize, FontsManager.MaxFontSize) && currentFontSize != ConfigManager.FontSize.Value)
    {
      ConfigManager.FontSize.Value = currentFontSize;
    }
    else if (ImGui.IsItemDeactivated())
    {
      Renderer.ChangeFontSize(currentFontSize);
    }
  }

  private static void AdminCategory()
  {
#if IS_ADMIN
    ImGui.NewLine();
    if (ImGui.CollapsingHeader("🛠️Admin"))
    {
      UUIDInput();
      PrefixInput();
      SuffixInput();
      ConfirmButton();
    }
#endif
  }

  private static void UUIDInput()
  {
    _ = ImGui.InputTextWithHint("##UUID", "UUID", ref UUID, 24);
  }

  private static void PrefixInput()
  {
    _ = ImGui.InputTextWithHint("##Prefixes", "Prefix", ref prefix, 5);
  }
  private static void SuffixInput()
  {
    _ = ImGui.InputTextWithHint("##Suffixes", "Suffix", ref suffix, 5);
  }

  private static void ConfirmButton()
  {
    Color orange = Color.Orange;
    var color = new Vector4(orange.R, orange.G, orange.B, 1f);
    ImGui.TextColored(color, "Not implemented yet...");

    _ = ImGui.Button("Confirm");

    // if (ImGui.Button("Confirm"))
    // {
    //   // TODO: Write the API code here
    //   _ = Environment.GetEnvironmentVariable("API_TOKEN");
    //   var player = new CustomAPIPlayer(UUID, null, null, null);
    //   System.Net.Http.HttpResponseMessage result = await API.AddCustomPlayer(player);

    //   if (result.StatusCode != System.Net.HttpStatusCode.OK)
    //   {
    //     var color = new Vector4(1f, 0f, 0f, 1f);
    //     ImGui.TextColored(color, result.Content.ToString());
    //   }
    //   else
    //   {
    //     var color = new Vector4(0f, 1f, 0f, 1f);
    //     ImGui.TextColored(color, result.Content.ToString());
    //   }

    //   UUID = "";
    //   prefix = "";
    //   suffix = "";
    // }
  }
}
