using System;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	public struct NetVersion : IEquatable<NetVersion>
	{
		private List<uint> _vector;

		private List<uint> vector
		{
			get
			{
				if (_vector == null)
				{
					_vector = new List<uint>();
				}
				return _vector;
			}
		}

		public uint this[int peerId]
		{
			get
			{
				if (peerId >= vector.Count)
				{
					return 0u;
				}
				return vector[peerId];
			}
			set
			{
				while (vector.Count <= peerId)
				{
					vector.Add(0u);
				}
				vector[peerId] = value;
			}
		}

		public NetVersion(NetVersion other)
		{
			_vector = new List<uint>();
			Set(other);
		}

		public NetTimestamp GetTimestamp(int peerId)
		{
			NetTimestamp result = default(NetTimestamp);
			result.PeerId = peerId;
			result.Tick = this[peerId];
			return result;
		}

		public int Size()
		{
			return vector.Count;
		}

		public void Set(NetVersion other)
		{
			for (int i = 0; i < Math.Max(Size(), other.Size()); i++)
			{
				this[i] = other[i];
			}
		}

		public void Merge(NetVersion other)
		{
			for (int i = 0; i < Math.Max(Size(), other.Size()); i++)
			{
				this[i] = Math.Max(this[i], other[i]);
			}
		}

		public bool IsPriorityOver(NetVersion other)
		{
			for (int i = 0; i < Math.Max(Size(), other.Size()); i++)
			{
				if (this[i] > other[i])
				{
					return true;
				}
				if (this[i] < other[i])
				{
					return false;
				}
			}
			return true;
		}

		public bool IsSimultaneousWith(NetVersion other)
		{
			return isOrdered(other, (uint a, uint b) => a == b);
		}

		public bool IsPrecededBy(NetVersion other)
		{
			return isOrdered(other, (uint a, uint b) => a >= b);
		}

		public bool IsFollowedBy(NetVersion other)
		{
			return isOrdered(other, (uint a, uint b) => a < b);
		}

		public bool IsIndependent(NetVersion other)
		{
			if (!IsSimultaneousWith(other) && !IsPrecededBy(other))
			{
				return !IsFollowedBy(other);
			}
			return false;
		}

		private bool isOrdered(NetVersion other, Func<uint, uint, bool> comparison)
		{
			for (int i = 0; i < Math.Max(Size(), other.Size()); i++)
			{
				if (!comparison(this[i], other[i]))
				{
					return false;
				}
			}
			return true;
		}

		public override string ToString()
		{
			if (Size() == 0)
			{
				return "v0";
			}
			return "v" + string.Join(",", vector);
		}

		public bool Equals(NetVersion other)
		{
			for (int i = 0; i < Math.Max(Size(), other.Size()); i++)
			{
				if (this[i] != other[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return vector.GetHashCode() ^ -583558975;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Size());
			for (int i = 0; i < Size(); i++)
			{
				writer.Write(this[i]);
			}
		}

		public void Read(BinaryReader reader)
		{
			int size = reader.ReadByte();
			while (vector.Count > size)
			{
				vector.RemoveAt(size);
			}
			while (vector.Count < size)
			{
				vector.Add(0u);
			}
			for (int j = 0; j < size; j++)
			{
				this[j] = reader.ReadUInt32();
			}
			for (int i = size; i < Size(); i++)
			{
				this[i] = 0u;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < Size(); i++)
			{
				this[i] = 0u;
			}
		}
	}
}
