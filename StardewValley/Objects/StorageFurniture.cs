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
				List<Item> list = heldItems.ToList();
				list.Sort(SortItems);
				Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
				foreach (Item current in list)
				{
					dictionary[current] = new int[2]
					{
						0,
						1
					};
				}
				Game1.activeClickableMenu = new ShopMenu(dictionary, 0, null, onDresserItemWithdrawn, onDresserItemDeposited, "Dresser")
				{
					source = this,
					behaviorBeforeCleanup = delegate
					{
						mutex.ReleaseLock();
					}
				};
			});
			return true;
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

		public bool onDresserItemWithdrawn(ISalable salable, Farmer who, int amount)
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

		public bool onDresserItemDeposited(ISalable deposited_salable)
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
