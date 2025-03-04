using HarmonyLib;
using Photon.Pun;
using PlayerList.GUI.Tabs;

namespace PlayerList.Patches;

public static class PhotonNetworkPatch
{
  [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.Disconnect))]
  [HarmonyPostfix]
  static void DisconnectPatch() => PlayersTab.Clear();
}
