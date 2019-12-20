using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	public class ItemDeliveryQuest : Quest
	{
		public string targetMessage;

		[XmlElement("target")]
		public readonly NetString target = new NetString();

		[XmlElement("item")]
		public readonly NetInt item = new NetInt();

		[XmlElement("number")]
		public readonly NetInt number = new NetInt(1);

		[XmlElement("deliveryItem")]
		public readonly NetRef<Object> deliveryItem = new NetRef<Object>();

		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

		[XmlIgnore]
		[Obsolete]
		public NPC actualTarget;

		public ItemDeliveryQuest()
		{
			questType.Value = 3;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(target, item, number, deliveryItem, parts, dialogueparts, objective);
		}

		public List<NPC> GetValidTargetList()
		{
			List<NPC> valid_targets = new List<NPC>();
			foreach (Farmer farmer2 in Game1.getAllFarmers())
			{
				if (farmer2 != null)
				{
					foreach (string key in farmer2.friendshipData.Keys)
					{
						NPC npc = Game1.getCharacterFromName(key);
						if (npc != null && !valid_targets.Contains(npc))
						{
							valid_targets.Add(npc);
						}
					}
				}
			}
			valid_targets.OrderBy((NPC n) => n.Name);
			for (int l = 0; l < valid_targets.Count; l++)
			{
				NPC target = valid_targets[l];
				if (target.IsInvisible)
				{
					valid_targets.RemoveAt(l);
					l--;
				}
				else if (target.Age == 2)
				{
					valid_targets.RemoveAt(l);
					l--;
				}
				else if (!target.isVillager())
				{
					valid_targets.RemoveAt(l);
					l--;
				}
				else if (target.Name.Equals("Krobus") || target.Name.Equals("Qi") || target.Name.Equals("Dwarf") || target.Name.Equals("Gunther") || target.Name.Equals("Bouncer") || target.Name.Equals("Henchman") || target.Name.Equals("Marlon") || target.Name.Equals("Mariner"))
				{
					valid_targets.RemoveAt(l);
					l--;
				}
			}
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				for (int k = 0; k < valid_targets.Count; k++)
				{
					if (valid_targets[k].Name.Equals(farmer.spouse))
					{
						valid_targets.RemoveAt(k);
						k--;
					}
				}
			}
			for (int j = 0; j < valid_targets.Count; j++)
			{
				if (valid_targets[j].Name.Equals("Sandy"))
				{
					bool seen_event = false;
					foreach (Farmer allFarmer in Game1.getAllFarmers())
					{
						if (allFarmer.eventsSeen.Contains(67))
						{
							seen_event = true;
							break;
						}
					}
					if (!seen_event)
					{
						valid_targets.RemoveAt(j);
						j--;
					}
					break;
				}
			}
			return valid_targets;
		}

		public void loadQuestInfo()
		{
			if (target.Value != null)
			{
				return;
			}
			base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13285");
			NPC actualTarget2 = null;
			List<NPC> valid_targets = GetValidTargetList();
			if (Game1.player.friendshipData == null || Game1.player.friendshipData.Count() <= 0 || valid_targets.Count <= 0)
			{
				return;
			}
			actualTarget2 = valid_targets[random.Next(valid_targets.Count)];
			if (actualTarget2 == null)
			{
				return;
			}
			target.Value = actualTarget2.name;
			if (target.Value.Equals("Wizard") && !Game1.player.mailReceived.Contains("wizardJunimoNote") && !Game1.player.mailReceived.Contains("JojaMember"))
			{
				target.Value = "Demetrius";
				actualTarget2 = Game1.getCharacterFromName(target.Value);
			}
			if (!Game1.currentSeason.Equals("winter") && random.NextDouble() < 0.15)
			{
				List<int> crops = Utility.possibleCropsAtThisTime(Game1.currentSeason, (Game1.dayOfMonth <= 7) ? true : false);
				item.Value = crops.ElementAt(random.Next(crops.Count));
				deliveryItem.Value = new Object(Vector2.Zero, item.Value, 1);
				parts.Clear();
				parts.Add((random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13299" : ((random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13300" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13301"));
				parts.Add((random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13302", deliveryItem.Value) : ((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13303", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13304", deliveryItem.Value)));
				parts.Add((random.NextDouble() < 0.25) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13306" : ((random.NextDouble() < 0.33) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13307" : ((random.NextDouble() < 0.5) ? "" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13308")));
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2));
				if (target.Value.Equals("Demetrius"))
				{
					parts.Clear();
					parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13311", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13314", deliveryItem.Value));
				}
				if (target.Value.Equals("Marnie"))
				{
					parts.Clear();
					parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13317", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13320", deliveryItem.Value));
				}
				if (target.Value.Equals("Sebastian"))
				{
					parts.Clear();
					parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13324", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13327", deliveryItem.Value));
				}
			}
			else
			{
				item.Value = Utility.getRandomItemFromSeason(Game1.currentSeason, 1000, forQuest: true);
				if ((int)item == -5)
				{
					item.Value = 176;
				}
				if ((int)item == -6)
				{
					item.Value = 184;
				}
				deliveryItem.Value = new Object(Vector2.Zero, item, 1);
				DescriptionElement[] questDescriptions11 = null;
				DescriptionElement[] questDescriptions10 = null;
				DescriptionElement[] questDescriptions9 = null;
				if (Game1.objectInformation[item].Split('/')[3].Split(' ')[0].Equals("Cooking") && !target.Value.Equals("Wizard"))
				{
					if (random.NextDouble() < 0.33)
					{
						DescriptionElement[] questStrings3 = new DescriptionElement[12]
						{
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341",
							(!Game1.samBandName.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156")) : ((!Game1.elliottBookName.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157")) : ((DescriptionElement)"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346")),
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13349",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13350",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13351",
							Game1.currentSeason.Equals("winter") ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13353" : (Game1.currentSeason.Equals("summer") ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13355" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13356"),
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357"
						};
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13333", deliveryItem.Value, questStrings3.ElementAt(random.Next(12))) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13334", deliveryItem.Value, questStrings3.ElementAt(random.Next(12))));
						parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2));
					}
					else
					{
						DescriptionElement day = new DescriptionElement();
						switch (Game1.dayOfMonth % 7)
						{
						case 0:
							day = "Strings\\StringsFromCSFiles:Game1.cs.3042";
							break;
						case 1:
							day = "Strings\\StringsFromCSFiles:Game1.cs.3043";
							break;
						case 2:
							day = "Strings\\StringsFromCSFiles:Game1.cs.3044";
							break;
						case 3:
							day = "Strings\\StringsFromCSFiles:Game1.cs.3045";
							break;
						case 4:
							day = "Strings\\StringsFromCSFiles:Game1.cs.3046";
							break;
						case 5:
							day = "Strings\\StringsFromCSFiles:Game1.cs.3047";
							break;
						case 6:
							day = "Strings\\StringsFromCSFiles:Game1.cs.3048";
							break;
						}
						questDescriptions11 = new DescriptionElement[5]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13360", deliveryItem.Value),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13364", deliveryItem.Value),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13367", deliveryItem.Value),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13370", deliveryItem.Value),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13373", day, deliveryItem.Value, actualTarget2)
						};
						questDescriptions10 = new DescriptionElement[5]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
							""
						};
						questDescriptions9 = new DescriptionElement[5]
						{
							"",
							"",
							"",
							"",
							""
						};
					}
					parts.Clear();
					int rand5 = random.Next(questDescriptions11.Count());
					parts.Add(questDescriptions11[rand5]);
					parts.Add(questDescriptions10[rand5]);
					parts.Add(questDescriptions9[rand5]);
					if (target.Value.Equals("Sebastian"))
					{
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13378", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13381", deliveryItem.Value));
					}
				}
				else if (random.NextDouble() < 0.5 && Convert.ToInt32(Game1.objectInformation[item].Split('/')[2]) > 0)
				{
					questDescriptions11 = new DescriptionElement[2]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13383", deliveryItem.Value, new DescriptionElement[12]
						{
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13385",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13386",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13387",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13388",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13389",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13390",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13391",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13392",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13393",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13394",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13395",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13396"
						}.ElementAt(random.Next(12))),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13400", deliveryItem.Value)
					};
					questDescriptions10 = new DescriptionElement[2]
					{
						new DescriptionElement((random.NextDouble() < 0.5) ? "" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13398"),
						new DescriptionElement((random.NextDouble() < 0.5) ? "" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13402")
					};
					questDescriptions9 = new DescriptionElement[2]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2)
					};
					if (random.NextDouble() < 0.33)
					{
						DescriptionElement[] questSTrings = new DescriptionElement[12]
						{
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341",
							(!Game1.samBandName.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156")) : ((!Game1.elliottBookName.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157")) : ((DescriptionElement)"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346")),
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13420",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13421",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13422",
							Game1.currentSeason.Equals("winter") ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13424" : (Game1.currentSeason.Equals("summer") ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13426" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13427"),
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357"
						};
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13333", deliveryItem.Value, questSTrings.ElementAt(random.Next(12))) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13334", deliveryItem.Value, questSTrings.ElementAt(random.Next(12))));
						parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2));
					}
					else
					{
						parts.Clear();
						int rand4 = random.Next(questDescriptions11.Count());
						parts.Add(questDescriptions11[rand4]);
						parts.Add(questDescriptions10[rand4]);
						parts.Add(questDescriptions9[rand4]);
					}
					if (target.Value.Equals("Demetrius"))
					{
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13311", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13314", deliveryItem.Value));
					}
					if (target.Value.Equals("Marnie"))
					{
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13317", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13320", deliveryItem.Value));
					}
					if (target.Value.Equals("Harvey"))
					{
						DescriptionElement[] questStrings2 = new DescriptionElement[12]
						{
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13448",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13449",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13450",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13451",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13452",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13453",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13454",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13455",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13456",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13457",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13458",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13459"
						};
						parts.Clear();
						parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13446", deliveryItem.Value, questStrings2.ElementAt(random.Next(12))));
					}
					if (target.Value.Equals("Gus") && random.NextDouble() < 0.6)
					{
						parts.Clear();
						parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13462", deliveryItem.Value));
					}
				}
				else if (random.NextDouble() < 0.5 && Convert.ToInt32(Game1.objectInformation[item].Split('/')[2]) < 0)
				{
					parts.Clear();
					parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13464", deliveryItem.Value, new DescriptionElement[5]
					{
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13465",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13466",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13467",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13468",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13469"
					}.ElementAt(random.Next(5))));
					parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2));
					if (target.Value.Equals("Emily"))
					{
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13473", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13476", deliveryItem.Value));
					}
				}
				else
				{
					DescriptionElement[] questStrings = new DescriptionElement[12]
					{
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13502",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13503",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13504",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13505",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13506",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13507",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13508",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13509",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13510",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13511",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13512",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13513"
					};
					questDescriptions11 = new DescriptionElement[9]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13480", actualTarget2, deliveryItem.Value),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13481", deliveryItem.Value),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13485", deliveryItem.Value),
						(random.NextDouble() < 0.4) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13491", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13492", deliveryItem.Value),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13494", deliveryItem.Value),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13497", deliveryItem.Value),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13500", deliveryItem.Value, questStrings.ElementAt(random.Next(12))),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13518", actualTarget2, deliveryItem.Value),
						(random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13520", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13523", deliveryItem.Value)
					};
					questDescriptions10 = new DescriptionElement[9]
					{
						"",
						(random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13482" : ((random.NextDouble() < 0.5) ? "" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13483"),
						(random.NextDouble() < 0.25) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13487" : ((random.NextDouble() < 0.33) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13488" : ((random.NextDouble() < 0.5) ? "" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13489")),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
						(random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13514" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13516",
						"",
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2)
					};
					questDescriptions9 = new DescriptionElement[9]
					{
						"",
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
						"",
						"",
						"",
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget2),
						"",
						""
					};
					parts.Clear();
					int rand3 = random.Next(questDescriptions11.Count());
					parts.Add(questDescriptions11[rand3]);
					parts.Add(questDescriptions10[rand3]);
					parts.Add(questDescriptions9[rand3]);
				}
			}
			dialogueparts.Clear();
			dialogueparts.Add((random.NextDouble() < 0.3 || target.Value.Equals("Evelyn")) ? ((DescriptionElement)"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13526") : ((random.NextDouble() < 0.5) ? ((DescriptionElement)"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13527") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13528", Game1.player.Name)));
			dialogueparts.Add((random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13530", deliveryItem.Value) : ((random.NextDouble() < 0.5) ? ((DescriptionElement)"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13532") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13533", (random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13534") : ((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13535") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13536")))));
			dialogueparts.Add((random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13538" : ((random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13539" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13540"));
			dialogueparts.Add((random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13542" : ((random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13543" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13544"));
			if (target.Value.Equals("Wizard"))
			{
				parts.Clear();
				if (random.NextDouble() < 0.5)
				{
					parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13546", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13548", deliveryItem.Value));
				}
				else
				{
					parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13551", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13553", deliveryItem.Value));
				}
				dialogueparts.Clear();
				dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13555");
			}
			if (target.Value.Equals("Haley"))
			{
				parts.Clear();
				parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13557", deliveryItem.Value) : (Game1.player.isMale ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13560", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13563", deliveryItem.Value)));
				dialogueparts.Clear();
				dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13566");
			}
			if (target.Value.Equals("Sam"))
			{
				parts.Clear();
				parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13568", deliveryItem.Value) : (Game1.player.isMale ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13571", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13574", deliveryItem.Value)));
				dialogueparts.Clear();
				dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13577", Game1.player.Name));
			}
			if (target.Value.Equals("Maru"))
			{
				parts.Clear();
				double rand2 = random.NextDouble();
				parts.Add((rand2 < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13580", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13583", deliveryItem.Value));
				dialogueparts.Clear();
				dialogueparts.Add((rand2 < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13585", Game1.player.Name) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13587", Game1.player.Name));
			}
			if (target.Value.Equals("Abigail"))
			{
				parts.Clear();
				double rand = random.NextDouble();
				parts.Add((rand < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13590", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13593", deliveryItem.Value));
				dialogueparts.Add((rand < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13597", Game1.player.Name) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13599", Game1.player.Name));
			}
			if (target.Value.Equals("Sebastian"))
			{
				dialogueparts.Clear();
				dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13602");
			}
			if (target.Value.Equals("Elliott"))
			{
				dialogueparts.Clear();
				dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13604", deliveryItem.Value, Game1.player.Name));
			}
			DescriptionElement lastPart = (random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13608", actualTarget2) : ((!(random.NextDouble() < 0.5)) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13612", actualTarget2) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13610", actualTarget2));
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", (int)deliveryItem.Value.price * 3));
			parts.Add(lastPart);
			objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13614", actualTarget2, deliveryItem.Value);
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
			else if ((int)id != 0)
			{
				Dictionary<int, string> questData = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
				if (questData != null && questData.ContainsKey(id))
				{
					string[] rawData = questData[id].Split('/');
					targetMessage = rawData[9];
				}
			}
		}

		public override void reloadObjective()
		{
			if (objective.Value != null)
			{
				base.currentObjective = objective.Value.loadDescriptionElement();
			}
		}

		public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -1, Item item = null, string monsterName = null)
		{
			if ((bool)completed)
			{
				return false;
			}
			if (item != null && item is Object && n != null && n.isVillager() && n.Name.Equals(target.Value) && ((item as Object).ParentSheetIndex == (int)this.item || (item as Object).Category == (int)this.item))
			{
				if (item.Stack >= (int)number)
				{
					Game1.player.ActiveObject.Stack -= (int)number - 1;
					reloadDescription();
					n.CurrentDialogue.Push(new Dialogue(targetMessage, n));
					Game1.drawDialogue(n);
					Game1.player.reduceActiveItemByOne();
					if ((bool)dailyQuest)
					{
						Game1.player.changeFriendship(150, n);
						if (deliveryItem.Value == null)
						{
							deliveryItem.Value = new Object(Vector2.Zero, this.item, 1);
						}
						moneyReward.Value = (int)deliveryItem.Value.price * 3;
					}
					else
					{
						Game1.player.changeFriendship(255, n);
					}
					questComplete();
					return true;
				}
				n.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13615", number.Value), n));
				Game1.drawDialogue(n);
				return false;
			}
			return false;
		}
	}
}
