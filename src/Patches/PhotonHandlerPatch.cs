using System;

using HarmonyLib;

using Il2CppPhoton.Pun;

using PlayerList.GUI.Tabs;

namespace PlayerList.Patches;

[HarmonyPatch]
internal static class Il2CppPhotonHandlerPatch
{
  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnJoinedRoom))]
  [HarmonyPostfix]
  private static void OnJoinedRoomPatch()
  {
    foreach (Il2CppPhoton.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
      PlayersTab.Add(player);
  }

  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnPlayerEnteredRoom), new Type[] { typeof(Il2CppPhoton.Realtime.Player) })]
  [HarmonyPostfix]
  private static void OnPlayerEnteredRoomPatch(Il2CppPhoton.Realtime.Player newPlayer) => PlayersTab.Add(newPlayer);

  [HarmonyPatch(typeof(PhotonHandler), nameof(PhotonHandler.OnPlayerLeftRoom))]
  [HarmonyPostfix]
  private static void OnPlayerLeftRoomPatch()
  {
    PlayersTab.Clear();

    foreach (Il2CppPhoton.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
      PlayersTab.Add(player);
  }
}
