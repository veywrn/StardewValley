using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	public class FishingQuest : Quest
	{
		[XmlElement("target")]
		public readonly NetString target = new NetString();

		public string targetMessage;

		[XmlElement("numberToFish")]
		public readonly NetInt numberToFish = new NetInt();

		[XmlElement("reward")]
		public readonly NetInt reward = new NetInt();

		[XmlElement("numberFished")]
		public readonly NetInt numberFished = new NetInt();

		[XmlElement("whichFish")]
		public readonly NetInt whichFish = new NetInt();

		[XmlElement("fish")]
		public readonly NetRef<Object> fish = new NetRef<Object>();

		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

		public FishingQuest()
		{
			questType.Value = 7;
		}

		protected override void initNetFields()
		{
			base.NetFields.AddFields(parts, dialogueparts, objective, target, numberToFish, reward, numberFished, whichFish, fish);
		}

		public void loadQuestInfo()
		{
			if (target.Value != null && fish.Value != null)
			{
				return;
			}
			base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingQuest.cs.13227");
			if (random.NextDouble() < 0.5)
			{
				switch (Game1.currentSeason)
				{
				case "spring":
				{
					int[] possiblefish8 = new int[8]
					{
						129,
						131,
						136,
						137,
						142,
						143,
						145,
						147
					};
					whichFish.Value = possiblefish8[random.Next(possiblefish8.Length)];
					break;
				}
				case "summer":
				{
					int[] possiblefish8 = new int[10]
					{
						130,
						131,
						136,
						138,
						142,
						144,
						145,
						146,
						149,
						150
					};
					whichFish.Value = possiblefish8[random.Next(possiblefish8.Length)];
					break;
				}
				case "fall":
				{
					int[] possiblefish8 = new int[8]
					{
						129,
						131,
						136,
						137,
						139,
						142,
						143,
						150
					};
					whichFish.Value = possiblefish8[random.Next(possiblefish8.Length)];
					break;
				}
				case "winter":
				{
					int[] possiblefish8 = new int[9]
					{
						130,
						131,
						136,
						141,
						144,
						146,
						147,
						150,
						151
					};
					whichFish.Value = possiblefish8[random.Next(possiblefish8.Length)];
					break;
				}
				}
				fish.Value = new Object(Vector2.Zero, whichFish, 1);
				numberToFish.Value = (int)Math.Ceiling(90.0 / (double)Math.Max(1, fish.Value.price)) + Game1.player.FishingLevel / 5;
				reward.Value = numberToFish.Value * (int)fish.Value.price;
				target.Value = "Demetrius";
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13228", fish.Value, numberToFish.Value));
				dialogueparts.Clear();
				dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13231", fish.Value, new DescriptionElement[4]
				{
					"Strings\\StringsFromCSFiles:FishingQuest.cs.13233",
					"Strings\\StringsFromCSFiles:FishingQuest.cs.13234",
					"Strings\\StringsFromCSFiles:FishingQuest.cs.13235",
					new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13236", fish.Value)
				}.ElementAt(random.Next(4))));
				objective.Value = (fish.Value.name.Equals("Octopus") ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13243", 0, numberToFish.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", 0, numberToFish.Value, fish.Value));
			}
			else
			{
				switch (Game1.currentSeason)
				{
				case "spring":
				{
					int[] possiblefish4 = new int[9]
					{
						129,
						131,
						136,
						137,
						142,
						143,
						145,
						147,
						702
					};
					whichFish.Value = possiblefish4[random.Next(possiblefish4.Length)];
					break;
				}
				case "summer":
				{
					int[] possiblefish4 = new int[12]
					{
						128,
						130,
						131,
						136,
						138,
						142,
						144,
						145,
						146,
						149,
						150,
						702
					};
					whichFish.Value = possiblefish4[random.Next(possiblefish4.Length)];
					break;
				}
				case "fall":
				{
					int[] possiblefish4 = new int[11]
					{
						129,
						131,
						136,
						137,
						139,
						142,
						143,
						150,
						699,
						702,
						705
					};
					whichFish.Value = possiblefish4[random.Next(possiblefish4.Length)];
					break;
				}
				case "winter":
				{
					int[] possiblefish4 = new int[13]
					{
						130,
						131,
						136,
						141,
						143,
						144,
						146,
						147,
						150,
						151,
						699,
						702,
						705
					};
					whichFish.Value = possiblefish4[random.Next(possiblefish4.Length)];
					break;
				}
				}
				target.Value = "Willy";
				fish.Value = new Object(Vector2.Zero, whichFish, 1);
				numberToFish.Value = (int)Math.Ceiling(90.0 / (double)Math.Max(1, fish.Value.price)) + Game1.player.FishingLevel / 5;
				reward.Value = numberToFish.Value * (int)fish.Value.price;
				parts.Clear();
				if ((bool)Game1.player.isMale)
				{
					parts.Add(fish.Value.name.Equals("Squid") ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13248", reward.Value, numberToFish.Value, new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13253")) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13248", reward.Value, numberToFish.Value, fish.Value));
				}
				else
				{
					parts.Add(fish.Value.name.Equals("Squid") ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13251", reward.Value, numberToFish.Value, new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13253")) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13251", reward.Value, numberToFish.Value, fish.Value));
				}
				dialogueparts.Clear();
				dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13256", fish.Value));
				dialogueparts.Add(new DescriptionElement[4]
				{
					"Strings\\StringsFromCSFiles:FishingQuest.cs.13258",
					"Strings\\StringsFromCSFiles:FishingQuest.cs.13259",
					new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13260", new DescriptionElement[6]
					{
						"Strings\\StringsFromCSFiles:FishingQuest.cs.13261",
						"Strings\\StringsFromCSFiles:FishingQuest.cs.13262",
						"Strings\\StringsFromCSFiles:FishingQuest.cs.13263",
						"Strings\\StringsFromCSFiles:FishingQuest.cs.13264",
						"Strings\\StringsFromCSFiles:FishingQuest.cs.13265",
						"Strings\\StringsFromCSFiles:FishingQuest.cs.13266"
					}.ElementAt(random.Next(6))),
					"Strings\\StringsFromCSFiles:FishingQuest.cs.13267"
				}.ElementAt(random.Next(4)));
				dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13268"));
				objective.Value = (fish.Value.name.Equals("Squid") ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13255", 0, numberToFish.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", 0, numberToFish.Value, fish.Value));
			}
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", reward.Value));
			parts.Add("Strings\\StringsFromCSFiles:FishingQuest.cs.13275");
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
				targetMessage = messageBuilder;
			}
		}

		public override void reloadObjective()
		{
			if ((int)numberFished < (int)numberToFish)
			{
				objective.Value = (fish.Value.name.Equals("Octopus") ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13243", numberFished.Value, numberToFish.Value) : (fish.Value.name.Equals("Squid") ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13255", numberFished.Value, numberToFish.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", numberFished.Value, numberToFish.Value, fish.Value)));
			}
			if (objective.Value != null)
			{
				base.currentObjective = objective.Value.loadDescriptionElement();
			}
		}

		public override bool checkIfComplete(NPC n = null, int fishid = -1, int number2 = 1, Item item = null, string monsterName = null)
		{
			loadQuestInfo();
			if (n == null && fishid != -1 && fishid == (int)whichFish && (int)numberFished < (int)numberToFish)
			{
				numberFished.Value = Math.Min(numberToFish, (int)numberFished + number2);
				if ((int)numberFished >= (int)numberToFish)
				{
					dailyQuest.Value = false;
					if (target.Value == null)
					{
						target.Value = "Willy";
					}
					NPC actualTarget = Game1.getCharacterFromName(target);
					objective.Value = new DescriptionElement("Strings\\Quests:ObjectiveReturnToNPC", actualTarget);
					Game1.playSound("jingle1");
				}
			}
			else if (n != null && (int)numberFished >= (int)numberToFish && target.Value != null && n.Name.Equals(target.Value) && n.isVillager() && !completed)
			{
				n.CurrentDialogue.Push(new Dialogue(targetMessage, n));
				moneyReward.Value = reward;
				questComplete();
				Game1.drawDialogue(n);
				return true;
			}
			return false;
		}
	}
}
