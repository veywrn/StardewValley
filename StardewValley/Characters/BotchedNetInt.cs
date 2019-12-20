using Netcode;
using System.Xml;

namespace StardewValley.Characters
{
	public class BotchedNetInt : BotchedNetField<int, NetInt>
	{
		public BotchedNetInt()
		{
			netField = new NetInt();
		}

		public BotchedNetInt(int default_value)
		{
			netField = new NetInt(default_value);
		}

		protected override object _ParseValue(XmlReader reader)
		{
			return int.Parse(reader.Value);
		}
	}
}
