using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Util;
using System;
using System.Xml.Serialization;

namespace StardewValley.Buildings
{
	public class Barn : Building
	{
		public static int openAnimalDoorPosition = -76;

		private const int closedAnimalDoorPosition = 0;

		[XmlElement("yPositionOfAnimalDoor")]
		private readonly NetInt yPositionOfAnimalDoor = new NetInt();

		[XmlElement("animalDoorMotion")]
		private readonly NetInt animalDoorMotion = new NetInt();

		public Barn(BluePrint b, Vector2 tileLocation)
			: base(b, tileLocation)
		{
		}

		public Barn()
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(yPositionOfAnimalDoor, animalDoorMotion);
		}

		protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
		{
			GameLocation indoors = new AnimalHouse("Maps\\" + nameOfIndoorsWithoutUnique, buildingType);
			indoors.IsFarm = true;
			indoors.isStructure.Value = true;
			indoors.uniqueName.Value = nameOfIndoorsWithoutUnique + StardewValley.Util.GuidHelper.NewGuid().ToString();
			if (!(nameOfIndoorsWithoutUnique == "Barn2"))
			{
				if (nameOfIndoorsWithoutUnique == "Barn3")
				{
					(indoors as AnimalHouse).animalLimit.Value = 12;
				}
			}
			else
			{
				(indoors as AnimalHouse).animalLimit.Value = 8;
			}
			foreach (Warp warp in indoors.warps)
			{
				warp.TargetX = humanDoor.X + (int)tileX;
				warp.TargetY = humanDoor.Y + (int)tileY + 1;
			}
			if ((bool)animalDoorOpen)
			{
				yPositionOfAnimalDoor.Value = openAnimalDoorPosition;
			}
			return indoors;
		}

		public override Rectangle getRectForAnimalDoor()
		{
			Point animalDoor = base.animalDoor.Get();
			return new Rectangle((animalDoor.X + (int)tileX) * 64, ((int)tileY + animalDoor.Y) * 64, 128, 64);
		}

		public override bool doAction(Vector2 tileLocation, Farmer who)
		{
			if ((int)daysOfConstructionLeft <= 0 && (tileLocation.X == (float)((int)tileX + animalDoor.X) || tileLocation.X == (float)((int)tileX + animalDoor.X + 1)) && tileLocation.Y == (float)((int)tileY + animalDoor.Y))
			{
				if (!animalDoorOpen)
				{
					who.currentLocation.playSound("doorCreak");
				}
				else
				{
					who.currentLocation.playSound("doorCreakReverse");
				}
				animalDoorOpen.Value = !animalDoorOpen;
				animalDoorMotion.Value = (animalDoorOpen ? (-3) : 2);
				return true;
			}
			return base.doAction(tileLocation, who);
		}

		public override void updateWhenFarmNotCurrentLocation(GameTime time)
		{
			base.updateWhenFarmNotCurrentLocation(time);
			((AnimalHouse)(GameLocation)indoors).updateWhenNotCurrentLocation(this, time);
		}

		public override Rectangle getSourceRectForMenu()
		{
			return new Rectangle(0, 0, texture.Value.Bounds.Width, texture.Value.Bounds.Height - 16);
		}

		public override void performActionOnUpgrade(GameLocation location)
		{
			(indoors.Value as AnimalHouse).animalLimit.Value += 4;
			if ((int)(indoors.Value as AnimalHouse).animalLimit == 8)
			{
				Object o = new Object(new Vector2(1f, 3f), 104);
				o.fragility.Value = 2;
				indoors.Value.objects.Add(new Vector2(1f, 3f), o);
			}
		}

		public override void performActionOnConstruction(GameLocation location)
		{
			base.performActionOnConstruction(location);
			Object o = new Object(new Vector2(6f, 3f), 99);
			o.fragility.Value = 2;
			indoors.Value.objects.Add(new Vector2(6f, 3f), o);
			daysOfConstructionLeft.Value = 3;
		}

		public override void dayUpdate(int dayOfMonth)
		{
			base.dayUpdate(dayOfMonth);
			if ((int)daysOfConstructionLeft > 0)
			{
				return;
			}
			currentOccupants.Value = ((AnimalHouse)(GameLocation)indoors).animals.Count();
			if ((int)(indoors.Value as AnimalHouse).animalLimit != 16)
			{
				return;
			}
			int num = (indoors.Value as AnimalHouse).animals.Count();
			int numExistingHay = indoors.Value.numberOfObjectsWithName("Hay");
			int piecesHay = Math.Min(num - numExistingHay, (Game1.getLocationFromName("Farm") as Farm).piecesOfHay);
			(Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value -= piecesHay;
			for (int i = 0; i < 16; i++)
			{
				if (piecesHay <= 0)
				{
					break;
				}
				Vector2 tile = new Vector2(8 + i, 3f);
				if (!indoors.Value.objects.ContainsKey(tile))
				{
					indoors.Value.objects.Add(tile, new Object(178, 1));
				}
				piecesHay--;
			}
		}

		public override void Update(GameTime time)
		{
			base.Update(time);
			if ((int)animalDoorMotion != 0)
			{
				if ((bool)animalDoorOpen && (int)yPositionOfAnimalDoor <= openAnimalDoorPosition)
				{
					animalDoorMotion.Value = 0;
					yPositionOfAnimalDoor.Value = openAnimalDoorPosition;
				}
				else if (!animalDoorOpen && (int)yPositionOfAnimalDoor >= 0)
				{
					animalDoorMotion.Value = 0;
					yPositionOfAnimalDoor.Value = 0;
				}
				yPositionOfAnimalDoor.Value += animalDoorMotion;
			}
		}

		public override void upgrade()
		{
			if (buildingType.Equals("Big Barn"))
			{
				animalDoor.X++;
				indoors.Value.moveObject(15, 3, 18, 13);
				indoors.Value.moveObject(16, 3, 19, 13);
				indoors.Value.moveObject(1, 4, 20, 3);
				for (int j = 4; j < 13; j++)
				{
					indoors.Value.moveObject(16, j, 20, j);
				}
			}
			else
			{
				indoors.Value.moveObject(20, 3, 1, 4);
				for (int i = 6; i < 12; i++)
				{
					indoors.Value.moveObject(20, i, 23, i);
				}
				indoors.Value.moveObject(20, 4, 20, 13);
				indoors.Value.moveObject(20, 5, 21, 13);
				indoors.Value.moveObject(20, 12, 22, 13);
			}
		}

		public override Vector2 getUpgradeSignLocation()
		{
			return new Vector2((int)tileX, (int)tileY + 1) * 64f + new Vector2(192f, 4f);
		}

		public override void drawInMenu(SpriteBatch b, int x, int y)
		{
			drawShadow(b, x, y);
			b.Draw(texture.Value, new Vector2(x, y) + new Vector2(animalDoor.X, animalDoor.Y + 3) * 64f, new Rectangle(64, 112, 32, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.888f);
			b.Draw(texture.Value, new Vector2(x, y) + new Vector2(animalDoor.X, (float)animalDoor.Y + 2.25f) * 64f, new Rectangle(0, 112, 32, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh - 1) * 64) / 10000f - 1E-07f);
			b.Draw(texture.Value, new Vector2(x, y), new Rectangle(0, 0, 112, 112), color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.89f);
		}

		public override void draw(SpriteBatch b)
		{
			if (base.isMoving)
			{
				return;
			}
			if ((int)daysOfConstructionLeft > 0)
			{
				drawInConstruction(b);
				return;
			}
			drawShadow(b);
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX + animalDoor.X, (int)tileY + animalDoor.Y - 1) * 64f), new Rectangle(32, 112, 32, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX + animalDoor.X, (int)tileY + animalDoor.Y) * 64f), new Rectangle(64, 112, 32, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(((int)tileX + animalDoor.X) * 64, ((int)tileY + animalDoor.Y) * 64 + (int)yPositionOfAnimalDoor - 48)), new Rectangle(0, 112, 32, 12), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f - 0.0001f);
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(((int)tileX + animalDoor.X) * 64, ((int)tileY + animalDoor.Y) * 64 + (int)yPositionOfAnimalDoor)), new Rectangle(0, 112, 32, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f - 0.0001f);
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), new Rectangle(0, 0, 112, 112), color.Value * alpha, 0f, new Vector2(0f, 112f), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f - 1E-05f);
			if ((int)daysUntilUpgrade > 0)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, getUpgradeSignLocation()), new Rectangle(367, 309, 16, 15), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
			}
		}
	}
}
