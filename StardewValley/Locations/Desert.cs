using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class Desert : GameLocation
	{
		public const int busDefaultXTile = 17;

		public const int busDefaultYTile = 24;

		private TemporaryAnimatedSprite busDoor;

		private Vector2 busPosition;

		private Vector2 busMotion;

		public bool drivingOff;

		public bool drivingBack;

		public bool leaving;

		private int chimneyTimer = 500;

		private Microsoft.Xna.Framework.Rectangle desertMerchantBounds = new Microsoft.Xna.Framework.Rectangle(2112, 1280, 836, 280);

		public static bool warpedToDesert;

		public static bool boughtMagicRockCandy;

		private Microsoft.Xna.Framework.Rectangle busSource = new Microsoft.Xna.Framework.Rectangle(288, 1247, 128, 64);

		private Microsoft.Xna.Framework.Rectangle pamSource = new Microsoft.Xna.Framework.Rectangle(384, 1311, 15, 19);

		private Microsoft.Xna.Framework.Rectangle transparentWindowSource = new Microsoft.Xna.Framework.Rectangle(0, 0, 21, 41);

		private Vector2 pamOffset = new Vector2(0f, 29f);

		public Desert()
		{
		}

		public Desert(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		public static Dictionary<ISalable, int[]> getDesertMerchantTradeStock(Farmer who)
		{
			Dictionary<ISalable, int[]> tradeStock = new Dictionary<ISalable, int[]>();
			Item i16 = new Object(275, 1);
			tradeStock.Add(i16, new int[4]
			{
				0,
				2147483647,
				749,
				5
			});
			i16 = new Object(261, 1);
			tradeStock.Add(i16, new int[4]
			{
				0,
				2147483647,
				749,
				3
			});
			i16 = new Object(253, 1);
			tradeStock.Add(i16, new int[4]
			{
				0,
				2147483647,
				72,
				1
			});
			i16 = new Object(226, 1);
			tradeStock.Add(i16, new int[4]
			{
				0,
				2147483647,
				64,
				1
			});
			i16 = new Object(288, 1);
			tradeStock.Add(i16, new int[4]
			{
				0,
				2147483647,
				386,
				5
			});
			i16 = new Object(287, 1);
			tradeStock.Add(i16, new int[4]
			{
				0,
				2147483647,
				80,
				5
			});
			if (Game1.dayOfMonth % 7 == 0)
			{
				i16 = new Object(Vector2.Zero, 71);
				tradeStock.Add(i16, new int[4]
				{
					0,
					2147483647,
					70,
					1
				});
			}
			if (Game1.dayOfMonth % 7 == 1)
			{
				i16 = new Object(178, 3);
				tradeStock.Add(i16, new int[4]
				{
					0,
					2147483647,
					749,
					1
				});
			}
			if (Game1.dayOfMonth % 7 == 2)
			{
				i16 = new Object(771, 1);
				tradeStock.Add(i16, new int[4]
				{
					0,
					2147483647,
					390,
					5
				});
			}
			if (Game1.dayOfMonth % 7 == 3)
			{
				i16 = new Object(428, 1);
				tradeStock.Add(i16, new int[4]
				{
					0,
					2147483647,
					62,
					3
				});
			}
			if (Game1.dayOfMonth % 7 == 4 && !boughtMagicRockCandy)
			{
				i16 = new Object(279, 1);
				tradeStock.Add(i16, new int[4]
				{
					0,
					1,
					74,
					3
				});
			}
			if (Game1.dayOfMonth % 7 == 5)
			{
				i16 = new Object(424, 1);
				tradeStock.Add(i16, new int[4]
				{
					0,
					2147483647,
					60,
					1
				});
			}
			if (Game1.dayOfMonth % 7 == 6)
			{
				i16 = new Object(495, 1);
				tradeStock.Add(i16, new int[4]
				{
					0,
					2147483647,
					496,
					2
				});
				i16 = new Object(496, 1);
				tradeStock.Add(i16, new int[4]
				{
					0,
					2147483647,
					497,
					2
				});
				i16 = new Object(497, 1);
				tradeStock.Add(i16, new int[4]
				{
					0,
					2147483647,
					498,
					2
				});
				i16 = new Object(498, 1);
				tradeStock.Add(i16, new int[4]
				{
					0,
					2147483647,
					495,
					2
				});
			}
			if (who != null && !who.craftingRecipes.ContainsKey("Warp Totem: Desert"))
			{
				i16 = new Object(261, 1, isRecipe: true);
				tradeStock.Add(i16, new int[4]
				{
					0,
					1,
					337,
					10
				});
			}
			if (who != null && who.getFriendshipHeartLevelForNPC("Krobus") >= 10 && (int)who.houseUpgradeLevel >= 1 && !who.isMarried() && !who.isEngaged() && !who.hasItemInInventory(808, 1))
			{
				i16 = new Object(808, 1);
				tradeStock.Add(i16, new int[4]
				{
					0,
					1,
					769,
					200
				});
			}
			i16 = new Furniture(1971, Vector2.Zero);
			tradeStock.Add(i16, new int[4]
			{
				0,
				2147483647,
				767,
				200
			});
			i16 = new Hat(72);
			tradeStock.Add(i16, new int[4]
			{
				0,
				2147483647,
				749,
				50
			});
			i16 = new Hat(73);
			if (Game1.stats.DaysPlayed % 2u == 0)
			{
				i16 = new Hat(74);
			}
			tradeStock.Add(i16, new int[4]
			{
				0,
				2147483647,
				749,
				333
			});
			return tradeStock;
		}

		public bool boughtTraderItem(ISalable s, Farmer f, int i)
		{
			if (s.Name == "Magic Rock Candy")
			{
				boughtMagicRockCandy = true;
			}
			return false;
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				_ = map.GetLayer("Buildings").Tiles[tileLocation].TileIndex;
				return base.checkAction(tileLocation, viewport, who);
			}
			if ((tileLocation.X == 41 || tileLocation.X == 42) && tileLocation.Y == 24)
			{
				if (isTravelingDeserteMerchantHere())
				{
					Game1.activeClickableMenu = new ShopMenu(getDesertMerchantTradeStock(Game1.player), 0, "DesertTrade", boughtTraderItem);
					return true;
				}
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Desert_Trader_Closed"));
				return true;
			}
			if (tileLocation.X >= 34 && tileLocation.X <= 38 && tileLocation.Y == 24)
			{
				Game1.soundBank.PlayCue("camel");
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(208, 591, 65, 49),
					sourceRectStartingPos = new Vector2(208f, 591f),
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = 200f,
					scale = 4f,
					position = new Vector2(536f, 340f) * 4f,
					layerDepth = 0.1332f,
					id = 999f
				});
				Game1.player.faceDirection(0);
				Game1.haltAfterCheck = false;
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (who.secretNotesSeen.Contains(18) && xLocation == 40 && yLocation == 55 && !who.mailReceived.Contains("SecretNote18_done"))
			{
				who.mailReceived.Add("SecretNote18_done");
				Game1.createObjectDebris(127, xLocation, yLocation, who.UniqueMultiplayerID, this);
				return "";
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		private void playerReachedBusDoor(Character c, GameLocation l)
		{
			Game1.player.position.X = -10000f;
			Game1.freezeControls = true;
			Game1.player.CanMove = false;
			busDriveOff();
			playSound("stoneStep");
		}

		public override bool answerDialogue(Response answer)
		{
			if (lastQuestionKey != null && afterQuestion == null)
			{
				string questionAndAnswer = lastQuestionKey.Split(' ')[0] + "_" + answer.responseKey;
				if (questionAndAnswer == "DesertBus_Yes")
				{
					playerReachedBusDoor(Game1.player, this);
					return true;
				}
			}
			return base.answerDialogue(answer);
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			leaving = false;
			Game1.ambientLight = Color.White;
			if (Game1.player.getTileX() == 35 && Game1.player.getTileY() == 43)
			{
				warpedToDesert = true;
			}
			if (Game1.player.getTileY() > 40 || Game1.player.getTileY() < 10)
			{
				drivingOff = false;
				drivingBack = false;
				busMotion = Vector2.Zero;
				busPosition = new Vector2(17f, 24f) * 64f;
				busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), busPosition + new Vector2(16f, 26f) * 4f, flipped: false, 0f, Color.White)
				{
					interval = 999999f,
					animationLength = 6,
					holdLastFrame = true,
					layerDepth = (busPosition.Y + 192f) / 10000f + 1E-05f,
					scale = 4f
				};
				Game1.changeMusicTrack("wavy");
			}
			else
			{
				if (Game1.isRaining)
				{
					Game1.changeMusicTrack("none");
				}
				busPosition = new Vector2(17f, 24f) * 64f;
				busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 1311, 16, 38), busPosition + new Vector2(16f, 26f) * 4f, flipped: false, 0f, Color.White)
				{
					interval = 999999f,
					animationLength = 1,
					holdLastFrame = true,
					layerDepth = (busPosition.Y + 192f) / 10000f + 1E-05f,
					scale = 4f
				};
				Game1.displayFarmer = false;
				busDriveBack();
			}
			temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
				sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 513, 208, 101),
				sourceRectStartingPos = new Vector2(0f, 513f),
				animationLength = 1,
				totalNumberOfLoops = 9999,
				interval = 99999f,
				scale = 4f,
				position = new Vector2(528f, 298f) * 4f,
				layerDepth = 0.1324f,
				id = 996f
			});
			if (isTravelingDeserteMerchantHere())
			{
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 614, 20, 26),
					sourceRectStartingPos = new Vector2(0f, 614f),
					animationLength = 1,
					totalNumberOfLoops = 999,
					interval = 99999f,
					scale = 4f,
					position = new Vector2(663f, 354f) * 4f,
					layerDepth = 0.1328f,
					id = 995f
				});
			}
			if (Game1.timeOfDay >= Game1.getModeratelyDarkTime())
			{
				lightMerchantLamps();
			}
		}

		private bool isTravelingDeserteMerchantHere()
		{
			if (!(Game1.currentSeason != "winter") && Game1.dayOfMonth >= 15)
			{
				return Game1.dayOfMonth > 17;
			}
			return true;
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			if (position.Intersects(desertMerchantBounds))
			{
				return true;
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (Game1.currentLocation != this)
			{
				return;
			}
			if (isTravelingDeserteMerchantHere())
			{
				if (Game1.random.NextDouble() < 0.33)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(40, 614, 20, 26),
						sourceRectStartingPos = new Vector2(40f, 614f),
						animationLength = 6,
						totalNumberOfLoops = 1,
						interval = 100f,
						scale = 4f,
						position = new Vector2(663f, 354f) * 4f,
						layerDepth = 0.1336f,
						id = 997f,
						pingPong = true
					});
				}
				else
				{
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(20, 614, 20, 26),
						sourceRectStartingPos = new Vector2(20f, 614f),
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = Game1.random.Next(100, 800),
						scale = 4f,
						position = new Vector2(663f, 354f) * 4f,
						layerDepth = 0.1332f,
						id = 998f
					});
				}
			}
			if (getTemporarySpriteByID(999) == null)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(208, 591, 65, 49),
					sourceRectStartingPos = new Vector2(208f, 591f),
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = Game1.random.Next(100, 1200),
					scale = 4f,
					position = new Vector2(536f, 340f) * 4f,
					layerDepth = 0.1332f,
					id = 999f,
					delayBeforeAnimationStart = Game1.random.Next(1000)
				});
			}
			if (timeOfDay == Game1.getModeratelyDarkTime() && Game1.currentLocation == this)
			{
				lightMerchantLamps();
			}
		}

		public void lightMerchantLamps()
		{
			if (getTemporarySpriteByID(1000) == null)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(181, 633, 7, 6),
					sourceRectStartingPos = new Vector2(181f, 633f),
					animationLength = 1,
					totalNumberOfLoops = 9999,
					interval = 99999f,
					scale = 4f,
					position = new Vector2(545f, 309f) * 4f,
					layerDepth = 0.134f,
					id = 1000f,
					light = true,
					lightRadius = 1f,
					lightcolor = Color.Black
				});
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(181, 633, 7, 6),
					sourceRectStartingPos = new Vector2(181f, 633f),
					animationLength = 1,
					totalNumberOfLoops = 9999,
					interval = 99999f,
					scale = 4f,
					position = new Vector2(644f, 360f) * 4f,
					layerDepth = 0.134f,
					id = 1000f,
					light = true,
					lightRadius = 1f,
					lightcolor = Color.Black
				});
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(181, 633, 7, 6),
					sourceRectStartingPos = new Vector2(181f, 633f),
					animationLength = 1,
					totalNumberOfLoops = 9999,
					interval = 99999f,
					scale = 4f,
					position = new Vector2(717f, 309f) * 4f,
					layerDepth = 0.134f,
					id = 1000f,
					light = true,
					lightRadius = 1f,
					lightcolor = Color.Black
				});
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (farmers.Count() <= 1)
			{
				busDoor = null;
			}
		}

		public void busDriveOff()
		{
			busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), busPosition + new Vector2(16f, 26f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 999999f,
				animationLength = 6,
				holdLastFrame = true,
				layerDepth = (busPosition.Y + 192f) / 10000f + 1E-05f,
				scale = 4f
			};
			busDoor.timer = 0f;
			busDoor.interval = 70f;
			busDoor.endFunction = busStartMovingOff;
			localSound("trashcanlid");
			drivingBack = false;
			busDoor.paused = false;
		}

		public void busDriveBack()
		{
			busPosition.X = map.GetLayer("Back").DisplayWidth;
			busDoor.Position = busPosition + new Vector2(16f, 26f) * 4f;
			drivingBack = true;
			drivingOff = false;
			localSound("busDriveOff");
			busMotion = new Vector2(-6f, 0f);
		}

		private void busStartMovingOff(int extraInfo)
		{
			Game1.globalFadeToBlack(delegate
			{
				Game1.globalFadeToClear();
				localSound("batFlap");
				drivingOff = true;
				localSound("busDriveOff");
				Game1.changeMusicTrack("none");
			});
		}

		public override void performTouchAction(string fullActionString, Vector2 playerStandingPosition)
		{
			string a = fullActionString.Split(' ')[0];
			if (a == "DesertBus")
			{
				Response[] returnOptions = new Response[2]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Locations:Desert_Return_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Locations:Desert_Return_No"))
				};
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Desert_Return_Question"), returnOptions, "DesertBus");
			}
			else
			{
				base.performTouchAction(fullActionString, playerStandingPosition);
			}
		}

		private void doorOpenAfterReturn(int extraInfo)
		{
			localSound("batFlap");
			busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), busPosition + new Vector2(16f, 26f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 999999f,
				animationLength = 6,
				holdLastFrame = true,
				layerDepth = (busPosition.Y + 192f) / 10000f + 1E-05f,
				scale = 4f
			};
			Game1.player.Position = new Vector2(18f, 27f) * 64f;
			lastTouchActionLocation = Game1.player.getTileLocation();
			Game1.displayFarmer = true;
			Game1.player.forceCanMove();
			Game1.player.faceDirection(2);
			Game1.changeMusicTrack("wavy");
		}

		private void busLeftToValley()
		{
			Game1.viewport.Y = -100000;
			Game1.viewportFreeze = true;
			Game1.warpFarmer("BusStop", 12, 10, flip: true);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (drivingOff && !leaving)
			{
				busMotion.X -= 0.075f;
				if (busPosition.X + 512f < 0f)
				{
					leaving = true;
					Game1.globalFadeToBlack(busLeftToValley, 0.01f);
				}
			}
			if (drivingBack && busMotion != Vector2.Zero)
			{
				Game1.player.Position = busDoor.position;
				Game1.player.freezePause = 100;
				if (busPosition.X - 1088f < 256f)
				{
					busMotion.X = Math.Min(-1f, busMotion.X * 0.98f);
				}
				if (Math.Abs(busPosition.X - 1088f) <= Math.Abs(busMotion.X * 1.5f))
				{
					busPosition.X = 1088f;
					busMotion = Vector2.Zero;
					Game1.globalFadeToBlack(delegate
					{
						drivingBack = false;
						busDoor.Position = busPosition + new Vector2(16f, 26f) * 4f;
						busDoor.pingPong = true;
						busDoor.interval = 70f;
						busDoor.currentParentTileIndex = 5;
						busDoor.endFunction = doorOpenAfterReturn;
						localSound("trashcanlid");
						Game1.globalFadeToClear();
					});
				}
			}
			if (!busMotion.Equals(Vector2.Zero))
			{
				busPosition += busMotion;
				if (busDoor != null)
				{
					busDoor.Position += busMotion;
				}
			}
			if (busDoor != null)
			{
				busDoor.update(time);
			}
			if (isTravelingDeserteMerchantHere())
			{
				chimneyTimer -= time.ElapsedGameTime.Milliseconds;
				if (chimneyTimer <= 0)
				{
					chimneyTimer = 500;
					Vector2 smokeSpot = new Vector2(670f, 308f) * 4f;
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), smokeSpot, flipped: false, 0.002f, new Color(255, 222, 198))
					{
						alpha = 0.05f,
						alphaFade = -0.01f,
						alphaFadeFade = -8E-05f,
						motion = new Vector2(0f, -0.5f),
						acceleration = new Vector2(0.002f, 0f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
					});
				}
			}
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			for (int x = 33; x < 46; x++)
			{
				for (int y = 20; y < 25; y++)
				{
					removeEverythingExceptCharactersFromThisTile(x, y);
				}
			}
			boughtMagicRockCandy = false;
		}

		public override bool isTilePlaceable(Vector2 v, Item item = null)
		{
			if (v.X >= 33f && v.X < 46f && v.Y >= 20f && v.Y < 25f)
			{
				return false;
			}
			return base.isTilePlaceable(v, item);
		}

		public override bool shouldHideCharacters()
		{
			if (!drivingOff)
			{
				return drivingBack;
			}
			return true;
		}

		public override void draw(SpriteBatch spriteBatch)
		{
			base.draw(spriteBatch);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)busPosition.X, (int)busPosition.Y)), busSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (busPosition.Y + 192f) / 10000f);
			if (busDoor != null)
			{
				busDoor.draw(spriteBatch);
			}
			if (drivingOff || drivingBack)
			{
				if (drivingOff && warpedToDesert)
				{
					Game1.player.faceDirection(3);
					Game1.player.blinkTimer = -1000;
					Game1.player.FarmerRenderer.draw(spriteBatch, new FarmerSprite.AnimationFrame(117, 99999, 0, secondaryArm: false, flip: true), 117, new Microsoft.Xna.Framework.Rectangle(48, 608, 16, 32), Game1.GlobalToLocal(new Vector2((int)(busPosition.X + 4f), (int)(busPosition.Y - 8f)) + pamOffset * 4f), Vector2.Zero, (busPosition.Y + 192f + 4f) / 10000f, Color.White, 0f, 1f, Game1.player);
					spriteBatch.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)busPosition.X, (int)busPosition.Y - 40) + pamOffset * 4f), transparentWindowSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (busPosition.Y + 192f + 8f) / 10000f);
				}
				else
				{
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)busPosition.X, (int)busPosition.Y) + pamOffset * 4f), pamSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (busPosition.Y + 192f + 4f) / 10000f);
				}
			}
		}
	}
}
