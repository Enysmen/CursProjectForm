using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.Json;

namespace CursProject.Service
{
    public class SalesforceService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SalesforceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // Метод для аутентификации в Salesforce, возвращает access token и instance URL
        public async Task<(string AccessToken, string InstanceUrl)> AuthenticateAsync()
        {
            var values = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", _configuration["Salesforce:ClientId"] },
            { "client_secret", _configuration["Salesforce:ClientSecret"] },
            { "username", _configuration["Salesforce:Username"] },
            { "password", _configuration["Salesforce:Password"] + _configuration["Salesforce:SecurityToken"] }
        };

            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync(_configuration["Salesforce:TokenRequestUrl"], content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var accessToken = root.GetProperty("access_token").GetString();
            var instanceUrl = root.GetProperty("instance_url").GetString();

            return (accessToken, instanceUrl);
        }

        // Метод для создания Account в Salesforce
        public async Task<bool> CreateAccountAsync(string instanceUrl, string accessToken, Dictionary<string, object> accountData)
        {
            // Укажите актуальную версию API, например, v56.0
            var apiVersion = "v56.0";
            var requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/Account/";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = JsonContent.Create(accountData)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
