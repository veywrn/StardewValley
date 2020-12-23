using Microsoft.Xna.Framework;

namespace StardewValley
{
	public class ShrineActivity : FarmActivity
	{
		public override bool AttemptActivity(Farm farm)
		{
			activityPosition = new Vector2(8f, 8f);
			activityDirection = 0;
			return true;
		}
	}
}
