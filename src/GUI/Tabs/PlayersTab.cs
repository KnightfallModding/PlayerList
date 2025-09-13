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
        string prefixes = (player.Prefixes.Length > 0) ? string.Concat(player.Prefixes) : "";
        string suffixes = (player.Suffixes.Length > 0) ? string.Concat(player.Suffixes) : "";

        if (prefixes.Length > 0)
        {
          ImGui.AlignTextToFramePadding();
          ImGui.Text(prefixes);
          VerticalSeparator();
        }

        ImGui.AlignTextToFramePadding();
        DisplayUsername(player.Username);

        if (suffixes.Length > 0)
        {
          ImGui.AlignTextToFramePadding();
          VerticalSeparator();
          ImGui.Text(suffixes);
        }
      }

      ImGui.EndTabItem();
    }
  }

  private static void DisplayUsername(List<TextSegment> usernameSegments)
  {
    for (int i = 0; i < usernameSegments.Count; i++)
    // foreach (TextSegment segment in usernameSegments)
    {
      TextSegment segment = usernameSegments[i];
      ImFontPtr font = FontsManager.RegularFont;

      if (segment.Bold)
        font = FontsManager.BoldFont;

      if (segment.Bold && segment.Italic)
        font = FontsManager.BoldItalicFont;

      if (segment.Italic)
        font = FontsManager.ItalicFont;

      ImGui.PushFont(font);

      // TODO: Implement sprite to emoji

      if (i > 0)
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

  private static void VerticalSeparator()
  {
    ImGui.SameLine();
    ImGui.Spacing();
    ImGui.SameLine();
    ImGui.AlignTextToFramePadding();
    ImGuiP.SeparatorEx(ImGuiSeparatorFlags.Vertical, 2f);
    ImGui.SameLine();
    ImGui.Spacing();
    ImGui.SameLine();
  }

  public static string[] GetPrefixes(string UUID)
  {
    string[] value = [];
    return CustomPlayers.Find(player => player.UUID == UUID)?.Prefixes ?? value;
  }
  public static string GetUsername(Il2CppPhoton.Realtime.Player player, string UUID) => CustomPlayers.Find(player => player.UUID == UUID)?.Username?.Replace("{nickname}", player.NickName) ?? player.NickName;
  public static string[] GetSuffixes(string UUID) => CustomPlayers.Find(player => player.UUID == UUID)?.Suffixes ?? System.Array.Empty<string>();

  public static void Add(Il2CppPhoton.Realtime.Player player)
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
