using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.Network
{
	public class BarGraph
	{
		public static double DYNAMIC_SCALE_MAX = -1.0;

		public static double DYNAMIC_SCALE_AVG = -2.0;

		private Queue<double> elements;

		private int height;

		private int width;

		private int x;

		private int y;

		private double maxValue;

		private Color barColor;

		private int elementWidth;

		private Texture2D whiteTexture;

		public BarGraph(Queue<double> elements, int x, int y, int width, int height, int elementWidth, double maxValue, Color barColor, Texture2D whiteTexture)
		{
			this.elements = elements;
			this.width = width;
			this.height = height;
			this.x = x;
			this.y = y;
			this.maxValue = maxValue;
			this.barColor = barColor;
			this.elementWidth = elementWidth;
			this.whiteTexture = whiteTexture;
		}

		public void Draw(SpriteBatch sb)
		{
			double scaleMaxValue = maxValue;
			if (scaleMaxValue == DYNAMIC_SCALE_MAX)
			{
				foreach (double element2 in elements)
				{
					scaleMaxValue = Math.Max(element2, scaleMaxValue);
				}
			}
			else if (scaleMaxValue == DYNAMIC_SCALE_AVG)
			{
				double total = 0.0;
				foreach (double element in elements)
				{
					total += element;
				}
				scaleMaxValue = total / (double)Math.Max(1, elements.Count);
			}
			sb.Draw(whiteTexture, new Rectangle(x - 1, y, width, height), null, Color.Black * 0.5f);
			int leftX = x + width - elementWidth * elements.Count;
			int i = 0;
			foreach (double element3 in elements)
			{
				int elementX = leftX + i * elementWidth;
				int elementY = y;
				int elementHeight = (int)((double)(float)element3 / scaleMaxValue * (double)height);
				if (element3 > scaleMaxValue)
				{
					Console.WriteLine("?");
				}
				sb.Draw(whiteTexture, new Rectangle(elementX, elementY + height - elementHeight, elementWidth, elementHeight), null, barColor);
				i++;
			}
		}
	}
}
