using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace StardewValley.Events
{
	public class QuestionEvent : FarmEvent, INetObject<NetFields>
	{
		public const int pregnancyQuestion = 1;

		public const int barnBirth = 2;

		public const int playerPregnancyQuestion = 3;

		private int whichQuestion;

		private AnimalHouse animalHouse;

		public FarmAnimal animal;

		public bool forceProceed;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public QuestionEvent(int whichQuestion)
		{
			this.whichQuestion = whichQuestion;
		}

		public bool setUp()
		{
			switch (whichQuestion)
			{
			case 1:
			{
				Response[] answers = new Response[2]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_No"))
				};
				if (!Game1.getCharacterFromName(Game1.player.spouse).isGaySpouse())
				{
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HaveBabyQuestion", Game1.player.Name), answers, answerPregnancyQuestion, Game1.getCharacterFromName(Game1.player.spouse));
				}
				else
				{
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HaveBabyQuestion_Adoption", Game1.player.Name), answers, answerPregnancyQuestion, Game1.getCharacterFromName(Game1.player.spouse));
				}
				Game1.messagePause = true;
				return false;
			}
			case 2:
			{
				FarmAnimal a = null;
				foreach (Building b in Game1.getFarm().buildings)
				{
					if ((b.owner.Equals(Game1.player.UniqueMultiplayerID) || !Game1.IsMultiplayer) && b.buildingType.Contains("Barn") && !b.buildingType.Equals("Barn") && !(b.indoors.Value as AnimalHouse).isFull() && Game1.random.NextDouble() < (double)(b.indoors.Value as AnimalHouse).animalsThatLiveHere.Count * 0.0055)
					{
						a = Utility.getAnimal((b.indoors.Value as AnimalHouse).animalsThatLiveHere[Game1.random.Next((b.indoors.Value as AnimalHouse).animalsThatLiveHere.Count)]);
						animalHouse = (b.indoors.Value as AnimalHouse);
						break;
					}
				}
				if (a != null && !a.isBaby() && (bool)a.allowReproduction)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Events:AnimalBirth", a.displayName, a.shortDisplayType()));
					Game1.messagePause = true;
					animal = a;
					return false;
				}
				break;
			}
			case 3:
			{
				Response[] answers2 = new Response[2]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_No"))
				};
				long spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				Farmer spouse = Game1.otherFarmers[spouseID];
				if (spouse.IsMale != Game1.player.IsMale)
				{
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion", spouse.Name), answers2, answerPlayerPregnancyQuestion);
				}
				else
				{
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion_Adoption", spouse.Name), answers2, answerPlayerPregnancyQuestion);
				}
				Game1.messagePause = true;
				return false;
			}
			}
			return true;
		}

		private void answerPregnancyQuestion(Farmer who, string answer)
		{
			if (answer.Equals("Yes"))
			{
				WorldDate birthingDate = new WorldDate(Game1.Date);
				birthingDate.TotalDays += 14;
				who.GetSpouseFriendship().NextBirthingDate = birthingDate;
				Game1.getCharacterFromName(who.spouse).isGaySpouse();
			}
		}

		private void answerPlayerPregnancyQuestion(Farmer who, string answer)
		{
			if (answer.Equals("Yes"))
			{
				long spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				Farmer spouse = Game1.otherFarmers[spouseID];
				Game1.player.team.SendProposal(spouse, ProposalType.Baby);
			}
		}

		public bool tickUpdate(GameTime time)
		{
			if (forceProceed)
			{
				return true;
			}
			if (whichQuestion == 2 && !Game1.dialogueUp)
			{
				if (Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new NamingMenu(animalHouse.addNewHatchedAnimal, (animal != null) ? Game1.content.LoadString("Strings\\Events:AnimalNamingTitle", animal.displayType) : Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestionEvent.cs.6692"));
				}
				return false;
			}
			return !Game1.dialogueUp;
		}

		public void draw(SpriteBatch b)
		{
		}

		public void drawAboveEverything(SpriteBatch b)
		{
		}

		public void makeChangesToLocation()
		{
			Game1.messagePause = false;
		}
	}
}
