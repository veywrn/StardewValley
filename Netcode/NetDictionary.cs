using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode
{
	public abstract class NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> : AbstractNetSerializable, IEquatable<TSelf>, IEnumerable<TSerialDict>, IEnumerable where TField : class, INetObject<INetSerializable>, new()where TSerialDict : IDictionary<TKey, TValue>, new()where TSelf : NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>
	{
		private class IncomingChange
		{
			public uint Tick;

			public bool Removal;

			public TKey Key;

			public TField Field;

			public NetVersion Reassigned;

			public IncomingChange(uint tick, bool removal, TKey key, TField field, NetVersion reassigned)
			{
				Tick = tick;
				Removal = removal;
				Key = key;
				Field = field;
				Reassigned = reassigned;
			}
		}

		private class OutgoingChange
		{
			public bool Removal;

			public TKey Key;

			public TField Field;

			public NetVersion Reassigned;

			public OutgoingChange(bool removal, TKey key, TField field, NetVersion reassigned)
			{
				Removal = removal;
				Key = key;
				Field = field;
				Reassigned = reassigned;
			}
		}

		public delegate void ContentsChangeEvent(TKey key, TValue value);

		public delegate void ConflictResolveEvent(TKey key, TField rejected, TField accepted);

		public delegate void ContentsUpdateEvent(TKey key, TValue old_target_value, TValue new_target_value);

		private delegate void ReadFunc(BinaryReader reader, NetVersion version);

		private delegate void WriteFunc<T>(BinaryWriter writer, T value);

		public struct PairsCollection : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
		{
			public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
			{
				private readonly NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

				private Dictionary<TKey, TField>.Enumerator _enumerator;

				private KeyValuePair<TKey, TValue> _current;

				private bool _done;

				public KeyValuePair<TKey, TValue> Current => _current;

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

				public Enumerator(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
				{
					_net = net;
					_enumerator = _net.dict.GetEnumerator();
					_current = default(KeyValuePair<TKey, TValue>);
					_done = false;
				}

				public bool MoveNext()
				{
					if (_enumerator.MoveNext())
					{
						KeyValuePair<TKey, TField> pair = _enumerator.Current;
						_current = new KeyValuePair<TKey, TValue>(pair.Key, _net.getFieldValue(pair.Value));
						return true;
					}
					_done = true;
					_current = default(KeyValuePair<TKey, TValue>);
					return false;
				}

				public void Dispose()
				{
				}

				void IEnumerator.Reset()
				{
					_enumerator = _net.dict.GetEnumerator();
					_current = default(KeyValuePair<TKey, TValue>);
					_done = false;
				}
			}

			private NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

			public PairsCollection(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
			{
				_net = net;
			}

			public int Count()
			{
				return _net.dict.Count;
			}

			public KeyValuePair<TKey, TValue> ElementAt(int index)
			{
				int count = 0;
				using (Enumerator enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<TKey, TValue> pair = enumerator.Current;
						if (count == index)
						{
							return pair;
						}
						count++;
					}
				}
				throw new ArgumentOutOfRangeException();
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_net);
			}

			IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
			{
				return new Enumerator(_net);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(_net);
			}
		}

		public struct ValuesCollection : IEnumerable<TValue>, IEnumerable
		{
			public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
			{
				private readonly NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

				private Dictionary<TKey, TField>.Enumerator _enumerator;

				private TValue _current;

				private bool _done;

				public TValue Current => _current;

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

				public Enumerator(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
				{
					_net = net;
					_enumerator = _net.dict.GetEnumerator();
					_current = default(TValue);
					_done = false;
				}

				public bool MoveNext()
				{
					if (_enumerator.MoveNext())
					{
						KeyValuePair<TKey, TField> pair = _enumerator.Current;
						_current = _net.getFieldValue(pair.Value);
						return true;
					}
					_done = true;
					_current = default(TValue);
					return false;
				}

				public void Dispose()
				{
				}

				void IEnumerator.Reset()
				{
					_enumerator = _net.dict.GetEnumerator();
					_current = default(TValue);
					_done = false;
				}
			}

			private NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

			public ValuesCollection(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
			{
				_net = net;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_net);
			}

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
			{
				return new Enumerator(_net);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(_net);
			}
		}

		public struct KeysCollection : IEnumerable<TKey>, IEnumerable
		{
			public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
			{
				private readonly Dictionary<TKey, TField> _dict;

				private Dictionary<TKey, TField>.Enumerator _enumerator;

				private TKey _current;

				private bool _done;

				public TKey Current => _current;

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

				public Enumerator(Dictionary<TKey, TField> dict)
				{
					_dict = dict;
					_enumerator = _dict.GetEnumerator();
					_current = default(TKey);
					_done = false;
				}

				public bool MoveNext()
				{
					if (_enumerator.MoveNext())
					{
						_current = _enumerator.Current.Key;
						return true;
					}
					_done = true;
					_current = default(TKey);
					return false;
				}

				public void Dispose()
				{
				}

				void IEnumerator.Reset()
				{
					_enumerator = _dict.GetEnumerator();
					_current = default(TKey);
					_done = false;
				}
			}

			private Dictionary<TKey, TField> _dict;

			public KeysCollection(Dictionary<TKey, TField> dict)
			{
				_dict = dict;
			}

			public bool Any()
			{
				return _dict.Count > 0;
			}

			public TKey First()
			{
				using (Dictionary<TKey, TField>.Enumerator enumerator = _dict.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						return enumerator.Current.Key;
					}
				}
				return default(TKey);
			}

			public bool Contains(TKey key)
			{
				return _dict.ContainsKey(key);
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_dict);
			}

			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
			{
				return new Enumerator(_dict);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(_dict);
			}
		}

		public bool InterpolationWait = true;

		private Dictionary<TKey, TField> dict = new Dictionary<TKey, TField>();

		private Dictionary<TKey, NetVersion> dictReassigns = new Dictionary<TKey, NetVersion>();

		private List<OutgoingChange> outgoingChanges = new List<OutgoingChange>();

		private List<IncomingChange> incomingChanges = new List<IncomingChange>();

		public bool IsReadOnly => false;

		public TValue this[TKey key]
		{
			get
			{
				return getFieldValue(dict[key]);
			}
			set
			{
				if (!dict.ContainsKey(key))
				{
					dict[key] = new TField();
					dictReassigns[key] = GetLocalVersion();
					setFieldValue(dict[key], key, value);
					added(key, dict[key], dictReassigns[key]);
				}
				else
				{
					setFieldValue(dict[key], key, value);
					addedEvent(key, dict[key]);
				}
			}
		}

		public KeysCollection Keys => new KeysCollection(dict);

		public ValuesCollection Values => new ValuesCollection(this);

		public PairsCollection Pairs => new PairsCollection(this);

		public Dictionary<TKey, TField> FieldDict => dict;

		public event ContentsChangeEvent OnValueAdded;

		public event ContentsChangeEvent OnValueRemoved;

		public event ContentsUpdateEvent OnValueTargetUpdated;

		public event ConflictResolveEvent OnConflictResolve;

		public NetDictionary()
		{
		}

		public NetDictionary(IEnumerable<KeyValuePair<TKey, TValue>> dict)
			: this()
		{
			CopyFrom(dict);
		}

		protected override bool tickImpl()
		{
			List<IncomingChange> triggeredChanges = null;
			foreach (IncomingChange ch2 in incomingChanges)
			{
				if (base.Root != null && GetLocalTick() < ch2.Tick)
				{
					break;
				}
				if (triggeredChanges == null)
				{
					triggeredChanges = new List<IncomingChange>();
				}
				triggeredChanges.Add(ch2);
			}
			if (triggeredChanges != null && triggeredChanges.Count > 0)
			{
				foreach (IncomingChange c in triggeredChanges)
				{
					incomingChanges.Remove(c);
				}
				foreach (IncomingChange ch in triggeredChanges)
				{
					if (ch.Removal)
					{
						performIncomingRemove(ch);
					}
					else
					{
						performIncomingAdd(ch);
					}
				}
			}
			return incomingChanges.Count > 0;
		}

		protected abstract void setFieldValue(TField field, TKey key, TValue value);

		protected abstract TValue getFieldValue(TField field);

		protected abstract TValue getFieldTargetValue(TField field);

		protected TField createField(TKey key, TValue value)
		{
			TField field = new TField();
			setFieldValue(field, key, value);
			return field;
		}

		public void CopyFrom(IEnumerable<KeyValuePair<TKey, TValue>> dict)
		{
			foreach (KeyValuePair<TKey, TValue> pair in dict)
			{
				this[pair.Key] = pair.Value;
			}
		}

		public void Set(IEnumerable<KeyValuePair<TKey, TValue>> dict)
		{
			Clear();
			CopyFrom(dict);
		}

		public void MoveFrom(TSelf dict)
		{
			List<KeyValuePair<TKey, TValue>> pairs = new List<KeyValuePair<TKey, TValue>>(dict.Pairs);
			dict.Clear();
			Set(pairs);
		}

		public void SetEqualityComparer(IEqualityComparer<TKey> comparer)
		{
			dict = new Dictionary<TKey, TField>(dict, comparer);
			dictReassigns = new Dictionary<TKey, NetVersion>(dictReassigns, comparer);
		}

		private void setFieldParent(TField arg)
		{
			if (base.Parent != null)
			{
				arg.NetFields.Parent = this;
			}
		}

		private void added(TKey key, TField field, NetVersion reassign)
		{
			outgoingChanges.Add(new OutgoingChange(removal: false, key, field, reassign));
			setFieldParent(field);
			MarkDirty();
			addedEvent(key, field);
			foreach (IncomingChange change2 in incomingChanges)
			{
				if (!change2.Removal && object.Equals(change2.Key, key))
				{
					clearFieldParent(change2.Field);
					if (this.OnConflictResolve != null)
					{
						this.OnConflictResolve(key, change2.Field, field);
					}
				}
			}
			incomingChanges.RemoveAll((IncomingChange change) => object.Equals(key, change.Key));
		}

		private void addedEvent(TKey key, TField field)
		{
			if (this.OnValueAdded != null)
			{
				this.OnValueAdded(key, getFieldValue(field));
			}
		}

		private void updatedEvent(TKey key, TValue old_target_value, TValue new_target_value)
		{
			if (this.OnValueTargetUpdated != null)
			{
				this.OnValueTargetUpdated(key, old_target_value, new_target_value);
			}
		}

		private void clearFieldParent(TField arg)
		{
			if (arg.NetFields.Parent == this)
			{
				arg.NetFields.Parent = null;
			}
		}

		private void removed(TKey key, TField field, NetVersion reassign)
		{
			outgoingChanges.Add(new OutgoingChange(removal: true, key, field, reassign));
			clearFieldParent(field);
			MarkDirty();
			removedEvent(key, field);
		}

		private void removedEvent(TKey key, TField field)
		{
			if (this.OnValueRemoved != null)
			{
				this.OnValueRemoved(key, getFieldValue(field));
			}
		}

		public void Add(TKey key, TValue value)
		{
			TField field = createField(key, value);
			Add(key, field);
		}

		public void Add(TKey key, TField field)
		{
			dict.Add(key, field);
			dictReassigns.Add(key, GetLocalVersion());
			added(key, field, dictReassigns[key]);
		}

		public void Clear()
		{
			KeysCollection keys = Keys;
			while (keys.Any())
			{
				Remove(keys.First());
			}
			outgoingChanges.RemoveAll((OutgoingChange ch) => !ch.Removal);
		}

		public bool ContainsKey(TKey key)
		{
			return dict.ContainsKey(key);
		}

		public int Count()
		{
			return dict.Count;
		}

		public bool Remove(TKey key)
		{
			if (dict.ContainsKey(key))
			{
				TField field = dict[key];
				NetVersion reassign = dictReassigns[key];
				dict.Remove(key);
				dictReassigns.Remove(key);
				removed(key, field, reassign);
				return true;
			}
			return false;
		}

		public void Filter(Func<KeyValuePair<TKey, TValue>, bool> f)
		{
			foreach (KeyValuePair<TKey, TValue> pair in Pairs.ToList())
			{
				if (!f(pair))
				{
					Remove(pair.Key);
				}
			}
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (dict.TryGetValue(key, out TField field))
			{
				value = getFieldValue(field);
				return true;
			}
			value = default(TValue);
			return false;
		}

		public bool Equals(TSelf other)
		{
			return object.Equals(dict, other.dict);
		}

		protected override void CleanImpl()
		{
			base.CleanImpl();
			outgoingChanges.Clear();
		}

		protected abstract TKey ReadKey(BinaryReader reader);

		protected abstract void WriteKey(BinaryWriter writer, TKey key);

		private void readMultiple(ReadFunc readFunc, BinaryReader reader, NetVersion version)
		{
			uint count = reader.Read7BitEncoded();
			for (uint i = 0u; i < count; i++)
			{
				readFunc(reader, version);
			}
		}

		private void writeMultiple<T>(WriteFunc<T> writeFunc, BinaryWriter writer, IEnumerable<T> values)
		{
			writer.Write7BitEncoded((uint)values.Count());
			foreach (T value in values)
			{
				writeFunc(writer, value);
			}
		}

		protected virtual TField ReadFieldFull(BinaryReader reader, NetVersion version)
		{
			TField val = new TField();
			val.NetFields.ReadFull(reader, version);
			return val;
		}

		protected virtual void WriteFieldFull(BinaryWriter writer, TField field)
		{
			field.NetFields.WriteFull(writer);
		}

		private void readAddition(BinaryReader reader, NetVersion version)
		{
			TKey key = ReadKey(reader);
			NetVersion reassign = default(NetVersion);
			reassign.Read(reader);
			TField field = ReadFieldFull(reader, version);
			setFieldParent(field);
			queueIncomingChange(removal: false, key, field, reassign);
		}

		protected virtual bool resolveConflict(TKey key, TField currentField, NetVersion currentReassign, TField incomingField, NetVersion incomingReassign)
		{
			if (incomingReassign.IsPriorityOver(currentReassign))
			{
				clearFieldParent(currentField);
				if (this.OnConflictResolve != null)
				{
					this.OnConflictResolve(key, currentField, incomingField);
				}
				return true;
			}
			clearFieldParent(incomingField);
			if (this.OnConflictResolve != null)
			{
				this.OnConflictResolve(key, incomingField, currentField);
			}
			return false;
		}

		private KeyValuePair<NetVersion, TField>? findConflict(TKey key)
		{
			foreach (IncomingChange change in incomingChanges.AsEnumerable().Reverse())
			{
				if (object.Equals(change.Key, key))
				{
					if (change.Removal)
					{
						return null;
					}
					return new KeyValuePair<NetVersion, TField>(change.Reassigned, change.Field);
				}
			}
			if (dict.ContainsKey(key))
			{
				return new KeyValuePair<NetVersion, TField>(dictReassigns[key], dict[key]);
			}
			return null;
		}

		private void queueIncomingChange(bool removal, TKey key, TField field, NetVersion fieldReassign)
		{
			if (!removal)
			{
				KeyValuePair<NetVersion, TField>? conflict = findConflict(key);
				if (conflict.HasValue && !resolveConflict(key, conflict.Value.Value, conflict.Value.Key, field, fieldReassign))
				{
					return;
				}
			}
			uint timestamp = (uint)((int)GetLocalTick() + ((InterpolationWait && base.Root != null) ? base.Root.Clock.InterpolationTicks : 0));
			incomingChanges.Add(new IncomingChange(timestamp, removal, key, field, fieldReassign));
			base.NeedsTick = true;
		}

		private void performIncomingAdd(IncomingChange add)
		{
			dict[add.Key] = add.Field;
			dictReassigns[add.Key] = add.Reassigned;
			addedEvent(add.Key, add.Field);
		}

		private void readRemoval(BinaryReader reader, NetVersion version)
		{
			TKey key = ReadKey(reader);
			NetVersion reassign = default(NetVersion);
			reassign.Read(reader);
			queueIncomingChange(removal: true, key, null, reassign);
		}

		private void readDictChange(BinaryReader reader, NetVersion version)
		{
			if (reader.ReadByte() != 0)
			{
				readRemoval(reader, version);
			}
			else
			{
				readAddition(reader, version);
			}
		}

		private void performIncomingRemove(IncomingChange remove)
		{
			if (dict.ContainsKey(remove.Key))
			{
				TField field = dict[remove.Key];
				clearFieldParent(field);
				dict.Remove(remove.Key);
				dictReassigns.Remove(remove.Key);
				removedEvent(remove.Key, field);
			}
		}

		private void readUpdate(BinaryReader reader, NetVersion version)
		{
			TKey key = ReadKey(reader);
			NetVersion reassign = default(NetVersion);
			reassign.Read(reader);
			reader.ReadSkippable(delegate
			{
				int num = incomingChanges.FindLastIndex((IncomingChange ch) => !ch.Removal && object.Equals(ch.Key, key) && reassign.Equals(ch.Reassigned));
				if (num != -1)
				{
					TField field = incomingChanges[num].Field;
					if (this.OnValueTargetUpdated != null)
					{
						TValue fieldTargetValue = getFieldTargetValue(field);
						field.NetFields.Read(reader, version);
						updatedEvent(key, fieldTargetValue, getFieldTargetValue(field));
					}
					else
					{
						field.NetFields.Read(reader, version);
					}
				}
				else if (dict.ContainsKey(key) && dictReassigns[key].Equals(reassign))
				{
					TField val = dict[key];
					if (this.OnValueTargetUpdated != null)
					{
						TValue fieldTargetValue2 = getFieldTargetValue(val);
						val.NetFields.Read(reader, version);
						updatedEvent(key, fieldTargetValue2, getFieldTargetValue(val));
					}
					else
					{
						val.NetFields.Read(reader, version);
					}
				}
			});
		}

		public override void Read(BinaryReader reader, NetVersion version)
		{
			readMultiple(readDictChange, reader, version);
			readMultiple(readUpdate, reader, version);
		}

		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			dict.Clear();
			dictReassigns.Clear();
			outgoingChanges.Clear();
			incomingChanges.Clear();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				TKey key = ReadKey(reader);
				NetVersion reassign = default(NetVersion);
				reassign.Read(reader);
				TField field = ReadFieldFull(reader, version);
				dict.Add(key, field);
				dictReassigns.Add(key, reassign);
				setFieldParent(field);
				addedEvent(key, field);
			}
		}

		private void writeAddition(BinaryWriter writer, OutgoingChange update)
		{
			WriteKey(writer, update.Key);
			update.Reassigned.Write(writer);
			WriteFieldFull(writer, update.Field);
		}

		private void writeRemoval(BinaryWriter writer, OutgoingChange update)
		{
			WriteKey(writer, update.Key);
			update.Reassigned.Write(writer);
		}

		private void writeDictChange(BinaryWriter writer, OutgoingChange ch)
		{
			if (ch.Removal)
			{
				writer.Write((byte)1);
				writeRemoval(writer, ch);
			}
			else
			{
				writer.Write((byte)0);
				writeAddition(writer, ch);
			}
		}

		private void writeUpdate(BinaryWriter writer, OutgoingChange update)
		{
			WriteKey(writer, update.Key);
			update.Reassigned.Write(writer);
			writer.WriteSkippable(delegate
			{
				update.Field.NetFields.Write(writer);
			});
		}

		private IEnumerable<OutgoingChange> updates()
		{
			foreach (KeyValuePair<TKey, TField> pair in dict)
			{
				if (pair.Value.NetFields.Dirty)
				{
					yield return new OutgoingChange(removal: false, pair.Key, pair.Value, dictReassigns[pair.Key]);
				}
			}
			foreach (OutgoingChange removal in outgoingChanges.Where((OutgoingChange ch) => ch.Removal))
			{
				if (removal.Field.NetFields.Dirty)
				{
					yield return removal;
				}
			}
		}

		public override void Write(BinaryWriter writer)
		{
			writeMultiple(writeDictChange, writer, outgoingChanges);
			writeMultiple(writeUpdate, writer, updates());
		}

		public override void WriteFull(BinaryWriter writer)
		{
			writer.Write(Count());
			foreach (TKey key in dict.Keys)
			{
				WriteKey(writer, key);
				dictReassigns[key].Write(writer);
				WriteFieldFull(writer, dict[key]);
			}
		}

		public IEnumerator<TSerialDict> GetEnumerator()
		{
			TSerialDict serial = new TSerialDict();
			foreach (KeyValuePair<TKey, TField> kvp in dict)
			{
				serial.Add(kvp.Key, getFieldValue(kvp.Value));
			}
			return new List<TSerialDict>
			{
				serial
			}.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			foreach (IncomingChange ch in incomingChanges)
			{
				if (ch.Field != null)
				{
					childAction(ch.Field.NetFields);
				}
			}
			foreach (TField field in dict.Values)
			{
				childAction(field.NetFields);
			}
		}

		public void Add(TSerialDict dict)
		{
			Set(dict);
		}

		protected override void ValidateChildren()
		{
			if ((base.Parent != null || base.Root == this) && !base.NeedsTick)
			{
				ForEachChild(ValidateChild);
			}
		}
	}
}
