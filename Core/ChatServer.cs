using App.Contracts;
using Infrastructure.Persistence.Context;
using Infrastructure.Provider;
using Microsoft.EntityFrameworkCore;

namespace Core
{
    public class ChatServer : ChatBase
    {
        private HashSet<User> _users = new HashSet<User>();
        private readonly ChatContext _context;

        private readonly IMessageSource _source;

        public ChatServer(IMessageSource source, ChatContext context)
        {
            _source = source;
            _context = context;
        }

        public override async Task Start()
        {
            await Task.CompletedTask;
            await Task.Run(Listener);
        }

        protected override async Task Listener()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    ReceiveResult result = await _source.Receive(CancellationToken) ?? throw new Exception("Пустое сообщение...");
                    switch (result.Message!.Command)
                    {
                        case Command.None:
                            await MessageHandler(result);
                            break;
                        case Command.Join:
                            await JoinHandler(result);
                            break;
                        case Command.Exit:
                            await ExitHandler(result);
                            break;
                        case Command.Users:
                            break;
                        case Command.Confirm:
                            break;
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }

        private async Task ExitHandler(ReceiveResult result)
        {
            var user = User.FromDomain(await _context.Users.FirstAsync(u => u.Id == result.Message!.SenderId));
            user.LastOnline = DateTime.Now;
            await _context.SaveChangesAsync();
            _users.Remove(_users.First(u => u.Id == result.Message?.SenderId));
        }

        private async Task MessageHandler(ReceiveResult result)
        {
            if (result.Message!.RecipentId < 0)
            {
                await SendAllAsync(result.Message);
            }
            else
            {
                await _source.Send(
                    result.Message,
                    _users.First(u => u.Id == result.Message.SenderId).EndPoint!,
                    CancellationToken);

                var recipientEndPoint = _users.First(u => u.Id == result.Message.SenderId)?.EndPoint;

                if (recipientEndPoint is not null)
                {
                    await _source.Send(
                        result.Message,
                        recipientEndPoint,
                        CancellationToken);
                }
            }
        }

        private async Task JoinHandler(ReceiveResult result)
        {
            User? user = _users.FirstOrDefault(u => u.Name == result.Message!.Text);
            if (user is null)
            {
                user = new User { Name = result.Message!.Text };
                _users.Add(user);
            }
            user.EndPoint = result.EndPoint;
            await _source.Send(
                new Message { Command = Command.Join, RecipentId = user.Id },
                user.EndPoint,
                CancellationToken);
            await SendAllAsync(new Message()
            { Command = Command.Users, RecipentId = user.Id, Users = _users });

            var unreaded = await _context.Messages
                .Where(m => m.RecipientId == user.Id)
                .ToListAsync();
            foreach (var message in unreaded)
            {
                await _source.Send(
                    Message.FromDomain(message),
                    user.EndPoint,
                    CancellationToken);
            }

        }

        private async Task SendAllAsync(Message message)
        {
            foreach (User user in _users)
            {
                await _source.Send(message, user.EndPoint!, CancellationToken);
            }
        }
    }
}

