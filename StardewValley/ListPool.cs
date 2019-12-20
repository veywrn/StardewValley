using System.Collections.Generic;

namespace StardewValley
{
	public class ListPool<T>
	{
		private readonly Stack<List<T>> _in;

		public ListPool()
		{
			_in = new Stack<List<T>>();
			_in.Push(new List<T>());
		}

		public List<T> Get()
		{
			if (_in.Count == 0)
			{
				_in.Push(new List<T>());
			}
			return _in.Pop();
		}

		public void Return(List<T> list)
		{
			list.Clear();
			_in.Push(list);
		}
	}
}
