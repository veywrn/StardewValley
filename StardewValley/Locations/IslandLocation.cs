using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class IslandLocation : GameLocation
	{
		public const int TOTAL_WALNUTS = 130;

		[XmlIgnore]
		public List<ParrotPlatform> parrotPlatforms = new List<ParrotPlatform>();

		[XmlIgnore]
		public NetList<ParrotUpgradePerch, NetRef<ParrotUpgradePerch>> parrotUpgradePerches = new NetList<ParrotUpgradePerch, NetRef<ParrotUpgradePerch>>();

		[XmlIgnore]
		public NetList<Point, NetPoint> buriedNutPoints = new NetList<Point, NetPoint>();

		[XmlElement("locationGemBird")]
		public NetRef<IslandGemBird> locationGemBird = new NetRef<IslandGemBird>();

		[XmlIgnore]
		protected Texture2D _dayParallaxTexture;

		[XmlIgnore]
		protected Texture2D _nightParallaxTexture;

		[XmlIgnore]
		protected List<TemporaryAnimatedSprite> underwaterSprites = new List<TemporaryAnimatedSprite>();

		public IslandLocation()
		{
		}

		public void ApplyUnsafeMapOverride(string override_map, Microsoft.Xna.Framework.Rectangle? source_rect, Microsoft.Xna.Framework.Rectangle dest_rect)
		{
			ApplyMapOverride(override_map, source_rect, dest_rect);
			Microsoft.Xna.Framework.Rectangle nontile_rect = new Microsoft.Xna.Framework.Rectangle(dest_rect.X * 64, dest_rect.Y * 64, dest_rect.Width * 64, dest_rect.Height * 64);
			if (this == Game1.player.currentLocation && nontile_rect.Intersects(Game1.player.GetBoundingBox()) && Game1.player.currentLocation.isCollidingPosition(Game1.player.GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, Game1.player))
			{
				Game1.player.TemporaryPassableTiles.Add(nontile_rect);
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(parrotUpgradePerches, buriedNutPoints, locationGemBird);
		}

		public override string doesTileHaveProperty(int xTile, int yTile, string propertyName, string layerName)
		{
			if (layerName == "Back" && propertyName == "Diggable" && IsBuriedNutLocation(new Point(xTile, yTile)))
			{
				return "T";
			}
			return base.doesTileHaveProperty(xTile, yTile, propertyName, layerName);
		}

		public virtual void SetBuriedNutLocations()
		{
		}

		public virtual List<Vector2> GetAdditionalWalnutBushes()
		{
			return null;
		}

		public IslandLocation(string map, string name)
			: base(map, name)
		{
			SetBuriedNutLocations();
			foreach (LargeTerrainFeature t in largeTerrainFeatures)
			{
				if (t is Bush)
				{
					(t as Bush).overrideSeason.Value = 1;
					(t as Bush).setUpSourceRect();
				}
			}
		}

		public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			return true;
		}

		public override bool answerDialogue(Response answer)
		{
			foreach (ParrotPlatform parrotPlatform in parrotPlatforms)
			{
				if (parrotPlatform.AnswerQuestion(answer))
				{
					return true;
				}
			}
			foreach (ParrotUpgradePerch parrotUpgradePerch in parrotUpgradePerches)
			{
				if (parrotUpgradePerch.AnswerQuestion(answer))
				{
					return true;
				}
			}
			return base.answerDialogue(answer);
		}

		public override void cleanupBeforePlayerExit()
		{
			foreach (ParrotPlatform parrotPlatform in parrotPlatforms)
			{
				parrotPlatform.Cleanup();
			}
			foreach (ParrotUpgradePerch parrotUpgradePerch in parrotUpgradePerches)
			{
				parrotUpgradePerch.Cleanup();
			}
			if (_dayParallaxTexture != null)
			{
				_dayParallaxTexture = null;
			}
			if (_nightParallaxTexture != null)
			{
				_nightParallaxTexture = null;
			}
			underwaterSprites.Clear();
			base.cleanupBeforePlayerExit();
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			foreach (ParrotPlatform parrotPlatform in parrotPlatforms)
			{
				if (parrotPlatform.CheckCollisions(position))
				{
					return true;
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
		}

		protected void addMoonlightJellies(int numTries, Random r, Microsoft.Xna.Framework.Rectangle exclusionRect)
		{
			for (int i = 0; i < numTries; i++)
			{
				Point tile = new Point(r.Next(base.Map.Layers[0].LayerWidth), r.Next(base.Map.Layers[0].LayerHeight));
				if (isOpenWater(tile.X, tile.Y) && !exclusionRect.Contains(tile) && FishingRod.distanceToLand(tile.X, tile.Y, this) >= 2)
				{
					bool tooClose = false;
					foreach (TemporaryAnimatedSprite t in underwaterSprites)
					{
						Point otherTile = new Point((int)t.position.X / 64, (int)t.position.Y / 64);
						if (Utility.distance(tile.X, otherTile.X, tile.Y, otherTile.Y) <= 2f)
						{
							tooClose = true;
							break;
						}
					}
					if (!tooClose)
					{
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle((r.NextDouble() < 0.2) ? 304 : 256, (r.NextDouble() < 0.01) ? 32 : 16, 16, 16), 250f, 3, 9999, new Vector2(tile.X, tile.Y) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White * 0.66f, 4f, 0f, 0f, 0f)
						{
							yPeriodic = (Game1.random.NextDouble() < 0.76),
							yPeriodicRange = 12f,
							yPeriodicLoopTime = Game1.random.Next(5500, 8000),
							xPeriodic = (Game1.random.NextDouble() < 0.76),
							xPeriodicLoopTime = Game1.random.Next(5500, 8000),
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							pingPong = true
						});
					}
				}
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (Game1.currentLocation == this)
			{
				foreach (ParrotPlatform parrotPlatform in parrotPlatforms)
				{
					parrotPlatform.Update(time);
				}
			}
			foreach (ParrotUpgradePerch parrotUpgradePerch in parrotUpgradePerches)
			{
				parrotUpgradePerch.Update(time);
			}
			for (int i = underwaterSprites.Count - 1; i >= 0; i--)
			{
				if (underwaterSprites[i].update(time))
				{
					underwaterSprites.RemoveAt(i);
				}
			}
			base.UpdateWhenCurrentLocation(time);
		}

		public override void tryToAddCritters(bool onlyIfOnScreen = false)
		{
			if (Game1.random.NextDouble() < 0.20000000298023224 && !Game1.IsRainingHere(this) && !Game1.isDarkOut())
			{
				Vector2 origin2 = Vector2.Zero;
				origin2 = ((Game1.random.NextDouble() < 0.75) ? new Vector2((float)Game1.viewport.X + Utility.RandomFloat(0f, Game1.viewport.Width), Game1.viewport.Y - 64) : new Vector2(Game1.viewport.X + Game1.viewport.Width + 64, Utility.RandomFloat(0f, Game1.viewport.Height)));
				int parrots_to_spawn = 1;
				if (Game1.random.NextDouble() < 0.5)
				{
					parrots_to_spawn++;
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					parrots_to_spawn++;
				}
				for (int i = 0; i < parrots_to_spawn; i++)
				{
					addCritter(new OverheadParrot(origin2 + new Vector2(i * 64, -i * 64)));
				}
			}
			if (!Game1.IsRainingHere(this))
			{
				double mapArea = map.Layers[0].LayerWidth * map.Layers[0].LayerHeight;
				double butterflyChance = Math.Max(0.1, Math.Min(0.25, mapArea / 15000.0));
				addButterflies(butterflyChance, onlyIfOnScreen);
			}
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			locationGemBird.Value = null;
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			foreach (ParrotUpgradePerch parrotUpgradePerch in parrotUpgradePerches)
			{
				parrotUpgradePerch.UpdateEvenIfFarmerIsntHere(time);
			}
			if (locationGemBird.Value != null && locationGemBird.Value.Update(time, this) && Game1.IsMasterGame)
			{
				locationGemBird.Value = null;
			}
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			foreach (ParrotUpgradePerch parrotUpgradePerch in parrotUpgradePerches)
			{
				parrotUpgradePerch.UpdateCompletionStatus();
			}
			if (l is IslandLocation)
			{
				locationGemBird.Value = (l as IslandLocation).locationGemBird.Value;
			}
		}

		public void AddAdditionalWalnutBushes()
		{
			List<Vector2> additional_bushes = GetAdditionalWalnutBushes();
			if (additional_bushes != null)
			{
				foreach (Vector2 point in additional_bushes)
				{
					LargeTerrainFeature existing_feature = getLargeTerrainFeatureAt((int)point.X, (int)point.Y);
					if (!(existing_feature is Bush) || (existing_feature as Bush).size.Value != 4)
					{
						largeTerrainFeatures.Add(new Bush(new Vector2((int)point.X, (int)point.Y), 4, this));
					}
				}
			}
		}

		public override bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			foreach (ParrotUpgradePerch perch in parrotUpgradePerches)
			{
				if (perch.IsAtTile(xTile, yTile) && perch.IsAvailable(use_cached_value: true))
				{
					return true;
				}
			}
			return base.isActionableTile(xTile, yTile, who);
		}

		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (IsBuriedNutLocation(new Point(xLocation, yLocation)))
			{
				Game1.player.team.MarkCollectedNut("Buried_" + base.Name + "_" + xLocation + "_" + yLocation);
				Game1.multiplayer.broadcastNutDig(this, new Point(xLocation, yLocation));
				return "";
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		public override void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
		{
			Random r = new Random(xLocation * 2000 + yLocation + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
			int toDigUp = -1;
			int stack = 1;
			if (Game1.netWorldState.Value.GoldenCoconutCracked.Value && r.NextDouble() < 0.1)
			{
				toDigUp = 791;
			}
			else if (r.NextDouble() < 0.33)
			{
				toDigUp = 831;
				stack = r.Next(2, 5);
			}
			else if (r.NextDouble() < 0.15)
			{
				toDigUp = 275;
				stack = r.Next(1, 3);
			}
			if (toDigUp != -1)
			{
				for (int i = 0; i < stack; i++)
				{
					Game1.createItemDebris(new Object(toDigUp, 1), new Vector2(xLocation, yLocation) * 64f, -1, this);
				}
			}
			base.digUpArtifactSpot(xLocation, yLocation, who);
		}

		public virtual bool IsBuriedNutLocation(Point point)
		{
			foreach (Point buriedNutPoint in buriedNutPoints)
			{
				if (buriedNutPoint == point)
				{
					return true;
				}
			}
			return false;
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			foreach (ParrotUpgradePerch parrotUpgradePerch in parrotUpgradePerches)
			{
				if (parrotUpgradePerch.CheckAction(tileLocation, who))
				{
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			if (new Random((int)(Game1.stats.DaysPlayed + Game1.stats.TimesFished + Game1.uniqueIDForThisGame)).NextDouble() < 0.15 && (!Game1.player.team.limitedNutDrops.ContainsKey("IslandFishing") || Game1.player.team.limitedNutDrops["IslandFishing"] < 5))
			{
				if (!Game1.IsMultiplayer)
				{
					if (!Game1.player.team.limitedNutDrops.ContainsKey("IslandFishing"))
					{
						Game1.player.team.limitedNutDrops["IslandFishing"] = 1;
					}
					else
					{
						Game1.player.team.limitedNutDrops["IslandFishing"]++;
					}
					return new Object(73, 1);
				}
				Game1.player.team.RequestLimitedNutDrops("IslandFishing", this, (int)bobberTile.X * 64, (int)bobberTile.Y * 64, 5);
				return null;
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (ParrotPlatform parrotPlatform in parrotPlatforms)
			{
				parrotPlatform.Draw(b);
			}
			foreach (ParrotUpgradePerch parrotUpgradePerch in parrotUpgradePerches)
			{
				parrotUpgradePerch.Draw(b);
			}
			if (locationGemBird.Value != null)
			{
				locationGemBird.Value.Draw(b);
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			foreach (ParrotUpgradePerch parrotUpgradePerch in parrotUpgradePerches)
			{
				parrotUpgradePerch.DrawAboveAlwaysFrontLayer(b);
			}
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (ParrotPlatform parrotPlatform in parrotPlatforms)
			{
				if (parrotPlatform.OccupiesTile(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		public override bool isTileOccupied(Vector2 tileLocation, string characterToIgnore = "", bool ignoreAllCharacters = false)
		{
			foreach (ParrotPlatform parrotPlatform in parrotPlatforms)
			{
				if (parrotPlatform.OccupiesTile(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupied(tileLocation, characterToIgnore, ignoreAllCharacters);
		}

		protected override void resetLocalState()
		{
			parrotPlatforms.Clear();
			parrotPlatforms = ParrotPlatform.CreateParrotPlatformsForArea(this);
			foreach (ParrotUpgradePerch parrotUpgradePerch in parrotUpgradePerches)
			{
				parrotUpgradePerch.ResetForPlayerEntry();
			}
			if (base.IsOutdoors && !Game1.isStartingToGetDarkOut() && !Game1.isDarkOut() && Game1.isMusicContextActiveButNotPlaying())
			{
				if (Game1.IsRainingHere(this))
				{
					Game1.changeMusicTrack("rain", track_interruptable: true);
				}
				else
				{
					Game1.changeMusicTrack("tropical_island_day_ambient", track_interruptable: true);
				}
			}
			base.resetLocalState();
		}

		public override void seasonUpdate(string season, bool onLoad = false)
		{
		}

		public override void updateSeasonalTileSheets(Map map = null)
		{
		}

		public override void checkForMusic(GameTime time)
		{
			if (base.IsOutdoors && Game1.isMusicContextActiveButNotPlaying() && !Game1.IsRainingHere(this) && !Game1.eventUp)
			{
				if (!Game1.isDarkOut())
				{
					Game1.changeMusicTrack("tropical_island_day_ambient", track_interruptable: true);
				}
				else if (Game1.isDarkOut() && Game1.timeOfDay < 2500)
				{
					Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
				}
			}
		}

		public override void drawWater(SpriteBatch b)
		{
			foreach (TemporaryAnimatedSprite underwaterSprite in underwaterSprites)
			{
				underwaterSprite.draw(b);
			}
			base.drawWater(b);
		}

		public virtual void DrawParallaxHorizon(SpriteBatch b, bool horizontal_parallax = true)
		{
			float draw_zoom = 4f;
			if (_dayParallaxTexture == null)
			{
				_dayParallaxTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cloudy_Ocean_BG");
			}
			if (_nightParallaxTexture == null)
			{
				_nightParallaxTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cloudy_Ocean_BG_Night");
			}
			float horizontal_parallax_amount = (float)_dayParallaxTexture.Width * draw_zoom - (float)map.DisplayWidth;
			float t2 = 0f;
			int background_y_adjustment = -640;
			int y = (int)((float)Game1.viewport.Y * 0.2f + (float)background_y_adjustment);
			if (horizontal_parallax)
			{
				if (map.DisplayWidth - Game1.viewport.Width < 0)
				{
					t2 = 0.5f;
				}
				else if (map.DisplayWidth - Game1.viewport.Width > 0)
				{
					t2 = (float)Game1.viewport.X / (float)(map.DisplayWidth - Game1.viewport.Width);
				}
			}
			else
			{
				t2 = 0.5f;
			}
			if (Game1.game1.takingMapScreenshot)
			{
				y = background_y_adjustment;
				t2 = 0.5f;
			}
			float arc = 0.25f;
			t2 = Utility.Lerp(0.5f + arc, 0.5f - arc, t2);
			float day_night_transition2 = (float)Utility.ConvertTimeToMinutes(Game1.timeOfDay + (int)((float)Game1.gameTimeInterval / 7000f * 10f % 10f) - Game1.getStartingToGetDarkTime()) / (float)Utility.ConvertTimeToMinutes(Game1.getTrulyDarkTime() - Game1.getStartingToGetDarkTime());
			day_night_transition2 = Utility.Clamp(day_night_transition2, 0f, 1f);
			b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Microsoft.Xna.Framework.Rectangle(0, 0, map.DisplayWidth, map.DisplayHeight)), new Color(1, 122, 217, 255));
			b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Microsoft.Xna.Framework.Rectangle(0, 0, map.DisplayWidth, map.DisplayHeight)), new Color(0, 7, 63, 255) * day_night_transition2);
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)((0f - horizontal_parallax_amount) * t2), y, (int)((float)_dayParallaxTexture.Width * draw_zoom), (int)((float)_dayParallaxTexture.Height * draw_zoom));
			Microsoft.Xna.Framework.Rectangle source_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, _dayParallaxTexture.Width, _dayParallaxTexture.Height);
			int left_boundary = 0;
			if (rectangle.X < left_boundary)
			{
				int offset2 = left_boundary - rectangle.X;
				rectangle.X += offset2;
				rectangle.Width -= offset2;
				source_rect.X += (int)((float)offset2 / draw_zoom);
				source_rect.Width -= (int)((float)offset2 / draw_zoom);
			}
			int right_boundary = map.DisplayWidth;
			if (rectangle.X + rectangle.Width > right_boundary)
			{
				int offset = rectangle.X + rectangle.Width - right_boundary;
				rectangle.Width -= offset;
				source_rect.Width -= (int)((float)offset / draw_zoom);
			}
			if (source_rect.Width > 0 && rectangle.Width > 0)
			{
				b.Draw(_dayParallaxTexture, Game1.GlobalToLocal(Game1.viewport, rectangle), source_rect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
				b.Draw(_nightParallaxTexture, Game1.GlobalToLocal(Game1.viewport, rectangle), source_rect, Color.White * day_night_transition2, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			}
		}
	}
}
