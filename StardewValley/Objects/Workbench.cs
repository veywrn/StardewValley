using Microsoft.Xna.Framework;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class Workbench : Object
	{
		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(mutex.NetFields);
		}

		public Workbench()
		{
		}

		public Workbench(Vector2 position)
			: base(position, 208)
		{
			Name = "Workbench";
			type.Value = "Crafting";
			bigCraftable.Value = true;
			canBeSetDown.Value = true;
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			List<Chest> nearby_chests = new List<Chest>();
			Vector2[] neighbor_tiles = new Vector2[8]
			{
				new Vector2(-1f, 1f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(-1f, 0f),
				new Vector2(1f, 0f),
				new Vector2(-1f, -1f),
				new Vector2(0f, -1f),
				new Vector2(1f, -1f)
			};
			for (int i = 0; i < neighbor_tiles.Length; i++)
			{
				if (who.currentLocation is FarmHouse && who.currentLocation.getTileIndexAt((int)(tileLocation.X + neighbor_tiles[i].X), (int)(tileLocation.Y + neighbor_tiles[i].Y), "Buildings") == 173)
				{
					nearby_chests.Add((who.currentLocation as FarmHouse).fridge.Value);
					continue;
				}
				Vector2 tile_location = new Vector2((int)(tileLocation.X + neighbor_tiles[i].X), (int)(tileLocation.Y + neighbor_tiles[i].Y));
				Object neighbor_object = null;
				if (who.currentLocation.objects.ContainsKey(tile_location))
				{
					neighbor_object = who.currentLocation.objects[tile_location];
				}
				if (neighbor_object != null && neighbor_object is Chest)
				{
					nearby_chests.Add(neighbor_object as Chest);
				}
			}
			List<NetMutex> muticies = new List<NetMutex>();
			foreach (Chest chest in nearby_chests)
			{
				muticies.Add(chest.mutex);
			}
			if (!mutex.IsLocked())
			{
				MultipleMutexRequest multipleMutexRequest = null;
				multipleMutexRequest = new MultipleMutexRequest(muticies, delegate
				{
					mutex.RequestLock(delegate
					{
						Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
						Game1.activeClickableMenu = new CraftingPage((int)topLeftPositionForCenteringOnScreen.X, (int)topLeftPositionForCenteringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, cooking: false, standalone_menu: true, nearby_chests);
						Game1.activeClickableMenu.exitFunction = delegate
						{
							mutex.ReleaseLock();
							multipleMutexRequest.ReleaseLocks();
						};
					}, delegate
					{
						multipleMutexRequest.ReleaseLocks();
					});
				}, delegate
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
				});
			}
			return true;
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			mutex.Update(environment);
			base.updateWhenCurrentLocation(time, environment);
		}
	}
}
