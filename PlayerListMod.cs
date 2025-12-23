using System.Collections.Generic;
using System.IO;
using DearImGuiInjection;
using MelonLoader;
using MelonLoader.Utils;
using PlayerList;
using PlayerList.GUI.Tabs;
using PlayerList.Utils;
using UnityEngine;
using Renderer = PlayerList.GUI.Renderer;

[assembly:
  MelonInfo(typeof(PlayerListMod), PlayerListModInfo.MOD_NAME, PlayerListModInfo.MOD_VERSION,
    PlayerListModInfo.MOD_AUTHOR, $"{PlayerListModInfo.MOD_LINK}/releases/latest/download/Release.zip")]
[assembly: MelonGame("Landfall Games", "Knightfall")]

namespace PlayerList;

internal class PlayerListMod : MelonMod
{
  public override async void OnInitializeMelon()
  {
    LoggerInstance.Msg($"Plugin {PlayerListModInfo.MOD_GUID} is loaded!");

    ConfigManager.Setup();

    try
    {
      PlayersTab.CustomPlayers = await API.FetchCustomPlayers();
    }
    catch
    {
      PlayersTab.CustomPlayers = new List<APIPlayer>();
    }

    var imGuiConfigPath = MelonEnvironment.GameRootDirectory;
    var assetsFolder = Path.Join(MelonEnvironment.PluginsDirectory, PlayerListModInfo.MOD_NAME, "Assets");
    ImGuiInjector.Init(imGuiConfigPath, assetsFolder, LoggerInstance);
    var renderer = new Renderer();
    ImGuiInjector.Render += renderer.Render;

    var managerGo = new GameObject(PlayerListModInfo.MOD_NAME) { hideFlags = HideFlags.HideAndDontSave };
    _ = managerGo.AddComponent<InputsManager>();
  }
}
