using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class Cabin : FarmHouse
	{
		private static Random farmhandIDRandom = new Random();

		[XmlElement("farmhand")]
		public readonly NetRef<Farmer> farmhand = new NetRef<Farmer>();

		[XmlIgnore]
		public readonly NetMutex inventoryMutex = new NetMutex();

		[XmlIgnore]
		public override Farmer owner
		{
			get
			{
				if (getFarmhand().Value.isActive())
				{
					return Game1.otherFarmers[getFarmhand().Value.UniqueMultiplayerID];
				}
				return getFarmhand().Value;
			}
		}

		[XmlIgnore]
		public override int upgradeLevel
		{
			get
			{
				if (farmhand.Value == null)
				{
					return 0;
				}
				return owner.houseUpgradeLevel;
			}
			set
			{
				owner.houseUpgradeLevel.Value = value;
			}
		}

		public Cabin()
		{
		}

		public Cabin(string map)
			: base(map, "Cabin")
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(farmhand, inventoryMutex.NetFields);
		}

		public NetRef<Farmer> getFarmhand()
		{
			if (farmhand.Value == null)
			{
				farmhand.Value = new Farmer(new FarmerSprite(null), new Vector2(0f, 0f), 1, "", Farmer.initialTools(), isMale: true);
				farmhand.Value.UniqueMultiplayerID = Utility.RandomLong(farmhandIDRandom);
				SocializeQuest quest = Quest.getQuestFromId(9) as SocializeQuest;
				farmhand.Value.questLog.Add(quest);
				resetFarmhandState();
			}
			return farmhand;
		}

		public void resetFarmhandState()
		{
			if (farmhand.Value != null)
			{
				farmhand.Value.farmName.Value = Game1.MasterPlayer.farmName.Value;
				farmhand.Value.homeLocation.Value = uniqueName;
				if (farmhand.Value.lastSleepLocation.Value == null || farmhand.Value.lastSleepLocation.Value == (string)uniqueName)
				{
					farmhand.Value.currentLocation = this;
					farmhand.Value.Position = Utility.PointToVector2(GetPlayerBedSpot()) * 64f;
				}
				farmhand.Value.resetState();
			}
		}

		public void saveFarmhand(NetFarmerRoot farmhand)
		{
			farmhand.CloneInto(this.farmhand);
			resetFarmhandState();
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				int tileIndex = map.GetLayer("Buildings").Tiles[tileLocation].TileIndex;
				if ((uint)(tileIndex - 647) <= 1u && !getFarmhand().Value.isActive())
				{
					inventoryMutex.RequestLock(delegate
					{
						playSound("Ship");
						openFarmhandInventory();
					});
					return true;
				}
			}
			if (base.checkAction(tileLocation, viewport, who))
			{
				return true;
			}
			return false;
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
			inventoryMutex.Update(Game1.getOnlineFarmers());
			if (inventoryMutex.IsLockHeld() && !(Game1.activeClickableMenu is ItemGrabMenu))
			{
				inventoryMutex.ReleaseLock();
			}
		}

		public NetObjectList<Item> getInventory()
		{
			return getFarmhand().Value.items;
		}

		public void openFarmhandInventory()
		{
			Game1.activeClickableMenu = new ItemGrabMenu(getInventory(), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromPlayerInventory, null, grabItemFromFarmhandInventory, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, -1, this);
		}

		public bool isInventoryOpen()
		{
			return inventoryMutex.IsLocked();
		}

		private void grabItemFromPlayerInventory(Item item, Farmer who)
		{
			if (item.Stack == 0)
			{
				item.Stack = 1;
			}
			Item tmp = getFarmhand().Value.addItemToInventory(item);
			if (tmp == null)
			{
				who.removeItemFromInventory(item);
			}
			else
			{
				who.addItemToInventory(tmp);
			}
			int oldID = (Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : (-1);
			openFarmhandInventory();
			if (oldID != -1)
			{
				Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}

		private void grabItemFromFarmhandInventory(Item item, Farmer who)
		{
			if (who.couldInventoryAcceptThisItem(item))
			{
				getInventory().Remove(item);
				openFarmhandInventory();
			}
		}

		public override void updateWarps()
		{
			base.updateWarps();
			if (Game1.getFarm() != null)
			{
				foreach (Building building in Game1.getFarm().buildings)
				{
					if (building.indoors.Value == this)
					{
						building.updateInteriorWarps();
					}
				}
			}
		}

		public List<Item> demolish()
		{
			List<Item> items = new List<Item>(getInventory()).Where((Item item) => item != null).ToList();
			getInventory().Clear();
			Farmer.removeInitialTools(items);
			Dictionary<string, string> NPCDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			foreach (NPC npc in new List<NPC>(characters))
			{
				if (npc.isVillager() && NPCDispositions.ContainsKey(npc.Name))
				{
					npc.reloadDefaultLocation();
					npc.clearSchedule();
					Game1.warpCharacter(npc, npc.DefaultMap, npc.DefaultPosition / 64f);
				}
				if (npc is Pet)
				{
					(npc as Pet).warpToFarmHouse(Game1.MasterPlayer);
				}
			}
			Cellar cellar = Game1.getLocationFromName(GetCellarName()) as Cellar;
			if (cellar != null)
			{
				cellar.objects.Clear();
				cellar.setUpAgingBoards();
			}
			if (farmhand.Value != null)
			{
				Game1.player.team.DeleteFarmhand(farmhand.Value);
			}
			Game1.updateCellarAssignments();
			return items;
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			if (farmhand.Value != null)
			{
				farmhand.Value.stamina = farmhand.Value.maxStamina.Value;
			}
		}

		public override Point getPorchStandingSpot()
		{
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building.isCabin && this == building.indoors.Value)
				{
					return building.getPorchStandingSpot();
				}
			}
			return base.getPorchStandingSpot();
		}
	}
}
