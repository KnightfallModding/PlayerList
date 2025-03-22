using HarmonyLib;

using Photon.Pun;

using PlayerList.GUI.Tabs;

namespace PlayerList.Patches;

internal static class PhotonNetworkPatch
{
  [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.Disconnect))]
  [HarmonyPostfix]
  private static void DisconnectPatch() => PlayersTab.Clear();
}
