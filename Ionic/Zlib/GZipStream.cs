using System;
using System.IO;
using System.Text;

namespace Ionic.Zlib
{
	public class GZipStream : Stream
	{
		public DateTime? LastModified;

		private int _headerByteCount;

		internal ZlibBaseStream _baseStream;

		private bool _disposed;

		private bool _firstReadDone;

		private string _FileName;

		private string _Comment;

		private int _Crc32;

		internal static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		internal static readonly Encoding iso8859dash1 = Encoding.GetEncoding("iso-8859-1");

		public string Comment
		{
			get
			{
				return _Comment;
			}
			set
			{
				if (_disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				_Comment = value;
			}
		}

		public string FileName
		{
			get
			{
				return _FileName;
			}
			set
			{
				if (_disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				_FileName = value;
				if (_FileName != null)
				{
					if (_FileName.IndexOf("/") != -1)
					{
						_FileName = _FileName.Replace("/", "\\");
					}
					if (_FileName.EndsWith("\\"))
					{
						throw new Exception("Illegal filename");
					}
					if (_FileName.IndexOf("\\") != -1)
					{
						_FileName = Path.GetFileName(_FileName);
					}
				}
			}
		}

		public int Crc32 => _Crc32;

		public virtual FlushType FlushMode
		{
			get
			{
				return _baseStream._flushMode;
			}
			set
			{
				if (_disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				_baseStream._flushMode = value;
			}
		}

		public int BufferSize
		{
			get
			{
				return _baseStream._bufferSize;
			}
			set
			{
				if (_disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				if (_baseStream._workingBuffer != null)
				{
					throw new ZlibException("The working buffer is already set.");
				}
				if (value < 1024)
				{
					throw new ZlibException($"Don't be silly. {value} bytes?? Use a bigger buffer, at least {1024}.");
				}
				_baseStream._bufferSize = value;
			}
		}

		public virtual long TotalIn => _baseStream._z.TotalBytesIn;

		public virtual long TotalOut => _baseStream._z.TotalBytesOut;

		public override bool CanRead
		{
			get
			{
				if (_disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				return _baseStream._stream.CanRead;
			}
		}

		public override bool CanSeek => false;

		public override bool CanWrite
		{
			get
			{
				if (_disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				return _baseStream._stream.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override long Position
		{
			get
			{
				if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
				{
					return _baseStream._z.TotalBytesOut + _headerByteCount;
				}
				if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
				{
					return _baseStream._z.TotalBytesIn + _baseStream._gzipHeaderByteCount;
				}
				return 0L;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public GZipStream(Stream stream, CompressionMode mode)
			: this(stream, mode, CompressionLevel.Default, leaveOpen: false)
		{
		}

		public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level)
			: this(stream, mode, level, leaveOpen: false)
		{
		}

		public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen)
			: this(stream, mode, CompressionLevel.Default, leaveOpen)
		{
		}

		public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
		{
			_baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.GZIP, leaveOpen);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!_disposed)
				{
					if (disposing && _baseStream != null)
					{
						_baseStream.Close();
						_Crc32 = _baseStream.Crc32;
					}
					_disposed = true;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override void Flush()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("GZipStream");
			}
			_baseStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("GZipStream");
			}
			int result = _baseStream.Read(buffer, offset, count);
			if (!_firstReadDone)
			{
				_firstReadDone = true;
				FileName = _baseStream._GzipFileName;
				Comment = _baseStream._GzipComment;
			}
			return result;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("GZipStream");
			}
			if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Undefined)
			{
				if (!_baseStream._wantCompress)
				{
					throw new InvalidOperationException();
				}
				_headerByteCount = EmitHeader();
			}
			_baseStream.Write(buffer, offset, count);
		}

		private int EmitHeader()
		{
			byte[] commentBytes = (Comment == null) ? null : iso8859dash1.GetBytes(Comment);
			byte[] filenameBytes = (FileName == null) ? null : iso8859dash1.GetBytes(FileName);
			int cbLength = (Comment != null) ? (commentBytes.Length + 1) : 0;
			int fnLength = (FileName != null) ? (filenameBytes.Length + 1) : 0;
			byte[] header = new byte[10 + cbLength + fnLength];
			int k = 0;
			header[k++] = 31;
			header[k++] = 139;
			header[k++] = 8;
			byte flag = 0;
			if (Comment != null)
			{
				flag = (byte)(flag ^ 0x10);
			}
			if (FileName != null)
			{
				flag = (byte)(flag ^ 8);
			}
			header[k++] = flag;
			if (!LastModified.HasValue)
			{
				LastModified = DateTime.Now;
			}
			Array.Copy(BitConverter.GetBytes((int)(LastModified.Value - _unixEpoch).TotalSeconds), 0, header, k, 4);
			k += 4;
			header[k++] = 0;
			header[k++] = byte.MaxValue;
			if (fnLength != 0)
			{
				Array.Copy(filenameBytes, 0, header, k, fnLength - 1);
				k += fnLength - 1;
				header[k++] = 0;
			}
			if (cbLength != 0)
			{
				Array.Copy(commentBytes, 0, header, k, cbLength - 1);
				k += cbLength - 1;
				header[k++] = 0;
			}
			_baseStream._stream.Write(header, 0, header.Length);
			return header.Length;
		}

		public static byte[] CompressString(string s)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Stream compressor = new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
				ZlibBaseStream.CompressString(s, compressor);
				return ms.ToArray();
			}
		}

		public static byte[] CompressBuffer(byte[] b)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Stream compressor = new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
				ZlibBaseStream.CompressBuffer(b, compressor);
				return ms.ToArray();
			}
		}

		public static string UncompressString(byte[] compressed)
		{
			using (MemoryStream input = new MemoryStream(compressed))
			{
				Stream decompressor = new GZipStream(input, CompressionMode.Decompress);
				return ZlibBaseStream.UncompressString(compressed, decompressor);
			}
		}

		public static byte[] UncompressBuffer(byte[] compressed)
		{
			using (MemoryStream input = new MemoryStream(compressed))
			{
				Stream decompressor = new GZipStream(input, CompressionMode.Decompress);
				return ZlibBaseStream.UncompressBuffer(compressed, decompressor);
			}
		}
	}
}
