using System.Net;
using System.Net.Sockets;
using Core;
using Infrastructure.Persistence.Context;
using Infrastructure.Provider;

namespace App;

class Program
{
    static async Task Main(string[] args)
    {
        var serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12000);
        UdpClient udpClient;
        IMessageSource source;
        Console.WriteLine("Введите 0 если сервер:");
        string command = Console.ReadLine();

        if(command == "0")
        {
            //server
            udpClient = new UdpClient(serverEndPoint);
            source = new MessageSource(udpClient);
            ChatServer chat = new ChatServer(source, new ChatContext());
            await chat.Start();
        }
        else
        {
            //client
            source = new MessageSource(new UdpClient());
            var chat = new ChatClient(command, serverEndPoint, source);
            chat.Start();
        }

    }
}

