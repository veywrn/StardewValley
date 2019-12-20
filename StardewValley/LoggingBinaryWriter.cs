using Netcode;
using System.Collections.Generic;
using System.IO;

namespace StardewValley
{
	public class LoggingBinaryWriter : BinaryWriter, ILoggingWriter
	{
		protected BinaryWriter writer;

		protected List<KeyValuePair<string, long>> stack = new List<KeyValuePair<string, long>>();

		public override Stream BaseStream => writer.BaseStream;

		public LoggingBinaryWriter(BinaryWriter writer)
		{
			this.writer = writer;
		}

		private string currentPath()
		{
			if (stack.Count == 0)
			{
				return "";
			}
			return stack[stack.Count - 1].Key;
		}

		public void Push(string name)
		{
			stack.Add(new KeyValuePair<string, long>(currentPath() + "/" + name, BaseStream.Position));
		}

		public void Pop()
		{
			KeyValuePair<string, long> pair = stack[stack.Count - 1];
			string path = pair.Key;
			long start = pair.Value;
			long length = BaseStream.Position - start;
			stack.RemoveAt(stack.Count - 1);
			Game1.multiplayer.logging.LogWrite(path, length);
		}

		public override void Close()
		{
			base.Close();
			writer.Close();
		}

		public override void Flush()
		{
			writer.Flush();
		}

		public override long Seek(int offset, SeekOrigin origin)
		{
			return writer.Seek(offset, origin);
		}

		public override void Write(short value)
		{
			writer.Write(value);
		}

		public override void Write(ushort value)
		{
			writer.Write(value);
		}

		public override void Write(int value)
		{
			writer.Write(value);
		}

		public override void Write(uint value)
		{
			writer.Write(value);
		}

		public override void Write(long value)
		{
			writer.Write(value);
		}

		public override void Write(ulong value)
		{
			writer.Write(value);
		}

		public override void Write(float value)
		{
			writer.Write(value);
		}

		public override void Write(string value)
		{
			writer.Write(value);
		}

		public override void Write(decimal value)
		{
			writer.Write(value);
		}

		public override void Write(bool value)
		{
			writer.Write(value);
		}

		public override void Write(byte value)
		{
			writer.Write(value);
		}

		public override void Write(sbyte value)
		{
			writer.Write(value);
		}

		public override void Write(byte[] buffer)
		{
			writer.Write(buffer);
		}

		public override void Write(byte[] buffer, int index, int count)
		{
			writer.Write(buffer, index, count);
		}

		public override void Write(char ch)
		{
			writer.Write(ch);
		}

		public override void Write(char[] chars)
		{
			writer.Write(chars);
		}

		public override void Write(char[] chars, int index, int count)
		{
			writer.Write(chars, index, count);
		}

		public override void Write(double value)
		{
			writer.Write(value);
		}
	}
}
