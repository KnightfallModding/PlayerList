using System.Linq;
using HarmonyLib;
using Il2Cpp;
using PlayerList.GUI.Tabs;

namespace PlayerList.Patches;

[HarmonyPatch(typeof(PL_Damagable))]
internal static class DamageableEventPatch
{
  [HarmonyPatch(typeof(PL_Damagable), nameof(PL_Damagable.RPCA_Die))]
  [HarmonyPostfix]
  private static void RPCA_Die(PL_Damagable __instance)
  {
    var killedPlayer = __instance.damagable.player;
    PlayerHandler.TryGetTeam(killedPlayer.TeamID, out var team);
    var teamPlayers = team.Players.ToArray();
    var teamWiped = teamPlayers.All(teammate => teammate.data.isGhost);

    if (teamWiped)
    {
      foreach (var player in teamPlayers)
      {
        var details =
          PlayersTab.Players.Find(playerDetails => playerDetails.LocalId == player.refs.view.OwnerActorNr);

        details.Suffixes = details.Suffixes.Where(prefix => prefix != "💀").Append("🏳️").ToArray();
      }
    }
    else
    {
      var details =
        PlayersTab.Players.Find(playerDetails => playerDetails.LocalId == killedPlayer.refs.view.OwnerActorNr);
      details.Suffixes = details.Suffixes.Append("💀").ToArray();
    }
  }

  private static void AddDeathEmoji() { }
}
