using System.Threading.Tasks;
using LiteDB.Async;
using MetaModFramework.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MetaModFramework.Controllers;

[ApiController, Route("/[controller]"), AllowAnonymous]
public class ApiReferenceController : ControllerBase
{
    private readonly ILogger<ApiReferenceController> _logger;
    private readonly LiteDatabaseAsync               _db;

    public ApiReferenceController(ILogger<ApiReferenceController> logger, LiteDatabaseAsync db)
    {
        _db     = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> OnGet() 
        => Ok(await _db.GetCollection<ApiReference>().Query().FirstAsync());
}