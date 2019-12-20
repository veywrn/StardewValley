using System;
using System.Globalization;
using System.Text;

namespace StardewValley
{
	public static class StringBuilderFormatEx
	{
		private static readonly char[] MsDigits = new char[16]
		{
			'0',
			'1',
			'2',
			'3',
			'4',
			'5',
			'6',
			'7',
			'8',
			'9',
			'A',
			'B',
			'C',
			'D',
			'E',
			'F'
		};

		private const uint MsDefaultDecimalPlaces = 5u;

		private const char MsDefaultPadChar = '0';

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount, char padChar, uint baseVal)
		{
			uint length = 0u;
			uint lengthCalc = uintVal;
			do
			{
				lengthCalc /= baseVal;
				length++;
			}
			while (lengthCalc != 0);
			stringBuilder.Append(padChar, (int)Math.Max(padAmount, length));
			int strpos = stringBuilder.Length;
			while (length != 0)
			{
				strpos--;
				stringBuilder[strpos] = MsDigits[uintVal % baseVal];
				uintVal /= baseVal;
				length--;
			}
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal)
		{
			stringBuilder.AppendEx(uintVal, 0u, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount)
		{
			stringBuilder.AppendEx(uintVal, padAmount, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount, char padChar)
		{
			stringBuilder.AppendEx(uintVal, padAmount, padChar, 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount, char padChar, uint baseVal)
		{
			if (intVal < 0)
			{
				stringBuilder.Append('-');
				uint uintVal = (uint)(-1 - intVal + 1);
				stringBuilder.AppendEx(uintVal, padAmount, padChar, baseVal);
			}
			else
			{
				stringBuilder.AppendEx((uint)intVal, padAmount, padChar, baseVal);
			}
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal)
		{
			stringBuilder.AppendEx(intVal, 0u, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount)
		{
			stringBuilder.AppendEx(intVal, padAmount, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount, char padChar)
		{
			stringBuilder.AppendEx(intVal, padAmount, padChar, 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces, uint padAmount, char padChar)
		{
			if (decimalPlaces == 0)
			{
				int intVal = (!(floatVal >= 0f)) ? ((int)(floatVal - 0.5f)) : ((int)(floatVal + 0.5f));
				stringBuilder.AppendEx(intVal, padAmount, padChar, 10u);
			}
			else
			{
				int intPart = (int)floatVal;
				stringBuilder.AppendEx(intPart, padAmount, padChar, 10u);
				stringBuilder.Append('.');
				float remainder = Math.Abs(floatVal - (float)intPart);
				for (int i = 0; i < decimalPlaces; i++)
				{
					remainder *= 10f;
				}
				stringBuilder.AppendEx((int)remainder, decimalPlaces, '0', 10u);
			}
			return stringBuilder;
		}

		public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal)
		{
			stringBuilder.AppendEx(floatVal, 5u, 0u, '0');
			return stringBuilder;
		}

		public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces)
		{
			stringBuilder.AppendEx(floatVal, decimalPlaces, 0u, '0');
			return stringBuilder;
		}

		public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces, uint padAmount)
		{
			stringBuilder.AppendEx(floatVal, decimalPlaces, padAmount, '0');
			return stringBuilder;
		}

		public static StringBuilder AppendFormatEx<TA>(this StringBuilder stringBuilder, string formatString, TA arg1) where TA : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, 0, 0, 0);
		}

		public static StringBuilder AppendFormatEx<TA, TB>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2) where TA : IConvertible where TB : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, arg2, 0, 0);
		}

		public static StringBuilder AppendFormatEx<TA, TB, TC>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3) where TA : IConvertible where TB : IConvertible where TC : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, arg2, arg3, 0);
		}

		public static StringBuilder AppendFormatEx<TA, TB, TC, TD>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3, TD arg4) where TA : IConvertible where TB : IConvertible where TC : IConvertible where TD : IConvertible
		{
			int verbatimRangeStart = 0;
			for (int index2 = 0; index2 < formatString.Length; index2++)
			{
				if (formatString[index2] != '{')
				{
					continue;
				}
				if (verbatimRangeStart < index2)
				{
					stringBuilder.Append(formatString, verbatimRangeStart, index2 - verbatimRangeStart);
				}
				uint baseValue = 10u;
				uint padding = 0u;
				uint decimalPlaces = 5u;
				index2++;
				char formatChar = formatString[index2];
				if (formatChar == '{')
				{
					stringBuilder.Append('{');
					index2++;
				}
				else
				{
					index2++;
					if (formatString[index2] == ':')
					{
						index2++;
						while (formatString[index2] == '0')
						{
							index2++;
							padding++;
						}
						if (formatString[index2] == 'X')
						{
							index2++;
							baseValue = 16u;
							if (formatString[index2] >= '0' && formatString[index2] <= '9')
							{
								padding = (uint)(formatString[index2] - 48);
								index2++;
							}
						}
						else if (formatString[index2] == '.')
						{
							index2++;
							decimalPlaces = 0u;
							while (formatString[index2] == '0')
							{
								index2++;
								decimalPlaces++;
							}
						}
					}
					for (; formatString[index2] != '}'; index2++)
					{
					}
					switch (formatChar)
					{
					case '0':
						stringBuilder.AppendFormatValue(arg1, padding, baseValue, decimalPlaces);
						break;
					case '1':
						stringBuilder.AppendFormatValue(arg2, padding, baseValue, decimalPlaces);
						break;
					case '2':
						stringBuilder.AppendFormatValue(arg3, padding, baseValue, decimalPlaces);
						break;
					case '3':
						stringBuilder.AppendFormatValue(arg4, padding, baseValue, decimalPlaces);
						break;
					}
				}
				verbatimRangeStart = index2 + 1;
			}
			if (verbatimRangeStart < formatString.Length)
			{
				stringBuilder.Append(formatString, verbatimRangeStart, formatString.Length - verbatimRangeStart);
			}
			return stringBuilder;
		}

		private static void AppendFormatValue<T>(this StringBuilder stringBuilder, T arg, uint padding, uint baseValue, uint decimalPlaces) where T : IConvertible
		{
			switch (arg.GetTypeCode())
			{
			case TypeCode.UInt32:
				stringBuilder.AppendEx(arg.ToUInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				break;
			case TypeCode.Int32:
				stringBuilder.AppendEx(arg.ToInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				break;
			case TypeCode.Single:
				stringBuilder.AppendEx(arg.ToSingle(NumberFormatInfo.CurrentInfo), decimalPlaces, padding, '0');
				break;
			case TypeCode.String:
				stringBuilder.Append(Convert.ToString(arg));
				break;
			}
		}
	}
}
