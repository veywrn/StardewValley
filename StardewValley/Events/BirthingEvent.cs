using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Menus;
using System;

namespace StardewValley.Events
{
	public class BirthingEvent : FarmEvent, INetObject<NetFields>
	{
		private int behavior;

		private int timer;

		private string soundName;

		private string message;

		private string babyName;

		private bool playedSound;

		private bool showedMessage;

		private bool isMale;

		private bool getBabyName;

		private bool naming;

		private Vector2 targetLocation;

		private TextBox babyNameBox;

		private ClickableTextureComponent okButton;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public bool setUp()
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			NPC spouse = Game1.getCharacterFromName(Game1.player.spouse);
			Game1.player.CanMove = false;
			if (Game1.player.getNumberOfChildren() == 0)
			{
				isMale = (r.NextDouble() < 0.5);
			}
			else
			{
				isMale = (Game1.player.getChildren()[0].Gender == 1);
			}
			if (spouse.isGaySpouse())
			{
				message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(isMale));
			}
			else if (spouse.Gender == 0)
			{
				message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(isMale));
			}
			else
			{
				message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(isMale), spouse.displayName);
			}
			return false;
		}

		public void returnBabyName(string name)
		{
			babyName = name;
			Game1.exitActiveMenu();
		}

		public void afterMessage()
		{
			getBabyName = true;
		}

		public bool tickUpdate(GameTime time)
		{
			Game1.player.CanMove = false;
			timer += time.ElapsedGameTime.Milliseconds;
			Game1.fadeToBlackAlpha = 1f;
			if (timer > 1500 && !playedSound && !getBabyName)
			{
				if (soundName != null && !soundName.Equals(""))
				{
					Game1.playSound(soundName);
					playedSound = true;
				}
				if (!playedSound && message != null && !Game1.dialogueUp && Game1.activeClickableMenu == null)
				{
					Game1.drawObjectDialogue(message);
					Game1.afterDialogues = afterMessage;
				}
			}
			else if (getBabyName)
			{
				if (!naming)
				{
					Game1.activeClickableMenu = new NamingMenu(returnBabyName, Game1.content.LoadString(isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
					naming = true;
				}
				if (babyName != null && babyName != "" && babyName.Length > 0)
				{
					double chance2 = Game1.player.spouse.Equals("Maru") ? 0.5 : 0.0;
					chance2 += (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
					bool isDarkSkinned = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed).NextDouble() < chance2;
					string newBabyName = babyName;
					DisposableList<NPC> all_characters = Utility.getAllCharacters();
					bool collision_found2 = false;
					do
					{
						collision_found2 = false;
						foreach (NPC item in all_characters)
						{
							if (item.name.Equals(newBabyName))
							{
								newBabyName += " ";
								collision_found2 = true;
								break;
							}
						}
					}
					while (collision_found2);
					Child baby = new Child(newBabyName, isMale, isDarkSkinned, Game1.player);
					baby.Age = 0;
					baby.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
					Utility.getHomeOfFarmer(Game1.player).characters.Add(baby);
					Game1.playSound("smallSelect");
					Game1.player.getSpouse().daysAfterLastBirth = 5;
					Game1.player.GetSpouseFriendship().NextBirthingDate = null;
					if (Game1.player.getChildrenCount() == 2)
					{
						Game1.player.getSpouse().shouldSayMarriageDialogue.Value = true;
						Game1.player.getSpouse().currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_SecondChild" + Game1.random.Next(1, 3), true));
						Game1.getSteamAchievement("Achievement_FullHouse");
					}
					else if (Game1.player.getSpouse().isGaySpouse())
					{
						Game1.player.getSpouse().currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_Adoption", true, babyName));
					}
					else
					{
						Game1.player.getSpouse().currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_FirstChild", true, babyName));
					}
					Game1.morningQueue.Enqueue(delegate
					{
						Game1.multiplayer.globalChatInfoMessage("Baby", Lexicon.capitalize(Game1.player.Name), Game1.player.spouse, Lexicon.getGenderedChildTerm(isMale), Lexicon.getPronoun(isMale), baby.displayName);
					});
					if (Game1.keyboardDispatcher != null)
					{
						Game1.keyboardDispatcher.Subscriber = null;
					}
					Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).GetPlayerBedSpot()) * 64f;
					Game1.globalFadeToClear();
					return true;
				}
			}
			return false;
		}

		public void draw(SpriteBatch b)
		{
		}

		public void makeChangesToLocation()
		{
		}

		public void drawAboveEverything(SpriteBatch b)
		{
		}
	}
}
