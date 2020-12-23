using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using xTile.Dimensions;

namespace StardewValley
{
	public class Background
	{
		public int defaultChunkIndex;

		public int numChunksInSheet;

		public double chanceForDeviationFromDefault;

		private Texture2D backgroundImage;

		private Texture2D cloudsTexture;

		private Vector2 position = Vector2.Zero;

		private int chunksWide;

		private int chunksHigh;

		private int chunkWidth;

		private int chunkHeight;

		private int[] chunks;

		private float zoom;

		public Color c;

		private bool summitBG;

		private bool onlyMapBG;

		public int yOffset;

		public List<TemporaryAnimatedSprite> tempSprites;

		private int initialViewportY;

		public Background()
		{
			summitBG = true;
			c = Color.White;
			initialViewportY = Game1.viewport.Y;
			cloudsTexture = Game1.content.Load<Texture2D>("Minigames\\Clouds");
		}

		public Background(Color color, bool onlyMapBG)
		{
			c = color;
			this.onlyMapBG = onlyMapBG;
			tempSprites = new List<TemporaryAnimatedSprite>();
		}

		public Background(Texture2D bgImage, int seedValue, int chunksWide, int chunksHigh, int chunkWidth, int chunkHeight, float zoom, int defaultChunkIndex, int numChunksInSheet, double chanceForDeviation, Color c)
		{
			backgroundImage = bgImage;
			this.chunksWide = chunksWide;
			this.chunksHigh = chunksHigh;
			this.zoom = zoom;
			this.chunkWidth = chunkWidth;
			this.chunkHeight = chunkHeight;
			this.defaultChunkIndex = defaultChunkIndex;
			this.numChunksInSheet = numChunksInSheet;
			chanceForDeviationFromDefault = chanceForDeviation;
			this.c = c;
			Random r = new Random(seedValue);
			chunks = new int[chunksWide * chunksHigh];
			for (int i = 0; i < chunksHigh * chunksWide; i++)
			{
				if (r.NextDouble() < chanceForDeviationFromDefault)
				{
					chunks[i] = r.Next(numChunksInSheet);
				}
				else
				{
					chunks[i] = defaultChunkIndex;
				}
			}
		}

		public void update(xTile.Dimensions.Rectangle viewport)
		{
			position.X = 0f - (float)(viewport.X + viewport.Width / 2) / ((float)Game1.currentLocation.map.GetLayer("Back").LayerWidth * 64f) * ((float)(chunksWide * chunkWidth) * zoom - (float)viewport.Width);
			position.Y = 0f - (float)(viewport.Y + viewport.Height / 2) / ((float)Game1.currentLocation.map.GetLayer("Back").LayerHeight * 64f) * ((float)(chunksHigh * chunkHeight) * zoom - (float)viewport.Height);
		}

		public void draw(SpriteBatch b)
		{
			if (summitBG)
			{
				if (Game1.viewport.X > -1000)
				{
					int seasonOffset = 0;
					switch (Game1.currentSeason)
					{
					case "summer":
						seasonOffset = 0;
						break;
					case "fall":
						seasonOffset = 1;
						break;
					case "winter":
						seasonOffset = 2;
						break;
					}
					int yOffset = -Game1.viewport.Y / 4 + initialViewportY / 4;
					float alpha = 1f;
					float skyAlpha = 1f;
					Color bgColor = Color.White;
					int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
					int oceanColorAddition = (Game1.currentSeason == "winter") ? 30 : 0;
					if (Game1.timeOfDay >= 1800)
					{
						c = new Color(255f, 255f - Math.Max(100f, (float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f - 1800f), 255f - Math.Max(100f, ((float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f - 1800f) / 2f));
						bgColor = ((Game1.currentSeason == "winter") ? (Color.Black * 0.5f) : (Color.Blue * 0.5f));
						alpha = Math.Max(0f, Math.Min(1f, (2000f - ((float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f)) / 200f));
						skyAlpha = Math.Max(0f, Math.Min(1f, (2200f - ((float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f)) / 400f));
						Game1.ambientLight = new Color((int)Utility.Lerp(0f, 30f, 1f - alpha), (int)Utility.Lerp(0f, 60f, 1f - alpha), (int)Utility.Lerp(0f, 15f, 1f - alpha));
					}
					b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Microsoft.Xna.Framework.Rectangle(639, 858, 1, 144), c * skyAlpha, 0f, Vector2.Zero, SpriteEffects.None, 5E-08f);
					b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), (Game1.currentSeason == "fall") ? new Microsoft.Xna.Framework.Rectangle(639, 1051, 1, 400) : new Microsoft.Xna.Framework.Rectangle(639 + (seasonOffset + 1), 1051, 1, 400), c * alpha, 0f, Vector2.Zero, SpriteEffects.None, 1E-07f);
					if (Game1.timeOfDay >= 1800)
					{
						b.Draw(Game1.mouseCursors, new Vector2(0f, Game1.viewport.Height / 2 - 780), new Microsoft.Xna.Framework.Rectangle(0, 1453, 638, 195), Color.White * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					if (Game1.dayOfMonth == 28 && Game1.timeOfDay > 1900)
					{
						b.Draw(Game1.mouseCursors, new Vector2(((float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f) / 2600f * (float)Game1.viewport.Width / 4f, (float)(Game1.viewport.Height / 2 + 176) - ((float)(adjustedTime - 1900) + (float)Game1.gameTimeInterval / 7000f * 16.6f) / 700f * (float)Game1.viewport.Height / 2f), new Microsoft.Xna.Framework.Rectangle(642, 834, 43, 44), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 5E-08f);
					}
					if (Game1.currentSeason != "winter" && ((bool)Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext()).isDebrisWeather || (bool)Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext()).isRaining))
					{
						b.Draw(cloudsTexture, new Vector2((float)Game1.viewport.Width - ((float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f) / 2600f * (float)(Game1.viewport.Width + 2048), Game1.viewport.Height - 584 - 600 + yOffset / 2 + Game1.dayOfMonth * 6), new Microsoft.Xna.Framework.Rectangle(0, 0, 512, 340), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 5.6E-08f);
					}
					b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(0, Game1.viewport.Height - 584 + yOffset / 2, Game1.viewport.Width, Game1.viewport.Height / 2), new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), new Color((int)((float)oceanColorAddition + 60f * skyAlpha), (int)((float)(oceanColorAddition + 10) + 170f * skyAlpha), (int)((float)(oceanColorAddition + 20) + 205f * skyAlpha)), 0f, Vector2.Zero, SpriteEffects.None, 2E-07f);
					b.Draw(Game1.mouseCursors, new Vector2(2556f, Game1.viewport.Height - 596 + yOffset), new Microsoft.Xna.Framework.Rectangle(0, 736 + seasonOffset * 149, 639, 149), Color.White * Math.Max((int)c.A, 0.5f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
					b.Draw(Game1.mouseCursors, new Vector2(2556f, Game1.viewport.Height - 596 + yOffset), new Microsoft.Xna.Framework.Rectangle(0, 736 + seasonOffset * 149, 639, 149), bgColor * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 2E-06f);
					b.Draw(Game1.mouseCursors, new Vector2(0f, Game1.viewport.Height - 596 + yOffset), new Microsoft.Xna.Framework.Rectangle(0, 736 + seasonOffset * 149, 639, 149), Color.White * Math.Max((int)c.A, 0.5f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
					b.Draw(Game1.mouseCursors, new Vector2(0f, Game1.viewport.Height - 596 + yOffset), new Microsoft.Xna.Framework.Rectangle(0, 736 + seasonOffset * 149, 639, 149), bgColor * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 2E-06f);
					foreach (TemporaryAnimatedSprite temporarySprite in Game1.currentLocation.temporarySprites)
					{
						temporarySprite.draw(b);
					}
					b.Draw(cloudsTexture, new Vector2(0f, (float)(Game1.viewport.Height - 568) + (float)yOffset * 2f), new Microsoft.Xna.Framework.Rectangle(0, 554 + seasonOffset * 153, 164, 142), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
					b.Draw(cloudsTexture, new Vector2(Game1.viewport.Width - 488, (float)(Game1.viewport.Height - 612) + (float)yOffset * 2f), new Microsoft.Xna.Framework.Rectangle(390, 543 + seasonOffset * 153, 122, 153), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
					b.Draw(cloudsTexture, new Vector2(0f, (float)(Game1.viewport.Height - 568) + (float)yOffset * 2f), new Microsoft.Xna.Framework.Rectangle(0, 554 + seasonOffset * 153, 164, 142), Color.Black * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					b.Draw(cloudsTexture, new Vector2(Game1.viewport.Width - 488, (float)(Game1.viewport.Height - 612) + (float)yOffset * 2f), new Microsoft.Xna.Framework.Rectangle(390, 543 + seasonOffset * 153, 122, 153), Color.Black * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
				return;
			}
			if (backgroundImage == null)
			{
				Microsoft.Xna.Framework.Rectangle display = new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
				if (onlyMapBG)
				{
					display.X = Math.Max(0, -Game1.viewport.X);
					display.Y = Math.Max(0, -Game1.viewport.Y);
					display.Width = Math.Min(Game1.viewport.Width, Game1.currentLocation.map.DisplayWidth);
					display.Height = Math.Min(Game1.viewport.Height, Game1.currentLocation.map.DisplayHeight);
				}
				b.Draw(Game1.staminaRect, display, Game1.staminaRect.Bounds, c, 0f, Vector2.Zero, SpriteEffects.None, 0f);
				for (int k = tempSprites.Count - 1; k >= 0; k--)
				{
					if (tempSprites[k].update(Game1.currentGameTime))
					{
						tempSprites.RemoveAt(k);
					}
				}
				for (int j = 0; j < tempSprites.Count; j++)
				{
					tempSprites[j].draw(b);
				}
				return;
			}
			Vector2 v = Vector2.Zero;
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(0, 0, chunkWidth, chunkHeight);
			for (int i = 0; i < chunks.Length; i++)
			{
				v.X = position.X + (float)(i * chunkWidth % (chunksWide * chunkWidth)) * zoom;
				v.Y = position.Y + (float)(i * chunkWidth / (chunksWide * chunkWidth) * chunkHeight) * zoom;
				if (backgroundImage == null)
				{
					b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)v.X, (int)v.Y, Game1.viewport.Width, Game1.viewport.Height), r, c, 0f, Vector2.Zero, SpriteEffects.None, 0f);
					continue;
				}
				r.X = chunks[i] * chunkWidth % backgroundImage.Width;
				r.Y = chunks[i] * chunkWidth / backgroundImage.Width * chunkHeight;
				b.Draw(backgroundImage, v, r, c, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			}
		}
	}
}
