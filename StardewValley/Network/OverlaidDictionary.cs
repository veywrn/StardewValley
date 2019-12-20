using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network
{
	public sealed class OverlaidDictionary : IEnumerable<SerializableDictionary<Vector2, Object>>, IEnumerable
	{
		public struct ValuesCollection : IEnumerable<Object>, IEnumerable
		{
			public struct Enumerator : IEnumerator<Object>, IDisposable, IEnumerator
			{
				private readonly OverlaidDictionary _dict;

				private NetDictionary<Vector2, Object, NetRef<Object>, SerializableDictionary<Vector2, Object>, NetVector2Dictionary<Object, NetRef<Object>>>.ValuesCollection.Enumerator _base;

				private Dictionary<Vector2, Object>.Enumerator _overlay;

				private Object _current;

				private bool _done;

				public Object Current => _current;

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

				public Enumerator(OverlaidDictionary dict)
				{
					_dict = dict;
					_base = _dict.baseDict.Values.GetEnumerator();
					_overlay = _dict.overlayDict.GetEnumerator();
					_current = null;
					_done = false;
				}

				public bool MoveNext()
				{
					if (_base.MoveNext())
					{
						_current = _base.Current;
						return true;
					}
					if (_overlay.MoveNext())
					{
						_current = _overlay.Current.Value;
						return true;
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
					_base = _dict.baseDict.Values.GetEnumerator();
					_overlay = _dict.overlayDict.GetEnumerator();
					_current = null;
					_done = false;
				}
			}

			private OverlaidDictionary _dict;

			public ValuesCollection(OverlaidDictionary dict)
			{
				_dict = dict;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_dict);
			}

			IEnumerator<Object> IEnumerable<Object>.GetEnumerator()
			{
				return new Enumerator(_dict);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(_dict);
			}
		}

		public struct KeysCollection : IEnumerable<Vector2>, IEnumerable
		{
			public struct Enumerator : IEnumerator<Vector2>, IDisposable, IEnumerator
			{
				private readonly OverlaidDictionary _dict;

				private NetDictionary<Vector2, Object, NetRef<Object>, SerializableDictionary<Vector2, Object>, NetVector2Dictionary<Object, NetRef<Object>>>.KeysCollection.Enumerator _base;

				private Dictionary<Vector2, Object>.Enumerator _overlay;

				private Vector2 _current;

				private bool _done;

				public Vector2 Current => _current;

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

				public Enumerator(OverlaidDictionary dict)
				{
					_dict = dict;
					_base = _dict.baseDict.Keys.GetEnumerator();
					_overlay = _dict.overlayDict.GetEnumerator();
					_current = Vector2.Zero;
					_done = false;
				}

				public bool MoveNext()
				{
					if (_base.MoveNext())
					{
						_current = _base.Current;
						return true;
					}
					while (_overlay.MoveNext())
					{
						Vector2 key = _overlay.Current.Key;
						if (!_dict.baseDict.ContainsKey(key))
						{
							_current = _overlay.Current.Key;
							return true;
						}
					}
					_done = true;
					_current = Vector2.Zero;
					return false;
				}

				public void Dispose()
				{
				}

				void IEnumerator.Reset()
				{
					_base = _dict.baseDict.Keys.GetEnumerator();
					_overlay = _dict.overlayDict.GetEnumerator();
					_current = Vector2.Zero;
					_done = false;
				}
			}

			private OverlaidDictionary _dict;

			public KeysCollection(OverlaidDictionary dict)
			{
				_dict = dict;
			}

			public int Count()
			{
				int count = 0;
				using (Enumerator enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_ = enumerator.Current;
						count++;
					}
					return count;
				}
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_dict);
			}

			IEnumerator<Vector2> IEnumerable<Vector2>.GetEnumerator()
			{
				return new Enumerator(_dict);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(_dict);
			}
		}

		public struct PairsCollection : IEnumerable<KeyValuePair<Vector2, Object>>, IEnumerable
		{
			public struct Enumerator : IEnumerator<KeyValuePair<Vector2, Object>>, IDisposable, IEnumerator
			{
				private readonly OverlaidDictionary _dict;

				private NetDictionary<Vector2, Object, NetRef<Object>, SerializableDictionary<Vector2, Object>, NetVector2Dictionary<Object, NetRef<Object>>>.PairsCollection.Enumerator _base;

				private Dictionary<Vector2, Object>.Enumerator _overlay;

				private KeyValuePair<Vector2, Object> _current;

				private bool _done;

				public KeyValuePair<Vector2, Object> Current => _current;

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

				public Enumerator(OverlaidDictionary dict)
				{
					_dict = dict;
					_base = _dict.baseDict.Pairs.GetEnumerator();
					_overlay = _dict.overlayDict.GetEnumerator();
					_current = default(KeyValuePair<Vector2, Object>);
					_done = false;
				}

				public bool MoveNext()
				{
					while (_base.MoveNext())
					{
						KeyValuePair<Vector2, Object> pair = _base.Current;
						if (!_dict.overlayDict.ContainsKey(pair.Key))
						{
							_current = new KeyValuePair<Vector2, Object>(pair.Key, pair.Value);
							return true;
						}
					}
					if (_overlay.MoveNext())
					{
						KeyValuePair<Vector2, Object> pair2 = _overlay.Current;
						_current = new KeyValuePair<Vector2, Object>(pair2.Key, pair2.Value);
						return true;
					}
					_done = true;
					_current = default(KeyValuePair<Vector2, Object>);
					return false;
				}

				public void Dispose()
				{
				}

				void IEnumerator.Reset()
				{
					_base = _dict.baseDict.Pairs.GetEnumerator();
					_overlay = _dict.overlayDict.GetEnumerator();
					_current = default(KeyValuePair<Vector2, Object>);
					_done = false;
				}
			}

			private OverlaidDictionary _dict;

			public PairsCollection(OverlaidDictionary dict)
			{
				_dict = dict;
			}

			public KeyValuePair<Vector2, Object> ElementAt(int index)
			{
				int count = 0;
				using (Enumerator enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<Vector2, Object> pair = enumerator.Current;
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
				return new Enumerator(_dict);
			}

			IEnumerator<KeyValuePair<Vector2, Object>> IEnumerable<KeyValuePair<Vector2, Object>>.GetEnumerator()
			{
				return new Enumerator(_dict);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(_dict);
			}
		}

		private NetVector2Dictionary<Object, NetRef<Object>> baseDict;

		private Dictionary<Vector2, Object> overlayDict;

		public Object this[Vector2 key]
		{
			get
			{
				if (overlayDict.ContainsKey(key))
				{
					return overlayDict[key];
				}
				return baseDict[key];
			}
			set
			{
				baseDict[key] = value;
			}
		}

		public KeysCollection Keys => new KeysCollection(this);

		public ValuesCollection Values => new ValuesCollection(this);

		public PairsCollection Pairs => new PairsCollection(this);

		public void SetEqualityComparer(IEqualityComparer<Vector2> comparer, ref NetVector2Dictionary<Object, NetRef<Object>> base_dict, ref Dictionary<Vector2, Object> overlay_dict)
		{
			baseDict.SetEqualityComparer(comparer);
			overlayDict = new Dictionary<Vector2, Object>(overlayDict);
			base_dict = baseDict;
			overlay_dict = overlayDict;
		}

		public OverlaidDictionary(NetVector2Dictionary<Object, NetRef<Object>> baseDict, Dictionary<Vector2, Object> overlayDict)
		{
			this.baseDict = baseDict;
			this.overlayDict = overlayDict;
		}

		public int Count()
		{
			return Keys.Count();
		}

		public void Add(Vector2 key, Object value)
		{
			baseDict.Add(key, value);
		}

		public void Clear()
		{
			baseDict.Clear();
			overlayDict.Clear();
		}

		public bool ContainsKey(Vector2 key)
		{
			if (!overlayDict.ContainsKey(key))
			{
				return baseDict.ContainsKey(key);
			}
			return true;
		}

		public bool Remove(Vector2 key)
		{
			if (overlayDict.ContainsKey(key))
			{
				return overlayDict.Remove(key);
			}
			return baseDict.Remove(key);
		}

		public bool TryGetValue(Vector2 key, out Object value)
		{
			if (overlayDict.TryGetValue(key, out value))
			{
				return true;
			}
			return baseDict.TryGetValue(key, out value);
		}

		public IEnumerator<SerializableDictionary<Vector2, Object>> GetEnumerator()
		{
			return baseDict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return baseDict.GetEnumerator();
		}

		public void Add(SerializableDictionary<Vector2, Object> dict)
		{
			foreach (KeyValuePair<Vector2, Object> pair in dict)
			{
				baseDict.Add(pair.Key, pair.Value);
			}
		}
	}
}
