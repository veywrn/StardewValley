using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class LanguageSelectionMenu : IClickableMenu
	{
		public new const int width = 500;

		public new const int height = 728;

		private Texture2D texture;

		public List<ClickableComponent> languages = new List<ClickableComponent>();

		public LanguageSelectionMenu()
		{
			texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\LanguageButtons");
			SetupButtons();
		}

		private void SetupButtons()
		{
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(1250, 728);
			languages.Clear();
			int buttonHeight = 83;
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 64, (int)topLeft.Y + 728 - 30 - buttonHeight * 6 - 16, 372, buttonHeight), "English", null)
			{
				myID = 0,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 448, (int)topLeft.Y + 728 - 30 - buttonHeight * 6 - 16, 372, buttonHeight), "Russian", null)
			{
				myID = 3,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 832, (int)topLeft.Y + 728 - 30 - buttonHeight * 6 - 16, 372, buttonHeight), "Chinese", null)
			{
				myID = 6,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 64, (int)topLeft.Y + 728 - 30 - buttonHeight * 5 - 16, 372, buttonHeight), "German", null)
			{
				myID = 1,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 448, (int)topLeft.Y + 728 - 30 - buttonHeight * 5 - 16, 372, buttonHeight), "Portuguese", null)
			{
				myID = 4,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 832, (int)topLeft.Y + 728 - 30 - buttonHeight * 5 - 16, 372, buttonHeight), "French", null)
			{
				myID = 7,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 64, (int)topLeft.Y + 728 - 30 - buttonHeight * 4 - 16, 372, buttonHeight), "Spanish", null)
			{
				myID = 2,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 448, (int)topLeft.Y + 728 - 30 - buttonHeight * 4 - 16, 372, buttonHeight), "Japanese", null)
			{
				myID = 5,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 832, (int)topLeft.Y + 728 - 30 - buttonHeight * 4 - 16, 372, buttonHeight), "Korean", null)
			{
				myID = 8,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 64, (int)topLeft.Y + 728 - 30 - buttonHeight * 3 - 16, 372, buttonHeight), "Italian", null)
			{
				myID = 9,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 448, (int)topLeft.Y + 728 - 30 - buttonHeight * 3 - 16, 372, buttonHeight), "Turkish", null)
			{
				myID = 10,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 832, (int)topLeft.Y + 728 - 30 - buttonHeight * 3 - 16, 372, buttonHeight), "Hungarian", null)
			{
				myID = 11,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			if (Game1.options.SnappyMenus)
			{
				int id = (currentlySnappedComponent != null) ? currentlySnappedComponent.myID : 0;
				populateClickableComponentList();
				currentlySnappedComponent = getComponentWithID(id);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			foreach (ClickableComponent component in languages)
			{
				if (component.containsPoint(x, y))
				{
					Game1.playSound("select");
					bool changed_language = true;
					switch (component.name)
					{
					case "English":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.en;
						break;
					case "German":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.de;
						break;
					case "Russian":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ru;
						break;
					case "Chinese":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.zh;
						break;
					case "Japanese":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ja;
						break;
					case "Spanish":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.es;
						break;
					case "Portuguese":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.pt;
						break;
					case "French":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.fr;
						break;
					case "Korean":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ko;
						break;
					case "Italian":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.it;
						break;
					case "Turkish":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.tr;
						break;
					case "Hungarian":
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.hu;
						break;
					default:
						changed_language = false;
						break;
					}
					if (Game1.options.SnappyMenus)
					{
						Game1.activeClickableMenu.setCurrentlySnappedComponentTo(81118);
						Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
					}
					if (changed_language)
					{
						exitThisMenu();
					}
				}
			}
			isWithinBounds(x, y);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableComponent component in languages)
			{
				if (component.containsPoint(x, y))
				{
					if (component.label == null)
					{
						Game1.playSound("Cowboy_Footstep");
						component.label = "hovered";
					}
				}
				else
				{
					component.label = null;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void draw(SpriteBatch b)
		{
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(1250, 728);
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)topLeft.X + 32, (int)topLeft.Y + 156, 1211, 389, Color.White, 4f);
			foreach (ClickableComponent c in languages)
			{
				int i = 0;
				switch (c.name)
				{
				case "English":
					i = 0;
					break;
				case "Spanish":
					i = 1;
					break;
				case "Portuguese":
					i = 2;
					break;
				case "Russian":
					i = 3;
					break;
				case "Chinese":
					i = 4;
					break;
				case "Japanese":
					i = 5;
					break;
				case "German":
					i = 6;
					break;
				case "French":
					i = 7;
					break;
				case "Korean":
					i = 8;
					break;
				case "Italian":
					i = 10;
					break;
				case "Turkish":
					i = 9;
					break;
				case "Hungarian":
					i = 11;
					break;
				}
				int buttonSourceY2 = (i <= 6) ? (i * 78) : ((i - 7) * 78);
				buttonSourceY2 += ((c.label != null) ? 39 : 0);
				int buttonSourceX = (i > 6) ? 174 : 0;
				b.Draw(texture, c.bounds, new Rectangle(buttonSourceX, buttonSourceY2, 174, 40), Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0f);
			}
		}

		public override bool readyToClose()
		{
			return true;
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			SetupButtons();
		}
	}
}
