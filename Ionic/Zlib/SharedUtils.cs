using System.IO;
using System.Text;

namespace Ionic.Zlib
{
	internal class SharedUtils
	{
		public static int URShift(int number, int bits)
		{
			return (int)((uint)number >> bits);
		}

		public static int ReadInput(TextReader sourceTextReader, byte[] target, int start, int count)
		{
			if (target.Length == 0)
			{
				return 0;
			}
			char[] charArray = new char[target.Length];
			int bytesRead = sourceTextReader.Read(charArray, start, count);
			if (bytesRead == 0)
			{
				return -1;
			}
			for (int index = start; index < start + bytesRead; index++)
			{
				target[index] = (byte)charArray[index];
			}
			return bytesRead;
		}

		internal static byte[] ToByteArray(string sourceString)
		{
			return Encoding.UTF8.GetBytes(sourceString);
		}

		internal static char[] ToCharArray(byte[] byteArray)
		{
			return Encoding.UTF8.GetChars(byteArray);
		}
	}
}
