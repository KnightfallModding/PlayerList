using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PlayerList.Utils;

internal class APIPlayer(string UUID, string[] prefixes, string username, string[] suffixes)
{
  public string UUID { get; } = UUID;
  public string[] Prefixes { get; } = prefixes;
  public string Username { get; } = username;
  public string[] Suffixes { get; } = suffixes;
}

#nullable enable
internal class CustomAPIPlayer(string UUID, string? prefix, string? username, string? suffix)
{
  public string UUID { get; } = UUID;
#pragma warning disable IDE1006
  public string? prefix { get; } = prefix;
  public string? username { get; } = username;
  public string? suffix { get; } = suffix;
#pragma warning restore IDE1006
}
#nullable disable

internal static class API
{
  private static readonly HttpClient Client = new();

  private static readonly string BaseURL = "https://knightfallbr.com/api/mods/" + PlayerListModInfo.MOD_GUID.Replace(".admin", "");

  public static async Task<List<APIPlayer>> FetchCustomPlayers()
  {
    string url = BaseURL + "/players";

    return await Client.GetFromJsonAsync<List<APIPlayer>>(url);
  }

  public static async Task<HttpResponseMessage> AddCustomPlayer(CustomAPIPlayer player)
  {
    string url = BaseURL + "/players";

    return await Client.PutAsJsonAsync(url, player);
  }
}
