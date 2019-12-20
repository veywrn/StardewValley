using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using System;

namespace StardewValley.Events
{
	public class WitchEvent : FarmEvent, INetObject<NetFields>
	{
		public const int identifier = 942069;

		private Vector2 witchPosition;

		private Building targetBuilding;

		private Farm f;

		private Random r;

		private int witchFrame;

		private int witchAnimationTimer;

		private int animationLoopsDone;

		private int timerSinceFade;

		private bool animateLeft;

		private bool terminate;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public bool setUp()
		{
			f = (Game1.getLocationFromName("Farm") as Farm);
			r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			foreach (Building b2 in f.buildings)
			{
				if (b2 is Coop && !b2.buildingType.Equals("Coop") && !(b2.indoors.Value as AnimalHouse).isFull() && b2.indoors.Value.objects.Count() < 50 && r.NextDouble() < 0.8)
				{
					targetBuilding = b2;
				}
			}
			if (targetBuilding == null)
			{
				foreach (Building b in f.buildings)
				{
					if (b.buildingType.Equals("Slime Hutch") && b.indoors.Value.characters.Count > 0 && r.NextDouble() < 0.5 && b.indoors.Value.numberOfObjectsOfType(83, bigCraftable: true) == 0)
					{
						targetBuilding = b;
					}
				}
			}
			if (targetBuilding == null)
			{
				return true;
			}
			Game1.currentLightSources.Add(new LightSource(4, witchPosition, 2f, Color.Black, 942069, LightSource.LightContext.None, 0L));
			Game1.currentLocation = f;
			f.resetForPlayerEntry();
			Game1.fadeClear();
			Game1.nonWarpFade = true;
			Game1.timeOfDay = 2400;
			Game1.ambientLight = new Color(200, 190, 40);
			Game1.displayHUD = false;
			Game1.freezeControls = true;
			Game1.viewportFreeze = true;
			Game1.displayFarmer = false;
			Game1.viewport.X = Math.Max(0, Math.Min(f.map.DisplayWidth - Game1.viewport.Width, (int)targetBuilding.tileX * 64 - Game1.viewport.Width / 2));
			Game1.viewport.Y = Math.Max(0, Math.Min(f.map.DisplayHeight - Game1.viewport.Height, ((int)targetBuilding.tileY - 3) * 64 - Game1.viewport.Height / 2));
			witchPosition = new Vector2(Game1.viewport.X + Game1.viewport.Width + 128, (int)targetBuilding.tileY * 64 - 64);
			Game1.changeMusicTrack("nightTime");
			DelayedAction.playSoundAfterDelay("cacklingWitch", 3200);
			return false;
		}

		public bool tickUpdate(GameTime time)
		{
			if (terminate)
			{
				return true;
			}
			Game1.UpdateGameClock(time);
			f.UpdateWhenCurrentLocation(time);
			f.updateEvenIfFarmerIsntHere(time);
			Game1.UpdateOther(time);
			Utility.repositionLightSource(942069, witchPosition + new Vector2(32f, 32f));
			if (animationLoopsDone < 1)
			{
				timerSinceFade += time.ElapsedGameTime.Milliseconds;
			}
			if (witchPosition.X > (float)((int)targetBuilding.tileX * 64 + 96))
			{
				if (timerSinceFade < 2000)
				{
					return false;
				}
				witchPosition.X -= (float)time.ElapsedGameTime.Milliseconds * 0.4f;
				witchPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 512.0) * 1f;
			}
			else if (animationLoopsDone < 4)
			{
				witchPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 512.0) * 1f;
				witchAnimationTimer += time.ElapsedGameTime.Milliseconds;
				if (witchAnimationTimer > 2000)
				{
					witchAnimationTimer = 0;
					if (!animateLeft)
					{
						witchFrame++;
						if (witchFrame == 1)
						{
							animateLeft = true;
							for (int i = 0; i < 75; i++)
							{
								f.temporarySprites.Add(new TemporaryAnimatedSprite(10, witchPosition + new Vector2(8f, 80f), (r.NextDouble() < 0.5) ? Color.Lime : Color.DarkViolet)
								{
									motion = new Vector2((float)r.Next(-100, 100) / 100f, 1.5f),
									alphaFade = 0.015f,
									delayBeforeAnimationStart = i * 30,
									layerDepth = 1f
								});
							}
							Game1.playSound("debuffSpell");
						}
					}
					else
					{
						witchFrame--;
						animationLoopsDone = 4;
						DelayedAction.playSoundAfterDelay("cacklingWitch", 2500);
					}
				}
			}
			else
			{
				witchAnimationTimer += time.ElapsedGameTime.Milliseconds;
				witchFrame = 0;
				if (witchAnimationTimer > 1000 && witchPosition.X > -999999f)
				{
					witchPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 256.0) * 2f;
					witchPosition.X -= (float)time.ElapsedGameTime.Milliseconds * 0.4f;
				}
				if (witchPosition.X < (float)(Game1.viewport.X - 128) || float.IsNaN(witchPosition.X))
				{
					if (!Game1.fadeToBlack && witchPosition.X != -999999f)
					{
						Game1.globalFadeToBlack(afterLastFade);
						Game1.changeMusicTrack("none");
						timerSinceFade = 0;
						witchPosition.X = -999999f;
					}
					timerSinceFade += time.ElapsedGameTime.Milliseconds;
				}
			}
			return false;
		}

		public void afterLastFade()
		{
			terminate = true;
			Game1.globalFadeToClear();
		}

		public void draw(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, witchPosition), new Rectangle(277, 1886 + witchFrame * 29, 34, 29), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
		}

		public void makeChangesToLocation()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (targetBuilding.buildingType.Equals("Slime Hutch"))
			{
				foreach (NPC i in targetBuilding.indoors.Value.characters)
				{
					if (i is GreenSlime)
					{
						(i as GreenSlime).color.Value = new Color(40 + r.Next(10), 40 + r.Next(10), 40 + r.Next(10));
					}
				}
				return;
			}
			int tries = 0;
			Vector2 v;
			while (true)
			{
				if (tries >= 200)
				{
					return;
				}
				v = new Vector2(r.Next(2, targetBuilding.indoors.Value.Map.Layers[0].LayerWidth - 2), r.Next(2, targetBuilding.indoors.Value.Map.Layers[0].LayerHeight - 2));
				if (targetBuilding.indoors.Value.isTileLocationTotallyClearAndPlaceable(v) || (targetBuilding.indoors.Value.terrainFeatures.ContainsKey(v) && targetBuilding.indoors.Value.terrainFeatures[v] is Flooring))
				{
					break;
				}
				tries++;
			}
			targetBuilding.indoors.Value.objects.Add(v, new Object(Vector2.Zero, 305, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: true));
		}

		public void drawAboveEverything(SpriteBatch b)
		{
		}
	}
}
