using Microsoft.Xna.Framework;
using System.IO;

namespace Netcode
{
	public sealed class NetPoint : NetField<Point, NetPoint>
	{
		public int X
		{
			get
			{
				return base.Value.X;
			}
			set
			{
				Point point = base.value;
				if (point.X != value)
				{
					Point newValue = new Point(value, point.Y);
					if (canShortcutSet())
					{
						base.value = newValue;
						return;
					}
					cleanSet(newValue);
					MarkDirty();
				}
			}
		}

		public int Y
		{
			get
			{
				return base.Value.Y;
			}
			set
			{
				Point point = base.value;
				if (point.Y != value)
				{
					Point newValue = new Point(point.X, value);
					if (canShortcutSet())
					{
						base.value = newValue;
						return;
					}
					cleanSet(newValue);
					MarkDirty();
				}
			}
		}

		public NetPoint()
		{
		}

		public NetPoint(Point value)
			: base(value)
		{
		}

		public void Set(int x, int y)
		{
			Set(new Point(x, y));
		}

		public override void Set(Point newValue)
		{
			if (canShortcutSet())
			{
				value = newValue;
			}
			else if (newValue != value)
			{
				cleanSet(newValue);
				MarkDirty();
			}
		}

		protected override Point interpolate(Point startValue, Point endValue, float factor)
		{
			Point delta = new Point(endValue.X - startValue.X, endValue.Y - startValue.Y);
			delta.X = (int)((float)delta.X * factor);
			delta.Y = (int)((float)delta.Y * factor);
			return new Point(startValue.X + delta.X, startValue.Y + delta.Y);
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			int newX = reader.ReadInt32();
			int newY = reader.ReadInt32();
			if (version.IsPriorityOver(ChangeVersion))
			{
				setInterpolationTarget(new Point(newX, newY));
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(base.Value.X);
			writer.Write(base.Value.Y);
		}
	}
}
