using HarmonyLib;
using Photon.Pun;
using PlayerList.GUI.Tabs;

namespace PlayerList.Patches;

public static class PhotonHandlerPatch
{
  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnJoinedRoom))]
  [HarmonyPostfix]
  static void OnJoinedRoomPatch()
  {
    foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
    {
      PlayersTab.Add(player);
    }
  }

  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnPlayerEnteredRoom))]
  [HarmonyPostfix]
  static void OnPlayerEnteredRoomPatch(Photon.Realtime.Player newPlayer) => PlayersTab.Add(newPlayer);

  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnPlayerLeftRoom))]
  [HarmonyPostfix]
  static void OnPlayerLeftRoomPatch()
  {
    PlayersTab.Clear();

    foreach (var player in PhotonNetwork.CurrentRoom.Players.Values) PlayersTab.Add(player);
  }
}
