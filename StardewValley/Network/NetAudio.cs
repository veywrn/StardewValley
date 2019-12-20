using Microsoft.Xna.Framework;
using Netcode;
using System.IO;

namespace StardewValley.Network
{
	public class NetAudio : INetObject<NetFields>
	{
		public enum SoundContext
		{
			Default,
			NPC
		}

		private readonly NetEventBinary audioEvent = new NetEventBinary();

		private readonly NetStringDictionary<bool, NetBool> activeCues = new NetStringDictionary<bool, NetBool>();

		private GameLocation location;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public NetDictionary<string, bool, NetBool, SerializableDictionary<string, bool>, NetStringDictionary<bool, NetBool>>.KeysCollection ActiveCues => activeCues.Keys;

		public NetAudio(GameLocation location)
		{
			this.location = location;
			NetFields.AddFields(audioEvent, activeCues);
			audioEvent.AddReaderHandler(handleAudioEvent);
		}

		private void handleAudioEvent(BinaryReader reader)
		{
			string audioName = reader.ReadString();
			Vector2 position = reader.ReadVector2();
			int pitch = reader.ReadInt32();
			SoundContext context = (SoundContext)reader.ReadInt32();
			PlayLocalAt(audioName, position, pitch, context);
		}

		public void PlayLocalAt(string audioName, Vector2 position, int pitch = -1, SoundContext sound_context = SoundContext.Default)
		{
			if (position == Vector2.Zero || Utility.isOnScreen(position * 64f, 384))
			{
				PlayLocal(audioName, pitch, sound_context);
			}
		}

		public void PlayLocal(string audioName, int pitch = -1, SoundContext sound_context = SoundContext.Default)
		{
			if ((!Game1.eventUp || sound_context != SoundContext.NPC) && Game1.currentLocation == location)
			{
				if (pitch == -1)
				{
					Game1.playSound(audioName);
				}
				else
				{
					Game1.playSoundPitched(audioName, pitch);
				}
			}
		}

		public void Update()
		{
			audioEvent.Poll();
		}

		public void Play(string audioName, SoundContext soundContext = SoundContext.Default)
		{
			audioEvent.Fire(delegate(BinaryWriter writer)
			{
				writer.Write(audioName);
				writer.WriteVector2(Vector2.Zero);
				writer.Write(-1);
				writer.Write((int)soundContext);
			});
			audioEvent.Poll();
		}

		public void PlayAt(string audioName, Vector2 position, SoundContext soundContext = SoundContext.Default)
		{
			audioEvent.Fire(delegate(BinaryWriter writer)
			{
				writer.Write(audioName);
				writer.WriteVector2(position);
				writer.Write(-1);
				writer.Write((int)soundContext);
			});
			audioEvent.Poll();
		}

		public void PlayPitched(string audioName, Vector2 position, int pitch, SoundContext soundContext = SoundContext.Default)
		{
			audioEvent.Fire(delegate(BinaryWriter writer)
			{
				writer.Write(audioName);
				writer.WriteVector2(position);
				writer.Write(pitch);
				writer.Write((int)soundContext);
			});
			audioEvent.Poll();
		}

		public void StartPlaying(string cueName)
		{
			activeCues[cueName] = false;
		}

		public void StopPlaying(string cueName)
		{
			activeCues.Remove(cueName);
		}
	}
}
