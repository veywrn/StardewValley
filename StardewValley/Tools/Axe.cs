using Microsoft.Xna.Framework;
using xTile.ObjectModel;

namespace StardewValley.Tools
{
	public class Axe : Tool
	{
		public const int StumpStrength = 4;

		private int stumpTileX;

		private int stumpTileY;

		private int hitsToStump;

		public Axe()
			: base("Axe", 0, 189, 215, stackable: false)
		{
			base.UpgradeLevel = 0;
		}

		public override Item getOne()
		{
			return new Axe
			{
				UpgradeLevel = base.UpgradeLevel
			};
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Axe.cs.1");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Axe.cs.14019");
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			Update(who.FacingDirection, 0, who);
			who.EndUsingTool();
			return true;
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			who.Stamina -= (float)(2 * power) - (float)who.ForagingLevel * 0.1f;
			int tileX = x / 64;
			int tileY = y / 64;
			Rectangle tileRect = new Rectangle(tileX * 64, tileY * 64, 64, 64);
			Vector2 tile = new Vector2(tileX, tileY);
			if (location.Map.GetLayer("Buildings").Tiles[tileX, tileY] != null)
			{
				PropertyValue value = null;
				location.Map.GetLayer("Buildings").Tiles[tileX, tileY].TileIndexProperties.TryGetValue("TreeStump", out value);
				if (value != null)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Axe.cs.14023"));
					return;
				}
			}
			location.performToolAction(this, tileX, tileY);
			if (location.terrainFeatures.ContainsKey(tile) && location.terrainFeatures[tile].performToolAction(this, 0, tile, location))
			{
				location.terrainFeatures.Remove(tile);
			}
			if (location.largeTerrainFeatures != null)
			{
				for (int i = location.largeTerrainFeatures.Count - 1; i >= 0; i--)
				{
					if (location.largeTerrainFeatures[i].getBoundingBox().Intersects(tileRect) && location.largeTerrainFeatures[i].performToolAction(this, 0, tile, location))
					{
						location.largeTerrainFeatures.RemoveAt(i);
					}
				}
			}
			Vector2 toolTilePosition = new Vector2(tileX, tileY);
			if (location.Objects.ContainsKey(toolTilePosition) && location.Objects[toolTilePosition].Type != null && location.Objects[toolTilePosition].performToolAction(this, location))
			{
				if (location.Objects[toolTilePosition].type.Equals("Crafting") && (int)location.Objects[toolTilePosition].fragility != 2)
				{
					location.debris.Add(new Debris(location.Objects[toolTilePosition].bigCraftable ? (-location.Objects[toolTilePosition].ParentSheetIndex) : location.Objects[toolTilePosition].ParentSheetIndex, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y)));
				}
				location.Objects[toolTilePosition].performRemoveAction(toolTilePosition, location);
				location.Objects.Remove(toolTilePosition);
			}
		}
	}
}
