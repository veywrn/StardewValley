using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	public class NetObjectShrinkList<T> : AbstractNetSerializable, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IEquatable<NetObjectShrinkList<T>> where T : class, INetObject<INetSerializable>
	{
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private readonly NetArray<T, NetRef<T>> _array;

			private int _index;

			private T _current;

			private bool _done;

			public T Current => _current;

			object IEnumerator.Current
			{
				get
				{
					if (_done)
					{
						throw new InvalidOperationException();
					}
					return _current;
				}
			}

			public Enumerator(NetArray<T, NetRef<T>> array)
			{
				_array = array;
				_index = 0;
				_current = null;
				_done = false;
			}

			public bool MoveNext()
			{
				while (_index < _array.Count)
				{
					T v = _array[_index];
					_index++;
					if (v != null)
					{
						_current = v;
						return true;
					}
				}
				_done = true;
				_current = null;
				return false;
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				_index = 0;
				_current = null;
				_done = false;
			}
		}

		private NetArray<T, NetRef<T>> array = new NetArray<T, NetRef<T>>();

		public T this[int index]
		{
			get
			{
				int count = 0;
				for (int i = 0; i < array.Count; i++)
				{
					T v = array[i];
					if (v != null)
					{
						if (index == count)
						{
							return v;
						}
						count++;
					}
				}
				throw new ArgumentOutOfRangeException("index");
			}
			set
			{
				int count = 0;
				for (int i = 0; i < array.Count; i++)
				{
					if (array[i] != null)
					{
						if (index == count)
						{
							array[i] = value;
							return;
						}
						count++;
					}
				}
				throw new ArgumentOutOfRangeException("index");
			}
		}

		public int Count
		{
			get
			{
				int count = 0;
				for (int i = 0; i < array.Count; i++)
				{
					if (array[i] != null)
					{
						count++;
					}
				}
				return count;
			}
		}

		public bool IsReadOnly => false;

		public NetObjectShrinkList()
		{
		}

		public NetObjectShrinkList(IEnumerable<T> values)
			: this()
		{
			foreach (T value in values)
			{
				array.Add(value);
			}
		}

		public void Add(T item)
		{
			array.Add(item);
		}

		public void Clear()
		{
			for (int i = 0; i < array.Count; i++)
			{
				array[i] = null;
			}
		}

		public void CopyFrom(IList<T> list)
		{
			if (list == this)
			{
				return;
			}
			if (list.Count > array.Count)
			{
				throw new InvalidOperationException();
			}
			for (int i = 0; i < array.Count; i++)
			{
				if (i < list.Count)
				{
					array[i] = list[i];
				}
				else
				{
					array[i] = null;
				}
			}
		}

		public void Set(IList<T> list)
		{
			CopyFrom(list);
		}

		public void MoveFrom(IList<T> list)
		{
			List<T> values = new List<T>(list);
			list.Clear();
			Set(values);
		}

		public bool Contains(T item)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == item)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (Count - arrayIndex > array.Length)
			{
				throw new ArgumentException();
			}
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					T value = enumerator.Current;
					array[arrayIndex++] = value;
				}
			}
		}

		public List<T> GetRange(int index, int count)
		{
			List<T> result = new List<T>();
			for (int i = index; i < index + count; i++)
			{
				result.Add(this[i]);
			}
			return result;
		}

		public void AddRange(IEnumerable<T> collection)
		{
			foreach (T value in collection)
			{
				Add(value);
			}
		}

		public void RemoveRange(int index, int count)
		{
			for (int i = 0; i < count; i++)
			{
				RemoveAt(index);
			}
		}

		public bool Equals(NetObjectShrinkList<T> other)
		{
			if (Count != other.Count)
			{
				return false;
			}
			for (int i = 0; i < Count; i++)
			{
				if (this[i] != other[i])
				{
					return false;
				}
			}
			return true;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(array);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new Enumerator(array);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(array);
		}

		public int IndexOf(T item)
		{
			int index = 0;
			for (int i = 0; i < array.Count; i++)
			{
				T v = array[i];
				if (v != null)
				{
					if (v == item)
					{
						return index;
					}
					index++;
				}
			}
			return -1;
		}

		public void Insert(int index, T item)
		{
			int count = 0;
			for (int i = 0; i < array.Count; i++)
			{
				if (array[i] != null)
				{
					if (count == index)
					{
						array.Insert(i, item);
						return;
					}
					count++;
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}

		public override void Read(BinaryReader reader, NetVersion version)
		{
			array.Read(reader, version);
		}

		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			array.ReadFull(reader, version);
		}

		public bool Remove(T item)
		{
			for (int i = 0; i < array.Count; i++)
			{
				if (array[i] == item)
				{
					array[i] = null;
					return true;
				}
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			int count = 0;
			int i = 0;
			while (true)
			{
				if (i >= array.Count)
				{
					return;
				}
				if (array[i] != null)
				{
					if (count == index)
					{
						break;
					}
					count++;
				}
				i++;
			}
			array[i] = null;
		}

		public override void Write(BinaryWriter writer)
		{
			array.Write(writer);
		}

		public override void WriteFull(BinaryWriter writer)
		{
			array.WriteFull(writer);
		}

		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			childAction(array);
		}

		public override string ToString()
		{
			return string.Join(",", this);
		}
	}
}
