using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley.Tools
{
	public class Shears : Tool
	{
		[XmlIgnore]
		private readonly NetEvent0 finishEvent = new NetEvent0();

		private FarmAnimal animal;

		public Shears()
			: base("Shears", -1, 7, 7, stackable: false)
		{
		}

		public override Item getOne()
		{
			return new Shears();
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14240");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14241");
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(finishEvent);
			finishEvent.onEvent += doFinish;
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			x = (int)who.GetToolLocation().X;
			y = (int)who.GetToolLocation().Y;
			Rectangle r = new Rectangle(x - 32, y - 32, 64, 64);
			if (location is Farm)
			{
				animal = Utility.GetBestHarvestableFarmAnimal((location as Farm).animals.Values, this, r);
			}
			else if (location is AnimalHouse)
			{
				animal = Utility.GetBestHarvestableFarmAnimal((location as AnimalHouse).animals.Values, this, r);
			}
			who.Halt();
			int g = who.FarmerSprite.CurrentFrame;
			who.FarmerSprite.animateOnce(283 + who.FacingDirection, 50f, 4);
			who.FarmerSprite.oldFrame = g;
			who.UsingTool = true;
			who.CanMove = false;
			return true;
		}

		public static void playSnip(Farmer who)
		{
			who.currentLocation.playSound("scissors");
		}

		public override void tickUpdate(GameTime time, Farmer who)
		{
			lastUser = who;
			base.tickUpdate(time, who);
			finishEvent.Poll();
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			who.Stamina -= 4f;
			playSnip(who);
			base.CurrentParentTileIndex = 7;
			base.IndexOfMenuItemView = 7;
			if (animal != null && (int)animal.currentProduce > 0 && (int)animal.age >= (byte)animal.ageWhenMature && animal.toolUsedForHarvest.Equals(base.BaseName))
			{
				if (who.addItemToInventoryBool(new Object(Vector2.Zero, animal.currentProduce, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
				{
					Quality = animal.produceQuality
				}))
				{
					Utility.RecordAnimalProduce(animal, animal.currentProduce);
					animal.currentProduce.Value = -1;
					Game1.playSound("coin");
					animal.friendshipTowardFarmer.Value = Math.Min(1000, (int)animal.friendshipTowardFarmer + 5);
					if ((bool)animal.showDifferentTextureWhenReadyForHarvest)
					{
						animal.Sprite.LoadTexture("Animals\\Sheared" + animal.type.Value);
					}
					who.gainExperience(0, 5);
				}
			}
			else
			{
				string toSay = "";
				if (animal != null && !animal.toolUsedForHarvest.Equals(base.BaseName))
				{
					toSay = Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14245", animal.displayName);
				}
				if (animal != null && animal.isBaby() && animal.toolUsedForHarvest.Equals(base.BaseName))
				{
					toSay = Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14246", animal.displayName);
				}
				if (animal != null && (int)animal.age >= (byte)animal.ageWhenMature && animal.toolUsedForHarvest.Equals(base.BaseName))
				{
					toSay = Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14247", animal.displayName);
				}
				if (toSay.Length > 0)
				{
					Game1.drawObjectDialogue(toSay);
				}
			}
			finish();
		}

		private void finish()
		{
			finishEvent.Fire();
		}

		private void doFinish()
		{
			animal = null;
			lastUser.CanMove = true;
			lastUser.completelyStopAnimatingOrDoingAction();
			lastUser.UsingTool = false;
			lastUser.canReleaseTool = true;
		}
	}
}
