using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using PlayerList.GUI;
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

    var renderer = new Renderer();
    Task.Run(renderer.Run);
  }
}
