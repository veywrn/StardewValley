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
using xTile.Dimensions;

namespace StardewValley.Objects
{
	[XmlInclude(typeof(MeleeWeapon))]
	public class Chest : Object
	{
		public enum SpecialChestTypes
		{
			None,
			MiniShippingBin,
			JunimoChest,
			AutoLoader,
			Enricher
		}

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

		public readonly NetLongDictionary<NetObjectList<Item>, NetRef<NetObjectList<Item>>> separateWalletItems = new NetLongDictionary<NetObjectList<Item>, NetRef<NetObjectList<Item>>>();

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

		[XmlElement("spriteIndexOverride")]
		public readonly NetInt bigCraftableSpriteIndex = new NetInt(-1);

		[XmlElement("dropContents")]
		public readonly NetBool dropContents = new NetBool(value: false);

		[XmlElement("synchronized")]
		public readonly NetBool synchronized = new NetBool(value: false);

		[XmlIgnore]
		protected int _shippingBinFrameCounter;

		[XmlIgnore]
		protected bool _farmerNearby;

		[XmlIgnore]
		public NetVector2 kickStartTile = new NetVector2(new Vector2(-1000f, -1000f));

		[XmlIgnore]
		public Vector2? localKickStartTile;

		[XmlIgnore]
		public float kickProgress = -1f;

		[XmlIgnore]
		public readonly NetEvent0 openChestEvent = new NetEvent0();

		[XmlElement("specialChestType")]
		public readonly NetEnum<SpecialChestTypes> specialChestType = new NetEnum<SpecialChestTypes>();

		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		[XmlIgnore]
		public SpecialChestTypes SpecialChestType
		{
			get
			{
				return specialChestType.Value;
			}
			set
			{
				specialChestType.Value = value;
			}
		}

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
			base.NetFields.AddFields(startingLidFrame, frameCounter, coins, items, chestType, tint, playerChoiceColor, playerChest, fridge, giftbox, giftboxIndex, mutex.NetFields, lidFrameCount, bigCraftableSpriteIndex, dropContents, openChestEvent.NetFields, synchronized, specialChestType, kickStartTile, separateWalletItems);
			openChestEvent.onEvent += performOpenChest;
			kickStartTile.fieldChangeVisibleEvent += delegate(NetVector2 field, Vector2 old_value, Vector2 new_value)
			{
				if (Game1.gameMode != 6 && new_value.X != -1000f && new_value.Y != -1000f)
				{
					localKickStartTile = kickStartTile;
					kickProgress = 0f;
				}
			};
		}

		public Chest()
		{
			Name = "Chest";
			type.Value = "interactive";
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public Chest(bool playerChest, Vector2 tileLocation, int parentSheetIndex = 130)
			: base(tileLocation, parentSheetIndex)
		{
			Name = "Chest";
			type.Value = "Crafting";
			if (playerChest)
			{
				this.playerChest.Value = playerChest;
				startingLidFrame.Value = parentSheetIndex + 1;
				bigCraftable.Value = true;
				canBeSetDown.Value = true;
			}
			else
			{
				lidFrameCount.Value = 3;
			}
		}

		public Chest(bool playerChest, int parentSheedIndex = 130)
			: base(Vector2.Zero, parentSheedIndex)
		{
			Name = "Chest";
			type.Value = "Crafting";
			if (playerChest)
			{
				this.playerChest.Value = playerChest;
				startingLidFrame.Value = parentSheedIndex + 1;
				bigCraftable.Value = true;
				canBeSetDown.Value = true;
			}
			else
			{
				lidFrameCount.Value = 3;
			}
		}

		public Chest(Vector2 location)
		{
			tileLocation.Value = location;
			base.name = "Chest";
			type.Value = "interactive";
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
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
			lidFrameCount.Value = 3;
			base.type.Value = "interactive";
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
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
			if (!this.giftbox.Value)
			{
				lidFrameCount.Value = 3;
			}
			if (items != null)
			{
				this.items.Set(items);
			}
			this.coins.Value = coins;
			tileLocation.Value = location;
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
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
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
			{
				return;
			}
			if ((bool)playerChest)
			{
				if (GetMutex().IsLocked() && !GetMutex().IsLockHeld())
				{
					currentLidFrame = getLastLidFrame();
				}
				else if (!GetMutex().IsLocked())
				{
					currentLidFrame = startingLidFrame;
				}
			}
			else if (currentLidFrame == startingLidFrame.Value && GetMutex().IsLocked() && !GetMutex().IsLockHeld())
			{
				currentLidFrame = getLastLidFrame();
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
						GetMutex().RequestLock(delegate
						{
							clearNulls();
							if (isEmpty())
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
								if (t != player.CurrentTool)
								{
									Vector2 zero = Vector2.Zero;
									zero = ((player.FacingDirection == 1) ? new Vector2(1f, 0f) : ((player.FacingDirection == 3) ? new Vector2(-1f, 0f) : ((player.FacingDirection == 0) ? new Vector2(0f, -1f) : new Vector2(0f, 1f))));
									if (base.TileLocation.X == 0f && base.TileLocation.Y == 0f && location.getObjectAtTile((int)c.X, (int)c.Y) == this)
									{
										base.TileLocation = c;
									}
									MoveToSafePosition(location, base.TileLocation, 0, zero);
								}
							}
							GetMutex().ReleaseLock();
						});
					}
				}
				return false;
			}
			if (t != null && t is Pickaxe && currentLidFrame == getLastLidFrame() && (int)frameCounter == -1 && isEmpty())
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

		public bool MoveToSafePosition(GameLocation location, Vector2 tile_position, int depth = 0, Vector2? prioritize_direction = null)
		{
			List<Vector2> offsets = new List<Vector2>();
			offsets.AddRange(new Vector2[4]
			{
				new Vector2(1f, 0f),
				new Vector2(-1f, 0f),
				new Vector2(0f, -1f),
				new Vector2(0f, 1f)
			});
			Utility.Shuffle(Game1.random, offsets);
			if (prioritize_direction.HasValue)
			{
				offsets.Remove(-prioritize_direction.Value);
				offsets.Insert(0, -prioritize_direction.Value);
				offsets.Remove(prioritize_direction.Value);
				offsets.Insert(0, prioritize_direction.Value);
			}
			foreach (Vector2 offset2 in offsets)
			{
				Vector2 new_position2 = tile_position + offset2;
				if (canBePlacedHere(location, new_position2) && location.isTilePlaceable(new_position2))
				{
					if (location.objects.ContainsKey(base.TileLocation) && !location.objects.ContainsKey(new_position2))
					{
						location.objects.Remove(base.TileLocation);
						kickStartTile.Value = base.TileLocation;
						base.TileLocation = new_position2;
						location.objects[new_position2] = this;
						boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
					}
					return true;
				}
			}
			Utility.Shuffle(Game1.random, offsets);
			if (prioritize_direction.HasValue)
			{
				offsets.Remove(-prioritize_direction.Value);
				offsets.Insert(0, -prioritize_direction.Value);
				offsets.Remove(prioritize_direction.Value);
				offsets.Insert(0, prioritize_direction.Value);
			}
			if (depth < 3)
			{
				foreach (Vector2 offset in offsets)
				{
					Vector2 new_position = tile_position + offset;
					if (location.isPointPassable(new Location((int)(new_position.X + 0.5f) * 64, (int)(new_position.Y + 0.5f) * 64), Game1.viewport) && MoveToSafePosition(location, new_position, depth + 1, prioritize_direction))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			localKickStartTile = null;
			kickProgress = -1f;
			return base.placementAction(location, x, y, who);
		}

		public void destroyAndDropContents(Vector2 pointToDropAt, GameLocation location)
		{
			List<Item> item_list = new List<Item>();
			item_list.AddRange(items);
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
			{
				foreach (NetObjectList<Item> separate_wallet_item_list in separateWalletItems.Values)
				{
					item_list.AddRange(separate_wallet_item_list);
				}
			}
			if (item_list.Count > 0)
			{
				location.playSound("throwDownITem");
			}
			foreach (Item item in item_list)
			{
				if (item != null)
				{
					Game1.createItemDebris(item, pointToDropAt, Game1.random.Next(4), location);
				}
			}
			items.Clear();
			separateWalletItems.Clear();
			clearNulls();
		}

		public void dumpContents(GameLocation location)
		{
			if (synchronized.Value && (GetMutex().IsLocked() || !Game1.IsMasterGame) && !GetMutex().IsLockHeld())
			{
				return;
			}
			if (items.Count > 0 && !chestType.Equals("Monster") && items.Count >= 1 && (GetMutex().IsLockHeld() || !playerChest))
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
						Game1.dayTimeMoneyBox.PingQuestLog();
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
				else if (dropContents.Value)
				{
					foreach (Item item2 in items)
					{
						if (item2 != null)
						{
							Game1.createItemDebris(item2, tileLocation.Value * 64f, -1, location);
						}
					}
					items.Clear();
					clearNulls();
					if (location is VolcanoDungeon)
					{
						if (bigCraftableSpriteIndex.Value == 223)
						{
							Game1.player.team.RequestLimitedNutDrops("VolcanoNormalChest", location, (int)tileLocation.Value.X * 64, (int)tileLocation.Value.Y * 64, 1);
						}
						else if (bigCraftableSpriteIndex.Value == 227)
						{
							Game1.player.team.RequestLimitedNutDrops("VolcanoRareChest", location, (int)tileLocation.Value.X * 64, (int)tileLocation.Value.Y * 64, 1);
						}
					}
				}
				else if (!synchronized.Value || GetMutex().IsLockHeld())
				{
					Item item = items[0];
					items[0] = null;
					items.RemoveAt(0);
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(item);
					if (location is Caldera)
					{
						Game1.player.mailReceived.Add("CalderaTreasure");
					}
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
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("LooseSprites\\Giftbox", new Microsoft.Xna.Framework.Rectangle(0, (int)giftboxIndex * 32, 16, 32), 80f, 11, 1, tileLocation.Value * 64f - new Vector2(0f, 52f), flicker: false, flipped: false, tileLocation.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
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

		public NetMutex GetMutex()
		{
			if (specialChestType.Value == SpecialChestTypes.JunimoChest)
			{
				return Game1.player.team.junimoChestMutex;
			}
			return mutex;
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
				if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
				{
					return false;
				}
				GetMutex().RequestLock(delegate
				{
					if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
					{
						OpenMiniShippingMenu();
					}
					else
					{
						frameCounter.Value = 5;
						Game1.playSound(fridge ? "doorCreak" : "openChest");
						Game1.player.Halt();
						Game1.player.freezePause = 1000;
					}
				});
			}
			else if (!playerChest)
			{
				if (currentLidFrame == startingLidFrame.Value && (int)frameCounter <= -1)
				{
					who.currentLocation.playSound("openChest");
					if (synchronized.Value)
					{
						GetMutex().RequestLock(delegate
						{
							openChestEvent.Fire();
						});
					}
					else
					{
						performOpenChest();
					}
				}
				else if (currentLidFrame == getLastLidFrame() && items.Count > 0 && !synchronized.Value)
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

		public virtual void OpenMiniShippingMenu()
		{
			Game1.playSound("shwip");
			ShowMenu();
		}

		public virtual void performOpenChest()
		{
			frameCounter.Value = 5;
		}

		public virtual void grabItemFromChest(Item item, Farmer who)
		{
			if (who.couldInventoryAcceptThisItem(item))
			{
				GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Remove(item);
				clearNulls();
				ShowMenu();
			}
		}

		public virtual Item addItem(Item item)
		{
			item.resetState();
			clearNulls();
			NetObjectList<Item> item_list = items;
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin || SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				item_list = GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
			}
			for (int i = 0; i < item_list.Count; i++)
			{
				if (item_list[i] != null && item_list[i].canStackWith(item))
				{
					item.Stack = item_list[i].addToStack(item);
					if (item.Stack <= 0)
					{
						return null;
					}
				}
			}
			if (item_list.Count < GetActualCapacity())
			{
				item_list.Add(item);
				return null;
			}
			return item;
		}

		public virtual int GetActualCapacity()
		{
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
			{
				return 9;
			}
			if (SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				return 9;
			}
			if (SpecialChestType == SpecialChestTypes.Enricher)
			{
				return 1;
			}
			return 36;
		}

		public virtual void CheckAutoLoad(Farmer who)
		{
			if (who.currentLocation != null)
			{
				Object beneath_object = null;
				if (who.currentLocation.objects.TryGetValue(new Vector2(base.TileLocation.X, base.TileLocation.Y + 1f), out beneath_object))
				{
					beneath_object?.AttemptAutoLoad(who);
				}
			}
		}

		public virtual void ShowMenu()
		{
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
			{
				Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, Utility.highlightShippableObjects, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 1, fridge ? null : this, -1, this);
			}
			else if (SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, this);
			}
			else if (SpecialChestType == SpecialChestTypes.AutoLoader)
			{
				ItemGrabMenu itemGrabMenu = new ItemGrabMenu(GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, this);
				itemGrabMenu.exitFunction = (IClickableMenu.onExit)Delegate.Combine(itemGrabMenu.exitFunction, (IClickableMenu.onExit)delegate
				{
					CheckAutoLoad(Game1.player);
				});
				Game1.activeClickableMenu = itemGrabMenu;
			}
			else if (SpecialChestType == SpecialChestTypes.Enricher)
			{
				Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, Object.HighlightFertilizers, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, this);
			}
			else
			{
				Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, this);
			}
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
			ShowMenu();
			(Game1.activeClickableMenu as ItemGrabMenu).heldItem = tmp;
			if (oldID != -1)
			{
				Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}

		public NetObjectList<Item> GetItemsForPlayer(long id)
		{
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value && SpecialChestType == SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value)
			{
				if (!separateWalletItems.ContainsKey(id))
				{
					separateWalletItems[id] = new NetObjectList<Item>();
				}
				return separateWalletItems[id];
			}
			if (SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				return Game1.player.team.junimoChest;
			}
			return items;
		}

		public virtual bool isEmpty()
		{
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value)
			{
				foreach (NetObjectList<Item> item_list in separateWalletItems.Values)
				{
					for (int k = item_list.Count() - 1; k >= 0; k--)
					{
						if (item_list[k] != null)
						{
							return false;
						}
					}
				}
				return true;
			}
			if (SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				NetObjectList<Item> actual_items = GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
				for (int j = actual_items.Count - 1; j >= 0; j--)
				{
					if (actual_items[j] != null)
					{
						return false;
					}
				}
				return true;
			}
			for (int i = items.Count - 1; i >= 0; i--)
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
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin || SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				NetObjectList<Item> item_list = GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
				for (int i = item_list.Count - 1; i >= 0; i--)
				{
					if (item_list[i] == null)
					{
						item_list.RemoveAt(i);
					}
				}
				return;
			}
			for (int j = items.Count - 1; j >= 0; j--)
			{
				if (items[j] == null)
				{
					items.RemoveAt(j);
				}
			}
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if (synchronized.Value)
			{
				openChestEvent.Poll();
			}
			if (localKickStartTile.HasValue)
			{
				if (Game1.currentLocation == environment)
				{
					if (kickProgress == 0f)
					{
						if (Utility.isOnScreen((localKickStartTile.Value + new Vector2(0.5f, 0.5f)) * 64f, 64))
						{
							Game1.playSound("clubhit");
						}
						shakeTimer = 100;
					}
				}
				else
				{
					localKickStartTile = null;
					kickProgress = -1f;
				}
				if (kickProgress >= 0f)
				{
					float move_duration = 0.25f;
					kickProgress += (float)(time.ElapsedGameTime.TotalSeconds / (double)move_duration);
					if (kickProgress >= 1f)
					{
						kickProgress = -1f;
						localKickStartTile = null;
					}
				}
			}
			else
			{
				kickProgress = -1f;
			}
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
				if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
				{
					UpdateFarmerNearby(environment);
					if (_shippingBinFrameCounter > -1)
					{
						_shippingBinFrameCounter--;
						if (_shippingBinFrameCounter <= 0)
						{
							_shippingBinFrameCounter = 5;
							if (_farmerNearby && currentLidFrame < getLastLidFrame())
							{
								currentLidFrame++;
							}
							else if (!_farmerNearby && currentLidFrame > startingLidFrame.Value)
							{
								currentLidFrame--;
							}
							else
							{
								_shippingBinFrameCounter = -1;
							}
						}
					}
					if (Game1.activeClickableMenu == null && GetMutex().IsLockHeld())
					{
						GetMutex().ReleaseLock();
					}
				}
				else if ((int)frameCounter > -1 && currentLidFrame < getLastLidFrame() + 1)
				{
					frameCounter.Value--;
					if ((int)frameCounter <= 0 && GetMutex().IsLockHeld())
					{
						if (currentLidFrame == getLastLidFrame())
						{
							ShowMenu();
							frameCounter.Value = -1;
						}
						else
						{
							frameCounter.Value = 5;
							currentLidFrame++;
						}
					}
				}
				else if ((((int)frameCounter == -1 && currentLidFrame > (int)startingLidFrame) || currentLidFrame >= getLastLidFrame()) && Game1.activeClickableMenu == null && GetMutex().IsLockHeld())
				{
					GetMutex().ReleaseLock();
					currentLidFrame = getLastLidFrame();
					frameCounter.Value = 2;
					environment.localSound("doorCreakReverse");
				}
			}
			else
			{
				if ((int)frameCounter <= -1 || currentLidFrame > getLastLidFrame())
				{
					return;
				}
				frameCounter.Value--;
				if ((int)frameCounter > 0)
				{
					return;
				}
				if (currentLidFrame == getLastLidFrame())
				{
					dumpContents(environment);
					frameCounter.Value = -1;
					return;
				}
				frameCounter.Value = 10;
				currentLidFrame++;
				if (currentLidFrame == getLastLidFrame())
				{
					frameCounter.Value += 5;
				}
			}
		}

		public virtual void UpdateFarmerNearby(GameLocation location, bool animate = true)
		{
			bool should_open = false;
			foreach (Farmer f in location.farmers)
			{
				if (Math.Abs((float)f.getTileX() - tileLocation.X) <= 1f && Math.Abs((float)f.getTileY() - tileLocation.Y) <= 1f)
				{
					should_open = true;
					break;
				}
			}
			if (should_open == _farmerNearby)
			{
				return;
			}
			_farmerNearby = should_open;
			_shippingBinFrameCounter = 5;
			if (!animate)
			{
				_shippingBinFrameCounter = -1;
				if (_farmerNearby)
				{
					currentLidFrame = getLastLidFrame();
				}
				else
				{
					currentLidFrame = startingLidFrame.Value;
				}
			}
			else if (Game1.gameMode != 6)
			{
				if (_farmerNearby)
				{
					location.localSound("doorCreak");
				}
				else
				{
					location.localSound("doorCreakReverse");
				}
			}
		}

		public override void actionOnPlayerEntry()
		{
			fixLidFrame();
			if (specialChestType.Value == SpecialChestTypes.MiniShippingBin)
			{
				UpdateFarmerNearby(Game1.currentLocation, animate: false);
			}
			kickProgress = -1f;
			localKickStartTile = null;
			if (!playerChest && items.Count == 0 && (int)coins == 0)
			{
				currentLidFrame = getLastLidFrame();
			}
		}

		public virtual void SetBigCraftableSpriteIndex(int sprite_index, int starting_lid_frame = -1, int lid_frame_count = 3)
		{
			bigCraftableSpriteIndex.Value = sprite_index;
			if (starting_lid_frame >= 0)
			{
				startingLidFrame.Value = starting_lid_frame;
			}
			else
			{
				startingLidFrame.Value = sprite_index + 1;
			}
			lidFrameCount.Value = lid_frame_count;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			float draw_x = x;
			float draw_y = y;
			if (localKickStartTile.HasValue)
			{
				draw_x = Utility.Lerp(localKickStartTile.Value.X, draw_x, kickProgress);
				draw_y = Utility.Lerp(localKickStartTile.Value.Y, draw_y, kickProgress);
			}
			float base_sort_order = Math.Max(0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;
			if (localKickStartTile.HasValue)
			{
				spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((draw_x + 0.5f) * 64f, (draw_y + 0.5f) * 64f)), Game1.shadowTexture.Bounds, Color.Black * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0001f);
				draw_y -= (float)Math.Sin((double)kickProgress * Math.PI) * 0.5f;
			}
			if ((bool)playerChest && (base.ParentSheetIndex == 130 || base.ParentSheetIndex == 232))
			{
				if (playerChoiceColor.Value.Equals(Color.Black))
				{
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, base.ParentSheetIndex, 16, 32), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame, 16, 32), tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
					return;
				}
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? 168 : base.ParentSheetIndex, 16, 32), playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, draw_y * 64f + 20f)), new Microsoft.Xna.Framework.Rectangle(0, ((base.ParentSheetIndex == 130) ? 168 : base.ParentSheetIndex) / 8 * 32 + 53, 16, 11), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 2E-05f);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? (currentLidFrame + 46) : (currentLidFrame + 8), 16, 32), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 2E-05f);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? (currentLidFrame + 38) : currentLidFrame, 16, 32), playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
				return;
			}
			if ((bool)playerChest)
			{
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, base.ParentSheetIndex, 16, 32), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame, 16, 32), tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
				return;
			}
			if ((bool)giftbox)
			{
				spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
				if (items.Count > 0 || (int)coins > 0)
				{
					int textureY = (int)giftboxIndex * 32;
					spriteBatch.Draw(Game1.giftboxTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), draw_y * 64f - 52f)), new Microsoft.Xna.Framework.Rectangle(0, textureY, 16, 32), tint, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
				}
				return;
			}
			int sprite_index = 500;
			Texture2D sprite_sheet = Game1.objectSpriteSheet;
			int sprite_sheet_height = 16;
			int y_offset = 0;
			if (bigCraftableSpriteIndex.Value >= 0)
			{
				sprite_index = bigCraftableSpriteIndex.Value;
				sprite_sheet = Game1.bigCraftableSpriteSheet;
				sprite_sheet_height = 32;
				y_offset = -64;
			}
			if (bigCraftableSpriteIndex.Value < 0)
			{
				spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
			}
			spriteBatch.Draw(sprite_sheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, draw_y * 64f + (float)y_offset)), Game1.getSourceRectForStandardTileSheet(sprite_sheet, sprite_index, 16, sprite_sheet_height), tint, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
			Vector2 lidPosition = new Vector2(draw_x * 64f, draw_y * 64f + (float)y_offset);
			if (bigCraftableSpriteIndex.Value < 0)
			{
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
			}
			spriteBatch.Draw(sprite_sheet, Game1.GlobalToLocal(Game1.viewport, lidPosition), Game1.getSourceRectForStandardTileSheet(sprite_sheet, currentLidFrame, 16, sprite_sheet_height), tint, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
		}

		public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f, bool local = false)
		{
			if ((bool)playerChest)
			{
				if (playerChoiceColor.Equals(Color.Black))
				{
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - 1) * 64)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, parentSheetIndex, 16, 32), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.89f : ((float)(y * 64 + 4) / 10000f));
					return;
				}
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? 168 : base.ParentSheetIndex, 16, 32), playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 4) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? (currentLidFrame + 38) : currentLidFrame, 16, 32), playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 5) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y + 20) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 + 20)), new Microsoft.Xna.Framework.Rectangle(0, ((base.ParentSheetIndex == 130) ? 168 : base.ParentSheetIndex) / 8 * 32 + 53, 16, 11), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? (currentLidFrame + 46) : (currentLidFrame + 8), 16, 32), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
			}
		}
	}
}
