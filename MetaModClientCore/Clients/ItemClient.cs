using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MetaModClientCore.DTOs;

namespace MetaModClientCore.Clients
{
    public class ItemClient : AuthorizedClient
    {
        public ItemClient(string baseUrl, string token, string game) : base(baseUrl, token)
        {
            this._game  = game;
        }
        
        private readonly string _game;

        /**
         * send as little requests as possible!
         * try to avoid using this inside of a loop!
         */
        public async Task<bool> PostItemsAsync(params ClientItem[] items)
        {
            var request = BuildRequest("Items", HttpMethod.Post, JsonSerializer.Serialize(items));
            var answer  = await this.Client.SendAsync(request);
            return answer.IsSuccessStatusCode;
        }
        
        /**
         * send as little requests as possible!
         * try to avoid using this inside of a loop!
         */
        public async Task<bool> RequestAsync(ClientItem items)
        {
            var body    = JsonSerializer.Serialize(items);
            var request = BuildRequest("Items", HttpMethod.Put, body);
            var answer  = await this.Client.SendAsync(request);
            return answer.IsSuccessStatusCode;
        }
        
        public async Task<List<string>> GetAllItemsForGameAsync()
        {
            var builder = new UriBuilder(this.urlBase + $"Items/{_game}/Available");
            return await JsonSerializer.DeserializeAsync<List<string>>(await Client.GetStreamAsync(builder.ToString()),SerializerOptions);
        }
        
        public async Task<List<ServerItem>> GetAllServerItemsForUserAsync()
        {
            var request = BuildRequest("Items/All", HttpMethod.Get);
            return await JsonSerializer.DeserializeAsync<List<ServerItem>>(await (await Client.SendAsync(request)).Content.ReadAsStreamAsync(), SerializerOptions);
        }
        
        public async Task<List<ClientItem>> GetAllClientItemsForUserAsync()
        {
            var request = BuildRequest($"Items/{_game}", HttpMethod.Get);
            return await JsonSerializer.DeserializeAsync<List<ClientItem>>(await (await Client.SendAsync(request)).Content.ReadAsStreamAsync(), SerializerOptions);
        }
    }
}