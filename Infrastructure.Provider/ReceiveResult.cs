using System.Net;
using App.Contracts;

namespace Infrastructure.Provider
{
    public record ReceiveResult(IPEndPoint EndPoint, Message? Message);
}

