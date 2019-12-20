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

		public Background()
		{
			summitBG = true;
			c = Color.White;
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
				float alpha = 1f;
				Color bgColor = Color.White;
				if (Game1.timeOfDay >= 1800)
				{
					int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
					c = new Color(255f, 255f - Math.Max(100f, (float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f - 1800f), 255f - Math.Max(100f, ((float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f - 1800f) / 2f));
					bgColor = Color.Blue * 0.5f;
					alpha = Math.Max(0f, Math.Min(1f, (2000f - ((float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f)) / 200f));
				}
				b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height * 3 / 4), new Microsoft.Xna.Framework.Rectangle(639 + (seasonOffset + 1), 1051, 1, 400), c * alpha, 0f, Vector2.Zero, SpriteEffects.None, 1E-07f);
				b.Draw(Game1.mouseCursors, new Vector2(0f, Game1.viewport.Height - 596), new Microsoft.Xna.Framework.Rectangle(0, 736 + seasonOffset * 149, 639, 149), Color.White * Math.Max((int)c.A, 0.5f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
				b.Draw(Game1.mouseCursors, new Vector2(0f, Game1.viewport.Height - 596), new Microsoft.Xna.Framework.Rectangle(0, 736 + seasonOffset * 149, 639, 149), bgColor * (0.75f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
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
