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

internal static class API
{
  public static async Task<List<APIPlayer>> FetchCustomPlayers()
  {
    var client = new HttpClient();
    const string url = $"https://knightfallbr.com/api/mods/{MyPluginInfo.PLUGIN_GUID}/prefixes";

    return await client.GetFromJsonAsync<List<APIPlayer>>(url);
  }
}
