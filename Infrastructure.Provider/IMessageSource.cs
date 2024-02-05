using System.Net;
using App.Contracts;

namespace Infrastructure.Provider
{
    public interface IMessageSource
	{
		Task<ReceiveResult> Receive(CancellationToken token);

		Task Send(Message message, IPEndPoint endPoint, CancellationToken token);
	}
}

