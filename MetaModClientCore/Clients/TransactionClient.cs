using System.Net.Http;
using System.Threading.Tasks;

namespace MetaModClientCore.Clients
{
    public class TransactionClient : AuthorizedClient
    {
        public TransactionClient(string baseUrl, string token) : base(baseUrl, token) {}
        
        public async Task<bool> BeginTransaction()
        {
            var request = BuildRequest("Transaction", HttpMethod.Get);
            var answer  = await this.Client.SendAsync(request);
            return answer.IsSuccessStatusCode;
        }
        
        public async Task<bool> EndTransaction()
        {
            var request = BuildRequest("Transaction", HttpMethod.Post);
            var answer  = await this.Client.SendAsync(request);
            return answer.IsSuccessStatusCode;
        }
    }
}