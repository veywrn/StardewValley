using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace StardewValley.Tools
{
	public class Blueprints : Tool
	{
		public Blueprints()
			: base("Farmer's Catalogue", 0, 75, 75, stackable: false)
		{
			base.UpgradeLevel = 0;
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
			base.InstantUse = true;
		}

		public override Item getOne()
		{
			Blueprints blueprints = new Blueprints();
			blueprints._GetOneFrom(this);
			return blueprints;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Blueprints.cs.14039");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Blueprints.cs.14040");
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new BlueprintsMenu(Game1.viewport.Width / 2 - (Game1.viewport.Width / 2 + 96) / 2, Game1.viewport.Height / 4);
				((BlueprintsMenu)Game1.activeClickableMenu).changePosition(((BlueprintsMenu)Game1.activeClickableMenu).xPositionOnScreen, Game1.viewport.Height / 2 - ((BlueprintsMenu)Game1.activeClickableMenu).height / 2);
			}
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
		}

		public override void tickUpdate(GameTime time, Farmer who)
		{
		}
	}
}
