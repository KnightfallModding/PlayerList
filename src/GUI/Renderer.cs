using System.IO;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using PlayerList.Utils;
using UnityEngine;

namespace PlayerList.GUI;

public class Renderer : Overlay
{
  public Renderer() : base(MyPluginInfo.PLUGIN_NAME, true, Screen.width, Screen.height)
  {
    FPSLimit = 30;
    VSync = false;

    Plugin.Log.LogDebug("Renderer has been created.");
  }

  protected override unsafe Task PostInitialized()
  {
    var fontPath = Path.Combine(BepInEx.Paths.PluginPath, MyPluginInfo.PLUGIN_NAME, "assets", "fonts");
    const string fontName = "UbuntuMonoNerdFontMono";

    ReplaceFont(_ => FontsManager.Setup(fontPath, fontName));

    return Task.CompletedTask;
  }

  protected override void Render()
  {
    // TODO: Create UI
  }
}
