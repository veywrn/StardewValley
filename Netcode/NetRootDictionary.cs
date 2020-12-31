using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Netcode
{
	public class NetRootDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable where TValue : class, INetObject<INetSerializable>
	{
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
		{
			private Dictionary<TKey, NetRoot<TValue>> _roots;

			private Dictionary<TKey, NetRoot<TValue>>.Enumerator _enumerator;

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

			public Enumerator(Dictionary<TKey, NetRoot<TValue>> roots)
			{
				_roots = roots;
				_enumerator = _roots.GetEnumerator();
				_current = default(KeyValuePair<TKey, TValue>);
				_done = false;
			}

			public bool MoveNext()
			{
				if (_enumerator.MoveNext())
				{
					KeyValuePair<TKey, NetRoot<TValue>> pair = _enumerator.Current;
					_current = new KeyValuePair<TKey, TValue>(pair.Key, pair.Value.Get());
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
				_enumerator = _roots.GetEnumerator();
				_current = default(KeyValuePair<TKey, TValue>);
				_done = false;
			}
		}

		public XmlSerializer Serializer;

		public Dictionary<TKey, NetRoot<TValue>> Roots = new Dictionary<TKey, NetRoot<TValue>>();

		public TValue this[TKey key]
		{
			get
			{
				return Roots[key].Get();
			}
			set
			{
				if (!ContainsKey(key))
				{
					Add(key, value);
				}
				else
				{
					Roots[key].Set(value);
				}
			}
		}

		public int Count => Roots.Count;

		public bool IsReadOnly => ((IDictionary)Roots).IsReadOnly;

		public ICollection<TKey> Keys => Roots.Keys;

		public ICollection<TValue> Values => Roots.Values.Select((NetRoot<TValue> root) => root.Get()).ToList();

		public NetRootDictionary()
		{
		}

		public NetRootDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values)
		{
			foreach (KeyValuePair<TKey, TValue> pair in values)
			{
				Add(pair.Key, pair.Value);
			}
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public void Add(TKey key, TValue value)
		{
			NetRoot<TValue> root = new NetRoot<TValue>(value);
			root.Serializer = Serializer;
			Roots.Add(key, root);
		}

		public void Clear()
		{
			Roots.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			if (!Roots.ContainsKey(item.Key))
			{
				return false;
			}
			return Roots[item.Key] == item.Value;
		}

		public bool ContainsKey(TKey key)
		{
			return Roots.ContainsKey(key);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (array.Length < Count - arrayIndex)
			{
				throw new ArgumentException();
			}
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<TKey, TValue> pair = enumerator.Current;
					array[arrayIndex++] = pair;
				}
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(Roots);
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new Enumerator(Roots);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(Roots);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (Contains(item))
			{
				return Remove(item.Key);
			}
			return false;
		}

		public bool Remove(TKey key)
		{
			return Roots.Remove(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (Roots.TryGetValue(key, out NetRoot<TValue> root))
			{
				value = root.Get();
				return true;
			}
			value = null;
			return false;
		}
	}
}
