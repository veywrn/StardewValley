using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class SocialPage : IClickableMenu
	{
		public const int slotsOnPage = 5;

		public static readonly string[] defaultFriendships = new string[2]
		{
			"Robin",
			"Lewis"
		};

		private string descriptionText = "";

		private string hoverText = "";

		private ClickableTextureComponent upButton;

		private ClickableTextureComponent downButton;

		private ClickableTextureComponent scrollBar;

		private Rectangle scrollBarRunner;

		public List<object> names;

		private List<ClickableTextureComponent> sprites;

		private int slotPosition;

		private int numFarmers;

		private List<string> kidsNames = new List<string>();

		private Dictionary<string, string> npcNames;

		public List<ClickableTextureComponent> characterSlots;

		private bool scrolling;

		public Friendship emptyFriendship = new Friendship();

		public SocialPage(int x, int y, int width, int height)
			: base(x, y, width, height)
		{
			string[] array = defaultFriendships;
			foreach (string name in array)
			{
				if (!Game1.player.friendshipData.ContainsKey(name))
				{
					Game1.player.friendshipData.Add(name, new Friendship());
				}
			}
			characterSlots = new List<ClickableTextureComponent>();
			Dictionary<string, string> NPCDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			npcNames = new Dictionary<string, string>();
			foreach (string name2 in Game1.player.friendshipData.Keys)
			{
				string displayName = name2;
				if (NPCDispositions.ContainsKey(name2) && NPCDispositions[name2].Split('/').Length > 11)
				{
					displayName = NPCDispositions[name2].Split('/')[11];
				}
				if (!npcNames.ContainsKey(name2))
				{
					npcNames.Add(name2, displayName);
				}
			}
			foreach (NPC l in Utility.getAllCharacters())
			{
				if (l.CanSocialize)
				{
					if (Game1.player.friendshipData.ContainsKey(l.Name) && l is Child)
					{
						kidsNames.Add(l.Name);
						npcNames[l.Name] = l.Name.Trim();
					}
					else if (Game1.player.friendshipData.ContainsKey(l.Name))
					{
						npcNames[l.Name] = l.displayName;
					}
					else
					{
						npcNames[l.Name] = "???";
					}
				}
			}
			names = new List<object>();
			sprites = new List<ClickableTextureComponent>();
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (((bool)farmer.isCustomized || farmer == Game1.MasterPlayer) && farmer != Game1.player)
				{
					names.Add(farmer.UniqueMultiplayerID);
					sprites.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.borderWidth + 4, 0, width, 64), null, "", farmer.Sprite.Texture, Rectangle.Empty, 4f));
					numFarmers++;
				}
			}
			foreach (KeyValuePair<string, string> namePair in npcNames.OrderBy((KeyValuePair<string, string> p) => -Game1.player.getFriendshipLevelForNPC(p.Key)))
			{
				NPC k = null;
				if (kidsNames.Contains(namePair.Key))
				{
					k = Game1.getCharacterFromName<Child>(namePair.Key, mustBeVillager: false);
				}
				else if (NPCDispositions.ContainsKey(namePair.Key))
				{
					string[] location = NPCDispositions[namePair.Key].Split('/')[10].Split(' ');
					string texture_name = NPC.getTextureNameForCharacter(namePair.Key);
					if (location.Length > 2)
					{
						k = new NPC(new AnimatedSprite("Characters\\" + texture_name, 0, 16, 32), new Vector2(Convert.ToInt32(location[1]) * 64, Convert.ToInt32(location[2]) * 64), location[0], 0, namePair.Key, null, Game1.content.Load<Texture2D>("Portraits\\" + texture_name), eventActor: false);
					}
				}
				if (k != null)
				{
					names.Add(k.Name);
					sprites.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.borderWidth + 4, 0, width, 64), null, "", k.Sprite.Texture, k.getMugShotSourceRect(), 4f));
				}
			}
			for (int j = 0; j < names.Count; j++)
			{
				ClickableTextureComponent slot_component = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth, 0, width - IClickableMenu.borderWidth * 2, rowPosition(1) - rowPosition(0)), null, new Rectangle(0, 0, 0, 0), 4f)
				{
					myID = j,
					downNeighborID = j + 1,
					upNeighborID = j - 1
				};
				if (slot_component.upNeighborID < 0)
				{
					slot_component.upNeighborID = 12342;
				}
				characterSlots.Add(slot_component);
			}
			upButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
			downButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
			scrollBar = new ClickableTextureComponent(new Rectangle(upButton.bounds.X + 12, upButton.bounds.Y + upButton.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
			scrollBarRunner = new Rectangle(scrollBar.bounds.X, upButton.bounds.Y + upButton.bounds.Height + 4, scrollBar.bounds.Width, height - 128 - upButton.bounds.Height - 8);
			int first_character_index = 0;
			for (int i = 0; i < names.Count; i++)
			{
				if (!(names[i] is long))
				{
					first_character_index = i;
					break;
				}
			}
			slotPosition = first_character_index;
			setScrollBarToCurrentIndex();
			updateSlots();
		}

		public static bool isRoommateOfAnyone(string name)
		{
			return Game1.getCharacterFromName(name)?.isRoommate() ?? false;
		}

		public static bool isDatable(string name)
		{
			Dictionary<string, string> NPCDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			if (!NPCDispositions.ContainsKey(name))
			{
				return false;
			}
			return NPCDispositions[name].Split('/')[5] == "datable";
		}

		public Friendship getFriendship(string name)
		{
			if (Game1.player.friendshipData.ContainsKey(name))
			{
				return Game1.player.friendshipData[name];
			}
			return emptyFriendship;
		}

		public override void snapToDefaultClickableComponent()
		{
			if (slotPosition < characterSlots.Count)
			{
				currentlySnappedComponent = characterSlots[slotPosition];
			}
			snapCursorToCurrentSnappedComponent();
		}

		public int getGender(string name)
		{
			if (kidsNames.Contains(name))
			{
				return Game1.getCharacterFromName<Child>(name, mustBeVillager: false).Gender;
			}
			Dictionary<string, string> NPCDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			if (!NPCDispositions.ContainsKey(name))
			{
				return 0;
			}
			if (!(NPCDispositions[name].Split('/')[4] == "female"))
			{
				return 0;
			}
			return 1;
		}

		public bool isMarriedToAnyone(string name)
		{
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.spouse == name && farmer.isMarried())
				{
					return true;
				}
			}
			return false;
		}

		public void updateSlots()
		{
			for (int j = 0; j < characterSlots.Count; j++)
			{
				characterSlots[j].bounds.Y = rowPosition(j - 1);
			}
			int index = 0;
			for (int i = slotPosition; i < slotPosition + 5; i++)
			{
				if (sprites.Count > i)
				{
					int y = yPositionOnScreen + IClickableMenu.borderWidth + 32 + 112 * index + 32;
					sprites[i].bounds.Y = y;
				}
				index++;
			}
			populateClickableComponentList();
			addTabsToClickableComponents();
		}

		public void addTabsToClickableComponents()
		{
			if (Game1.activeClickableMenu is GameMenu && !allClickableComponents.Contains((Game1.activeClickableMenu as GameMenu).tabs[0]))
			{
				allClickableComponents.AddRange((Game1.activeClickableMenu as GameMenu).tabs);
			}
		}

		protected void _SelectSlot(ClickableComponent slot_component)
		{
			if (slot_component != null && characterSlots.Contains(slot_component))
			{
				int index = characterSlots.IndexOf(slot_component as ClickableTextureComponent);
				currentlySnappedComponent = slot_component;
				if (index < slotPosition)
				{
					slotPosition = index;
				}
				else if (index >= slotPosition + 5)
				{
					slotPosition = index - 5 + 1;
				}
				setScrollBarToCurrentIndex();
				updateSlots();
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public void ConstrainSelectionToVisibleSlots()
		{
			if (characterSlots.Contains(currentlySnappedComponent))
			{
				int index = characterSlots.IndexOf(currentlySnappedComponent as ClickableTextureComponent);
				if (index < slotPosition)
				{
					index = slotPosition;
				}
				else if (index >= slotPosition + 5)
				{
					index = slotPosition + 5 - 1;
				}
				currentlySnappedComponent = characterSlots[index];
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public override void snapCursorToCurrentSnappedComponent()
		{
			if (currentlySnappedComponent != null && characterSlots.Contains(currentlySnappedComponent))
			{
				Game1.setMousePosition(currentlySnappedComponent.bounds.Left + 64, currentlySnappedComponent.bounds.Center.Y);
			}
			else
			{
				base.snapCursorToCurrentSnappedComponent();
			}
		}

		public override void applyMovementKey(int direction)
		{
			base.applyMovementKey(direction);
			if (characterSlots.Contains(currentlySnappedComponent))
			{
				_SelectSlot(currentlySnappedComponent);
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (scrolling)
			{
				int y2 = scrollBar.bounds.Y;
				scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upButton.bounds.Height + 20));
				float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
				slotPosition = Math.Min(sprites.Count - 5, Math.Max(0, (int)((float)sprites.Count * percentage)));
				setScrollBarToCurrentIndex();
				if (y2 != scrollBar.bounds.Y)
				{
					Game1.playSound("shiny4");
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			scrolling = false;
		}

		private void setScrollBarToCurrentIndex()
		{
			if (sprites.Count > 0)
			{
				scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, sprites.Count - 5 + 1) * slotPosition + upButton.bounds.Bottom + 4;
				if (slotPosition == sprites.Count - 5)
				{
					scrollBar.bounds.Y = downButton.bounds.Y - scrollBar.bounds.Height - 4;
				}
			}
			updateSlots();
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && slotPosition > 0)
			{
				upArrowPressed();
				ConstrainSelectionToVisibleSlots();
				Game1.playSound("shiny4");
			}
			else if (direction < 0 && slotPosition < Math.Max(0, sprites.Count - 5))
			{
				downArrowPressed();
				ConstrainSelectionToVisibleSlots();
				Game1.playSound("shiny4");
			}
		}

		public void upArrowPressed()
		{
			slotPosition--;
			updateSlots();
			upButton.scale = 3.5f;
			setScrollBarToCurrentIndex();
		}

		public void downArrowPressed()
		{
			slotPosition++;
			updateSlots();
			downButton.scale = 3.5f;
			setScrollBarToCurrentIndex();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (upButton.containsPoint(x, y) && slotPosition > 0)
			{
				upArrowPressed();
				Game1.playSound("shwip");
				return;
			}
			if (downButton.containsPoint(x, y) && slotPosition < sprites.Count - 5)
			{
				downArrowPressed();
				Game1.playSound("shwip");
				return;
			}
			if (scrollBar.containsPoint(x, y))
			{
				scrolling = true;
				return;
			}
			if (!downButton.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
			{
				scrolling = true;
				leftClickHeld(x, y);
				releaseLeftClick(x, y);
				return;
			}
			for (int i = 0; i < characterSlots.Count; i++)
			{
				if (i < slotPosition || i >= slotPosition + 5 || !characterSlots[i].bounds.Contains(x, y))
				{
					continue;
				}
				bool fail2 = true;
				if (names[i] is string)
				{
					Character character = Game1.getCharacterFromName((string)names[i]);
					if (character != null && Game1.player.friendshipData.ContainsKey(character.name))
					{
						fail2 = false;
						Game1.playSound("bigSelect");
						int cached_slot_position = slotPosition;
						ProfileMenu menu = new ProfileMenu(character);
						menu.exitFunction = delegate
						{
							SocialPage socialPage = ((GameMenu)(Game1.activeClickableMenu = new GameMenu(2, -1, playOpeningSound: false))).GetCurrentPage() as SocialPage;
							if (socialPage != null)
							{
								Character character2 = menu.GetCharacter();
								if (character2 != null)
								{
									int num = 0;
									while (true)
									{
										if (num >= socialPage.names.Count)
										{
											return;
										}
										if (socialPage.names[num] is string && character2.Name == (string)socialPage.names[num])
										{
											break;
										}
										num++;
									}
									socialPage.slotPosition = cached_slot_position;
									socialPage._SelectSlot(socialPage.characterSlots[num]);
								}
							}
						};
						Game1.activeClickableMenu = menu;
						if (Game1.options.SnappyMenus)
						{
							menu.snapToDefaultClickableComponent();
						}
						return;
					}
				}
				if (fail2)
				{
					Game1.playSound("shiny4");
				}
				break;
			}
			slotPosition = Math.Max(0, Math.Min(sprites.Count - 5, slotPosition));
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			descriptionText = "";
			hoverText = "";
			upButton.tryHover(x, y);
			downButton.tryHover(x, y);
		}

		private bool isCharacterSlotClickable(int i)
		{
			if (names[i] is string)
			{
				Character character = Game1.getCharacterFromName((string)names[i]);
				if (character != null && Game1.player.friendshipData.ContainsKey(character.name))
				{
					return true;
				}
			}
			return false;
		}

		private void drawNPCSlot(SpriteBatch b, int i)
		{
			if (isCharacterSlotClickable(i) && characterSlots[i].bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
			{
				b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth - 4, sprites[i].bounds.Y - 4, characterSlots[i].bounds.Width, characterSlots[i].bounds.Height - 12), Color.White * 0.25f);
			}
			sprites[i].draw(b);
			string name = names[i] as string;
			int heartLevel = Game1.player.getFriendshipHeartLevelForNPC(name);
			bool datable = isDatable(name);
			Friendship friendship = getFriendship(name);
			bool spouse = friendship.IsMarried();
			bool housemate = spouse && isRoommateOfAnyone(name);
			float lineHeight = Game1.smallFont.MeasureString("W").Y;
			float russianOffsetY = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? ((0f - lineHeight) / 2f) : 0f;
			b.DrawString(Game1.dialogueFont, npcNames[name], new Vector2((float)(xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 64 - 20 + 96) - Game1.dialogueFont.MeasureString(npcNames[name]).X / 2f, (float)(sprites[i].bounds.Y + 48) + russianOffsetY - (float)(datable ? 24 : 20)), Game1.textColor);
			for (int hearts = 0; hearts < Math.Max(Utility.GetMaximumHeartsForCharacter(Game1.getCharacterFromName(name)), 10); hearts++)
			{
				int xSource = (hearts < heartLevel) ? 211 : 218;
				if (datable && !friendship.IsDating() && !spouse && hearts >= 8)
				{
					xSource = 211;
				}
				if (hearts < 10)
				{
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 320 - 4 + hearts * 32, sprites[i].bounds.Y + 64 - 28), new Rectangle(xSource, 428, 7, 6), (datable && !friendship.IsDating() && !spouse && hearts >= 8) ? (Color.Black * 0.35f) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 320 - 4 + (hearts - 10) * 32, sprites[i].bounds.Y + 64), new Rectangle(xSource, 428, 7, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
			}
			if (datable | housemate)
			{
				string text2 = (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.pt) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((getGender(name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').First() : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last());
				if (housemate)
				{
					text2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Housemate");
				}
				else if (spouse)
				{
					text2 = ((getGender(name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11636") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11637"));
				}
				else if (isMarriedToAnyone(name))
				{
					text2 = ((getGender(name) == 0) ? Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
				}
				else if (!Game1.player.isMarried() && friendship.IsDating())
				{
					text2 = ((getGender(name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
				}
				else if (getFriendship(name).IsDivorced())
				{
					text2 = ((getGender(name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
				}
				int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
				text2 = Game1.parseText(text2, Game1.smallFont, width);
				Vector2 textSize = Game1.smallFont.MeasureString(text2);
				b.DrawString(Game1.smallFont, text2, new Vector2((float)(xPositionOnScreen + 192 + 8) - textSize.X / 2f, (float)sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
			}
			if (!getFriendship(name).IsMarried() && !kidsNames.Contains(name))
			{
				Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(xPositionOnScreen + 384 + 304, sprites[i].bounds.Y - 4), new Rectangle(166, 174, 14, 12), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f, 0, -1, 0.2f);
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 384 + 296, sprites[i].bounds.Y + 32 + 20), new Rectangle(227 + ((getFriendship(name).GiftsThisWeek >= 2) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 384 + 336, sprites[i].bounds.Y + 32 + 20), new Rectangle(227 + ((getFriendship(name).GiftsThisWeek >= 1) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(xPositionOnScreen + 384 + 424, sprites[i].bounds.Y), new Rectangle(180, 175, 13, 11), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f, 0, -1, 0.2f);
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 384 + 432, sprites[i].bounds.Y + 32 + 20), new Rectangle(227 + (getFriendship(name).TalkedToToday ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			}
			if (spouse)
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 460, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
			}
			else if (friendship.IsDating())
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
			}
		}

		private int rowPosition(int i)
		{
			int j = i - slotPosition;
			int rowHeight = 112;
			return yPositionOnScreen + IClickableMenu.borderWidth + 160 + 4 + j * rowHeight;
		}

		private void drawFarmerSlot(SpriteBatch b, int i)
		{
			long farmerID = (long)names[i];
			Farmer farmer = Game1.getFarmerMaybeOffline(farmerID);
			if (farmer != null)
			{
				int gender = (!farmer.IsMale) ? 1 : 0;
				ClickableTextureComponent clickableTextureComponent = sprites[i];
				int x = clickableTextureComponent.bounds.X;
				int y = clickableTextureComponent.bounds.Y;
				Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
				Rectangle newClip = origClip;
				newClip.Height = Math.Min(newClip.Bottom, rowPosition(i)) - newClip.Y - 4;
				b.GraphicsDevice.ScissorRectangle = newClip;
				FarmerRenderer.isDrawingForUI = true;
				try
				{
					farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(farmer.bathingClothes ? 108 : 0, 0, secondaryArm: false, flip: false), farmer.bathingClothes ? 108 : 0, new Rectangle(0, farmer.bathingClothes ? 576 : 0, 16, 32), new Vector2(x, y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, farmer);
				}
				finally
				{
					b.GraphicsDevice.ScissorRectangle = origClip;
				}
				FarmerRenderer.isDrawingForUI = false;
				Friendship friendship = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, farmerID);
				bool spouse = friendship.IsMarried();
				float lineHeight = Game1.smallFont.MeasureString("W").Y;
				float russianOffsetY = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? ((0f - lineHeight) / 2f) : 0f;
				b.DrawString(Game1.dialogueFont, farmer.Name, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 96 - 20, (float)(sprites[i].bounds.Y + 48) + russianOffsetY - 24f), Game1.textColor);
				string text2 = (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.pt) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').First() : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last());
				if (spouse)
				{
					text2 = ((gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11636") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11637"));
				}
				else if (farmer.isMarried() && !farmer.hasRoommate())
				{
					text2 = ((gender == 0) ? Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
				}
				else if (!Game1.player.isMarried() && friendship.IsDating())
				{
					text2 = ((gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
				}
				else if (friendship.IsDivorced())
				{
					text2 = ((gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
				}
				int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
				text2 = Game1.parseText(text2, Game1.smallFont, width);
				Vector2 textSize = Game1.smallFont.MeasureString(text2);
				b.DrawString(Game1.smallFont, text2, new Vector2((float)(xPositionOnScreen + 192 + 8) - textSize.X / 2f, (float)sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
				if (spouse)
				{
					b.Draw(Game1.objectSpriteSheet, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 801, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
				}
				else if (friendship.IsDating())
				{
					b.Draw(Game1.objectSpriteSheet, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.End();
			b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
			drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 128 + 4, small: true);
			drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 192 + 32 + 20, small: true);
			drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 320 + 36, small: true);
			drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 384 + 32 + 52, small: true);
			for (int i = slotPosition; i < slotPosition + 5; i++)
			{
				if (i < sprites.Count)
				{
					if (names[i] is string)
					{
						drawNPCSlot(b, i);
					}
					else if (names[i] is long)
					{
						drawFarmerSlot(b, i);
					}
				}
			}
			Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
			Rectangle newClip = origClip;
			newClip.Y = Math.Max(0, rowPosition(numFarmers - 1));
			newClip.Height -= newClip.Y;
			if (newClip.Height > 0)
			{
				b.GraphicsDevice.ScissorRectangle = newClip;
				try
				{
					drawVerticalPartition(b, xPositionOnScreen + 256 + 12, small: true);
					drawVerticalPartition(b, xPositionOnScreen + 384 + 368, small: true);
					drawVerticalPartition(b, xPositionOnScreen + 256 + 12 + 352, small: true);
				}
				finally
				{
					b.GraphicsDevice.ScissorRectangle = origClip;
				}
			}
			upButton.draw(b);
			downButton.draw(b);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
			scrollBar.draw(b);
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		}
	}
}
