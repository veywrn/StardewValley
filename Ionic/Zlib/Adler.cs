namespace Ionic.Zlib
{
	public sealed class Adler
	{
		private static readonly uint BASE = 65521u;

		private static readonly int NMAX = 5552;

		public static uint Adler32(uint adler, byte[] buf, int index, int len)
		{
			if (buf == null)
			{
				return 1u;
			}
			uint s17 = adler & 0xFFFF;
			uint s21 = (adler >> 16) & 0xFFFF;
			while (len > 0)
			{
				int i = (len < NMAX) ? len : NMAX;
				len -= i;
				while (i >= 16)
				{
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					s17 += buf[index++];
					s21 += s17;
					i -= 16;
				}
				if (i != 0)
				{
					do
					{
						s17 += buf[index++];
						s21 += s17;
					}
					while (--i != 0);
				}
				s17 %= BASE;
				s21 %= BASE;
			}
			return (s21 << 16) | s17;
		}
	}
}
