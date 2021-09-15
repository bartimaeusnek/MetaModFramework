using System;
using System.Threading.Tasks;
using MetaModClientCore.Clients;
using NUnit.Framework;

namespace ClientTests
{
    public class AccountTests
    {
        private AccountClient _client;

        [SetUp]
        public void SetUp()
        {
            _client = new AccountClient("https://localhost:5001/");
        }
        
        [Test, Order(1)]
        public async Task TestRegistration() => Assert.IsTrue(await _client.RegisterAsync("test", "test@test.test", "test"));
        
        [Test, Order(2)]
        public async Task TestLogin() => Console.WriteLine(await _client.LoginAsync("test", "test", "testserverA"));
    }
}