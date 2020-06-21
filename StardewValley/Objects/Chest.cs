using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	[XmlInclude(typeof(MeleeWeapon))]
	public class Chest : Object
	{
		public const int capacity = 36;

		[XmlElement("currentLidFrame")]
		public readonly NetInt startingLidFrame = new NetInt(501);

		public readonly NetInt lidFrameCount = new NetInt(5);

		private int currentLidFrame;

		[XmlElement("frameCounter")]
		public readonly NetInt frameCounter = new NetInt(-1);

		[XmlElement("coins")]
		public readonly NetInt coins = new NetInt();

		public readonly NetObjectList<Item> items = new NetObjectList<Item>();

		[XmlElement("chestType")]
		public readonly NetString chestType = new NetString("");

		[XmlElement("tint")]
		public readonly NetColor tint = new NetColor(Color.White);

		[XmlElement("playerChoiceColor")]
		public readonly NetColor playerChoiceColor = new NetColor(Color.Black);

		[XmlElement("playerChest")]
		public readonly NetBool playerChest = new NetBool();

		[XmlElement("fridge")]
		public readonly NetBool fridge = new NetBool();

		[XmlElement("giftbox")]
		public readonly NetBool giftbox = new NetBool();

		[XmlElement("giftboxIndex")]
		public readonly NetInt giftboxIndex = new NetInt();

		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		[XmlIgnore]
		public Color Tint
		{
			get
			{
				return tint;
			}
			set
			{
				tint.Value = value;
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(startingLidFrame, frameCounter, coins, items, chestType, tint, playerChoiceColor, playerChest, fridge, giftbox, giftboxIndex, mutex.NetFields, lidFrameCount);
		}

		public Chest()
		{
			Name = "Chest";
			type.Value = "interactive";
			boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public Chest(bool playerChest, Vector2 tileLocation)
			: base(tileLocation, 130)
		{
			Name = "Chest";
			type.Value = "Crafting";
			if (playerChest)
			{
				this.playerChest.Value = playerChest;
				startingLidFrame.Value = 131;
				bigCraftable.Value = true;
				canBeSetDown.Value = true;
			}
		}

		public Chest(bool playerChest)
			: base(Vector2.Zero, 130)
		{
			Name = "Chest";
			type.Value = "Crafting";
			if (playerChest)
			{
				this.playerChest.Value = playerChest;
				startingLidFrame.Value = 131;
				bigCraftable.Value = true;
				canBeSetDown.Value = true;
			}
		}

		public Chest(Vector2 location)
		{
			tileLocation.Value = location;
			base.name = "Chest";
			type.Value = "interactive";
			boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public Chest(string type, Vector2 location, MineShaft mine)
		{
			tileLocation.Value = location;
			switch (type)
			{
			case "OreChest":
			{
				for (int i = 0; i < 8; i++)
				{
					items.Add(new Object(tileLocation, (Game1.random.NextDouble() < 0.5) ? 384 : 382, 1));
				}
				break;
			}
			case "dungeon":
				switch ((int)location.X % 5)
				{
				case 1:
					coins.Value = (int)location.Y % 3 + 2;
					break;
				case 2:
					items.Add(new Object(tileLocation, 382, (int)location.Y % 3 + 1));
					break;
				case 3:
					items.Add(new Object(tileLocation, (mine.getMineArea() == 0) ? 378 : ((mine.getMineArea() == 40) ? 380 : 384), (int)location.Y % 3 + 1));
					break;
				case 4:
					chestType.Value = "Monster";
					break;
				}
				break;
			case "Grand":
				tint.Value = new Color(150, 150, 255);
				coins.Value = (int)location.Y % 8 + 6;
				break;
			}
			base.name = "Chest";
			base.type.Value = "interactive";
			boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public Chest(int parent_sheet_index, Vector2 tile_location, int starting_lid_frame, int lid_frame_count)
			: base(tile_location, parent_sheet_index)
		{
			playerChest.Value = true;
			startingLidFrame.Value = starting_lid_frame;
			lidFrameCount.Value = lid_frame_count;
			bigCraftable.Value = true;
			canBeSetDown.Value = true;
		}

		public Chest(int coins, List<Item> items, Vector2 location, bool giftbox = false, int giftboxIndex = 0)
		{
			base.name = "Chest";
			type.Value = "interactive";
			this.giftbox.Value = giftbox;
			this.giftboxIndex.Value = giftboxIndex;
			if (items != null)
			{
				this.items.Set(items);
			}
			this.coins.Value = coins;
			tileLocation.Value = location;
			boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public void resetLidFrame()
		{
			currentLidFrame = startingLidFrame;
		}

		public void fixLidFrame()
		{
			if (currentLidFrame == 0)
			{
				currentLidFrame = startingLidFrame;
			}
			if ((bool)playerChest)
			{
				if (mutex.IsLocked() && !mutex.IsLockHeld())
				{
					currentLidFrame = getLastLidFrame();
				}
				else if (!mutex.IsLocked())
				{
					currentLidFrame = startingLidFrame;
				}
			}
			else if (currentLidFrame == 501 && mutex.IsLocked() && !mutex.IsLockHeld())
			{
				currentLidFrame = 503;
			}
		}

		public int getLastLidFrame()
		{
			return startingLidFrame.Value + lidFrameCount.Value - 1;
		}

		public override bool performObjectDropInAction(Item dropIn, bool probe, Farmer who)
		{
			return false;
		}

		public override bool performToolAction(Tool t, GameLocation location)
		{
			if (t != null && t.getLastFarmerToUse() != null && t.getLastFarmerToUse() != Game1.player)
			{
				return false;
			}
			if ((bool)playerChest)
			{
				if (t == null)
				{
					return false;
				}
				if (t is MeleeWeapon || !t.isHeavyHitter())
				{
					return false;
				}
				if (base.performToolAction(t, location))
				{
					Farmer player = t.getLastFarmerToUse();
					if (player != null)
					{
						Vector2 c = player.GetToolLocation() / 64f;
						c.X = (int)c.X;
						c.Y = (int)c.Y;
						mutex.RequestLock(delegate
						{
							clearNulls();
							if (items.Count == 0)
							{
								performRemoveAction(tileLocation, location);
								if (location.Objects.Remove(c) && type.Equals("Crafting") && (int)fragility != 2)
								{
									location.debris.Add(new Debris(bigCraftable ? (-base.ParentSheetIndex) : base.ParentSheetIndex, player.GetToolLocation(), new Vector2(player.GetBoundingBox().Center.X, player.GetBoundingBox().Center.Y)));
								}
							}
							else if (t != null && t.isHeavyHitter() && !(t is MeleeWeapon))
							{
								location.playSound("hammer");
								shakeTimer = 100;
							}
							mutex.ReleaseLock();
						});
					}
				}
				return false;
			}
			if (t != null && t is Pickaxe && currentLidFrame == 503 && (int)frameCounter == -1 && items.Count == 0)
			{
				return true;
			}
			return false;
		}

		public void addContents(int coins, Item item)
		{
			this.coins.Value += coins;
			items.Add(item);
		}

		public void destroyAndDropContents(Vector2 pointToDropAt, GameLocation location)
		{
			if (items.Count > 0)
			{
				location.playSound("throwDownITem");
			}
			foreach (Item item in items)
			{
				if (item != null)
				{
					Game1.createItemDebris(item, pointToDropAt, Game1.random.Next(4), location);
				}
			}
			items.Clear();
			clearNulls();
		}

		public void dumpContents(GameLocation location)
		{
			Random r = new Random((int)tileLocation.X + (int)tileLocation.Y + (int)Game1.uniqueIDForThisGame + Game1.CurrentMineLevel);
			if ((int)coins <= 0 && items.Count <= 0)
			{
				if (tileLocation.X % 7f == 0f)
				{
					chestType.Value = "Monster";
				}
				else
				{
					addContents(r.Next(4, Math.Max(8, Game1.CurrentMineLevel / 10 - 5)), Utility.getUncommonItemForThisMineLevel(Game1.CurrentMineLevel, new Point((int)tileLocation.X, (int)tileLocation.Y)));
				}
			}
			if (items.Count > 0 && !chestType.Equals("Monster") && items.Count >= 1 && (mutex.IsLockHeld() || !playerChest))
			{
				bool isStardrop = Utility.IsNormalObjectAtParentSheetIndex(items[0], 434);
				if (location is FarmHouse)
				{
					if ((location as FarmHouse).owner.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Objects:ParsnipSeedPackage_SomeoneElse"));
						return;
					}
					if (!isStardrop)
					{
						Game1.player.addQuest(6);
						Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(128, 208, 16, 16), 200f, 2, 30, new Vector2(Game1.dayTimeMoneyBox.questButton.bounds.Left - 16, Game1.dayTimeMoneyBox.questButton.bounds.Bottom + 8), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true));
					}
				}
				if (isStardrop)
				{
					string stardropName = (location is FarmHouse) ? "CF_Spouse" : "CF_Mines";
					if (!Game1.player.mailReceived.Contains(stardropName))
					{
						Game1.player.eatObject(items[0] as Object, overrideFullness: true);
						Game1.player.mailReceived.Add(stardropName);
					}
					items.Clear();
				}
				else
				{
					Item item = items[0];
					items[0] = null;
					items.RemoveAt(0);
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(item);
					ItemGrabMenu grab_menu;
					if ((grab_menu = (Game1.activeClickableMenu as ItemGrabMenu)) != null)
					{
						ItemGrabMenu itemGrabMenu = grab_menu;
						itemGrabMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(itemGrabMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
						{
							grab_menu.DropRemainingItems();
						});
					}
				}
				if (Game1.mine != null)
				{
					Game1.mine.chestConsumed();
				}
			}
			if (chestType.Equals("Monster"))
			{
				Monster monster = Game1.mine.getMonsterForThisLevel(Game1.CurrentMineLevel, (int)tileLocation.X, (int)tileLocation.Y);
				Vector2 v = Utility.getVelocityTowardPlayer(new Point((int)tileLocation.X, (int)tileLocation.Y), 8f, Game1.player);
				monster.xVelocity = v.X;
				monster.yVelocity = v.Y;
				location.characters.Add(monster);
				location.playSound("explosion");
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
				location.objects.Remove(tileLocation);
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Chest.cs.12531"), Color.Red, 3500f));
			}
			else
			{
				Game1.player.gainExperience(5, 25 + Game1.CurrentMineLevel);
			}
			if ((bool)giftbox)
			{
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("LooseSprites\\Giftbox", new Rectangle(0, (int)giftboxIndex * 32, 16, 32), 80f, 11, 1, tileLocation.Value * 64f - new Vector2(0f, 52f), flicker: false, flipped: false, tileLocation.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					destroyable = false,
					holdLastFrame = true
				};
				if (location.netObjects.ContainsKey(tileLocation) && location.netObjects[tileLocation] == this)
				{
					Game1.multiplayer.broadcastSprites(location, sprite);
					location.removeObject(tileLocation, showDestroyedObject: false);
				}
				else
				{
					location.temporarySprites.Add(sprite);
				}
			}
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if ((bool)giftbox)
			{
				Game1.player.Halt();
				Game1.player.freezePause = 1000;
				who.currentLocation.playSound("Ship");
				dumpContents(who.currentLocation);
			}
			else if ((bool)playerChest)
			{
				mutex.RequestLock(delegate
				{
					frameCounter.Value = 5;
					Game1.playSound(fridge ? "doorCreak" : "openChest");
					Game1.player.Halt();
					Game1.player.freezePause = 1000;
				});
			}
			else if (!playerChest)
			{
				if (currentLidFrame == 501 && (int)frameCounter <= -1)
				{
					frameCounter.Value = 5;
					who.currentLocation.playSound("openChest");
				}
				else if (currentLidFrame == 503 && items.Count > 0)
				{
					Item item = items[0];
					items[0] = null;
					items.RemoveAt(0);
					if (Game1.mine != null)
					{
						Game1.mine.chestConsumed();
					}
					who.addItemByMenuIfNecessaryElseHoldUp(item);
					ItemGrabMenu grab_menu;
					if ((grab_menu = (Game1.activeClickableMenu as ItemGrabMenu)) != null)
					{
						ItemGrabMenu itemGrabMenu = grab_menu;
						itemGrabMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(itemGrabMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
						{
							grab_menu.DropRemainingItems();
						});
					}
				}
			}
			if (items.Count == 0 && (int)coins == 0 && !playerChest)
			{
				who.currentLocation.removeObject(tileLocation, showDestroyedObject: false);
				who.currentLocation.playSound("woodWhack");
			}
			return true;
		}

		public virtual void grabItemFromChest(Item item, Farmer who)
		{
			if (who.couldInventoryAcceptThisItem(item))
			{
				items.Remove(item);
				clearNulls();
				Game1.activeClickableMenu = new ItemGrabMenu(items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this);
			}
		}

		public virtual Item addItem(Item item)
		{
			item.resetState();
			clearNulls();
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null && items[i].canStackWith(item))
				{
					item.Stack = items[i].addToStack(item);
					if (item.Stack <= 0)
					{
						return null;
					}
				}
			}
			if (items.Count < 36)
			{
				items.Add(item);
				return null;
			}
			return item;
		}

		public virtual void grabItemFromInventory(Item item, Farmer who)
		{
			if (item.Stack == 0)
			{
				item.Stack = 1;
			}
			Item tmp = addItem(item);
			if (tmp == null)
			{
				who.removeItemFromInventory(item);
			}
			else
			{
				tmp = who.addItemToInventory(tmp);
			}
			clearNulls();
			int oldID = (Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : (-1);
			Game1.activeClickableMenu = new ItemGrabMenu(items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this);
			(Game1.activeClickableMenu as ItemGrabMenu).heldItem = tmp;
			if (oldID != -1)
			{
				Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}

		public virtual bool isEmpty()
		{
			for (int i = items.Count() - 1; i >= 0; i--)
			{
				if (items[i] != null)
				{
					return false;
				}
			}
			return true;
		}

		public virtual void clearNulls()
		{
			for (int i = items.Count - 1; i >= 0; i--)
			{
				if (items[i] == null)
				{
					items.RemoveAt(i);
				}
			}
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			fixLidFrame();
			mutex.Update(environment);
			if (shakeTimer > 0)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
				if (shakeTimer <= 0)
				{
					health = 10;
				}
			}
			if ((bool)playerChest)
			{
				if ((int)frameCounter > -1 && currentLidFrame < getLastLidFrame() + 1)
				{
					frameCounter.Value--;
					if ((int)frameCounter <= 0 && mutex.IsLockHeld())
					{
						if (currentLidFrame == getLastLidFrame())
						{
							Game1.activeClickableMenu = new ItemGrabMenu(items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, this);
							frameCounter.Value = -1;
						}
						else
						{
							frameCounter.Value = 5;
							currentLidFrame++;
						}
					}
				}
				else if ((int)frameCounter == -1 && currentLidFrame > (int)startingLidFrame && Game1.activeClickableMenu == null && mutex.IsLockHeld())
				{
					mutex.ReleaseLock();
					currentLidFrame = getLastLidFrame();
					frameCounter.Value = 2;
					environment.localSound("doorCreakReverse");
				}
			}
			else
			{
				if ((int)frameCounter <= -1 || currentLidFrame >= 504)
				{
					return;
				}
				frameCounter.Value--;
				if ((int)frameCounter > 0)
				{
					return;
				}
				if (currentLidFrame == 503)
				{
					dumpContents(environment);
					frameCounter.Value = -1;
					return;
				}
				frameCounter.Value = 10;
				currentLidFrame++;
				if (currentLidFrame == 503)
				{
					frameCounter.Value += 5;
				}
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			float base_sort_order = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
			if ((bool)playerChest && base.ParentSheetIndex == 130)
			{
				if (playerChoiceColor.Value.Equals(Color.Black))
				{
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - 1) * 64)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, 130, 16, 32), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - 1) * 64)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame, 16, 32), tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
					return;
				}
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, 168, 16, 32), playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 + 20)), new Rectangle(0, 725, 16, 11), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 2E-05f);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame + 46, 16, 32), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 2E-05f);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame + 38, 16, 32), playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
				return;
			}
			if ((bool)playerChest)
			{
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - 1) * 64)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, base.ParentSheetIndex, 16, 32), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - 1) * 64)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame, 16, 32), tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
				return;
			}
			if ((bool)giftbox)
			{
				spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
				if (items.Count > 0 || (int)coins > 0)
				{
					int textureY = (int)giftboxIndex * 32;
					spriteBatch.Draw(Game1.giftboxTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - 52)), new Rectangle(0, textureY, 16, 32), tint, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
				}
				return;
			}
			spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
			spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 500, 16, 16), tint, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
			Vector2 lidPosition = new Vector2(x * 64, y * 64);
			switch (currentLidFrame)
			{
			case 501:
				lidPosition.Y -= 32f;
				break;
			case 502:
				lidPosition.Y -= 40f;
				break;
			case 503:
				lidPosition.Y -= 60f;
				break;
			}
			spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, lidPosition), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, currentLidFrame, 16, 16), tint, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
		}

		public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f, bool local = false)
		{
			if ((bool)playerChest)
			{
				if (playerChoiceColor.Equals(Color.Black))
				{
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - 1) * 64)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, 130, 16, 32), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.89f : ((float)(y * 64 + 4) / 10000f));
					return;
				}
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, 168, 16, 32), playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 4) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame + 38, 16, 32), playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 5) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y + 20) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 + 20)), new Rectangle(0, 725, 16, 11), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame + 46, 16, 32), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
			}
		}
	}
}
