using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.Menus
{
	public class ColorPicker
	{
		public const int sliderChunks = 24;

		private int colorIndexSelection;

		private Rectangle bounds;

		public SliderBar hueBar;

		public SliderBar valueBar;

		public SliderBar saturationBar;

		public SliderBar recentSliderBar;

		public string Name;

		public Color LastColor;

		public bool Dirty;

		public ColorPicker(string name, int x, int y)
		{
			Name = name;
			hueBar = new SliderBar(0, 0, 50);
			saturationBar = new SliderBar(0, 20, 50);
			valueBar = new SliderBar(0, 40, 50);
			bounds = new Rectangle(x, y, SliderBar.defaultWidth, 60);
		}

		public Color getSelectedColor()
		{
			return HsvToRgb((double)hueBar.value / 100.0 * 360.0, (double)saturationBar.value / 100.0, (double)valueBar.value / 100.0);
		}

		public Color click(int x, int y)
		{
			if (bounds.Contains(x, y))
			{
				x -= bounds.X;
				y -= bounds.Y;
				if (hueBar.bounds.Contains(x, y))
				{
					hueBar.click(x, y);
					recentSliderBar = hueBar;
				}
				if (saturationBar.bounds.Contains(x, y))
				{
					recentSliderBar = saturationBar;
					saturationBar.click(x, y);
				}
				if (valueBar.bounds.Contains(x, y))
				{
					recentSliderBar = valueBar;
					valueBar.click(x, y);
				}
			}
			return getSelectedColor();
		}

		public void changeHue(int amount)
		{
			hueBar.changeValueBy(amount);
			recentSliderBar = hueBar;
		}

		public void changeSaturation(int amount)
		{
			saturationBar.changeValueBy(amount);
			recentSliderBar = saturationBar;
		}

		public void changeValue(int amount)
		{
			valueBar.changeValueBy(amount);
			recentSliderBar = valueBar;
		}

		public Color clickHeld(int x, int y)
		{
			if (recentSliderBar != null)
			{
				x = Math.Max(x, bounds.X);
				x = Math.Min(x, bounds.Right - 1);
				y = recentSliderBar.bounds.Center.Y;
				x -= bounds.X;
				if (recentSliderBar.Equals(hueBar))
				{
					hueBar.click(x, y);
				}
				if (recentSliderBar.Equals(saturationBar))
				{
					saturationBar.click(x, y);
				}
				if (recentSliderBar.Equals(valueBar))
				{
					valueBar.click(x, y);
				}
			}
			return getSelectedColor();
		}

		public void releaseClick()
		{
			hueBar.release(0, 0);
			saturationBar.release(0, 0);
			valueBar.release(0, 0);
			recentSliderBar = null;
		}

		public void draw(SpriteBatch b)
		{
			for (int k = 0; k < 24; k++)
			{
				Color c = HsvToRgb((double)k / 24.0 * 360.0, 0.9, 0.9);
				b.Draw(Game1.staminaRect, new Rectangle(bounds.X + bounds.Width / 24 * k, bounds.Y + hueBar.bounds.Center.Y - 2, hueBar.bounds.Width / 24, 4), c);
			}
			b.Draw(Game1.mouseCursors, new Vector2(bounds.X + (int)((float)hueBar.value / 100f * (float)hueBar.bounds.Width), bounds.Y + hueBar.bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
			Utility.drawTextWithShadow(b, string.Concat(hueBar.value), Game1.smallFont, new Vector2(bounds.X + bounds.Width + 8, bounds.Y + hueBar.bounds.Y), Game1.textColor);
			for (int j = 0; j < 24; j++)
			{
				Color c2 = HsvToRgb((double)hueBar.value / 100.0 * 360.0, (double)j / 24.0, (double)valueBar.value / 100.0);
				b.Draw(Game1.staminaRect, new Rectangle(bounds.X + bounds.Width / 24 * j, bounds.Y + saturationBar.bounds.Center.Y - 2, saturationBar.bounds.Width / 24, 4), c2);
			}
			b.Draw(Game1.mouseCursors, new Vector2(bounds.X + (int)((float)saturationBar.value / 100f * (float)saturationBar.bounds.Width), bounds.Y + saturationBar.bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.87f);
			Utility.drawTextWithShadow(b, string.Concat(saturationBar.value), Game1.smallFont, new Vector2(bounds.X + bounds.Width + 8, bounds.Y + saturationBar.bounds.Y), Game1.textColor);
			for (int i = 0; i < 24; i++)
			{
				Color c3 = HsvToRgb((double)hueBar.value / 100.0 * 360.0, (double)saturationBar.value / 100.0, (double)i / 24.0);
				b.Draw(Game1.staminaRect, new Rectangle(bounds.X + bounds.Width / 24 * i, bounds.Y + valueBar.bounds.Center.Y - 2, valueBar.bounds.Width / 24, 4), c3);
			}
			b.Draw(Game1.mouseCursors, new Vector2(bounds.X + (int)((float)valueBar.value / 100f * (float)valueBar.bounds.Width), bounds.Y + valueBar.bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
			Utility.drawTextWithShadow(b, string.Concat(valueBar.value), Game1.smallFont, new Vector2(bounds.X + bounds.Width + 8, bounds.Y + valueBar.bounds.Y), Game1.textColor);
		}

		public bool containsPoint(int x, int y)
		{
			return bounds.Contains(x, y);
		}

		public void setColor(Color color)
		{
			RGBtoHSV((int)color.R, (int)color.G, (int)color.B, out float hue, out float sat, out float val);
			if (float.IsNaN(hue))
			{
				hue = 0f;
			}
			if (float.IsNaN(sat))
			{
				sat = 0f;
			}
			if (float.IsNaN(hue))
			{
				hue = 0f;
			}
			hueBar.value = (int)(hue / 360f * 100f);
			saturationBar.value = (int)(sat * 100f);
			valueBar.value = (int)(val / 255f * 100f);
		}

		private void RGBtoHSV(float r, float g, float b, out float h, out float s, out float v)
		{
			float min = Math.Min(Math.Min(r, g), b);
			float max = v = Math.Max(Math.Max(r, g), b);
			float delta = max - min;
			if (max != 0f)
			{
				s = delta / max;
				if (r == max)
				{
					h = (g - b) / delta;
				}
				else if (g == max)
				{
					h = 2f + (b - r) / delta;
				}
				else
				{
					h = 4f + (r - g) / delta;
				}
				h *= 60f;
				if (h < 0f)
				{
					h += 360f;
				}
			}
			else
			{
				s = 0f;
				h = -1f;
			}
		}

		private Color HsvToRgb(double h, double S, double V)
		{
			double H = h;
			while (H < 0.0)
			{
				H += 1.0;
				if (H < -1000000.0)
				{
					H = 0.0;
				}
			}
			while (H >= 360.0)
			{
				H -= 1.0;
			}
			double R;
			double G;
			double B;
			if (V <= 0.0)
			{
				R = (G = (B = 0.0));
			}
			else if (S <= 0.0)
			{
				R = (G = (B = V));
			}
			else
			{
				double num = H / 60.0;
				int i = (int)Math.Floor(num);
				double f = num - (double)i;
				double pv = V * (1.0 - S);
				double qv = V * (1.0 - S * f);
				double tv = V * (1.0 - S * (1.0 - f));
				switch (i)
				{
				case 0:
					R = V;
					G = tv;
					B = pv;
					break;
				case 1:
					R = qv;
					G = V;
					B = pv;
					break;
				case 2:
					R = pv;
					G = V;
					B = tv;
					break;
				case 3:
					R = pv;
					G = qv;
					B = V;
					break;
				case 4:
					R = tv;
					G = pv;
					B = V;
					break;
				case 5:
					R = V;
					G = pv;
					B = qv;
					break;
				case 6:
					R = V;
					G = tv;
					B = pv;
					break;
				case -1:
					R = V;
					G = pv;
					B = qv;
					break;
				default:
					R = (G = (B = V));
					break;
				}
			}
			return new Color(Clamp((int)(R * 255.0)), Clamp((int)(G * 255.0)), Clamp((int)(B * 255.0)));
		}

		private int Clamp(int i)
		{
			if (i < 0)
			{
				return 0;
			}
			if (i > 255)
			{
				return 255;
			}
			return i;
		}
	}
}
