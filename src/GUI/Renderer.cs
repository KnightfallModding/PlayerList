using System.IO;
using System.Threading.Tasks;
using BepInEx;
using ClickableTransparentOverlay;
using Hexa.NET.ImGui;
using HexaGen.Runtime;
using PlayerList.Utils;
using UnityEngine;

namespace PlayerList.GUI;

public class Renderer : Overlay
{
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
    var fontPath = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME, "assets", "fonts");
    const string fontName = "UbuntuMonoNerdFontMono";
    const string emojisFontName = "Twemoji";
    ReplaceFont(__ => _ = new FontsManager(fontPath, fontName, emojisFontName));

    return Task.CompletedTask;
  }

  protected override void Render()
  {
    ImGui.PushFont(FontsManager.BoldItalicFont);
    ImGui.Begin(MyPluginInfo.PLUGIN_NAME);
    // TODO: Create the UI
    ImGui.End();
    ImGui.PopFont();
  }
}
