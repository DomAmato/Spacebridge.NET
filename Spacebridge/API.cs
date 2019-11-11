using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace Spacebridge
{
    static class API
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly String api_base = "https://dashboard.hologram.io/api/1/";

        public static void setApiKey(String apiKey)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("apikey:" + apiKey)));
        }

        public static async Task<Dictionary<string, JsonElement>> getDevicesAsync(int orgId)
        {
            var responseString = await client.GetStringAsync(api_base + "links/cellular?tunnelable=1&limit=1000&orgid=" + orgId);
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseString);
        }

        public static async Task<Dictionary<string, JsonElement>> getOrganizationsAsync(int userId)
        {
            var responseString = await client.GetStringAsync(api_base + "organizations?userid=" + userId);
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseString);
        }

        public static async Task<Dictionary<string, JsonElement>> getUserInfoAsync()
        {
            var responseString = await client.GetStringAsync(api_base + "users/me");
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseString);
        }

        public static async Task<HttpResponseMessage> postTunnelKey(byte[] publicKey)
        {
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("apikey:"), 0, publicKey, 0, 7);
            var content = new ByteArrayContent(publicKey);
            return await client.PostAsync(api_base + "tunnelkeys", content);
        }
    }
}
