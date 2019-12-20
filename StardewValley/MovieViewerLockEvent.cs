using Netcode;
using System.Collections.Generic;
using System.IO;

namespace StardewValley
{
	public class MovieViewerLockEvent : NetEventArg
	{
		public List<long> uids;

		public int movieStartTime;

		public MovieViewerLockEvent()
		{
			uids = new List<long>();
			movieStartTime = 0;
		}

		public MovieViewerLockEvent(List<Farmer> present_farmers, int movie_start_time)
		{
			movieStartTime = movie_start_time;
			uids = new List<long>();
			foreach (Farmer farmer in present_farmers)
			{
				uids.Add(farmer.UniqueMultiplayerID);
			}
		}

		public void Read(BinaryReader reader)
		{
			uids.Clear();
			movieStartTime = reader.ReadInt32();
			int capacity = reader.ReadInt32();
			for (int i = 0; i < capacity; i++)
			{
				uids.Add(reader.ReadInt64());
			}
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(movieStartTime);
			writer.Write(uids.Count);
			for (int i = 0; i < uids.Count; i++)
			{
				writer.Write(uids[i]);
			}
		}
	}
}
