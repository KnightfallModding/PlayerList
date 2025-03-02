using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BepInEx;
using ClickableTransparentOverlay;
using Hexa.NET.ImGui;
using HexaGen.Runtime;
using PlayerList.Config;
using PlayerList.Utils;
using UnityEngine;

namespace PlayerList.GUI;

public class Renderer : Overlay
{
  public static bool IsVisible = ConfigManager.EnableMenu.Value;

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
    if (!IsVisible) return;

    ImGui.PushFont(FontsManager.RegularFont);
    ImGui.Begin(MyPluginInfo.PLUGIN_NAME);
    // TODO: Create the UI
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
      Plugin.Log.LogInfo($"Window focus changed: {@event.IsFocused}. Sender: {sender}");

      CursorUnlocker.IsCursorUnlocked = @event.FocusedWindow == FocusedWindow.Overlay;
      IsVisible = @event.IsFocused;
    };
  }
}
