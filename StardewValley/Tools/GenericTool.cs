namespace StardewValley.Tools
{
	public class GenericTool : Tool
	{
		public new string description;

		public GenericTool()
		{
		}

		public GenericTool(string name, string description, int upgradeLevel, int parentSheetIndex, int menuViewIndex)
			: base(name, upgradeLevel, parentSheetIndex, menuViewIndex, stackable: false)
		{
			this.description = description;
		}

		public override Item getOne()
		{
			GenericTool genericTool = new GenericTool(base.BaseName, description, base.UpgradeLevel, base.InitialParentTileIndex, base.IndexOfMenuItemView);
			genericTool._GetOneFrom(this);
			return genericTool;
		}

		protected override string loadDescription()
		{
			return description;
		}

		protected override string loadDisplayName()
		{
			return base.BaseName;
		}
	}
}
