using Netcode;
using StardewValley.Network;
using System;
using System.Collections.Generic;

namespace StardewValley.Util
{
	public class SynchronizedShopStock : INetObject<NetFields>
	{
		public enum SynchedShop
		{
			Krobus,
			TravelingMerchant,
			Sandy,
			Saloon
		}

		private readonly NetIntDictionary<int, NetInt> lastDayUpdated = new NetIntDictionary<int, NetInt>();

		private readonly NetStringDictionary<int, NetInt> sharedKrobusStock = new NetStringDictionary<int, NetInt>();

		private readonly NetStringDictionary<int, NetInt> sharedSandyStock = new NetStringDictionary<int, NetInt>();

		private readonly NetStringDictionary<int, NetInt> sharedTravelingMerchantStock = new NetStringDictionary<int, NetInt>();

		private readonly NetStringDictionary<int, NetInt> sharedSaloonStock = new NetStringDictionary<int, NetInt>();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public SynchronizedShopStock()
		{
			initNetFields();
		}

		private void initNetFields()
		{
			NetFields.AddFields(lastDayUpdated, sharedKrobusStock, sharedSandyStock, sharedTravelingMerchantStock, sharedSaloonStock);
		}

		private NetStringDictionary<int, NetInt> getSharedStock(SynchedShop shop)
		{
			switch (shop)
			{
			case SynchedShop.Krobus:
				return sharedKrobusStock;
			case SynchedShop.Sandy:
				return sharedSandyStock;
			case SynchedShop.TravelingMerchant:
				return sharedTravelingMerchantStock;
			case SynchedShop.Saloon:
				return sharedSaloonStock;
			default:
				Console.WriteLine("Tried to get shared stock for invalid shop " + shop);
				return null;
			}
		}

		private int getLastDayUpdated(SynchedShop shop)
		{
			if (!lastDayUpdated.ContainsKey((int)shop))
			{
				lastDayUpdated[(int)shop] = -1;
			}
			return lastDayUpdated[(int)shop];
		}

		private int setLastDayUpdated(SynchedShop shop, int value)
		{
			if (!lastDayUpdated.ContainsKey((int)shop))
			{
				lastDayUpdated[(int)shop] = 0;
			}
			return lastDayUpdated[(int)shop] = value;
		}

		public void OnItemPurchased(SynchedShop shop, ISalable item, int amount)
		{
			NetStringDictionary<int, NetInt> sharedStock = getSharedStock(shop);
			string itemString = Utility.getStandardDescriptionFromItem(item as Item, 1);
			if (sharedStock.ContainsKey(itemString) && sharedStock[itemString] != int.MaxValue && (!(item is Object) || !(item as Object).IsRecipe))
			{
				sharedStock[itemString] -= amount;
			}
		}

		public void UpdateLocalStockWithSyncedQuanitities(SynchedShop shop, Dictionary<ISalable, int[]> localStock, Dictionary<string, Func<bool>> conditionalItemFilters = null)
		{
			List<Item> itemsToRemove = new List<Item>();
			NetStringDictionary<int, NetInt> sharedStock = getSharedStock(shop);
			if (getLastDayUpdated(shop) != Game1.Date.TotalDays)
			{
				setLastDayUpdated(shop, Game1.Date.TotalDays);
				sharedStock.Clear();
				foreach (Item item5 in localStock.Keys)
				{
					string itemString3 = Utility.getStandardDescriptionFromItem(item5, 1);
					sharedStock.Add(itemString3, localStock[item5][1]);
					if (sharedStock[itemString3] != int.MaxValue)
					{
						item5.Stack = sharedStock[itemString3];
					}
				}
			}
			else
			{
				itemsToRemove.Clear();
				foreach (Item item4 in localStock.Keys)
				{
					string itemString2 = Utility.getStandardDescriptionFromItem(item4, 1);
					if (sharedStock.ContainsKey(itemString2) && sharedStock[itemString2] > 0)
					{
						localStock[item4][1] = sharedStock[itemString2];
						if (sharedStock[itemString2] != int.MaxValue)
						{
							item4.Stack = sharedStock[itemString2];
						}
					}
					else
					{
						itemsToRemove.Add(item4);
					}
				}
				foreach (Item item3 in itemsToRemove)
				{
					localStock.Remove(item3);
				}
			}
			itemsToRemove.Clear();
			if (conditionalItemFilters != null)
			{
				foreach (Item item2 in localStock.Keys)
				{
					string itemString = Utility.getStandardDescriptionFromItem(item2, 1);
					if (conditionalItemFilters.ContainsKey(itemString) && !conditionalItemFilters[itemString]())
					{
						itemsToRemove.Add(item2);
					}
				}
				foreach (Item item in itemsToRemove)
				{
					localStock.Remove(item);
				}
			}
		}
	}
}
