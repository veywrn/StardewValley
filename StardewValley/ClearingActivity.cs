using Microsoft.Xna.Framework;
using xTile.Dimensions;

namespace StardewValley
{
	public class ClearingActivity : FarmActivity
	{
		protected override bool _AttemptActivity(Farm farm)
		{
			Microsoft.Xna.Framework.Rectangle map_bounds = new Microsoft.Xna.Framework.Rectangle(0, 0, farm.map.Layers[0].LayerWidth, farm.map.Layers[0].LayerHeight);
			for (int i = 0; i < 5; i++)
			{
				Vector2 random_point = Utility.getRandomPositionInThisRectangle(map_bounds, Game1.random);
				random_point.X = (int)random_point.X;
				random_point.Y = (int)random_point.Y;
				Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle((int)random_point.X, (int)random_point.Y, 1, 1);
				if (!farm.isTileLocationTotallyClearAndPlaceableIgnoreFloors(random_point))
				{
					continue;
				}
				rect.Inflate(1, 1);
				bool fail = false;
				for (int x = rect.Left; x < rect.Right; x++)
				{
					for (int y = rect.Top; y < rect.Bottom; y++)
					{
						if ((float)x != random_point.X || (float)y != random_point.Y)
						{
							if (!farm.isTileOnMap(new Vector2(x, y)))
							{
								fail = true;
								break;
							}
							if (farm.isTileOccupiedIgnoreFloors(new Vector2(x, y)))
							{
								fail = true;
								break;
							}
							if (!farm.isTilePassable(new Location(x, y), Game1.viewport))
							{
								fail = true;
								break;
							}
							if (farm.getBuildingAt(new Vector2(x, y)) != null)
							{
								fail = true;
								break;
							}
						}
					}
					if (fail)
					{
						break;
					}
				}
				if (!fail)
				{
					activityPosition = random_point;
					activityDirection = 2;
					return true;
				}
			}
			return false;
		}

		protected override void _BeginActivity()
		{
			if (_character.Name == "Haley" && Game1.random.NextDouble() <= 0.5)
			{
				_character.StartActivityRouteEndBehavior("haley_photo", "");
			}
			else
			{
				_character.StartActivityWalkInSquare(2, 2, 0);
			}
		}

		protected override bool _Update(GameTime time)
		{
			if ((double)_age > 5.0)
			{
				if (!_character.IsReturningToEndPoint())
				{
					_character.EndActivityRouteEndBehavior();
				}
				if (!_character.IsWalkingInSquare)
				{
					return true;
				}
			}
			return false;
		}

		protected override void _EndActivity()
		{
		}
	}
}
