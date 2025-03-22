using HarmonyLib;

using Photon.Pun;

using PlayerList.GUI.Tabs;

namespace PlayerList.Patches;

internal static class PhotonHandlerPatch
{
  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnJoinedRoom))]
  [HarmonyPostfix]
  private static void OnJoinedRoomPatch()
  {
    foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
      PlayersTab.Add(player);
  }

  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnPlayerEnteredRoom))]
  [HarmonyPostfix]
  private static void OnPlayerEnteredRoomPatch(Photon.Realtime.Player newPlayer) => PlayersTab.Add(newPlayer);

  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnPlayerLeftRoom))]
  [HarmonyPostfix]
  private static void OnPlayerLeftRoomPatch()
  {
    PlayersTab.Clear();

    foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
      PlayersTab.Add(player);
  }
}
