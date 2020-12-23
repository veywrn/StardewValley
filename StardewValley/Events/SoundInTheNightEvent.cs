using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace StardewValley.Events
{
	public class SoundInTheNightEvent : FarmEvent, INetObject<NetFields>
	{
		public const int cropCircle = 0;

		public const int meteorite = 1;

		public const int dogs = 2;

		public const int owl = 3;

		public const int earthquake = 4;

		private readonly NetInt behavior = new NetInt();

		private int timer;

		private string soundName;

		private string message;

		private bool playedSound;

		private bool showedMessage;

		private Vector2 targetLocation;

		private Building targetBuilding;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public SoundInTheNightEvent()
		{
			NetFields.AddField(behavior);
		}

		public SoundInTheNightEvent(int which)
			: this()
		{
			behavior.Value = which;
		}

		public bool setUp()
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			Farm f = Game1.getLocationFromName("Farm") as Farm;
			f.updateMap();
			switch ((int)behavior)
			{
			case 0:
			{
				soundName = "UFO";
				message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_UFO");
				int attempts2;
				for (attempts2 = 50; attempts2 > 0; attempts2--)
				{
					targetLocation = new Vector2(r.Next(5, f.map.GetLayer("Back").TileWidth - 4), r.Next(5, f.map.GetLayer("Back").TileHeight - 4));
					if (f.isTileLocationTotallyClearAndPlaceable(targetLocation))
					{
						break;
					}
				}
				if (attempts2 <= 0)
				{
					return true;
				}
				break;
			}
			case 1:
			{
				soundName = "Meteorite";
				message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_Meteorite");
				targetLocation = new Vector2(r.Next(5, f.map.GetLayer("Back").TileWidth - 20), r.Next(5, f.map.GetLayer("Back").TileHeight - 4));
				for (int j = (int)targetLocation.X; (float)j <= targetLocation.X + 1f; j++)
				{
					for (int i = (int)targetLocation.Y; (float)i <= targetLocation.Y + 1f; i++)
					{
						Vector2 v = new Vector2(j, i);
						if (!f.isTileOpenBesidesTerrainFeatures(v) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X + 1f, v.Y)) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X + 1f, v.Y - 1f)) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X, v.Y - 1f)) || f.doesTileHaveProperty((int)v.X, (int)v.Y, "Water", "Back") != null || f.doesTileHaveProperty((int)v.X + 1, (int)v.Y, "Water", "Back") != null)
						{
							return true;
						}
					}
				}
				break;
			}
			case 2:
				soundName = "dogs";
				if (r.NextDouble() < 0.5)
				{
					return true;
				}
				foreach (Building b in f.buildings)
				{
					if (b.indoors.Value != null && b.indoors.Value is AnimalHouse && !b.animalDoorOpen && (b.indoors.Value as AnimalHouse).animalsThatLiveHere.Count > (b.indoors.Value as AnimalHouse).animals.Count() && r.NextDouble() < (double)(1f / (float)f.buildings.Count))
					{
						targetBuilding = b;
						break;
					}
				}
				if (targetBuilding == null)
				{
					return true;
				}
				return false;
			case 3:
			{
				soundName = "owl";
				int attempts2;
				for (attempts2 = 50; attempts2 > 0; attempts2--)
				{
					targetLocation = new Vector2(r.Next(5, f.map.GetLayer("Back").TileWidth - 4), r.Next(5, f.map.GetLayer("Back").TileHeight - 4));
					if (f.isTileLocationTotallyClearAndPlaceable(targetLocation))
					{
						break;
					}
				}
				if (attempts2 <= 0)
				{
					return true;
				}
				break;
			}
			case 4:
				soundName = "thunder_small";
				message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_Earthquake");
				break;
			}
			Game1.freezeControls = true;
			return false;
		}

		public bool tickUpdate(GameTime time)
		{
			timer += time.ElapsedGameTime.Milliseconds;
			if (timer > 1500 && !playedSound)
			{
				if (soundName != null && !soundName.Equals(""))
				{
					Game1.playSound(soundName);
					playedSound = true;
				}
				if (!playedSound && message != null)
				{
					Game1.drawObjectDialogue(message);
					Game1.globalFadeToClear();
					showedMessage = true;
				}
			}
			if (timer > 7000 && !showedMessage)
			{
				Game1.pauseThenMessage(10, message, showProgressBar: false);
				showedMessage = true;
			}
			if (showedMessage && playedSound)
			{
				Game1.freezeControls = false;
				return true;
			}
			return false;
		}

		public void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
		}

		public void makeChangesToLocation()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farm f = Game1.getLocationFromName("Farm") as Farm;
			switch ((int)behavior)
			{
			case 0:
			{
				Object o = new Object(targetLocation, 96);
				o.minutesUntilReady.Value = 24000 - Game1.timeOfDay;
				f.objects.Add(targetLocation, o);
				break;
			}
			case 1:
				if (f.terrainFeatures.ContainsKey(targetLocation))
				{
					f.terrainFeatures.Remove(targetLocation);
				}
				if (f.terrainFeatures.ContainsKey(targetLocation + new Vector2(1f, 0f)))
				{
					f.terrainFeatures.Remove(targetLocation + new Vector2(1f, 0f));
				}
				if (f.terrainFeatures.ContainsKey(targetLocation + new Vector2(1f, 1f)))
				{
					f.terrainFeatures.Remove(targetLocation + new Vector2(1f, 1f));
				}
				if (f.terrainFeatures.ContainsKey(targetLocation + new Vector2(0f, 1f)))
				{
					f.terrainFeatures.Remove(targetLocation + new Vector2(0f, 1f));
				}
				f.resourceClumps.Add(new ResourceClump(622, 2, 2, targetLocation));
				break;
			case 2:
			{
				AnimalHouse indoors = targetBuilding.indoors.Value as AnimalHouse;
				long idOfRemove = 0L;
				foreach (long a in indoors.animalsThatLiveHere)
				{
					if (!indoors.animals.ContainsKey(a))
					{
						idOfRemove = a;
						break;
					}
				}
				if (Game1.getFarm().animals.ContainsKey(idOfRemove))
				{
					Game1.getFarm().animals.Remove(idOfRemove);
					indoors.animalsThatLiveHere.Remove(idOfRemove);
					foreach (KeyValuePair<long, FarmAnimal> pair in Game1.getFarm().animals.Pairs)
					{
						pair.Value.moodMessage.Value = 5;
					}
				}
				break;
			}
			case 3:
				f.objects.Add(targetLocation, new Object(targetLocation, 95));
				break;
			}
		}

		public void drawAboveEverything(SpriteBatch b)
		{
		}
	}
}
