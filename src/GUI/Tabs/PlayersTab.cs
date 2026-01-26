using System;
using System.Collections.Generic;
using System.Threading;
using Hexa.NET.ImGui;
using Il2CppPhoton.Realtime;
using PlayerList.Utils;

namespace PlayerList.GUI.Tabs;

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
  private static readonly ReaderWriterLockSlim Locker = new();

  public static List<PlayerDetails> Players { get; internal set; } = [];
  public static List<APIPlayer> CustomPlayers { get; set; }

  public static void Render()
  {
    if (!ImGui.BeginTabItem($"Players - {Players.Count}###players"))
      return;

    Locker.EnterReadLock();

    try
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
        var prefixes = player.Prefixes.Length > 0 ? string.Concat(player.Prefixes) : "";
        var suffixes = player.Suffixes.Length > 0 ? string.Concat(player.Suffixes) : "";

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
    finally
    {
      Locker.ExitReadLock();
    }
  }

  private static void DisplayUsername(List<TextSegment> usernameSegments)
  {
    for (var i = 0; i < usernameSegments.Count; i++)
    // foreach (TextSegment segment in usernameSegments)
    {
      var segment = usernameSegments[i];
      var font = FontsManager.RegularFont;

      if (segment.Bold)
        font = FontsManager.BoldFont;

      if (segment.Bold && segment.Italic)
        font = FontsManager.BoldItalicFont;

      if (segment.Italic)
        font = FontsManager.ItalicFont;

      ImGui.PushFont(font, ConfigManager.FontSize.Value);

      // TODO: Implement sprite to emoji

      if (i > 0)
        ImGui.SameLine(0, 0);

      if (segment.Color != default)
        ImGui.TextColored(segment.Color, segment.Text);
      else
        ImGui.TextUnformatted(segment.Text);

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

  public static string GetUsername(Player player, string UUID) =>
    CustomPlayers
      .Find(player => player.UUID == UUID)
      ?.Username?.Replace("{nickname}", player.NickName)
    ?? player.NickName;

  public static string[] GetSuffixes(string UUID) =>
    CustomPlayers.Find(player => player.UUID == UUID)?.Suffixes ?? Array.Empty<string>();

  public static void Add(Player player)
  {
    Locker.EnterWriteLock();

    var UUID = default(string);
    try
    {
      UUID = player.CustomProperties["UUID"].ToString().ToLower();

      var markupParser = new XMLParser(GetUsername(player, UUID));
      var details = new PlayerDetails
      {
        LocalId = player.ActorNumber,
        UUID = UUID,
        Prefixes = GetPrefixes(UUID),
        Username = markupParser.Parse(),
        Suffixes = GetSuffixes(UUID),
      };
      Players.Add(details);
    }
    finally
    {
      Locker.ExitWriteLock();
    }
  }

  public static void Clear()
  {
    Locker.EnterWriteLock();

    try
    {
      Players.Clear();
    }
    finally
    {
      Locker.ExitWriteLock();
    }
  }
}
