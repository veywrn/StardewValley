using Lidgren.Network;
using System;
using System.IO;

namespace StardewValley.Network
{
	public class NetBufferWriteStream : Stream
	{
		private int offset;

		public NetBuffer Buffer;

		public override bool CanRead => false;

		public override bool CanSeek => true;

		public override bool CanWrite => true;

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override long Position
		{
			get
			{
				return (Buffer.LengthBits - offset) / 8;
			}
			set
			{
				Buffer.LengthBits = (int)(offset + value * 8);
			}
		}

		public NetBufferWriteStream(NetBuffer buffer)
		{
			Buffer = buffer;
			offset = buffer.LengthBits;
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
			case SeekOrigin.Begin:
				Position = offset;
				break;
			case SeekOrigin.Current:
				Position += offset;
				break;
			case SeekOrigin.End:
				throw new NotSupportedException();
			}
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			Buffer.Write(buffer, offset, count);
		}
	}
}
