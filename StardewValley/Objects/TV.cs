using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StardewValley.Objects
{
	public class TV : Furniture
	{
		public const int customChannel = 1;

		public const int weatherChannel = 2;

		public const int fortuneTellerChannel = 3;

		public const int tipsChannel = 4;

		public const int cookingChannel = 5;

		public const int fishingChannel = 6;

		private int currentChannel;

		private TemporaryAnimatedSprite screen;

		private TemporaryAnimatedSprite screenOverlay;

		private static Dictionary<int, string> weekToRecipeMap;

		public TV()
		{
		}

		public TV(int which, Vector2 tile)
			: base(which, tile)
		{
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			List<Response> channels = new List<Response>();
			channels.Add(new Response("Weather", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13105")));
			channels.Add(new Response("Fortune", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13107")));
			string day = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
			if (day.Equals("Mon") || day.Equals("Thu"))
			{
				channels.Add(new Response("Livin'", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13111")));
			}
			if (day.Equals("Sun"))
			{
				channels.Add(new Response("The", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13114")));
			}
			if (day.Equals("Wed") && Game1.stats.DaysPlayed > 7)
			{
				channels.Add(new Response("The", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13117")));
			}
			if (Game1.Date.Season == "fall" && Game1.Date.DayOfMonth == 26 && Game1.stats.getStat("childrenTurnedToDoves") != 0 && !who.mailReceived.Contains("cursed_doll"))
			{
				channels.Add(new Response("???", "???"));
			}
			if (Game1.player.mailReceived.Contains("pamNewChannel"))
			{
				channels.Add(new Response("Fishing", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel")));
			}
			channels.Add(new Response("(Leave)", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13118")));
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13120"), channels.ToArray(), selectChannel);
			Game1.player.Halt();
			return true;
		}

		public override Item getOne()
		{
			TV tV = new TV(parentSheetIndex, tileLocation);
			tV.drawPosition.Value = drawPosition;
			tV.defaultBoundingBox.Value = defaultBoundingBox;
			tV.boundingBox.Value = boundingBox;
			tV.currentRotation.Value = (int)currentRotation - 1;
			tV.rotations.Value = rotations;
			tV.rotate();
			tV._GetOneFrom(this);
			return tV;
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			base.updateWhenCurrentLocation(time, environment);
		}

		public virtual void selectChannel(Farmer who, string answer)
		{
			string text = answer.Split(' ')[0];
			switch (text)
			{
			case "Weather":
				currentChannel = 2;
				screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 305, 42, 28), 150f, 2, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(getWeatherChannelOpening()));
				Game1.afterDialogues = proceedToNextScene;
				return;
			case "Fortune":
				currentChannel = 3;
				screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 305, 42, 28), 150f, 2, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(getFortuneTellerOpening()));
				Game1.afterDialogues = proceedToNextScene;
				return;
			case "Livin'":
				currentChannel = 4;
				screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(517, 361, 42, 28), 150f, 2, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13124")));
				Game1.afterDialogues = proceedToNextScene;
				return;
			case "The":
				currentChannel = 5;
				screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(602, 361, 42, 28), 150f, 2, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13127")));
				Game1.afterDialogues = proceedToNextScene;
				return;
			case "???":
				Game1.changeMusicTrack("none");
				currentChannel = 666;
				screen = new TemporaryAnimatedSprite("Maps\\springobjects", new Rectangle(112, 64, 16, 16), 150f, 1, 999999, getScreenPosition() + (((int)parentSheetIndex == 1468) ? new Vector2(56f, 32f) : new Vector2(8f, 8f)), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, 3f, 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Cursed_Doll")));
				Game1.afterDialogues = proceedToNextScene;
				return;
			}
			if (text == "Fishing")
			{
				currentChannel = 6;
				screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(172, 33, 42, 28), 150f, 2, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Fishing_Channel_Intro")));
				Game1.afterDialogues = proceedToNextScene;
			}
		}

		protected virtual string getFortuneTellerOpening()
		{
			switch (Game1.random.Next(5))
			{
			case 0:
				if (!Game1.player.IsMale)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13130");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13128");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13132");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13133");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13134");
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13135");
			default:
				return "";
			}
		}

		protected virtual string getWeatherChannelOpening()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13136");
		}

		public virtual float getScreenSizeModifier()
		{
			if ((int)parentSheetIndex != 1468 && (int)parentSheetIndex != 2326)
			{
				return 2f;
			}
			return 4f;
		}

		public virtual Vector2 getScreenPosition()
		{
			if ((int)parentSheetIndex == 1466)
			{
				return new Vector2(boundingBox.X + 24, boundingBox.Y);
			}
			if ((int)parentSheetIndex == 1468)
			{
				return new Vector2(boundingBox.X + 12, boundingBox.Y - 128 + 32);
			}
			if ((int)parentSheetIndex == 2326)
			{
				return new Vector2(boundingBox.X + 12, boundingBox.Y - 128 + 40);
			}
			if ((int)parentSheetIndex == 1680)
			{
				return new Vector2(boundingBox.X + 24, boundingBox.Y - 12);
			}
			return Vector2.Zero;
		}

		public virtual void proceedToNextScene()
		{
			if (currentChannel == 2)
			{
				if (screenOverlay == null)
				{
					screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(497, 305, 42, 28), 9999f, 1, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f)
					{
						id = 777f
					};
					Game1.drawObjectDialogue(Game1.parseText(getWeatherForecast()));
					setWeatherOverlay();
					Game1.afterDialogues = proceedToNextScene;
				}
				else if (Game1.player.hasOrWillReceiveMail("Visited_Island") && screen.id == 777f)
				{
					screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(148, 62, 42, 28), 9999f, 1, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
					Game1.drawObjectDialogue(Game1.parseText(getIslandWeatherForecast()));
					setWeatherOverlay(island: true);
					Game1.afterDialogues = proceedToNextScene;
				}
				else
				{
					turnOffTV();
				}
			}
			else if (currentChannel == 3)
			{
				if (screenOverlay == null)
				{
					screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(624, 305, 42, 28), 9999f, 1, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
					Game1.drawObjectDialogue(Game1.parseText(getFortuneForecast(Game1.player)));
					setFortuneOverlay(Game1.player);
					Game1.afterDialogues = proceedToNextScene;
				}
				else
				{
					turnOffTV();
				}
			}
			else if (currentChannel == 4)
			{
				if (screenOverlay == null)
				{
					Game1.drawObjectDialogue(Game1.parseText(getTodaysTip()));
					Game1.afterDialogues = proceedToNextScene;
					screenOverlay = new TemporaryAnimatedSprite
					{
						alpha = 1E-07f
					};
				}
				else
				{
					turnOffTV();
				}
			}
			else if (currentChannel == 5)
			{
				if (screenOverlay == null)
				{
					Game1.multipleDialogues(getWeeklyRecipe());
					Game1.afterDialogues = proceedToNextScene;
					screenOverlay = new TemporaryAnimatedSprite
					{
						alpha = 1E-07f
					};
				}
				else
				{
					turnOffTV();
				}
			}
			else if (currentChannel == 666)
			{
				Game1.flashAlpha = 1f;
				Game1.soundBank.PlayCue("batScreech");
				Game1.createItemDebris(new Object(103, 1), Game1.player.getStandingPosition(), 1, Game1.currentLocation);
				Game1.player.mailReceived.Add("cursed_doll");
				turnOffTV();
			}
			else if (currentChannel == 6)
			{
				if (screenOverlay == null)
				{
					Game1.multipleDialogues(getFishingInfo());
					Game1.afterDialogues = proceedToNextScene;
					screenOverlay = new TemporaryAnimatedSprite
					{
						alpha = 1E-07f
					};
				}
				else
				{
					turnOffTV();
				}
			}
		}

		public virtual void turnOffTV()
		{
			screen = null;
			screenOverlay = null;
		}

		protected virtual void setWeatherOverlay(bool island = false)
		{
			WorldDate tomorrow = new WorldDate(Game1.Date);
			int num = ++tomorrow.TotalDays;
			switch (island ? Game1.netWorldState.Value.GetWeatherForLocation(Game1.getLocationFromName("IslandSouth").GetLocationContext()).weatherForTomorrow.Value : ((!Game1.IsMasterGame) ? Game1.getWeatherModificationsForDate(tomorrow, Game1.netWorldState.Value.WeatherForTomorrow) : Game1.getWeatherModificationsForDate(tomorrow, Game1.weatherForTomorrow)))
			{
			case 0:
			case 6:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 333, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			case 5:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(465, 346, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			case 1:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(465, 333, 13, 13), 70f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			case 2:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.currentSeason.Equals("spring") ? new Rectangle(465, 359, 13, 13) : (Game1.currentSeason.Equals("fall") ? new Rectangle(413, 359, 13, 13) : new Rectangle(465, 346, 13, 13)), 70f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			case 3:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 346, 13, 13), 120f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			case 4:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 372, 13, 13), 120f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			}
		}

		private string[] getFishingInfo()
		{
			List<string> allDialogues = new List<string>();
			StringBuilder sb = new StringBuilder();
			StringBuilder singleLineSB = new StringBuilder();
			int currentSeasonNumber = Utility.getSeasonNumber(Game1.currentSeason);
			sb.AppendLine("---" + Utility.getSeasonNameFromNumber(currentSeasonNumber) + "---^^");
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			Dictionary<string, string> locationsData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
			List<string> locationsFound = new List<string>();
			int count = 0;
			foreach (KeyValuePair<int, string> v in dictionary)
			{
				if (!v.Value.Contains("spring summer fall winter"))
				{
					locationsFound.Clear();
					foreach (KeyValuePair<string, string> i in locationsData)
					{
						if (i.Value.Split('/')[4 + currentSeasonNumber].Contains(string.Concat(v.Key)) && !locationsFound.Contains(getSanitizedFishingLocation(i.Key)))
						{
							locationsFound.Add(getSanitizedFishingLocation(i.Key));
						}
					}
					if (locationsFound.Count > 0)
					{
						string[] split = v.Value.Split('/');
						string name = (split.Count() > 13) ? split[13] : split[0];
						string weather = split[7];
						string lowerTime = split[5].Split(' ')[0];
						string upperTime = split[5].Split(' ')[1];
						singleLineSB.Append(name);
						singleLineSB.Append("...... ");
						singleLineSB.Append(Game1.getTimeOfDayString(Convert.ToInt32(lowerTime)).Replace(" ", ""));
						singleLineSB.Append("-");
						singleLineSB.Append(Game1.getTimeOfDayString(Convert.ToInt32(upperTime)).Replace(" ", ""));
						if (weather != "both")
						{
							singleLineSB.Append(", " + Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_" + weather));
						}
						bool anySanitized = false;
						foreach (string s in locationsFound)
						{
							if (s != "")
							{
								anySanitized = true;
								singleLineSB.Append(", ");
								singleLineSB.Append(s);
							}
						}
						if (anySanitized)
						{
							singleLineSB.Append("^^");
							sb.Append(singleLineSB.ToString());
							count++;
						}
						singleLineSB.Clear();
						if (count > 3)
						{
							allDialogues.Add(sb.ToString());
							sb.Clear();
							count = 0;
						}
					}
				}
			}
			return allDialogues.ToArray();
		}

		private string getSanitizedFishingLocation(string rawLocationName)
		{
			if (!(rawLocationName == "Town") && !(rawLocationName == "Forest"))
			{
				if (!(rawLocationName == "Beach"))
				{
					if (rawLocationName == "Mountain")
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_Lake");
					}
					return "";
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_Ocean");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_River");
		}

		protected virtual string getTodaysTip()
		{
			Dictionary<string, string> tips = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\TipChannel");
			if (!tips.ContainsKey(string.Concat(Game1.stats.DaysPlayed % 224u)))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13148");
			}
			return tips[string.Concat(Game1.stats.DaysPlayed % 224u)];
		}

		protected int getRerunWeek()
		{
			int totalRerunWeeksAvailable = Math.Min((int)(Game1.stats.DaysPlayed - 3) / 7, 32);
			if (weekToRecipeMap == null)
			{
				weekToRecipeMap = new Dictionary<int, string>();
				Dictionary<string, string> cookingRecipeChannel = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");
				foreach (string key in cookingRecipeChannel.Keys)
				{
					weekToRecipeMap[Convert.ToInt32(key)] = cookingRecipeChannel[key].Split('/')[0];
				}
			}
			List<int> recipeWeeksNotKnownByAllFarmers = new List<int>();
			IEnumerable<Farmer> farmers = Game1.getAllFarmers();
			for (int i = 1; i <= totalRerunWeeksAvailable; i++)
			{
				foreach (Farmer item in farmers)
				{
					if (!item.cookingRecipes.ContainsKey(weekToRecipeMap[i]))
					{
						recipeWeeksNotKnownByAllFarmers.Add(i);
						break;
					}
				}
			}
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			if (recipeWeeksNotKnownByAllFarmers.Count == 0)
			{
				return Math.Max(1, 1 + r.Next(totalRerunWeeksAvailable));
			}
			return recipeWeeksNotKnownByAllFarmers[r.Next(recipeWeeksNotKnownByAllFarmers.Count)];
		}

		protected virtual string[] getWeeklyRecipe()
		{
			string[] text = new string[2];
			int whichWeek = (int)(Game1.stats.DaysPlayed % 224u / 7u);
			if (Game1.stats.DaysPlayed % 224u == 0)
			{
				whichWeek = 32;
			}
			Dictionary<string, string> cookingRecipeChannel = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");
			FarmerTeam team = Game1.player.team;
			if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed"))
			{
				if ((int)team.lastDayQueenOfSauceRerunUpdated != Game1.Date.TotalDays)
				{
					team.lastDayQueenOfSauceRerunUpdated.Set(Game1.Date.TotalDays);
					team.queenOfSauceRerunWeek.Set(getRerunWeek());
				}
				whichWeek = team.queenOfSauceRerunWeek.Value;
			}
			try
			{
				string recipeName2 = cookingRecipeChannel[string.Concat(whichWeek)].Split('/')[0];
				text[0] = cookingRecipeChannel[string.Concat(whichWeek)].Split('/')[1];
				if (CraftingRecipe.cookingRecipes.ContainsKey(recipeName2))
				{
					string[] split = CraftingRecipe.cookingRecipes[recipeName2].Split('/');
					text[1] = ((LocalizedContentManager.CurrentLanguageCode != 0) ? (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel[string.Concat(whichWeek)].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", split[split.Length - 1]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", split[split.Length - 1])) : (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel[string.Concat(whichWeek)].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", recipeName2) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", recipeName2)));
				}
				else
				{
					text[1] = ((LocalizedContentManager.CurrentLanguageCode != 0) ? (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel[string.Concat(whichWeek)].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", cookingRecipeChannel[string.Concat(whichWeek)].Split('/').Last()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", cookingRecipeChannel[string.Concat(whichWeek)].Split('/').Last())) : (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel[string.Concat(whichWeek)].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", recipeName2) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", recipeName2)));
				}
				if (Game1.player.cookingRecipes.ContainsKey(recipeName2))
				{
					return text;
				}
				Game1.player.cookingRecipes.Add(recipeName2, 0);
				return text;
			}
			catch (Exception)
			{
				string recipeName = cookingRecipeChannel["1"].Split('/')[0];
				text[0] = cookingRecipeChannel["1"].Split('/')[1];
				text[1] = ((LocalizedContentManager.CurrentLanguageCode != 0) ? (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel["1"].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", cookingRecipeChannel["1"].Split('/').Last()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", cookingRecipeChannel["1"].Split('/').Last())) : (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel["1"].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", recipeName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", recipeName)));
				if (Game1.player.cookingRecipes.ContainsKey(recipeName))
				{
					return text;
				}
				Game1.player.cookingRecipes.Add(recipeName, 0);
				return text;
			}
		}

		private string getIslandWeatherForecast()
		{
			int num = ++new WorldDate(Game1.Date).TotalDays;
			int forecast = Game1.netWorldState.Value.GetWeatherForLocation(Game1.getLocationFromName("IslandSouth").GetLocationContext()).weatherForTomorrow.Value;
			string response = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_IslandWeatherIntro");
			switch (forecast)
			{
			case 0:
				return response + ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13182") : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13183"));
			case 1:
				return response + Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13184");
			case 3:
				return response + Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13185");
			default:
				return response + "???";
			}
		}

		protected virtual string getWeatherForecast()
		{
			WorldDate tomorrow = new WorldDate(Game1.Date);
			int num = ++tomorrow.TotalDays;
			switch ((!Game1.IsMasterGame) ? Game1.getWeatherModificationsForDate(tomorrow, Game1.netWorldState.Value.WeatherForTomorrow) : Game1.getWeatherModificationsForDate(tomorrow, Game1.weatherForTomorrow))
			{
			case 4:
			{
				Dictionary<string, string> festivalData;
				try
				{
					festivalData = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + (Game1.dayOfMonth + 1));
				}
				catch (Exception)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13164");
				}
				string festName = festivalData["name"];
				string locationName = festivalData["conditions"].Split('/')[0];
				int startTime = Convert.ToInt32(festivalData["conditions"].Split('/')[1].Split(' ')[0]);
				int endTime = Convert.ToInt32(festivalData["conditions"].Split('/')[1].Split(' ')[1]);
				string locationFullName = "";
				switch (locationName)
				{
				case "Town":
					locationFullName = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13170");
					break;
				case "Beach":
					locationFullName = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13172");
					break;
				case "Forest":
					locationFullName = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13174");
					break;
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13175", festName, locationFullName, Game1.getTimeOfDayString(startTime), Game1.getTimeOfDayString(endTime));
			}
			case 5:
				if (!(Game1.random.NextDouble() < 0.5))
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13181");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13180");
			case 0:
			case 6:
				if (!(Game1.random.NextDouble() < 0.5))
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13183");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13182");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13184");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13185");
			case 2:
				if (!Game1.currentSeason.Equals("spring"))
				{
					if (!Game1.currentSeason.Equals("fall"))
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13190");
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13189");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13187");
			default:
				return "";
			}
		}

		public virtual void setFortuneOverlay(Farmer who)
		{
			if (who.DailyLuck < -0.07)
			{
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(592, 346, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(15f, 1f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
			}
			else if (who.DailyLuck < -0.02)
			{
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 346, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(15f, 1f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
			}
			else if (who.DailyLuck > 0.07)
			{
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(644, 333, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(15f, 1f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
			}
			else if (who.DailyLuck > 0.02)
			{
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(592, 333, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(15f, 1f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
			}
			else
			{
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 333, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(15f, 1f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
			}
		}

		public virtual string getFortuneForecast(Farmer who)
		{
			string fortune2 = "";
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			fortune2 = (((double)who.team.sharedDailyLuck == -0.12) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13191") : ((who.DailyLuck < -0.07) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13192") : ((who.DailyLuck < -0.02) ? ((r.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13193") : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13195")) : (((double)who.team.sharedDailyLuck == 0.12) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13197") : ((who.DailyLuck > 0.07) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13198") : ((!(who.DailyLuck > 0.02)) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13200") : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13199")))))));
			if (who.DailyLuck == 0.0)
			{
				fortune2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13201");
			}
			return fortune2;
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			base.draw(spriteBatch, x, y, alpha);
			if (screen != null)
			{
				screen.update(Game1.currentGameTime);
				screen.draw(spriteBatch);
				if (screenOverlay != null)
				{
					screenOverlay.update(Game1.currentGameTime);
					screenOverlay.draw(spriteBatch);
				}
			}
		}
	}
}
