using Microsoft.Xna.Framework;
using StardewValley.Menus;
using System.Collections.Generic;

namespace StardewValley.Tools
{
	public class MagnifyingGlass : Tool
	{
		public MagnifyingGlass()
			: base("Magnifying Glass", -1, 5, 5, stackable: false)
		{
			base.InstantUse = true;
		}

		public override Item getOne()
		{
			return new MagnifyingGlass();
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:MagnifyingGlass.cs.14119");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:MagnifyingGlass.cs.14120");
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			who.Halt();
			who.canMove = true;
			who.UsingTool = false;
			DoFunction(location, Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y, 0, who);
			return true;
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			base.CurrentParentTileIndex = 5;
			base.IndexOfMenuItemView = 5;
			Rectangle tileRect = new Rectangle(x / 64 * 64, y / 64 * 64, 64, 64);
			if (location is Farm)
			{
				foreach (KeyValuePair<long, FarmAnimal> a2 in (location as Farm).animals.Pairs)
				{
					if (a2.Value.GetBoundingBox().Intersects(tileRect))
					{
						Game1.activeClickableMenu = new AnimalQueryMenu(a2.Value);
						break;
					}
				}
			}
			else if (location is AnimalHouse)
			{
				foreach (KeyValuePair<long, FarmAnimal> a in (location as AnimalHouse).animals.Pairs)
				{
					if (a.Value.GetBoundingBox().Intersects(tileRect))
					{
						Game1.activeClickableMenu = new AnimalQueryMenu(a.Value);
						break;
					}
				}
			}
		}
	}
}
