using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using System;

namespace StardewValley.Events
{
	public class WorldChangeEvent : FarmEvent, INetObject<NetFields>
	{
		public const int identifier = 942066;

		public const int jojaGreenhouse = 0;

		public const int junimoGreenHouse = 1;

		public const int jojaBoiler = 2;

		public const int junimoBoiler = 3;

		public const int jojaBridge = 4;

		public const int junimoBridge = 5;

		public const int jojaBus = 6;

		public const int junimoBus = 7;

		public const int jojaBoulder = 8;

		public const int junimoBoulder = 9;

		public const int jojaMovieTheater = 10;

		public const int junimoMovieTheater = 11;

		public const int movieTheaterLightning = 12;

		public const int willyBoatRepair = 13;

		public const int treehouseBuild = 14;

		public readonly NetInt whichEvent = new NetInt();

		private int cutsceneLengthTimer;

		private int timerSinceFade;

		private int soundTimer;

		private int soundInterval = 99999;

		private GameLocation location;

		private string sound;

		private bool kill;

		private bool wasRaining;

		public GameLocation preEventLocation;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public WorldChangeEvent()
		{
			NetFields.AddField(whichEvent);
		}

		public WorldChangeEvent(int which)
			: this()
		{
			whichEvent.Value = which;
		}

		private void obliterateJojaMartDoor()
		{
			(Game1.getLocationFromName("Town") as Town).crackOpenAbandonedJojaMartDoor();
			for (int i = 0; i < 16; i++)
			{
				Game1.getLocationFromName("Town").temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(96f, 50f) * 64f + new Vector2(Game1.random.Next(-32, 64), 0f), flipped: false, 0.002f, Color.Gray)
				{
					alpha = 0.75f,
					motion = new Vector2(0f, -0.5f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
					interval = 99999f,
					layerDepth = 0.95f + (float)i * 0.001f,
					scale = 3f,
					scaleChange = 0.01f,
					rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
					delayBeforeAnimationStart = i * 25
				});
			}
			Utility.addDirtPuffs(Game1.getLocationFromName("Town"), 95, 49, 2, 2);
			Game1.getLocationFromName("Town").temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(96f, 50f) * 64f + new Vector2(Game1.random.Next(-32, 64), 0f), flipped: false, 0f, Color.Gray)
			{
				alpha = 0.01f,
				interval = 99999f,
				layerDepth = 0.9f,
				light = true,
				lightRadius = 4f,
				lightcolor = new Color(1, 1, 1)
			});
		}

		public bool setUp()
		{
			preEventLocation = Game1.currentLocation;
			Game1.currentLightSources.Clear();
			location = null;
			int targetXTile = 0;
			int targetYTile = 0;
			cutsceneLengthTimer = 8000;
			wasRaining = Game1.isRaining;
			Game1.isRaining = false;
			Game1.changeMusicTrack("nightTime");
			switch ((int)whichEvent)
			{
			case 13:
				location = Game1.getLocationFromName("BoatTunnel");
				location.resetForPlayerEntry();
				targetXTile = 7;
				targetYTile = 7;
				if (Game1.IsMasterGame)
				{
					Game1.addMailForTomorrow("willyBoatFixed", noLetter: true);
				}
				Game1.mailbox.Add("willyHours");
				location.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Willy", new Rectangle(0, 320, 16, 32), 120f, 3, 999, new Vector2(412f, 332f), flicker: false, flipped: false)
				{
					pingPong = true,
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Robin", new Rectangle(0, 192, 16, 32), 140f, 4, 999, new Vector2(704f, 256f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				soundInterval = 560;
				sound = "crafting";
				break;
			case 12:
			{
				cutsceneLengthTimer += 3000;
				Game1.isRaining = true;
				Game1.changeMusicTrack("rain");
				location = Game1.getLocationFromName("Town");
				location.resetForPlayerEntry();
				targetXTile = 95;
				targetYTile = 48;
				if (Game1.IsMasterGame)
				{
					Game1.addMailForTomorrow("abandonedJojaMartAccessible", noLetter: true);
				}
				Rectangle lightningSourceRect = new Rectangle(644, 1078, 37, 57);
				Vector2 strikePosition = new Vector2(96f, 50f) * 64f;
				Vector2 drawPosition = strikePosition + new Vector2(-lightningSourceRect.Width * 4 / 2, -lightningSourceRect.Height * 4);
				while (drawPosition.Y > (float)(-lightningSourceRect.Height * 4))
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", lightningSourceRect, 9999f, 1, 999, drawPosition, flicker: false, Game1.random.NextDouble() < 0.5, (strikePosition.Y + 32f) / 10000f + 0.001f, 0.025f, Color.White, 4f, 0f, 0f, 0f)
					{
						light = true,
						lightRadius = 2f,
						delayBeforeAnimationStart = 6200,
						lightcolor = Color.Black
					});
					drawPosition.Y -= lightningSourceRect.Height * 4;
				}
				DelayedAction.playSoundAfterDelay("thunder_small", 6000);
				DelayedAction.playSoundAfterDelay("boulderBreak", 6300);
				DelayedAction.screenFlashAfterDelay(1f, 6000);
				DelayedAction.functionAfterDelay(obliterateJojaMartDoor, 6050);
				break;
			}
			case 10:
				location = Game1.getLocationFromName("Town");
				location.resetForPlayerEntry();
				targetXTile = 52;
				targetYTile = 18;
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1349, 19, 28), 150f, 5, 999, new Vector2(3760f, 1056f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 140f, 5, 999, new Vector2(2948f, 1088f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1000f, 2, 999, new Vector2(3144f, 1280f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				Game1.player.activeDialogueEvents.Add("movieTheater", 3);
				soundInterval = 560;
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, LightSource.LightContext.None, 0L));
				sound = "axchop";
				break;
			case 11:
				location = Game1.getLocationFromName("Town");
				location.resetForPlayerEntry();
				targetXTile = 95;
				targetYTile = 48;
				Utility.addSprinklesToLocation(location, targetXTile, targetYTile, 7, 7, 15000, 150, Color.LightCyan);
				Utility.addStarsAndSpirals(location, targetXTile, targetYTile, 7, 7, 15000, 150, Color.White);
				Game1.player.activeDialogueEvents.Add("movieTheater", 3);
				sound = "junimoMeep1";
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6080f, 2880f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				soundInterval = 800;
				break;
			case 0:
				location = Game1.getLocationFromName("Farm");
				location.resetForPlayerEntry();
				targetXTile = 28;
				targetYTile = 13;
				if (Game1.whichFarm == 5)
				{
					targetXTile = 39;
					targetYTile = 32;
				}
				if (location != null && location is Farm)
				{
					foreach (Building b in (location as Farm).buildings)
					{
						if (b is GreenhouseBuilding)
						{
							targetXTile = (int)b.tileX + 3;
							targetYTile = (int)b.tileY + 3;
							break;
						}
					}
				}
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1349, 19, 28), 150f, 5, 999, new Vector2((targetXTile - 3) * 64 + 8, (targetYTile - 1) * 64 - 32), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 140f, 5, 999, new Vector2((targetXTile + 3) * 64 - 16, (targetYTile - 2) * 64), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1000f, 2, 999, new Vector2(targetXTile * 64 + 8, (targetYTile - 4) * 64), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				soundInterval = 560;
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, LightSource.LightContext.None, 0L));
				sound = "axchop";
				break;
			case 1:
				location = Game1.getLocationFromName("Farm");
				location.resetForPlayerEntry();
				targetXTile = 28;
				targetYTile = 13;
				if (Game1.whichFarm == 5)
				{
					targetXTile = 39;
					targetYTile = 32;
				}
				if (location != null && location is Farm)
				{
					foreach (Building b2 in (location as Farm).buildings)
					{
						if (b2 is GreenhouseBuilding)
						{
							targetXTile = (int)b2.tileX + 3;
							targetYTile = (int)b2.tileY + 3;
							break;
						}
					}
				}
				Utility.addSprinklesToLocation(location, targetXTile, targetYTile - 1, 7, 7, 15000, 150, Color.LightCyan);
				Utility.addStarsAndSpirals(location, targetXTile, targetYTile - 1, 7, 7, 15000, 150, Color.White);
				Game1.player.activeDialogueEvents.Add("cc_Greenhouse", 3);
				sound = "junimoMeep1";
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(targetXTile * 64, (targetYTile - 1) * 64 - 64), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				soundInterval = 800;
				break;
			case 6:
				location = Game1.getLocationFromName("BusStop");
				location.resetForPlayerEntry();
				targetXTile = 14;
				targetYTile = 8;
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1349, 19, 28), 150f, 5, 999, new Vector2(1216f, 480f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 140f, 5, 999, new Vector2(640f, 512f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(904f, 192f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				Game1.player.activeDialogueEvents.Add("cc_Bus", 7);
				soundInterval = 560;
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, LightSource.LightContext.None, 0L));
				sound = "clank";
				break;
			case 7:
				location = Game1.getLocationFromName("BusStop");
				location.resetForPlayerEntry();
				targetXTile = 14;
				targetYTile = 8;
				Utility.addSprinklesToLocation(location, targetXTile, targetYTile, 9, 4, 10000, 200, Color.LightCyan, null, motionTowardCenter: true);
				Utility.addStarsAndSpirals(location, targetXTile, targetYTile, 9, 4, 15000, 150, Color.White);
				sound = "junimoMeep1";
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(640f, 640f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(768f, 640f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2300f,
					xPeriodicRange = 16f,
					color = Color.Pink,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(896f, 640f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2200f,
					xPeriodicRange = 16f,
					color = Color.Yellow,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(1024f, 640f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2100f,
					xPeriodicRange = 16f,
					color = Color.LightBlue,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				Game1.player.activeDialogueEvents.Add("cc_Bus", 7);
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
				soundInterval = 500;
				break;
			case 2:
				location = Game1.getLocationFromName("Town");
				location.resetForPlayerEntry();
				targetXTile = 105;
				targetYTile = 79;
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 100f, 5, 999, new Vector2(6656f, 5024f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1406, 22, 26), 700f, 2, 999, new Vector2(6888f, 5014f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(6792f, 4864f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(335, 1410, 21, 21), 999f, 1, 9999, new Vector2(6912f, 5136f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				Game1.player.activeDialogueEvents.Add("cc_Minecart", 7);
				soundInterval = 500;
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, LightSource.LightContext.None, 0L));
				sound = "clank";
				break;
			case 3:
				location = Game1.getLocationFromName("Town");
				location.resetForPlayerEntry();
				targetXTile = 105;
				targetYTile = 79;
				Utility.addSprinklesToLocation(location, targetXTile + 1, targetYTile, 6, 4, 15000, 350, Color.LightCyan);
				Utility.addStarsAndSpirals(location, targetXTile + 1, targetYTile, 6, 4, 15000, 350, Color.White);
				Game1.player.activeDialogueEvents.Add("cc_Minecart", 7);
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6656f, 5056f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6912f, 5056f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2300f,
					xPeriodicRange = 16f,
					color = Color.HotPink,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				sound = "junimoMeep1";
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
				soundInterval = 800;
				break;
			case 4:
				location = Game1.getLocationFromName("Mountain");
				location.resetForPlayerEntry();
				targetXTile = 95;
				targetYTile = 27;
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(383, 1378, 28, 27), 400f, 2, 999, new Vector2(5504f, 1632f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					motion = new Vector2(0.5f, 0f)
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1406, 22, 26), 350f, 2, 999, new Vector2(6272f, 1632f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(358, 1415, 31, 20), 999f, 1, 9999, new Vector2(5888f, 1648f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(335, 1410, 21, 21), 999f, 1, 9999, new Vector2(6400f, 1648f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(5824f, 1584f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 0.8f
				});
				Game1.player.activeDialogueEvents.Add("cc_Bridge", 7);
				soundInterval = 700;
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, LightSource.LightContext.None, 0L));
				sound = "axchop";
				break;
			case 5:
				location = Game1.getLocationFromName("Mountain");
				location.resetForPlayerEntry();
				targetXTile = 95;
				targetYTile = 27;
				Utility.addSprinklesToLocation(location, targetXTile, targetYTile, 7, 4, 15000, 150, Color.LightCyan);
				Utility.addStarsAndSpirals(location, targetXTile + 1, targetYTile, 7, 4, 15000, 350, Color.White);
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(5824f, 1648f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6336f, 1648f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2300f,
					xPeriodicRange = 16f,
					color = Color.Yellow,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				Game1.player.activeDialogueEvents.Add("cc_Bridge", 7);
				sound = "junimoMeep1";
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
				soundInterval = 800;
				break;
			case 8:
				location = Game1.getLocationFromName("Mountain");
				location.resetForPlayerEntry();
				targetXTile = 48;
				targetYTile = 5;
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 100f, 5, 999, new Vector2(2880f, 288f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(387, 1340, 17, 37), 50f, 2, 99999, new Vector2(3040f, 160f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					yPeriodic = true,
					yPeriodicLoopTime = 100f,
					yPeriodicRange = 2f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(335, 1410, 21, 21), 999f, 1, 9999, new Vector2(2816f, 368f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(3200f, 368f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				Game1.player.activeDialogueEvents.Add("cc_Boulder", 7);
				soundInterval = 100;
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, LightSource.LightContext.None, 0L));
				sound = "thudStep";
				break;
			case 9:
				location = Game1.getLocationFromName("Mountain");
				location.resetForPlayerEntry();
				Game1.player.activeDialogueEvents.Add("cc_Boulder", 7);
				targetXTile = 48;
				targetYTile = 5;
				Utility.addSprinklesToLocation(location, targetXTile, targetYTile, 4, 4, 15000, 350, Color.LightCyan);
				Utility.addStarsAndSpirals(location, targetXTile + 1, targetYTile, 4, 4, 15000, 550, Color.White);
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(2880f, 368f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(3200f, 368f), flicker: false, flipped: false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2300f,
					xPeriodicRange = 16f,
					color = Color.Yellow,
					light = true,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				sound = "junimoMeep1";
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 1f, Color.DarkCyan, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
				soundInterval = 1000;
				break;
			case 14:
			{
				location = Game1.getLocationFromName("Mountain");
				location.resetForPlayerEntry();
				targetXTile = 16;
				targetYTile = 7;
				cutsceneLengthTimer = 12000;
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetXTile, targetYTile) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(0, 0, 24, 24), new Vector2(14f, 4.5f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 777f,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 9999f,
					animationLength = 1,
					layerDepth = 1f,
					drawAboveAlwaysFront = true
				});
				DelayedAction.functionAfterDelay(ParrotSquawk, 2000);
				for (int j = 0; j < 16; j++)
				{
					Rectangle rect = new Rectangle(15, 5, 3, 3);
					TemporaryAnimatedSprite t2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(49 + 16 * Game1.random.Next(3), 229, 16, 6), Utility.getRandomPositionInThisRectangle(rect, Game1.random) * 64f, Game1.random.NextDouble() < 0.5, 0f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-2, 3), -16f),
						acceleration = new Vector2(0f, 0.5f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 1000 + Game1.random.Next(500),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						yStopCoordinate = (rect.Bottom + 1) * 64,
						delayBeforeAnimationStart = 4000 + j * 250
					};
					t2.reachedStopCoordinate = t2.bounce;
					location.TemporarySprites.Add(t2);
					t2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(49 + 16 * Game1.random.Next(3), 229, 16, 6), Utility.getRandomPositionInThisRectangle(rect, Game1.random) * 64f, Game1.random.NextDouble() < 0.5, 0f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-2, 3), -16f),
						acceleration = new Vector2(0f, 0.5f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 1000 + Game1.random.Next(500),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						delayBeforeAnimationStart = 4500 + j * 250,
						yStopCoordinate = (rect.Bottom + 1) * 64
					};
					t2.reachedStopCoordinate = t2.bounce;
					location.TemporarySprites.Add(t2);
				}
				for (int i = 0; i < 20; i++)
				{
					Vector2 start_point = new Vector2(Utility.RandomFloat(13f, 19f), 0f) * 64f;
					float x_offset = 1024f - start_point.X;
					TemporaryAnimatedSprite parrot = new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(48 + Game1.random.Next(2) * 72, Game1.random.Next(2) * 48, 24, 24), start_point, flipped: false, 0f, Color.White)
					{
						motion = new Vector2(x_offset * 0.01f, 10f),
						acceleration = new Vector2(0f, -0.05f),
						id = 778f,
						scale = 4f,
						yStopCoordinate = 448,
						totalNumberOfLoops = 99999,
						interval = 50f,
						animationLength = 3,
						flipped = (x_offset > 0f),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						delayBeforeAnimationStart = 3500 + i * 250,
						alpha = 0f,
						alphaFade = -0.1f
					};
					DelayedAction.playSoundAfterDelay("batFlap", 3500 + i * 250);
					parrot.reachedStopCoordinateSprite = ParrotBounce;
					location.temporarySprites.Add(parrot);
				}
				DelayedAction.functionAfterDelay(FinishTreehouse, 8000);
				DelayedAction.functionAfterDelay(ParrotSquawk, 9000);
				DelayedAction.functionAfterDelay(ParrotFlyAway, 11000);
				break;
			}
			}
			soundTimer = soundInterval;
			Game1.currentLocation = location;
			Game1.fadeClear();
			Game1.nonWarpFade = true;
			Game1.timeOfDay = 2400;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.player.position.X = -999999f;
			Game1.viewport.X = Math.Max(0, Math.Min(location.map.DisplayWidth - Game1.viewport.Width, targetXTile * 64 - Game1.viewport.Width / 2));
			Game1.viewport.Y = Math.Max(0, Math.Min(location.map.DisplayHeight - Game1.viewport.Height, targetYTile * 64 - Game1.viewport.Height / 2));
			if (!location.IsOutdoors)
			{
				Game1.viewport.X = targetXTile * 64 - Game1.viewport.Width / 2;
				Game1.viewport.Y = targetYTile * 64 - Game1.viewport.Height / 2;
			}
			Game1.previousViewportPosition = new Vector2(Game1.viewport.X, Game1.viewport.Y);
			if (Game1.debrisWeather != null && Game1.debrisWeather.Count > 0)
			{
				Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
			}
			Game1.randomizeRainPositions();
			return false;
		}

		public virtual void ParrotFlyAway()
		{
			location.removeTemporarySpritesWithIDLocal(777f);
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(48, 0, 24, 24), new Vector2(14f, 4.5f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 777f,
				scale = 4f,
				totalNumberOfLoops = 99999,
				layerDepth = 1f,
				drawAboveAlwaysFront = true,
				interval = 50f,
				animationLength = 3,
				motion = new Vector2(-2f, 0f),
				acceleration = new Vector2(0f, -0.1f)
			});
		}

		public virtual void ParrotSquawk()
		{
			TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(777);
			temporarySpriteByID.shakeIntensity = 1f;
			temporarySpriteByID.sourceRectStartingPos.X = 24f;
			temporarySpriteByID.sourceRect.X = 24;
			Game1.playSound("parrot");
			DelayedAction.functionAfterDelay(ParrotStopSquawk, 500);
		}

		public virtual void ParrotStopSquawk()
		{
			TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(777);
			temporarySpriteByID.shakeIntensity = 0f;
			temporarySpriteByID.sourceRectStartingPos.X = 0f;
			temporarySpriteByID.sourceRect.X = 0;
		}

		public virtual void FinishTreehouse()
		{
			Game1.flashAlpha = 1f;
			Game1.playSound("yoba");
			Game1.playSound("axchop");
			(location as Mountain).ApplyTreehouseIfNecessary();
			location.removeTemporarySpritesWithIDLocal(778f);
			for (int i = 0; i < 20; i++)
			{
				Vector2 start_point = new Vector2(Utility.RandomFloat(13f, 19f), Utility.RandomFloat(4f, 7f)) * 64f;
				float x_offset = 1024f - start_point.X;
				TemporaryAnimatedSprite parrot = new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(192, Game1.random.Next(2) * 48, 24, 24), start_point, flipped: false, 0f, Color.White)
				{
					motion = new Vector2(x_offset * -0.01f, Utility.RandomFloat(-2f, 0f)),
					acceleration = new Vector2(0f, -0.05f),
					id = 778f,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 50f,
					animationLength = 3,
					flipped = (x_offset > 0f),
					layerDepth = 1f,
					drawAboveAlwaysFront = true
				};
				location.TemporarySprites.Add(parrot);
			}
		}

		public void ParrotBounce(TemporaryAnimatedSprite sprite)
		{
			float x_offset = 1024f - sprite.Position.X;
			sprite.motion.X = (float)Math.Sign(x_offset) * Utility.RandomFloat(0.5f, 4f);
			sprite.motion.Y = Utility.RandomFloat(-15f, -10f);
			sprite.acceleration.Y = 0.5f;
			sprite.yStopCoordinate = 448;
			sprite.flipped = (x_offset > 0f);
			sprite.sourceRectStartingPos.X = 48 + Game1.random.Next(2) * 72;
			if (Game1.random.NextDouble() < 0.05000000074505806)
			{
				Game1.playSound("axe");
			}
			else if (Game1.random.NextDouble() < 0.05000000074505806)
			{
				Game1.playSound("crafting");
			}
			else
			{
				Game1.playSound("dirtyHit");
			}
		}

		public bool tickUpdate(GameTime time)
		{
			Game1.UpdateGameClock(time);
			location.updateWater(time);
			cutsceneLengthTimer -= time.ElapsedGameTime.Milliseconds;
			if (timerSinceFade > 0)
			{
				timerSinceFade -= time.ElapsedGameTime.Milliseconds;
				Game1.globalFade = true;
				Game1.fadeToBlackAlpha = 1f;
				if (timerSinceFade <= 0)
				{
					return true;
				}
				return false;
			}
			if (cutsceneLengthTimer <= 0 && !Game1.globalFade)
			{
				Game1.globalFadeToBlack(endEvent, 0.01f);
			}
			soundTimer -= time.ElapsedGameTime.Milliseconds;
			if (soundTimer <= 0 && sound != null)
			{
				Game1.playSound(sound);
				soundTimer = soundInterval;
			}
			return false;
		}

		public void endEvent()
		{
			if (preEventLocation != null)
			{
				Game1.currentLocation = preEventLocation;
				Game1.currentLocation.resetForPlayerEntry();
				preEventLocation = null;
			}
			Game1.changeMusicTrack("none");
			timerSinceFade = 1500;
			Game1.isRaining = wasRaining;
			Game1.getFarm().temporarySprites.Clear();
		}

		public void draw(SpriteBatch b)
		{
		}

		public void makeChangesToLocation()
		{
		}

		public void drawAboveEverything(SpriteBatch b)
		{
		}
	}
}
