using System.Text.Json;

namespace CartService.Tests.ApiTests.Helpers
{
    public class AuthenticationHelper
    {
        public static async Task<string> GetAccessTokenAsync()
        {
            var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/realms/MicroservicesRealm/protocol/openid-connect/token");

            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", "client" },
                { "username", "username" },
                { "password", "pass" },
                { "client_secret", "123" } 
            };

            request.Content = new FormUrlEncodedContent(parameters);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            var token = doc.RootElement.GetProperty("access_token").GetString();

            return token!;
        }

    }
}
