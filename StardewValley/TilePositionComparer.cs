using Microsoft.Xna.Framework;
using System.Collections.Generic;

public class TilePositionComparer : IEqualityComparer<Vector2>
{
	public bool Equals(Vector2 a, Vector2 b)
	{
		return a.Equals(b);
	}

	public int GetHashCode(Vector2 a)
	{
		return (ushort)a.X | ((ushort)a.Y << 16);
	}
}
