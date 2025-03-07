using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PlayerList.Utils;

public class APIPlayer
{
  public string UUID { get; set; }
  public string[] prefixes { get; set; }
  public string username { get; set; }
  public string[] postfixes { get; set; }
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
