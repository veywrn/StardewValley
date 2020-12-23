using Ionic.Crc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ionic.Zlib
{
	internal class ZlibBaseStream : Stream
	{
		internal enum StreamMode
		{
			Writer,
			Reader,
			Undefined
		}

		protected internal ZlibCodec _z;

		protected internal StreamMode _streamMode = StreamMode.Undefined;

		protected internal FlushType _flushMode;

		protected internal ZlibStreamFlavor _flavor;

		protected internal CompressionMode _compressionMode;

		protected internal CompressionLevel _level;

		protected internal bool _leaveOpen;

		protected internal byte[] _workingBuffer;

		protected internal int _bufferSize = 16384;

		protected internal byte[] _buf1 = new byte[1];

		protected internal Stream _stream;

		protected internal CompressionStrategy Strategy;

		private CRC32 crc;

		protected internal string _GzipFileName;

		protected internal string _GzipComment;

		protected internal DateTime _GzipMtime;

		protected internal int _gzipHeaderByteCount;

		private bool nomoreinput;

		internal int Crc32
		{
			get
			{
				if (crc == null)
				{
					return 0;
				}
				return crc.Crc32Result;
			}
		}

		protected internal bool _wantCompress => _compressionMode == CompressionMode.Compress;

		private ZlibCodec z
		{
			get
			{
				if (_z == null)
				{
					bool wantRfc1950Header = _flavor == ZlibStreamFlavor.ZLIB;
					_z = new ZlibCodec();
					if (_compressionMode == CompressionMode.Decompress)
					{
						_z.InitializeInflate(wantRfc1950Header);
					}
					else
					{
						_z.Strategy = Strategy;
						_z.InitializeDeflate(_level, wantRfc1950Header);
					}
				}
				return _z;
			}
		}

		private byte[] workingBuffer
		{
			get
			{
				if (_workingBuffer == null)
				{
					_workingBuffer = new byte[_bufferSize];
				}
				return _workingBuffer;
			}
		}

		public override bool CanRead => _stream.CanRead;

		public override bool CanSeek => _stream.CanSeek;

		public override bool CanWrite => _stream.CanWrite;

		public override long Length => _stream.Length;

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public ZlibBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level, ZlibStreamFlavor flavor, bool leaveOpen)
		{
			_flushMode = FlushType.None;
			_stream = stream;
			_leaveOpen = leaveOpen;
			_compressionMode = compressionMode;
			_flavor = flavor;
			_level = level;
			if (flavor == ZlibStreamFlavor.GZIP)
			{
				crc = new CRC32();
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (crc != null)
			{
				crc.SlurpBlock(buffer, offset, count);
			}
			if (_streamMode == StreamMode.Undefined)
			{
				_streamMode = StreamMode.Writer;
			}
			else if (_streamMode != 0)
			{
				throw new ZlibException("Cannot Write after Reading.");
			}
			if (count == 0)
			{
				return;
			}
			z.InputBuffer = buffer;
			_z.NextIn = offset;
			_z.AvailableBytesIn = count;
			bool done2 = false;
			while (true)
			{
				_z.OutputBuffer = workingBuffer;
				_z.NextOut = 0;
				_z.AvailableBytesOut = _workingBuffer.Length;
				int rc = _wantCompress ? _z.Deflate(_flushMode) : _z.Inflate(_flushMode);
				if (rc != 0 && rc != 1)
				{
					break;
				}
				_stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);
				done2 = (_z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0);
				if (_flavor == ZlibStreamFlavor.GZIP && !_wantCompress)
				{
					done2 = (_z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0);
				}
				if (done2)
				{
					return;
				}
			}
			throw new ZlibException((_wantCompress ? "de" : "in") + "flating: " + _z.Message);
		}

		private void finish()
		{
			if (_z == null)
			{
				return;
			}
			if (_streamMode == StreamMode.Writer)
			{
				bool done2 = false;
				do
				{
					_z.OutputBuffer = workingBuffer;
					_z.NextOut = 0;
					_z.AvailableBytesOut = _workingBuffer.Length;
					int rc = _wantCompress ? _z.Deflate(FlushType.Finish) : _z.Inflate(FlushType.Finish);
					if (rc != 1 && rc != 0)
					{
						string verb = (_wantCompress ? "de" : "in") + "flating";
						if (_z.Message == null)
						{
							throw new ZlibException($"{verb}: (rc = {rc})");
						}
						throw new ZlibException(verb + ": " + _z.Message);
					}
					if (_workingBuffer.Length - _z.AvailableBytesOut > 0)
					{
						_stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);
					}
					done2 = (_z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0);
					if (_flavor == ZlibStreamFlavor.GZIP && !_wantCompress)
					{
						done2 = (_z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0);
					}
				}
				while (!done2);
				Flush();
				if (_flavor == ZlibStreamFlavor.GZIP)
				{
					if (!_wantCompress)
					{
						throw new ZlibException("Writing with decompression is not supported.");
					}
					int c3 = crc.Crc32Result;
					_stream.Write(BitConverter.GetBytes(c3), 0, 4);
					int c2 = (int)(crc.TotalBytesRead & uint.MaxValue);
					_stream.Write(BitConverter.GetBytes(c2), 0, 4);
				}
			}
			else
			{
				if (_streamMode != StreamMode.Reader || _flavor != ZlibStreamFlavor.GZIP)
				{
					return;
				}
				if (_wantCompress)
				{
					throw new ZlibException("Reading with compression is not supported.");
				}
				if (_z.TotalBytesOut == 0L)
				{
					return;
				}
				byte[] trailer = new byte[8];
				if (_z.AvailableBytesIn < 8)
				{
					Array.Copy(_z.InputBuffer, _z.NextIn, trailer, 0, _z.AvailableBytesIn);
					int bytesNeeded = 8 - _z.AvailableBytesIn;
					int bytesRead = _stream.Read(trailer, _z.AvailableBytesIn, bytesNeeded);
					if (bytesNeeded != bytesRead)
					{
						throw new ZlibException($"Missing or incomplete GZIP trailer. Expected 8 bytes, got {_z.AvailableBytesIn + bytesRead}.");
					}
				}
				else
				{
					Array.Copy(_z.InputBuffer, _z.NextIn, trailer, 0, trailer.Length);
				}
				int crc32_expected = BitConverter.ToInt32(trailer, 0);
				int crc32_actual = crc.Crc32Result;
				int isize_expected = BitConverter.ToInt32(trailer, 4);
				int isize_actual = (int)(_z.TotalBytesOut & uint.MaxValue);
				if (crc32_actual != crc32_expected)
				{
					throw new ZlibException($"Bad CRC32 in GZIP trailer. (actual({crc32_actual:X8})!=expected({crc32_expected:X8}))");
				}
				if (isize_actual != isize_expected)
				{
					throw new ZlibException($"Bad size in GZIP trailer. (actual({isize_actual})!=expected({isize_expected}))");
				}
			}
		}

		private void end()
		{
			if (z != null)
			{
				if (_wantCompress)
				{
					_z.EndDeflate();
				}
				else
				{
					_z.EndInflate();
				}
				_z = null;
			}
		}

		public override void Close()
		{
			if (_stream != null)
			{
				try
				{
					finish();
				}
				finally
				{
					end();
					if (!_leaveOpen)
					{
						_stream.Close();
					}
					_stream = null;
				}
			}
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}

		private string ReadZeroTerminatedString()
		{
			List<byte> list = new List<byte>();
			bool done = false;
			do
			{
				if (_stream.Read(_buf1, 0, 1) != 1)
				{
					throw new ZlibException("Unexpected EOF reading GZIP header.");
				}
				if (_buf1[0] == 0)
				{
					done = true;
				}
				else
				{
					list.Add(_buf1[0]);
				}
			}
			while (!done);
			byte[] a = list.ToArray();
			return GZipStream.iso8859dash1.GetString(a, 0, a.Length);
		}

		private int _ReadAndValidateGzipHeader()
		{
			int totalBytesRead3 = 0;
			byte[] header = new byte[10];
			int k = _stream.Read(header, 0, header.Length);
			switch (k)
			{
			case 0:
				return 0;
			default:
				throw new ZlibException("Not a valid GZIP stream.");
			case 10:
			{
				if (header[0] != 31 || header[1] != 139 || header[2] != 8)
				{
					throw new ZlibException("Bad GZIP header.");
				}
				int timet = BitConverter.ToInt32(header, 4);
				_GzipMtime = GZipStream._unixEpoch.AddSeconds(timet);
				totalBytesRead3 += k;
				if ((header[3] & 4) == 4)
				{
					k = _stream.Read(header, 0, 2);
					totalBytesRead3 += k;
					short extraLength = (short)(header[0] + header[1] * 256);
					byte[] extra = new byte[extraLength];
					k = _stream.Read(extra, 0, extra.Length);
					if (k != extraLength)
					{
						throw new ZlibException("Unexpected end-of-file reading GZIP header.");
					}
					totalBytesRead3 += k;
				}
				if ((header[3] & 8) == 8)
				{
					_GzipFileName = ReadZeroTerminatedString();
				}
				if ((header[3] & 0x10) == 16)
				{
					_GzipComment = ReadZeroTerminatedString();
				}
				if ((header[3] & 2) == 2)
				{
					Read(_buf1, 0, 1);
				}
				return totalBytesRead3;
			}
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_streamMode == StreamMode.Undefined)
			{
				if (!_stream.CanRead)
				{
					throw new ZlibException("The stream is not readable.");
				}
				_streamMode = StreamMode.Reader;
				z.AvailableBytesIn = 0;
				if (_flavor == ZlibStreamFlavor.GZIP)
				{
					_gzipHeaderByteCount = _ReadAndValidateGzipHeader();
					if (_gzipHeaderByteCount == 0)
					{
						return 0;
					}
				}
			}
			if (_streamMode != StreamMode.Reader)
			{
				throw new ZlibException("Cannot Read after Writing.");
			}
			if (count == 0)
			{
				return 0;
			}
			if (nomoreinput && _wantCompress)
			{
				return 0;
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (offset < buffer.GetLowerBound(0))
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (offset + count > buffer.GetLength(0))
			{
				throw new ArgumentOutOfRangeException("count");
			}
			int rc2 = 0;
			_z.OutputBuffer = buffer;
			_z.NextOut = offset;
			_z.AvailableBytesOut = count;
			_z.InputBuffer = workingBuffer;
			do
			{
				if (_z.AvailableBytesIn == 0 && !nomoreinput)
				{
					_z.NextIn = 0;
					_z.AvailableBytesIn = _stream.Read(_workingBuffer, 0, _workingBuffer.Length);
					if (_z.AvailableBytesIn == 0)
					{
						nomoreinput = true;
					}
				}
				rc2 = (_wantCompress ? _z.Deflate(_flushMode) : _z.Inflate(_flushMode));
				if (nomoreinput && rc2 == -5)
				{
					return 0;
				}
				if (rc2 != 0 && rc2 != 1)
				{
					throw new ZlibException(string.Format("{0}flating:  rc={1}  msg={2}", _wantCompress ? "de" : "in", rc2, _z.Message));
				}
			}
			while (((!nomoreinput && rc2 != 1) || _z.AvailableBytesOut != count) && _z.AvailableBytesOut > 0 && !nomoreinput && rc2 == 0);
			if (_z.AvailableBytesOut > 0)
			{
				if (rc2 == 0)
				{
					_ = _z.AvailableBytesIn;
				}
				if (nomoreinput && _wantCompress)
				{
					rc2 = _z.Deflate(FlushType.Finish);
					if (rc2 != 0 && rc2 != 1)
					{
						throw new ZlibException($"Deflating:  rc={rc2}  msg={_z.Message}");
					}
				}
			}
			rc2 = count - _z.AvailableBytesOut;
			if (crc != null)
			{
				crc.SlurpBlock(buffer, offset, rc2);
			}
			return rc2;
		}

		public static void CompressString(string s, Stream compressor)
		{
			byte[] uncompressed = Encoding.UTF8.GetBytes(s);
			using (compressor)
			{
				compressor.Write(uncompressed, 0, uncompressed.Length);
			}
		}

		public static void CompressBuffer(byte[] b, Stream compressor)
		{
			using (compressor)
			{
				compressor.Write(b, 0, b.Length);
			}
		}

		public static string UncompressString(byte[] compressed, Stream decompressor)
		{
			byte[] working = new byte[1024];
			Encoding encoding = Encoding.UTF8;
			using (MemoryStream output = new MemoryStream())
			{
				using (decompressor)
				{
					int i;
					while ((i = decompressor.Read(working, 0, working.Length)) != 0)
					{
						output.Write(working, 0, i);
					}
				}
				output.Seek(0L, SeekOrigin.Begin);
				return new StreamReader(output, encoding).ReadToEnd();
			}
		}

		public static byte[] UncompressBuffer(byte[] compressed, Stream decompressor)
		{
			byte[] working = new byte[1024];
			using (MemoryStream output = new MemoryStream())
			{
				using (decompressor)
				{
					int i;
					while ((i = decompressor.Read(working, 0, working.Length)) != 0)
					{
						output.Write(working, 0, i);
					}
				}
				return output.ToArray();
			}
		}
	}
}
