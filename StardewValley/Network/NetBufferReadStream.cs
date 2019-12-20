using Lidgren.Network;
using System;
using System.IO;

namespace StardewValley.Network
{
	public class NetBufferReadStream : Stream
	{
		private long offset;

		public NetBuffer Buffer;

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => false;

		public override long Length => (Buffer.LengthBits - offset) / 8;

		public override long Position
		{
			get
			{
				return (Buffer.Position - offset) / 8;
			}
			set
			{
				Buffer.Position = offset + value * 8;
			}
		}

		public NetBufferReadStream(NetBuffer buffer)
		{
			Buffer = buffer;
			offset = buffer.Position;
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			Buffer.ReadBytes(buffer, offset, count);
			return count;
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
				Position = Length + offset;
				break;
			}
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}
}
