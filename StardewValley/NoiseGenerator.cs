using System;

namespace StardewValley
{
	[InstanceStatics]
	internal static class NoiseGenerator
	{
		public static int Seed
		{
			get;
			set;
		}

		public static int Octaves
		{
			get;
			set;
		}

		public static double Amplitude
		{
			get;
			set;
		}

		public static double Persistence
		{
			get;
			set;
		}

		public static double Frequency
		{
			get;
			set;
		}

		static NoiseGenerator()
		{
			Seed = new Random().Next(int.MaxValue);
			Octaves = 8;
			Amplitude = 1.0;
			Frequency = 0.015;
			Persistence = 0.65;
		}

		public static double Noise(int x, int y)
		{
			double total = 0.0;
			double freq = Frequency;
			double amp = Amplitude;
			for (int i = 0; i < Octaves; i++)
			{
				total += Smooth((double)x * freq, (double)y * freq) * amp;
				freq *= 2.0;
				amp *= Persistence;
			}
			if (total < -2.4)
			{
				total = -2.4;
			}
			else if (total > 2.4)
			{
				total = 2.4;
			}
			return total / 2.4;
		}

		public static double NoiseGeneration(int x, int y)
		{
			int j = x + y * 57;
			j = ((j << 13) ^ j);
			return 1.0 - (double)((j * (j * j * 15731 + 789221) + Seed) & int.MaxValue) / 1073741824.0;
		}

		private static double Interpolate(double x, double y, double a)
		{
			double value = (1.0 - Math.Cos(a * Math.PI)) * 0.5;
			return x * (1.0 - value) + y * value;
		}

		private static double Smooth(double x, double y)
		{
			double x2 = NoiseGeneration((int)x, (int)y);
			double n6 = NoiseGeneration((int)x + 1, (int)y);
			double n5 = NoiseGeneration((int)x, (int)y + 1);
			double n4 = NoiseGeneration((int)x + 1, (int)y + 1);
			double x3 = Interpolate(x2, n6, x - (double)(int)x);
			double i2 = Interpolate(n5, n4, x - (double)(int)x);
			return Interpolate(x3, i2, y - (double)(int)y);
		}
	}
}
