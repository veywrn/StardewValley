using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class ChooseFromListMenu : IClickableMenu
	{
		public delegate void actionOnChoosingListOption(string s);

		public const int region_backButton = 101;

		public const int region_forwardButton = 102;

		public const int region_okButton = 103;

		public const int region_cancelButton = 104;

		public const int w = 640;

		public const int h = 192;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent cancelButton;

		private List<string> options = new List<string>();

		private int index;

		private actionOnChoosingListOption chooseAction;

		private bool isJukebox;

		public ChooseFromListMenu(List<string> options, actionOnChoosingListOption chooseAction, bool isJukebox = false, string default_selection = null)
			: base(Game1.uiViewport.Width / 2 - 320, Game1.uiViewport.Height - 64 - 192, 640, 192)
		{
			this.chooseAction = chooseAction;
			backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen - 128 - 4, yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 101,
				rightNeighborID = 102
			};
			forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 640 + 16 + 64, yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 102,
				leftNeighborID = 101,
				rightNeighborID = 103
			};
			okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width + 128 + 8, yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, new Rectangle(175, 379, 16, 15), 4f)
			{
				myID = 103,
				leftNeighborID = 102,
				rightNeighborID = 104
			};
			cancelButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width + 192 + 12, yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = 104,
				leftNeighborID = 103
			};
			Game1.playSound("bigSelect");
			this.isJukebox = isJukebox;
			if (isJukebox)
			{
				FilterJukeboxTracks(options);
			}
			this.options = options;
			if (default_selection != null)
			{
				int default_index = options.IndexOf(default_selection);
				if (default_index >= 0)
				{
					index = default_index;
				}
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public static void FilterJukeboxTracks(List<string> options)
		{
			for (int i = options.Count - 1; i >= 0; i--)
			{
				if (!IsValidJukeboxSong(options[i]))
				{
					options.RemoveAt(i);
				}
				else
				{
					switch (options[i])
					{
					case "ocean":
						options.RemoveAt(i);
						break;
					case "communityCenter":
						options.RemoveAt(i);
						break;
					case "nightTime":
						options.RemoveAt(i);
						break;
					case "title_day":
						options.RemoveAt(i);
						options.Add("MainTheme");
						break;
					case "coin":
						options.RemoveAt(i);
						break;
					case "buglevelloop":
						options.RemoveAt(i);
						break;
					case "jojaOfficeSoundscape":
						options.RemoveAt(i);
						break;
					}
				}
			}
		}

		public static bool IsValidJukeboxSong(string name)
		{
			name = name.ToLower();
			if (name.Trim() == "")
			{
				return false;
			}
			if (name.Contains("ambient") || name.Contains("bigdrums") || name.Contains("clubloop") || name.Contains("ambience"))
			{
				return false;
			}
			return true;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(103);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			xPositionOnScreen = Game1.uiViewport.Width / 2 - 320;
			yPositionOnScreen = Game1.uiViewport.Height - 64 - 192;
			backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen - 128 - 4, yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
			forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 640 + 16 + 64, yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
			okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width + 128 + 8, yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, new Rectangle(175, 379, 16, 15), 4f);
			cancelButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width + 192 + 12, yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f);
		}

		public static void playSongAction(string s)
		{
			Game1.startedJukeboxMusic = true;
			Game1.changeMusicTrack(s);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			okButton.tryHover(x, y);
			cancelButton.tryHover(x, y);
			backButton.tryHover(x, y);
			forwardButton.tryHover(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (okButton.containsPoint(x, y) && chooseAction != null)
			{
				chooseAction(options[index]);
				Game1.playSound("select");
			}
			if (cancelButton.containsPoint(x, y))
			{
				exitThisMenu();
			}
			if (backButton.containsPoint(x, y))
			{
				index--;
				if (index < 0)
				{
					index = options.Count - 1;
				}
				backButton.scale = backButton.baseScale - 1f;
				Game1.playSound("shwip");
			}
			if (forwardButton.containsPoint(x, y))
			{
				index++;
				index %= options.Count;
				Game1.playSound("shwip");
				forwardButton.scale = forwardButton.baseScale - 1f;
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			string maxWidthJukeboxString = "Summer (The Sun Can Bend An Orange Sky)";
			int stringWidth = (int)Game1.dialogueFont.MeasureString(isJukebox ? maxWidthJukeboxString : options[index]).X;
			IClickableMenu.drawTextureBox(b, xPositionOnScreen + width / 2 - stringWidth / 2 - 16, yPositionOnScreen + 64 - 4, stringWidth + 32, 80, Color.White);
			if (index < options.Count)
			{
				Utility.drawTextWithShadow(b, isJukebox ? Utility.getSongTitleFromCueName(options[index]) : options[index], Game1.dialogueFont, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.dialogueFont.MeasureString(isJukebox ? Utility.getSongTitleFromCueName(options[index]) : options[index]).X / 2f, yPositionOnScreen + height / 2 - 16), Game1.textColor);
			}
			okButton.draw(b);
			cancelButton.draw(b);
			forwardButton.draw(b);
			backButton.draw(b);
			if (isJukebox)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:JukeboxMenu_Title"), xPositionOnScreen + width / 2, yPositionOnScreen - 32);
			}
			drawMouse(b);
		}
	}
}
