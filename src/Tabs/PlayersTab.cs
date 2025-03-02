using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Hexa.NET.ImGui;
using PlayerList.ConfigManager;

namespace PlayerList.Tabs;

public class PlayerDetails
{
  public string[] Prefixes { get; set; }
  public string Username { get; set; }
  public string[] Postfixes { get; set; }
  public int LocalId { get; set; }

#nullable enable
  public string? UUID { get; set; }
#nullable disable
}

public static class PlayersTab
{
  public static List<PlayerDetails> Players { get; private set; } = [];

  public static void Render()
  {
    if (ImGui.BeginTabItem($"Players - {Players.Count}###players"))
    {
      if (Players.Count == 0)
      {
        ImGui.Text("You aren't in a game");
        ImGui.EndTabItem();

        return;
      }
      if (!ConfigManager.ConfigManager.DisplayUsernames.Value) return;

      foreach (var player in Players)
      {
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{string.Concat(player.Prefixes)} {player.Username} {string.Concat(player.Postfixes)}");
      }

      ImGui.EndTabItem();
    }
  }

  public static void GetPrefixes(Photon.Realtime.Player player) { }

  public static void Add(Photon.Realtime.Player player)
  {
    var UUID = player.CustomProperties["UUID"];

    PlayerDetails details = new()
    {
      LocalId = player.ActorNumber,
      UUID = player.CustomProperties.ContainsKey("UUID") ? UUID.ToString() : null,
      Username = "",
      Prefixes = [],
      Postfixes = [],
    };
  }

  public static void Clear() => Players = [];
}
