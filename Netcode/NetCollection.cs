using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	public sealed class NetCollection<T> : AbstractNetSerializable, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IEquatable<NetCollection<T>> where T : class, INetObject<INetSerializable>
	{
		public delegate void ContentsChangeEvent(T value);

		private List<Guid> guids = new List<Guid>();

		private List<T> list = new List<T>();

		private NetGuidDictionary<T, NetRef<T>> elements = new NetGuidDictionary<T, NetRef<T>>();

		public int Count => list.Count;

		public bool IsFixedSize => false;

		public bool IsReadOnly => false;

		public bool InterpolationWait
		{
			get
			{
				return elements.InterpolationWait;
			}
			set
			{
				elements.InterpolationWait = value;
			}
		}

		public T this[int index]
		{
			get
			{
				return list[index];
			}
			set
			{
				elements[guids[index]] = value;
			}
		}

		public T this[Guid guid] => elements[guid];

		public event ContentsChangeEvent OnValueAdded;

		public event ContentsChangeEvent OnValueRemoved;

		public NetCollection()
		{
			elements.OnValueTargetUpdated += delegate(Guid guid, T old_target_value, T new_target_value)
			{
				if (old_target_value != new_target_value)
				{
					int num3 = guids.IndexOf(guid);
					if (num3 == -1)
					{
						guids.Add(guid);
						list.Add(new_target_value);
					}
					else
					{
						list[num3] = new_target_value;
					}
				}
			};
			elements.OnValueAdded += delegate(Guid guid, T value)
			{
				int num2 = guids.IndexOf(guid);
				if (num2 == -1)
				{
					guids.Add(guid);
					list.Add(value);
				}
				else
				{
					list[num2] = value;
				}
				if (this.OnValueAdded != null)
				{
					this.OnValueAdded(value);
				}
			};
			elements.OnValueRemoved += delegate(Guid guid, T value)
			{
				int num = guids.IndexOf(guid);
				if (num != -1)
				{
					guids.RemoveAt(num);
					list.RemoveAt(num);
				}
				if (this.OnValueRemoved != null)
				{
					this.OnValueRemoved(value);
				}
			};
		}

		public NetCollection(IEnumerable<T> values)
			: this()
		{
			foreach (T value in values)
			{
				Add(value);
			}
		}

		public void Add(T item)
		{
			Guid key = Guid.NewGuid();
			elements.Add(key, item);
		}

		public bool Equals(NetCollection<T> other)
		{
			return elements.Equals(other.elements);
		}

		public List<T>.Enumerator GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Clear()
		{
			elements.Clear();
		}

		public void Set(ICollection<T> other)
		{
			Clear();
			foreach (T elem in other)
			{
				Add(elem);
			}
		}

		public bool Contains(T item)
		{
			return list.Contains(item);
		}

		public bool ContainsGuid(Guid guid)
		{
			return elements.ContainsKey(guid);
		}

		public Guid GuidOf(T item)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == item)
				{
					return guids[i];
				}
			}
			return Guid.Empty;
		}

		public int IndexOf(T item)
		{
			return list.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			throw new NotSupportedException();
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
			using (List<T>.Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					T value = enumerator.Current;
					array[arrayIndex++] = value;
				}
			}
		}

		public bool Remove(T item)
		{
			foreach (Guid key in guids)
			{
				if (elements[key] == item)
				{
					elements.Remove(key);
					return true;
				}
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			elements.Remove(guids[index]);
		}

		public void Remove(Guid guid)
		{
			elements.Remove(guid);
		}

		public void Filter(Func<T, bool> f)
		{
			int i = 0;
			while (i < list.Count)
			{
				T v = list[i];
				if (!f(v))
				{
					elements.Remove(guids[i]);
				}
				else
				{
					i++;
				}
			}
		}

		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			childAction(elements);
		}

		public override void Read(BinaryReader reader, NetVersion version)
		{
			elements.Read(reader, version);
		}

		public override void Write(BinaryWriter writer)
		{
			elements.Write(writer);
		}

		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			elements.ReadFull(reader, version);
		}

		public override void WriteFull(BinaryWriter writer)
		{
			elements.WriteFull(writer);
		}
	}
}
