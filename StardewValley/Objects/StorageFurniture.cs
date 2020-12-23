using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Menus;
using StardewValley.Network;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class StorageFurniture : Furniture
	{
		[XmlElement("heldItems")]
		public readonly NetObjectList<Item> heldItems = new NetObjectList<Item>();

		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		public StorageFurniture()
		{
		}

		public StorageFurniture(int which, Vector2 tile, int initialRotations)
			: base(which, tile, initialRotations)
		{
		}

		public StorageFurniture(int which, Vector2 tile)
			: base(which, tile)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(heldItems, mutex.NetFields);
		}

		public override bool canBeRemoved(Farmer who)
		{
			if (mutex.IsLocked())
			{
				return false;
			}
			return base.canBeRemoved(who);
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			mutex.RequestLock(delegate
			{
				ShowMenu();
			});
			return true;
		}

		public virtual void ShowMenu()
		{
			ShowShopMenu();
		}

		public virtual void ShowChestMenu()
		{
			Game1.activeClickableMenu = new ItemGrabMenu(heldItems, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, GrabItemFromInventory, null, GrabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this)
			{
				behaviorBeforeCleanup = delegate
				{
					mutex.ReleaseLock();
					OnMenuClose();
				}
			};
			Game1.playSound("dwop");
		}

		public virtual void GrabItemFromInventory(Item item, Farmer who)
		{
			if (item.Stack == 0)
			{
				item.Stack = 1;
			}
			Item tmp = AddItem(item);
			if (tmp == null)
			{
				who.removeItemFromInventory(item);
			}
			else
			{
				tmp = who.addItemToInventory(tmp);
			}
			ClearNulls();
			int oldID = (Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : (-1);
			ShowChestMenu();
			(Game1.activeClickableMenu as ItemGrabMenu).heldItem = tmp;
			if (oldID != -1)
			{
				Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}

		public virtual bool HighlightItems(Item item)
		{
			return InventoryMenu.highlightAllItems(item);
		}

		public virtual void GrabItemFromChest(Item item, Farmer who)
		{
			if (who.couldInventoryAcceptThisItem(item))
			{
				heldItems.Remove(item);
				ClearNulls();
				ShowChestMenu();
			}
		}

		public virtual void ClearNulls()
		{
			for (int i = heldItems.Count - 1; i >= 0; i--)
			{
				if (heldItems[i] == null)
				{
					heldItems.RemoveAt(i);
				}
			}
		}

		public virtual Item AddItem(Item item)
		{
			item.resetState();
			ClearNulls();
			for (int i = 0; i < heldItems.Count; i++)
			{
				if (heldItems[i] != null && heldItems[i].canStackWith(item))
				{
					item.Stack = heldItems[i].addToStack(item);
					if (item.Stack <= 0)
					{
						return null;
					}
				}
			}
			if (heldItems.Count < 36)
			{
				heldItems.Add(item);
				return null;
			}
			return item;
		}

		public void ShowShopMenu()
		{
			List<Item> list = heldItems.ToList();
			list.Sort(SortItems);
			Dictionary<ISalable, int[]> contents = new Dictionary<ISalable, int[]>();
			foreach (Item item in list)
			{
				contents[item] = new int[2]
				{
					0,
					1
				};
			}
			Game1.activeClickableMenu = new ShopMenu(contents, 0, null, onDresserItemWithdrawn, onDresserItemDeposited, GetShopMenuContext())
			{
				source = this,
				behaviorBeforeCleanup = delegate
				{
					mutex.ReleaseLock();
					OnMenuClose();
				}
			};
		}

		public virtual void OnMenuClose()
		{
		}

		public virtual string GetShopMenuContext()
		{
			return "Dresser";
		}

		public override bool canBeTrashed()
		{
			if (heldItems.Count > 0)
			{
				return false;
			}
			return base.canBeTrashed();
		}

		public override void DayUpdate(GameLocation location)
		{
			base.DayUpdate(location);
			mutex.ReleaseLock();
		}

		public override Item getOne()
		{
			StorageFurniture storageFurniture = new StorageFurniture(parentSheetIndex, tileLocation);
			storageFurniture.drawPosition.Value = drawPosition;
			storageFurniture.defaultBoundingBox.Value = defaultBoundingBox;
			storageFurniture.boundingBox.Value = boundingBox;
			storageFurniture.currentRotation.Value = (int)currentRotation - 1;
			storageFurniture.isOn.Value = false;
			storageFurniture.rotations.Value = rotations;
			storageFurniture.rotate();
			storageFurniture._GetOneFrom(this);
			return storageFurniture;
		}

		public int SortItems(Item a, Item b)
		{
			if (a.Category != b.Category)
			{
				return a.Category.CompareTo(b.Category);
			}
			if (a is Clothing && b is Clothing && (a as Clothing).clothesType.Value != (b as Clothing).clothesType.Value)
			{
				return (a as Clothing).clothesType.Value.CompareTo((b as Clothing).clothesType.Value);
			}
			if (a is Hat && b is Hat)
			{
				return (a as Hat).which.Value.CompareTo((b as Hat).which.Value);
			}
			return a.ParentSheetIndex.CompareTo(b.ParentSheetIndex);
		}

		public virtual bool onDresserItemWithdrawn(ISalable salable, Farmer who, int amount)
		{
			if (salable is Item)
			{
				heldItems.Remove(salable as Item);
			}
			return false;
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			mutex.Update(environment);
			base.updateWhenCurrentLocation(time, environment);
		}

		public virtual bool onDresserItemDeposited(ISalable deposited_salable)
		{
			if (deposited_salable is Item)
			{
				heldItems.Add(deposited_salable as Item);
				if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu)
				{
					Dictionary<ISalable, int[]> contents = new Dictionary<ISalable, int[]>();
					List<Item> list = heldItems.ToList();
					list.Sort(SortItems);
					foreach (Item item in list)
					{
						contents[item] = new int[2]
						{
							0,
							1
						};
					}
					(Game1.activeClickableMenu as ShopMenu).setItemPriceAndStock(contents);
					Game1.playSound("dwop");
					return true;
				}
			}
			return false;
		}
	}
}
