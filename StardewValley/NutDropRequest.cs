using Microsoft.Xna.Framework;
using Netcode;
using System.IO;

namespace StardewValley
{
	public class NutDropRequest : NetEventArg
	{
		public string key;

		public string locationName;

		public Point position;

		public int limit = 1;

		public int rewardAmount = 1;

		public NutDropRequest()
		{
		}

		public NutDropRequest(string key, string location_name, Point position, int limit, int reward_amount)
		{
			this.key = key;
			if (location_name == null)
			{
				locationName = "null";
			}
			else
			{
				locationName = location_name;
			}
			this.position = position;
			this.limit = limit;
			rewardAmount = reward_amount;
		}

		public void Read(BinaryReader reader)
		{
			key = reader.ReadString();
			locationName = reader.ReadString();
			position.X = reader.ReadInt32();
			position.Y = reader.ReadInt32();
			limit = reader.ReadInt32();
			rewardAmount = reader.ReadInt32();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(key);
			writer.Write(locationName);
			writer.Write(position.X);
			writer.Write(position.Y);
			writer.Write(limit);
			writer.Write(rewardAmount);
		}
	}
}
