using Hexa.NET.ImGui;
using PlayerList.GUI;
using PlayerList.Utils;

namespace PlayerList.Tabs;

public static class ConfigTab
{
  public static string CurrentlySettingKeybind { get; set; } = "";

  public static void Render()
  {
    if (ImGui.BeginTabItem("Config"))
    {
      ToggleMenuCheckbox();
      ImGui.SameLine();
      DisplayUsernamesCheckbox();
      ImGui.EndTabItem();
    }
  }

  private static unsafe void ToggleMenuCheckbox()
  {
    // var isMenuEnabled = ConfigManager.EnableMenu.Value;
    var isMenuEnabled = Renderer.IsVisible;

    ImGui.Checkbox("Enable menu", &isMenuEnabled);
    if (isMenuEnabled != Renderer.IsVisible) ConfigManager.EnableMenu.Value = isMenuEnabled;
  }

  private static unsafe void DisplayUsernamesCheckbox()
  {
    var areUsernamesDisplayed = ConfigManager.DisplayUsernames.Value;

    ImGui.Checkbox("Display usernames", &areUsernamesDisplayed);
    if (areUsernamesDisplayed != ConfigManager.DisplayUsernames.Value) ConfigManager.DisplayUsernames.Value = areUsernamesDisplayed;
  }
}
