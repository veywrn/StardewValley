using Netcode;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class OrderReward : INetObject<NetFields>
	{
		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public OrderReward()
		{
			InitializeNetFields();
		}

		public virtual void InitializeNetFields()
		{
		}

		public virtual void Grant()
		{
		}

		public virtual void Load(SpecialOrder order, Dictionary<string, string> data)
		{
		}
	}
}
