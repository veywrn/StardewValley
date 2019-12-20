using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace StardewValley.Events
{
	public class PlayerCoupleBirthingEvent : FarmEvent, INetObject<NetFields>
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

		private FarmHouse farmHouse;

		private long spouseID;

		private Farmer spouse;

		private bool isPlayersTurn;

		private Child child;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public PlayerCoupleBirthingEvent()
		{
			spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
			Game1.otherFarmers.TryGetValue(spouseID, out spouse);
			farmHouse = chooseHome();
			if (farmHouse.getChildren().Count >= 1)
			{
				Game1.getSteamAchievement("Achievement_FullHouse");
			}
		}

		private bool isSuitableHome(FarmHouse home)
		{
			if (home.getChildrenCount() < 2)
			{
				return home.upgradeLevel >= 2;
			}
			return false;
		}

		private FarmHouse chooseHome()
		{
			List<Farmer> parents = new List<Farmer>();
			parents.Add(Game1.player);
			parents.Add(spouse);
			parents.Sort((Farmer p1, Farmer p2) => p1.UniqueMultiplayerID.CompareTo(p2.UniqueMultiplayerID));
			foreach (Farmer parent in parents)
			{
				FarmHouse home2 = Game1.getLocationFromName(parent.homeLocation) as FarmHouse;
				if (home2 != null && home2 == parent.currentLocation && isSuitableHome(home2))
				{
					return home2;
				}
			}
			foreach (Farmer item in parents)
			{
				FarmHouse home = Game1.getLocationFromName(item.homeLocation) as FarmHouse;
				if (home != null && isSuitableHome(home))
				{
					return home;
				}
			}
			return Game1.player.currentLocation as FarmHouse;
		}

		public bool setUp()
		{
			if (spouse == null || farmHouse == null)
			{
				return true;
			}
			Random r = new Random((int)Game1.uniqueIDForThisGame ^ Game1.Date.TotalDays);
			Game1.player.CanMove = false;
			if (farmHouse.getChildrenCount() == 0)
			{
				isMale = (r.NextDouble() < 0.5);
			}
			else
			{
				isMale = (farmHouse.getChildren()[0].Gender == 1);
			}
			Friendship friendship = Game1.player.GetSpouseFriendship();
			isPlayersTurn = (friendship.Proposer != Game1.player.UniqueMultiplayerID == (farmHouse.getChildrenCount() % 2 == 0));
			if (spouse.IsMale == Game1.player.IsMale)
			{
				message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(isMale));
			}
			else if (spouse.IsMale)
			{
				message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(isMale));
			}
			else
			{
				message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(isMale), spouse.Name);
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
			if (isPlayersTurn)
			{
				getBabyName = true;
				double chance = spouse.hasDarkSkin() ? 0.5 : 0.0;
				chance += (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
				bool isDarkSkinned = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed).NextDouble() < chance;
				farmHouse.characters.Add(child = new Child("Baby", isMale, isDarkSkinned, Game1.player));
				child.Age = 0;
				child.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
				Game1.player.GetSpouseFriendship().NextBirthingDate = null;
			}
			else
			{
				Game1.afterDialogues = delegate
				{
					getBabyName = true;
				};
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseNaming_" + (isMale ? "Male" : "Female"), spouse.Name));
			}
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
				if (!isPlayersTurn)
				{
					Game1.globalFadeToClear();
					return true;
				}
				if (!naming)
				{
					Game1.activeClickableMenu = new NamingMenu(returnBabyName, Game1.content.LoadString(isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
					naming = true;
				}
				if (babyName != null && babyName != "" && babyName.Length > 0)
				{
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
					child.Name = newBabyName;
					Game1.playSound("smallSelect");
					if (Game1.keyboardDispatcher != null)
					{
						Game1.keyboardDispatcher.Subscriber = null;
					}
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
