using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB.Async;
using MetaModFramework.DTOs;
using MetaModFramework.Services;
using MetaModFramework.WebSocketProtocol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MetaModFramework.Controllers;

[ApiController, Route("/v1/"), Authorize]
public class ItemTransferController : ControllerBase
{
    private readonly LiteDatabaseAsync    _db;
    private readonly ItemTranslationLayer _itemTranslationLayer;

    public ItemTransferController(LiteDatabaseAsync db, ItemTranslationLayer itemTranslationLayer)
    {
        _db                   = db;
        _itemTranslationLayer = itemTranslationLayer;
    }

    [HttpGet, Route("/v1/Items/All"), Obsolete("Use WebSocket Connection instead!")]
    public async Task<IActionResult> GetAllAsync()
    {
        var userClaimsPrincipal = HttpContext.User;
        if (userClaimsPrincipal.Identity == null)
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        return Ok(await _db.GetCollection<ServerItem>(userClaimsPrincipal.Identity.Name + "_" + nameof(ServerItemDefinition)).Query().ToArrayAsync());
    }

    /**
         * NEVER Directly Expose this to the Internet!
         */
    [HttpGet, Route("/v1/Items/Web/{user}/{game?}"), AllowAnonymous]
    public async Task<IActionResult> WebEndpoint(string user, string game = null)
    {
        var serverItems = await _db
                               .GetCollection<ServerItem>(user + "_" +
                                                          nameof(ServerItemDefinition)).Query().ToArrayAsync();

        return game != null ? Ok(await _itemTranslationLayer.GetClientNamesAsync(game, serverItems)) : Ok(serverItems);
    }

    [HttpGet, Route("/v1/Items/{game}"), Obsolete("Use WebSocket Connection instead!")]
    public async Task<IActionResult> GetAsync([FromRoute] string game)
    {
        var userClaimsPrincipal = HttpContext.User;
        if (userClaimsPrincipal.Identity == null)
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            
        return Ok(await GetAsyncInternal(game, userClaimsPrincipal.Identity.Name, _itemTranslationLayer, _db));
    }

    [NonAction]
    internal static async Task<IEnumerable<ClientItem>> GetAsyncInternal(string game, string name, ItemTranslationLayer itemTranslationLayer, LiteDatabaseAsync liteDatabaseAsync)
    {
        var serverItems = await liteDatabaseAsync
                               .GetCollection<ServerItem>(name + "_" +
                                                          nameof(ServerItemDefinition)).Query().ToArrayAsync();
            
        var content = await itemTranslationLayer.GetClientNamesAsync(game, serverItems);
        return content;
    }

    [HttpPut, Route("/v1/Items"), Obsolete("Use WebSocket Connection instead!")]
    public async Task<IActionResult> RequestAsync([FromBody] ClientItem item)
    {
        var userClaimsPrincipal = HttpContext.User;
        if (userClaimsPrincipal.Identity == null)
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        var name = userClaimsPrincipal.Identity.Name;
        WsEventHandler.OnHandler();
        return new StatusCodeResult(
                                    await PutRequestItems(
                                                          item,
                                                          name,
                                                          _itemTranslationLayer,
                                                          _db
                                                         )
                                   );
    }

    [NonAction]
    internal static async Task<int> PutRequestItems(ClientItem item, string name, ItemTranslationLayer itemTranslationLayer, LiteDatabaseAsync liteDatabaseAsync)
    {
        var serverItemDefinitions = (await itemTranslationLayer.GetServerNamesAsync(item)).FirstOrDefault();
        if (serverItemDefinitions == default)
            return StatusCodes.Status400BadRequest;

        var col = liteDatabaseAsync.GetCollection<ServerItem>(name + "_" +
                                                              nameof(ServerItemDefinition));
            
        var stuff = await col
                         .Query().Where(x => x.ItemDefinition == serverItemDefinitions.ItemDefinition).FirstOrDefaultAsync();
            
        if (stuff == default)
            return StatusCodes.Status406NotAcceptable;

        if (stuff.Amount < item.Amount)
            return StatusCodes.Status507InsufficientStorage;

        stuff.Amount -= item.Amount;

        if (!await col.UpdateAsync(stuff)) 
            return StatusCodes.Status500InternalServerError;
            
        await col.DeleteManyAsync(x => x.Amount == 0);
        return StatusCodes.Status200OK;
    }

    [HttpPost, Route("/v1/Items"), Obsolete("Use WebSocket Connection instead!")]
    public async Task<IActionResult> PostAsync([FromBody] params ClientItem[] items)
    {
        var userClaimsPrincipal = HttpContext.User;
        if (userClaimsPrincipal.Identity == null)
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        WsEventHandler.OnHandler();
            
        var ret = await PostUpsertItems(items, userClaimsPrincipal.Identity.Name, _itemTranslationLayer,
                                        _db);

        return Ok(ret);
    }

    [NonAction]
    internal static async Task<int> PostUpsertItems(ClientItem[] items, string userName, ItemTranslationLayer itemTranslationLayer, LiteDatabaseAsync liteDatabaseAsync)
    {
        var serverItemDefinitions = await itemTranslationLayer.GetServerNamesAsync(items);

        var col = liteDatabaseAsync.GetCollection<ServerItem>(userName + "_" +
                                                              nameof(ServerItemDefinition));

        foreach (var def in serverItemDefinitions)
        {
            if (await col.ExistsAsync(x => x.ItemDefinition == def.ItemDefinition))
                def.Amount += await col.Query().Where(x => x.ItemDefinition == def.ItemDefinition).Select(x => x.Amount)
                                       .SingleOrDefaultAsync();
        }
            
        var ret = await col.UpsertAsync(serverItemDefinitions);
        return ret;
    }

    [HttpGet, Route("/v1/Items/{game}/Available"), AllowAnonymous]
    public async Task<IActionResult> GetDefinitions([FromRoute] string game) 
        => Ok(await _itemTranslationLayer.GetAllDefinitions(game));
}