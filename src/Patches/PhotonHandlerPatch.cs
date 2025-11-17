using HarmonyLib;
using Il2CppPhoton.Pun;
using Il2CppPhoton.Realtime;
using PlayerList.GUI.Tabs;

namespace PlayerList.Patches;

[HarmonyPatch]
internal static class Il2CppPhotonHandlerPatch
{
  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnJoinedRoom))]
  [HarmonyPostfix]
  private static void OnJoinedRoomPatch()
  {
    foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
      PlayersTab.Add(player);
  }

  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnPlayerEnteredRoom), typeof(Player))]
  [HarmonyPostfix]
  private static void OnPlayerEnteredRoomPatch(Player newPlayer)
  {
    PlayersTab.Add(newPlayer);
  }

  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnPlayerLeftRoom))]
  [HarmonyPostfix]
  private static void OnPlayerLeftRoomPatch()
  {
    PlayersTab.Clear();

    foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
      PlayersTab.Add(player);
  }
}
