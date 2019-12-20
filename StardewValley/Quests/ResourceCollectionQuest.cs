using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	public class ResourceCollectionQuest : Quest
	{
		[XmlElement("target")]
		public readonly NetString target = new NetString();

		[XmlElement("targetMessage")]
		public readonly NetString targetMessage = new NetString();

		[XmlElement("numberCollected")]
		public readonly NetInt numberCollected = new NetInt();

		[XmlElement("number")]
		public readonly NetInt number = new NetInt();

		[XmlElement("reward")]
		public readonly NetInt reward = new NetInt();

		[XmlElement("resource")]
		public readonly NetInt resource = new NetInt();

		[XmlElement("deliveryItem")]
		public readonly NetRef<Object> deliveryItem = new NetRef<Object>();

		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

		public ResourceCollectionQuest()
		{
			questType.Value = 10;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(parts, dialogueparts, objective, target, targetMessage, numberCollected, number, reward, resource, deliveryItem);
		}

		public void loadQuestInfo()
		{
			if (target.Value != null || Game1.gameMode == 6)
			{
				return;
			}
			base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13640");
			resource.Value = random.Next(6) * 2;
			for (int i = 0; i < random.Next(1, 100); i++)
			{
				random.Next();
			}
			int highest_mining_level = 0;
			int highest_foraging_level = 0;
			foreach (Farmer farmer2 in Game1.getAllFarmers())
			{
				highest_mining_level = Math.Max(highest_mining_level, farmer2.MiningLevel);
			}
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				highest_foraging_level = Math.Max(highest_foraging_level, farmer.ForagingLevel);
			}
			switch (resource.Value)
			{
			case 0:
				resource.Value = 378;
				deliveryItem.Value = new Object(Vector2.Zero, resource, 1);
				number.Value = 20 + highest_mining_level * 2 + random.Next(-2, 4) * 2;
				reward.Value = (int)number * 10;
				number.Value = (int)number - (int)number % 5;
				target.Value = "Clint";
				break;
			case 2:
				resource.Value = 380;
				deliveryItem.Value = new Object(Vector2.Zero, resource, 1);
				number.Value = 15 + highest_mining_level + random.Next(-1, 3) * 2;
				reward.Value = (int)number * 15;
				number.Value = (int)((float)(int)number * 0.75f);
				number.Value = (int)number - (int)number % 5;
				target.Value = "Clint";
				break;
			case 4:
				resource.Value = 382;
				deliveryItem.Value = new Object(Vector2.Zero, resource, 1);
				number.Value = 10 + highest_mining_level + random.Next(-1, 3) * 2;
				reward.Value = (int)number * 25;
				number.Value = (int)((float)(int)number * 0.75f);
				number.Value = (int)number - (int)number % 5;
				target.Value = "Clint";
				break;
			case 6:
				resource.Value = ((Utility.GetAllPlayerDeepestMineLevel() > 40) ? 384 : 378);
				deliveryItem.Value = new Object(Vector2.Zero, resource, 1);
				number.Value = 8 + highest_mining_level / 2 + random.Next(-1, 1) * 2;
				reward.Value = (int)number * 30;
				number.Value = (int)((float)(int)number * 0.75f);
				number.Value = (int)number - (int)number % 2;
				target.Value = "Clint";
				break;
			case 8:
				resource.Value = 388;
				deliveryItem.Value = new Object(Vector2.Zero, resource, 1);
				number.Value = 25 + highest_foraging_level + random.Next(-3, 3) * 2;
				number.Value = (int)number - (int)number % 5;
				reward.Value = (int)number * 8;
				target.Value = "Robin";
				break;
			case 10:
				resource.Value = 390;
				deliveryItem.Value = new Object(Vector2.Zero, resource, 1);
				number.Value = 25 + highest_mining_level + random.Next(-3, 3) * 2;
				number.Value = (int)number - (int)number % 5;
				reward.Value = (int)number * 8;
				target.Value = "Robin";
				break;
			}
			if (target.Value == null)
			{
				return;
			}
			if ((int)resource < 388)
			{
				parts.Clear();
				int rand = random.Next(4);
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13647", number.Value, deliveryItem.Value, new DescriptionElement[4]
				{
					"Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13649",
					"Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13650",
					"Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13651",
					"Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13652"
				}.ElementAt(rand)));
				if (rand == 3)
				{
					dialogueparts.Clear();
					dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13655");
					dialogueparts.Add((random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13656" : ((random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13657" : "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13658"));
					dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13659");
				}
				else
				{
					dialogueparts.Clear();
					dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13662");
					dialogueparts.Add((random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13656" : ((random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13657" : "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13658"));
					dialogueparts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13667", (random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13668") : ((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13669") : new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13670"))) : ((DescriptionElement)"Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13672"));
					dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13673");
				}
			}
			else
			{
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13674", number.Value, deliveryItem.Value));
				dialogueparts.Clear();
				dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13677", ((int)resource == 388) ? new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13678") : new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13679")));
				dialogueparts.Add((random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13681" : ((random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13682" : "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13683"));
			}
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", reward.Value));
			parts.Add(target.Value.Equals("Clint") ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13688" : "");
			objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13691", "0", number.Value, deliveryItem.Value);
		}

		public override void reloadDescription()
		{
			if (_questDescription == "")
			{
				loadQuestInfo();
			}
			if (parts.Count != 0 && parts != null && dialogueparts.Count != 0 && dialogueparts != null)
			{
				string descriptionBuilder = "";
				string messageBuilder = "";
				foreach (DescriptionElement a in parts)
				{
					descriptionBuilder += a.loadDescriptionElement();
				}
				foreach (DescriptionElement b in dialogueparts)
				{
					messageBuilder += b.loadDescriptionElement();
				}
				base.questDescription = descriptionBuilder;
				targetMessage.Value = messageBuilder;
			}
		}

		public override void reloadObjective()
		{
			if ((int)numberCollected < (int)number)
			{
				objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13691", numberCollected.Value, number.Value, deliveryItem.Value);
			}
			if (objective.Value != null)
			{
				base.currentObjective = objective.Value.loadDescriptionElement();
			}
		}

		public override bool checkIfComplete(NPC n = null, int resourceCollected = -1, int amount = -1, Item item = null, string monsterName = null)
		{
			if ((bool)completed)
			{
				return false;
			}
			if (n == null && resourceCollected != -1 && amount != -1 && resourceCollected == (int)resource && (int)numberCollected < (int)number)
			{
				numberCollected.Value = Math.Min(number, (int)numberCollected + amount);
				if ((int)numberCollected < (int)number)
				{
					if (deliveryItem.Value == null)
					{
						deliveryItem.Value = new Object(Vector2.Zero, resource, 1);
					}
				}
				else
				{
					NPC actualTarget = Game1.getCharacterFromName(target);
					objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13277", actualTarget);
					Game1.playSound("jingle1");
				}
				Game1.dayTimeMoneyBox.moneyDial.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(387, 497, 3, 8), 800f, 1, 0, Game1.dayTimeMoneyBox.position + new Vector2(228f, 244f), flicker: false, flipped: false, 1f, 0.01f, Color.White, 4f, 0.3f, 0f, 0f)
				{
					scaleChangeChange = -0.012f
				});
			}
			else if (n != null && target.Value != null && (int)numberCollected >= (int)number && n.Name.Equals(target.Value) && n.isVillager())
			{
				n.CurrentDialogue.Push(new Dialogue(targetMessage, n));
				moneyReward.Value = reward;
				n.Name.Equals("Robin");
				questComplete();
				Game1.drawDialogue(n);
				return true;
			}
			return false;
		}
	}
}
