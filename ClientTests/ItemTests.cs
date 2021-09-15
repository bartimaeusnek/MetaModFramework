using System.Threading.Tasks;
using MetaModClientCore.Clients;
using MetaModClientCore.DTOs;
using NUnit.Framework;

namespace ClientTests
{
    public class ItemTests
    {
        private AccountClient _accountClient;
        private ItemClient _client;
        
        [SetUp]
        public void SetUp()
        {
            _accountClient = new AccountClient("https://localhost:5001/");
            _client = new ItemClient("https://localhost:5001/", _accountClient.LoginAsync("test", "test", "testserverA").Result,
                                     "Minecraft_1.17.1");
        }
        
        [Test, Order(1)]
        public async Task PostItems() => Assert.IsTrue(await _client.PostItemsAsync(new ClientItem{Amount = 1, ItemDefinition = new ClientItemDefinition {Game = "Minecraft_1.17.1", UniqueIdentifier = "minecraft:stone"}}));
        
        [Test, Order(2)]
        public async Task GetAllAvailableItems() => Assert.IsNotEmpty(await _client.GetAllItemsForGameAsync());

        [Test, Order(3)]
        public async Task GetAllServerItemsForUser()
        {
            var defs = await _client.GetAllServerItemsForUserAsync();
            Assert.IsNotNull(defs);
        } 
        
        [Test, Order(4)]
        public async Task GetAllClientItemsForUser()
        {
            var defs = await _client.GetAllClientItemsForUserAsync();
            Assert.IsNotNull(defs);
        } 
        
        [Test, Order(5)]
        public async Task RequestItems() => Assert.IsTrue(await _client.RequestAsync(new ClientItem{Amount = 1, ItemDefinition = new ClientItemDefinition {Game = "Minecraft_1.17.1", UniqueIdentifier = "minecraft:stone"}}));
    }
}