using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley
{
	public class TreeActivity : FarmActivity
	{
		protected override bool _AttemptActivity(Farm farm)
		{
			TerrainFeature tree = GetRandomTerrainFeature(farm, (TerrainFeature feature) => (feature is Tree && (feature as Tree).growthStage.Value >= 5) || (feature is FruitTree && (feature as FruitTree).growthStage.Value >= 4));
			if (tree != null)
			{
				Vector2 target_tile = tree.currentTileLocation + new Vector2(0f, 1f);
				Rectangle rect = new Rectangle((int)tree.currentTileLocation.X, (int)tree.currentTileLocation.Y, 1, 1);
				if (!farm.isTileLocationTotallyClearAndPlaceableIgnoreFloors(target_tile))
				{
					return false;
				}
				rect.Inflate(2, 2);
				for (int x = rect.Left; x < rect.Right; x++)
				{
					for (int y = rect.Top; y < rect.Bottom; y++)
					{
						if ((float)x == tree.currentTileLocation.X && (float)y == tree.currentTileLocation.Y)
						{
							continue;
						}
						Object tile_object = farm.getObjectAtTile(x, y);
						if (tile_object != null)
						{
							if (tile_object.Name.Equals("Weeds"))
							{
								return false;
							}
							if (tile_object.Name.Equals("Stone"))
							{
								return false;
							}
						}
						if (farm.terrainFeatures.ContainsKey(new Vector2(x, y)))
						{
							TerrainFeature feature2 = farm.terrainFeatures[new Vector2(x, y)];
							if (feature2 is Tree || feature2 is FruitTree)
							{
								return false;
							}
						}
					}
				}
				activityPosition = target_tile;
				activityDirection = 2;
				return true;
			}
			return false;
		}

		protected override void _BeginActivity()
		{
			if (_character.Name == "Haley")
			{
				_character.StartActivityRouteEndBehavior("haley_photo", "");
			}
			else if (_character.Name == "Penny")
			{
				_character.StartActivityRouteEndBehavior("penny_read", "");
			}
			else if (_character.Name == "Leah")
			{
				_character.StartActivityRouteEndBehavior("leah_draw", "");
			}
		}

		protected override void _EndActivity()
		{
			_character.EndActivityRouteEndBehavior();
		}
	}
}
