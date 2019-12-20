using System.Collections.Generic;

namespace StardewValley
{
	public class PriorityQueue
	{
		private int total_size;

		private SortedDictionary<int, Queue<PathNode>> nodes;

		public PriorityQueue()
		{
			nodes = new SortedDictionary<int, Queue<PathNode>>();
			total_size = 0;
		}

		public bool IsEmpty()
		{
			return total_size == 0;
		}

		public void Clear()
		{
			total_size = 0;
			foreach (KeyValuePair<int, Queue<PathNode>> node in nodes)
			{
				node.Value.Clear();
			}
		}

		public bool Contains(PathNode p, int priority)
		{
			if (!nodes.TryGetValue(priority, out Queue<PathNode> v))
			{
				return false;
			}
			return v.Contains(p);
		}

		public PathNode Dequeue()
		{
			if (!IsEmpty())
			{
				foreach (Queue<PathNode> q in nodes.Values)
				{
					if (q.Count > 0)
					{
						total_size--;
						return q.Dequeue();
					}
				}
			}
			return null;
		}

		public object Peek()
		{
			if (!IsEmpty())
			{
				foreach (Queue<PathNode> q in nodes.Values)
				{
					if (q.Count > 0)
					{
						return q.Peek();
					}
				}
			}
			return null;
		}

		public object Dequeue(int priority)
		{
			total_size--;
			return nodes[priority].Dequeue();
		}

		public void Enqueue(PathNode item, int priority)
		{
			if (!nodes.ContainsKey(priority))
			{
				nodes.Add(priority, new Queue<PathNode>());
				Enqueue(item, priority);
			}
			else
			{
				nodes[priority].Enqueue(item);
				total_size++;
			}
		}
	}
}
