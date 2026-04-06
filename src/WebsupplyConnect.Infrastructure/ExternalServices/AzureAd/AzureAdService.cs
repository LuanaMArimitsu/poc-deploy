using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.Infrastructure.ExternalServices.AzureAd
{
    public class AzureAdService : IAzureAdService
    {
        private readonly AzureAdOptions _options;
        private readonly HttpClient _httpClient;

        public AzureAdService(IOptions<AzureAdOptions> options)
        {
            _options = options.Value;
            _httpClient = new HttpClient();
        }

        private async Task<string> GetAcessTokenAsync()
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _options.ClientId),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await _httpClient.PostAsync($"https://login.microsoftonline.com/{_options.TenantId}/oauth2/v2.0/token", content);
            var responseString = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(responseString);
            return json.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<List<AzureUserDTO>> GetUserAsync(string? startsWith = null)
        {
            var token = await GetAcessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userList = new List<AzureUserDTO>();

            var query = string.IsNullOrWhiteSpace(startsWith) ? "" : $"&$filter=startsWith(displayName,'{startsWith}')";

            var url = $"https://graph.microsoft.com/v1.0/users?$select=id,displayName,mail,jobTitle,department,userPrincipalName{query}";

            while (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    using var response = await _httpClient.GetAsync(url);

                    var result = await response.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(result);

                    var root = doc.RootElement;

                    if (root.TryGetProperty("value", out var users))
                    {
                        foreach (var user in users.EnumerateArray())
                        {
                            var upn = user.TryGetProperty("userPrincipalName", out var upnProp) ? upnProp.GetString() : null;

                            userList.Add(new AzureUserDTO
                            {
                                Id = user.GetProperty("id").GetString(),
                                DisplayName = user.GetProperty("displayName").GetString(),
                                Email = user.TryGetProperty("mail", out var mail) ? mail.GetString() : upn,
                                Upn = upn,
                                Cargo = user.TryGetProperty("jobTitle", out var cargo) ? cargo.GetString() : null,
                                Departamento = user.TryGetProperty("department", out var dept) ? dept.GetString() : null
                            });
                        }
                    }

                    url = root.TryGetProperty("@odata.nextLink", out var nextLink) ? nextLink.GetString() : null;
                }
                catch (HttpRequestException ex)
                {
                    throw new ApplicationException("Erro de conexão ao acessar o Microsoft Graph.", ex);
                }
                catch (JsonException ex)
                {
                    throw new ApplicationException("Erro ao processar resposta JSON do Microsoft Graph.", ex);
                }
            }

            return userList;
        }

        public async Task<AzureUserDTO?> GetUserByIdAsync(string azureUserId)
        {
            var token = await GetAcessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"https://graph.microsoft.com/v1.0/users/{azureUserId}?$select=id,displayName,mail,jobTitle,department,userPrincipalName";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var user = doc.RootElement;

            var upn = user.TryGetProperty("userPrincipalName", out var upnProp) ? upnProp.GetString() : null;

            return new AzureUserDTO
            {
                Id = user.GetProperty("id").GetString(),
                DisplayName = user.GetProperty("displayName").GetString(),
                Email = user.TryGetProperty("mail", out var mail) ? mail.GetString() : upn,
                Upn = upn,
                Cargo = user.TryGetProperty("jobTitle", out var cargo) ? cargo.GetString() : null,
                Departamento = user.TryGetProperty("department", out var dept) ? dept.GetString() : null
            };
        }

        public async Task<AzureUserDTO?> GetUserByAccessTokenAsync(string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = "https://graph.microsoft.com/v1.0/me?$select=id,displayName,mail,jobTitle,department,userPrincipalName";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var user = doc.RootElement;

            var upn = user.TryGetProperty("userPrincipalName", out var upnProp) ? upnProp.GetString() : null;

            return new AzureUserDTO
            {
                Id = user.GetProperty("id").GetString(),
                DisplayName = user.GetProperty("displayName").GetString(),
                Email = user.TryGetProperty("mail", out var mail) ? mail.GetString() : upn,
                Upn = upn,
                Cargo = user.TryGetProperty("jobTitle", out var cargo) ? cargo.GetString() : null,
                Departamento = user.TryGetProperty("department", out var dept) ? dept.GetString() : null
            };
        }
    }
}
