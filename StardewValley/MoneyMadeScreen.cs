using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley
{
	public class MoneyMadeScreen
	{
		private const int timeToDisplayEachItem = 200;

		private Dictionary<ShippedItem, int> shippingItems = new Dictionary<ShippedItem, int>();

		public bool complete;

		public bool canProceed;

		public bool throbUp;

		public bool day;

		private int currentItemIndex;

		private int timeOnCurrentItem;

		private int total;

		private float starScale = 1f;

		public MoneyMadeScreen(List<Object> shippingItems, int timeOfDay)
		{
			if (timeOfDay < 2000)
			{
				day = true;
			}
			int itemOfTheDay = Utility.getRandomItemFromSeason(Game1.currentSeason, 0, forQuest: false);
			int cropOfTheWeek = Game1.cropsOfTheWeek[(Game1.dayOfMonth - 1) / 7];
			foreach (Object o in shippingItems)
			{
				ShippedItem s = new ShippedItem(o.ParentSheetIndex, o.Price, o.name);
				int price2 = o.Price * o.Stack;
				if (o.ParentSheetIndex == itemOfTheDay)
				{
					price2 = (int)((float)price2 * 1.2f);
				}
				if (o.ParentSheetIndex == cropOfTheWeek)
				{
					price2 = (int)((float)price2 * 1.1f);
				}
				if (o.Name.Contains("="))
				{
					price2 += price2 / 2;
				}
				price2 -= price2 % 5;
				if (this.shippingItems.ContainsKey(s))
				{
					this.shippingItems[s]++;
				}
				else
				{
					this.shippingItems.Add(s, o.Stack);
				}
				total += price2;
			}
		}

		public void update(int milliseconds, bool keyDown)
		{
			if (!complete)
			{
				timeOnCurrentItem += (keyDown ? (milliseconds * 2) : milliseconds);
				if (timeOnCurrentItem >= 200)
				{
					currentItemIndex++;
					Game1.playSound("shiny4");
					timeOnCurrentItem = 0;
					if (currentItemIndex == shippingItems.Count)
					{
						complete = true;
					}
				}
			}
			else
			{
				timeOnCurrentItem += (keyDown ? (milliseconds * 2) : milliseconds);
				if (timeOnCurrentItem >= 1000)
				{
					canProceed = true;
				}
			}
			if (throbUp)
			{
				starScale += 0.01f;
			}
			else
			{
				starScale -= 0.01f;
			}
			if (starScale >= 1.2f)
			{
				throbUp = false;
			}
			else if (starScale <= 1f)
			{
				throbUp = true;
			}
		}

		public void draw(GameTime gametime)
		{
			if (day)
			{
				Game1.graphics.GraphicsDevice.Clear(Utility.getSkyColorForSeason(Game1.currentSeason));
			}
			Game1.drawTitleScreenBackground(gametime, day ? "_day" : "_night", Utility.weatherDebrisOffsetForSeason(Game1.currentSeason));
			int outerheight = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Height - 128;
			int x = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().X + Game1.graphics.GraphicsDevice.Viewport.Width / 2 - (int)((float)((shippingItems.Count / (outerheight / 64 - 4) + 1) * 64) * 3f);
			int y = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Y + 64;
			int width = (int)((float)((shippingItems.Count / (outerheight / 64 - 4) + 1) * 64) * 6f);
			Game1.drawDialogueBox(x, y, width, outerheight, speaker: false, drawOnlyBox: true);
			int height = outerheight - 192;
			Point topLeftCorner = new Point(x + 64, y + 64);
			for (int i = 0; i < currentItemIndex; i++)
			{
				Game1.spriteBatch.Draw(Game1.objectSpriteSheet, new Vector2(topLeftCorner.X + i * 64 / (height - 128) * 64 * 4 + 32, i * 64 % (height - 128) - i * 64 % (height - 128) % 64 + Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Y + 192 + 32), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, shippingItems.Keys.ElementAt(i).index), Color.White, 0f, new Vector2(32f, 32f), shippingItems.Keys.ElementAt(i).name.Contains("=") ? starScale : 1f, SpriteEffects.None, 0.999999f);
				Game1.spriteBatch.DrawString(Game1.dialogueFont, "x" + shippingItems[shippingItems.Keys.ElementAt(i)] + " : " + shippingItems.Keys.ElementAt(i).price * shippingItems[shippingItems.Keys.ElementAt(i)] + "g", new Vector2(topLeftCorner.X + i * 64 / (height - 128) * 64 * 4 + 64, (float)(i * 64 % (height - 128) - i * 64 % (height - 128) % 64 + 32) - Game1.dialogueFont.MeasureString("9").Y / 2f + (float)Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Y + 192f), Game1.textColor);
			}
			if (complete)
			{
				Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:MoneyMadeScreen.cs.3854", total), new Vector2((float)(x + width - 64) - Game1.dialogueFont.MeasureString("Total: " + total).X, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 160), Game1.textColor);
			}
		}
	}
}
