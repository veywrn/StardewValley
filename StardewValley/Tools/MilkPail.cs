using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley.Tools
{
	public class MilkPail : Tool
	{
		[XmlIgnore]
		private readonly NetEvent0 finishEvent = new NetEvent0();

		private FarmAnimal animal;

		public MilkPail()
			: base("Milk Pail", -1, 6, 6, stackable: false)
		{
		}

		public override Item getOne()
		{
			MilkPail pail = new MilkPail();
			CopyEnchantments(this, pail);
			pail._GetOneFrom(this);
			return pail;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14167");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14168");
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
			if (animal != null && (int)animal.currentProduce > 0 && (int)animal.age >= (byte)animal.ageWhenMature && animal.toolUsedForHarvest.Equals(base.BaseName) && who.couldInventoryAcceptThisObject(animal.currentProduce, 1))
			{
				animal.doEmote(20);
				animal.friendshipTowardFarmer.Value = Math.Min(1000, (int)animal.friendshipTowardFarmer + 5);
				who.currentLocation.localSound("Milking");
				animal.pauseTimer = 1500;
			}
			else if (animal != null && (int)animal.currentProduce > 0 && (int)animal.age >= (byte)animal.ageWhenMature)
			{
				if (who != null && Game1.player.Equals(who))
				{
					if (!animal.toolUsedForHarvest.Equals(base.BaseName))
					{
						if (!(animal.toolUsedForHarvest == null) && !animal.toolUsedForHarvest.Equals("null"))
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14167", animal.toolUsedForHarvest));
						}
					}
					else if (!who.couldInventoryAcceptThisObject(animal.currentProduce, 1))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					}
				}
			}
			else if (who != null && Game1.player.Equals(who))
			{
				DelayedAction.playSoundAfterDelay("fishingRodBend", 300);
				DelayedAction.playSoundAfterDelay("fishingRodBend", 1200);
				string toSay = "";
				if (animal != null && !animal.toolUsedForHarvest.Equals(base.BaseName))
				{
					toSay = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14175", animal.displayName);
				}
				if (animal != null && animal.isBaby() && animal.toolUsedForHarvest.Equals(base.BaseName))
				{
					toSay = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14176", animal.displayName);
				}
				if (animal != null && (int)animal.age >= (byte)animal.ageWhenMature && animal.toolUsedForHarvest.Equals(base.BaseName))
				{
					toSay = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14177", animal.displayName);
				}
				if (toSay.Length > 0)
				{
					DelayedAction.showDialogueAfterDelay(toSay, 1000);
				}
			}
			who.Halt();
			int g = who.FarmerSprite.CurrentFrame;
			who.FarmerSprite.animateOnce(287 + who.FacingDirection, 50f, 4);
			who.FarmerSprite.oldFrame = g;
			who.UsingTool = true;
			who.CanMove = false;
			return true;
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
			base.CurrentParentTileIndex = 6;
			base.IndexOfMenuItemView = 6;
			if (animal != null && (int)animal.currentProduce > 0 && (int)animal.age >= (byte)animal.ageWhenMature && animal.toolUsedForHarvest.Equals(base.BaseName) && who.addItemToInventoryBool(new Object(Vector2.Zero, animal.currentProduce, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
			{
				Quality = animal.produceQuality
			}))
			{
				Utility.RecordAnimalProduce(animal, animal.currentProduce);
				Game1.playSound("coin");
				animal.currentProduce.Value = -1;
				if ((bool)animal.showDifferentTextureWhenReadyForHarvest)
				{
					animal.Sprite.LoadTexture("Animals\\Sheared" + animal.type.Value);
				}
				who.gainExperience(0, 5);
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
