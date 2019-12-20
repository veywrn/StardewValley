namespace StardewValley.Tools
{
	public class ToolFactory
	{
		public const byte axe = 0;

		public const byte hoe = 1;

		public const byte fishingRod = 2;

		public const byte pickAxe = 3;

		public const byte wateringCan = 4;

		public const byte meleeWeapon = 5;

		public const byte slingshot = 6;

		public static ToolDescription getIndexFromTool(Tool t)
		{
			if (t is Axe)
			{
				return new ToolDescription(0, (byte)(int)t.upgradeLevel);
			}
			if (t is Hoe)
			{
				return new ToolDescription(1, (byte)(int)t.upgradeLevel);
			}
			if (t is FishingRod)
			{
				return new ToolDescription(2, (byte)(int)t.upgradeLevel);
			}
			if (t is Pickaxe)
			{
				return new ToolDescription(3, (byte)(int)t.upgradeLevel);
			}
			if (t is WateringCan)
			{
				return new ToolDescription(4, (byte)(int)t.upgradeLevel);
			}
			if (t is MeleeWeapon)
			{
				return new ToolDescription(5, (byte)(int)t.upgradeLevel);
			}
			if (t is Slingshot)
			{
				return new ToolDescription(6, (byte)(int)t.upgradeLevel);
			}
			return new ToolDescription(0, 0);
		}

		public static Tool getToolFromDescription(byte index, int upgradeLevel)
		{
			Tool t = null;
			switch (index)
			{
			case 0:
				t = new Axe();
				break;
			case 1:
				t = new Hoe();
				break;
			case 2:
				t = new FishingRod();
				break;
			case 3:
				t = new Pickaxe();
				break;
			case 4:
				t = new WateringCan();
				break;
			case 5:
				t = new MeleeWeapon(0, upgradeLevel);
				break;
			case 6:
				t = new Slingshot();
				break;
			}
			t.UpgradeLevel = upgradeLevel;
			return t;
		}
	}
}
