using System;

namespace Ionic.Zlib
{
	internal sealed class DeflateManager
	{
		internal delegate BlockState CompressFunc(FlushType flush);

		internal class Config
		{
			internal int GoodLength;

			internal int MaxLazy;

			internal int NiceLength;

			internal int MaxChainLength;

			internal DeflateFlavor Flavor;

			private static readonly Config[] Table;

			private Config(int goodLength, int maxLazy, int niceLength, int maxChainLength, DeflateFlavor flavor)
			{
				GoodLength = goodLength;
				MaxLazy = maxLazy;
				NiceLength = niceLength;
				MaxChainLength = maxChainLength;
				Flavor = flavor;
			}

			public static Config Lookup(CompressionLevel level)
			{
				return Table[(int)level];
			}

			static Config()
			{
				Table = new Config[10]
				{
					new Config(0, 0, 0, 0, DeflateFlavor.Store),
					new Config(4, 4, 8, 4, DeflateFlavor.Fast),
					new Config(4, 5, 16, 8, DeflateFlavor.Fast),
					new Config(4, 6, 32, 32, DeflateFlavor.Fast),
					new Config(4, 4, 16, 16, DeflateFlavor.Slow),
					new Config(8, 16, 32, 32, DeflateFlavor.Slow),
					new Config(8, 16, 128, 128, DeflateFlavor.Slow),
					new Config(8, 32, 128, 256, DeflateFlavor.Slow),
					new Config(32, 128, 258, 1024, DeflateFlavor.Slow),
					new Config(32, 258, 258, 4096, DeflateFlavor.Slow)
				};
			}
		}

		private static readonly int MEM_LEVEL_MAX = 9;

		private static readonly int MEM_LEVEL_DEFAULT = 8;

		private CompressFunc DeflateFunction;

		private static readonly string[] _ErrorMessage = new string[10]
		{
			"need dictionary",
			"stream end",
			"",
			"file error",
			"stream error",
			"data error",
			"insufficient memory",
			"buffer error",
			"incompatible version",
			""
		};

		private static readonly int PRESET_DICT = 32;

		private static readonly int INIT_STATE = 42;

		private static readonly int BUSY_STATE = 113;

		private static readonly int FINISH_STATE = 666;

		private static readonly int Z_DEFLATED = 8;

		private static readonly int STORED_BLOCK = 0;

		private static readonly int STATIC_TREES = 1;

		private static readonly int DYN_TREES = 2;

		private static readonly int Z_BINARY = 0;

		private static readonly int Z_ASCII = 1;

		private static readonly int Z_UNKNOWN = 2;

		private static readonly int Buf_size = 16;

		private static readonly int MIN_MATCH = 3;

		private static readonly int MAX_MATCH = 258;

		private static readonly int MIN_LOOKAHEAD = MAX_MATCH + MIN_MATCH + 1;

		private static readonly int HEAP_SIZE = 2 * InternalConstants.L_CODES + 1;

		private static readonly int END_BLOCK = 256;

		internal ZlibCodec _codec;

		internal int status;

		internal byte[] pending;

		internal int nextPending;

		internal int pendingCount;

		internal sbyte data_type;

		internal int last_flush;

		internal int w_size;

		internal int w_bits;

		internal int w_mask;

		internal byte[] window;

		internal int window_size;

		internal short[] prev;

		internal short[] head;

		internal int ins_h;

		internal int hash_size;

		internal int hash_bits;

		internal int hash_mask;

		internal int hash_shift;

		internal int block_start;

		private Config config;

		internal int match_length;

		internal int prev_match;

		internal int match_available;

		internal int strstart;

		internal int match_start;

		internal int lookahead;

		internal int prev_length;

		internal CompressionLevel compressionLevel;

		internal CompressionStrategy compressionStrategy;

		internal short[] dyn_ltree;

		internal short[] dyn_dtree;

		internal short[] bl_tree;

		internal Tree treeLiterals = new Tree();

		internal Tree treeDistances = new Tree();

		internal Tree treeBitLengths = new Tree();

		internal short[] bl_count = new short[InternalConstants.MAX_BITS + 1];

		internal int[] heap = new int[2 * InternalConstants.L_CODES + 1];

		internal int heap_len;

		internal int heap_max;

		internal sbyte[] depth = new sbyte[2 * InternalConstants.L_CODES + 1];

		internal int _lengthOffset;

		internal int lit_bufsize;

		internal int last_lit;

		internal int _distanceOffset;

		internal int opt_len;

		internal int static_len;

		internal int matches;

		internal int last_eob_len;

		internal short bi_buf;

		internal int bi_valid;

		private bool Rfc1950BytesEmitted;

		private bool _WantRfc1950HeaderBytes = true;

		internal bool WantRfc1950HeaderBytes
		{
			get
			{
				return _WantRfc1950HeaderBytes;
			}
			set
			{
				_WantRfc1950HeaderBytes = value;
			}
		}

		internal DeflateManager()
		{
			dyn_ltree = new short[HEAP_SIZE * 2];
			dyn_dtree = new short[(2 * InternalConstants.D_CODES + 1) * 2];
			bl_tree = new short[(2 * InternalConstants.BL_CODES + 1) * 2];
		}

		private void _InitializeLazyMatch()
		{
			window_size = 2 * w_size;
			Array.Clear(head, 0, hash_size);
			config = Config.Lookup(compressionLevel);
			SetDeflater();
			strstart = 0;
			block_start = 0;
			lookahead = 0;
			match_length = (prev_length = MIN_MATCH - 1);
			match_available = 0;
			ins_h = 0;
		}

		private void _InitializeTreeData()
		{
			treeLiterals.dyn_tree = dyn_ltree;
			treeLiterals.staticTree = StaticTree.Literals;
			treeDistances.dyn_tree = dyn_dtree;
			treeDistances.staticTree = StaticTree.Distances;
			treeBitLengths.dyn_tree = bl_tree;
			treeBitLengths.staticTree = StaticTree.BitLengths;
			bi_buf = 0;
			bi_valid = 0;
			last_eob_len = 8;
			_InitializeBlocks();
		}

		internal void _InitializeBlocks()
		{
			for (int i = 0; i < InternalConstants.L_CODES; i++)
			{
				dyn_ltree[i * 2] = 0;
			}
			for (int j = 0; j < InternalConstants.D_CODES; j++)
			{
				dyn_dtree[j * 2] = 0;
			}
			for (int k = 0; k < InternalConstants.BL_CODES; k++)
			{
				bl_tree[k * 2] = 0;
			}
			dyn_ltree[END_BLOCK * 2] = 1;
			opt_len = (static_len = 0);
			last_lit = (matches = 0);
		}

		internal void pqdownheap(short[] tree, int k)
		{
			int v = heap[k];
			for (int i = k << 1; i <= heap_len; i <<= 1)
			{
				if (i < heap_len && _IsSmaller(tree, heap[i + 1], heap[i], depth))
				{
					i++;
				}
				if (_IsSmaller(tree, v, heap[i], depth))
				{
					break;
				}
				heap[k] = heap[i];
				k = i;
			}
			heap[k] = v;
		}

		internal static bool _IsSmaller(short[] tree, int n, int m, sbyte[] depth)
		{
			short tn2 = tree[n * 2];
			short tm2 = tree[m * 2];
			if (tn2 >= tm2)
			{
				if (tn2 == tm2)
				{
					return depth[n] <= depth[m];
				}
				return false;
			}
			return true;
		}

		internal void scan_tree(short[] tree, int max_code)
		{
			int prevlen = -1;
			int nextlen = tree[1];
			int count = 0;
			int max_count = 7;
			int min_count = 4;
			if (nextlen == 0)
			{
				max_count = 138;
				min_count = 3;
			}
			tree[(max_code + 1) * 2 + 1] = short.MaxValue;
			for (int i = 0; i <= max_code; i++)
			{
				int curlen = nextlen;
				nextlen = tree[(i + 1) * 2 + 1];
				if (++count < max_count && curlen == nextlen)
				{
					continue;
				}
				if (count < min_count)
				{
					bl_tree[curlen * 2] = (short)(bl_tree[curlen * 2] + count);
				}
				else if (curlen != 0)
				{
					if (curlen != prevlen)
					{
						bl_tree[curlen * 2]++;
					}
					bl_tree[InternalConstants.REP_3_6 * 2]++;
				}
				else if (count <= 10)
				{
					bl_tree[InternalConstants.REPZ_3_10 * 2]++;
				}
				else
				{
					bl_tree[InternalConstants.REPZ_11_138 * 2]++;
				}
				count = 0;
				prevlen = curlen;
				if (nextlen == 0)
				{
					max_count = 138;
					min_count = 3;
				}
				else if (curlen == nextlen)
				{
					max_count = 6;
					min_count = 3;
				}
				else
				{
					max_count = 7;
					min_count = 4;
				}
			}
		}

		internal int build_bl_tree()
		{
			scan_tree(dyn_ltree, treeLiterals.max_code);
			scan_tree(dyn_dtree, treeDistances.max_code);
			treeBitLengths.build_tree(this);
			int max_blindex = InternalConstants.BL_CODES - 1;
			while (max_blindex >= 3 && bl_tree[Tree.bl_order[max_blindex] * 2 + 1] == 0)
			{
				max_blindex--;
			}
			opt_len += 3 * (max_blindex + 1) + 5 + 5 + 4;
			return max_blindex;
		}

		internal void send_all_trees(int lcodes, int dcodes, int blcodes)
		{
			send_bits(lcodes - 257, 5);
			send_bits(dcodes - 1, 5);
			send_bits(blcodes - 4, 4);
			for (int rank = 0; rank < blcodes; rank++)
			{
				send_bits(bl_tree[Tree.bl_order[rank] * 2 + 1], 3);
			}
			send_tree(dyn_ltree, lcodes - 1);
			send_tree(dyn_dtree, dcodes - 1);
		}

		internal void send_tree(short[] tree, int max_code)
		{
			int prevlen = -1;
			int nextlen = tree[1];
			int count = 0;
			int max_count = 7;
			int min_count = 4;
			if (nextlen == 0)
			{
				max_count = 138;
				min_count = 3;
			}
			for (int i = 0; i <= max_code; i++)
			{
				int curlen = nextlen;
				nextlen = tree[(i + 1) * 2 + 1];
				if (++count < max_count && curlen == nextlen)
				{
					continue;
				}
				if (count < min_count)
				{
					do
					{
						send_code(curlen, bl_tree);
					}
					while (--count != 0);
				}
				else if (curlen != 0)
				{
					if (curlen != prevlen)
					{
						send_code(curlen, bl_tree);
						count--;
					}
					send_code(InternalConstants.REP_3_6, bl_tree);
					send_bits(count - 3, 2);
				}
				else if (count <= 10)
				{
					send_code(InternalConstants.REPZ_3_10, bl_tree);
					send_bits(count - 3, 3);
				}
				else
				{
					send_code(InternalConstants.REPZ_11_138, bl_tree);
					send_bits(count - 11, 7);
				}
				count = 0;
				prevlen = curlen;
				if (nextlen == 0)
				{
					max_count = 138;
					min_count = 3;
				}
				else if (curlen == nextlen)
				{
					max_count = 6;
					min_count = 3;
				}
				else
				{
					max_count = 7;
					min_count = 4;
				}
			}
		}

		private void put_bytes(byte[] p, int start, int len)
		{
			Array.Copy(p, start, pending, pendingCount, len);
			pendingCount += len;
		}

		internal void send_code(int c, short[] tree)
		{
			int c2 = c * 2;
			send_bits(tree[c2] & 0xFFFF, tree[c2 + 1] & 0xFFFF);
		}

		internal void send_bits(int value, int length)
		{
			if (bi_valid > Buf_size - length)
			{
				bi_buf |= (short)((value << bi_valid) & 0xFFFF);
				pending[pendingCount++] = (byte)bi_buf;
				pending[pendingCount++] = (byte)(bi_buf >> 8);
				bi_buf = (short)((uint)value >> Buf_size - bi_valid);
				bi_valid += length - Buf_size;
			}
			else
			{
				bi_buf |= (short)((value << bi_valid) & 0xFFFF);
				bi_valid += length;
			}
		}

		internal void _tr_align()
		{
			send_bits(STATIC_TREES << 1, 3);
			send_code(END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
			bi_flush();
			if (1 + last_eob_len + 10 - bi_valid < 9)
			{
				send_bits(STATIC_TREES << 1, 3);
				send_code(END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
				bi_flush();
			}
			last_eob_len = 7;
		}

		internal bool _tr_tally(int dist, int lc)
		{
			pending[_distanceOffset + last_lit * 2] = (byte)((uint)dist >> 8);
			pending[_distanceOffset + last_lit * 2 + 1] = (byte)dist;
			pending[_lengthOffset + last_lit] = (byte)lc;
			last_lit++;
			if (dist == 0)
			{
				dyn_ltree[lc * 2]++;
			}
			else
			{
				matches++;
				dist--;
				dyn_ltree[(Tree.LengthCode[lc] + InternalConstants.LITERALS + 1) * 2]++;
				dyn_dtree[Tree.DistanceCode(dist) * 2]++;
			}
			if ((last_lit & 0x1FFF) == 0 && compressionLevel > CompressionLevel.Level2)
			{
				int out_length2 = last_lit << 3;
				int in_length = strstart - block_start;
				for (int dcode = 0; dcode < InternalConstants.D_CODES; dcode++)
				{
					out_length2 = (int)(out_length2 + dyn_dtree[dcode * 2] * (5L + (long)Tree.ExtraDistanceBits[dcode]));
				}
				out_length2 >>= 3;
				if (matches < last_lit / 2 && out_length2 < in_length / 2)
				{
					return true;
				}
			}
			if (last_lit != lit_bufsize - 1)
			{
				return last_lit == lit_bufsize;
			}
			return true;
		}

		internal void send_compressed_block(short[] ltree, short[] dtree)
		{
			int lx = 0;
			if (last_lit != 0)
			{
				do
				{
					int ix = _distanceOffset + lx * 2;
					int distance3 = ((pending[ix] << 8) & 0xFF00) | (pending[ix + 1] & 0xFF);
					int lc2 = pending[_lengthOffset + lx] & 0xFF;
					lx++;
					if (distance3 == 0)
					{
						send_code(lc2, ltree);
						continue;
					}
					int code2 = Tree.LengthCode[lc2];
					send_code(code2 + InternalConstants.LITERALS + 1, ltree);
					int extra2 = Tree.ExtraLengthBits[code2];
					if (extra2 != 0)
					{
						lc2 -= Tree.LengthBase[code2];
						send_bits(lc2, extra2);
					}
					distance3--;
					code2 = Tree.DistanceCode(distance3);
					send_code(code2, dtree);
					extra2 = Tree.ExtraDistanceBits[code2];
					if (extra2 != 0)
					{
						distance3 -= Tree.DistanceBase[code2];
						send_bits(distance3, extra2);
					}
				}
				while (lx < last_lit);
			}
			send_code(END_BLOCK, ltree);
			last_eob_len = ltree[END_BLOCK * 2 + 1];
		}

		internal void set_data_type()
		{
			int i = 0;
			int ascii_freq = 0;
			int bin_freq = 0;
			for (; i < 7; i++)
			{
				bin_freq += dyn_ltree[i * 2];
			}
			for (; i < 128; i++)
			{
				ascii_freq += dyn_ltree[i * 2];
			}
			for (; i < InternalConstants.LITERALS; i++)
			{
				bin_freq += dyn_ltree[i * 2];
			}
			data_type = (sbyte)((bin_freq > ascii_freq >> 2) ? Z_BINARY : Z_ASCII);
		}

		internal void bi_flush()
		{
			if (bi_valid == 16)
			{
				pending[pendingCount++] = (byte)bi_buf;
				pending[pendingCount++] = (byte)(bi_buf >> 8);
				bi_buf = 0;
				bi_valid = 0;
			}
			else if (bi_valid >= 8)
			{
				pending[pendingCount++] = (byte)bi_buf;
				bi_buf >>= 8;
				bi_valid -= 8;
			}
		}

		internal void bi_windup()
		{
			if (bi_valid > 8)
			{
				pending[pendingCount++] = (byte)bi_buf;
				pending[pendingCount++] = (byte)(bi_buf >> 8);
			}
			else if (bi_valid > 0)
			{
				pending[pendingCount++] = (byte)bi_buf;
			}
			bi_buf = 0;
			bi_valid = 0;
		}

		internal void copy_block(int buf, int len, bool header)
		{
			bi_windup();
			last_eob_len = 8;
			if (header)
			{
				pending[pendingCount++] = (byte)len;
				pending[pendingCount++] = (byte)(len >> 8);
				pending[pendingCount++] = (byte)(~len);
				pending[pendingCount++] = (byte)(~len >> 8);
			}
			put_bytes(window, buf, len);
		}

		internal void flush_block_only(bool eof)
		{
			_tr_flush_block((block_start >= 0) ? block_start : (-1), strstart - block_start, eof);
			block_start = strstart;
			_codec.flush_pending();
		}

		internal BlockState DeflateNone(FlushType flush)
		{
			int max_block_size = 65535;
			if (max_block_size > pending.Length - 5)
			{
				max_block_size = pending.Length - 5;
			}
			while (true)
			{
				if (lookahead <= 1)
				{
					_fillWindow();
					if (lookahead == 0 && flush == FlushType.None)
					{
						return BlockState.NeedMore;
					}
					if (lookahead == 0)
					{
						break;
					}
				}
				strstart += lookahead;
				lookahead = 0;
				int max_start = block_start + max_block_size;
				if (strstart == 0 || strstart >= max_start)
				{
					lookahead = strstart - max_start;
					strstart = max_start;
					flush_block_only(eof: false);
					if (_codec.AvailableBytesOut == 0)
					{
						return BlockState.NeedMore;
					}
				}
				if (strstart - block_start >= w_size - MIN_LOOKAHEAD)
				{
					flush_block_only(eof: false);
					if (_codec.AvailableBytesOut == 0)
					{
						return BlockState.NeedMore;
					}
				}
			}
			flush_block_only(flush == FlushType.Finish);
			if (_codec.AvailableBytesOut == 0)
			{
				if (flush != FlushType.Finish)
				{
					return BlockState.NeedMore;
				}
				return BlockState.FinishStarted;
			}
			if (flush != FlushType.Finish)
			{
				return BlockState.BlockDone;
			}
			return BlockState.FinishDone;
		}

		internal void _tr_stored_block(int buf, int stored_len, bool eof)
		{
			send_bits((STORED_BLOCK << 1) + (eof ? 1 : 0), 3);
			copy_block(buf, stored_len, header: true);
		}

		internal void _tr_flush_block(int buf, int stored_len, bool eof)
		{
			int max_blindex = 0;
			int opt_lenb;
			int static_lenb;
			if (compressionLevel > CompressionLevel.None)
			{
				if (data_type == Z_UNKNOWN)
				{
					set_data_type();
				}
				treeLiterals.build_tree(this);
				treeDistances.build_tree(this);
				max_blindex = build_bl_tree();
				opt_lenb = opt_len + 3 + 7 >> 3;
				static_lenb = static_len + 3 + 7 >> 3;
				if (static_lenb <= opt_lenb)
				{
					opt_lenb = static_lenb;
				}
			}
			else
			{
				opt_lenb = (static_lenb = stored_len + 5);
			}
			if (stored_len + 4 <= opt_lenb && buf != -1)
			{
				_tr_stored_block(buf, stored_len, eof);
			}
			else if (static_lenb == opt_lenb)
			{
				send_bits((STATIC_TREES << 1) + (eof ? 1 : 0), 3);
				send_compressed_block(StaticTree.lengthAndLiteralsTreeCodes, StaticTree.distTreeCodes);
			}
			else
			{
				send_bits((DYN_TREES << 1) + (eof ? 1 : 0), 3);
				send_all_trees(treeLiterals.max_code + 1, treeDistances.max_code + 1, max_blindex + 1);
				send_compressed_block(dyn_ltree, dyn_dtree);
			}
			_InitializeBlocks();
			if (eof)
			{
				bi_windup();
			}
		}

		private void _fillWindow()
		{
			do
			{
				int more = window_size - lookahead - strstart;
				int m;
				if (more == 0 && strstart == 0 && lookahead == 0)
				{
					more = w_size;
				}
				else if (more == -1)
				{
					more--;
				}
				else if (strstart >= w_size + w_size - MIN_LOOKAHEAD)
				{
					Array.Copy(window, w_size, window, 0, w_size);
					match_start -= w_size;
					strstart -= w_size;
					block_start -= w_size;
					m = hash_size;
					int p2 = m;
					do
					{
						int l = head[--p2] & 0xFFFF;
						head[p2] = (short)((l >= w_size) ? (l - w_size) : 0);
					}
					while (--m != 0);
					m = w_size;
					p2 = m;
					do
					{
						int l = prev[--p2] & 0xFFFF;
						prev[p2] = (short)((l >= w_size) ? (l - w_size) : 0);
					}
					while (--m != 0);
					more += w_size;
				}
				if (_codec.AvailableBytesIn == 0)
				{
					break;
				}
				m = _codec.read_buf(window, strstart + lookahead, more);
				lookahead += m;
				if (lookahead >= MIN_MATCH)
				{
					ins_h = (window[strstart] & 0xFF);
					ins_h = (((ins_h << hash_shift) ^ (window[strstart + 1] & 0xFF)) & hash_mask);
				}
			}
			while (lookahead < MIN_LOOKAHEAD && _codec.AvailableBytesIn != 0);
		}

		internal BlockState DeflateFast(FlushType flush)
		{
			int hash_head = 0;
			while (true)
			{
				if (lookahead < MIN_LOOKAHEAD)
				{
					_fillWindow();
					if (lookahead < MIN_LOOKAHEAD && flush == FlushType.None)
					{
						return BlockState.NeedMore;
					}
					if (lookahead == 0)
					{
						break;
					}
				}
				if (lookahead >= MIN_MATCH)
				{
					ins_h = (((ins_h << hash_shift) ^ (window[strstart + (MIN_MATCH - 1)] & 0xFF)) & hash_mask);
					hash_head = (head[ins_h] & 0xFFFF);
					prev[strstart & w_mask] = head[ins_h];
					head[ins_h] = (short)strstart;
				}
				if (hash_head != 0L && ((strstart - hash_head) & 0xFFFF) <= w_size - MIN_LOOKAHEAD && compressionStrategy != CompressionStrategy.HuffmanOnly)
				{
					match_length = longest_match(hash_head);
				}
				bool bflush;
				if (match_length >= MIN_MATCH)
				{
					bflush = _tr_tally(strstart - match_start, match_length - MIN_MATCH);
					lookahead -= match_length;
					if (match_length <= config.MaxLazy && lookahead >= MIN_MATCH)
					{
						match_length--;
						do
						{
							strstart++;
							ins_h = (((ins_h << hash_shift) ^ (window[strstart + (MIN_MATCH - 1)] & 0xFF)) & hash_mask);
							hash_head = (head[ins_h] & 0xFFFF);
							prev[strstart & w_mask] = head[ins_h];
							head[ins_h] = (short)strstart;
						}
						while (--match_length != 0);
						strstart++;
					}
					else
					{
						strstart += match_length;
						match_length = 0;
						ins_h = (window[strstart] & 0xFF);
						ins_h = (((ins_h << hash_shift) ^ (window[strstart + 1] & 0xFF)) & hash_mask);
					}
				}
				else
				{
					bflush = _tr_tally(0, window[strstart] & 0xFF);
					lookahead--;
					strstart++;
				}
				if (bflush)
				{
					flush_block_only(eof: false);
					if (_codec.AvailableBytesOut == 0)
					{
						return BlockState.NeedMore;
					}
				}
			}
			flush_block_only(flush == FlushType.Finish);
			if (_codec.AvailableBytesOut == 0)
			{
				if (flush == FlushType.Finish)
				{
					return BlockState.FinishStarted;
				}
				return BlockState.NeedMore;
			}
			if (flush != FlushType.Finish)
			{
				return BlockState.BlockDone;
			}
			return BlockState.FinishDone;
		}

		internal BlockState DeflateSlow(FlushType flush)
		{
			int hash_head = 0;
			while (true)
			{
				if (lookahead < MIN_LOOKAHEAD)
				{
					_fillWindow();
					if (lookahead < MIN_LOOKAHEAD && flush == FlushType.None)
					{
						return BlockState.NeedMore;
					}
					if (lookahead == 0)
					{
						break;
					}
				}
				if (lookahead >= MIN_MATCH)
				{
					ins_h = (((ins_h << hash_shift) ^ (window[strstart + (MIN_MATCH - 1)] & 0xFF)) & hash_mask);
					hash_head = (head[ins_h] & 0xFFFF);
					prev[strstart & w_mask] = head[ins_h];
					head[ins_h] = (short)strstart;
				}
				prev_length = match_length;
				prev_match = match_start;
				match_length = MIN_MATCH - 1;
				if (hash_head != 0 && prev_length < config.MaxLazy && ((strstart - hash_head) & 0xFFFF) <= w_size - MIN_LOOKAHEAD)
				{
					if (compressionStrategy != CompressionStrategy.HuffmanOnly)
					{
						match_length = longest_match(hash_head);
					}
					if (match_length <= 5 && (compressionStrategy == CompressionStrategy.Filtered || (match_length == MIN_MATCH && strstart - match_start > 4096)))
					{
						match_length = MIN_MATCH - 1;
					}
				}
				if (prev_length >= MIN_MATCH && match_length <= prev_length)
				{
					int max_insert = strstart + lookahead - MIN_MATCH;
					bool bflush2 = _tr_tally(strstart - 1 - prev_match, prev_length - MIN_MATCH);
					lookahead -= prev_length - 1;
					prev_length -= 2;
					do
					{
						if (++strstart <= max_insert)
						{
							ins_h = (((ins_h << hash_shift) ^ (window[strstart + (MIN_MATCH - 1)] & 0xFF)) & hash_mask);
							hash_head = (head[ins_h] & 0xFFFF);
							prev[strstart & w_mask] = head[ins_h];
							head[ins_h] = (short)strstart;
						}
					}
					while (--prev_length != 0);
					match_available = 0;
					match_length = MIN_MATCH - 1;
					strstart++;
					if (bflush2)
					{
						flush_block_only(eof: false);
						if (_codec.AvailableBytesOut == 0)
						{
							return BlockState.NeedMore;
						}
					}
				}
				else if (match_available != 0)
				{
					if (_tr_tally(0, window[strstart - 1] & 0xFF))
					{
						flush_block_only(eof: false);
					}
					strstart++;
					lookahead--;
					if (_codec.AvailableBytesOut == 0)
					{
						return BlockState.NeedMore;
					}
				}
				else
				{
					match_available = 1;
					strstart++;
					lookahead--;
				}
			}
			if (match_available != 0)
			{
				bool bflush2 = _tr_tally(0, window[strstart - 1] & 0xFF);
				match_available = 0;
			}
			flush_block_only(flush == FlushType.Finish);
			if (_codec.AvailableBytesOut == 0)
			{
				if (flush == FlushType.Finish)
				{
					return BlockState.FinishStarted;
				}
				return BlockState.NeedMore;
			}
			if (flush != FlushType.Finish)
			{
				return BlockState.BlockDone;
			}
			return BlockState.FinishDone;
		}

		internal int longest_match(int cur_match)
		{
			int chain_length = config.MaxChainLength;
			int scan2 = strstart;
			int best_len = prev_length;
			int limit = (strstart > w_size - MIN_LOOKAHEAD) ? (strstart - (w_size - MIN_LOOKAHEAD)) : 0;
			int niceLength = config.NiceLength;
			int wmask = w_mask;
			int strend = strstart + MAX_MATCH;
			byte scan_end2 = window[scan2 + best_len - 1];
			byte scan_end = window[scan2 + best_len];
			if (prev_length >= config.GoodLength)
			{
				chain_length >>= 2;
			}
			if (niceLength > lookahead)
			{
				niceLength = lookahead;
			}
			do
			{
				int match10 = cur_match;
				if (window[match10 + best_len] != scan_end || window[match10 + best_len - 1] != scan_end2 || window[match10] != window[scan2] || window[++match10] != window[scan2 + 1])
				{
					continue;
				}
				scan2 += 2;
				match10++;
				while (window[++scan2] == window[++match10] && window[++scan2] == window[++match10] && window[++scan2] == window[++match10] && window[++scan2] == window[++match10] && window[++scan2] == window[++match10] && window[++scan2] == window[++match10] && window[++scan2] == window[++match10] && window[++scan2] == window[++match10] && scan2 < strend)
				{
				}
				int len = MAX_MATCH - (strend - scan2);
				scan2 = strend - MAX_MATCH;
				if (len > best_len)
				{
					match_start = cur_match;
					best_len = len;
					if (len >= niceLength)
					{
						break;
					}
					scan_end2 = window[scan2 + best_len - 1];
					scan_end = window[scan2 + best_len];
				}
			}
			while ((cur_match = (prev[cur_match & wmask] & 0xFFFF)) > limit && --chain_length != 0);
			if (best_len <= lookahead)
			{
				return best_len;
			}
			return lookahead;
		}

		internal int Initialize(ZlibCodec codec, CompressionLevel level)
		{
			return Initialize(codec, level, 15);
		}

		internal int Initialize(ZlibCodec codec, CompressionLevel level, int bits)
		{
			return Initialize(codec, level, bits, MEM_LEVEL_DEFAULT, CompressionStrategy.Default);
		}

		internal int Initialize(ZlibCodec codec, CompressionLevel level, int bits, CompressionStrategy compressionStrategy)
		{
			return Initialize(codec, level, bits, MEM_LEVEL_DEFAULT, compressionStrategy);
		}

		internal int Initialize(ZlibCodec codec, CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy)
		{
			_codec = codec;
			_codec.Message = null;
			if (windowBits < 9 || windowBits > 15)
			{
				throw new ZlibException("windowBits must be in the range 9..15.");
			}
			if (memLevel < 1 || memLevel > MEM_LEVEL_MAX)
			{
				throw new ZlibException($"memLevel must be in the range 1.. {MEM_LEVEL_MAX}");
			}
			_codec.dstate = this;
			w_bits = windowBits;
			w_size = 1 << w_bits;
			w_mask = w_size - 1;
			hash_bits = memLevel + 7;
			hash_size = 1 << hash_bits;
			hash_mask = hash_size - 1;
			hash_shift = (hash_bits + MIN_MATCH - 1) / MIN_MATCH;
			window = new byte[w_size * 2];
			prev = new short[w_size];
			head = new short[hash_size];
			lit_bufsize = 1 << memLevel + 6;
			pending = new byte[lit_bufsize * 4];
			_distanceOffset = lit_bufsize;
			_lengthOffset = 3 * lit_bufsize;
			compressionLevel = level;
			compressionStrategy = strategy;
			Reset();
			return 0;
		}

		internal void Reset()
		{
			_codec.TotalBytesIn = (_codec.TotalBytesOut = 0L);
			_codec.Message = null;
			pendingCount = 0;
			nextPending = 0;
			Rfc1950BytesEmitted = false;
			status = (WantRfc1950HeaderBytes ? INIT_STATE : BUSY_STATE);
			_codec._Adler32 = Adler.Adler32(0u, null, 0, 0);
			last_flush = 0;
			_InitializeTreeData();
			_InitializeLazyMatch();
		}

		internal int End()
		{
			if (status != INIT_STATE && status != BUSY_STATE && status != FINISH_STATE)
			{
				return -2;
			}
			pending = null;
			head = null;
			prev = null;
			window = null;
			if (status != BUSY_STATE)
			{
				return 0;
			}
			return -3;
		}

		private void SetDeflater()
		{
			switch (config.Flavor)
			{
			case DeflateFlavor.Store:
				DeflateFunction = DeflateNone;
				break;
			case DeflateFlavor.Fast:
				DeflateFunction = DeflateFast;
				break;
			case DeflateFlavor.Slow:
				DeflateFunction = DeflateSlow;
				break;
			}
		}

		internal int SetParams(CompressionLevel level, CompressionStrategy strategy)
		{
			int result = 0;
			if (compressionLevel != level)
			{
				Config newConfig = Config.Lookup(level);
				if (newConfig.Flavor != config.Flavor && _codec.TotalBytesIn != 0L)
				{
					result = _codec.Deflate(FlushType.Partial);
				}
				compressionLevel = level;
				config = newConfig;
				SetDeflater();
			}
			compressionStrategy = strategy;
			return result;
		}

		internal int SetDictionary(byte[] dictionary)
		{
			int length = dictionary.Length;
			int index = 0;
			if (dictionary == null || status != INIT_STATE)
			{
				throw new ZlibException("Stream error.");
			}
			_codec._Adler32 = Adler.Adler32(_codec._Adler32, dictionary, 0, dictionary.Length);
			if (length < MIN_MATCH)
			{
				return 0;
			}
			if (length > w_size - MIN_LOOKAHEAD)
			{
				length = w_size - MIN_LOOKAHEAD;
				index = dictionary.Length - length;
			}
			Array.Copy(dictionary, index, window, 0, length);
			strstart = length;
			block_start = length;
			ins_h = (window[0] & 0xFF);
			ins_h = (((ins_h << hash_shift) ^ (window[1] & 0xFF)) & hash_mask);
			for (int i = 0; i <= length - MIN_MATCH; i++)
			{
				ins_h = (((ins_h << hash_shift) ^ (window[i + (MIN_MATCH - 1)] & 0xFF)) & hash_mask);
				prev[i & w_mask] = head[ins_h];
				head[ins_h] = (short)i;
			}
			return 0;
		}

		internal int Deflate(FlushType flush)
		{
			if (_codec.OutputBuffer == null || (_codec.InputBuffer == null && _codec.AvailableBytesIn != 0) || (status == FINISH_STATE && flush != FlushType.Finish))
			{
				_codec.Message = _ErrorMessage[4];
				throw new ZlibException($"Something is fishy. [{_codec.Message}]");
			}
			if (_codec.AvailableBytesOut == 0)
			{
				_codec.Message = _ErrorMessage[7];
				throw new ZlibException("OutputBuffer is full (AvailableBytesOut == 0)");
			}
			int old_flush = last_flush;
			last_flush = (int)flush;
			if (status == INIT_STATE)
			{
				int header = Z_DEFLATED + (w_bits - 8 << 4) << 8;
				int level_flags = (int)((compressionLevel - 1) & (CompressionLevel)255) >> 1;
				if (level_flags > 3)
				{
					level_flags = 3;
				}
				header |= level_flags << 6;
				if (strstart != 0)
				{
					header |= PRESET_DICT;
				}
				header += 31 - header % 31;
				status = BUSY_STATE;
				pending[pendingCount++] = (byte)(header >> 8);
				pending[pendingCount++] = (byte)header;
				if (strstart != 0)
				{
					pending[pendingCount++] = (byte)((uint)((int)_codec._Adler32 & -16777216) >> 24);
					pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF0000) >> 16);
					pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF00) >> 8);
					pending[pendingCount++] = (byte)(_codec._Adler32 & 0xFF);
				}
				_codec._Adler32 = Adler.Adler32(0u, null, 0, 0);
			}
			if (pendingCount != 0)
			{
				_codec.flush_pending();
				if (_codec.AvailableBytesOut == 0)
				{
					last_flush = -1;
					return 0;
				}
			}
			else if (_codec.AvailableBytesIn == 0 && (int)flush <= old_flush && flush != FlushType.Finish)
			{
				return 0;
			}
			if (status == FINISH_STATE && _codec.AvailableBytesIn != 0)
			{
				_codec.Message = _ErrorMessage[7];
				throw new ZlibException("status == FINISH_STATE && _codec.AvailableBytesIn != 0");
			}
			if (_codec.AvailableBytesIn != 0 || lookahead != 0 || (flush != 0 && status != FINISH_STATE))
			{
				BlockState bstate = DeflateFunction(flush);
				if (bstate == BlockState.FinishStarted || bstate == BlockState.FinishDone)
				{
					status = FINISH_STATE;
				}
				switch (bstate)
				{
				case BlockState.NeedMore:
				case BlockState.FinishStarted:
					if (_codec.AvailableBytesOut == 0)
					{
						last_flush = -1;
					}
					return 0;
				case BlockState.BlockDone:
					if (flush == FlushType.Partial)
					{
						_tr_align();
					}
					else
					{
						_tr_stored_block(0, 0, eof: false);
						if (flush == FlushType.Full)
						{
							for (int i = 0; i < hash_size; i++)
							{
								head[i] = 0;
							}
						}
					}
					_codec.flush_pending();
					if (_codec.AvailableBytesOut == 0)
					{
						last_flush = -1;
						return 0;
					}
					break;
				}
			}
			if (flush != FlushType.Finish)
			{
				return 0;
			}
			if (!WantRfc1950HeaderBytes || Rfc1950BytesEmitted)
			{
				return 1;
			}
			pending[pendingCount++] = (byte)((uint)((int)_codec._Adler32 & -16777216) >> 24);
			pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF0000) >> 16);
			pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF00) >> 8);
			pending[pendingCount++] = (byte)(_codec._Adler32 & 0xFF);
			_codec.flush_pending();
			Rfc1950BytesEmitted = true;
			if (pendingCount == 0)
			{
				return 1;
			}
			return 0;
		}
	}
}
