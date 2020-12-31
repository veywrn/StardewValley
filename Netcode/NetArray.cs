using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode
{
	public class NetArray<T, TField> : AbstractNetSerializable, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IEquatable<NetArray<T, TField>> where TField : NetField<T, TField>, new()
	{
		public delegate void FieldCreateEvent(int index, TField field);

		private int appendPosition;

		private readonly List<TField> elements = new List<TField>();

		public List<TField> Fields => elements;

		public T this[int index]
		{
			get
			{
				return elements[index].Get();
			}
			set
			{
				elements[index].Set(value);
			}
		}

		public int Count => elements.Count;

		public int Length => elements.Count;

		public bool IsReadOnly => false;

		public bool IsFixedSize => base.Parent != null;

		public event FieldCreateEvent OnFieldCreate;

		public NetArray()
		{
		}

		public NetArray(IEnumerable<T> values)
			: this()
		{
			int i = 0;
			foreach (T value in values)
			{
				TField field = createField(i++);
				field.Set(value);
				elements.Add(field);
			}
		}

		public NetArray(int size)
			: this()
		{
			for (int i = 0; i < size; i++)
			{
				elements.Add(createField(i));
			}
		}

		private TField createField(int index)
		{
			TField field = new TField().Interpolated(interpolate: false, wait: false);
			if (this.OnFieldCreate != null)
			{
				this.OnFieldCreate(index, field);
			}
			return field;
		}

		public void Add(T item)
		{
			if (IsFixedSize)
			{
				throw new InvalidOperationException();
			}
			while (appendPosition >= elements.Count)
			{
				elements.Add(createField(elements.Count));
			}
			elements[appendPosition].Set(item);
			appendPosition++;
		}

		public void Clear()
		{
			if (IsFixedSize)
			{
				throw new InvalidOperationException();
			}
			elements.Clear();
		}

		public bool Contains(T item)
		{
			foreach (TField element in elements)
			{
				if (object.Equals(element.Get(), item))
				{
					return true;
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
			using (IEnumerator<T> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					T value = enumerator.Current;
					array[arrayIndex++] = value;
				}
			}
		}

		private void ensureCapacity(int size)
		{
			if (IsFixedSize && size != Count)
			{
				throw new InvalidOperationException();
			}
			while (Count < size)
			{
				elements.Add(createField(Count));
			}
		}

		public void SetCount(int size)
		{
			ensureCapacity(size);
		}

		public void Set(IList<T> values)
		{
			ensureCapacity(values.Count);
			for (int i = 0; i < Count; i++)
			{
				this[i] = values[i];
			}
		}

		public void Set(IEnumerable<T> values)
		{
			ensureCapacity(values.Count());
			int i = 0;
			foreach (T value in values)
			{
				this[i++] = value;
			}
		}

		public bool Equals(NetArray<T, TField> other)
		{
			return object.Equals(elements, other.elements);
		}

		public override bool Equals(object obj)
		{
			if (obj is NetArray<T, TField>)
			{
				return Equals(obj as NetArray<T, TField>);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return elements.GetHashCode() ^ 0x300A5A8D;
		}

		public IEnumerator<T> GetEnumerator()
		{
			foreach (TField elementField in elements)
			{
				yield return elementField.Get();
			}
		}

		public int IndexOf(T item)
		{
			for (int i = 0; i < Count; i++)
			{
				if (object.Equals(elements[i].Get(), item))
				{
					return i;
				}
			}
			return -1;
		}

		public void Insert(int index, T item)
		{
			if (IsFixedSize)
			{
				throw new InvalidOperationException();
			}
			TField field = createField(index);
			field.Set(item);
			elements.Insert(index, field);
		}

		public bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index != -1)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			if (IsFixedSize)
			{
				throw new InvalidOperationException();
			}
			elements.RemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override void Read(BinaryReader reader, NetVersion version)
		{
			BitArray dirtyBits = reader.ReadBitArray();
			for (int i = 0; i < elements.Count; i++)
			{
				if (dirtyBits[i])
				{
					elements[i].Read(reader, version);
				}
			}
		}

		public override void Write(BinaryWriter writer)
		{
			BitArray dirtyBits = new BitArray(elements.Count);
			for (int j = 0; j < elements.Count; j++)
			{
				dirtyBits[j] = elements[j].Dirty;
			}
			writer.WriteBitArray(dirtyBits);
			for (int i = 0; i < elements.Count; i++)
			{
				if (dirtyBits[i])
				{
					elements[i].Write(writer);
				}
			}
		}

		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			int size = reader.ReadInt32();
			elements.Clear();
			for (int i = 0; i < size; i++)
			{
				TField element = createField(elements.Count);
				element.ReadFull(reader, version);
				if (base.Parent != null)
				{
					element.Parent = this;
				}
				elements.Add(element);
			}
		}

		public override void WriteFull(BinaryWriter writer)
		{
			writer.Write(Count);
			foreach (TField element in elements)
			{
				element.WriteFull(writer);
			}
		}

		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			foreach (TField elementField in elements)
			{
				childAction(elementField);
			}
		}

		public override string ToString()
		{
			return string.Join(",", this);
		}
	}
}
