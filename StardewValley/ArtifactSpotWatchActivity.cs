namespace StardewValley
{
	public class ArtifactSpotWatchActivity : FarmActivity
	{
		protected override bool _AttemptActivity(Farm farm)
		{
			Object found_object = GetRandomObject(farm, (Object o) => Utility.IsNormalObjectAtParentSheetIndex(o, 595));
			if (found_object != null)
			{
				activityPosition = GetNearbyTile(farm, found_object.TileLocation);
				return true;
			}
			return false;
		}
	}
}
