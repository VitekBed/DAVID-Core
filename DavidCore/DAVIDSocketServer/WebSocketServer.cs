using System.Runtime.CompilerServices;
using DAVID.CodeAnnotations;
using DAVID.Interface;

namespace DAVID.SocketServer;

[Singleton()]
public class WebSocketServer : IWebSocketServer
{
    private static WebSocketServer? _socketServer;
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    [FactoryMethod(typeof(IWebSocketServer))]
    public static WebSocketServer CurrentSocketServer() => _socketServer is not null ? _socketServer : _socketServer ?? new WebSocketServer();
}
