namespace StardewValley.Tools
{
	public class Seeds : Stackable
	{
		private string seedType;

		private int numberInStack;

		public new int NumberInStack
		{
			get
			{
				return numberInStack;
			}
			set
			{
				numberInStack = value;
			}
		}

		public string SeedType
		{
			get
			{
				return seedType;
			}
			set
			{
				seedType = value;
			}
		}

		public Seeds()
		{
		}

		public Seeds(string seedType, int numberInStack)
			: base("Seeds", 0, 0, 0, stackable: true)
		{
			this.seedType = seedType;
			this.numberInStack = numberInStack;
			setCurrentTileIndexToSeedType();
			base.IndexOfMenuItemView = base.CurrentParentTileIndex;
		}

		public override Item getOne()
		{
			return new Seeds(SeedType, 1);
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Seeds.cs.14209");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Seeds.cs.14210");
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			who.Stamina -= 2f - (float)who.FarmingLevel * 0.1f;
			numberInStack--;
			setCurrentTileIndexToSeedType();
			location.playSound("seeds");
		}

		private void setCurrentTileIndexToSeedType()
		{
			switch (seedType)
			{
			case "Parsnip":
				base.CurrentParentTileIndex = 0;
				break;
			case "Green Bean":
				base.CurrentParentTileIndex = 1;
				break;
			case "Cauliflower":
				base.CurrentParentTileIndex = 2;
				break;
			case "Potato":
				base.CurrentParentTileIndex = 3;
				break;
			case "Garlic":
				base.CurrentParentTileIndex = 4;
				break;
			case "Kale":
				base.CurrentParentTileIndex = 5;
				break;
			case "Rhubarb":
				base.CurrentParentTileIndex = 6;
				break;
			case "Melon":
				base.CurrentParentTileIndex = 7;
				break;
			case "Tomato":
				base.CurrentParentTileIndex = 8;
				break;
			case "Blueberry":
				base.CurrentParentTileIndex = 9;
				break;
			case "Yellow Pepper":
				base.CurrentParentTileIndex = 10;
				break;
			case "Wheat":
				base.CurrentParentTileIndex = 11;
				break;
			case "Radish":
				base.CurrentParentTileIndex = 12;
				break;
			case "Red Cabbage":
				base.CurrentParentTileIndex = 13;
				break;
			case "Starfruit":
				base.CurrentParentTileIndex = 14;
				break;
			case "Corn":
				base.CurrentParentTileIndex = 15;
				break;
			case "Eggplant":
				base.CurrentParentTileIndex = 56;
				break;
			case "Artichoke":
				base.CurrentParentTileIndex = 57;
				break;
			case "Pumpkin":
				base.CurrentParentTileIndex = 58;
				break;
			case "Bok Choy":
				base.CurrentParentTileIndex = 59;
				break;
			case "Yam":
				base.CurrentParentTileIndex = 60;
				break;
			case "Cranberries":
				base.CurrentParentTileIndex = 61;
				break;
			case "Beet":
				base.CurrentParentTileIndex = 62;
				break;
			case "Spring Mix":
				base.CurrentParentTileIndex = 63;
				break;
			case "Summer Mix":
				base.CurrentParentTileIndex = 64;
				break;
			case "Fall Mix":
				base.CurrentParentTileIndex = 65;
				break;
			case "Winter Mix":
				base.CurrentParentTileIndex = 66;
				break;
			case "Ancient Fruit":
				base.CurrentParentTileIndex = 72;
				break;
			}
		}
	}
}
