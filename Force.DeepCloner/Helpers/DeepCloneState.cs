using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Force.DeepCloner.Helpers
{
	internal class DeepCloneState
	{
		private class CustomEqualityComparer : IEqualityComparer<object>, IEqualityComparer
		{
			bool IEqualityComparer<object>.Equals(object x, object y)
			{
				return x == y;
			}

			bool IEqualityComparer.Equals(object x, object y)
			{
				return x == y;
			}

			public int GetHashCode(object obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
		}

		private class MiniDictionary
		{
			private struct Entry
			{
				public int HashCode;

				public int Next;

				public object Key;

				public object Value;
			}

			private int[] _buckets;

			private Entry[] _entries;

			private int _count;

			private static readonly int[] _primes = new int[72]
			{
				3,
				7,
				11,
				17,
				23,
				29,
				37,
				47,
				59,
				71,
				89,
				107,
				131,
				163,
				197,
				239,
				293,
				353,
				431,
				521,
				631,
				761,
				919,
				1103,
				1327,
				1597,
				1931,
				2333,
				2801,
				3371,
				4049,
				4861,
				5839,
				7013,
				8419,
				10103,
				12143,
				14591,
				17519,
				21023,
				25229,
				30293,
				36353,
				43627,
				52361,
				62851,
				75431,
				90523,
				108631,
				130363,
				156437,
				187751,
				225307,
				270371,
				324449,
				389357,
				467237,
				560689,
				672827,
				807403,
				968897,
				1162687,
				1395263,
				1674319,
				2009191,
				2411033,
				2893249,
				3471899,
				4166287,
				4999559,
				5999471,
				7199369
			};

			public MiniDictionary()
				: this(5)
			{
			}

			public MiniDictionary(int capacity)
			{
				if (capacity > 0)
				{
					Initialize(capacity);
				}
			}

			public object FindEntry(object key)
			{
				if (_buckets != null)
				{
					int hashCode = RuntimeHelpers.GetHashCode(key) & int.MaxValue;
					Entry[] entries = _entries;
					for (int i = _buckets[hashCode % _buckets.Length]; i >= 0; i = entries[i].Next)
					{
						if (entries[i].HashCode == hashCode && entries[i].Key == key)
						{
							return entries[i].Value;
						}
					}
				}
				return null;
			}

			private static int GetPrime(int min)
			{
				for (int j = 0; j < _primes.Length; j++)
				{
					int prime = _primes[j];
					if (prime >= min)
					{
						return prime;
					}
				}
				for (int i = min | 1; i < int.MaxValue; i += 2)
				{
					if (IsPrime(i) && (i - 1) % 101 != 0)
					{
						return i;
					}
				}
				return min;
			}

			private static bool IsPrime(int candidate)
			{
				if ((candidate & 1) != 0)
				{
					int limit = (int)Math.Sqrt(candidate);
					for (int divisor = 3; divisor <= limit; divisor += 2)
					{
						if (candidate % divisor == 0)
						{
							return false;
						}
					}
					return true;
				}
				return candidate == 2;
			}

			private static int ExpandPrime(int oldSize)
			{
				int newSize = 2 * oldSize;
				if ((uint)newSize > 2146435069u && 2146435069 > oldSize)
				{
					return 2146435069;
				}
				return GetPrime(newSize);
			}

			private void Initialize(int size)
			{
				_buckets = new int[size];
				for (int i = 0; i < _buckets.Length; i++)
				{
					_buckets[i] = -1;
				}
				_entries = new Entry[size];
			}

			public void Insert(object key, object value)
			{
				if (_buckets == null)
				{
					Initialize(0);
				}
				int hashCode = RuntimeHelpers.GetHashCode(key) & int.MaxValue;
				int targetBucket = hashCode % _buckets.Length;
				Entry[] entries = _entries;
				if (_count == entries.Length)
				{
					Resize();
					entries = _entries;
					targetBucket = hashCode % _buckets.Length;
				}
				int index = _count;
				_count++;
				entries[index].HashCode = hashCode;
				entries[index].Next = _buckets[targetBucket];
				entries[index].Key = key;
				entries[index].Value = value;
				_buckets[targetBucket] = index;
			}

			private void Resize()
			{
				Resize(ExpandPrime(_count));
			}

			private void Resize(int newSize)
			{
				int[] newBuckets = new int[newSize];
				for (int j = 0; j < newBuckets.Length; j++)
				{
					newBuckets[j] = -1;
				}
				Entry[] newEntries = new Entry[newSize];
				Array.Copy(_entries, 0, newEntries, 0, _count);
				for (int i = 0; i < _count; i++)
				{
					if (newEntries[i].HashCode >= 0)
					{
						int bucket = newEntries[i].HashCode % newSize;
						newEntries[i].Next = newBuckets[bucket];
						newBuckets[bucket] = i;
					}
				}
				_buckets = newBuckets;
				_entries = newEntries;
			}
		}

		private MiniDictionary _loops;

		private readonly object[] _baseFromTo = new object[6];

		private int _idx;

		public object GetKnownRef(object from)
		{
			object[] baseFromTo = _baseFromTo;
			if (from == baseFromTo[0])
			{
				return baseFromTo[3];
			}
			if (from == baseFromTo[1])
			{
				return baseFromTo[4];
			}
			if (from == baseFromTo[2])
			{
				return baseFromTo[5];
			}
			if (_loops == null)
			{
				return null;
			}
			return _loops.FindEntry(from);
		}

		public void AddKnownRef(object from, object to)
		{
			if (_idx < 3)
			{
				_baseFromTo[_idx] = from;
				_baseFromTo[_idx + 3] = to;
				_idx++;
				return;
			}
			if (_loops == null)
			{
				_loops = new MiniDictionary();
			}
			_loops.Insert(from, to);
		}
	}
}
