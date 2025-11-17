using System.Linq;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using PlayerList.GUI.Tabs;

namespace PlayerList.Patches;

[HarmonyPatch(typeof(Pl_Revive))]
internal static class Pl_RevivePatch
{
  [HarmonyPatch(nameof(Pl_Revive.RPCA_Revive))]
  [HarmonyPostfix]
  private static void RPCA_Revive(Pl_Revive __instance)
  {
    var player = PlayersTab.Players.Find(p => p.LocalId == __instance.player.refs.view.Owner.ActorNumber);
    Melon<PlayerListMod>.Logger.Msg($"Player found in revive: {player}");
    if (player is null) return;

    player.Suffixes = player.Suffixes.Where(suffix => suffix is not ("💀" or "🏳️")).ToArray();
  }
}
