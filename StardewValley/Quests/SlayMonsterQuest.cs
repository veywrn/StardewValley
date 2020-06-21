using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	public class SlayMonsterQuest : Quest
	{
		public string targetMessage;

		[XmlElement("monsterName")]
		public readonly NetString monsterName = new NetString();

		[XmlElement("target")]
		public readonly NetString target = new NetString();

		[XmlElement("monster")]
		public readonly NetRef<Monster> monster = new NetRef<Monster>();

		[XmlIgnore]
		[Obsolete]
		public NPC actualTarget;

		[XmlElement("numberToKill")]
		public readonly NetInt numberToKill = new NetInt();

		[XmlElement("reward")]
		public readonly NetInt reward = new NetInt();

		[XmlElement("numberKilled")]
		public readonly NetInt numberKilled = new NetInt();

		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

		public SlayMonsterQuest()
		{
			questType.Value = 4;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(parts, dialogueparts, objective, monsterName, target, monster, numberToKill, reward, numberKilled);
		}

		public void loadQuestInfo()
		{
			for (int i = 0; i < random.Next(1, 100); i++)
			{
				random.Next();
			}
			if (target.Value != null && monster != null)
			{
				return;
			}
			base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13696");
			List<string> possibleMonsters = new List<string>();
			int mineLevel = Utility.GetAllPlayerDeepestMineLevel();
			if (mineLevel < 39)
			{
				possibleMonsters.Add("Green Slime");
				if (mineLevel > 10)
				{
					possibleMonsters.Add("Rock Crab");
				}
				if (mineLevel > 30)
				{
					possibleMonsters.Add("Duggy");
				}
			}
			else if (mineLevel < 79)
			{
				possibleMonsters.Add("Frost Jelly");
				if (mineLevel > 70)
				{
					possibleMonsters.Add("Skeleton");
				}
				possibleMonsters.Add("Dust Spirit");
			}
			else
			{
				possibleMonsters.Add("Sludge");
				possibleMonsters.Add("Ghost");
				possibleMonsters.Add("Lava Crab");
				possibleMonsters.Add("Squid Kid");
			}
			bool num = monsterName.Value == null || numberToKill.Value == 0;
			if (num)
			{
				monsterName.Value = possibleMonsters.ElementAt(random.Next(possibleMonsters.Count));
			}
			if (monsterName.Value == "Frost Jelly" || monsterName.Value == "Sludge")
			{
				monster.Value = new Monster("Green Slime", Vector2.Zero);
				monster.Value.Name = monsterName.Value;
			}
			else
			{
				monster.Value = new Monster(monsterName.Value, Vector2.Zero);
			}
			if (num)
			{
				switch (monsterName.Value)
				{
				case "Green Slime":
					numberToKill.Value = random.Next(4, 9);
					numberToKill.Value = (int)numberToKill - (int)numberToKill % 2;
					reward.Value = (int)numberToKill * 60;
					break;
				case "Rock Crab":
					numberToKill.Value = random.Next(2, 6);
					reward.Value = (int)numberToKill * 75;
					break;
				case "Duggy":
					parts.Clear();
					parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13711", numberToKill.Value));
					target.Value = "Clint";
					numberToKill.Value = random.Next(2, 4);
					reward.Value = (int)numberToKill * 150;
					break;
				case "Frost Jelly":
					numberToKill.Value = random.Next(4, 9);
					numberToKill.Value = (int)numberToKill - (int)numberToKill % 2;
					reward.Value = (int)numberToKill * 85;
					break;
				case "Ghost":
					numberToKill.Value = random.Next(1, 3);
					reward.Value = (int)numberToKill * 250;
					break;
				case "Sludge":
					numberToKill.Value = random.Next(4, 9);
					numberToKill.Value = (int)numberToKill - (int)numberToKill % 2;
					reward.Value = (int)numberToKill * 125;
					break;
				case "Lava Crab":
					numberToKill.Value = random.Next(2, 6);
					reward.Value = (int)numberToKill * 180;
					break;
				case "Squid Kid":
					numberToKill.Value = random.Next(1, 3);
					reward.Value = (int)numberToKill * 350;
					break;
				default:
					numberToKill.Value = random.Next(1, 4);
					reward.Value = (int)numberToKill * 120;
					break;
				}
			}
			if (monsterName.Value.Equals("Green Slime") || monsterName.Value.Equals("Frost Jelly") || monsterName.Value.Equals("Sludge"))
			{
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13723", numberToKill.Value, monsterName.Value.Equals("Frost Jelly") ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13725") : (monsterName.Value.Equals("Sludge") ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13727") : new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13728"))));
				target.Value = "Lewis";
				dialogueparts.Clear();
				dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13730");
				if (random.NextDouble() < 0.5)
				{
					dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13731");
					dialogueparts.Add((random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13732" : "Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13733");
					DescriptionElement color = new DescriptionElement[16]
					{
						"Strings\\StringsFromCSFiles:Dialogue.cs.795",
						"Strings\\StringsFromCSFiles:Dialogue.cs.796",
						"Strings\\StringsFromCSFiles:Dialogue.cs.797",
						"Strings\\StringsFromCSFiles:Dialogue.cs.798",
						"Strings\\StringsFromCSFiles:Dialogue.cs.799",
						"Strings\\StringsFromCSFiles:Dialogue.cs.800",
						"Strings\\StringsFromCSFiles:Dialogue.cs.801",
						"Strings\\StringsFromCSFiles:Dialogue.cs.802",
						"Strings\\StringsFromCSFiles:Dialogue.cs.803",
						"Strings\\StringsFromCSFiles:Dialogue.cs.804",
						"Strings\\StringsFromCSFiles:Dialogue.cs.805",
						"Strings\\StringsFromCSFiles:Dialogue.cs.806",
						"Strings\\StringsFromCSFiles:Dialogue.cs.807",
						"Strings\\StringsFromCSFiles:Dialogue.cs.808",
						"Strings\\StringsFromCSFiles:Dialogue.cs.809",
						"Strings\\StringsFromCSFiles:Dialogue.cs.810"
					}.ElementAt(random.Next(16));
					dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13734", (random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13735") : new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13736"), color, (random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13740") : ((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13741") : new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13742"))));
				}
				else
				{
					dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13744");
				}
			}
			else if (monsterName.Value.Equals("Rock Crab") || monsterName.Value.Equals("Lava Crab"))
			{
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13747", numberToKill.Value));
				target.Value = "Demetrius";
				dialogueparts.Clear();
				dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13750", monster.Value));
			}
			else
			{
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13752", monster.Value, numberToKill.Value, (random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13755") : ((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13756") : new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13757"))));
				target.Value = "Wizard";
				dialogueparts.Clear();
				dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13760");
			}
			if (target.Value.Equals("Wizard") && !Utility.doesAnyFarmerHaveMail("wizardJunimoNote") && !Utility.doesAnyFarmerHaveMail("JojaMember"))
			{
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13764", numberToKill.Value, monster.Value));
				target.Value = "Lewis";
				dialogueparts.Clear();
				dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13767");
			}
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", reward.Value));
			objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13770", "0", numberToKill.Value, monster.Value);
		}

		public override void reloadDescription()
		{
			if (_questDescription == "")
			{
				loadQuestInfo();
			}
			string descriptionBuilder = "";
			string messageBuilder = "";
			if (parts != null && parts.Count != 0)
			{
				foreach (DescriptionElement a in parts)
				{
					descriptionBuilder += a.loadDescriptionElement();
				}
				base.questDescription = descriptionBuilder;
			}
			if (dialogueparts != null && dialogueparts.Count != 0)
			{
				foreach (DescriptionElement b in dialogueparts)
				{
					messageBuilder += b.loadDescriptionElement();
				}
				targetMessage = messageBuilder;
			}
			else
			{
				if ((int)id == 0)
				{
					return;
				}
				Dictionary<int, string> questData = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
				if (questData != null && questData.ContainsKey(id))
				{
					string[] rawData = questData[id].Split('/');
					if (rawData != null && rawData.Length >= 9)
					{
						targetMessage = rawData[9];
					}
				}
			}
		}

		public override void reloadObjective()
		{
			if ((int)numberKilled != 0 || (int)id == 0)
			{
				if ((int)numberKilled < (int)numberToKill)
				{
					objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13770", numberKilled.Value, numberToKill.Value, monster.Value);
				}
				if (objective.Value != null)
				{
					base.currentObjective = objective.Value.loadDescriptionElement();
				}
			}
		}

		public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -1, Item item = null, string monsterName = null)
		{
			if ((bool)completed)
			{
				return false;
			}
			if (monsterName == null)
			{
				monsterName = "Green Slime";
			}
			if (n == null && monsterName != null && monsterName.Contains(this.monsterName.Value) && (int)numberKilled < (int)numberToKill)
			{
				numberKilled.Value = Math.Min(numberToKill, (int)numberKilled + 1);
				if ((int)numberKilled >= (int)numberToKill)
				{
					if (target.Value == null || target.Value.Equals("null"))
					{
						questComplete();
					}
					else
					{
						NPC actualTarget = Game1.getCharacterFromName(target);
						objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13277", actualTarget);
						Game1.playSound("jingle1");
					}
				}
				else if (monster.Value == null)
				{
					if (monsterName == "Frost Jelly" || monsterName == "Sludge")
					{
						monster.Value = new Monster("Green Slime", Vector2.Zero);
						monster.Value.Name = monsterName;
					}
					else
					{
						monster.Value = new Monster(monsterName, Vector2.Zero);
					}
				}
				Game1.dayTimeMoneyBox.moneyDial.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(387, 497, 3, 8), 800f, 1, 0, Game1.dayTimeMoneyBox.position + new Vector2(228f, 244f), flicker: false, flipped: false, 1f, 0.01f, Color.White, 4f, 0.3f, 0f, 0f)
				{
					scaleChangeChange = -0.012f
				});
			}
			else if (n != null && target.Value != null && !target.Value.Equals("null") && (int)numberKilled >= (int)numberToKill && n.Name.Equals(target.Value) && n.isVillager())
			{
				reloadDescription();
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
