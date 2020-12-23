using System;
using System.IO;

namespace Ionic.Crc
{
	public class CrcCalculatorStream : Stream, IDisposable
	{
		private static readonly long UnsetLengthLimit = -99L;

		internal Stream _innerStream;

		private CRC32 _Crc32;

		private long _lengthLimit = -99L;

		private bool _leaveOpen;

		public long TotalBytesSlurped => _Crc32.TotalBytesRead;

		public int Crc => _Crc32.Crc32Result;

		public bool LeaveOpen
		{
			get
			{
				return _leaveOpen;
			}
			set
			{
				_leaveOpen = value;
			}
		}

		public override bool CanRead => _innerStream.CanRead;

		public override bool CanSeek => false;

		public override bool CanWrite => _innerStream.CanWrite;

		public override long Length
		{
			get
			{
				if (_lengthLimit == UnsetLengthLimit)
				{
					return _innerStream.Length;
				}
				return _lengthLimit;
			}
		}

		public override long Position
		{
			get
			{
				return _Crc32.TotalBytesRead;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public CrcCalculatorStream(Stream stream)
			: this(leaveOpen: true, UnsetLengthLimit, stream, null)
		{
		}

		public CrcCalculatorStream(Stream stream, bool leaveOpen)
			: this(leaveOpen, UnsetLengthLimit, stream, null)
		{
		}

		public CrcCalculatorStream(Stream stream, long length)
			: this(leaveOpen: true, length, stream, null)
		{
			if (length < 0)
			{
				throw new ArgumentException("length");
			}
		}

		public CrcCalculatorStream(Stream stream, long length, bool leaveOpen)
			: this(leaveOpen, length, stream, null)
		{
			if (length < 0)
			{
				throw new ArgumentException("length");
			}
		}

		public CrcCalculatorStream(Stream stream, long length, bool leaveOpen, CRC32 crc32)
			: this(leaveOpen, length, stream, crc32)
		{
			if (length < 0)
			{
				throw new ArgumentException("length");
			}
		}

		private CrcCalculatorStream(bool leaveOpen, long length, Stream stream, CRC32 crc32)
		{
			_innerStream = stream;
			_Crc32 = (crc32 ?? new CRC32());
			_lengthLimit = length;
			_leaveOpen = leaveOpen;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int bytesToRead = count;
			if (_lengthLimit != UnsetLengthLimit)
			{
				if (_Crc32.TotalBytesRead >= _lengthLimit)
				{
					return 0;
				}
				long bytesRemaining = _lengthLimit - _Crc32.TotalBytesRead;
				if (bytesRemaining < count)
				{
					bytesToRead = (int)bytesRemaining;
				}
			}
			int i = _innerStream.Read(buffer, offset, bytesToRead);
			if (i > 0)
			{
				_Crc32.SlurpBlock(buffer, offset, i);
			}
			return i;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count > 0)
			{
				_Crc32.SlurpBlock(buffer, offset, count);
			}
			_innerStream.Write(buffer, offset, count);
		}

		public override void Flush()
		{
			_innerStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		void IDisposable.Dispose()
		{
			Close();
		}

		public override void Close()
		{
			base.Close();
			if (!_leaveOpen)
			{
				_innerStream.Close();
			}
		}
	}
}
