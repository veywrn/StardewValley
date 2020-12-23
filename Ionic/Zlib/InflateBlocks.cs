using System;

namespace Ionic.Zlib
{
	internal sealed class InflateBlocks
	{
		private enum InflateBlockMode
		{
			TYPE,
			LENS,
			STORED,
			TABLE,
			BTREE,
			DTREE,
			CODES,
			DRY,
			DONE,
			BAD
		}

		private const int MANY = 1440;

		internal static readonly int[] border = new int[19]
		{
			16,
			17,
			18,
			0,
			8,
			7,
			9,
			6,
			10,
			5,
			11,
			4,
			12,
			3,
			13,
			2,
			14,
			1,
			15
		};

		private InflateBlockMode mode;

		internal int left;

		internal int table;

		internal int index;

		internal int[] blens;

		internal int[] bb = new int[1];

		internal int[] tb = new int[1];

		internal InflateCodes codes = new InflateCodes();

		internal int last;

		internal ZlibCodec _codec;

		internal int bitk;

		internal int bitb;

		internal int[] hufts;

		internal byte[] window;

		internal int end;

		internal int readAt;

		internal int writeAt;

		internal object checkfn;

		internal uint check;

		internal InfTree inftree = new InfTree();

		internal InflateBlocks(ZlibCodec codec, object checkfn, int w)
		{
			_codec = codec;
			hufts = new int[4320];
			window = new byte[w];
			end = w;
			this.checkfn = checkfn;
			mode = InflateBlockMode.TYPE;
			Reset();
		}

		internal uint Reset()
		{
			uint result = check;
			mode = InflateBlockMode.TYPE;
			bitk = 0;
			bitb = 0;
			readAt = (writeAt = 0);
			if (checkfn != null)
			{
				_codec._Adler32 = (check = Adler.Adler32(0u, null, 0, 0));
			}
			return result;
		}

		internal int Process(int r)
		{
			int p = _codec.NextIn;
			int n2 = _codec.AvailableBytesIn;
			int b4 = bitb;
			int k2 = bitk;
			int q = writeAt;
			int m2 = (q < readAt) ? (readAt - q - 1) : (end - q);
			while (true)
			{
				switch (mode)
				{
				case InflateBlockMode.TYPE:
				{
					for (; k2 < 3; k2 += 8)
					{
						if (n2 != 0)
						{
							r = 0;
							n2--;
							b4 |= (_codec.InputBuffer[p++] & 0xFF) << k2;
							continue;
						}
						bitb = b4;
						bitk = k2;
						_codec.AvailableBytesIn = n2;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					int t12 = b4 & 7;
					last = (t12 & 1);
					switch ((uint)t12 >> 1)
					{
					case 0u:
						b4 >>= 3;
						k2 -= 3;
						t12 = (k2 & 7);
						b4 >>= t12;
						k2 -= t12;
						mode = InflateBlockMode.LENS;
						break;
					case 1u:
					{
						int[] bl = new int[1];
						int[] bd = new int[1];
						int[][] tl = new int[1][];
						int[][] td = new int[1][];
						InfTree.inflate_trees_fixed(bl, bd, tl, td, _codec);
						codes.Init(bl[0], bd[0], tl[0], 0, td[0], 0);
						b4 >>= 3;
						k2 -= 3;
						mode = InflateBlockMode.CODES;
						break;
					}
					case 2u:
						b4 >>= 3;
						k2 -= 3;
						mode = InflateBlockMode.TABLE;
						break;
					case 3u:
						b4 >>= 3;
						k2 -= 3;
						mode = InflateBlockMode.BAD;
						_codec.Message = "invalid block type";
						r = -3;
						bitb = b4;
						bitk = k2;
						_codec.AvailableBytesIn = n2;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					break;
				}
				case InflateBlockMode.LENS:
					for (; k2 < 32; k2 += 8)
					{
						if (n2 != 0)
						{
							r = 0;
							n2--;
							b4 |= (_codec.InputBuffer[p++] & 0xFF) << k2;
							continue;
						}
						bitb = b4;
						bitk = k2;
						_codec.AvailableBytesIn = n2;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					if (((~b4 >> 16) & 0xFFFF) != (b4 & 0xFFFF))
					{
						mode = InflateBlockMode.BAD;
						_codec.Message = "invalid stored block lengths";
						r = -3;
						bitb = b4;
						bitk = k2;
						_codec.AvailableBytesIn = n2;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					left = (b4 & 0xFFFF);
					b4 = (k2 = 0);
					mode = ((left != 0) ? InflateBlockMode.STORED : ((last != 0) ? InflateBlockMode.DRY : InflateBlockMode.TYPE));
					break;
				case InflateBlockMode.STORED:
				{
					if (n2 == 0)
					{
						bitb = b4;
						bitk = k2;
						_codec.AvailableBytesIn = n2;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					if (m2 == 0)
					{
						if (q == end && readAt != 0)
						{
							q = 0;
							m2 = ((q < readAt) ? (readAt - q - 1) : (end - q));
						}
						if (m2 == 0)
						{
							writeAt = q;
							r = Flush(r);
							q = writeAt;
							m2 = ((q < readAt) ? (readAt - q - 1) : (end - q));
							if (q == end && readAt != 0)
							{
								q = 0;
								m2 = ((q < readAt) ? (readAt - q - 1) : (end - q));
							}
							if (m2 == 0)
							{
								bitb = b4;
								bitk = k2;
								_codec.AvailableBytesIn = n2;
								_codec.TotalBytesIn += p - _codec.NextIn;
								_codec.NextIn = p;
								writeAt = q;
								return Flush(r);
							}
						}
					}
					r = 0;
					int t12 = left;
					if (t12 > n2)
					{
						t12 = n2;
					}
					if (t12 > m2)
					{
						t12 = m2;
					}
					Array.Copy(_codec.InputBuffer, p, window, q, t12);
					p += t12;
					n2 -= t12;
					q += t12;
					m2 -= t12;
					if ((left -= t12) == 0)
					{
						mode = ((last != 0) ? InflateBlockMode.DRY : InflateBlockMode.TYPE);
					}
					break;
				}
				case InflateBlockMode.TABLE:
				{
					for (; k2 < 14; k2 += 8)
					{
						if (n2 != 0)
						{
							r = 0;
							n2--;
							b4 |= (_codec.InputBuffer[p++] & 0xFF) << k2;
							continue;
						}
						bitb = b4;
						bitk = k2;
						_codec.AvailableBytesIn = n2;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					int t12 = table = (b4 & 0x3FFF);
					if ((t12 & 0x1F) > 29 || ((t12 >> 5) & 0x1F) > 29)
					{
						mode = InflateBlockMode.BAD;
						_codec.Message = "too many length or distance symbols";
						r = -3;
						bitb = b4;
						bitk = k2;
						_codec.AvailableBytesIn = n2;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					t12 = 258 + (t12 & 0x1F) + ((t12 >> 5) & 0x1F);
					if (blens == null || blens.Length < t12)
					{
						blens = new int[t12];
					}
					else
					{
						Array.Clear(blens, 0, t12);
					}
					b4 >>= 14;
					k2 -= 14;
					index = 0;
					mode = InflateBlockMode.BTREE;
					goto case InflateBlockMode.BTREE;
				}
				case InflateBlockMode.BTREE:
				{
					while (index < 4 + (table >> 10))
					{
						for (; k2 < 3; k2 += 8)
						{
							if (n2 != 0)
							{
								r = 0;
								n2--;
								b4 |= (_codec.InputBuffer[p++] & 0xFF) << k2;
								continue;
							}
							bitb = b4;
							bitk = k2;
							_codec.AvailableBytesIn = n2;
							_codec.TotalBytesIn += p - _codec.NextIn;
							_codec.NextIn = p;
							writeAt = q;
							return Flush(r);
						}
						blens[border[index++]] = (b4 & 7);
						b4 >>= 3;
						k2 -= 3;
					}
					while (index < 19)
					{
						blens[border[index++]] = 0;
					}
					bb[0] = 7;
					int t12 = inftree.inflate_trees_bits(blens, bb, tb, hufts, _codec);
					if (t12 != 0)
					{
						r = t12;
						if (r == -3)
						{
							blens = null;
							mode = InflateBlockMode.BAD;
						}
						bitb = b4;
						bitk = k2;
						_codec.AvailableBytesIn = n2;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					index = 0;
					mode = InflateBlockMode.DTREE;
					goto case InflateBlockMode.DTREE;
				}
				case InflateBlockMode.DTREE:
				{
					int t12;
					while (true)
					{
						t12 = table;
						if (index >= 258 + (t12 & 0x1F) + ((t12 >> 5) & 0x1F))
						{
							break;
						}
						for (t12 = bb[0]; k2 < t12; k2 += 8)
						{
							if (n2 != 0)
							{
								r = 0;
								n2--;
								b4 |= (_codec.InputBuffer[p++] & 0xFF) << k2;
								continue;
							}
							bitb = b4;
							bitk = k2;
							_codec.AvailableBytesIn = n2;
							_codec.TotalBytesIn += p - _codec.NextIn;
							_codec.NextIn = p;
							writeAt = q;
							return Flush(r);
						}
						t12 = hufts[(tb[0] + (b4 & InternalInflateConstants.InflateMask[t12])) * 3 + 1];
						int c2 = hufts[(tb[0] + (b4 & InternalInflateConstants.InflateMask[t12])) * 3 + 2];
						if (c2 < 16)
						{
							b4 >>= t12;
							k2 -= t12;
							blens[index++] = c2;
							continue;
						}
						int i2 = (c2 == 18) ? 7 : (c2 - 14);
						int j2 = (c2 == 18) ? 11 : 3;
						for (; k2 < t12 + i2; k2 += 8)
						{
							if (n2 != 0)
							{
								r = 0;
								n2--;
								b4 |= (_codec.InputBuffer[p++] & 0xFF) << k2;
								continue;
							}
							bitb = b4;
							bitk = k2;
							_codec.AvailableBytesIn = n2;
							_codec.TotalBytesIn += p - _codec.NextIn;
							_codec.NextIn = p;
							writeAt = q;
							return Flush(r);
						}
						b4 >>= t12;
						k2 -= t12;
						j2 += (b4 & InternalInflateConstants.InflateMask[i2]);
						b4 >>= i2;
						k2 -= i2;
						i2 = index;
						t12 = table;
						if (i2 + j2 > 258 + (t12 & 0x1F) + ((t12 >> 5) & 0x1F) || (c2 == 16 && i2 < 1))
						{
							blens = null;
							mode = InflateBlockMode.BAD;
							_codec.Message = "invalid bit length repeat";
							r = -3;
							bitb = b4;
							bitk = k2;
							_codec.AvailableBytesIn = n2;
							_codec.TotalBytesIn += p - _codec.NextIn;
							_codec.NextIn = p;
							writeAt = q;
							return Flush(r);
						}
						c2 = ((c2 == 16) ? blens[i2 - 1] : 0);
						do
						{
							blens[i2++] = c2;
						}
						while (--j2 != 0);
						index = i2;
					}
					tb[0] = -1;
					int[] bl2 = new int[1]
					{
						9
					};
					int[] bd2 = new int[1]
					{
						6
					};
					int[] tl2 = new int[1];
					int[] td2 = new int[1];
					t12 = table;
					t12 = inftree.inflate_trees_dynamic(257 + (t12 & 0x1F), 1 + ((t12 >> 5) & 0x1F), blens, bl2, bd2, tl2, td2, hufts, _codec);
					if (t12 != 0)
					{
						if (t12 == -3)
						{
							blens = null;
							mode = InflateBlockMode.BAD;
						}
						r = t12;
						bitb = b4;
						bitk = k2;
						_codec.AvailableBytesIn = n2;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					codes.Init(bl2[0], bd2[0], hufts, tl2[0], hufts, td2[0]);
					mode = InflateBlockMode.CODES;
					goto case InflateBlockMode.CODES;
				}
				case InflateBlockMode.CODES:
					bitb = b4;
					bitk = k2;
					_codec.AvailableBytesIn = n2;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					r = codes.Process(this, r);
					if (r != 1)
					{
						return Flush(r);
					}
					r = 0;
					p = _codec.NextIn;
					n2 = _codec.AvailableBytesIn;
					b4 = bitb;
					k2 = bitk;
					q = writeAt;
					m2 = ((q < readAt) ? (readAt - q - 1) : (end - q));
					if (last == 0)
					{
						mode = InflateBlockMode.TYPE;
						break;
					}
					mode = InflateBlockMode.DRY;
					goto case InflateBlockMode.DRY;
				case InflateBlockMode.DRY:
					writeAt = q;
					r = Flush(r);
					q = writeAt;
					m2 = ((q < readAt) ? (readAt - q - 1) : (end - q));
					if (readAt != writeAt)
					{
						bitb = b4;
						bitk = k2;
						_codec.AvailableBytesIn = n2;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					mode = InflateBlockMode.DONE;
					goto case InflateBlockMode.DONE;
				case InflateBlockMode.DONE:
					r = 1;
					bitb = b4;
					bitk = k2;
					_codec.AvailableBytesIn = n2;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				case InflateBlockMode.BAD:
					r = -3;
					bitb = b4;
					bitk = k2;
					_codec.AvailableBytesIn = n2;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				default:
					r = -2;
					bitb = b4;
					bitk = k2;
					_codec.AvailableBytesIn = n2;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
			}
		}

		internal void Free()
		{
			Reset();
			window = null;
			hufts = null;
		}

		internal void SetDictionary(byte[] d, int start, int n)
		{
			Array.Copy(d, start, window, 0, n);
			readAt = (writeAt = n);
		}

		internal int SyncPoint()
		{
			if (mode != InflateBlockMode.LENS)
			{
				return 0;
			}
			return 1;
		}

		internal int Flush(int r)
		{
			for (int pass = 0; pass < 2; pass++)
			{
				int nBytes = (pass != 0) ? (writeAt - readAt) : (((readAt <= writeAt) ? writeAt : end) - readAt);
				if (nBytes == 0)
				{
					if (r == -5)
					{
						r = 0;
					}
					return r;
				}
				if (nBytes > _codec.AvailableBytesOut)
				{
					nBytes = _codec.AvailableBytesOut;
				}
				if (nBytes != 0 && r == -5)
				{
					r = 0;
				}
				_codec.AvailableBytesOut -= nBytes;
				_codec.TotalBytesOut += nBytes;
				if (checkfn != null)
				{
					_codec._Adler32 = (check = Adler.Adler32(check, window, readAt, nBytes));
				}
				Array.Copy(window, readAt, _codec.OutputBuffer, _codec.NextOut, nBytes);
				_codec.NextOut += nBytes;
				readAt += nBytes;
				if (readAt == end && pass == 0)
				{
					readAt = 0;
					if (writeAt == end)
					{
						writeAt = 0;
					}
				}
				else
				{
					pass++;
				}
			}
			return r;
		}
	}
}
