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

        public static void SetApiKey(String apiKey)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("apikey:" + apiKey)));
        }

        public static async Task<Dictionary<string, JsonElement>> GetDevicesAsync(int orgId)
        {
            var responseString = await client.GetStringAsync(api_base + "links/cellular?tunnelable=1&limit=1000&orgid=" + orgId);
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseString);
        }

        public static async Task<Dictionary<string, JsonElement>> GetOrganizationsAsync(int userId)
        {
            var responseString = await client.GetStringAsync(api_base + "organizations?userid=" + userId);
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseString);
        }

        public static async Task<Dictionary<string, JsonElement>> GetUserInfoAsync()
        {
            var responseString = await client.GetStringAsync(api_base + "users/me");
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseString);
        }

        public static async Task<Dictionary<string, JsonElement>> HasTunnelKey(int userId)
        {
            var responseString = await client.GetStringAsync(api_base + "tunnelkeys?userid=" + userId);
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseString);
        }

        public static async void CreateTunnelKey()
        {
            var response = await client.PostAsync(api_base + "tunnelkeys", null);
            if (response.IsSuccessStatusCode) {
                var jsonresponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(await response.Content.ReadAsStringAsync());
                SSH.CreateRSAKey(
                    Encoding.ASCII.GetBytes(jsonresponse["data"].GetProperty("public_key").GetString()),
                    Encoding.ASCII.GetBytes(jsonresponse["data"].GetProperty("private_key").GetString()));
            }
        }

        public static async Task<bool> UploadTunnelKey()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram/spacebridge.key.pub");
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {
                    "public_key", File.ReadAllText(path)
                }
                
            });
            var response = await client.PostAsync(api_base + "tunnelkeys", content);
            return response.IsSuccessStatusCode;
        }
    }
}
