using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using xTile.Dimensions;
using xTile.ObjectModel;

namespace StardewValley.Locations
{
	public class JojaMart : GameLocation
	{
		public const int JojaMembershipPrice = 5000;

		public static NPC Morris;

		private Texture2D communityDevelopmentTexture;

		public JojaMart()
		{
		}

		public JojaMart(string map, string name)
			: base(map, name)
		{
		}

		private bool signUpForJoja(int response)
		{
			if (response == 0)
			{
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:JojaMart_SignUp")), createYesNoResponses(), "JojaSignUp");
				return true;
			}
			Game1.dialogueUp = false;
			Game1.player.forceCanMove();
			localSound("smallSelect");
			Game1.currentSpeaker = null;
			Game1.dialogueTyping = false;
			return true;
		}

		public override bool answerDialogue(Response answer)
		{
			if (lastQuestionKey != null && afterQuestion == null)
			{
				string questionAndAnswer = lastQuestionKey.Split(' ')[0] + "_" + answer.responseKey;
				if (questionAndAnswer == "JojaSignUp_Yes")
				{
					if (Game1.player.Money >= 5000)
					{
						Game1.player.Money -= 5000;
						Game1.addMailForTomorrow("JojaMember", noLetter: true, sendToEveryone: true);
						Game1.player.removeQuest(26);
						Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_PlayerSignedUp"));
						Game1.drawDialogue(Morris);
					}
					else if (Game1.player.Money < 5000)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
					}
					return true;
				}
			}
			return base.answerDialogue(answer);
		}

		public override void cleanupBeforePlayerExit()
		{
			if (!Game1.isRaining)
			{
				Game1.changeMusicTrack("none");
			}
			base.cleanupBeforePlayerExit();
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				PropertyValue action = "";
				map.GetLayer("Buildings").Tiles[tileLocation].Properties.TryGetValue("Action", out action);
				if (action != null)
				{
					string a = action.ToString();
					if (!(a == "JojaShop") && a == "JoinJoja")
					{
						Morris.CurrentDialogue.Clear();
						if (Game1.player.mailForTomorrow.Contains("JojaMember%&NL&%"))
						{
							Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_ComeBackLater"));
							Game1.drawDialogue(Morris);
						}
						else if (!Game1.player.mailReceived.Contains("JojaMember"))
						{
							if (!Game1.player.mailReceived.Contains("JojaGreeting"))
							{
								Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_Greeting"));
								Game1.player.mailReceived.Add("JojaGreeting");
								Game1.drawDialogue(Morris);
							}
							else if (Game1.stats.DaysPlayed < 0)
							{
								string greeting2 = (Game1.dayOfMonth % 7 == 0 || Game1.dayOfMonth % 7 == 6) ? "Data\\ExtraDialogue:Morris_WeekendGreeting" : "Data\\ExtraDialogue:Morris_FirstGreeting";
								Morris.setNewDialogue(Game1.content.LoadString(greeting2));
								Game1.drawDialogue(Morris);
							}
							else
							{
								string greeting = (Game1.dayOfMonth % 7 == 0 || Game1.dayOfMonth % 7 == 6) ? "Data\\ExtraDialogue:Morris_WeekendGreeting" : "Data\\ExtraDialogue:Morris_FirstGreeting";
								if (Game1.IsMasterGame)
								{
									if (!Game1.player.eventsSeen.Contains(611439))
									{
										Morris.setNewDialogue(Game1.content.LoadString(greeting));
										Game1.drawDialogue(Morris);
									}
									else if (Game1.player.mailReceived.Contains("ccIsComplete"))
									{
										Morris.setNewDialogue(Game1.content.LoadString(greeting + "_CommunityCenterComplete"));
										Game1.drawDialogue(Morris);
									}
									else
									{
										Morris.setNewDialogue(Game1.content.LoadString(greeting + "_MembershipAvailable", 5000));
										Morris.CurrentDialogue.Peek().answerQuestionBehavior = signUpForJoja;
										Game1.drawDialogue(Morris);
									}
								}
								else
								{
									Morris.setNewDialogue(Game1.content.LoadString(greeting + "_SecondPlayer"));
									Game1.drawDialogue(Morris);
								}
							}
						}
						else
						{
							if (Game1.player.eventsSeen.Contains(502261) && !Game1.player.hasOrWillReceiveMail("ccMovieTheater"))
							{
								Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_BuyMovieTheater"));
								Morris.CurrentDialogue.Peek().answerQuestionBehavior = buyMovieTheater;
							}
							else if (Game1.player.mailForTomorrow.Contains("jojaFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaVault%&NL&%"))
							{
								Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_StillProcessingOrder"));
							}
							else if (Game1.player.eventsSeen.Contains(502261))
							{
								Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_NoMoreCD"));
							}
							else
							{
								if (Game1.player.IsMale)
								{
									Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_CommunityDevelopmentForm_PlayerMale"));
								}
								else
								{
									Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_CommunityDevelopmentForm_PlayerFemale"));
								}
								Morris.CurrentDialogue.Peek().answerQuestionBehavior = viewJojaNote;
							}
							Game1.drawDialogue(Morris);
						}
					}
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		private bool buyMovieTheater(int response)
		{
			if (response == 0)
			{
				if (Game1.player.Money >= 500000)
				{
					Game1.player.Money -= 500000;
					Game1.addMailForTomorrow("ccMovieTheater", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccMovieTheaterJoja", noLetter: true, sendToEveryone: true);
					if (Game1.player.team.theaterBuildDate.Value < 0)
					{
						Game1.player.team.theaterBuildDate.Set(Game1.Date.TotalDays + 1);
					}
					Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_TheaterBought"));
					Game1.drawDialogue(Morris);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"));
				}
			}
			return true;
		}

		private bool viewJojaNote(int response)
		{
			if (response == 0)
			{
				Game1.activeClickableMenu = new JojaCDMenu(communityDevelopmentTexture);
				if (!Game1.player.activeDialogueEvents.ContainsKey("joja_Begin"))
				{
					Game1.player.activeDialogueEvents.Add("joja_Begin", 7);
				}
			}
			Game1.dialogueUp = false;
			Game1.player.forceCanMove();
			localSound("smallSelect");
			Game1.currentSpeaker = null;
			Game1.dialogueTyping = false;
			return true;
		}

		protected override void resetLocalState()
		{
			communityDevelopmentTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JojaCDForm");
			Morris = new NPC(null, Vector2.Zero, "JojaMart", 2, "Morris", datable: false, null, Game1.temporaryContent.Load<Texture2D>("Portraits\\Morris"));
			base.resetLocalState();
		}
	}
}
