using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MetaModClientCore.Clients
{
    public class AccountClient : BaseClient
    {
        public AccountClient(string baseUrl) : base(baseUrl) {}

        public async Task<bool> RegisterAsync(string name, string email, string password)
        {
            var hashedPassword = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password)));
            var request        = BuildRequest("Register", HttpMethod.Post, new[] { (nameof(name), name), (nameof(email), email), (nameof(password), hashedPassword) });
            var answer         = await Client.SendAsync(request);
            return answer.StatusCode == HttpStatusCode.Accepted;
        }
        
        public async Task<string> LoginAsync(string userName, string password, string audience)
        {
            var hashedPassword = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password)));
            var request        = BuildRequest("Login", HttpMethod.Post, new[] { (nameof(password), hashedPassword), (nameof(userName), userName), (nameof(audience), audience) });
            return await (await Client.SendAsync(request)).Content.ReadAsStringAsync();
        }
    }
}