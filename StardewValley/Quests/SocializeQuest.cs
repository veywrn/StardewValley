using Microsoft.Xna.Framework;
using Netcode;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	public class SocializeQuest : Quest
	{
		public readonly NetStringList whoToGreet = new NetStringList();

		[XmlElement("total")]
		public readonly NetInt total = new NetInt();

		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

		public SocializeQuest()
		{
			questType.Value = 5;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(whoToGreet, total, parts, objective);
		}

		public void loadQuestInfo()
		{
			if (whoToGreet.Count <= 0)
			{
				base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SocializeQuest.cs.13785");
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13786", (random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13787") : ((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13788") : new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13789"))));
				parts.Add("Strings\\StringsFromCSFiles:SocializeQuest.cs.13791");
				Dictionary<string, string> npcs = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				foreach (string name in npcs.Keys)
				{
					switch (name)
					{
					default:
						if (!(npcs[name].Split('/')[7] != "Town"))
						{
							whoToGreet.Add(name);
						}
						break;
					case "Kent":
					case "Sandy":
					case "Dwarf":
					case "Marlon":
					case "Wizard":
					case "Krobus":
					case "Leo":
						break;
					}
				}
				objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", "2", whoToGreet.Count);
				total.Value = whoToGreet.Count;
				whoToGreet.Remove("Lewis");
				whoToGreet.Remove("Robin");
			}
		}

		public override void reloadDescription()
		{
			if (_questDescription == "")
			{
				loadQuestInfo();
			}
			if (parts.Count != 0 && parts != null)
			{
				string descriptionBuilder = "";
				foreach (DescriptionElement a in parts)
				{
					descriptionBuilder += a.loadDescriptionElement();
				}
				base.questDescription = descriptionBuilder;
			}
		}

		public override void reloadObjective()
		{
			loadQuestInfo();
			if (objective.Value == null && whoToGreet.Count > 0)
			{
				objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", (int)total - whoToGreet.Count, total.Value);
			}
			if (objective.Value != null)
			{
				base.currentObjective = objective.Value.loadDescriptionElement();
			}
		}

		public override bool checkIfComplete(NPC npc = null, int number1 = -1, int number2 = -1, Item item = null, string monsterName = null)
		{
			loadQuestInfo();
			if (npc != null && whoToGreet.Remove(npc.Name))
			{
				Game1.dayTimeMoneyBox.moneyDial.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(387, 497, 3, 8), 800f, 1, 0, Game1.dayTimeMoneyBox.position + new Vector2(228f, 244f), flicker: false, flipped: false, 1f, 0.01f, Color.White, 4f, 0.3f, 0f, 0f)
				{
					scaleChangeChange = -0.012f
				});
			}
			if (whoToGreet.Count == 0 && !completed)
			{
				foreach (string s in Game1.player.friendshipData.Keys)
				{
					if (Game1.player.friendshipData[s].Points < 2729)
					{
						Game1.player.changeFriendship(100, Game1.getCharacterFromName(s));
					}
				}
				questComplete();
				return true;
			}
			objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", (int)total - whoToGreet.Count, total.Value);
			return false;
		}
	}
}
