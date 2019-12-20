using Netcode;
using System.IO;

namespace StardewValley.Monsters
{
	internal class ParryEventArgs : NetEventArg
	{
		public int damage;

		private long farmerId;

		public Farmer who
		{
			get
			{
				return Game1.getFarmer(farmerId);
			}
			set
			{
				farmerId = value.UniqueMultiplayerID;
			}
		}

		public ParryEventArgs()
		{
		}

		public ParryEventArgs(int damage, Farmer who)
		{
			this.damage = damage;
			this.who = who;
		}

		public void Read(BinaryReader reader)
		{
			damage = reader.ReadInt32();
			farmerId = reader.ReadInt64();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(damage);
			writer.Write(farmerId);
		}
	}
}
