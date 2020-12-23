using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class LevelUpMenu : IClickableMenu
	{
		public const int region_okButton = 101;

		public const int region_leftProfession = 102;

		public const int region_rightProfession = 103;

		public const int basewidth = 768;

		public const int baseheight = 512;

		public bool informationUp;

		public bool isActive;

		public bool isProfessionChooser;

		public bool hasUpdatedProfessions;

		private int currentLevel;

		private int currentSkill;

		private int timerBeforeStart;

		private float scale;

		private Color leftProfessionColor = Game1.textColor;

		private Color rightProfessionColor = Game1.textColor;

		private MouseState oldMouseState;

		public ClickableTextureComponent starIcon;

		public ClickableTextureComponent okButton;

		public ClickableComponent leftProfession;

		public ClickableComponent rightProfession;

		private List<CraftingRecipe> newCraftingRecipes = new List<CraftingRecipe>();

		private List<string> extraInfoForLevel = new List<string>();

		private List<string> leftProfessionDescription = new List<string>();

		private List<string> rightProfessionDescription = new List<string>();

		private Rectangle sourceRectForLevelIcon;

		private string title;

		private List<int> professionsToChoose = new List<int>();

		private List<TemporaryAnimatedSprite> littleStars = new List<TemporaryAnimatedSprite>();

		public bool hasMovedSelection;

		public LevelUpMenu()
			: base(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512)
		{
			Game1.player.team.endOfNightStatus.UpdateState("level");
			width = 768;
			height = 512;
			okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 101
			};
		}

		public LevelUpMenu(int skill, int level)
			: base(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512)
		{
			Game1.player.team.endOfNightStatus.UpdateState("level");
			timerBeforeStart = 250;
			isActive = true;
			width = 960;
			height = 512;
			okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 101
			};
			newCraftingRecipes.Clear();
			extraInfoForLevel.Clear();
			Game1.player.completelyStopAnimatingOrDoingAction();
			informationUp = true;
			isProfessionChooser = false;
			currentLevel = level;
			currentSkill = skill;
			if (level == 10)
			{
				Game1.getSteamAchievement("Achievement_SingularTalent");
				if ((int)Game1.player.farmingLevel == 10 && (int)Game1.player.miningLevel == 10 && (int)Game1.player.fishingLevel == 10 && (int)Game1.player.foragingLevel == 10 && (int)Game1.player.combatLevel == 10)
				{
					Game1.getSteamAchievement("Achievement_MasterOfTheFiveWays");
				}
				if (skill == 0)
				{
					Game1.addMailForTomorrow("marnieAutoGrabber");
				}
			}
			title = Game1.content.LoadString("Strings\\UI:LevelUp_Title", currentLevel, Farmer.getSkillDisplayNameFromIndex(currentSkill));
			extraInfoForLevel = getExtraInfoForLevel(currentSkill, currentLevel);
			switch (currentSkill)
			{
			case 0:
				sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
				break;
			case 1:
				sourceRectForLevelIcon = new Rectangle(16, 0, 16, 16);
				break;
			case 3:
				sourceRectForLevelIcon = new Rectangle(32, 0, 16, 16);
				break;
			case 2:
				sourceRectForLevelIcon = new Rectangle(80, 0, 16, 16);
				break;
			case 4:
				sourceRectForLevelIcon = new Rectangle(128, 16, 16, 16);
				break;
			case 5:
				sourceRectForLevelIcon = new Rectangle(64, 0, 16, 16);
				break;
			}
			if ((currentLevel == 5 || currentLevel == 10) && currentSkill != 5)
			{
				professionsToChoose.Clear();
				isProfessionChooser = true;
			}
			int newHeight = 0;
			foreach (KeyValuePair<string, string> v2 in CraftingRecipe.craftingRecipes)
			{
				string conditions2 = v2.Value.Split('/')[4];
				if (conditions2.Contains(Farmer.getSkillNameFromIndex(currentSkill)) && conditions2.Contains(string.Concat(currentLevel)))
				{
					newCraftingRecipes.Add(new CraftingRecipe(v2.Key, isCookingRecipe: false));
					if (!Game1.player.craftingRecipes.ContainsKey(v2.Key))
					{
						Game1.player.craftingRecipes.Add(v2.Key, 0);
					}
					newHeight += (newCraftingRecipes.Last().bigCraftable ? 128 : 64);
				}
			}
			foreach (KeyValuePair<string, string> v in CraftingRecipe.cookingRecipes)
			{
				string conditions = v.Value.Split('/')[3];
				if (conditions.Contains(Farmer.getSkillNameFromIndex(currentSkill)) && conditions.Contains(string.Concat(currentLevel)))
				{
					newCraftingRecipes.Add(new CraftingRecipe(v.Key, isCookingRecipe: true));
					if (!Game1.player.cookingRecipes.ContainsKey(v.Key))
					{
						Game1.player.cookingRecipes.Add(v.Key, 0);
						if (!Game1.player.hasOrWillReceiveMail("robinKitchenLetter"))
						{
							Game1.mailbox.Add("robinKitchenLetter");
						}
					}
					newHeight += (newCraftingRecipes.Last().bigCraftable ? 128 : 64);
				}
			}
			height = newHeight + 256 + extraInfoForLevel.Count * 64 * 3 / 4;
			Game1.player.freezePause = 100;
			gameWindowSizeChanged(Rectangle.Empty, Rectangle.Empty);
			if (isProfessionChooser)
			{
				leftProfession = new ClickableComponent(new Rectangle(xPositionOnScreen, yPositionOnScreen + 128, width / 2, height), "")
				{
					myID = 102,
					rightNeighborID = 103
				};
				rightProfession = new ClickableComponent(new Rectangle(width / 2 + xPositionOnScreen, yPositionOnScreen + 128, width / 2, height), "")
				{
					myID = 103,
					leftNeighborID = 102
				};
			}
			populateClickableComponentList();
		}

		public bool CanReceiveInput()
		{
			if (!informationUp)
			{
				return false;
			}
			if (timerBeforeStart > 0)
			{
				return false;
			}
			return true;
		}

		public override void snapToDefaultClickableComponent()
		{
			if (isProfessionChooser)
			{
				currentlySnappedComponent = getComponentWithID(103);
				Game1.setMousePosition(xPositionOnScreen + width + 64, yPositionOnScreen + height + 64);
			}
			else
			{
				currentlySnappedComponent = getComponentWithID(101);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void applyMovementKey(int direction)
		{
			if (CanReceiveInput())
			{
				if (direction == 3 || direction == 1)
				{
					hasMovedSelection = true;
				}
				base.applyMovementKey(direction);
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2;
			okButton.bounds = new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 64, 64);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public List<string> getExtraInfoForLevel(int whichSkill, int whichLevel)
		{
			List<string> extraInfo = new List<string>();
			switch (whichSkill)
			{
			case 0:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Farming1"));
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Farming2"));
				break;
			case 3:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Mining"));
				break;
			case 1:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Fishing"));
				break;
			case 2:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging1"));
				if (whichLevel == 1)
				{
					extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging2"));
				}
				if (whichLevel == 4 || whichLevel == 8)
				{
					extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging3"));
				}
				break;
			case 4:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Combat"));
				break;
			case 5:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Luck"));
				break;
			}
			return extraInfo;
		}

		private static void addProfessionDescriptions(List<string> descriptions, string professionName)
		{
			descriptions.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + professionName));
			descriptions.AddRange(Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionDescription_" + professionName).Split('\n'));
		}

		private static string getProfessionName(int whichProfession)
		{
			switch (whichProfession)
			{
			case 0:
				return "Rancher";
			case 1:
				return "Tiller";
			case 2:
				return "Coopmaster";
			case 3:
				return "Shepherd";
			case 4:
				return "Artisan";
			case 5:
				return "Agriculturist";
			case 6:
				return "Fisher";
			case 7:
				return "Trapper";
			case 8:
				return "Angler";
			case 9:
				return "Pirate";
			case 10:
				return "Mariner";
			case 11:
				return "Luremaster";
			case 12:
				return "Forester";
			case 13:
				return "Gatherer";
			case 14:
				return "Lumberjack";
			case 15:
				return "Tapper";
			case 16:
				return "Botanist";
			case 17:
				return "Tracker";
			case 18:
				return "Miner";
			case 19:
				return "Geologist";
			case 20:
				return "Blacksmith";
			case 21:
				return "Prospector";
			case 22:
				return "Excavator";
			case 23:
				return "Gemologist";
			case 24:
				return "Fighter";
			case 25:
				return "Scout";
			case 26:
				return "Brute";
			case 27:
				return "Defender";
			case 28:
				return "Acrobat";
			default:
				return "Desperado";
			}
		}

		public static List<string> getProfessionDescription(int whichProfession)
		{
			List<string> list = new List<string>();
			addProfessionDescriptions(list, getProfessionName(whichProfession));
			return list;
		}

		public static string getProfessionTitleFromNumber(int whichProfession)
		{
			return Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + getProfessionName(whichProfession));
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if ((b == Buttons.Start || b == Buttons.B) && !isProfessionChooser && isActive)
			{
				okButtonClicked();
			}
		}

		public static void AddMissedProfessionChoices(Farmer farmer)
		{
			int[] skills = new int[5]
			{
				0,
				1,
				2,
				3,
				4
			};
			foreach (int skill in skills)
			{
				if (farmer.GetUnmodifiedSkillLevel(skill) >= 5 && !farmer.newLevels.Contains(new Point(skill, 5)) && farmer.getProfessionForSkill(skill, 5) == -1)
				{
					farmer.newLevels.Add(new Point(skill, 5));
				}
				if (farmer.GetUnmodifiedSkillLevel(skill) >= 10 && !farmer.newLevels.Contains(new Point(skill, 10)) && farmer.getProfessionForSkill(skill, 10) == -1)
				{
					farmer.newLevels.Add(new Point(skill, 10));
				}
			}
		}

		public static void AddMissedLevelRecipes(Farmer farmer)
		{
			int[] skills = new int[5]
			{
				0,
				1,
				2,
				3,
				4
			};
			foreach (int skill in skills)
			{
				for (int level = 0; level <= farmer.GetUnmodifiedSkillLevel(skill); level++)
				{
					if (!farmer.newLevels.Contains(new Point(skill, level)))
					{
						foreach (KeyValuePair<string, string> v2 in CraftingRecipe.craftingRecipes)
						{
							string conditions2 = v2.Value.Split('/')[4];
							if (conditions2.Contains(Farmer.getSkillNameFromIndex(skill)) && conditions2.Contains(string.Concat(level)) && !farmer.craftingRecipes.ContainsKey(v2.Key))
							{
								Console.WriteLine(farmer.Name + " was missing recipe " + v2.Key + " from skill level up.");
								farmer.craftingRecipes.Add(v2.Key, 0);
							}
						}
						foreach (KeyValuePair<string, string> v in CraftingRecipe.cookingRecipes)
						{
							string conditions = v.Value.Split('/')[3];
							if (conditions.Contains(Farmer.getSkillNameFromIndex(skill)) && conditions.Contains(string.Concat(level)) && !farmer.cookingRecipes.ContainsKey(v.Key))
							{
								Console.WriteLine(farmer.Name + " was missing recipe " + v.Key + " from skill level up.");
								farmer.cookingRecipes.Add(v.Key, 0);
							}
						}
					}
				}
			}
		}

		public static void removeImmediateProfessionPerk(int whichProfession)
		{
			switch (whichProfession)
			{
			case 24:
				Game1.player.maxHealth -= 15;
				break;
			case 27:
				Game1.player.maxHealth -= 25;
				break;
			}
			if (Game1.player.health > Game1.player.maxHealth)
			{
				Game1.player.health = Game1.player.maxHealth;
			}
		}

		public void getImmediateProfessionPerk(int whichProfession)
		{
			switch (whichProfession)
			{
			case 24:
				Game1.player.maxHealth += 15;
				break;
			case 27:
				Game1.player.maxHealth += 25;
				break;
			}
			Game1.player.health = Game1.player.maxHealth;
			Game1.player.Stamina = (int)Game1.player.maxStamina;
		}

		public static void RevalidateHealth(Farmer farmer)
		{
			_ = farmer.maxHealth;
			int expected_max_health = 100;
			if (farmer.mailReceived.Contains("qiCave"))
			{
				expected_max_health += 25;
			}
			for (int i = 1; i <= farmer.GetUnmodifiedSkillLevel(4); i++)
			{
				if (!farmer.newLevels.Contains(new Point(4, i)) && i != 5 && i != 10)
				{
					expected_max_health += 5;
				}
			}
			if (farmer.professions.Contains(24))
			{
				expected_max_health += 15;
			}
			if (farmer.professions.Contains(27))
			{
				expected_max_health += 25;
			}
			if (farmer.maxHealth < expected_max_health)
			{
				Console.WriteLine("Fixing max health of: " + farmer.Name + " was " + farmer.maxHealth + " (expected: " + expected_max_health + ")");
				int difference = expected_max_health - farmer.maxHealth;
				farmer.maxHealth = expected_max_health;
				farmer.health += difference;
			}
		}

		public override void update(GameTime time)
		{
			if (!isActive)
			{
				exitThisMenu();
				return;
			}
			if (isProfessionChooser && !hasUpdatedProfessions)
			{
				if (currentLevel == 5)
				{
					professionsToChoose.Add(currentSkill * 6);
					professionsToChoose.Add(currentSkill * 6 + 1);
				}
				else if (Game1.player.professions.Contains(currentSkill * 6))
				{
					professionsToChoose.Add(currentSkill * 6 + 2);
					professionsToChoose.Add(currentSkill * 6 + 3);
				}
				else
				{
					professionsToChoose.Add(currentSkill * 6 + 4);
					professionsToChoose.Add(currentSkill * 6 + 5);
				}
				leftProfessionDescription = getProfessionDescription(professionsToChoose[0]);
				rightProfessionDescription = getProfessionDescription(professionsToChoose[1]);
				hasUpdatedProfessions = true;
			}
			for (int i = littleStars.Count - 1; i >= 0; i--)
			{
				if (littleStars[i].update(time))
				{
					littleStars.RemoveAt(i);
				}
			}
			if (Game1.random.NextDouble() < 0.03)
			{
				Vector2 position = new Vector2(0f, Game1.random.Next(yPositionOnScreen - 128, yPositionOnScreen - 4) / 20 * 4 * 5 + 32);
				if (Game1.random.NextDouble() < 0.5)
				{
					position.X = Game1.random.Next(xPositionOnScreen + width / 2 - 228, xPositionOnScreen + width / 2 - 132);
				}
				else
				{
					position.X = Game1.random.Next(xPositionOnScreen + width / 2 + 116, xPositionOnScreen + width - 160);
				}
				if (position.Y < (float)(yPositionOnScreen - 64 - 8))
				{
					position.X = Game1.random.Next(xPositionOnScreen + width / 2 - 116, xPositionOnScreen + width / 2 + 116);
				}
				position.X = position.X / 20f * 4f * 5f;
				littleStars.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(364, 79, 5, 5), 80f, 7, 1, position, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					local = true
				});
			}
			if (timerBeforeStart > 0)
			{
				timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
				if (timerBeforeStart <= 0 && Game1.options.SnappyMenus)
				{
					populateClickableComponentList();
					snapToDefaultClickableComponent();
				}
				return;
			}
			if (isActive && isProfessionChooser)
			{
				leftProfessionColor = Game1.textColor;
				rightProfessionColor = Game1.textColor;
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.player.freezePause = 100;
				if (Game1.getMouseY() > yPositionOnScreen + 192 && Game1.getMouseY() < yPositionOnScreen + height)
				{
					if (Game1.getMouseX() > xPositionOnScreen && Game1.getMouseX() < xPositionOnScreen + width / 2)
					{
						leftProfessionColor = Color.Green;
						if (((Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released) || (Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))) && readyToClose())
						{
							Game1.player.professions.Add(professionsToChoose[0]);
							getImmediateProfessionPerk(professionsToChoose[0]);
							isActive = false;
							informationUp = false;
							isProfessionChooser = false;
							RemoveLevelFromLevelList();
						}
					}
					else if (Game1.getMouseX() > xPositionOnScreen + width / 2 && Game1.getMouseX() < xPositionOnScreen + width)
					{
						rightProfessionColor = Color.Green;
						if (((Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released) || (Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))) && readyToClose())
						{
							Game1.player.professions.Add(professionsToChoose[1]);
							getImmediateProfessionPerk(professionsToChoose[1]);
							isActive = false;
							informationUp = false;
							isProfessionChooser = false;
							RemoveLevelFromLevelList();
						}
					}
				}
				height = 512;
			}
			oldMouseState = Game1.input.GetMouseState();
			if (isActive && !informationUp && starIcon != null)
			{
				if (starIcon.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
				{
					starIcon.sourceRect.X = 294;
				}
				else
				{
					starIcon.sourceRect.X = 310;
				}
			}
			if (isActive && starIcon != null && !informationUp && (oldMouseState.LeftButton == ButtonState.Pressed || (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) && starIcon.containsPoint(oldMouseState.X, oldMouseState.Y))
			{
				newCraftingRecipes.Clear();
				extraInfoForLevel.Clear();
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.playSound("bigSelect");
				informationUp = true;
				isProfessionChooser = false;
				currentLevel = Game1.player.newLevels.First().Y;
				currentSkill = Game1.player.newLevels.First().X;
				title = Game1.content.LoadString("Strings\\UI:LevelUp_Title", currentLevel, Farmer.getSkillDisplayNameFromIndex(currentSkill));
				extraInfoForLevel = getExtraInfoForLevel(currentSkill, currentLevel);
				switch (currentSkill)
				{
				case 0:
					sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
					break;
				case 1:
					sourceRectForLevelIcon = new Rectangle(16, 0, 16, 16);
					break;
				case 3:
					sourceRectForLevelIcon = new Rectangle(32, 0, 16, 16);
					break;
				case 2:
					sourceRectForLevelIcon = new Rectangle(80, 0, 16, 16);
					break;
				case 4:
					sourceRectForLevelIcon = new Rectangle(128, 16, 16, 16);
					break;
				case 5:
					sourceRectForLevelIcon = new Rectangle(64, 0, 16, 16);
					break;
				}
				if ((currentLevel == 5 || currentLevel == 10) && currentSkill != 5)
				{
					professionsToChoose.Clear();
					isProfessionChooser = true;
					if (currentLevel == 5)
					{
						professionsToChoose.Add(currentSkill * 6);
						professionsToChoose.Add(currentSkill * 6 + 1);
					}
					else if (Game1.player.professions.Contains(currentSkill * 6))
					{
						professionsToChoose.Add(currentSkill * 6 + 2);
						professionsToChoose.Add(currentSkill * 6 + 3);
					}
					else
					{
						professionsToChoose.Add(currentSkill * 6 + 4);
						professionsToChoose.Add(currentSkill * 6 + 5);
					}
					leftProfessionDescription = getProfessionDescription(professionsToChoose[0]);
					rightProfessionDescription = getProfessionDescription(professionsToChoose[1]);
				}
				int newHeight = 0;
				foreach (KeyValuePair<string, string> v2 in CraftingRecipe.craftingRecipes)
				{
					string conditions2 = v2.Value.Split('/')[4];
					if (conditions2.Contains(Farmer.getSkillNameFromIndex(currentSkill)) && conditions2.Contains(string.Concat(currentLevel)))
					{
						newCraftingRecipes.Add(new CraftingRecipe(v2.Key, isCookingRecipe: false));
						if (!Game1.player.craftingRecipes.ContainsKey(v2.Key))
						{
							Game1.player.craftingRecipes.Add(v2.Key, 0);
						}
						newHeight += (newCraftingRecipes.Last().bigCraftable ? 128 : 64);
					}
				}
				foreach (KeyValuePair<string, string> v in CraftingRecipe.cookingRecipes)
				{
					string conditions = v.Value.Split('/')[3];
					if (conditions.Contains(Farmer.getSkillNameFromIndex(currentSkill)) && conditions.Contains(string.Concat(currentLevel)))
					{
						newCraftingRecipes.Add(new CraftingRecipe(v.Key, isCookingRecipe: true));
						if (!Game1.player.cookingRecipes.ContainsKey(v.Key))
						{
							Game1.player.cookingRecipes.Add(v.Key, 0);
						}
						newHeight += (newCraftingRecipes.Last().bigCraftable ? 128 : 64);
					}
				}
				height = newHeight + 256 + extraInfoForLevel.Count * 64 * 3 / 4;
				Game1.player.freezePause = 100;
			}
			if (!isActive || !informationUp)
			{
				return;
			}
			Game1.player.completelyStopAnimatingOrDoingAction();
			if (okButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !isProfessionChooser)
			{
				okButton.scale = Math.Min(1.1f, okButton.scale + 0.05f);
				if ((oldMouseState.LeftButton == ButtonState.Pressed || (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) && readyToClose())
				{
					okButtonClicked();
				}
			}
			else
			{
				okButton.scale = Math.Max(1f, okButton.scale - 0.05f);
			}
			Game1.player.freezePause = 100;
		}

		public void okButtonClicked()
		{
			getLevelPerk(currentSkill, currentLevel);
			RemoveLevelFromLevelList();
			isActive = false;
			informationUp = false;
		}

		public virtual void RemoveLevelFromLevelList()
		{
			for (int i = 0; i < Game1.player.newLevels.Count; i++)
			{
				Point level = Game1.player.newLevels[i];
				if (level.X == currentSkill && level.Y == currentLevel)
				{
					Game1.player.newLevels.RemoveAt(i);
					i--;
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.SnappyMenus && ((!Game1.options.doesInputListContain(Game1.options.cancelButton, key) && !Game1.options.doesInputListContain(Game1.options.menuButton, key)) || !isProfessionChooser))
			{
				base.receiveKeyPress(key);
			}
		}

		public void getLevelPerk(int skill, int level)
		{
			switch (skill)
			{
			case 4:
				Game1.player.maxHealth += 5;
				break;
			case 1:
				switch (level)
				{
				case 2:
					if (!Game1.player.hasOrWillReceiveMail("fishing2"))
					{
						Game1.addMailForTomorrow("fishing2");
					}
					break;
				case 6:
					if (!Game1.player.hasOrWillReceiveMail("fishing6"))
					{
						Game1.addMailForTomorrow("fishing6");
					}
					break;
				}
				break;
			}
			Game1.player.health = Game1.player.maxHealth;
			Game1.player.Stamina = (int)Game1.player.maxStamina;
		}

		public override void draw(SpriteBatch b)
		{
			if (timerBeforeStart > 0)
			{
				return;
			}
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			foreach (TemporaryAnimatedSprite littleStar in littleStars)
			{
				littleStar.draw(b);
			}
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + width / 2 - 116, yPositionOnScreen - 32 + 12), new Rectangle(363, 87, 58, 22), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			if (!informationUp && isActive && starIcon != null)
			{
				starIcon.draw(b);
			}
			else
			{
				if (!informationUp)
				{
					return;
				}
				if (isProfessionChooser)
				{
					if (professionsToChoose.Count() == 0)
					{
						return;
					}
					Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
					drawHorizontalPartition(b, yPositionOnScreen + 192);
					drawVerticalIntersectingPartition(b, xPositionOnScreen + width / 2 - 32, yPositionOnScreen + 192);
					Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f);
					b.DrawString(Game1.dialogueFont, title, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.dialogueFont.MeasureString(title).X / 2f, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Game1.textColor);
					Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f);
					string chooseProfession = Game1.content.LoadString("Strings\\UI:LevelUp_ChooseProfession");
					b.DrawString(Game1.smallFont, chooseProfession, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.smallFont.MeasureString(chooseProfession).X / 2f, yPositionOnScreen + 64 + IClickableMenu.spaceToClearTopBorder), Game1.textColor);
					b.DrawString(Game1.dialogueFont, leftProfessionDescription[0], new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160), leftProfessionColor);
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + width / 2 - 112, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16), new Rectangle(professionsToChoose[0] % 6 * 16, 624 + professionsToChoose[0] / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					for (int j = 1; j < leftProfessionDescription.Count; j++)
					{
						b.DrawString(Game1.smallFont, Game1.parseText(leftProfessionDescription[j], Game1.smallFont, width / 2 - 64), new Vector2(-4 + xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * (j + 1)), leftProfessionColor);
					}
					b.DrawString(Game1.dialogueFont, rightProfessionDescription[0], new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + width / 2, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160), rightProfessionColor);
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + width - 128, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16), new Rectangle(professionsToChoose[1] % 6 * 16, 624 + professionsToChoose[1] / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					for (int i = 1; i < rightProfessionDescription.Count; i++)
					{
						b.DrawString(Game1.smallFont, Game1.parseText(rightProfessionDescription[i], Game1.smallFont, width / 2 - 48), new Vector2(-4 + xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + width / 2, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * (i + 1)), rightProfessionColor);
					}
				}
				else
				{
					Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
					Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f);
					b.DrawString(Game1.dialogueFont, title, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.dialogueFont.MeasureString(title).X / 2f, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Game1.textColor);
					Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f);
					int y = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 80;
					foreach (string s2 in extraInfoForLevel)
					{
						b.DrawString(Game1.smallFont, s2, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.smallFont.MeasureString(s2).X / 2f, y), Game1.textColor);
						y += 48;
					}
					foreach (CraftingRecipe s in newCraftingRecipes)
					{
						string cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_" + (s.isCookingRecipe ? "cooking" : "crafting"));
						string message = Game1.content.LoadString("Strings\\UI:LevelUp_NewRecipe", cookingOrCrafting, s.DisplayName);
						b.DrawString(Game1.smallFont, message, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.smallFont.MeasureString(message).X / 2f - 64f, y + (s.bigCraftable ? 38 : 12)), Game1.textColor);
						s.drawMenuView(b, (int)((float)(xPositionOnScreen + width / 2) + Game1.smallFont.MeasureString(message).X / 2f - 48f), y - 16);
						y += (s.bigCraftable ? 128 : 64) + 8;
					}
					okButton.draw(b);
				}
				if (!Game1.options.SnappyMenus || !isProfessionChooser || hasMovedSelection)
				{
					Game1.mouseCursorTransparency = 1f;
					drawMouse(b);
				}
			}
		}
	}
}
