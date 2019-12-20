using Microsoft.Xna.Framework.Audio;
using Netcode;
using System.Collections.Generic;

namespace StardewValley.Network
{
	public class NetAudioCueManager
	{
		private Dictionary<string, ICue> playingCues = new Dictionary<string, ICue>();

		private List<string> cuesToStop = new List<string>();

		public virtual void Update(GameLocation currentLocation)
		{
			NetDictionary<string, bool, NetBool, SerializableDictionary<string, bool>, NetStringDictionary<bool, NetBool>>.KeysCollection activeCues = currentLocation.netAudio.ActiveCues;
			foreach (string cue3 in activeCues)
			{
				if (!playingCues.ContainsKey(cue3))
				{
					playingCues[cue3] = Game1.soundBank.GetCue(cue3);
					playingCues[cue3].Play();
				}
			}
			foreach (KeyValuePair<string, ICue> playingCue in playingCues)
			{
				string cue2 = playingCue.Key;
				if (!activeCues.Contains(cue2))
				{
					cuesToStop.Add(cue2);
				}
			}
			foreach (string cue in cuesToStop)
			{
				playingCues[cue].Stop(AudioStopOptions.AsAuthored);
				playingCues.Remove(cue);
			}
			cuesToStop.Clear();
		}

		public void StopAll()
		{
			foreach (ICue value in playingCues.Values)
			{
				value.Stop(AudioStopOptions.Immediate);
			}
			playingCues.Clear();
		}
	}
}
