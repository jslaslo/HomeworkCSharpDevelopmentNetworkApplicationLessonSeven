using System.Text;
using System.Text.Json;

namespace App.Contracts.Extentions
{
	public static class MessageExtentions
	{
		public static Message? ToMessage(this byte[] data)
		{
			return JsonSerializer.Deserialize<Message>(Encoding.UTF8.GetString(data));
		}
		public static byte[] ToBytes(this Message message)
		{
			return Encoding.UTF8.GetBytes(JsonSerializer.Serialize<Message>(message));
		}		
	}
}

