using Netcode;
using System.Xml;

namespace StardewValley.Characters
{
	public class BotchedNetLong : BotchedNetField<long, NetLong>
	{
		public BotchedNetLong()
		{
			netField = new NetLong();
		}

		public BotchedNetLong(long default_value)
		{
			netField = new NetLong(default_value);
		}

		protected override object _ParseValue(XmlReader reader)
		{
			return long.Parse(reader.Value);
		}
	}
}
