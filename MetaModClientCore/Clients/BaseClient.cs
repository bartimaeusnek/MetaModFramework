using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web;
using MetaModClientCore.DTOs;

namespace MetaModClientCore.Clients
{
    public abstract class BaseClient : IDisposable
    {
        protected HttpClient            Client  { get; set; } = new HttpClient();
        protected string                urlBase { get; }

        protected JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        protected BaseClient(string baseUrl)
        {
            baseUrl      = baseUrl.Trim(' ', '/', '"');
            urlBase = $"{baseUrl}/{GetApiVersion(baseUrl)}/";
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected string GetApiVersion(string baseUrl)
        {
            var builder = new UriBuilder(baseUrl +"/ApiReference");
            var answer  = Client.GetStringAsync(builder.ToString()).Result;
            var version = JsonSerializer.Deserialize<ApiReference>(answer, SerializerOptions);
            return "v" + version?.Version;
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
        
        protected HttpRequestMessage BuildRequest(string path, HttpMethod method) 
            => BuildRequest(path, method, new (string, string)[] { });

        protected HttpRequestMessage BuildRequest(string path, HttpMethod method, (string, string)[] queries) 
            => BuildRequest(path, method, queries, null);

        protected HttpRequestMessage BuildRequest(string path, HttpMethod method, string content) 
            => BuildRequest(path, method, new (string, string)[] { }, content);

        protected virtual HttpRequestMessage BuildRequest(string path, HttpMethod method, (string, string)[] queries, string content)
        {
            var builder = new UriBuilder(urlBase + path);
            var query   = HttpUtility.ParseQueryString(builder.Query);
            foreach (var (key, value) in queries)
            {
                query[key] = value;
            }
            builder.Query = query.ToString();
            
            var request = new HttpRequestMessage
                          {
                              Method     = method,
                              RequestUri = new Uri(builder.ToString())
                          };
            
            if (content != null)
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            
            return request;
        }
    }
}