using System.Net.Http;
using System.Net.Http.Headers;

namespace MetaModClientCore.Clients
{
    public class AuthorizedClient : BaseClient
    {
        public AuthorizedClient(string baseUrl, string token) : base(baseUrl)
        {
            Token = token;
        }

        protected string Token { get; }
        
        protected override HttpRequestMessage BuildRequest(string path, HttpMethod method, (string, string)[] queries, string content)
        {
            var request = base.BuildRequest(path, method, queries, content);
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer " + Token);
            return request;
        }
    }
}