using System;

namespace StardewValley
{
	public class PathNode : IEquatable<PathNode>
	{
		public readonly int x;

		public readonly int y;

		public readonly int id;

		public byte g;

		public PathNode parent;

		public PathNode(int x, int y, PathNode parent)
		{
			this.x = x;
			this.y = y;
			this.parent = parent;
			id = ComputeHash(x, y);
		}

		public PathNode(int x, int y, byte g, PathNode parent)
		{
			this.x = x;
			this.y = y;
			this.g = g;
			this.parent = parent;
			id = ComputeHash(x, y);
		}

		public bool Equals(PathNode obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (x == obj.x)
			{
				return y == obj.y;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			PathNode other = obj as PathNode;
			if (other == null)
			{
				return false;
			}
			if (x == other.x)
			{
				return y == other.y;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return id;
		}

		public static int ComputeHash(int x, int y)
		{
			return 100000 * x + y;
		}
	}
}
