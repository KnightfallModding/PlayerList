using HarmonyLib;
using Photon.Pun;
using PlayerList.Tabs;

namespace PlayerList.Patches;

public static class PhotonNetworkPatch
{
  [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.Disconnect))]
  [HarmonyPostfix]
  static void DisconnectPatch() => PlayersTab.Clear();
}
