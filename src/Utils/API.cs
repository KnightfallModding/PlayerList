using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PlayerList.Utils;

public class APIPlayer(string UUID, string[] prefixes, string username, string[] postfixes)
{
  public string UUID { get; } = UUID;
  public string[] prefixes { get; } = prefixes;
  public string username { get; } = username;
  public string[] postfixes { get; } = postfixes;
}

public static class API
{
  public static async Task<List<APIPlayer>> FetchCustomPlayers()
  {
    var client = new HttpClient();
    const string url = $"https://knightfallbr.com/api/mods/{MyPluginInfo.PLUGIN_GUID}/prefixes";

    return await client.GetFromJsonAsync<List<APIPlayer>>(url);
  }
}
