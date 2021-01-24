using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class BeachNightMarket : GameLocation
	{
		private Texture2D shopClosedTexture;

		private float smokeTimer;

		private string paintingMailKey;

		private bool hasReceivedFreeGift;

		private bool hasShownCCUpgrade;

		public BeachNightMarket()
		{
			forceLoadPathLayerLights = true;
		}

		public BeachNightMarket(string mapPath, string name)
			: base(mapPath, name)
		{
			forceLoadPathLayerLights = true;
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			objects.Clear();
			hasReceivedFreeGift = false;
			paintingMailKey = "NightMarketYear" + Game1.year + "Day" + getDayOfNightMarket() + "_paintingSold";
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (Game1.timeOfDay < 1700)
			{
				b.Draw(shopClosedTexture, Game1.GlobalToLocal(new Vector2(39f, 29f) * 64f + new Vector2(-1f, -3f) * 4f), new Microsoft.Xna.Framework.Rectangle(72, 167, 16, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(shopClosedTexture, Game1.GlobalToLocal(new Vector2(47f, 34f) * 64f + new Vector2(7f, -3f) * 4f), new Microsoft.Xna.Framework.Rectangle(45, 170, 26, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(shopClosedTexture, Game1.GlobalToLocal(new Vector2(19f, 31f) * 64f + new Vector2(6f, 10f) * 4f), new Microsoft.Xna.Framework.Rectangle(89, 164, 18, 23), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			}
			if (!Game1.player.mailReceived.Contains(paintingMailKey))
			{
				b.Draw(shopClosedTexture, Game1.GlobalToLocal(new Vector2(41f, 33f) * 64f + new Vector2(2f, 2f) * 4f), new Microsoft.Xna.Framework.Rectangle(144 + (getDayOfNightMarket() - 1 + (Game1.year - 1) % 3 * 3) * 28, 201, 28, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.225000009f);
			}
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				switch (map.GetLayer("Buildings").Tiles[tileLocation].TileIndex)
				{
				case 595:
					if (Game1.timeOfDay < 1700)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_Closed"));
					}
					else
					{
						Game1.activeClickableMenu = new ShopMenu(getBlueBoatStock(), 0, "BlueBoat");
					}
					break;
				case 69:
				case 877:
					if (Game1.timeOfDay < 1700)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverClosed"));
					}
					else if (!hasReceivedFreeGift)
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverQuestion"), createYesNoResponses(), "GiftGiverQuestion");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverEnjoy"));
					}
					break;
				case 653:
					if ((Game1.getLocationFromName("Submarine") as Submarine).submerged.Value || Game1.netWorldState.Value.IsSubmarineLocked)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_SubmarineInUse"));
						return true;
					}
					break;
				case 399:
					if (Game1.timeOfDay < 1700)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_Closed"));
					}
					else
					{
						Game1.activeClickableMenu = new ShopMenu(Utility.getTravelingMerchantStock((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed)), 0, "TravelerNightMarket", Utility.onTravelingMerchantShopPurchase);
					}
					break;
				case 70:
					if (Game1.timeOfDay < 1700)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_Closed"));
					}
					else
					{
						Game1.activeClickableMenu = new ShopMenu(geMagicShopStock(), 0, "magicBoatShop");
					}
					break;
				case 68:
					if (Game1.timeOfDay < 1700)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterClosed"));
					}
					else if (Game1.player.mailReceived.Contains(paintingMailKey))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterSold"));
					}
					else
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterQuestion"), createYesNoResponses(), "PainterQuestion");
					}
					break;
				case 1285:
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_WarperQuestion"), createYesNoResponses(), "WarperQuestion");
					break;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public int getDayOfNightMarket()
		{
			switch (Game1.dayOfMonth)
			{
			case 15:
				return 1;
			case 16:
				return 2;
			case 17:
				return 3;
			default:
				return -1;
			}
		}

		public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			return true;
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			Beach beach = Game1.getLocationFromName("Beach") as Beach;
			if (beach != null)
			{
				return beach.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "WarperQuestion_Yes"))
			{
				if (!(questionAndAnswer == "PainterQuestion_Yes"))
				{
					if (questionAndAnswer == "GiftGiverQuestion_Yes")
					{
						if (hasReceivedFreeGift)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverEnjoy"));
						}
						else
						{
							Game1.player.freezePause = 5000;
							temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = shopClosedTexture,
								layerDepth = 0.2442f,
								scale = 4f,
								sourceRectStartingPos = new Vector2(354f, 168f),
								sourceRect = new Microsoft.Xna.Framework.Rectangle(354, 168, 32, 32),
								animationLength = 1,
								id = 777f,
								holdLastFrame = true,
								interval = 250f,
								position = new Vector2(13f, 36f) * 64f,
								delayBeforeAnimationStart = 500,
								endFunction = getFreeGiftPartOne
							});
							hasReceivedFreeGift = true;
						}
					}
				}
				else if (Game1.player.mailReceived.Contains(paintingMailKey))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterSold"));
				}
				else if (Game1.player.Money < 1200)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
				}
				else
				{
					Game1.player.Money -= 1200;
					Game1.activeClickableMenu = null;
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Furniture(1838 + ((getDayOfNightMarket() - 1) * 2 + (Game1.year - 1) % 3 * 6), Vector2.Zero));
					Game1.multiplayer.globalChatInfoMessage("Lupini", Game1.player.Name);
					Game1.multiplayer.broadcastPartyWideMail(paintingMailKey, Multiplayer.PartyWideMessageQueue.SeenMail, no_letter: true);
				}
				return base.answerDialogueAction(questionAndAnswer, questionParams);
			}
			if (Game1.player.Money < 250)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
			}
			else
			{
				Game1.player.Money -= 250;
				Game1.player.CanMove = true;
				new Object(688, 1).performUseAction(this);
				Game1.player.freezePause = 5000;
			}
			return true;
		}

		public void getFreeGiftPartOne(int extra)
		{
			removeTemporarySpritesWithIDLocal(777f);
			Game1.soundBank.PlayCue("Milking");
			temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = shopClosedTexture,
				layerDepth = 0.2442f,
				scale = 4f,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(386, 168, 32, 32),
				animationLength = 1,
				id = 778f,
				holdLastFrame = true,
				interval = 9500f,
				position = new Vector2(13f, 36f) * 64f
			});
			for (int i = 0; i <= 2000; i += 100)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = shopClosedTexture,
					delayBeforeAnimationStart = i,
					id = 778f,
					layerDepth = 0.244300008f,
					scale = 4f,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(362, 170, 2, 2),
					animationLength = 1,
					interval = 100f,
					position = new Vector2(13f, 36f) * 64f + new Vector2(8f, 12f) * 4f,
					motion = new Vector2(0f, 2f)
				});
				if (i == 2000)
				{
					temporarySprites.Last().endFunction = getFreeGift;
				}
			}
		}

		public void getFreeGift(int extra)
		{
			Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Object(395, 1));
			removeTemporarySpritesWithIDLocal(778f);
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (force)
			{
				hasShownCCUpgrade = false;
			}
			if ((bool)(Game1.getLocationFromName("Beach") as Beach).bridgeFixed || NetWorldState.checkAnywhereForWorldStateID("beachBridgeFixed"))
			{
				Beach.fixBridge(this);
			}
			if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				Beach.showCommunityUpgradeShortcuts(this, ref hasShownCCUpgrade);
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.timeOfDay >= 1700)
			{
				Game1.changeMusicTrack("night_market");
			}
			else
			{
				Game1.changeMusicTrack("ocean");
			}
			shopClosedTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
			temporarySprites.Add(new EmilysParrot(new Vector2(2968f, 2056f)));
			paintingMailKey = "NightMarketYear" + Game1.year + "Day" + getDayOfNightMarket() + "_paintingSold";
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (timeOfDay == 1700)
			{
				if (Game1.currentSeason.Equals("winter") && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 17)
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Events:BeachNightMarket_NowOpen"));
				}
				if (Game1.currentLocation.Equals(this))
				{
					Game1.changeMusicTrack("night_market");
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = shopClosedTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(89, 164, 18, 23),
						layerDepth = 0.001f,
						interval = 100f,
						position = new Vector2(19f, 31f) * 64f + new Vector2(6f, 10f) * 4f,
						scale = 4f,
						animationLength = 3
					});
				}
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			smokeTimer -= time.ElapsedGameTime.Milliseconds;
			if (smokeTimer <= 0f)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = shopClosedTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 180, 9, 11),
					sourceRectStartingPos = new Vector2(0f, 180f),
					layerDepth = 1f,
					interval = 250f,
					position = new Vector2(35f, 38f) * 64f + new Vector2(9f, 6f) * 4f,
					scale = 4f,
					scaleChange = 0.005f,
					alpha = 0.75f,
					alphaFade = 0.005f,
					motion = new Vector2(0f, -0.5f),
					acceleration = new Vector2((float)(Game1.random.NextDouble() - 0.5) / 100f, 0f),
					animationLength = 3,
					holdLastFrame = true
				});
				smokeTimer = 1250f;
			}
		}

		public Dictionary<ISalable, int[]> getBlueBoatStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			stock.Add(new Object(Vector2.Zero, 40), new int[2]
			{
				200,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 41), new int[2]
			{
				200,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 42), new int[2]
			{
				200,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 43), new int[2]
			{
				200,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 44), new int[2]
			{
				200,
				2147483647
			});
			stock.Add(new Furniture(2397, Vector2.Zero), new int[2]
			{
				800,
				2147483647
			});
			stock.Add(new Furniture(2398, Vector2.Zero), new int[2]
			{
				800,
				2147483647
			});
			stock.Add(new Furniture(1975, Vector2.Zero), new int[2]
			{
				1000,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 48), new int[2]
			{
				500,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 184), new int[2]
			{
				500,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 188), new int[2]
			{
				500,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 192), new int[2]
			{
				500,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 196), new int[2]
			{
				500,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 200), new int[2]
			{
				500,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 204), new int[2]
			{
				500,
				2147483647
			});
			return stock;
		}

		public Dictionary<ISalable, int[]> geMagicShopStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			switch (getDayOfNightMarket())
			{
			case 1:
				stock.Add(new Object(Vector2.Zero, 47), new int[2]
				{
					200,
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 52), new int[2]
				{
					500,
					2147483647
				});
				stock.Add(new Hat(39), new int[2]
				{
					5000,
					2147483647
				});
				stock.Add(new Object(472, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[472].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(473, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[473].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(474, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[474].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(475, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[475].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(427, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[427].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(477, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[477].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(429, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[429].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				if (Game1.year > 1)
				{
					stock.Add(new Object(476, 1), new int[2]
					{
						Convert.ToInt32(Game1.objectInformation[476].Split('/')[1]) * 2,
						2147483647
					});
				}
				stock.Add(new Furniture(1796, Vector2.Zero), new int[2]
				{
					15000,
					2147483647
				});
				break;
			case 2:
				stock.Add(new Object(Vector2.Zero, 33), new int[2]
				{
					200,
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 53), new int[2]
				{
					500,
					2147483647
				});
				stock.Add(new Hat(39), new int[2]
				{
					2500,
					2147483647
				});
				stock.Add(new Object(479, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[479].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(480, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[480].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(481, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[481].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(482, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[482].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(483, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[483].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(484, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[484].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(453, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[453].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(455, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[455].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(302, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[302].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(487, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[487].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(431, 1), new int[2]
				{
					(int)(200f * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				if (Game1.year > 1)
				{
					stock.Add(new Object(485, 1), new int[2]
					{
						(int)((float)(Convert.ToInt32(Game1.objectInformation[485].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
						2147483647
					});
				}
				stock.Add(new Furniture(1796, Vector2.Zero), new int[2]
				{
					15000,
					2147483647
				});
				break;
			case 3:
				stock.Add(new Object(Vector2.Zero, 46), new int[2]
				{
					200,
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 54), new int[2]
				{
					500,
					2147483647
				});
				stock.Add(new Hat(39), new int[2]
				{
					10000,
					2147483647
				});
				stock.Add(new Object(487, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[487].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(488, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[488].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(490, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[490].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(491, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[491].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(492, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[492].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(493, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[493].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(483, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[483].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(425, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[425].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(299, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[299].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(301, 1), new int[2]
				{
					(int)((float)(Convert.ToInt32(Game1.objectInformation[301].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				stock.Add(new Object(431, 1), new int[2]
				{
					(int)(200f * Game1.MasterPlayer.difficultyModifier),
					2147483647
				});
				if (Game1.year > 1)
				{
					stock.Add(new Object(489, 1), new int[2]
					{
						(int)((float)(Convert.ToInt32(Game1.objectInformation[489].Split('/')[1]) * 2) * Game1.MasterPlayer.difficultyModifier),
						2147483647
					});
				}
				stock.Add(new Furniture(1796, Vector2.Zero), new int[2]
				{
					15000,
					2147483647
				});
				break;
			}
			int donated_minerals = 0;
			int donated_artifacts = 0;
			foreach (KeyValuePair<Vector2, int> v in Game1.netWorldState.Value.MuseumPieces.Pairs)
			{
				string obj = Game1.objectInformation[v.Value].Split('/')[3];
				if (obj.Contains("Arch"))
				{
					donated_artifacts++;
				}
				if (obj.Contains("Minerals"))
				{
					donated_minerals++;
				}
			}
			if (donated_artifacts >= 20)
			{
				stock.Add(new Object(Vector2.Zero, 139), new int[2]
				{
					5000,
					2147483647
				});
			}
			if (donated_minerals + donated_artifacts >= 40)
			{
				stock.Add(new Object(Vector2.Zero, 140), new int[2]
				{
					5000,
					2147483647
				});
			}
			return stock;
		}
	}
}
