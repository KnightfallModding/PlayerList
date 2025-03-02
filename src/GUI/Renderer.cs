using BepInEx;
using ClickableTransparentOverlay;
using Hexa.NET.ImGui;
using HexaGen.Runtime;
using PlayerList.ConfigManager;
using PlayerList.Tabs;
using PlayerList.Utils;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

namespace PlayerList.GUI;

public class Renderer : Overlay
{
  public static bool IsVisible { get; set; } = ConfigManager.ConfigManager.EnableMenu.Value;

  public Renderer() : base(MyPluginInfo.PLUGIN_NAME, true, Screen.width, Screen.height)
  {
    // Avoid `cimgui.dll` not being detected
    LibraryLoader.CustomLoadFolders.Add(Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME, "runtime"));

    FPSLimit = 30;
    VSync = false;

    Plugin.Log.LogDebug("Renderer has been created.");
  }

  protected override unsafe Task PostInitialized()
  {
    StartProcessHider();

    var fontPath = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME, "assets", "fonts");
    const string fontName = "UbuntuMonoNerdFontMono";
    const string emojisFontName = "Twemoji";
    ReplaceFont(__ => _ = new FontsManager(fontPath, fontName, emojisFontName));

    return Task.CompletedTask;
  }

  protected override void Render()
  {
    if (!IsVisible || !ConfigManager.ConfigManager.EnableMenu.Value) return;

    MoveWindow();

    ImGui.PushFont(FontsManager.RegularFont);
    ImGui.Begin(MyPluginInfo.PLUGIN_NAME);
    ImGui.BeginTabBar(MyPluginInfo.PLUGIN_GUID);
    PlayersTab.Render();
    ConfigTabs.Render();
    ImGui.EndTabBar();
    ImGui.End();
    ImGui.PopFont();
  }

  private static void StartProcessHider()
  {
    ProcessUtils.OverlayWindowHandle = ProcessUtils.FindWindow(null, MyPluginInfo.PLUGIN_NAME);
    ProcessUtils.HideOverlayFromTaskbar();

    // TODO: Improve visibility detection instead of just focus.
    ProcessUtils.GameFocusChanged += (sender, @event) =>
    {
      Plugin.Log.LogInfo($"Window fcocus changed: {@event.IsFocused}. Sender: {sender}");

      CursorUnlocker.IsCursorUnlocked = @event.FocusedWindow == FocusedWindow.Overlay;
      IsVisible = @event.IsFocused;
    };
  }

  private void MoveWindow()
  {
    var screenWidth = window.Dimensions.Width;
    var screenHeight = window.Dimensions.Height;
    var windowWidth = ImGui.GetWindowWidth();
    var windowHeight = ImGui.GetWindowHeight();

    switch (ConfigManager.ConfigManager.Position.Value)
    {
      case PositionEnum.TopLeft:
        ImGui.SetNextWindowPos(new(0, 0));
        break;

      case PositionEnum.TopRight:
        ImGui.SetNextWindowPos(new(screenWidth - windowWidth, 0));
        break;

      case PositionEnum.BottomLeft:
        ImGui.SetNextWindowPos(new(0, screenHeight - windowHeight));
        break;

      case PositionEnum.BottomRight:
        ImGui.SetNextWindowPos(new(screenWidth - windowWidth, 0));
        break;
    }
  }
}
