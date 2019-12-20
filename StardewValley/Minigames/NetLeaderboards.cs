using Netcode;
using System.Collections.Generic;

namespace StardewValley.Minigames
{
	public class NetLeaderboards : INetObject<NetFields>
	{
		public NetObjectList<NetLeaderboardsEntry> entries = new NetObjectList<NetLeaderboardsEntry>();

		public NetInt maxEntries = new NetInt(10);

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public void InitNetFields()
		{
			NetFields.AddFields(entries, maxEntries);
		}

		public NetLeaderboards()
		{
			InitNetFields();
		}

		public void AddScore(string name, int score)
		{
			List<NetLeaderboardsEntry> temp_entries = new List<NetLeaderboardsEntry>(entries);
			temp_entries.Add(new NetLeaderboardsEntry(name, score));
			temp_entries.Sort((NetLeaderboardsEntry a, NetLeaderboardsEntry b) => a.score.Value.CompareTo(b.score.Value));
			temp_entries.Reverse();
			while (temp_entries.Count > maxEntries.Value)
			{
				temp_entries.RemoveAt(temp_entries.Count - 1);
			}
			entries.Set(temp_entries);
		}

		public List<KeyValuePair<string, int>> GetScores()
		{
			List<KeyValuePair<string, int>> scores = new List<KeyValuePair<string, int>>();
			foreach (NetLeaderboardsEntry entry in entries)
			{
				scores.Add(new KeyValuePair<string, int>(entry.name.Value, entry.score.Value));
			}
			scores.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => a.Value.CompareTo(b.Value));
			scores.Reverse();
			return scores;
		}

		public void LoadScores(List<KeyValuePair<string, int>> scores)
		{
			entries.Clear();
			foreach (KeyValuePair<string, int> score in scores)
			{
				AddScore(score.Key, score.Value);
			}
		}
	}
}
