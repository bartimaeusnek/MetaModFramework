using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Async;
using MetaModFramework.DTOs;
using MetaModFramework.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MetaModFramework.WebSocketProtocol
{
    public class FlowControl
    {
        private readonly ItemTranslationLayer _itemTranslationLayer;
        private readonly LiteDatabaseAsync    _liteDatabaseAsync;
        private readonly IConfiguration       _config;
        private          string               _name;
        
        private static readonly JsonSerializerOptions Options     = new (){ PropertyNameCaseInsensitive = true };
        private static readonly ReadOnlyMemory<byte>  NeedsResync = new(new byte[] { 1 });
        
        public FlowControl(ItemTranslationLayer itemTranslationLayer, LiteDatabaseAsync liteDatabaseAsync, IConfiguration config)
        {
            _itemTranslationLayer = itemTranslationLayer;
            _liteDatabaseAsync    = liteDatabaseAsync;
            _config               = config;
        }

        private string Authenticate(string token) {
            var           validator = new JwtSecurityTokenHandler();
            
            var validationParameters = new TokenValidationParameters
                                       {
                                           ValidateIssuer   = true,
                                           ValidateAudience = true,
                                           ValidAudiences = _config.GetSection("JWT:ValidAudiences").Get<List<string>>(),
                                           ValidIssuer = _config["JWT:ValidIssuer"],
                                           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]))
                                       };
            
            if (!validator.CanReadToken(token))
                return string.Empty;

            try
            {
                var principal = validator.ValidateToken(token, validationParameters, out _);
                if (principal.HasClaim(c => c.Type == ClaimTypes.Name))
                {
                    return principal.Claims.First(c => c.Type == ClaimTypes.Name).Value;
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }
        
        internal async Task HandleWebSocketConnection(WebSocket socket, CancellationToken cancellationToken)
        {
            var buffer        = new byte[1024 * _config.GetSection("Misc").GetValue<int>("WebSocketBufferKiloBytes")];
            var bufferSegment = new ArraySegment<byte>(buffer);

            var loginResult = await socket.ReceiveAsync(bufferSegment, cancellationToken);
            
            _name = Authenticate(Encoding.UTF8.GetString(bufferSegment.Array![..loginResult.Count]!));
            
            if (_name == string.Empty)
                await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "ProtocolError: Not Authenticated!", cancellationToken);
            
            while(true) 
            {
                var result = await socket.ReceiveAsync(bufferSegment, cancellationToken);
                if (result.Count == 0)
                    continue;
                if (await HandleCloseRequestAsync(socket, result, cancellationToken))
                    return;
                
                if (await CloseOnMethodErrorAsync(socket, result, cancellationToken))
                    return;
                
                var method = GetMethodes(bufferSegment[..result.Count]);
                if (WsEventHandler.NeedsSync && method != Methodes.RequestItems)
                {
                    await socket.SendAsync(NeedsResync,
                                           WebSocketMessageType.Binary,
                                           true,
                                           cancellationToken
                                          );
                    continue;
                }
                var param      = GetParameters(method, bufferSegment[..result.Count]);
                var invokation = InvokeClientMethod(method, param);
                var returnType = await GetReturnType(invokation);
                var payload    = Encoding.UTF8.GetBytes(returnType);
                await socket.SendAsync(payload,
                                       WebSocketMessageType.Text,
                                       true,
                                       cancellationToken);
            }
        }

#region ClientStuffs

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object? InvokeClientMethod(Methodes method,object[] param)
        {
            var methodInfo = method.GetMethodInfo();
            var ret        = methodInfo.Invoke(null, param);
            WsEventHandler.NeedsSync = method != Methodes.RequestItems;
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<string> GetReturnType(object? obj)
        {
            return obj switch
                   {
                       Task<int> task                     => "" + await task,
                       Task<IEnumerable<ClientItem>> task => JsonSerializer.Serialize(await task, Options),
                       _                                  => null
                   };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object[] GetParameters(Methodes methodes, ArraySegment<byte> bufferSegment)
        {
            var content = Encoding.UTF8.GetString(bufferSegment[8..]);
            return methodes switch
                   {
                       //string game, string name, ItemTranslationLayer itemTranslationLayer, LiteDatabaseAsync liteDatabaseAsync
                       Methodes.RequestItems 
                           => new object[]
                              {
                                  content,
                                  _name,
                                  _itemTranslationLayer,
                                  _liteDatabaseAsync
                              },
                       Methodes.OverwriteData =>
                           throw new NotImplementedException(),
                       Methodes.RequestAndDecrementItems
                           => new object[]
                              {
                                  JsonSerializer
                                     .Deserialize<ClientItem>(content, Options),
                                  _name,
                                  _itemTranslationLayer,
                                  _liteDatabaseAsync
                              },
                       Methodes.UpsertItems
                           => new object[]
                              {
                                  JsonSerializer.Deserialize<ClientItem[]>(content, Options),
                                  _name,
                                  _itemTranslationLayer,
                                  _liteDatabaseAsync
                              },
                       _ => throw new ArgumentOutOfRangeException(nameof(methodes), methodes, null)
                   };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Methodes GetMethodes(ArraySegment<byte> bufferSegment)
        {
            var  state  = MemoryMarshal.Cast<byte, long>(bufferSegment[..8].Reverse().ToArray());
            if (state.Length == 0)
                throw new ArgumentException("Parameter is empty!", nameof(bufferSegment));

            return (Methodes) state[0];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task<bool> CloseOnMethodErrorAsync(WebSocket         socket, WebSocketReceiveResult result,
                                                                CancellationToken cancellationToken)
        {
            if (result.MessageType != WebSocketMessageType.Text)
                return false;
            await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "ProtocolError: NoMethod Specified!", cancellationToken);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task<bool> HandleCloseRequestAsync(WebSocket socket, WebSocketReceiveResult result, CancellationToken cancellationToken)
        {
            if (result.MessageType != WebSocketMessageType.Close)
                return false;
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close Requested", cancellationToken);
            return true;
        }
#endregion

    }
}