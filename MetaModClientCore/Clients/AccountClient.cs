using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MetaModClientCore.Clients
{
    public class AccountClient : BaseClient
    {
        public AccountClient(string baseUrl) : base(baseUrl) {}

        public async Task<bool> RegisterAsync(string name, string email, string password)
        {
            var request = BuildRequest("Register", HttpMethod.Post, new[] { (nameof(name), name), (nameof(email), email), (nameof(password), password) });
            
            var answer = await Client.SendAsync(request);
            return answer.StatusCode == HttpStatusCode.Accepted;
        }
        
        public async Task<string> LoginAsync(string userName, string password, string audience)
        {
            var request = BuildRequest("Login", HttpMethod.Post, new[] { (nameof(password), password), (nameof(userName), userName), (nameof(audience), audience) });
            return await (await Client.SendAsync(request)).Content.ReadAsStringAsync();
        }
    }
}