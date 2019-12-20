using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using System.Collections.Generic;

namespace StardewValley
{
	public class Shed : DecoratableLocation
	{
		public readonly NetInt upgradeLevel = new NetInt(0);

		private bool isRobinUpgrading;

		public Shed()
		{
		}

		public Shed(string m, string name)
			: base(m, name)
		{
			List<Rectangle> rooms2 = getWalls();
			wallPaper.SetCountAtLeast(rooms2.Count);
			rooms2 = getFloors();
			floor.SetCountAtLeast(rooms2.Count);
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(upgradeLevel);
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.isDarkOut())
			{
				Game1.ambientLight = new Color(180, 180, 0);
			}
			if (Game1.getFarm().isThereABuildingUnderConstruction() && Game1.getFarm().getBuildingUnderConstruction().indoors.Value != null && Game1.getFarm().getBuildingUnderConstruction().indoors.Value.Equals(this))
			{
				isRobinUpgrading = true;
			}
			else
			{
				isRobinUpgrading = false;
			}
		}

		public Building getBuilding()
		{
			foreach (Building b in Game1.getFarm().buildings)
			{
				if (b.indoors.Value != null && b.indoors.Value.Equals(this))
				{
					return b;
				}
			}
			return null;
		}

		public virtual void setUpgradeLevel(int upgrade_level)
		{
			upgradeLevel.Set(upgrade_level);
			updateMap();
			updateLayout();
		}

		public void updateLayout()
		{
			updateDoors();
			updateWarps();
			setWallpapers();
			setFloors();
		}

		public override List<Rectangle> getWalls()
		{
			if ((int)upgradeLevel > 0)
			{
				return new List<Rectangle>
				{
					new Rectangle(1, 1, 17, 3)
				};
			}
			return base.getWalls();
		}

		public override List<Rectangle> getFloors()
		{
			if ((int)upgradeLevel > 0)
			{
				return new List<Rectangle>
				{
					new Rectangle(1, 3, 17, 14)
				};
			}
			return base.getFloors();
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (isRobinUpgrading)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(64f, 64f)), new Rectangle(90, 0, 33, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01546f);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(64f, 84f)), new Rectangle(90, 0, 33, 31), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0153600005f);
			}
		}
	}
}
