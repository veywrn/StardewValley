using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.IO;

namespace Netcode
{
	public static class BinaryReaderWriterExtensions
	{
		public static void ReadSkippable(this BinaryReader reader, Action readAction)
		{
			uint size = reader.ReadUInt32();
			long startPosition = reader.BaseStream.Position;
			readAction();
			if (reader.BaseStream.Position > startPosition + size)
			{
				throw new InvalidOperationException();
			}
			reader.BaseStream.Position = startPosition + size;
		}

		public static byte[] ReadSkippableBytes(this BinaryReader reader)
		{
			uint dataLength = reader.ReadUInt32();
			return reader.ReadBytes((int)dataLength);
		}

		public static void Skip(this BinaryReader reader)
		{
			reader.ReadSkippable(delegate
			{
			});
		}

		public static void WriteSkippable(this BinaryWriter writer, Action writeAction)
		{
			long sizePosition = writer.BaseStream.Position;
			writer.Write(0u);
			long startPosition = writer.BaseStream.Position;
			writeAction();
			long endPosition = writer.BaseStream.Position;
			long size = endPosition - startPosition;
			writer.BaseStream.Position = sizePosition;
			writer.Write((uint)size);
			writer.BaseStream.Position = endPosition;
		}

		public static BitArray ReadBitArray(this BinaryReader reader)
		{
			int length = (int)reader.Read7BitEncoded();
			return new BitArray(reader.ReadBytes((length + 7) / 8))
			{
				Length = length
			};
		}

		public static void WriteBitArray(this BinaryWriter writer, BitArray bits)
		{
			byte[] buf = new byte[(bits.Length + 7) / 8];
			bits.CopyTo(buf, 0);
			writer.Write7BitEncoded((uint)bits.Length);
			writer.Write(buf);
		}

		public static void Write7BitEncoded(this BinaryWriter writer, uint value)
		{
			do
			{
				byte chunk = (byte)(value & 0x7F);
				value >>= 7;
				if (value != 0)
				{
					chunk = (byte)(chunk | 0x80);
				}
				writer.Write(chunk);
			}
			while (value != 0);
		}

		public static uint Read7BitEncoded(this BinaryReader reader)
		{
			uint value = 0u;
			byte chunk = reader.ReadByte();
			int shift = 0;
			while ((chunk & 0x80) != 0)
			{
				value = (uint)((int)value | ((chunk & 0x7F) << shift));
				shift += 7;
				chunk = reader.ReadByte();
			}
			return (uint)((int)value | ((chunk & 0x7F) << shift));
		}

		public static Guid ReadGuid(this BinaryReader reader)
		{
			return new Guid(reader.ReadBytes(16));
		}

		public static void WriteGuid(this BinaryWriter writer, Guid guid)
		{
			writer.Write(guid.ToByteArray());
		}

		public static Vector2 ReadVector2(this BinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			return new Vector2(x, y);
		}

		public static void WriteVector2(this BinaryWriter writer, Vector2 vec)
		{
			writer.Write(vec.X);
			writer.Write(vec.Y);
		}

		public static Point ReadPoint(this BinaryReader reader)
		{
			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			return new Point(x, y);
		}

		public static void WritePoint(this BinaryWriter writer, Point p)
		{
			writer.Write(p.X);
			writer.Write(p.Y);
		}

		public static Rectangle ReadRectangle(this BinaryReader reader)
		{
			Point pos = reader.ReadPoint();
			Point size = reader.ReadPoint();
			return new Rectangle(pos.X, pos.Y, size.X, size.Y);
		}

		public static void WriteRectangle(this BinaryWriter writer, Rectangle rect)
		{
			writer.WritePoint(rect.Location);
			writer.WritePoint(new Point(rect.Width, rect.Height));
		}

		public static Color ReadColor(this BinaryReader reader)
		{
			Color color = default(Color);
			color.PackedValue = reader.ReadUInt32();
			return color;
		}

		public static void WriteColor(this BinaryWriter writer, Color color)
		{
			writer.Write(color.PackedValue);
		}

		public static T ReadEnum<T>(this BinaryReader reader) where T : struct, IConvertible
		{
			return (T)Enum.ToObject(typeof(T), reader.ReadInt16());
		}

		public static void WriteEnum<T>(this BinaryWriter writer, T enumValue) where T : struct, IConvertible
		{
			writer.Write(Convert.ToInt16(enumValue));
		}

		public static void WriteEnum(this BinaryWriter writer, object enumValue)
		{
			writer.Write(Convert.ToInt16(enumValue));
		}

		public static void Push(this BinaryWriter writer, string name)
		{
			if (writer is ILoggingWriter)
			{
				(writer as ILoggingWriter).Push(name);
			}
		}

		public static void Pop(this BinaryWriter writer)
		{
			if (writer is ILoggingWriter)
			{
				(writer as ILoggingWriter).Pop();
			}
		}
	}
}
