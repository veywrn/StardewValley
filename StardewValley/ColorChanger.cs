using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley
{
	internal class ColorChanger
	{
		private static Color[] _buffer;

		public static Texture2D swapColor(Texture2D texture, int targetColorIndex, int red, int green, int blue)
		{
			return swapColor(texture, targetColorIndex, red, green, blue, 0, texture.Width * texture.Height);
		}

		private static Color[] getBuffer(int len)
		{
			if (_buffer == null || _buffer.Length < len)
			{
				_buffer = new Color[len];
			}
			return _buffer;
		}

		public unsafe static Texture2D swapColor(Texture2D texture, int targetColorIndex1, int r1, int g1, int b1, int startPixelIndex, int endPixelIndex)
		{
			r1 = Math.Min(Math.Max(1, r1), 255);
			g1 = Math.Min(Math.Max(1, g1), 255);
			b1 = Math.Min(Math.Max(1, b1), 255);
			uint dstColor1_packed = new Color(r1, g1, b1).PackedValue;
			Color[] data = getBuffer(texture.Width * texture.Height);
			texture.GetData(data);
			Color srcColor = data[targetColorIndex1];
			uint srcColor1_packed = srcColor.PackedValue;
			fixed (Color* pData = data)
			{
				Color* intPtr = pData + startPixelIndex;
				Color* pEnd = pData + endPixelIndex;
				for (Color* itr = intPtr; itr <= pEnd; itr++)
				{
					if (itr->PackedValue == srcColor1_packed)
					{
						itr->PackedValue = dstColor1_packed;
					}
				}
			}
			texture.SetData(data);
			return texture;
		}

		public unsafe static void swapColors(Texture2D texture, int targetColorIndex1, byte r1, byte g1, byte b1, int targetColorIndex2, byte r2, byte g2, byte b2)
		{
			r1 = Math.Min(Math.Max((byte)1, r1), byte.MaxValue);
			g1 = Math.Min(Math.Max((byte)1, g1), byte.MaxValue);
			b1 = Math.Min(Math.Max((byte)1, b1), byte.MaxValue);
			r2 = Math.Min(Math.Max((byte)1, r2), byte.MaxValue);
			g2 = Math.Min(Math.Max((byte)1, g2), byte.MaxValue);
			b2 = Math.Min(Math.Max((byte)1, b2), byte.MaxValue);
			Color dstColor3 = new Color(r1, g1, b1);
			Color dstColor2 = new Color(r2, g2, b2);
			uint dstColor1_packed = dstColor3.PackedValue;
			uint dstColor2_packed = dstColor2.PackedValue;
			int num = texture.Width * texture.Height;
			Color[] data = getBuffer(num);
			texture.GetData(data);
			Color srcColor3 = data[targetColorIndex1];
			Color srcColor2 = data[targetColorIndex2];
			uint srcColor1_packed = srcColor3.PackedValue;
			uint srcColor2_packed = srcColor2.PackedValue;
			int startPixelIndex = 0;
			int endPixelIndex = num;
			fixed (Color* pData = data)
			{
				Color* intPtr = pData + startPixelIndex;
				Color* pEnd = pData + endPixelIndex;
				for (Color* itr = intPtr; itr <= pEnd; itr++)
				{
					if (itr->PackedValue == srcColor1_packed)
					{
						itr->PackedValue = dstColor1_packed;
					}
					else if (itr->PackedValue == srcColor2_packed)
					{
						itr->PackedValue = dstColor2_packed;
					}
				}
			}
			texture.SetData(data);
		}
	}
}
