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
            if (!this.HttpContext.WebSockets.IsWebSocketRequest)
            {
                this.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            else
            {
                using var webSocket = await this.HttpContext.WebSockets.AcceptWebSocketAsync();
                await this.flowControl.HandleWebSocketConnection(webSocket, CancellationToken.None);
            }
        }
    }
}