using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using PlayerList.ConfigManager;
using PlayerList.GUI;
using PlayerList.Utils;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PlayerList;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
  internal static new ManualLogSource Log;

  public override void Load()
  {
    // Plugin startup logic
    Log = base.Log;
    Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

    ProcessUtils.Process = Process.GetProcessesByName("Knightfall")[0];
    ProcessUtils.GameWindowHandle = ProcessUtils.FindWindow(null, "Knightfall");
    ConfigManager.ConfigManager.Setup();

    var renderer = new Renderer();
    Task.Run(renderer.Run);
  }
}
