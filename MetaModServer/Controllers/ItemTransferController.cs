using System.Linq;
using System.Threading.Tasks;
using LiteDB.Async;
using MetaModFramework.DTOs;
using MetaModFramework.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MetaModFramework.Controllers
{
    [ApiController, Route("/v1/"), Authorize]
    public class ItemTransferController : ControllerBase
    {
        private readonly LiteDatabaseAsync          _db;
        private readonly ILogger<AccountController> _logger;
        private readonly ItemTranslationLayer       _itemTranslationLayer;

        public ItemTransferController(LiteDatabaseAsync db, ILogger<AccountController> logger, ItemTranslationLayer itemTranslationLayer)
        {
            this._db                   = db;
            this._logger               = logger;
            this._itemTranslationLayer = itemTranslationLayer;
        }

        [HttpGet, Route("/v1/Items/All")]
        public async Task<IActionResult> GetAllAsync()
        {
            var userClaimsPrincipal = this.HttpContext.User;
            if (userClaimsPrincipal.Identity == null)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            if (ServiceTransactions.TransactionNotInUse(userClaimsPrincipal.Identity.Name))
                return new StatusCodeResult(StatusCodes.Status423Locked);
            return this.Ok(await this._db.GetCollection<ServerItem>(userClaimsPrincipal.Identity.Name + "_"+ nameof(ServerItemDefinition)).Query().ToArrayAsync());
        }

        /**
         * NEVER Directly Expose this to the Internet!
         */
        [HttpGet, Route("/v1/Items/Web/{user}/{game?}"), AllowAnonymous]
        public async Task<IActionResult> WebEndpoint(string user, string game = null)
        {
            var serverItems = await this._db
                                        .GetCollection<ServerItem>(user + "_" +
                                                                   nameof(ServerItemDefinition)).Query().ToArrayAsync();

            return game != null ? this.Ok(await this._itemTranslationLayer.GetClientNamesAsync(game, serverItems)) : this.Ok(serverItems);
        }

        [HttpGet, Route("/v1/Items/{game}")]
        public async Task<IActionResult> GetAsync([FromRoute]string game)
        {
            var userClaimsPrincipal = this.HttpContext.User;
            if (userClaimsPrincipal.Identity == null)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            
            if (ServiceTransactions.TransactionNotInUse(userClaimsPrincipal.Identity.Name))
                return new StatusCodeResult(StatusCodes.Status423Locked);
            
            var serverItems = await this._db
                                        .GetCollection<ServerItem>(userClaimsPrincipal.Identity.Name + "_" +
                                                                   nameof(ServerItemDefinition)).Query().ToArrayAsync();
            
            var content = await this._itemTranslationLayer.GetClientNamesAsync(game, serverItems);
            
            return this.Ok(content);
        }
        
        [HttpPut, Route("/v1/Items")]
        public async Task<IActionResult> RequestAsync([FromBody] ClientItem item)
        {
            var userClaimsPrincipal = this.HttpContext.User;
            if (userClaimsPrincipal.Identity == null)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            if (ServiceTransactions.TransactionNotInUse(userClaimsPrincipal.Identity.Name))
                return new StatusCodeResult(StatusCodes.Status423Locked);
            var serverItemDefinitions = (await this._itemTranslationLayer.GetServerNamesAsync(item)).FirstOrDefault();
            if (serverItemDefinitions == default)
                return new StatusCodeResult(StatusCodes.Status400BadRequest);

            var col = this._db.GetCollection<ServerItem>(userClaimsPrincipal.Identity.Name + "_" +
                                                         nameof(ServerItemDefinition));
            
            var stuff = await col
                             .Query().Where(x => x.ItemDefinition == serverItemDefinitions.ItemDefinition).FirstOrDefaultAsync();
            
            if (stuff == default)
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);

            if (stuff.Amount < item.Amount)
                return new StatusCodeResult(StatusCodes.Status507InsufficientStorage);

            stuff.Amount -= item.Amount;

            if (!await col.UpdateAsync(stuff)) 
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            
            await col.DeleteManyAsync(x => x.Amount == 0);
            return this.Ok();
        }
        
        [HttpPost, Route("/v1/Items")]
        public async Task<IActionResult> PostAsync([FromBody] params ClientItem[] items)
        {
            var userClaimsPrincipal = this.HttpContext.User;
            if (userClaimsPrincipal.Identity == null)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            if (ServiceTransactions.TransactionNotInUse(userClaimsPrincipal.Identity.Name))
                return new StatusCodeResult(StatusCodes.Status423Locked);
            var serverItemDefinitions = await this._itemTranslationLayer.GetServerNamesAsync(items);
            
            var col    = this._db.GetCollection<ServerItem>(userClaimsPrincipal.Identity.Name + "_" + nameof(ServerItemDefinition));
            
            foreach (var def in serverItemDefinitions)
            {
                if (await col.ExistsAsync(x => x.ItemDefinition == def.ItemDefinition))
                    def.Amount += await col.Query().Where(x => x.ItemDefinition == def.ItemDefinition).Select(x => x.Amount).SingleOrDefaultAsync();
            }
            
            return this.Ok(await col.UpsertAsync(serverItemDefinitions));
        }

        [HttpGet, Route("/v1/Items/{game}/Available"), AllowAnonymous]
        public async Task<IActionResult> GetDefinitions([FromRoute] string game) 
            => this.Ok(await this._itemTranslationLayer.GetAllDefinitions(game));
    }
}