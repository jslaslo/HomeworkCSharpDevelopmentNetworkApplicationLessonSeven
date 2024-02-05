using Domain;

namespace App.Contracts
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int RecipentId { get; set; } = -1;

        public string Text { get; set; } = string.Empty;

        public DateTime SendMessage { get; set; } = DateTime.Now;

        public Command Command { get; set; } = Command.None;

        public IEnumerable<User>? Users { get; set; }

        public static Message FromDomain(MessageEntity entity)
        {
            return new Message
            {
                Id = entity.Id,
                SenderId = entity.SenderId,
                RecipentId = entity.RecipientId,
                SendMessage = entity.SendMessage,
            };
        }
    }
}

