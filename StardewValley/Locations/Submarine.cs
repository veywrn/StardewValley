using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class Submarine : GameLocation
	{
		public const float submergeTime = 20000f;

		[XmlElement("submerged")]
		public readonly NetBool submerged = new NetBool();

		[XmlElement("ascending")]
		public readonly NetBool ascending = new NetBool();

		private Texture2D submarineSprites;

		private float curtainMovement;

		private float curtainOpenPercent;

		private float submergeTimer;

		private Color ambientLightTargetColor;

		private bool hasLitSubmergeLight;

		private bool hasLitAscendLight;

		private bool doneUntilReset;

		private bool localAscending;

		public Submarine()
		{
		}

		public Submarine(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(submerged, ascending);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			b.Draw(submarineSprites, Game1.GlobalToLocal(new Vector2(9f, 7f) * 64f) + new Vector2(0f, -2f) * 4f, new Microsoft.Xna.Framework.Rectangle((int)(257f + 100f * curtainOpenPercent), 0, (int)(100f * (1f - curtainOpenPercent)), 80), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(submarineSprites, Game1.GlobalToLocal(new Vector2(15f, 7f) * 64f + new Vector2(-3f, -2f) * 4f + new Vector2(100f * curtainOpenPercent, 0f) * 4f), new Microsoft.Xna.Framework.Rectangle(357, 0, (int)(100f * (1f - curtainOpenPercent)), 80), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(submarineSprites, Game1.GlobalToLocal(new Vector2(82f, 123f) * 4f + new Vector2(0f, (submerged.Value && !doneUntilReset) ? (104f * (1f - submergeTimer / 20000f)) : 0f)), new Microsoft.Xna.Framework.Rectangle(457, 0, 9, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			hasLitSubmergeLight = false;
			curtainOpenPercent = 0f;
			curtainMovement = 0f;
			submergeTimer = 0f;
			submerged.Value = false;
			hasLitAscendLight = false;
			doneUntilReset = false;
			if ((bool)submerged)
			{
				submerged.Value = false;
			}
			if ((bool)ascending)
			{
				ascending.Value = false;
			}
			Game1.netWorldState.Value.IsSubmarineLocked = false;
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				int tileIndex = map.GetLayer("Buildings").Tiles[tileLocation].TileIndex;
				if (tileIndex == 217)
				{
					if (doneUntilReset)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Submarine_Done"));
						return false;
					}
					if (!submerged.Value)
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Submarine_SubmergeQuestion"), createYesNoResponses(), "SubmergeQuestion");
					}
					else if (submerged.Value && submergeTimer <= 0f && curtainOpenPercent >= 1f)
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Submarine_AscendQuestion"), createYesNoResponses(), "AscendQuestion");
					}
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "SubmergeQuestion_Yes"))
			{
				if (questionAndAnswer == "AscendQuestion_Yes")
				{
					ascending.Value = true;
					localAscending = true;
				}
			}
			else if (Game1.player.Money < 1000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
			}
			else
			{
				Game1.player.Money -= 1000;
				submerged.Value = true;
				Game1.netWorldState.Value.IsSubmarineLocked = true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		private void changeSubmergeLight(bool red, bool clear = false)
		{
			if (clear)
			{
				setMapTileIndex(3, 4, 98, "Buildings");
				setMapTileIndex(4, 4, 99, "Buildings");
				setMapTileIndex(3, 5, 122, "Buildings");
				setMapTileIndex(4, 5, 123, "Buildings");
			}
			else if (red)
			{
				setMapTileIndex(3, 4, 425, "Buildings");
				setMapTileIndex(4, 4, 426, "Buildings");
				setMapTileIndex(3, 5, 449, "Buildings");
				setMapTileIndex(4, 5, 450, "Buildings");
			}
			else
			{
				setMapTileIndex(3, 4, 427, "Buildings");
				setMapTileIndex(4, 4, 428, "Buildings");
				setMapTileIndex(3, 5, 451, "Buildings");
				setMapTileIndex(4, 5, 452, "Buildings");
			}
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			submerged.Value = false;
			ascending.Value = false;
			Game1.netWorldState.Value.IsSubmarineLocked = false;
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			submarineSprites = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
			Game1.ambientLight = Color.Black;
			ambientLightTargetColor = Color.Black;
			hasLitSubmergeLight = false;
			Game1.background = new Background(new Color(0, 50, 255), onlyMapBG: true);
			curtainOpenPercent = 0f;
			curtainMovement = 0f;
			submergeTimer = 0f;
			hasLitAscendLight = false;
			doneUntilReset = false;
			localAscending = false;
		}

		public override bool canFishHere()
		{
			return curtainOpenPercent >= 1f;
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			Random r = new Random(timeOfDay + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
			if (fishSplashPoint.Value.Equals(Point.Zero) && r.NextDouble() < 1.0 && curtainOpenPercent >= 1f)
			{
				int tries = 0;
				Point p;
				while (true)
				{
					if (tries >= 2)
					{
						return;
					}
					p = new Point(r.Next(9, 21), r.Next(7, 12));
					if (isOpenWater(p.X, p.Y))
					{
						int toLand = FishingRod.distanceToLand(p.X, p.Y, this);
						if (toLand > 1 && toLand < 5)
						{
							break;
						}
					}
					tries++;
				}
				if (Game1.player.currentLocation.Equals(this))
				{
					playSound("waterSlosh");
				}
				fishSplashPoint.Value = p;
			}
			else if (!fishSplashPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.25)
			{
				fishSplashPoint.Value = Point.Zero;
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (!Game1.player.currentLocation.Equals(this) || !Game1.shouldTimePass())
			{
				return;
			}
			if (curtainMovement != 0f)
			{
				float old = curtainOpenPercent;
				curtainOpenPercent = Math.Max(0f, Math.Min(1f, curtainOpenPercent + curtainMovement * (float)time.ElapsedGameTime.Milliseconds));
				if (curtainOpenPercent >= 1f && old < 1f)
				{
					curtainMovement = 0f;
					changeSubmergeLight(red: false);
					ambientLightTargetColor = new Color(200, 150, 100);
					Game1.soundBank.PlayCue("newArtifact");
					Game1.changeMusicTrack("submarine_song");
				}
			}
			if (submerged.Value && !hasLitSubmergeLight)
			{
				changeSubmergeLight(red: true);
				DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 200);
				DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 400);
				Game1.changeMusicTrack("Hospital_Ambient");
				submergeTimer = 20000f;
				hasLitSubmergeLight = true;
				ignoreWarps = true;
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = submarineSprites,
					sourceRectStartingPos = new Vector2(457f, 11f),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(457, 11, 14, 18),
					initialPosition = new Vector2(21f, 143f) * 4f,
					animationLength = 3,
					pingPong = true,
					position = new Vector2(21f, 143f) * 4f,
					scale = 4f
				});
			}
			if (ascending.Value && !hasLitAscendLight)
			{
				changeSubmergeLight(red: true);
				DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 200);
				DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 400);
				Game1.changeMusicTrack("Hospital_Ambient");
				submergeTimer = 1f;
				hasLitAscendLight = true;
				curtainMovement = -0.0002f;
				Game1.soundBank.PlayCue("submarine_landing");
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = submarineSprites,
					sourceRectStartingPos = new Vector2(457f, 11f),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(457, 11, 14, 18),
					initialPosition = new Vector2(21f, 143f) * 4f,
					animationLength = 3,
					pingPong = true,
					position = new Vector2(21f, 143f) * 4f,
					scale = 4f
				});
				if (Game1.IsMasterGame)
				{
					fishSplashPoint.Value = Point.Zero;
				}
				if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is BobberBar)
				{
					Game1.activeClickableMenu.emergencyShutDown();
				}
				if (Game1.player.UsingTool && Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod)
				{
					(Game1.player.CurrentTool as FishingRod).doneFishing(Game1.player);
				}
				Game1.player.completelyStopAnimatingOrDoingAction();
				foreach (TemporaryAnimatedSprite tempSprite in Game1.background.tempSprites)
				{
					tempSprite.yStopCoordinate = ((tempSprite.position.X > 320f) ? 320 : 896);
					tempSprite.motion = new Vector2(0f, 2f);
					tempSprite.yPeriodic = false;
				}
			}
			if (submergeTimer > 0f)
			{
				if ((bool)ascending && !localAscending)
				{
					localAscending = true;
				}
				submergeTimer -= ((!localAscending) ? 1 : (-1)) * time.ElapsedGameTime.Milliseconds;
				Game1.background.c.B = (byte)(Math.Max(submergeTimer / 20000f, 0.2f) * 255f);
				Game1.background.c.G = (byte)(Math.Max(submergeTimer / 20000f, 0f) * 50f);
				if (submergeTimer <= 0f)
				{
					curtainMovement = 0.0002f;
					Game1.changeMusicTrack("none");
					Game1.soundBank.PlayCue("submarine_landing");
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(0f, -1f),
						yStopCoordinate = 120,
						texture = submarineSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(257, 98, 182, 25),
						animationLength = 1,
						interval = 999999f,
						position = new Vector2(148f, 56f) * 4f,
						scale = 4f
					});
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(0f, -1f),
						yStopCoordinate = 460,
						texture = submarineSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(441, 86, 66, 37),
						animationLength = 1,
						interval = 999999f,
						position = new Vector2(18f, 149f) * 4f,
						scale = 4f
					});
				}
				else
				{
					ambientLightTargetColor = new Color((byte)(250f - submergeTimer / 20000f * 250f), (byte)(200f - submergeTimer / 20000f * 200f), (byte)(150f - submergeTimer / 20000f * 150f));
					if (Game1.random.NextDouble() < 0.11)
					{
						Vector2 pos5 = new Vector2(Game1.random.Next(12, map.DisplayWidth - 64), ascending.Value ? 1 : 640);
						int which3 = Game1.random.Next(3);
						Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
						{
							motion = new Vector2(0f, (float)((!ascending.Value) ? 1 : (-1)) * (-3f + (float)which3)),
							yStopCoordinate = ((!ascending.Value) ? 1 : 832),
							texture = submarineSprites,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(132 + which3 * 8, 20, 8, 8),
							xPeriodic = true,
							xPeriodicLoopTime = 1500f,
							xPeriodicRange = 12f,
							initialPosition = pos5,
							animationLength = 1,
							interval = 5000f,
							position = pos5,
							scale = 4f
						});
					}
				}
				if (submergeTimer >= 20000f)
				{
					Game1.changeMusicTrack("night_market");
					ignoreWarps = false;
					changeSubmergeLight(red: true, clear: true);
					Game1.soundBank.PlayCue("pullItemFromWater");
					Game1.ambientLight = Color.Black;
					ambientLightTargetColor = Color.Black;
					hasLitSubmergeLight = false;
					Game1.background = new Background(new Color(0, 50, 255), onlyMapBG: true);
					curtainOpenPercent = 0f;
					curtainMovement = 0f;
					submergeTimer = 0f;
					submerged.Value = false;
					ascending.Value = false;
					Game1.netWorldState.Value.IsSubmarineLocked = false;
					hasLitAscendLight = false;
					doneUntilReset = false;
					localAscending = false;
				}
			}
			else if (submerged.Value && !doneUntilReset)
			{
				if (Game1.random.NextDouble() < 0.01)
				{
					Vector2 pos4 = new Vector2(Game1.random.Next(384, map.DisplayWidth - 64), 320f);
					int which2 = Game1.random.Next(3);
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(0f, -1f + (float)which2 * 0.2f),
						yStopCoordinate = 1,
						texture = submarineSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(132 + which2 * 8, 20, 8, 8),
						animationLength = 1,
						interval = 20000f,
						xPeriodic = true,
						xPeriodicLoopTime = 1500f,
						xPeriodicRange = 12f,
						initialPosition = pos4,
						position = pos4,
						scale = 4f
					});
				}
				if (Game1.random.NextDouble() < 0.001)
				{
					Vector2 pos3 = new Vector2(1344f, Game1.random.Next(448, 704));
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(-0.5f, 0f),
						xStopCoordinate = 448,
						texture = submarineSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(3, 194, 16, 16),
						animationLength = 1,
						interval = 50000f,
						alpha = 0.5f,
						yPeriodic = true,
						yPeriodicLoopTime = 5500f,
						yPeriodicRange = 32f,
						initialPosition = pos3,
						position = pos3,
						scale = 4f
					});
				}
				if (Game1.random.NextDouble() < 0.001)
				{
					Game1.background.tempSprites.Insert(0, new TemporaryAnimatedSprite
					{
						texture = submarineSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 146, 16, 13),
						animationLength = 9,
						interval = 100f,
						position = new Vector2(Game1.random.Next(96, 381) * 4, Game1.random.Next(24, 66) * 4),
						scale = 4f
					});
				}
				if (Game1.random.NextDouble() < 5E-05)
				{
					Vector2 pos2 = new Vector2(3f, 10f) * 64f;
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(-0f, -1f),
						color = new Color(0, 50, 150),
						yStopCoordinate = 64,
						texture = submarineSprites,
						sourceRectStartingPos = new Vector2(67f, 189f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
						totalNumberOfLoops = 50,
						animationLength = 3,
						pingPong = true,
						interval = 192f,
						xPeriodic = true,
						xPeriodicLoopTime = 3500f,
						xPeriodicRange = 12f,
						initialPosition = pos2,
						position = pos2,
						scale = 4f
					});
				}
				if (Game1.random.NextDouble() < 0.00035)
				{
					Vector2 pos = new Vector2(24f, 2f) * 64f;
					int which = Game1.random.Next(3);
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(-0.5f, 0f),
						xStopCoordinate = 64,
						texture = submarineSprites,
						sourceRectStartingPos = new Vector2(257 + which * 48, 81f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(257 + which * 48, 81, 16, 16),
						totalNumberOfLoops = 250,
						animationLength = 3,
						interval = 200f,
						pingPong = true,
						yPeriodic = true,
						yPeriodicLoopTime = 3500f,
						yPeriodicRange = 12f,
						initialPosition = pos,
						position = pos,
						scale = 4f
					});
				}
			}
			if (!Game1.ambientLight.Equals(ambientLightTargetColor))
			{
				if (Game1.ambientLight.R < ambientLightTargetColor.R)
				{
					Game1.ambientLight.R++;
				}
				else if (Game1.ambientLight.R > ambientLightTargetColor.R)
				{
					Game1.ambientLight.R--;
				}
				if (Game1.ambientLight.G < ambientLightTargetColor.G)
				{
					Game1.ambientLight.G++;
				}
				else if (Game1.ambientLight.G > ambientLightTargetColor.G)
				{
					Game1.ambientLight.G--;
				}
				if (Game1.ambientLight.B < ambientLightTargetColor.B)
				{
					Game1.ambientLight.B++;
				}
				else if (Game1.ambientLight.B > ambientLightTargetColor.B)
				{
					Game1.ambientLight.B--;
				}
			}
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			bool bobberAddition = false;
			if (who != null && who.CurrentTool is FishingRod && (who.CurrentTool as FishingRod).getBobberAttachmentIndex() == 856)
			{
				bobberAddition = true;
			}
			if (Game1.random.NextDouble() < 0.1 + (bobberAddition ? 0.1 : 0.0))
			{
				return new Object(800, 1);
			}
			if (Game1.random.NextDouble() < 0.18 + (bobberAddition ? 0.05 : 0.0))
			{
				return new Object(799, 1);
			}
			if (Game1.random.NextDouble() < 0.28)
			{
				return new Object(798, 1);
			}
			if (Game1.random.NextDouble() < 0.1)
			{
				return new Object(154, 1);
			}
			if (Game1.random.NextDouble() < 0.08 + (bobberAddition ? 0.1 : 0.0))
			{
				return new Object(155, 1);
			}
			if (Game1.random.NextDouble() < 0.05)
			{
				return new Object(149, 1);
			}
			if (Game1.random.NextDouble() < 0.01 + (bobberAddition ? 0.02 : 0.0))
			{
				return new Object(797, 1);
			}
			return new Object(152, 1);
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			Game1.background = null;
		}
	}
}
