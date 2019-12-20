using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley
{
	public class Dialogue
	{
		public delegate bool onAnswerQuestion(int whichResponse);

		public const string dialogueHappy = "$h";

		public const string dialogueSad = "$s";

		public const string dialogueUnique = "$u";

		public const string dialogueNeutral = "$neutral";

		public const string dialogueLove = "$l";

		public const string dialogueAngry = "$a";

		public const string dialogueEnd = "$e";

		public const string dialogueBreak = "$b";

		public const string dialogueKill = "$k";

		public const string dialogueChance = "$c";

		public const string dialogueDependingOnWorldState = "$d";

		public const string dialogueQuickResponse = "$y";

		public const string dialoguePrerequisite = "$p";

		public const string dialogueSingle = "$1";

		public const string dialogueQuestion = "$q";

		public const string dialogueResponse = "$r";

		public const string breakSpecialCharacter = "{";

		public const string playerNameSpecialCharacter = "@";

		public const string genderDialogueSplitCharacter = "^";

		public const string genderDialogueSplitCharacter2 = "¦";

		public const string quickResponseDelineator = "*";

		public const string randomAdjectiveSpecialCharacter = "%adj";

		public const string randomNounSpecialCharacter = "%noun";

		public const string randomPlaceSpecialCharacter = "%place";

		public const string spouseSpecialCharacter = "%spouse";

		public const string randomNameSpecialCharacter = "%name";

		public const string firstNameLettersSpecialCharacter = "%firstnameletter";

		public const string timeSpecialCharacter = "%time";

		public const string bandNameSpecialCharacter = "%band";

		public const string bookNameSpecialCharacter = "%book";

		public const string rivalSpecialCharacter = "%rival";

		public const string petSpecialCharacter = "%pet";

		public const string farmNameSpecialCharacter = "%farm";

		public const string favoriteThingSpecialCharacter = "%favorite";

		public const string eventForkSpecialCharacter = "%fork";

		public const string kid1specialCharacter = "%kid1";

		public const string kid2SpecialCharacter = "%kid2";

		public const string revealTasteCharacter = "%revealtaste";

		private static bool nameArraysTranslated = false;

		public static string[] adjectives = new string[20]
		{
			"Purple",
			"Gooey",
			"Chalky",
			"Green",
			"Plush",
			"Chunky",
			"Gigantic",
			"Greasy",
			"Gloomy",
			"Practical",
			"Lanky",
			"Dopey",
			"Crusty",
			"Fantastic",
			"Rubbery",
			"Silly",
			"Courageous",
			"Reasonable",
			"Lonely",
			"Bitter"
		};

		public static string[] nouns = new string[23]
		{
			"Dragon",
			"Buffet",
			"Biscuit",
			"Robot",
			"Planet",
			"Pepper",
			"Tomb",
			"Hyena",
			"Lip",
			"Quail",
			"Cheese",
			"Disaster",
			"Raincoat",
			"Shoe",
			"Castle",
			"Elf",
			"Pump",
			"Chip",
			"Wig",
			"Mermaid",
			"Drumstick",
			"Puppet",
			"Submarine"
		};

		public static string[] verbs = new string[13]
		{
			"ran",
			"danced",
			"spoke",
			"galloped",
			"ate",
			"floated",
			"stood",
			"flowed",
			"smelled",
			"swam",
			"grilled",
			"cracked",
			"melted"
		};

		public static string[] positional = new string[13]
		{
			"atop",
			"near",
			"with",
			"alongside",
			"away from",
			"too close to",
			"dangerously close to",
			"far, far away from",
			"uncomfortably close to",
			"way above the",
			"miles below",
			"on a different planet from",
			"in a different century than"
		};

		public static string[] places = new string[12]
		{
			"Castle Village",
			"Basket Town",
			"Pine Mesa City",
			"Point Drake",
			"Minister Valley",
			"Grampleton",
			"Zuzu City",
			"a small island off the coast",
			"Fort Josa",
			"Chestervale",
			"Fern Islands",
			"Tanker Grove"
		};

		public static string[] colors = new string[16]
		{
			"/crimson",
			"/green",
			"/tan",
			"/purple",
			"/deep blue",
			"/neon pink",
			"/pale/yellow",
			"/chocolate/brown",
			"/sky/blue",
			"/bubblegum/pink",
			"/blood/red",
			"/bright/orange",
			"/aquamarine",
			"/silvery",
			"/glimmering/gold",
			"/rainbow"
		};

		private List<string> dialogues = new List<string>();

		private List<NPCDialogueResponse> playerResponses;

		private List<string> quickResponses;

		private bool isLastDialogueInteractive;

		private bool quickResponse;

		public bool isCurrentStringContinuedOnNextScreen;

		private bool dialogueToBeKilled;

		private bool finishedLastDialogue;

		public bool showPortrait;

		public bool removeOnNextMove;

		public int currentDialogueIndex;

		private string currentEmotion;

		public string temporaryDialogue;

		public NPC speaker;

		public onAnswerQuestion answerQuestionBehavior;

		public string CurrentEmotion
		{
			get
			{
				return currentEmotion;
			}
			set
			{
				currentEmotion = value;
			}
		}

		public Farmer farmer
		{
			get
			{
				if (Game1.CurrentEvent != null)
				{
					return Game1.CurrentEvent.farmer;
				}
				return Game1.player;
			}
		}

		private static void TranslateArraysOfStrings()
		{
			colors = new string[16]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.795"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.796"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.797"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.798"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.799"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.800"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.801"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.802"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.803"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.804"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.805"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.806"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.807"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.808"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.809"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.810")
			};
			adjectives = new string[20]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.679"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.680"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.681"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.682"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.683"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.684"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.685"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.686"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.687"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.688"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.689"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.690"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.691"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.692"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.693"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.694"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.695"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.696"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.697"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.698")
			};
			nouns = new string[23]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.699"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.700"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.701"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.702"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.703"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.704"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.705"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.706"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.707"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.708"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.709"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.710"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.711"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.712"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.713"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.714"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.715"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.716"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.717"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.718"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.719"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.720"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.721")
			};
			verbs = new string[13]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.722"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.723"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.724"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.725"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.726"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.727"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.728"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.729"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.730"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.731"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.732"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.733"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.734")
			};
			positional = new string[13]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.735"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.736"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.737"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.738"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.739"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.740"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.741"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.742"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.743"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.744"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.745"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.746"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.747")
			};
			places = new string[12]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.748"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.749"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.750"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.751"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.752"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.753"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.754"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.755"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.756"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.757"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.758"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.759")
			};
			nameArraysTranslated = true;
		}

		public Dialogue(string masterDialogue, NPC speaker)
		{
			if (!nameArraysTranslated)
			{
				TranslateArraysOfStrings();
			}
			this.speaker = speaker;
			parseDialogueString(masterDialogue);
			checkForSpecialDialogueAttributes();
		}

		public void setCurrentDialogue(string dialogue)
		{
			dialogues.Clear();
			currentDialogueIndex = 0;
			parseDialogueString(dialogue);
		}

		public void addMessageToFront(string dialogue)
		{
			currentDialogueIndex = 0;
			List<string> tmp = new List<string>();
			tmp.AddRange(dialogues);
			dialogues.Clear();
			parseDialogueString(dialogue);
			dialogues.AddRange(tmp);
			checkForSpecialDialogueAttributes();
		}

		public static string getRandomVerb()
		{
			if (!nameArraysTranslated)
			{
				TranslateArraysOfStrings();
			}
			return verbs[Game1.random.Next(verbs.Count())];
		}

		public static string getRandomAdjective()
		{
			if (!nameArraysTranslated)
			{
				TranslateArraysOfStrings();
			}
			return adjectives[Game1.random.Next(adjectives.Count())];
		}

		public static string getRandomNoun()
		{
			if (!nameArraysTranslated)
			{
				TranslateArraysOfStrings();
			}
			return nouns[Game1.random.Next(nouns.Count())];
		}

		public static string getRandomPositional()
		{
			if (!nameArraysTranslated)
			{
				TranslateArraysOfStrings();
			}
			return positional[Game1.random.Next(positional.Count())];
		}

		public int getPortraitIndex()
		{
			switch (currentEmotion)
			{
			case "$neutral":
				return 0;
			case "$h":
				return 1;
			case "$s":
				return 2;
			case "$u":
				return 3;
			case "$l":
				return 4;
			case "$a":
				return 5;
			default:
			{
				if (int.TryParse(currentEmotion.Substring(1), out int _))
				{
					return Convert.ToInt32(currentEmotion.Substring(1));
				}
				return 0;
			}
			}
		}

		private void parseDialogueString(string masterString)
		{
			if (masterString == null)
			{
				masterString = "...";
			}
			temporaryDialogue = null;
			if (playerResponses != null)
			{
				playerResponses.Clear();
			}
			string[] masterDialogueSplit = masterString.Split('#');
			int m = 0;
			string messageID;
			while (true)
			{
				if (m >= masterDialogueSplit.Length)
				{
					return;
				}
				if (masterDialogueSplit[m].Length >= 2)
				{
					masterDialogueSplit[m] = checkForSpecialCharacters(masterDialogueSplit[m]);
					string stringIdentifier;
					try
					{
						stringIdentifier = masterDialogueSplit[m].Substring(0, 2);
					}
					catch (Exception)
					{
						stringIdentifier = "     ";
					}
					if (!stringIdentifier.Equals("$e"))
					{
						if (stringIdentifier.Equals("$b"))
						{
							if (dialogues.Count > 0)
							{
								dialogues[dialogues.Count - 1] += "{";
							}
						}
						else if (stringIdentifier.Equals("$k"))
						{
							dialogueToBeKilled = true;
						}
						else if (stringIdentifier.Equals("$1") && masterDialogueSplit[m].Split(' ').Length > 1)
						{
							messageID = masterDialogueSplit[m].Split(' ')[1];
							if (!farmer.mailReceived.Contains(messageID))
							{
								break;
							}
							m += 3;
							if (m < masterDialogueSplit.Count())
							{
								masterDialogueSplit[m] = checkForSpecialCharacters(masterDialogueSplit[m]);
								dialogues.Add(masterDialogueSplit[m]);
							}
						}
						else if (stringIdentifier.Equals("$c") && masterDialogueSplit[m].Split(' ').Length > 1)
						{
							double chance = Convert.ToDouble(masterDialogueSplit[m].Split(' ')[1]);
							if (Game1.random.NextDouble() > chance)
							{
								m++;
							}
							else
							{
								dialogues.Add(masterDialogueSplit[m + 1]);
								m += 3;
							}
						}
						else if (stringIdentifier.Equals("$q"))
						{
							if (dialogues.Count > 0)
							{
								dialogues[dialogues.Count - 1] += "{";
							}
							string[] questionSplit = masterDialogueSplit[m].Split(' ');
							string[] answerIDs = questionSplit[1].Split('/');
							bool alreadySeenAnswer = false;
							for (int k = 0; k < answerIDs.Length; k++)
							{
								if (farmer.DialogueQuestionsAnswered.Contains(Convert.ToInt32(answerIDs[k])))
								{
									alreadySeenAnswer = true;
									break;
								}
							}
							if (alreadySeenAnswer && Convert.ToInt32(answerIDs[0]) != -1)
							{
								if (!questionSplit[2].Equals("null"))
								{
									masterDialogueSplit = masterDialogueSplit.Take(m).ToArray().Concat(speaker.Dialogue[questionSplit[2]].Split('#'))
										.ToArray();
									m--;
								}
							}
							else
							{
								isLastDialogueInteractive = true;
							}
						}
						else if (stringIdentifier.Equals("$r"))
						{
							string[] responseSplit = masterDialogueSplit[m].Split(' ');
							if (playerResponses == null)
							{
								playerResponses = new List<NPCDialogueResponse>();
							}
							isLastDialogueInteractive = true;
							playerResponses.Add(new NPCDialogueResponse(Convert.ToInt32(responseSplit[1]), Convert.ToInt32(responseSplit[2]), responseSplit[3], masterDialogueSplit[m + 1]));
							m++;
						}
						else if (stringIdentifier.Equals("$p"))
						{
							string[] prerequisiteSplit = masterDialogueSplit[m].Split(' ');
							string[] prerequisiteDialogueSplit = masterDialogueSplit[m + 1].Split('|');
							bool choseOne = false;
							for (int j = 1; j < prerequisiteSplit.Length; j++)
							{
								if (farmer.DialogueQuestionsAnswered.Contains(Convert.ToInt32(prerequisiteSplit[1])))
								{
									choseOne = true;
									break;
								}
							}
							if (choseOne)
							{
								masterDialogueSplit = prerequisiteDialogueSplit[0].Split('#');
								m = -1;
							}
							else
							{
								masterDialogueSplit[m + 1] = masterDialogueSplit[m + 1].Split('|').Last();
							}
						}
						else if (stringIdentifier.Equals("$d"))
						{
							string[] array = masterDialogueSplit[m].Split(' ');
							string prerequisiteDialogue = masterString.Substring(masterString.IndexOf('#') + 1);
							bool worldStateConfirmed = false;
							switch (array[1].ToLower())
							{
							case "joja":
								worldStateConfirmed = Game1.isLocationAccessible("JojaMart");
								break;
							case "cc":
							case "communitycenter":
								worldStateConfirmed = Game1.isLocationAccessible("CommunityCenter");
								break;
							case "bus":
								worldStateConfirmed = farmer.mailReceived.Contains("ccVault");
								break;
							case "kent":
								worldStateConfirmed = (Game1.year >= 2);
								break;
							}
							char toLookFor = prerequisiteDialogue.Contains('|') ? '|' : '#';
							masterDialogueSplit = ((!worldStateConfirmed) ? prerequisiteDialogue.Split(toLookFor)[1].Split('#') : prerequisiteDialogue.Split(toLookFor)[0].Split('#'));
							m--;
						}
						else if (stringIdentifier.Equals("$y"))
						{
							quickResponse = true;
							isLastDialogueInteractive = true;
							if (quickResponses == null)
							{
								quickResponses = new List<string>();
							}
							if (playerResponses == null)
							{
								playerResponses = new List<NPCDialogueResponse>();
							}
							string raw2 = masterDialogueSplit[m].Substring(masterDialogueSplit[m].IndexOf('\'') + 1);
							raw2 = raw2.Substring(0, raw2.Length - 1);
							string[] rawSplit = raw2.Split('_');
							dialogues.Add(rawSplit[0]);
							for (int i = 1; i < rawSplit.Length; i += 2)
							{
								playerResponses.Add(new NPCDialogueResponse(-1, -1, "quickResponse" + i, Game1.parseText(rawSplit[i])));
								quickResponses.Add(rawSplit[i + 1].Replace("*", "#$b#"));
							}
						}
						else if (masterDialogueSplit[m].Contains("^"))
						{
							if (farmer.IsMale)
							{
								dialogues.Add(masterDialogueSplit[m].Substring(0, masterDialogueSplit[m].IndexOf("^")));
							}
							else
							{
								dialogues.Add(masterDialogueSplit[m].Substring(masterDialogueSplit[m].IndexOf("^") + 1));
							}
						}
						else if (masterDialogueSplit[m].Contains("¦"))
						{
							if (farmer.IsMale)
							{
								dialogues.Add(masterDialogueSplit[m].Substring(0, masterDialogueSplit[m].IndexOf("¦")));
							}
							else
							{
								dialogues.Add(masterDialogueSplit[m].Substring(masterDialogueSplit[m].IndexOf("¦") + 1));
							}
						}
						else
						{
							dialogues.Add(masterDialogueSplit[m]);
						}
					}
				}
				m++;
			}
			masterDialogueSplit[m + 1] = checkForSpecialCharacters(masterDialogueSplit[m + 1]);
			dialogues.Add(messageID + "}" + masterDialogueSplit[m + 1]);
			m = 99999;
		}

		public void prepareDialogueForDisplay()
		{
			if (dialogues.Count > 0)
			{
				dialogues[currentDialogueIndex] = Utility.ParseGiftReveals(dialogues[currentDialogueIndex]);
			}
		}

		public string getCurrentDialogue()
		{
			if (currentDialogueIndex >= dialogues.Count || finishedLastDialogue)
			{
				return "";
			}
			showPortrait = true;
			if (speaker.Name.Equals("Dwarf") && !farmer.canUnderstandDwarves)
			{
				return convertToDwarvish(dialogues[currentDialogueIndex]);
			}
			if (temporaryDialogue != null)
			{
				return temporaryDialogue;
			}
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("}"))
			{
				farmer.mailReceived.Add(dialogues[currentDialogueIndex].Split('}')[0]);
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Substring(dialogues[currentDialogueIndex].IndexOf("}") + 1);
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$k", "");
			}
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains('[') && dialogues[currentDialogueIndex].IndexOf(']') > 0)
			{
				string items = dialogues[currentDialogueIndex].Substring(dialogues[currentDialogueIndex].IndexOf('[') + 1, dialogues[currentDialogueIndex].IndexOf(']') - dialogues[currentDialogueIndex].IndexOf('[') - 1);
				string[] split = items.Split(' ');
				int result = -1;
				if (int.TryParse(split[Game1.random.Next(split.Length)], out result) && Game1.objectInformation.ContainsKey(result))
				{
					farmer.addItemToInventoryBool(new Object(Vector2.Zero, result, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false), makeActiveObject: true);
					farmer.showCarrying();
				}
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("[" + items + "]", "");
			}
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$k"))
			{
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$k", "");
				dialogues.RemoveRange(currentDialogueIndex + 1, dialogues.Count - 1 - currentDialogueIndex);
				finishedLastDialogue = true;
			}
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Length > 1 && dialogues[currentDialogueIndex][0] == '%')
			{
				showPortrait = false;
				return dialogues[currentDialogueIndex].Substring(1);
			}
			if (dialogues.Count() <= 0)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.792");
			}
			return dialogues[currentDialogueIndex].Replace("%time", Game1.getTimeOfDayString(Game1.timeOfDay));
		}

		public bool isItemGrabDialogue()
		{
			if (dialogues.Count <= 0)
			{
				return false;
			}
			return dialogues[currentDialogueIndex].Contains('[');
		}

		public bool isOnFinalDialogue()
		{
			return currentDialogueIndex == dialogues.Count - 1;
		}

		public bool isDialogueFinished()
		{
			return finishedLastDialogue;
		}

		public string checkForSpecialCharacters(string str)
		{
			str = str.Replace("@", farmer.Name);
			str = str.Replace("%adj", adjectives[Game1.random.Next(adjectives.Length)].ToLower());
			if (str.Contains("%noun"))
			{
				str = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? (str.Substring(0, str.IndexOf("%noun") + "%noun".Length).Replace("%noun", nouns[Game1.random.Next(nouns.Length)]) + str.Substring(str.IndexOf("%noun") + "%noun".Length).Replace("%noun", nouns[Game1.random.Next(nouns.Length)])) : (str.Substring(0, str.IndexOf("%noun") + "%noun".Length).Replace("%noun", nouns[Game1.random.Next(nouns.Length)].ToLower()) + str.Substring(str.IndexOf("%noun") + "%noun".Length).Replace("%noun", nouns[Game1.random.Next(nouns.Length)].ToLower())));
			}
			str = str.Replace("%place", places[Game1.random.Next(places.Length)]);
			str = str.Replace("%name", randomName());
			str = str.Replace("%firstnameletter", farmer.Name.Substring(0, Math.Max(0, farmer.Name.Length / 2)));
			str = str.Replace("%band", Game1.samBandName);
			if (str.Contains("%book"))
			{
				str = str.Replace("%book", Game1.elliottBookName);
			}
			if (!string.IsNullOrEmpty(str) && str.Contains("%spouse"))
			{
				if (farmer.spouse != null)
				{
					string[] split = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions")[farmer.spouse].Split('/');
					str = str.Replace("%spouse", split[split.Length - 1]);
				}
				else if (farmer.team.GetSpouse(farmer.UniqueMultiplayerID).HasValue)
				{
					Farmer spouse = Game1.getFarmerMaybeOffline(farmer.team.GetSpouse(farmer.UniqueMultiplayerID).Value);
					str = str.Replace("%spouse", spouse.Name);
				}
			}
			str = str.Replace("%farm", farmer.farmName);
			str = str.Replace("%favorite", farmer.favoriteThing);
			int kids = farmer.getNumberOfChildren();
			str = str.Replace("%kid1", (kids > 0) ? farmer.getChildren()[0].displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.793"));
			str = str.Replace("%kid2", (kids > 1) ? farmer.getChildren()[1].displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.794"));
			str = str.Replace("%pet", farmer.getPetDisplayName());
			if (str.Contains("¦"))
			{
				str = (farmer.IsMale ? str.Substring(0, str.IndexOf("¦")) : str.Substring(str.IndexOf("¦") + 1));
			}
			if (str.Contains("%fork"))
			{
				str = str.Replace("%fork", "");
				if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.specialEventVariable1 = true;
				}
			}
			str = str.Replace("%rival", Utility.getOtherFarmerNames()[0].Split(' ')[1]);
			return str;
		}

		public static string randomName()
		{
			string name2 = "";
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				string[] names3 = new string[38]
				{
					"ロ\u30fcゼン",
					"ミルド",
					"ココ",
					"ナミ",
					"こころ",
					"サルコ",
					"ハンゾ\u30fc",
					"クッキ\u30fc",
					"ココナツ",
					"せん",
					"ハル",
					"ラン",
					"オサム",
					"ヨシ",
					"ソラ",
					"ホシ",
					"まこと",
					"マサ",
					"ナナ",
					"リオ",
					"リン",
					"フジ",
					"うどん",
					"ミント",
					"さくら",
					"ボンボン",
					"レオ",
					"モリ",
					"コ\u30fcヒ\u30fc",
					"ミルク",
					"マロン",
					"クルミ",
					"サムライ",
					"カミ",
					"ゴロ",
					"マル",
					"チビ",
					"ユキダマ"
				};
				name2 = names3[new Random().Next(0, names3.Length)];
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				string[] names2 = new string[183]
				{
					"雨果",
					"蛋挞",
					"小百合",
					"毛毛",
					"小雨",
					"小溪",
					"精灵",
					"安琪儿",
					"小糕",
					"玫瑰",
					"小黄",
					"晓雨",
					"阿江",
					"铃铛",
					"马琪",
					"果粒",
					"郁金香",
					"小黑",
					"雨露",
					"小江",
					"灵力",
					"萝拉",
					"豆豆",
					"小莲",
					"斑点",
					"小雾",
					"阿川",
					"丽丹",
					"玛雅",
					"阿豆",
					"花花",
					"琉璃",
					"滴答",
					"阿山",
					"丹麦",
					"梅西",
					"橙子",
					"花儿",
					"晓璃",
					"小夕",
					"山大",
					"咪咪",
					"卡米",
					"红豆",
					"花朵",
					"洋洋",
					"太阳",
					"小岩",
					"汪汪",
					"玛利亚",
					"小菜",
					"花瓣",
					"阳阳",
					"小夏",
					"石头",
					"阿狗",
					"邱洁",
					"苹果",
					"梨花",
					"小希",
					"天天",
					"浪子",
					"阿猫",
					"艾薇儿",
					"雪梨",
					"桃花",
					"阿喜",
					"云朵",
					"风儿",
					"狮子",
					"绮丽",
					"雪莉",
					"樱花",
					"小喜",
					"朵朵",
					"田田",
					"小红",
					"宝娜",
					"梅子",
					"小樱",
					"嘻嘻",
					"云儿",
					"小草",
					"小黄",
					"纳香",
					"阿梅",
					"茶花",
					"哈哈",
					"芸儿",
					"东东",
					"小羽",
					"哈豆",
					"桃子",
					"茶叶",
					"双双",
					"沫沫",
					"楠楠",
					"小爱",
					"麦当娜",
					"杏仁",
					"椰子",
					"小王",
					"泡泡",
					"小林",
					"小灰",
					"马格",
					"鱼蛋",
					"小叶",
					"小李",
					"晨晨",
					"小琳",
					"小慧",
					"布鲁",
					"晓梅",
					"绿叶",
					"甜豆",
					"小雪",
					"晓林",
					"康康",
					"安妮",
					"樱桃",
					"香板",
					"甜甜",
					"雪花",
					"虹儿",
					"美美",
					"葡萄",
					"薇儿",
					"金豆",
					"雪玲",
					"瑶瑶",
					"龙眼",
					"丁香",
					"晓云",
					"雪豆",
					"琪琪",
					"麦子",
					"糖果",
					"雪丽",
					"小艺",
					"小麦",
					"小圆",
					"雨佳",
					"小火",
					"麦茶",
					"圆圆",
					"春儿",
					"火灵",
					"板子",
					"黑点",
					"冬冬",
					"火花",
					"米粒",
					"喇叭",
					"晓秋",
					"跟屁虫",
					"米果",
					"欢欢",
					"爱心",
					"松子",
					"丫头",
					"双子",
					"豆芽",
					"小子",
					"彤彤",
					"棉花糖",
					"阿贵",
					"仙儿",
					"冰淇淋",
					"小彬",
					"贤儿",
					"冰棒",
					"仔仔",
					"格子",
					"水果",
					"悠悠",
					"莹莹",
					"巧克力",
					"梦洁",
					"汤圆",
					"静香",
					"茄子",
					"珍珠"
				};
				name2 = names2[new Random().Next(0, names2.Length)];
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
			{
				string[] names = new string[50]
				{
					"Августина",
					"Альф",
					"Анфиса",
					"Ариша",
					"Афоня",
					"Баламут",
					"Балкан",
					"Бандит",
					"Бланка",
					"Бобик",
					"Боня",
					"Борька",
					"Буренка",
					"Бусинка",
					"Вася",
					"Гаврюша",
					"Глаша",
					"Гоша",
					"Дуня",
					"Дуся",
					"Зорька",
					"Ивонна",
					"Игнат",
					"Кеша",
					"Клара",
					"Кузя",
					"Лада",
					"Максимус",
					"Маня",
					"Марта",
					"Маруся",
					"Моня",
					"Мотя",
					"Мурзик",
					"Мурка",
					"Нафаня",
					"Ника",
					"Нюша",
					"Проша",
					"Пятнушка",
					"Сеня",
					"Сивка",
					"Тихон",
					"Тоша",
					"Фунтик",
					"Шайтан",
					"Юнона",
					"Юпитер",
					"Ягодка",
					"Яшка"
				};
				name2 = names[new Random().Next(0, names.Length)];
			}
			else
			{
				int nameLength = Game1.random.Next(3, 6);
				string[] startingConsonants = new string[24]
				{
					"B",
					"Br",
					"J",
					"F",
					"S",
					"M",
					"C",
					"Ch",
					"L",
					"P",
					"K",
					"W",
					"G",
					"Z",
					"Tr",
					"T",
					"Gr",
					"Fr",
					"Pr",
					"N",
					"Sn",
					"R",
					"Sh",
					"St"
				};
				string[] consonants = new string[12]
				{
					"ll",
					"tch",
					"l",
					"m",
					"n",
					"p",
					"r",
					"s",
					"t",
					"c",
					"rt",
					"ts"
				};
				string[] vowels = new string[5]
				{
					"a",
					"e",
					"i",
					"o",
					"u"
				};
				string[] consonantEndings = new string[5]
				{
					"ie",
					"o",
					"a",
					"ers",
					"ley"
				};
				Dictionary<string, string[]> endings = new Dictionary<string, string[]>();
				Dictionary<string, string[]> endingsForShortNames = new Dictionary<string, string[]>();
				endings.Add("a", new string[6]
				{
					"nie",
					"bell",
					"bo",
					"boo",
					"bella",
					"s"
				});
				endings.Add("e", new string[4]
				{
					"ll",
					"llo",
					"",
					"o"
				});
				endings.Add("i", new string[18]
				{
					"ck",
					"e",
					"bo",
					"ba",
					"lo",
					"la",
					"to",
					"ta",
					"no",
					"na",
					"ni",
					"a",
					"o",
					"zor",
					"que",
					"ca",
					"co",
					"mi"
				});
				endings.Add("o", new string[12]
				{
					"nie",
					"ze",
					"dy",
					"da",
					"o",
					"ver",
					"la",
					"lo",
					"s",
					"ny",
					"mo",
					"ra"
				});
				endings.Add("u", new string[4]
				{
					"rt",
					"mo",
					"",
					"s"
				});
				endingsForShortNames.Add("a", new string[12]
				{
					"nny",
					"sper",
					"trina",
					"bo",
					"-bell",
					"boo",
					"lbert",
					"sko",
					"sh",
					"ck",
					"ishe",
					"rk"
				});
				endingsForShortNames.Add("e", new string[9]
				{
					"lla",
					"llo",
					"rnard",
					"cardo",
					"ffe",
					"ppo",
					"ppa",
					"tch",
					"x"
				});
				endingsForShortNames.Add("i", new string[18]
				{
					"llard",
					"lly",
					"lbo",
					"cky",
					"card",
					"ne",
					"nnie",
					"lbert",
					"nono",
					"nano",
					"nana",
					"ana",
					"nsy",
					"msy",
					"skers",
					"rdo",
					"rda",
					"sh"
				});
				endingsForShortNames.Add("o", new string[17]
				{
					"nie",
					"zzy",
					"do",
					"na",
					"la",
					"la",
					"ver",
					"ng",
					"ngus",
					"ny",
					"-mo",
					"llo",
					"ze",
					"ra",
					"ma",
					"cco",
					"z"
				});
				endingsForShortNames.Add("u", new string[11]
				{
					"ssie",
					"bbie",
					"ffy",
					"bba",
					"rt",
					"s",
					"mby",
					"mbo",
					"mbus",
					"ngus",
					"cky"
				});
				name2 += startingConsonants[Game1.random.Next(startingConsonants.Length - 1)];
				for (int j = 1; j < nameLength - 1; j++)
				{
					name2 = ((j % 2 != 0) ? (name2 + vowels[Game1.random.Next(vowels.Length)]) : (name2 + consonants[Game1.random.Next(consonants.Length)]));
					if (name2.Length >= nameLength)
					{
						break;
					}
				}
				if (Game1.random.NextDouble() < 0.5 && !vowels.Contains(name2.ElementAt(name2.Length - 1).ToString() ?? ""))
				{
					name2 += consonantEndings[Game1.random.Next(consonantEndings.Length)];
				}
				else if (vowels.Contains(name2.ElementAt(name2.Length - 1).ToString() ?? ""))
				{
					if (Game1.random.NextDouble() < 0.8)
					{
						name2 = ((name2.Length > 3) ? (name2 + endings[name2.ElementAt(name2.Length - 1).ToString() ?? ""].ElementAt(Game1.random.Next(endings[name2.ElementAt(name2.Length - 1).ToString() ?? ""].Length - 1))) : (name2 + endingsForShortNames[name2.ElementAt(name2.Length - 1).ToString() ?? ""].ElementAt(Game1.random.Next(endingsForShortNames[name2.ElementAt(name2.Length - 1).ToString() ?? ""].Length - 1))));
					}
				}
				else
				{
					name2 += vowels[Game1.random.Next(vowels.Length)];
				}
				for (int i = name2.Length - 1; i > 2; i--)
				{
					if (vowels.Contains(name2[i].ToString()) && vowels.Contains(name2[i - 2].ToString()))
					{
						switch (name2[i - 1])
						{
						case 'c':
							name2 = name2.Substring(0, i) + "k" + name2.Substring(i);
							i--;
							break;
						case 'r':
							name2 = name2.Substring(0, i - 1) + "k" + name2.Substring(i);
							i--;
							break;
						case 'l':
							name2 = name2.Substring(0, i - 1) + "n" + name2.Substring(i);
							i--;
							break;
						}
					}
				}
				if (name2.Length <= 3 && Game1.random.NextDouble() < 0.1)
				{
					name2 = ((Game1.random.NextDouble() < 0.5) ? (name2 + name2) : (name2 + "-" + name2));
				}
				if (name2.Length <= 2 && name2.Last() == 'e')
				{
					name2 += ((Game1.random.NextDouble() < 0.3) ? "m" : ((Game1.random.NextDouble() < 0.5) ? "p" : "b"));
				}
				if (name2.ToLower().Contains("sex") || name2.ToLower().Contains("taboo") || name2.ToLower().Contains("fuck") || name2.ToLower().Contains("rape") || name2.ToLower().Contains("cock") || name2.ToLower().Contains("willy") || name2.ToLower().Contains("cum") || name2.ToLower().Contains("goock") || name2.ToLower().Contains("trann") || name2.ToLower().Contains("gook") || name2.ToLower().Contains("bitch") || name2.ToLower().Contains("shit") || name2.ToLower().Contains("pusie") || name2.ToLower().Contains("kike") || name2.ToLower().Contains("nigg") || name2.ToLower().Contains("puss"))
				{
					name2 = ((Game1.random.NextDouble() < 0.5) ? "Bobo" : "Wumbus");
				}
			}
			return name2;
		}

		public string exitCurrentDialogue()
		{
			if (temporaryDialogue != null)
			{
				return null;
			}
			bool num = isCurrentStringContinuedOnNextScreen;
			if (currentDialogueIndex < dialogues.Count - 1)
			{
				currentDialogueIndex++;
				checkForSpecialDialogueAttributes();
			}
			else
			{
				finishedLastDialogue = true;
			}
			if (num)
			{
				return getCurrentDialogue();
			}
			return null;
		}

		private void checkForSpecialDialogueAttributes()
		{
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("{"))
			{
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("{", "");
				isCurrentStringContinuedOnNextScreen = true;
			}
			else
			{
				isCurrentStringContinuedOnNextScreen = false;
			}
			checkEmotions();
		}

		private void checkEmotions()
		{
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$h"))
			{
				currentEmotion = "$h";
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$h", "");
			}
			else if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$s"))
			{
				currentEmotion = "$s";
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$s", "");
			}
			else if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$u"))
			{
				currentEmotion = "$u";
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$u", "");
			}
			else if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$l"))
			{
				currentEmotion = "$l";
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$l", "");
			}
			else if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$a"))
			{
				currentEmotion = "$a";
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$a", "");
			}
			else if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$"))
			{
				int digits = (dialogues[currentDialogueIndex].Length <= dialogues[currentDialogueIndex].IndexOf("$") + 2 || !char.IsDigit(dialogues[currentDialogueIndex][dialogues[currentDialogueIndex].IndexOf("$") + 2])) ? 1 : 2;
				string emote = currentEmotion = dialogues[currentDialogueIndex].Substring(dialogues[currentDialogueIndex].IndexOf("$"), digits + 1);
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace(emote, "");
			}
			else
			{
				currentEmotion = "$neutral";
			}
		}

		public List<NPCDialogueResponse> getNPCResponseOptions()
		{
			return playerResponses;
		}

		public List<Response> getResponseOptions()
		{
			return new List<Response>(((IEnumerable<NPCDialogueResponse>)playerResponses).Select((Func<NPCDialogueResponse, Response>)((NPCDialogueResponse x) => x)));
		}

		public bool isCurrentDialogueAQuestion()
		{
			if (isLastDialogueInteractive)
			{
				return currentDialogueIndex == dialogues.Count - 1;
			}
			return false;
		}

		public bool chooseResponse(Response response)
		{
			for (int i = 0; i < playerResponses.Count; i++)
			{
				if (playerResponses[i].responseKey == null || response.responseKey == null || !playerResponses[i].responseKey.Equals(response.responseKey))
				{
					continue;
				}
				if (answerQuestionBehavior != null)
				{
					if (answerQuestionBehavior(i))
					{
						Game1.currentSpeaker = null;
					}
					isLastDialogueInteractive = false;
					finishedLastDialogue = true;
					answerQuestionBehavior = null;
					return true;
				}
				if (quickResponse)
				{
					isLastDialogueInteractive = false;
					finishedLastDialogue = true;
					isCurrentStringContinuedOnNextScreen = true;
					speaker.setNewDialogue(quickResponses[i]);
					Game1.drawDialogue(speaker);
					speaker.faceTowardFarmerForPeriod(4000, 3, faceAway: false, farmer);
					return true;
				}
				if (Game1.isFestival())
				{
					Game1.currentLocation.currentEvent.answerDialogueQuestion(speaker, playerResponses[i].responseKey);
					isLastDialogueInteractive = false;
					finishedLastDialogue = true;
					return false;
				}
				farmer.changeFriendship(playerResponses[i].friendshipChange, speaker);
				if (playerResponses[i].id != -1)
				{
					farmer.addSeenResponse(playerResponses[i].id);
				}
				isLastDialogueInteractive = false;
				finishedLastDialogue = false;
				parseDialogueString(speaker.Dialogue[playerResponses[i].responseKey]);
				isCurrentStringContinuedOnNextScreen = true;
				return false;
			}
			return false;
		}

		public static string convertToDwarvish(string str)
		{
			string translated = "";
			for (int i = 0; i < str.Length; i++)
			{
				switch (str[i])
				{
				case 'a':
					translated += "o";
					continue;
				case 'e':
					translated += "u";
					continue;
				case 'i':
					translated += "e";
					continue;
				case 'o':
					translated += "a";
					continue;
				case 'u':
					translated += "i";
					continue;
				case 'y':
					translated += "ol";
					continue;
				case 'z':
					translated += "b";
					continue;
				case 'A':
					translated += "O";
					continue;
				case 'E':
					translated += "U";
					continue;
				case 'I':
					translated += "E";
					continue;
				case 'O':
					translated += "A";
					continue;
				case 'U':
					translated += "I";
					continue;
				case 'Y':
					translated += "Ol";
					continue;
				case 'Z':
					translated += "B";
					continue;
				case '1':
					translated += "M";
					continue;
				case '5':
					translated += "X";
					continue;
				case '9':
					translated += "V";
					continue;
				case '0':
					translated += "Q";
					continue;
				case 'g':
					translated += "l";
					continue;
				case 'c':
					translated += "t";
					continue;
				case 't':
					translated += "n";
					continue;
				case 'd':
					translated += "p";
					continue;
				case ' ':
				case '!':
				case '"':
				case '\'':
				case ',':
				case '.':
				case '?':
				case 'h':
				case 'm':
				case 's':
					translated += str[i].ToString();
					continue;
				case '\n':
				case 'n':
				case 'p':
					continue;
				}
				if (char.IsLetterOrDigit(str[i]))
				{
					translated += ((char)(str[i] + 2)).ToString();
				}
			}
			return translated.Replace("nhu", "doo");
		}
	}
}
