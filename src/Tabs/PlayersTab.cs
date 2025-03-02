using System.Collections.Generic;
using Hexa.NET.ImGui;
using PlayerList.Utils;

namespace PlayerList.Tabs;

public struct Prefixes
{
  public const string Creator = "👑";
  public const string T1nquen = "🤡";
}

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
      if (!ConfigManager.DisplayUsernames.Value) return;

      foreach (var player in Players)
      {
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{string.Concat(player.Prefixes)} {player.Username} {string.Concat(player.Postfixes)}");
      }

      ImGui.EndTabItem();
    }
  }

  // TODO: Fetch prefixes, usernames and postfixes from an API
  public static string[] GetPrefixes(string UUID) => UUID switch
  {
    "aeryle" => [Prefixes.Creator],
    "t1nquen#goat" => [Prefixes.T1nquen],
    _ => [],
  };
  public static string GetUsername(Photon.Realtime.Player player, string UUID)
  {
    return UUID switch
    {
      "souptis" => $"<color=#D3F5F5>{player.NickName}</color>",
      _ => player.NickName,
    };
  }
  public static string[] GetPostfixes(string UUID) => UUID switch
  {
    _ => [],
  };

  public static void Add(Photon.Realtime.Player player)
  {
#nullable enable
    string? UUID = null;
#nullable disable
    try
    {
      UUID = player.CustomProperties["UUID"].ToString().ToLower();
    }
    catch { }

    PlayerDetails details = new()
    {
      LocalId = player.ActorNumber,
      UUID = UUID,
      Prefixes = UUID != null ? GetPrefixes(UUID) : [],
      Username = UUID != null ? GetUsername(player, UUID) : player.NickName,
      Postfixes = UUID != null ? GetPostfixes(UUID) : [],
    };
    Players.Add(details);
  }

  public static void Clear() => Players = [];
}
