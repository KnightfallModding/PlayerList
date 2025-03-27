using HarmonyLib;

using Il2CppPhoton.Pun;

using PlayerList.GUI.Tabs;

namespace PlayerList.Patches;

[HarmonyPatch]
internal static class Il2CppPhotonNetworkPatch
{
  [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.Disconnect))]
  [HarmonyPostfix]
  private static void DisconnectPatch() => PlayersTab.Clear();
}
