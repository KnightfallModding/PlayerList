using Hexa.NET.ImGui;
using PlayerList.GUI;

namespace PlayerList.Tabs;

public static class ConfigTabs
{
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
    if (isMenuEnabled != Renderer.IsVisible) ConfigManager.ConfigManager.EnableMenu.Value = isMenuEnabled;
  }

  private static unsafe void DisplayUsernamesCheckbox()
  {
    var areUsernamesDisplayed = ConfigManager.ConfigManager.DisplayUsernames.Value;

    ImGui.Checkbox("Display usernames", &areUsernamesDisplayed);
    if (areUsernamesDisplayed != ConfigManager.ConfigManager.DisplayUsernames.Value) ConfigManager.ConfigManager.DisplayUsernames.Value = areUsernamesDisplayed;
  }
}
