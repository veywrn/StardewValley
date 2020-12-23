namespace StardewValley
{
	public class LocationRequest
	{
		public delegate void Callback();

		public string Name;

		public bool IsStructure;

		public GameLocation Location;

		public event Callback OnLoad;

		public event Callback OnWarp;

		public LocationRequest(string name, bool isStructure, GameLocation location)
		{
			Name = name;
			IsStructure = isStructure;
			Location = location;
		}

		public void Loaded(GameLocation location)
		{
			if (this.OnLoad != null)
			{
				this.OnLoad();
			}
		}

		public void Warped(GameLocation location)
		{
			if (this.OnWarp != null)
			{
				this.OnWarp();
			}
			Game1.player.ridingMineElevator = false;
			Game1.forceSnapOnNextViewportUpdate = true;
		}

		public bool IsRequestFor(GameLocation location)
		{
			if (!IsStructure && location.Name == Name)
			{
				return true;
			}
			if (location.uniqueName.Value == Name)
			{
				return location.isStructure;
			}
			return false;
		}

		public bool IsRequestFor(string name, bool isStructure)
		{
			if (!IsStructure)
			{
				return name == Name;
			}
			return name == Name && isStructure;
		}

		public override string ToString()
		{
			return "LocationRequest(" + Name + ", " + IsStructure.ToString() + ")";
		}
	}
}
