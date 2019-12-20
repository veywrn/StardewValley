using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class GeodeMenu : MenuWithInventory
	{
		public const int region_geodeSpot = 998;

		public ClickableComponent geodeSpot;

		public AnimatedSprite clint;

		public TemporaryAnimatedSprite geodeDestructionAnimation;

		public TemporaryAnimatedSprite sparkle;

		public int geodeAnimationTimer;

		public int yPositionOfGem;

		public int alertTimer;

		public float delayBeforeShowArtifactTimer;

		public Object geodeTreasure;

		private List<TemporaryAnimatedSprite> fluffSprites = new List<TemporaryAnimatedSprite>();

		public GeodeMenu()
			: base(null, okButton: true, trashCan: true, 12, 132)
		{
			if (yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				movePosition(0, -IClickableMenu.spaceToClearTopBorder);
			}
			inventory.highlightMethod = highlightGeodes;
			geodeSpot = new ClickableComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 4, 560, 308), "")
			{
				myID = 998,
				downNeighborID = 0
			};
			clint = new AnimatedSprite("Characters\\Clint", 8, 32, 48);
			if (inventory.inventory != null && inventory.inventory.Count >= 12)
			{
				for (int i = 0; i < 12; i++)
				{
					if (inventory.inventory[i] != null)
					{
						inventory.inventory[i].upNeighborID = 998;
					}
				}
			}
			if (trashCan != null)
			{
				trashCan.myID = 106;
			}
			if (okButton != null)
			{
				okButton.leftNeighborID = 11;
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override bool readyToClose()
		{
			if (base.readyToClose() && geodeAnimationTimer <= 0)
			{
				return heldItem == null;
			}
			return false;
		}

		public bool highlightGeodes(Item i)
		{
			if (heldItem != null)
			{
				return true;
			}
			int num = i.parentSheetIndex;
			if (num == 275 || (uint)(num - 535) <= 2u || num == 749)
			{
				return true;
			}
			return false;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound: true);
			if (!geodeSpot.containsPoint(x, y))
			{
				return;
			}
			if (heldItem != null && (heldItem.Name.Contains("Geode") || heldItem.ParentSheetIndex == 275) && Game1.player.Money >= 25 && geodeAnimationTimer <= 0)
			{
				if (Game1.player.freeSpotsInInventory() > 1 || (Game1.player.freeSpotsInInventory() == 1 && heldItem.Stack == 1))
				{
					geodeSpot.item = heldItem.getOne();
					heldItem.Stack--;
					if (heldItem.Stack <= 0)
					{
						heldItem = null;
					}
					geodeAnimationTimer = 2700;
					Game1.player.Money -= 25;
					Game1.playSound("stoneStep");
					clint.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(8, 300),
						new FarmerSprite.AnimationFrame(9, 200),
						new FarmerSprite.AnimationFrame(10, 80),
						new FarmerSprite.AnimationFrame(11, 200),
						new FarmerSprite.AnimationFrame(12, 100),
						new FarmerSprite.AnimationFrame(8, 300)
					});
					clint.loop = false;
				}
				else
				{
					descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_InventoryFull");
					wiggleWordsTimer = 500;
					alertTimer = 1500;
				}
			}
			else if (Game1.player.Money < 25)
			{
				wiggleWordsTimer = 500;
				Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			base.receiveRightClick(x, y, playSound: true);
		}

		public override void performHoverAction(int x, int y)
		{
			if (alertTimer > 0)
			{
				return;
			}
			base.performHoverAction(x, y);
			if (descriptionText.Equals(""))
			{
				if (Game1.player.Money < 25)
				{
					descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_Description_NotEnoughMoney");
				}
				else
				{
					descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_Description");
				}
			}
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			if (heldItem != null)
			{
				Game1.player.addItemToInventoryBool(heldItem);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			for (int j = fluffSprites.Count - 1; j >= 0; j--)
			{
				if (fluffSprites[j].update(time))
				{
					fluffSprites.RemoveAt(j);
				}
			}
			if (alertTimer > 0)
			{
				alertTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (geodeAnimationTimer <= 0)
			{
				return;
			}
			Game1.changeMusicTrack("none");
			geodeAnimationTimer -= time.ElapsedGameTime.Milliseconds;
			if (geodeAnimationTimer <= 0)
			{
				geodeDestructionAnimation = null;
				geodeSpot.item = null;
				Game1.player.addItemToInventoryBool(geodeTreasure);
				geodeTreasure = null;
				yPositionOfGem = 0;
				fluffSprites.Clear();
				delayBeforeShowArtifactTimer = 0f;
				return;
			}
			int frame = clint.currentFrame;
			clint.animateOnce(time);
			if (clint.currentFrame == 11 && frame != 11)
			{
				if (geodeSpot.item != null && (int)geodeSpot.item.parentSheetIndex == 275)
				{
					Game1.playSound("hammer");
					Game1.playSound("woodWhack");
				}
				else
				{
					Game1.playSound("hammer");
					Game1.playSound("stoneCrack");
				}
				Game1.stats.GeodesCracked++;
				int geodeDestructionYOffset = 448;
				if (geodeSpot.item != null)
				{
					switch ((int)(geodeSpot.item as Object).parentSheetIndex)
					{
					case 536:
						geodeDestructionYOffset += 64;
						break;
					case 537:
						geodeDestructionYOffset += 128;
						break;
					}
					geodeDestructionAnimation = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, geodeDestructionYOffset, 64, 64), 100f, 8, 0, new Vector2(geodeSpot.bounds.X + 392 - 32, geodeSpot.bounds.Y + 192 - 32), flicker: false, flipped: false);
					if (geodeSpot.item != null && (int)geodeSpot.item.parentSheetIndex == 275)
					{
						geodeDestructionAnimation = new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites//temporary_sprites_1"),
							sourceRect = new Rectangle(388, 123, 18, 21),
							sourceRectStartingPos = new Vector2(388f, 123f),
							animationLength = 6,
							position = new Vector2(geodeSpot.bounds.X + 380 - 32, geodeSpot.bounds.Y + 192 - 32),
							holdLastFrame = true,
							interval = 100f,
							id = 777f,
							scale = 4f
						};
						for (int i = 0; i < 6; i++)
						{
							fluffSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(geodeSpot.bounds.X + 392 - 32 + Game1.random.Next(21), geodeSpot.bounds.Y + 192 - 16), flipped: false, 0.002f, new Color(255, 222, 198))
							{
								alphaFade = 0.02f,
								motion = new Vector2((float)Game1.random.Next(-20, 21) / 10f, (float)Game1.random.Next(5, 20) / 10f),
								interval = 99999f,
								layerDepth = 0.9f,
								scale = 3f,
								scaleChange = 0.01f,
								rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
								delayBeforeAnimationStart = i * 20
							});
							fluffSprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites//temporary_sprites_1"),
								sourceRect = new Rectangle(499, 132, 5, 5),
								sourceRectStartingPos = new Vector2(499f, 132f),
								motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-7, -4)),
								acceleration = new Vector2(0f, 0.25f),
								totalNumberOfLoops = 1,
								interval = 1000f,
								alphaFade = 0.015f,
								animationLength = 1,
								layerDepth = 1f,
								scale = 4f,
								rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
								delayBeforeAnimationStart = i * 10,
								position = new Vector2(geodeSpot.bounds.X + 392 - 32 + Game1.random.Next(21), geodeSpot.bounds.Y + 192 - 16)
							});
							delayBeforeShowArtifactTimer = 500f;
						}
					}
					geodeTreasure = Utility.getTreasureFromGeode(geodeSpot.item);
					if ((int)geodeSpot.item.parentSheetIndex == 275)
					{
						Game1.player.foundArtifact(geodeTreasure.parentSheetIndex, 1);
					}
					else if (geodeTreasure.Type.Contains("Mineral"))
					{
						Game1.player.foundMineral(geodeTreasure.parentSheetIndex);
					}
					else if (geodeTreasure.Type.Contains("Arch") && !Game1.player.hasOrWillReceiveMail("artifactFound"))
					{
						geodeTreasure = new Object(390, 5);
					}
				}
			}
			if (geodeDestructionAnimation != null && ((geodeDestructionAnimation.id != 777f && geodeDestructionAnimation.currentParentTileIndex < 7) || (geodeDestructionAnimation.id == 777f && geodeDestructionAnimation.currentParentTileIndex < 5)))
			{
				geodeDestructionAnimation.update(time);
				if (delayBeforeShowArtifactTimer > 0f)
				{
					delayBeforeShowArtifactTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
					if (delayBeforeShowArtifactTimer <= 0f)
					{
						fluffSprites.Add(geodeDestructionAnimation);
						fluffSprites.Reverse();
						geodeDestructionAnimation = new TemporaryAnimatedSprite
						{
							interval = 100f,
							animationLength = 6,
							alpha = 0.001f,
							id = 777f
						};
					}
				}
				else
				{
					if (geodeDestructionAnimation.currentParentTileIndex < 3)
					{
						yPositionOfGem--;
					}
					yPositionOfGem--;
					if ((geodeDestructionAnimation.currentParentTileIndex == 7 || (geodeDestructionAnimation.id == 777f && geodeDestructionAnimation.currentParentTileIndex == 5)) && (int)geodeTreasure.price > 75)
					{
						sparkle = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 640, 64, 64), 100f, 8, 0, new Vector2(geodeSpot.bounds.X + 392 - 32, geodeSpot.bounds.Y + 192 + yPositionOfGem - 32), flicker: false, flipped: false);
						Game1.playSound("discoverMineral");
					}
					else if ((geodeDestructionAnimation.currentParentTileIndex == 7 || (geodeDestructionAnimation.id == 777f && geodeDestructionAnimation.currentParentTileIndex == 5)) && (int)geodeTreasure.price <= 75)
					{
						Game1.playSound("newArtifact");
					}
				}
			}
			if (sparkle != null && sparkle.update(time))
			{
				sparkle = null;
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			geodeSpot = new ClickableComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 4, 560, 308), "Anvil");
			int yPositionForInventory = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
			inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPositionForInventory, playerInventory: false, null, inventory.highlightMethod);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			base.draw(b);
			Game1.dayTimeMoneyBox.drawMoneyBox(b);
			b.Draw(Game1.mouseCursors, new Vector2(geodeSpot.bounds.X, geodeSpot.bounds.Y), new Rectangle(0, 512, 140, 78), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			if (geodeSpot.item != null)
			{
				if (geodeDestructionAnimation == null)
				{
					geodeSpot.item.drawInMenu(b, new Vector2(geodeSpot.bounds.X + 360 + (((int)geodeSpot.item.parentSheetIndex == 275) ? (-8) : 0), geodeSpot.bounds.Y + 160 + (((int)geodeSpot.item.parentSheetIndex == 275) ? 8 : 0)), 1f);
				}
				else
				{
					geodeDestructionAnimation.draw(b, localPosition: true);
				}
				foreach (TemporaryAnimatedSprite fluffSprite in fluffSprites)
				{
					fluffSprite.draw(b, localPosition: true);
				}
				if (geodeTreasure != null && delayBeforeShowArtifactTimer <= 0f)
				{
					geodeTreasure.drawInMenu(b, new Vector2(geodeSpot.bounds.X + 360, geodeSpot.bounds.Y + 160 + yPositionOfGem), 1f);
				}
				if (sparkle != null)
				{
					sparkle.draw(b, localPosition: true);
				}
			}
			clint.draw(b, new Vector2(geodeSpot.bounds.X + 384, geodeSpot.bounds.Y + 64), 0.877f);
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
			}
			if (!Game1.options.hardwareCursor)
			{
				drawMouse(b);
			}
		}
	}
}
