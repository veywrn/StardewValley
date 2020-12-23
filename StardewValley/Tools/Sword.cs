using Microsoft.Xna.Framework;
using System;

namespace StardewValley.Tools
{
	public class Sword : Tool
	{
		public const double baseCritChance = 0.02;

		public int whichUpgrade;

		public Sword()
		{
		}

		public Sword(string name, int spriteIndex)
			: base(name, 0, spriteIndex, spriteIndex, stackable: false)
		{
		}

		public override Item getOne()
		{
			Sword sword = new Sword(base.BaseName, base.InitialParentTileIndex);
			sword._GetOneFrom(this);
			return sword;
		}

		public void DoFunction(GameLocation location, int x, int y, int facingDirection, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			Vector2 tileLocation3 = Vector2.Zero;
			Vector2 tileLocation2 = Vector2.Zero;
			Rectangle areaOfEffect = Rectangle.Empty;
			Rectangle playerBoundingBox = who.GetBoundingBox();
			switch (facingDirection)
			{
			case 0:
				areaOfEffect = new Rectangle(x - 64, playerBoundingBox.Y - 64, 128, 64);
				tileLocation3 = new Vector2(((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Left : areaOfEffect.Right) / 64, areaOfEffect.Top / 64);
				tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Top / 64);
				break;
			case 2:
				areaOfEffect = new Rectangle(x - 64, playerBoundingBox.Bottom, 128, 64);
				tileLocation3 = new Vector2(((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Left : areaOfEffect.Right) / 64, areaOfEffect.Center.Y / 64);
				tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
				break;
			case 1:
				areaOfEffect = new Rectangle(playerBoundingBox.Right, y - 64, 64, 128);
				tileLocation3 = new Vector2(areaOfEffect.Center.X / 64, ((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Top : areaOfEffect.Bottom) / 64);
				tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
				break;
			case 3:
				areaOfEffect = new Rectangle(playerBoundingBox.Left - 64, y - 64, 64, 128);
				tileLocation3 = new Vector2(areaOfEffect.Left / 64, ((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Top : areaOfEffect.Bottom) / 64);
				tileLocation2 = new Vector2(areaOfEffect.Left / 64, areaOfEffect.Center.Y / 64);
				break;
			}
			int minDamage = ((whichUpgrade == 2) ? 3 : ((whichUpgrade == 4) ? 6 : whichUpgrade)) + 1;
			int maxDamage = 4 * (((whichUpgrade == 2) ? 3 : ((whichUpgrade == 4) ? 5 : whichUpgrade)) + 1);
			bool dontdestroyObjects = location.damageMonster(areaOfEffect, minDamage, maxDamage, isBomb: false, who);
			if (whichUpgrade == 4 && !dontdestroyObjects)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(352, Game1.random.Next(50, 120), 2, 1, new Vector2(areaOfEffect.Center.X - 32, areaOfEffect.Center.Y - 32) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
			}
			string soundToPlay = "";
			if (!dontdestroyObjects)
			{
				if (location.objects.ContainsKey(tileLocation3) && !location.Objects[tileLocation3].Name.Contains("Stone") && !location.Objects[tileLocation3].Name.Contains("Stick") && !location.Objects[tileLocation3].Name.Contains("Stump") && !location.Objects[tileLocation3].Name.Contains("Boulder") && !location.Objects[tileLocation3].Name.Contains("Lumber") && !location.Objects[tileLocation3].IsHoeDirt)
				{
					if (location.Objects[tileLocation3].Name.Contains("Weed"))
					{
						if (!(who.Stamina > 0f))
						{
							return;
						}
						Game1.stats.WeedsEliminated++;
						checkWeedForTreasure(tileLocation3, who);
						int category = location.Objects[tileLocation3].Category;
						soundToPlay = ((category != -2) ? "cut" : "stoneCrack");
						location.removeObject(tileLocation3, showDestroyedObject: true);
					}
					else
					{
						location.objects[tileLocation3].performToolAction(this, location);
					}
				}
				if (location.objects.ContainsKey(tileLocation2) && !location.Objects[tileLocation2].Name.Contains("Stone") && !location.Objects[tileLocation2].Name.Contains("Stick") && !location.Objects[tileLocation2].Name.Contains("Stump") && !location.Objects[tileLocation2].Name.Contains("Boulder") && !location.Objects[tileLocation2].Name.Contains("Lumber") && !location.Objects[tileLocation2].IsHoeDirt)
				{
					if (location.Objects[tileLocation2].Name.Contains("Weed"))
					{
						if (!(who.Stamina > 0f))
						{
							return;
						}
						Game1.stats.WeedsEliminated++;
						checkWeedForTreasure(tileLocation2, who);
					}
					else
					{
						location.objects[tileLocation2].performToolAction(this, location);
					}
				}
			}
			bool success2 = false;
			foreach (Vector2 v in Utility.getListOfTileLocationsForBordersOfNonTileRectangle(areaOfEffect))
			{
				if (location.terrainFeatures.ContainsKey(v) && location.terrainFeatures[v].performToolAction(this, 0, v, location))
				{
					location.terrainFeatures.Remove(v);
					success2 = true;
				}
			}
			if (!soundToPlay.Equals(""))
			{
				Game1.playSound(soundToPlay);
			}
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
		}

		public void checkWeedForTreasure(Vector2 tileLocation, Farmer who)
		{
			Random r = new Random((int)((float)(double)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed) + tileLocation.X * 13f + tileLocation.Y * 29f));
			if (r.NextDouble() < 0.07)
			{
				Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, r.Next(1, 3));
			}
			else if (r.NextDouble() < 0.02 + (double)who.LuckLevel / 10.0)
			{
				Game1.createDebris((r.NextDouble() < 0.5) ? 4 : 8, (int)tileLocation.X, (int)tileLocation.Y, r.Next(1, 4));
			}
			else if (r.NextDouble() < 0.006 + (double)who.LuckLevel / 20.0)
			{
				Game1.createObjectDebris(114, (int)tileLocation.X, (int)tileLocation.Y);
			}
		}

		protected override string loadDisplayName()
		{
			if (Name.Equals("Battered Sword"))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1205");
			}
			switch (whichUpgrade)
			{
			default:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14290");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14292");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14294");
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14296");
			}
		}

		protected override string loadDescription()
		{
			switch (whichUpgrade)
			{
			default:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1206");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14291");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14293");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14295");
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14297");
			}
		}

		public void upgrade(int which)
		{
			if (which > whichUpgrade)
			{
				whichUpgrade = which;
				switch (which)
				{
				case 1:
					Name = "Hero's Sword";
					base.IndexOfMenuItemView = 68;
					break;
				case 2:
					Name = "Holy Sword";
					base.IndexOfMenuItemView = 70;
					break;
				case 3:
					Name = "Dark Sword";
					base.IndexOfMenuItemView = 69;
					break;
				case 4:
					Name = "Galaxy Sword";
					base.IndexOfMenuItemView = 71;
					break;
				}
				displayName = null;
				base.description = null;
				base.UpgradeLevel = which;
			}
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
		}
	}
}
