using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace StardewValley.Locations
{
	public class Cellar : GameLocation
	{
		public readonly NetLong ownerUID = new NetLong(0L);

		public Cellar()
		{
		}

		public Cellar(string mapPath, string name)
			: base(mapPath, name)
		{
			setUpAgingBoards();
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
		}

		public void setUpAgingBoards()
		{
			for (int i = 6; i < 17; i++)
			{
				Vector2 v3 = new Vector2(i, 8f);
				if (!objects.ContainsKey(v3))
				{
					objects.Add(v3, new Cask(v3));
				}
				v3 = new Vector2(i, 10f);
				if (!objects.ContainsKey(v3))
				{
					objects.Add(v3, new Cask(v3));
				}
				v3 = new Vector2(i, 12f);
				if (!objects.ContainsKey(v3))
				{
					objects.Add(v3, new Cask(v3));
				}
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			string target = "Farmhouse";
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building.indoors.Value != null && building.indoors.Value is Cabin)
				{
					Cabin cabin = building.indoors.Value as Cabin;
					if (cabin.GetCellarName() == base.Name)
					{
						target = cabin.NameOrUniqueName;
						break;
					}
				}
			}
			foreach (Warp warp in warps)
			{
				warp.TargetName = target;
			}
		}

		public override void updateWarps()
		{
			base.updateWarps();
		}
	}
}
