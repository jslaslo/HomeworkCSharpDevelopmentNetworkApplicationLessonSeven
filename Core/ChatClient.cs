using System.Net;
using App.Contracts;
using Infrastructure.Provider;

namespace Core
{
    public class ChatClient : ChatBase
    {

        private IEnumerable<User> _users = new List<User>();
        private readonly User _username;
        private readonly IMessageSource _source;
        private readonly IPEndPoint _serverEndPoint;

        public ChatClient(string username, IPEndPoint serverEndPoint, IMessageSource source)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            _username = new User { Name = username, LastOnline = DateTime.Now, EndPoint = iPEndPoint, Id = _users.Count() };
            _source = source;
            _serverEndPoint = serverEndPoint;

        }
        public override async Task Start()
        {
            var join = new Message { Text = _username.Name, Command = Command.Join };
            await _source.Send(join, _serverEndPoint, CancellationToken);
            Task.Run(Listener);

            while (!CancellationToken.IsCancellationRequested)
            {
                Console.Write("\nВведите сообщение: \n");
                string input = (await Console.In.ReadLineAsync()) ?? string.Empty;
                Message message;
                if (input.Trim().ToLower() == "/exit")
                {
                    
                    message = new() { SenderId = _username.Id, Command = Command.Exit };
                }
                else
                {
                    
                    message = new() { SenderId = _username.Id, Command = Command.None, Text = input };
                }

                await _source.Send(message, _serverEndPoint, CancellationToken);
            }
        }
        protected override async Task Listener()
        {

            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    ReceiveResult? result = await _source.Receive(CancellationToken) ?? throw new Exception("Пустое сообщение...");

                    if (result?.Message?.Command == Command.Join)
                    {
                        JoinHandler(result.Message);
                    }
                    else if (result?.Message?.Command == Command.Users)
                    {
                        UsersHandler(result.Message);
                    }
                    else if (result?.Message?.Command == Command.None)
                    {
                        MessageHandler(result.Message);
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }

            }
        }

        private void JoinHandler(Message message)
        {
            if(message.Users is null)
            {
                _username.Id = 0;
            }
            else
            {
                _username.Id = message.Users.Count();
            }
            Console.WriteLine("Присоеденение к чату выполнено!\n");
        }

        private void UsersHandler(Message? message)
        {
            _users = message.Users;
        }

        private void MessageHandler(Message message)
        {
            Console.WriteLine($"\n({message.SendMessage}) {_users.First(u => u.Id == message.SenderId)}: {message.Text} \n");
        }
    }
}

