
using System.Net.Sockets;
namespace SocketClientTest
{
    public interface IDoWork
    {
        void Handle(ClsSockteClient Client);
    }
}
