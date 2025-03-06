using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.Json;
using System.Net;

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
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                System.Console.WriteLine("Salesforce Authentication Error: " + responseBody);
                throw new HttpRequestException($"Authentication error: {response.StatusCode} - {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            var accessToken = root.GetProperty("access_token").GetString();
            var instanceUrl = root.GetProperty("instance_url").GetString();

            return (accessToken, instanceUrl);
        }

        
        public async Task<string> CreateAccountAsync(string instanceUrl, string accessToken, Dictionary<string, object> accountData)
        {
            var apiVersion = "v63.0";
            var requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/Account/";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = JsonContent.Create(accountData)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                var accountId = root.GetProperty("id").GetString();
                return accountId;
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Console.WriteLine("CreateAccountAsync error: " + errorBody);
                return null;
            }
        }

        
        public async Task<string> CreateContactAsync(string instanceUrl, string accessToken, Dictionary<string, object> contactData)
        {
            var apiVersion = "v63.0";
            var requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/Contact/";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = JsonContent.Create(contactData)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                var contactId = root.GetProperty("id").GetString();
                return contactId;
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Console.WriteLine("CreateContactAsync error: " + errorBody);
                return null;
            }
        }

        
        public async Task<bool> UpdateAccountAsync(string instanceUrl, string accessToken, string accountId, Dictionary<string, object> accountData)
        {
            var apiVersion = "v63.0";
            var requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/Account/{accountId}";

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUrl)
            {
                Content = JsonContent.Create(accountData)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        
        public async Task<bool> UpdateContactAsync(string instanceUrl, string accessToken, string contactId, Dictionary<string, object> contactData)
        {
            var apiVersion = "v63.0";
            var requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/Contact/{contactId}";

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUrl)
            {
                Content = JsonContent.Create(contactData)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }



        
        public async Task<Dictionary<string, object>> GetContactByAccountIdAsync(string instanceUrl, string accessToken, string accountId)
        {
            var apiVersion = "v63.0";
            var query = $"SELECT Id, FirstName, LastName, Email, Phone, Title FROM Contact WHERE AccountId = '{accountId}' LIMIT 1";
            var requestUrl = $"{instanceUrl}/services/data/{apiVersion}/query/?q={WebUtility.UrlEncode(query)}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                if (root.TryGetProperty("records", out JsonElement records) && records.GetArrayLength() > 0)
                {
                    var record = records[0];
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in record.EnumerateObject())
                    {
                        // If the value is of type String, use GetString(), otherwise ToString()
                        if (prop.Value.ValueKind == JsonValueKind.String)
                        {
                            dict[prop.Name] = prop.Value.GetString();
                        }
                        else
                        {
                            dict[prop.Name] = prop.Value.ToString();
                        }
                    }
                    return dict;
                }
            }
            return null;
        }

        
        public async Task<Dictionary<string, object>> GetAccountDataAsync(string instanceUrl, string accessToken, string accountId)
        {
            var apiVersion = "v63.0";
            var requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/Account/{accountId}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            return new Dictionary<string, object>();
        }



        public async Task<bool> DeleteAccountAsync(string instanceUrl, string accessToken, string accountId)
        {
            var apiVersion = "v63.0";
            var requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/Account/{accountId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteContactAsync(string instanceUrl, string accessToken, string contactId)
        {
            var apiVersion = "v63.0";
            var requestUrl = $"{instanceUrl}/services/data/{apiVersion}/sobjects/Contact/{contactId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }


    }
}
