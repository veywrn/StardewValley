using Netcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Network
{
	public class NetMutexQueue<T> : INetObject<NetFields>
	{
		private readonly NetLongDictionary<bool, NetBool> requests = new NetLongDictionary<bool, NetBool>();

		private readonly NetLong currentOwner = new NetLong();

		private readonly List<T> localJobs = new List<T>();

		[XmlIgnore]
		public Action<T> Processor = delegate
		{
		};

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public NetMutexQueue()
		{
			NetFields.AddFields(requests, currentOwner);
			requests.InterpolationWait = false;
			currentOwner.InterpolationWait = false;
		}

		public void Add(T job)
		{
			localJobs.Add(job);
		}

		public bool Contains(T job)
		{
			return localJobs.Contains(job);
		}

		public void Clear()
		{
			localJobs.Clear();
		}

		public void Update(GameLocation location)
		{
			FarmerCollection farmers = location.farmers;
			if (farmers.Contains(Game1.player) && localJobs.Count > 0)
			{
				requests[Game1.player.UniqueMultiplayerID] = true;
			}
			else
			{
				requests.Remove(Game1.player.UniqueMultiplayerID);
			}
			if (Game1.IsMasterGame)
			{
				requests.Filter((KeyValuePair<long, bool> kv) => farmers.FirstOrDefault((Farmer f) => f.UniqueMultiplayerID == kv.Key) != null);
				if (!requests.ContainsKey(currentOwner))
				{
					currentOwner.Value = -1L;
				}
			}
			if ((long)currentOwner == Game1.player.UniqueMultiplayerID)
			{
				foreach (T job in localJobs)
				{
					Processor(job);
				}
				localJobs.Clear();
				requests.Remove(Game1.player.UniqueMultiplayerID);
				currentOwner.Value = -1L;
			}
			if (Game1.IsMasterGame && (long)currentOwner == -1 && requests.Count() > 0)
			{
				currentOwner.Value = requests.Keys.ElementAt(Game1.random.Next(requests.Count()));
			}
		}
	}
}
