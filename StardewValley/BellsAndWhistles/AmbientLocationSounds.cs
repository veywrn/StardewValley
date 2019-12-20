using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class AmbientLocationSounds
	{
		public const int sound_babblingBrook = 0;

		public const int sound_cracklingFire = 1;

		public const int sound_engine = 2;

		public const int sound_cricket = 3;

		public const int numberOfSounds = 4;

		public const float doNotPlay = 9999999f;

		private static Dictionary<Vector2, int> sounds = new Dictionary<Vector2, int>();

		private static int updateTimer = 100;

		private static int farthestSoundDistance = 1024;

		private static float[] shortestDistanceForCue;

		private static ICue babblingBrook;

		private static ICue cracklingFire;

		private static ICue engine;

		private static ICue cricket;

		private static float volumeOverrideForLocChange;

		public static void InitShared()
		{
			if (Game1.soundBank != null)
			{
				if (babblingBrook == null)
				{
					babblingBrook = Game1.soundBank.GetCue("babblingBrook");
					babblingBrook.Play();
					babblingBrook.Pause();
				}
				if (cracklingFire == null)
				{
					cracklingFire = Game1.soundBank.GetCue("cracklingFire");
					cracklingFire.Play();
					cracklingFire.Pause();
				}
				if (engine == null)
				{
					engine = Game1.soundBank.GetCue("heavyEngine");
					engine.Play();
					engine.Pause();
				}
				if (cricket == null)
				{
					cricket = Game1.soundBank.GetCue("cricketsAmbient");
					cricket.Play();
					cricket.Pause();
				}
			}
			shortestDistanceForCue = new float[4];
		}

		public static void update(GameTime time)
		{
			if (sounds.Count == 0)
			{
				return;
			}
			if (volumeOverrideForLocChange < 1f)
			{
				volumeOverrideForLocChange += (float)time.ElapsedGameTime.Milliseconds * 0.0003f;
			}
			updateTimer -= time.ElapsedGameTime.Milliseconds;
			if (updateTimer > 0)
			{
				return;
			}
			for (int j = 0; j < shortestDistanceForCue.Length; j++)
			{
				shortestDistanceForCue[j] = 9999999f;
			}
			Vector2 farmerPosition = Game1.player.getStandingPosition();
			foreach (KeyValuePair<Vector2, int> pair in sounds)
			{
				float distance = Vector2.Distance(pair.Key, farmerPosition);
				if (shortestDistanceForCue[pair.Value] > distance)
				{
					shortestDistanceForCue[pair.Value] = distance;
				}
			}
			if (volumeOverrideForLocChange >= 0f)
			{
				for (int i = 0; i < shortestDistanceForCue.Length; i++)
				{
					if (shortestDistanceForCue[i] <= (float)farthestSoundDistance)
					{
						float volume = Math.Min(volumeOverrideForLocChange, Math.Min(1f, 1f - shortestDistanceForCue[i] / (float)farthestSoundDistance));
						switch (i)
						{
						case 0:
							if (babblingBrook != null)
							{
								babblingBrook.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
								babblingBrook.Resume();
							}
							break;
						case 1:
							if (cracklingFire != null)
							{
								cracklingFire.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
								cracklingFire.Resume();
							}
							break;
						case 2:
							if (engine != null)
							{
								engine.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
								engine.Resume();
							}
							break;
						case 3:
							if (cricket != null)
							{
								cricket.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
								cricket.Resume();
							}
							break;
						}
						continue;
					}
					switch (i)
					{
					case 0:
						if (babblingBrook != null)
						{
							babblingBrook.Pause();
						}
						break;
					case 1:
						if (cracklingFire != null)
						{
							cracklingFire.Pause();
						}
						break;
					case 2:
						if (engine != null)
						{
							engine.Pause();
						}
						break;
					case 3:
						if (cricket != null)
						{
							cricket.Pause();
						}
						break;
					}
				}
			}
			updateTimer = 100;
		}

		public static void changeSpecificVariable(string variableName, float value, int whichSound)
		{
			if (whichSound == 2 && engine != null)
			{
				engine.SetVariable(variableName, value);
			}
		}

		public static void addSound(Vector2 tileLocation, int whichSound)
		{
			if (!sounds.ContainsKey(tileLocation * 64f))
			{
				sounds.Add(tileLocation * 64f, whichSound);
			}
		}

		public static void removeSound(Vector2 tileLocation)
		{
			if (!sounds.ContainsKey(tileLocation * 64f))
			{
				return;
			}
			switch (sounds[tileLocation * 64f])
			{
			case 0:
				if (babblingBrook != null)
				{
					babblingBrook.Pause();
				}
				break;
			case 1:
				if (cracklingFire != null)
				{
					cracklingFire.Pause();
				}
				break;
			case 2:
				if (engine != null)
				{
					engine.Pause();
				}
				break;
			case 3:
				if (cricket != null)
				{
					cricket.Pause();
				}
				break;
			}
			sounds.Remove(tileLocation * 64f);
		}

		public static void onLocationLeave()
		{
			sounds.Clear();
			volumeOverrideForLocChange = -0.5f;
			if (babblingBrook != null)
			{
				babblingBrook.Pause();
			}
			if (cracklingFire != null)
			{
				cracklingFire.Pause();
			}
			if (engine != null)
			{
				engine.SetVariable("Frequency", 100f);
				engine.Pause();
			}
			if (cricket != null)
			{
				cricket.Pause();
			}
		}
	}
}
