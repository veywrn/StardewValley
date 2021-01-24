using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class Woods : GameLocation
	{
		public const int numBaubles = 25;

		private List<Vector2> baubles;

		private List<WeatherDebris> weatherDebris;

		public readonly NetObjectList<ResourceClump> stumps = new NetObjectList<ResourceClump>();

		[XmlElement("hasUnlockedStatue")]
		public readonly NetBool hasUnlockedStatue = new NetBool();

		[XmlIgnore]
		[Obsolete]
		public bool hasFoundStardrop;

		[XmlElement("addedSlimesToday")]
		private readonly NetBool addedSlimesToday = new NetBool();

		[XmlIgnore]
		private readonly NetEvent0 statueAnimationEvent = new NetEvent0();

		protected Color _ambientLightColor = Color.White;

		private int statueTimer;

		public Woods()
		{
		}

		public Woods(string map, string name)
			: base(map, name)
		{
			isOutdoors.Value = true;
			ignoreDebrisWeather.Value = true;
			ignoreOutdoorLighting.Value = true;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(stumps, addedSlimesToday, statueAnimationEvent, hasUnlockedStatue);
			statueAnimationEvent.onEvent += doStatueAnimation;
		}

		public bool localPlayerHasFoundStardrop()
		{
			return Game1.player.hasOrWillReceiveMail("CF_Statue");
		}

		public override void checkForMusic(GameTime time)
		{
			if (Game1.isMusicContextActiveButNotPlaying())
			{
				if (Game1.isRaining)
				{
					Game1.changeMusicTrack("rain");
				}
				else if (!Game1.isDarkOut())
				{
					Game1.changeMusicTrack(Game1.currentSeason + "_day_ambient");
				}
			}
		}

		public void statueAnimation(Farmer who)
		{
			if (!hasUnlockedStatue)
			{
				who.reduceActiveItemByOne();
				hasUnlockedStatue.Value = true;
				statueAnimationEvent.Fire();
			}
		}

		private void doStatueAnimation()
		{
			temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(8f, 7f) * 64f, Color.White, 9, flipped: false, 50f));
			temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(9f, 7f) * 64f, Color.Orange, 9, flipped: false, 70f));
			temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(8f, 6f) * 64f, Color.White, 9, flipped: false, 60f));
			temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(9f, 6f) * 64f, Color.OrangeRed, 9, flipped: false, 120f));
			temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(8f, 5f) * 64f, Color.Red, 9));
			temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(9f, 5f) * 64f, Color.White, 9, flipped: false, 170f));
			temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(544f, 464f), Color.Orange, 9, flipped: false, 40f));
			temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(608f, 464f), Color.White, 9, flipped: false, 90f));
			temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(544f, 400f), Color.OrangeRed, 9, flipped: false, 190f));
			temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(608f, 400f), Color.White, 9, flipped: false, 80f));
			temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(544f, 336f), Color.Red, 9, flipped: false, 69f));
			temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(608f, 336f), Color.OrangeRed, 9, flipped: false, 130f));
			temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(480f, 464f), Color.Orange, 9, flipped: false, 40f));
			temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(672f, 368f), Color.White, 9, flipped: false, 90f));
			temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(480f, 464f), Color.Red, 9, flipped: false, 30f));
			temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(672f, 368f), Color.White, 9, flipped: false, 180f));
			localSound("secret1");
			updateStatueEyes();
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Tile tile = map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
			if (tile != null && who.IsLocalPlayer)
			{
				int tileIndex = tile.TileIndex;
				if ((uint)(tileIndex - 1140) <= 1u)
				{
					if (!hasUnlockedStatue)
					{
						if (who.ActiveObject != null && who.ActiveObject.ParentSheetIndex == 417)
						{
							statueTimer = 1000;
							who.freezePause = 1000;
							Game1.changeMusicTrack("none");
							playSound("newArtifact");
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Woods_Statue").Replace('\n', '^'));
						}
					}
					if ((bool)hasUnlockedStatue && !localPlayerHasFoundStardrop() && who.freeSpotsInInventory() > 0)
					{
						who.addItemByMenuIfNecessaryElseHoldUp(new Object(434, 1));
						if (!Game1.player.mailReceived.Contains("CF_Statue"))
						{
							Game1.player.mailReceived.Add("CF_Statue");
						}
					}
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			if (Game1.random.NextDouble() < 0.08)
			{
				return new Furniture(2425, Vector2.Zero);
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			foreach (ResourceClump stump in stumps)
			{
				if (stump.getBoundingBox(stump.tile).Intersects(position))
				{
					return true;
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (t is Axe)
			{
				Point p = new Point(tileX * 64 + 32, tileY * 64 + 32);
				for (int i = stumps.Count - 1; i >= 0; i--)
				{
					if (stumps[i].getBoundingBox(stumps[i].tile).Contains(p))
					{
						if (stumps[i].performToolAction(t, 1, stumps[i].tile, this))
						{
							stumps.RemoveAt(i);
						}
						return true;
					}
				}
			}
			return false;
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			for (int j = 0; j < characters.Count; j++)
			{
				if (characters[j] != null && characters[j] is Monster)
				{
					characters.RemoveAt(j);
					j--;
				}
			}
			addedSlimesToday.Value = false;
			map.Properties.TryGetValue("Stumps", out PropertyValue rawStumpData);
			if (rawStumpData == null)
			{
				return;
			}
			string[] stumpData = rawStumpData.ToString().Split(' ');
			for (int i = 0; i < stumpData.Length; i += 3)
			{
				int x = Convert.ToInt32(stumpData[i]);
				int y = Convert.ToInt32(stumpData[i + 1]);
				Vector2 tile = new Vector2(x, y);
				bool foundStump = false;
				foreach (ResourceClump stump in stumps)
				{
					if (stump.tile.Equals(tile))
					{
						foundStump = true;
						break;
					}
				}
				if (!foundStump)
				{
					stumps.Add(new ResourceClump(600, 2, 2, tile));
					removeObject(tile, showDestroyedObject: false);
					removeObject(tile + new Vector2(1f, 0f), showDestroyedObject: false);
					removeObject(tile + new Vector2(1f, 1f), showDestroyedObject: false);
					removeObject(tile + new Vector2(0f, 1f), showDestroyedObject: false);
				}
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			Game1.changeMusicTrack("none");
			if (baubles != null)
			{
				baubles.Clear();
			}
			if (weatherDebris != null)
			{
				weatherDebris.Clear();
			}
		}

		public override bool isTileLocationTotallyClearAndPlaceable(Vector2 v)
		{
			foreach (ResourceClump stump in stumps)
			{
				if (stump.occupiesTile((int)v.X, (int)v.Y))
				{
					return false;
				}
			}
			return base.isTileLocationTotallyClearAndPlaceable(v);
		}

		protected override void resetSharedState()
		{
			if (!addedSlimesToday)
			{
				addedSlimesToday.Value = true;
				Random rand = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame + 12);
				for (int tries = 50; tries > 0; tries--)
				{
					Vector2 tile = getRandomTile();
					if (rand.NextDouble() < 0.25 && isTileLocationTotallyClearAndPlaceable(tile))
					{
						switch (Game1.currentSeason)
						{
						case "spring":
							characters.Add(new GreenSlime(tile * 64f, 0));
							break;
						case "summer":
							characters.Add(new GreenSlime(tile * 64f, 0));
							break;
						case "fall":
							characters.Add(new GreenSlime(tile * 64f, (!(rand.NextDouble() < 0.5)) ? 40 : 0));
							break;
						case "winter":
							characters.Add(new GreenSlime(tile * 64f, 40));
							break;
						}
					}
				}
			}
			base.resetSharedState();
		}

		protected void _updateWoodsLighting()
		{
			if (Game1.currentLocation == this)
			{
				int fade_start_time = Utility.ConvertTimeToMinutes(Game1.getStartingToGetDarkTime());
				int fade_end_time = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime());
				int light_fade_start_time = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime());
				int light_fade_end_time = Utility.ConvertTimeToMinutes(Game1.getTrulyDarkTime());
				float num = (float)Utility.ConvertTimeToMinutes(Game1.timeOfDay) + (float)Game1.gameTimeInterval / 7000f * 10f;
				float lerp = Utility.Clamp((num - (float)fade_start_time) / (float)(fade_end_time - fade_start_time), 0f, 1f);
				float light_lerp = Utility.Clamp((num - (float)light_fade_start_time) / (float)(light_fade_end_time - light_fade_start_time), 0f, 1f);
				Game1.ambientLight.R = (byte)Utility.Lerp((int)_ambientLightColor.R, (int)Game1.outdoorLight.R, lerp);
				Game1.ambientLight.G = (byte)Utility.Lerp((int)_ambientLightColor.G, (int)Game1.outdoorLight.G, lerp);
				Game1.ambientLight.B = (byte)Utility.Lerp((int)_ambientLightColor.B, (int)Game1.outdoorLight.B, lerp);
				Game1.ambientLight.A = (byte)Utility.Lerp((int)_ambientLightColor.A, (int)Game1.outdoorLight.A, lerp);
				Color light_color = Color.Black;
				light_color.A = (byte)Utility.Lerp(255f, 0f, light_lerp);
				foreach (LightSource light in Game1.currentLightSources)
				{
					if (light.lightContext.Value == LightSource.LightContext.MapLight)
					{
						light.color.Value = light_color;
					}
				}
			}
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			updateStatueEyes();
		}

		protected override void resetLocalState()
		{
			_ambientLightColor = new Color(150, 120, 50);
			ignoreOutdoorLighting.Value = false;
			if (!Game1.player.mailReceived.Contains("beenToWoods"))
			{
				Game1.player.mailReceived.Add("beenToWoods");
			}
			base.resetLocalState();
			_updateWoodsLighting();
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			int numberOfBaubles = 25 + r.Next(0, 75);
			if (!Game1.isRaining)
			{
				baubles = new List<Vector2>();
				for (int i = 0; i < numberOfBaubles; i++)
				{
					baubles.Add(new Vector2(Game1.random.Next(0, map.DisplayWidth), Game1.random.Next(0, map.DisplayHeight)));
				}
				if (!Game1.currentSeason.Equals("winter"))
				{
					weatherDebris = new List<WeatherDebris>();
					int spacing = 192;
					int leafType = 1;
					if (Game1.currentSeason.Equals("fall"))
					{
						leafType = 2;
					}
					for (int j = 0; j < numberOfBaubles; j++)
					{
						weatherDebris.Add(new WeatherDebris(new Vector2(j * spacing % Game1.graphics.GraphicsDevice.Viewport.Width + Game1.random.Next(spacing), j * spacing / Game1.graphics.GraphicsDevice.Viewport.Width * spacing % Game1.graphics.GraphicsDevice.Viewport.Height + Game1.random.Next(spacing)), leafType, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
					}
				}
			}
			if (Game1.timeOfDay < 1800)
			{
				Game1.changeMusicTrack("woodsTheme");
			}
		}

		private void updateStatueEyes()
		{
			if ((bool)hasUnlockedStatue && !localPlayerHasFoundStardrop())
			{
				map.GetLayer("Front").Tiles[8, 6].TileIndex = 1117;
				map.GetLayer("Front").Tiles[9, 6].TileIndex = 1118;
			}
			else
			{
				map.GetLayer("Front").Tiles[8, 6].TileIndex = 1115;
				map.GetLayer("Front").Tiles[9, 6].TileIndex = 1116;
			}
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
			statueAnimationEvent.Poll();
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			_updateWoodsLighting();
			if (statueTimer > 0)
			{
				statueTimer -= time.ElapsedGameTime.Milliseconds;
				if (statueTimer <= 0)
				{
					statueAnimation(Game1.player);
				}
			}
			if (baubles != null)
			{
				for (int i = 0; i < baubles.Count; i++)
				{
					Vector2 v = default(Vector2);
					v.X = baubles[i].X - Math.Max(0.4f, Math.Min(1f, (float)i * 0.01f)) - (float)((double)((float)i * 0.01f) * Math.Sin(Math.PI * 2.0 * (double)time.TotalGameTime.Milliseconds / 8000.0));
					v.Y = baubles[i].Y + Math.Max(0.5f, Math.Min(1.2f, (float)i * 0.02f));
					if (v.Y > (float)map.DisplayHeight || v.X < 0f)
					{
						v.X = Game1.random.Next(0, map.DisplayWidth);
						v.Y = -64f;
					}
					baubles[i] = v;
				}
			}
			if (weatherDebris != null)
			{
				foreach (WeatherDebris weatherDebri in weatherDebris)
				{
					weatherDebri.update();
				}
				Game1.updateDebrisWeatherForMovement(weatherDebris);
			}
			foreach (ResourceClump stump in stumps)
			{
				stump.tickUpdate(time, stump.tile, this);
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (!Game1.eventUp || (currentEvent != null && currentEvent.showGroundObjects))
			{
				foreach (ResourceClump stump in stumps)
				{
					stump.draw(b, stump.tile);
				}
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			if (baubles != null)
			{
				for (int i = 0; i < baubles.Count; i++)
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, baubles[i]), new Microsoft.Xna.Framework.Rectangle(346 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(i * 25)) % 600.0) / 150 * 5, 1971, 5, 5), Color.White, (float)i * ((float)Math.PI / 8f), Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
			}
			if (weatherDebris != null && currentEvent == null)
			{
				foreach (WeatherDebris weatherDebri in weatherDebris)
				{
					weatherDebri.draw(b);
				}
			}
		}
	}
}
