using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using MelonLoader;

using PlayerList;
using PlayerList.GUI.Tabs;
using PlayerList.Utils;

using UnityEngine;

[assembly: MelonInfo(typeof(PlayerListMod), PlayerListModInfo.MOD_NAME, PlayerListModInfo.MOD_VERSION, PlayerListModInfo.MOD_AUTHOR, $"{PlayerListModInfo.MOD_LINK}/releases/latest/download/{PlayerListModInfo.MOD_NAME}.zip")]
[assembly: MelonGame("Landfall Games", "Knightfall")]

namespace PlayerList;

internal class PlayerListMod : MelonMod
{
  public override async void OnInitializeMelon()
  {
    LoggerInstance.Msg($"Plugin {PlayerListModInfo.MOD_GUID} is loaded!");

    ProcessUtils.Process = Process.GetProcessesByName("Knightfall")[0];
    ProcessUtils.GameWindowHandle = ProcessUtils.FindWindow(null, "Knightfall");
    ConfigManager.Setup();

    try
    {
      PlayersTab.CustomPlayers = await API.FetchCustomPlayers();
    }
    catch
    {
      PlayersTab.CustomPlayers = new List<APIPlayer>();
    }

    var renderer = new GUI.Renderer();
    _ = Task.Run(() => renderer.Run());

    var managerGo = new GameObject(PlayerListModInfo.MOD_NAME) { hideFlags = HideFlags.HideAndDontSave };
    _ = managerGo.AddComponent<InputsManager>();
  }
}
