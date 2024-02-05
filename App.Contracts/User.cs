using System.Net;
using System.Text.Json.Serialization;
using Domain;

namespace App.Contracts
{
    public record User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime LastOnline { get; set; }

        [JsonIgnore]
        public IPEndPoint? EndPoint;

        public static User FromDomain(UserEntity userEntity) => new User()
        {
            Id = userEntity.Id,
            Name = userEntity.Name,
            LastOnline = userEntity.LastOnline
        };
        public override string ToString()
        {
            return $"{Name}";
        }
    }
}

