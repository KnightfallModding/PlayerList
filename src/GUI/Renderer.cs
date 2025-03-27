using System.IO;
using System.Threading.Tasks;

using ClickableTransparentOverlay;

using Hexa.NET.ImGui;

using HexaGen.Runtime;

using MelonLoader;
using MelonLoader.Utils;

using PlayerList.GUI.Skins.Default;
using PlayerList.GUI.Tabs;
using PlayerList.Utils;

using UnityEngine;

namespace PlayerList.GUI;

internal class Renderer : Overlay
{
  public static bool IsVisible { get; set; } = true;

  private float windowWidth;
  private float windowHeight;

  public Renderer() : base(PlayerListModInfo.MOD_NAME, true, Screen.width, Screen.height)
  {
    // Avoid `cimgui.dll` not being detected
    // LibraryLoader.CustomLoadFolders.Add(Path.Combine(Paths.PluginPath, MyPluginInfo.MOD_NAME, "runtime"));
    LibraryLoader.CustomLoadFolders.Add(Path.Combine(MelonEnvironment.ModsDirectory, PlayerListModInfo.MOD_NAME));

    FPSLimit = 30;
    VSync = false;

    MelonDebug.Msg("Renderer has been created.");
  }

  protected override unsafe Task PostInitialized()
  {
    StartProcessHider();

    // string fontPath = Path.Combine(Paths.PluginPath, MyPluginInfo.MOD_NAME, "assets", "fonts");
    string fontPath = Path.Combine(MelonEnvironment.ModsDirectory, PlayerListModInfo.MOD_NAME, "assets", "fonts");
    const string fontName = "UbuntuMonoNerdFontMono";
    const string emojisFontName = "Twemoji";
    _ = ReplaceFont(__ => _ = new FontsManager(fontPath, fontName, emojisFontName));
    LoadStyle();

    return Task.CompletedTask;
  }

  protected override void Render()
  {
    if (!IsVisible || !ConfigManager.EnableMenu.Value)
      return;

    MoveWindow();

    ImGui.PushFont(FontsManager.RegularFont);
    _ = ImGui.Begin(PlayerListModInfo.MOD_NAME, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoNav);
    _ = ImGui.BeginTabBar(PlayerListModInfo.MOD_GUID);

    windowWidth = ImGui.GetWindowWidth();
    windowHeight = ImGui.GetWindowHeight();

    InputsManager.DetectImGuiKeybinds();
    PlayersTab.Render();
    ConfigTab.Render();
    ImGui.EndTabBar();
    ImGui.End();
    ImGui.PopFont();
  }

  private static void StartProcessHider()
  {
    ProcessUtils.OverlayWindowHandle = ProcessUtils.FindWindow(null, PlayerListModInfo.MOD_NAME);
    ProcessUtils.HideOverlayFromTaskbar();

    // TODO: Improve visibility detection instead of just focus.
    ProcessUtils.GameFocusChanged += static (_, @event) =>
    {
      MelonDebug.Msg($"Window focus changed: {@event.IsFocused}.");

      CursorUnlocker.IsCursorUnlocked = @event.FocusedWindow == FocusedWindow.Overlay;
      IsVisible = @event.IsFocused;
    };
  }

  private void MoveWindow()
  {
    int screenWidth = window.Dimensions.Width;
    int screenHeight = window.Dimensions.Height;

    switch (ConfigManager.Position.Value)
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
        ImGui.SetNextWindowPos(new(screenWidth - windowWidth, screenHeight - windowHeight));
        break;
    }
  }

  private static void LoadStyle() => DefaultSkin.Setup();

  public static void ToggleMenu()
  {
    ConfigManager.EnableMenu.Value = !ConfigManager.EnableMenu.Value;
    IsVisible = ConfigManager.EnableMenu.Value;

    if (!IsVisible || !ConfigManager.EnableMenu.Value)
      ProcessUtils.FocusGame();
  }

  public static void ToggleUsernames() => ConfigManager.DisplayUsernames.Value = !ConfigManager.DisplayUsernames.Value;

  public static void ChangeFontSize(int fontSize)
  {
    ImGui.GetFont().Scale = fontSize / (float)FontsManager.DefaultFontSize;
    ConfigManager.FontSize.Value = fontSize;
  }
}
