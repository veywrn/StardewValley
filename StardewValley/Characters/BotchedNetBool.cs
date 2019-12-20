using Netcode;
using System.Xml;

namespace StardewValley.Characters
{
	public class BotchedNetBool : BotchedNetField<bool, NetBool>
	{
		public BotchedNetBool()
		{
			netField = new NetBool();
		}

		public BotchedNetBool(bool default_value)
		{
			netField = new NetBool(default_value);
		}

		protected override object _ParseValue(XmlReader reader)
		{
			return bool.Parse(reader.Value);
		}
	}
}
