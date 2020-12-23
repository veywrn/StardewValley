using System;

namespace Ionic.Zlib
{
	internal sealed class InflateCodes
	{
		private const int START = 0;

		private const int LEN = 1;

		private const int LENEXT = 2;

		private const int DIST = 3;

		private const int DISTEXT = 4;

		private const int COPY = 5;

		private const int LIT = 6;

		private const int WASH = 7;

		private const int END = 8;

		private const int BADCODE = 9;

		internal int mode;

		internal int len;

		internal int[] tree;

		internal int tree_index;

		internal int need;

		internal int lit;

		internal int bitsToGet;

		internal int dist;

		internal byte lbits;

		internal byte dbits;

		internal int[] ltree;

		internal int ltree_index;

		internal int[] dtree;

		internal int dtree_index;

		internal InflateCodes()
		{
		}

		internal void Init(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index)
		{
			mode = 0;
			lbits = (byte)bl;
			dbits = (byte)bd;
			ltree = tl;
			ltree_index = tl_index;
			dtree = td;
			dtree_index = td_index;
			tree = null;
		}

		internal int Process(InflateBlocks blocks, int r)
		{
			int b2 = 0;
			int k3 = 0;
			int p2 = 0;
			ZlibCodec z = blocks._codec;
			p2 = z.NextIn;
			int n2 = z.AvailableBytesIn;
			b2 = blocks.bitb;
			k3 = blocks.bitk;
			int q = blocks.writeAt;
			int n = (q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q);
			while (true)
			{
				switch (mode)
				{
				case 0:
					if (n >= 258 && n2 >= 10)
					{
						blocks.bitb = b2;
						blocks.bitk = k3;
						z.AvailableBytesIn = n2;
						z.TotalBytesIn += p2 - z.NextIn;
						z.NextIn = p2;
						blocks.writeAt = q;
						r = InflateFast(lbits, dbits, ltree, ltree_index, dtree, dtree_index, blocks, z);
						p2 = z.NextIn;
						n2 = z.AvailableBytesIn;
						b2 = blocks.bitb;
						k3 = blocks.bitk;
						q = blocks.writeAt;
						n = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
						if (r != 0)
						{
							mode = ((r == 1) ? 7 : 9);
							break;
						}
					}
					need = lbits;
					tree = ltree;
					tree_index = ltree_index;
					mode = 1;
					goto case 1;
				case 1:
				{
					int k;
					for (k = need; k3 < k; k3 += 8)
					{
						if (n2 != 0)
						{
							r = 0;
							n2--;
							b2 |= (z.InputBuffer[p2++] & 0xFF) << k3;
							continue;
						}
						blocks.bitb = b2;
						blocks.bitk = k3;
						z.AvailableBytesIn = n2;
						z.TotalBytesIn += p2 - z.NextIn;
						z.NextIn = p2;
						blocks.writeAt = q;
						return blocks.Flush(r);
					}
					int tindex2 = (tree_index + (b2 & InternalInflateConstants.InflateMask[k])) * 3;
					b2 >>= tree[tindex2 + 1];
					k3 -= tree[tindex2 + 1];
					int e2 = tree[tindex2];
					if (e2 == 0)
					{
						lit = tree[tindex2 + 2];
						mode = 6;
						break;
					}
					if ((e2 & 0x10) != 0)
					{
						bitsToGet = (e2 & 0xF);
						len = tree[tindex2 + 2];
						mode = 2;
						break;
					}
					if ((e2 & 0x40) == 0)
					{
						need = e2;
						tree_index = tindex2 / 3 + tree[tindex2 + 2];
						break;
					}
					if ((e2 & 0x20) != 0)
					{
						mode = 7;
						break;
					}
					mode = 9;
					z.Message = "invalid literal/length code";
					r = -3;
					blocks.bitb = b2;
					blocks.bitk = k3;
					z.AvailableBytesIn = n2;
					z.TotalBytesIn += p2 - z.NextIn;
					z.NextIn = p2;
					blocks.writeAt = q;
					return blocks.Flush(r);
				}
				case 2:
				{
					int k;
					for (k = bitsToGet; k3 < k; k3 += 8)
					{
						if (n2 != 0)
						{
							r = 0;
							n2--;
							b2 |= (z.InputBuffer[p2++] & 0xFF) << k3;
							continue;
						}
						blocks.bitb = b2;
						blocks.bitk = k3;
						z.AvailableBytesIn = n2;
						z.TotalBytesIn += p2 - z.NextIn;
						z.NextIn = p2;
						blocks.writeAt = q;
						return blocks.Flush(r);
					}
					len += (b2 & InternalInflateConstants.InflateMask[k]);
					b2 >>= k;
					k3 -= k;
					need = dbits;
					tree = dtree;
					tree_index = dtree_index;
					mode = 3;
					goto case 3;
				}
				case 3:
				{
					int k;
					for (k = need; k3 < k; k3 += 8)
					{
						if (n2 != 0)
						{
							r = 0;
							n2--;
							b2 |= (z.InputBuffer[p2++] & 0xFF) << k3;
							continue;
						}
						blocks.bitb = b2;
						blocks.bitk = k3;
						z.AvailableBytesIn = n2;
						z.TotalBytesIn += p2 - z.NextIn;
						z.NextIn = p2;
						blocks.writeAt = q;
						return blocks.Flush(r);
					}
					int tindex2 = (tree_index + (b2 & InternalInflateConstants.InflateMask[k])) * 3;
					b2 >>= tree[tindex2 + 1];
					k3 -= tree[tindex2 + 1];
					int e2 = tree[tindex2];
					if ((e2 & 0x10) != 0)
					{
						bitsToGet = (e2 & 0xF);
						dist = tree[tindex2 + 2];
						mode = 4;
						break;
					}
					if ((e2 & 0x40) == 0)
					{
						need = e2;
						tree_index = tindex2 / 3 + tree[tindex2 + 2];
						break;
					}
					mode = 9;
					z.Message = "invalid distance code";
					r = -3;
					blocks.bitb = b2;
					blocks.bitk = k3;
					z.AvailableBytesIn = n2;
					z.TotalBytesIn += p2 - z.NextIn;
					z.NextIn = p2;
					blocks.writeAt = q;
					return blocks.Flush(r);
				}
				case 4:
				{
					int k;
					for (k = bitsToGet; k3 < k; k3 += 8)
					{
						if (n2 != 0)
						{
							r = 0;
							n2--;
							b2 |= (z.InputBuffer[p2++] & 0xFF) << k3;
							continue;
						}
						blocks.bitb = b2;
						blocks.bitk = k3;
						z.AvailableBytesIn = n2;
						z.TotalBytesIn += p2 - z.NextIn;
						z.NextIn = p2;
						blocks.writeAt = q;
						return blocks.Flush(r);
					}
					dist += (b2 & InternalInflateConstants.InflateMask[k]);
					b2 >>= k;
					k3 -= k;
					mode = 5;
					goto case 5;
				}
				case 5:
				{
					int f;
					for (f = q - dist; f < 0; f += blocks.end)
					{
					}
					while (len != 0)
					{
						if (n == 0)
						{
							if (q == blocks.end && blocks.readAt != 0)
							{
								q = 0;
								n = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
							}
							if (n == 0)
							{
								blocks.writeAt = q;
								r = blocks.Flush(r);
								q = blocks.writeAt;
								n = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
								if (q == blocks.end && blocks.readAt != 0)
								{
									q = 0;
									n = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
								}
								if (n == 0)
								{
									blocks.bitb = b2;
									blocks.bitk = k3;
									z.AvailableBytesIn = n2;
									z.TotalBytesIn += p2 - z.NextIn;
									z.NextIn = p2;
									blocks.writeAt = q;
									return blocks.Flush(r);
								}
							}
						}
						blocks.window[q++] = blocks.window[f++];
						n--;
						if (f == blocks.end)
						{
							f = 0;
						}
						len--;
					}
					mode = 0;
					break;
				}
				case 6:
					if (n == 0)
					{
						if (q == blocks.end && blocks.readAt != 0)
						{
							q = 0;
							n = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
						}
						if (n == 0)
						{
							blocks.writeAt = q;
							r = blocks.Flush(r);
							q = blocks.writeAt;
							n = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
							if (q == blocks.end && blocks.readAt != 0)
							{
								q = 0;
								n = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
							}
							if (n == 0)
							{
								blocks.bitb = b2;
								blocks.bitk = k3;
								z.AvailableBytesIn = n2;
								z.TotalBytesIn += p2 - z.NextIn;
								z.NextIn = p2;
								blocks.writeAt = q;
								return blocks.Flush(r);
							}
						}
					}
					r = 0;
					blocks.window[q++] = (byte)lit;
					n--;
					mode = 0;
					break;
				case 7:
					if (k3 > 7)
					{
						k3 -= 8;
						n2++;
						p2--;
					}
					blocks.writeAt = q;
					r = blocks.Flush(r);
					q = blocks.writeAt;
					n = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
					if (blocks.readAt != blocks.writeAt)
					{
						blocks.bitb = b2;
						blocks.bitk = k3;
						z.AvailableBytesIn = n2;
						z.TotalBytesIn += p2 - z.NextIn;
						z.NextIn = p2;
						blocks.writeAt = q;
						return blocks.Flush(r);
					}
					mode = 8;
					goto case 8;
				case 8:
					r = 1;
					blocks.bitb = b2;
					blocks.bitk = k3;
					z.AvailableBytesIn = n2;
					z.TotalBytesIn += p2 - z.NextIn;
					z.NextIn = p2;
					blocks.writeAt = q;
					return blocks.Flush(r);
				case 9:
					r = -3;
					blocks.bitb = b2;
					blocks.bitk = k3;
					z.AvailableBytesIn = n2;
					z.TotalBytesIn += p2 - z.NextIn;
					z.NextIn = p2;
					blocks.writeAt = q;
					return blocks.Flush(r);
				default:
					r = -2;
					blocks.bitb = b2;
					blocks.bitk = k3;
					z.AvailableBytesIn = n2;
					z.TotalBytesIn += p2 - z.NextIn;
					z.NextIn = p2;
					blocks.writeAt = q;
					return blocks.Flush(r);
				}
			}
		}

		internal int InflateFast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InflateBlocks s, ZlibCodec z)
		{
			int p5 = z.NextIn;
			int n3 = z.AvailableBytesIn;
			int b = s.bitb;
			int k4 = s.bitk;
			int q = s.writeAt;
			int m2 = (q < s.readAt) ? (s.readAt - q - 1) : (s.end - q);
			int ml = InternalInflateConstants.InflateMask[bl];
			int md = InternalInflateConstants.InflateMask[bd];
			int c10;
			while (true)
			{
				if (k4 < 20)
				{
					n3--;
					b |= (z.InputBuffer[p5++] & 0xFF) << k4;
					k4 += 8;
					continue;
				}
				int t4 = b & ml;
				int[] tp2 = tl;
				int tp_index2 = tl_index;
				int tp_index_t_4 = (tp_index2 + t4) * 3;
				int e6;
				if ((e6 = tp2[tp_index_t_4]) == 0)
				{
					b >>= tp2[tp_index_t_4 + 1];
					k4 -= tp2[tp_index_t_4 + 1];
					s.window[q++] = (byte)tp2[tp_index_t_4 + 2];
					m2--;
				}
				else
				{
					while (true)
					{
						b >>= tp2[tp_index_t_4 + 1];
						k4 -= tp2[tp_index_t_4 + 1];
						if ((e6 & 0x10) != 0)
						{
							e6 &= 0xF;
							c10 = tp2[tp_index_t_4 + 2] + (b & InternalInflateConstants.InflateMask[e6]);
							b >>= e6;
							for (k4 -= e6; k4 < 15; k4 += 8)
							{
								n3--;
								b |= (z.InputBuffer[p5++] & 0xFF) << k4;
							}
							t4 = (b & md);
							tp2 = td;
							tp_index2 = td_index;
							tp_index_t_4 = (tp_index2 + t4) * 3;
							e6 = tp2[tp_index_t_4];
							while (true)
							{
								b >>= tp2[tp_index_t_4 + 1];
								k4 -= tp2[tp_index_t_4 + 1];
								if ((e6 & 0x10) != 0)
								{
									break;
								}
								if ((e6 & 0x40) == 0)
								{
									t4 += tp2[tp_index_t_4 + 2];
									t4 += (b & InternalInflateConstants.InflateMask[e6]);
									tp_index_t_4 = (tp_index2 + t4) * 3;
									e6 = tp2[tp_index_t_4];
									continue;
								}
								z.Message = "invalid distance code";
								c10 = z.AvailableBytesIn - n3;
								c10 = ((k4 >> 3 < c10) ? (k4 >> 3) : c10);
								n3 += c10;
								p5 -= c10;
								k4 -= c10 << 3;
								s.bitb = b;
								s.bitk = k4;
								z.AvailableBytesIn = n3;
								z.TotalBytesIn += p5 - z.NextIn;
								z.NextIn = p5;
								s.writeAt = q;
								return -3;
							}
							for (e6 &= 0xF; k4 < e6; k4 += 8)
							{
								n3--;
								b |= (z.InputBuffer[p5++] & 0xFF) << k4;
							}
							int d = tp2[tp_index_t_4 + 2] + (b & InternalInflateConstants.InflateMask[e6]);
							b >>= e6;
							k4 -= e6;
							m2 -= c10;
							int r3;
							if (q >= d)
							{
								r3 = q - d;
								if (q - r3 > 0 && 2 > q - r3)
								{
									s.window[q++] = s.window[r3++];
									s.window[q++] = s.window[r3++];
									c10 -= 2;
								}
								else
								{
									Array.Copy(s.window, r3, s.window, q, 2);
									q += 2;
									r3 += 2;
									c10 -= 2;
								}
							}
							else
							{
								r3 = q - d;
								do
								{
									r3 += s.end;
								}
								while (r3 < 0);
								e6 = s.end - r3;
								if (c10 > e6)
								{
									c10 -= e6;
									if (q - r3 > 0 && e6 > q - r3)
									{
										do
										{
											s.window[q++] = s.window[r3++];
										}
										while (--e6 != 0);
									}
									else
									{
										Array.Copy(s.window, r3, s.window, q, e6);
										q += e6;
										r3 += e6;
										e6 = 0;
									}
									r3 = 0;
								}
							}
							if (q - r3 > 0 && c10 > q - r3)
							{
								do
								{
									s.window[q++] = s.window[r3++];
								}
								while (--c10 != 0);
							}
							else
							{
								Array.Copy(s.window, r3, s.window, q, c10);
								q += c10;
								r3 += c10;
								c10 = 0;
							}
							break;
						}
						if ((e6 & 0x40) == 0)
						{
							t4 += tp2[tp_index_t_4 + 2];
							t4 += (b & InternalInflateConstants.InflateMask[e6]);
							tp_index_t_4 = (tp_index2 + t4) * 3;
							if ((e6 = tp2[tp_index_t_4]) == 0)
							{
								b >>= tp2[tp_index_t_4 + 1];
								k4 -= tp2[tp_index_t_4 + 1];
								s.window[q++] = (byte)tp2[tp_index_t_4 + 2];
								m2--;
								break;
							}
							continue;
						}
						if ((e6 & 0x20) != 0)
						{
							c10 = z.AvailableBytesIn - n3;
							c10 = ((k4 >> 3 < c10) ? (k4 >> 3) : c10);
							n3 += c10;
							p5 -= c10;
							k4 -= c10 << 3;
							s.bitb = b;
							s.bitk = k4;
							z.AvailableBytesIn = n3;
							z.TotalBytesIn += p5 - z.NextIn;
							z.NextIn = p5;
							s.writeAt = q;
							return 1;
						}
						z.Message = "invalid literal/length code";
						c10 = z.AvailableBytesIn - n3;
						c10 = ((k4 >> 3 < c10) ? (k4 >> 3) : c10);
						n3 += c10;
						p5 -= c10;
						k4 -= c10 << 3;
						s.bitb = b;
						s.bitk = k4;
						z.AvailableBytesIn = n3;
						z.TotalBytesIn += p5 - z.NextIn;
						z.NextIn = p5;
						s.writeAt = q;
						return -3;
					}
				}
				if (m2 < 258 || n3 < 10)
				{
					break;
				}
			}
			c10 = z.AvailableBytesIn - n3;
			c10 = ((k4 >> 3 < c10) ? (k4 >> 3) : c10);
			n3 += c10;
			p5 -= c10;
			k4 -= c10 << 3;
			s.bitb = b;
			s.bitk = k4;
			z.AvailableBytesIn = n3;
			z.TotalBytesIn += p5 - z.NextIn;
			z.NextIn = p5;
			s.writeAt = q;
			return 0;
		}
	}
}
