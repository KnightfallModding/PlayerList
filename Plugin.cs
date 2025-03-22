using System.Diagnostics;
using System.Threading.Tasks;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

using HarmonyLib;

using PlayerList.GUI;
using PlayerList.GUI.Tabs;
using PlayerList.Patches;
using PlayerList.Utils;

namespace PlayerList;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
internal class Plugin : BasePlugin
{
  new internal static ManualLogSource Log;

  public override async void Load()
  {
    // Plugin startup logic
    Log = base.Log;
    Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

    _ = AddComponent<InputsManager>();

    ProcessUtils.Process = Process.GetProcessesByName("Knightfall")[0];
    ProcessUtils.GameWindowHandle = ProcessUtils.FindWindow(null, "Knightfall");
    ConfigManager.Setup();

    try
    {
      PlayersTab.CustomPlayers = await API.FetchCustomPlayers();
    }
    catch
    {
      PlayersTab.CustomPlayers = new System.Collections.Generic.List<APIPlayer>();
    }

    var renderer = new Renderer();
    _ = Task.Run(() => renderer.Run());

    _ = Harmony.CreateAndPatchAll(typeof(PhotonHandlerPatch));
    _ = Harmony.CreateAndPatchAll(typeof(PhotonNetworkPatch));
  }
}
