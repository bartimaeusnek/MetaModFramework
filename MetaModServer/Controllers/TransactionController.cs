using System.Threading.Tasks;
using MetaModFramework.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetaModFramework.Controllers
{
    [ApiController, Route("/v1/"), Authorize]
    public class TransactionController : ControllerBase
    {
        [HttpGet, Route("/v1/Transaction")]
        public async Task<IActionResult> GetLock()
        {
            return await Task.Run<IActionResult>(() =>
                                                 {
                                                     var userClaimsPrincipal = HttpContext.User;
                                                     if (userClaimsPrincipal.Identity == null)
                                                         return new StatusCodeResult(StatusCodes
                                                            .Status500InternalServerError);
                                                     var busy = ServiceTransactions
                                                        .CanNotRequestTransaction(userClaimsPrincipal
                                                            .Identity.Name);
                                                     return new StatusCodeResult( busy ? StatusCodes.Status423Locked : StatusCodes.Status200OK);
                                                 });
        }
        
        [HttpPost, Route("/v1/Transaction")]
        public async Task<IActionResult> PostLock()
        {
            return await Task.Run<IActionResult>(() =>
                                                 {
                                                     var userClaimsPrincipal = HttpContext.User;
                                                     if (userClaimsPrincipal.Identity == null)
                                                         return new StatusCodeResult(StatusCodes
                                                            .Status500InternalServerError);

                                                     ServiceTransactions
                                                                       .EndTransaction(userClaimsPrincipal
                                                                        .Identity.Name);
                                                     return Ok();
                                                 });
        }
    }
}