using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class TutorialMenu : IClickableMenu
	{
		public const int farmingTab = 0;

		public const int fishingTab = 1;

		public const int miningTab = 2;

		public const int craftingTab = 3;

		public const int constructionTab = 4;

		public const int friendshipTab = 5;

		public const int townTab = 6;

		public const int animalsTab = 7;

		private int currentTab = -1;

		private List<ClickableTextureComponent> topics = new List<ClickableTextureComponent>();

		private ClickableTextureComponent backButton;

		private ClickableTextureComponent okButton;

		private List<ClickableTextureComponent> icons = new List<ClickableTextureComponent>();

		public TutorialMenu()
			: base(Game1.viewport.Width / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 192, 600 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 192)
		{
			int xPos = xPositionOnScreen + 64 + 42 - 2;
			int yPos9 = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
			topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos9, width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11805"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
			icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos9, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 276), 1f));
			yPos9 += 68;
			topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos9, width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11807"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
			icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos9, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 142), 1f));
			yPos9 += 68;
			topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos9, width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11809"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
			icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos9, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 334), 1f));
			yPos9 += 68;
			topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos9, width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11811"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
			icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos9, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 308), 1f));
			yPos9 += 68;
			topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos9, width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11813"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
			icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos9, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 395), 1f));
			yPos9 += 68;
			topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos9, width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11815"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
			icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos9, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 458), 1f));
			yPos9 += 68;
			topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos9, width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11817"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
			icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos9, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 102), 1f));
			yPos9 += 68;
			topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos9, width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11819"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
			icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos9, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 403), 1f));
			yPos9 += 68;
			okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
			backButton = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 48, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (currentTab == -1)
			{
				for (int i = 0; i < topics.Count; i++)
				{
					if (topics[i].containsPoint(x, y))
					{
						currentTab = i;
						Game1.playSound("smallSelect");
						break;
					}
				}
			}
			if (currentTab != -1 && backButton.containsPoint(x, y))
			{
				currentTab = -1;
				Game1.playSound("bigDeSelect");
			}
			else if (currentTab == -1 && okButton.containsPoint(x, y))
			{
				Game1.playSound("bigDeSelect");
				Game1.exitActiveMenu();
				if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			foreach (ClickableTextureComponent c in topics)
			{
				if (c.containsPoint(x, y))
				{
					c.scale = 2f;
				}
				else
				{
					c.scale = 1f;
				}
			}
			if (okButton.containsPoint(x, y))
			{
				okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.1f);
			}
			else
			{
				okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);
			}
			if (backButton.containsPoint(x, y))
			{
				backButton.scale = Math.Min(backButton.scale + 0.02f, backButton.baseScale + 0.1f);
			}
			else
			{
				backButton.scale = Math.Max(backButton.scale - 0.02f, backButton.baseScale);
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
			if (currentTab != -1)
			{
				backButton.draw(b);
				b.Draw(topics[currentTab].texture, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16), topics[currentTab].texture.Bounds, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.89f);
			}
			else
			{
				foreach (ClickableTextureComponent c in topics)
				{
					Color color = (c.scale > 1f) ? Color.Blue : Game1.textColor;
					b.DrawString(Game1.smallFont, c.label, new Vector2(c.bounds.X + 64 + 16, c.bounds.Y + 21), color);
				}
				foreach (ClickableTextureComponent icon in icons)
				{
					icon.draw(b);
				}
				okButton.draw(b);
			}
			drawMouse(b);
		}
	}
}
