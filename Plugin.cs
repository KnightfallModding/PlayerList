using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppSystem.Runtime.InteropServices;
using PlayerList.GUI;
using PlayerList.GUI.Tabs;
using PlayerList.Patches;
using PlayerList.Utils;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PlayerList;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
  internal static new ManualLogSource Log;

  public override async void Load()
  {
    // Plugin startup logic
    Log = base.Log;
    Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

    AddComponent<InputsManager>();

    ProcessUtils.Process = Process.GetProcessesByName("Knightfall")[0];
    ProcessUtils.GameWindowHandle = ProcessUtils.FindWindow(null, "Knightfall");
    ConfigManager.Setup();

    try
    { PlayersTab.CustomPlayers = await API.FetchCustomPlayers(); }
    catch
    { PlayersTab.CustomPlayers = []; }

    var renderer = new Renderer();
    _ = Task.Run(renderer.Run);

    Harmony.CreateAndPatchAll(typeof(PhotonHandlerPatch));
    Harmony.CreateAndPatchAll(typeof(PhotonNetworkPatch));
  }
}
