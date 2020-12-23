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
			if (CanHear(position))
			{
				PlayLocal(audioName, pitch, sound_context);
			}
		}

		public void PlayLocal(string audioName, int pitch = -1, SoundContext sound_context = SoundContext.Default)
		{
			if ((!Game1.eventUp || sound_context != SoundContext.NPC) && Game1.currentLocation == location)
			{
				_PlayAudio(audioName, pitch);
			}
		}

		protected void _PlayAudio(string audioName, int pitch)
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

		public void Update()
		{
			audioEvent.Poll();
		}

		public bool CanHear(Vector2 position)
		{
			if (!(position == Vector2.Zero))
			{
				return Utility.isOnScreen(position * 64f, 384);
			}
			return true;
		}

		public bool CanShortcutPlay(Vector2 position, SoundContext sound_context)
		{
			if (!LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
			{
				return false;
			}
			if (Game1.eventUp && sound_context == SoundContext.NPC)
			{
				return false;
			}
			if ((location == null || location == Game1.currentLocation) && CanHear(position))
			{
				return true;
			}
			bool someone_can_hear = false;
			if (location != null)
			{
				foreach (Game1 gameInstance in GameRunner.instance.gameInstances)
				{
					if (gameInstance.instanceGameLocation == location)
					{
						someone_can_hear = true;
						break;
					}
				}
				if (someone_can_hear && position != Vector2.Zero)
				{
					someone_can_hear = false;
					GameRunner.instance.ExecuteForInstances(delegate
					{
						if (!someone_can_hear && location == Game1.currentLocation && CanHear(position))
						{
							someone_can_hear = true;
						}
					});
				}
				return someone_can_hear;
			}
			return true;
		}

		public void Play(string audioName, SoundContext soundContext = SoundContext.Default)
		{
			if (CanShortcutPlay(Vector2.Zero, soundContext))
			{
				_PlayAudio(audioName, -1);
				return;
			}
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
			if (CanShortcutPlay(position, soundContext))
			{
				_PlayAudio(audioName, -1);
				return;
			}
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
			if (CanShortcutPlay(Vector2.Zero, soundContext))
			{
				_PlayAudio(audioName, pitch);
				return;
			}
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
