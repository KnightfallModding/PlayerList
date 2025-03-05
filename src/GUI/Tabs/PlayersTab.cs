using Hexa.NET.ImGui;
using PlayerList.Utils;
using PlayerList.Utils.RichTextParser;
using System.Collections.Generic;
using System.Numerics;

namespace PlayerList.GUI.Tabs;

public struct Prefixes
{
  public const string Creator = "👑";
  public const string T1nquen = "🤡";
}

public class PlayerDetails
{
  public string[] Prefixes { get; set; }
  public List<TextSegment> Username { get; set; }
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
      if (!ConfigManager.DisplayUsernames.Value)
      {
        ImGui.EndTabItem();

        return;
      }

      foreach (var player in Players)
      {
        var prefixes = player.Prefixes.Length > 0 ? $"[{string.Join("][", player.Prefixes)}]" : "";
        var postfixes = player.Prefixes.Length > 0 ? $"[{string.Join("][", player.Postfixes)}]" : "";

        ImGui.AlignTextToFramePadding();
        ImGui.Text(prefixes);
        ImGui.SameLine();
        DisplayUsername(player.Username);
        ImGui.Text(postfixes);
        ImGui.SameLine();
      }

      ImGui.EndTabItem();
    }
  }

  private static void DisplayUsername(List<TextSegment> usernameSegments)
  {
    foreach (var segment in usernameSegments)
    {
      var font = FontsManager.RegularFont;

      if (segment.Bold) font = FontsManager.BoldFont;
      if (segment.Bold && segment.Italic) font = FontsManager.BoldItalicFont;
      if (segment.Italic) font = FontsManager.ItalicFont;

      ImGui.PushFont(font);

      // TODO: Implement sprite to emoji

      if (segment.Color != default) ImGui.TextColored((Vector4)segment.Color, segment.Content);
      else ImGui.TextUnformatted(segment.Content);

      ImGui.PopFont();
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

    MarkupParser markupParser = new(GetUsername(player, UUID));

    PlayerDetails details = new()
    {
      LocalId = player.ActorNumber,
      UUID = UUID,
      Prefixes = GetPrefixes(UUID),
      Username = markupParser.Parse(),
      Postfixes = GetPostfixes(UUID),
    };
    Players.Add(details);
  }

  public static void Clear() => Players = [];
}
