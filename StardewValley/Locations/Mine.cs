namespace StardewValley.Locations
{
	public class Mine : GameLocation
	{
		public Mine()
		{
		}

		public Mine(string map, string name)
			: base(map, name)
		{
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			MineShaft.mushroomLevelsGeneratedToday.Clear();
		}
	}
}
