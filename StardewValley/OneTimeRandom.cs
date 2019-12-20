namespace StardewValley
{
	internal class OneTimeRandom
	{
		private const double shift3 = 0.125;

		private const double shift9 = 0.001953125;

		private const double shift27 = 7.4505805969238281E-09;

		private const double shift53 = 1.1102230246251565E-16;

		public static ulong GetLong(ulong a, ulong b, ulong c, ulong d)
		{
			ulong e = ((a ^ ((b >> 14) | (b << 50))) + (((c >> 31) | (c << 33)) ^ ((d >> 18) | (d << 46)))) * 1911413418482053185L;
			ulong f = ((((a >> 30) | (a << 34)) ^ c) + (((b >> 32) | (b << 32)) ^ ((d >> 50) | (d << 14)))) * 1139072524405308145L;
			ulong g = ((((a >> 49) | (a << 15)) ^ ((d >> 33) | (d << 31))) + (b ^ ((c >> 48) | (c << 16)))) * 8792993707439626365L;
			ulong h = ((((a >> 17) | (a << 47)) ^ ((b >> 47) | (b << 17))) + (((c >> 15) | (c << 49)) ^ d)) * 1089642907432013597L;
			return (e ^ f ^ ((g >> 21) | (g << 43)) ^ ((h >> 44) | (h << 20))) * 2550117894111961111L + (((e >> 20) | (e << 44)) ^ ((f >> 41) | (f << 23)) ^ ((g >> 42) | (g << 22)) ^ h) * 8786584852613159497L + (((e >> 43) | (e << 21)) ^ ((f >> 22) | (f << 42)) ^ g ^ ((h >> 23) | (h << 41))) * 3971056679291618767L;
		}

		public static double GetDouble(ulong a, ulong b, ulong c, ulong d)
		{
			return (double)(GetLong(a, b, c, d) >> 11) * 1.1102230246251565E-16;
		}
	}
}
