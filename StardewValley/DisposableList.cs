using System;
using System.Collections.Generic;

namespace StardewValley
{
	public struct DisposableList<T>
	{
		public struct Enumerator : IDisposable
		{
			private readonly DisposableList<T> _parent;

			private int _index;

			public T Current
			{
				get
				{
					if (_parent._list == null || _index == 0)
					{
						throw new InvalidOperationException();
					}
					return _parent._list[_index - 1];
				}
			}

			public Enumerator(DisposableList<T> parent)
			{
				_parent = parent;
				_index = 0;
			}

			public bool MoveNext()
			{
				_index++;
				if (_parent._list != null)
				{
					return _parent._list.Count >= _index;
				}
				return false;
			}

			public void Reset()
			{
				_index = 0;
			}

			public void Dispose()
			{
				lock (_parent._pool)
				{
					_parent._pool.Return(_parent._list);
				}
			}
		}

		private readonly ListPool<T> _pool;

		private readonly List<T> _list;

		public DisposableList(List<T> list, ListPool<T> pool)
		{
			_list = list;
			_pool = pool;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
	}
}
