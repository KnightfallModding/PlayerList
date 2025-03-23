using System.Collections.Generic;

using Hexa.NET.ImGui;

using PlayerList.Utils;

namespace PlayerList.GUI.Tabs;

public struct Prefixes
{
  public const string Creator = "👑";
  public const string T1nquen = "🤡";
}

internal class PlayerDetails
{
  public string[] Prefixes { get; set; }
  public List<TextSegment> Username { get; set; }
  public string[] Suffixes { get; set; }
  public int LocalId { get; set; }

#nullable enable
  public string? UUID { get; set; }
#nullable disable
}

internal static class PlayersTab
{
  public static List<PlayerDetails> Players { get; private set; } = [];
  public static List<APIPlayer> CustomPlayers { get; set; }

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

      foreach (PlayerDetails player in Players)
      {
        string prefixes = (player.Prefixes.Length > 0) ? $"[{string.Join("][", player.Prefixes)}] " : "";
        string suffixes = (player.Suffixes.Length > 0) ? $" [{string.Join("][", player.Suffixes)}]" : "";

        ImGui.AlignTextToFramePadding();
        ImGui.Text(prefixes);
        ImGui.SameLine(0, 0);
        DisplayUsername(player.Username);
        ImGui.SameLine(0, 0);
        ImGui.Text(suffixes);
      }

      ImGui.EndTabItem();
    }
  }

  private static void DisplayUsername(List<TextSegment> usernameSegments)
  {
    foreach (TextSegment segment in usernameSegments)
    {
      ImFontPtr font = FontsManager.RegularFont;

      if (segment.Bold)
        font = FontsManager.BoldFont;

      if (segment.Bold && segment.Italic)
        font = FontsManager.BoldItalicFont;

      if (segment.Italic)
        font = FontsManager.ItalicFont;

      ImGui.PushFont(font);

      // TODO: Implement sprite to emoji

      ImGui.SameLine(0, 0);
      if (segment.Color != default)
      {
        ImGui.TextColored(segment.Color, segment.Text);
      }
      else
      {
        ImGui.TextUnformatted(segment.Text);
      }

      ImGui.PopFont();
    }
  }

  public static string[] GetPrefixes(string UUID)
  {
    string[] value = [];
    return CustomPlayers.Find(player => player.UUID == UUID)?.Prefixes ?? value;
  }
  public static string GetUsername(Photon.Realtime.Player player, string UUID) => CustomPlayers.Find(player => player.UUID == UUID)?.Username?.Replace("{nickname}", player.NickName) ?? player.NickName;
  public static string[] GetSuffixes(string UUID) => CustomPlayers.Find(player => player.UUID == UUID)?.Suffixes ?? System.Array.Empty<string>();

  public static void Add(Photon.Realtime.Player player)
  {
    var UUID = default(string);
    try
    {
      UUID = player.CustomProperties["UUID"].ToString().ToLower();
    }
    catch { }

    var markupParser = new XMLParser(GetUsername(player, UUID));
    var details = new PlayerDetails()
    {
      LocalId = player.ActorNumber,
      UUID = UUID,
      Prefixes = GetPrefixes(UUID),
      Username = markupParser.Parse(),
      Suffixes = GetSuffixes(UUID),
    };
    Players.Add(details);
  }

  public static void Clear() => Players = new List<PlayerDetails>();
}
