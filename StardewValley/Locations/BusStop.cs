using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;
using System;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class BusStop : GameLocation
	{
		public const int busDefaultXTile = 11;

		public const int busDefaultYTile = 6;

		private TemporaryAnimatedSprite minecartSteam;

		private TemporaryAnimatedSprite busDoor;

		private Vector2 busPosition;

		private Vector2 busMotion;

		public bool drivingOff;

		public bool drivingBack;

		public bool leaving;

		private int forceWarpTimer;

		private Microsoft.Xna.Framework.Rectangle busSource = new Microsoft.Xna.Framework.Rectangle(288, 1247, 128, 64);

		private Microsoft.Xna.Framework.Rectangle pamSource = new Microsoft.Xna.Framework.Rectangle(384, 1311, 15, 19);

		private Vector2 pamOffset = new Vector2(0f, 29f);

		public BusStop()
		{
		}

		public BusStop(string mapPath, string name)
			: base(mapPath, name)
		{
			busPosition = new Vector2(11f, 6f) * 64f;
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				switch (map.GetLayer("Buildings").Tiles[tileLocation].TileIndex)
				{
				case 958:
				case 1080:
				case 1081:
					if (Game1.player.mount != null)
					{
						return true;
					}
					if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
					{
						if (!Game1.player.isRidingHorse() || Game1.player.mount == null)
						{
							createQuestionDialogue(answerChoices: (!Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom")) ? new Response[3]
							{
								new Response("Mines", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Mines")),
								new Response("Town", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Town")),
								new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"))
							} : new Response[4]
							{
								new Response("Mines", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Mines")),
								new Response("Town", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Town")),
								new Response("Quarry", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Quarry")),
								new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"))
							}, question: Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"), dialogKey: "Minecart");
							break;
						}
						Game1.player.mount.checkAction(Game1.player, this);
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_OutOfOrder"));
					}
					return true;
				case 1057:
					if (Game1.MasterPlayer.mailReceived.Contains("ccVault"))
					{
						if (!Game1.player.isRidingHorse() || Game1.player.mount == null)
						{
							if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.es)
							{
								createQuestionDialogueWithCustomWidth(Game1.content.LoadString("Strings\\Locations:BusStop_BuyTicketToDesert"), createYesNoResponses(), "Bus");
							}
							else
							{
								createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_BuyTicketToDesert"), createYesNoResponses(), "Bus");
							}
							break;
						}
						Game1.player.mount.checkAction(Game1.player, this);
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_DesertOutOfService"));
					}
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		private void playerReachedBusDoor(Character c, GameLocation l)
		{
			forceWarpTimer = 0;
			Game1.player.position.X = -10000f;
			Game1.changeMusicTrack("none");
			busDriveOff();
			playSound("stoneStep");
			if (Game1.player.mount != null)
			{
				Game1.player.mount.farmerPassesThrough = false;
			}
		}

		public override bool answerDialogue(Response answer)
		{
			if (lastQuestionKey != null && afterQuestion == null)
			{
				string questionAndAnswer = lastQuestionKey.Split(' ')[0] + "_" + answer.responseKey;
				if (questionAndAnswer == "Bus_Yes")
				{
					NPC pam = Game1.getCharacterFromName("Pam");
					if (Game1.player.Money >= (Game1.shippingTax ? 50 : 500) && characters.Contains(pam) && pam.getTileLocation().Equals(new Vector2(11f, 10f)))
					{
						Game1.player.Money -= (Game1.shippingTax ? 50 : 500);
						Game1.freezeControls = true;
						Game1.viewportFreeze = true;
						forceWarpTimer = 8000;
						Game1.player.controller = new PathFindController(Game1.player, this, new Point(12, 9), 0, playerReachedBusDoor);
						Game1.player.setRunning(isRunning: true);
						if (Game1.player.mount != null)
						{
							Game1.player.mount.farmerPassesThrough = true;
						}
						Desert.warpedToDesert = false;
					}
					else if (Game1.player.Money < (Game1.shippingTax ? 50 : 500))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NoDriver"));
					}
					return true;
				}
			}
			return base.answerDialogue(answer);
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			leaving = false;
			if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
			{
				minecartSteam = new TemporaryAnimatedSprite(27, new Vector2(392f, 144f), Color.White)
				{
					totalNumberOfLoops = 999999,
					interval = 60f,
					flipped = true
				};
			}
			if ((int)Game1.getFarm().grandpaScore == 0 && Game1.year >= 3 && Game1.player.eventsSeen.Contains(558292))
			{
				Game1.player.eventsSeen.Remove(558292);
			}
			if (Game1.player.getTileY() > 16 || Game1.eventUp || Game1.player.getTileX() == 0 || Game1.player.isRidingHorse() || !Game1.player.previousLocationName.Equals("Desert"))
			{
				drivingOff = false;
				drivingBack = false;
				busMotion = Vector2.Zero;
				busPosition = new Vector2(11f, 6f) * 64f;
				busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), busPosition + new Vector2(16f, 26f) * 4f, flipped: false, 0f, Color.White)
				{
					interval = 999999f,
					animationLength = 6,
					holdLastFrame = true,
					layerDepth = (busPosition.Y + 192f) / 10000f + 1E-05f,
					scale = 4f
				};
			}
			else
			{
				busPosition = new Vector2(11f, 6f) * 64f;
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
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (farmers.Count <= 1)
			{
				minecartSteam = null;
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
			Game1.freezeControls = true;
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

		private void pamReturnedToSpot(Character c, GameLocation l)
		{
		}

		private void doorOpenAfterReturn(int extraInfo)
		{
			busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), busPosition + new Vector2(16f, 26f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 999999f,
				animationLength = 6,
				holdLastFrame = true,
				layerDepth = (busPosition.Y + 192f) / 10000f + 1E-05f,
				scale = 4f
			};
			Game1.player.Position = new Vector2(12f, 10f) * 64f;
			lastTouchActionLocation = Game1.player.getTileLocation();
			Game1.displayFarmer = true;
			Game1.player.forceCanMove();
			Game1.player.faceDirection(2);
		}

		private void busLeftToDesert()
		{
			Game1.viewportFreeze = true;
			Game1.warpFarmer("Desert", 16, 24, flip: true);
			Game1.globalFade = false;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (forceWarpTimer > 0)
			{
				forceWarpTimer -= time.ElapsedGameTime.Milliseconds;
				if (forceWarpTimer <= 0)
				{
					playerReachedBusDoor(Game1.player, this);
				}
			}
			if (minecartSteam != null)
			{
				minecartSteam.update(time);
			}
			if (drivingOff && !leaving)
			{
				busMotion.X -= 0.075f;
				if (busPosition.X + 512f < 0f)
				{
					leaving = true;
					busLeftToDesert();
				}
			}
			if (drivingBack && busMotion != Vector2.Zero)
			{
				Game1.player.Position = busPosition;
				if (busPosition.X - 704f < 256f)
				{
					busMotion.X = Math.Min(-1f, busMotion.X * 0.98f);
				}
				if (Math.Abs(busPosition.X - 704f) <= Math.Abs(busMotion.X * 1.5f))
				{
					busPosition.X = 704f;
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
						if (Game1.player.horseName.Value != null && Game1.player.horseName.Value != "")
						{
							for (int i = 0; i < characters.Count; i++)
							{
								if (characters[i] is Horse && (characters[i] as Horse).getOwner() == Game1.player)
								{
									if (characters[i].Name == null || characters[i].Name.Length == 0)
									{
										Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:BusStop_ReturnToHorse2", characters[i].displayName));
									}
									else
									{
										Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:BusStop_ReturnToHorse" + (Game1.random.Next(2) + 1), characters[i].displayName));
									}
									break;
								}
							}
						}
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
			if (minecartSteam != null)
			{
				minecartSteam.draw(spriteBatch);
			}
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)busPosition.X, (int)busPosition.Y)), busSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (busPosition.Y + 192f) / 10000f);
			if (busDoor != null)
			{
				busDoor.draw(spriteBatch);
			}
			if (drivingBack && Desert.warpedToDesert)
			{
				Game1.player.faceDirection(3);
				Game1.player.blinkTimer = -1000;
				Game1.player.FarmerRenderer.draw(spriteBatch, new FarmerSprite.AnimationFrame(117, 99999, 0, secondaryArm: false, flip: true), 117, new Microsoft.Xna.Framework.Rectangle(48, 608, 16, 32), Game1.GlobalToLocal(new Vector2((int)(busPosition.X + 4f), (int)(busPosition.Y - 8f)) + pamOffset * 4f), Vector2.Zero, (busPosition.Y + 192f + 4f) / 10000f, Color.White, 0f, 1f, Game1.player);
				spriteBatch.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)busPosition.X, (int)busPosition.Y - 40) + pamOffset * 4f), new Microsoft.Xna.Framework.Rectangle(0, 0, 21, 41), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (busPosition.Y + 192f + 8f) / 10000f);
			}
			else if (drivingOff || drivingBack)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)busPosition.X, (int)busPosition.Y) + pamOffset * 4f), pamSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (busPosition.Y + 192f + 4f) / 10000f);
			}
		}
	}
}
