using System.Threading;
using System.Threading.Tasks;
using MetaModFramework.WebSocketProtocol;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetaModFramework.Controllers
{
    [ApiController, Route("/v1/ws/")]
    public class WebSocketController : ControllerBase
    {
        private FlowControl flowControl;
        
        public WebSocketController(FlowControl flowControl)
        {
            this.flowControl = flowControl;
        }

        [HttpGet("/v1/ws")]
        public async Task Get()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            else
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await flowControl.HandleWebSocketConnection(webSocket, CancellationToken.None);
            }
        }
    }
}