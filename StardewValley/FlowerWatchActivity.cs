using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley
{
	public class FlowerWatchActivity : FarmActivity
	{
		protected override bool _AttemptActivity(Farm farm)
		{
			HoeDirt found_crop = GetRandomCrop(farm, (Crop crop) => crop.currentPhase.Value >= crop.phaseDays.Count - 1 && new Object(Vector2.Zero, crop.indexOfHarvest.Value, 1).category.Value == -80);
			if (found_crop != null)
			{
				activityPosition = found_crop.currentTileLocation;
				return true;
			}
			return false;
		}
	}
}
