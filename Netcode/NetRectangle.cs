using Microsoft.Xna.Framework;
using System.IO;

namespace Netcode
{
	public sealed class NetRectangle : NetField<Rectangle, NetRectangle>
	{
		public int X
		{
			get
			{
				return base.Value.X;
			}
			set
			{
				Rectangle rect = base.value;
				if (rect.X != value)
				{
					Rectangle newValue = new Rectangle(value, rect.Y, rect.Width, rect.Height);
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
				Rectangle rect = base.value;
				if (rect.Y != value)
				{
					Rectangle newValue = new Rectangle(rect.X, value, rect.Width, rect.Height);
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

		public int Width
		{
			get
			{
				return base.Value.Width;
			}
			set
			{
				Rectangle rect = base.value;
				if (rect.Width != value)
				{
					Rectangle newValue = new Rectangle(rect.X, rect.Y, value, rect.Height);
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

		public int Height
		{
			get
			{
				return base.Value.Height;
			}
			set
			{
				Rectangle rect = base.value;
				if (rect.Height != value)
				{
					Rectangle newValue = new Rectangle(rect.X, rect.Y, rect.Width, value);
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

		public Point Center => value.Center;

		public int Top => value.Top;

		public int Bottom => value.Bottom;

		public int Left => value.Left;

		public int Right => value.Right;

		public NetRectangle()
		{
		}

		public NetRectangle(Rectangle value)
			: base(value)
		{
		}

		public void Set(int x, int y, int width, int height)
		{
			Set(new Rectangle(x, y, width, height));
		}

		public override void Set(Rectangle newValue)
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

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			int newX = reader.ReadInt32();
			int newY = reader.ReadInt32();
			int newWidth = reader.ReadInt32();
			int newHeight = reader.ReadInt32();
			if (version.IsPriorityOver(ChangeVersion))
			{
				setInterpolationTarget(new Rectangle(newX, newY, newWidth, newHeight));
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(value.X);
			writer.Write(value.Y);
			writer.Write(value.Width);
			writer.Write(value.Height);
		}
	}
}
