using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class SparklingText
	{
		public static int maxDistanceForSparkle = 32;

		private SpriteFont font;

		private Color color;

		private Color sparkleColor;

		private bool rainbow;

		private int millisecondsDuration;

		private int amplitude;

		private int period;

		private int colorCycle;

		public string text;

		private float[] individualCharacterOffsets;

		public float offsetDecay = 1f;

		public float alpha = 1f;

		public float textWidth;

		public float drawnTextWidth;

		public float layerDepth = 1f;

		private double sparkleFrequency;

		private List<TemporaryAnimatedSprite> sparkles;

		private List<Vector2> sparkleTrash;

		private Rectangle boundingBox;

		public SparklingText(SpriteFont font, string text, Color color, Color sparkleColor, bool rainbow = false, double sparkleFrequency = 0.1, int millisecondsDuration = 2500, int amplitude = -1, int speed = 500, float depth = 1f)
		{
			if (amplitude == -1)
			{
				amplitude = 64;
			}
			maxDistanceForSparkle = 32;
			this.font = font;
			this.color = color;
			this.sparkleColor = sparkleColor;
			this.text = text;
			this.rainbow = rainbow;
			if (rainbow)
			{
				color = Color.Yellow;
			}
			this.sparkleFrequency = sparkleFrequency;
			this.millisecondsDuration = millisecondsDuration;
			individualCharacterOffsets = new float[text.Length];
			this.amplitude = amplitude;
			period = speed;
			sparkles = new List<TemporaryAnimatedSprite>();
			boundingBox = new Rectangle(-maxDistanceForSparkle, -maxDistanceForSparkle, (int)font.MeasureString(text).X + maxDistanceForSparkle * 2, (int)font.MeasureString(text).Y + maxDistanceForSparkle * 2);
			sparkleTrash = new List<Vector2>();
			textWidth = font.MeasureString(text).X;
			layerDepth = depth;
			int xOffset = 0;
			for (int i = 0; i < text.Length; i++)
			{
				xOffset += (int)font.MeasureString(text[i].ToString() ?? "").X;
			}
			drawnTextWidth = xOffset;
		}

		public bool update(GameTime time)
		{
			millisecondsDuration -= time.ElapsedGameTime.Milliseconds;
			offsetDecay -= 0.001f;
			amplitude = (int)((float)amplitude * offsetDecay);
			if (millisecondsDuration <= 500)
			{
				alpha = (float)millisecondsDuration / 500f;
			}
			for (int j = 0; j < individualCharacterOffsets.Length; j++)
			{
				individualCharacterOffsets[j] = (float)((double)(amplitude / 2) * Math.Sin(Math.PI * 2.0 / (double)period * (double)(millisecondsDuration - j * 100)));
			}
			if (millisecondsDuration > 500 && Game1.random.NextDouble() < sparkleFrequency)
			{
				int speed = Game1.random.Next(100, 600);
				sparkles.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 704, 64, 64), speed / 6, 6, 0, new Vector2(Game1.random.Next(boundingBox.X, boundingBox.Right), Game1.random.Next(boundingBox.Y, boundingBox.Bottom)), flicker: false, flipped: false, layerDepth, 0f, rainbow ? color : sparkleColor, 1f, 0f, 0f, 0f));
			}
			for (int i = sparkles.Count - 1; i >= 0; i--)
			{
				if (sparkles[i].update(time))
				{
					sparkles.RemoveAt(i);
				}
			}
			if (rainbow)
			{
				incrementRainbowColors();
			}
			return millisecondsDuration <= 0;
		}

		private void incrementRainbowColors()
		{
			if (colorCycle != 0)
			{
				return;
			}
			if ((color.G += 4) >= byte.MaxValue)
			{
				colorCycle = 1;
			}
			else
			{
				if (colorCycle != 1)
				{
					return;
				}
				if ((color.R -= 4) <= 0)
				{
					colorCycle = 2;
				}
				else
				{
					if (colorCycle != 2)
					{
						return;
					}
					if ((color.B += 4) >= byte.MaxValue)
					{
						colorCycle = 3;
					}
					else
					{
						if (colorCycle != 3)
						{
							return;
						}
						if ((color.G -= 4) <= 0)
						{
							colorCycle = 4;
						}
						else if (colorCycle == 4)
						{
							if (++color.R >= byte.MaxValue)
							{
								colorCycle = 5;
							}
							else if (colorCycle == 5 && (color.B -= 4) <= 0)
							{
								colorCycle = 0;
							}
						}
					}
				}
			}
		}

		private static Color getRainbowColorFromIndex(int index)
		{
			switch (index % 8)
			{
			case 0:
				return Color.Red;
			case 1:
				return Color.Orange;
			case 2:
				return Color.Yellow;
			case 3:
				return Color.Chartreuse;
			case 4:
				return Color.Green;
			case 5:
				return Color.Cyan;
			case 6:
				return Color.Blue;
			case 7:
				return Color.Violet;
			default:
				return Color.White;
			}
		}

		public void draw(SpriteBatch b, Vector2 onScreenPosition)
		{
			int xOffset = 0;
			for (int i = 0; i < text.Length; i++)
			{
				b.DrawString(font, text[i].ToString() ?? "", onScreenPosition + new Vector2(xOffset - 2, individualCharacterOffsets[i]), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
				b.DrawString(font, text[i].ToString() ?? "", onScreenPosition + new Vector2(xOffset + 2, individualCharacterOffsets[i]), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.991f);
				b.DrawString(font, text[i].ToString() ?? "", onScreenPosition + new Vector2(xOffset, individualCharacterOffsets[i] - 2f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.992f);
				b.DrawString(font, text[i].ToString() ?? "", onScreenPosition + new Vector2(xOffset, individualCharacterOffsets[i] + 2f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.993f);
				b.DrawString(font, text[i].ToString() ?? "", onScreenPosition + new Vector2(xOffset, individualCharacterOffsets[i]), rainbow ? getRainbowColorFromIndex(i) : (color * alpha), 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
				xOffset += (int)font.MeasureString(text[i].ToString() ?? "").X;
			}
			font.MeasureString(text);
			foreach (TemporaryAnimatedSprite sparkle in sparkles)
			{
				sparkle.Position += onScreenPosition;
				sparkle.draw(b, localPosition: true);
				sparkle.Position -= onScreenPosition;
			}
		}
	}
}
