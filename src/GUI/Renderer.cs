using System.IO;
using Hexa.NET.ImGui;
using HexaGen.Runtime;
using MelonLoader;
using MelonLoader.Utils;
using PlayerList.GUI.Skins.Default;
using PlayerList.GUI.Tabs;
using PlayerList.Utils;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace PlayerList.GUI;

internal class Renderer
{
  private bool initialized;
  private float windowHeight;

  private float windowWidth;

  public Renderer()
  {
    LibraryLoader.CustomLoadFolders.Add(Path.Combine(MelonEnvironment.ModsDirectory, PlayerListModInfo.MOD_NAME));

    MelonDebug.Msg("Renderer has been created.");
  }

  private void Init()
  {
    var fontPath = Path.Combine(MelonEnvironment.ModsDirectory, PlayerListModInfo.MOD_NAME, "assets", "fonts");
    const string fontName = "UbuntuMonoNerdFontMono";
    const string emojisFontName = "Twemoji";
    _ = new FontsManager(fontPath, fontName, emojisFontName);
    LoadStyle();

    initialized = true;
  }

  internal void Render()
  {
    if (!initialized)
      Init();

    if (!ConfigManager.EnableMenu.Value)
      return;

    MoveWindow();

    _ = ImGui.Begin(PlayerListModInfo.MOD_NAME,
      ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoNav);
    _ = ImGui.BeginTabBar(PlayerListModInfo.MOD_GUID);

    windowWidth = ImGui.GetWindowWidth();
    windowHeight = ImGui.GetWindowHeight();

    ImGui.PushFont(null, ConfigManager.FontSize.Value);
    PlayersTab.Render();
    ConfigTab.Render();
    ImGui.EndTabBar();
    ImGui.PopFont();
    ImGui.End();
  }

  private void MoveWindow()
  {
    var screenWidth = Screen.width;
    var screenHeight = Screen.height;

    switch (ConfigManager.Position.Value)
    {
      case PositionEnum.TopLeft:
        ImGui.SetNextWindowPos(new Vector2(0, 0));
        break;
      case PositionEnum.TopRight:
        ImGui.SetNextWindowPos(new Vector2(screenWidth - windowWidth, 0));
        break;
      case PositionEnum.BottomLeft:
        ImGui.SetNextWindowPos(new Vector2(0, screenHeight - windowHeight));
        break;
      case PositionEnum.BottomRight:
        ImGui.SetNextWindowPos(new Vector2(screenWidth - windowWidth, screenHeight - windowHeight));
        break;
    }
  }

  private static void LoadStyle() => DefaultSkin.Setup();

  public static void ToggleMenu() => ConfigManager.EnableMenu.Value = !ConfigManager.EnableMenu.Value;

  public static void ToggleUsernames() => ConfigManager.DisplayUsernames.Value = !ConfigManager.DisplayUsernames.Value;

  public static void ChangeFontSize(int fontSize) => ConfigManager.FontSize.Value = fontSize;
}
