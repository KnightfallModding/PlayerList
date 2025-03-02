using System.Threading.Tasks;
using ClickableTransparentOverlay;
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

  protected override Task PostInitialized()
  {
    return Task.CompletedTask;
  }

  protected override void Render()
  {
    // TODO: Create UI
  }
}
