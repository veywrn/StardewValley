using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.IO;

namespace StardewValley.Network
{
	public sealed class NetDirection : NetField<int, NetInt>
	{
		public NetPosition Position;

		public NetDirection()
		{
			base.InterpolationEnabled = true;
			base.InterpolationWait = true;
		}

		public NetDirection(int value)
			: base(value)
		{
			base.InterpolationEnabled = true;
			base.InterpolationWait = true;
		}

		public static implicit operator int(NetDirection netField)
		{
			if (netField == null)
			{
				return 0;
			}
			return netField.Get();
		}

		public override void Set(int newValue)
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

		protected override bool setUpInterpolation(int oldValue, int newValue)
		{
			return true;
		}

		public int getInterpolatedDirection()
		{
			if (Position != null && Position.IsInterpolating() && !Position.IsPausePending())
			{
				Vector2 dir = Position.CurrentInterpolationDirection();
				if (Math.Abs(dir.X) > Math.Abs(dir.Y))
				{
					if (dir.X < 0f)
					{
						return 3;
					}
					return 1;
				}
				if (Math.Abs(dir.Y) > Math.Abs(dir.X))
				{
					if (dir.Y < 0f)
					{
						return 0;
					}
					return 2;
				}
			}
			return value;
		}

		protected override int interpolate(int startValue, int endValue, float factor)
		{
			if (Position != null && Position.IsInterpolating() && !Position.IsPausePending())
			{
				Vector2 dir = Position.CurrentInterpolationDirection();
				if (Math.Abs(dir.X) > Math.Abs(dir.Y))
				{
					if (dir.X < 0f)
					{
						return 3;
					}
					return 1;
				}
				if (Math.Abs(dir.Y) > Math.Abs(dir.X))
				{
					if (dir.Y < 0f)
					{
						return 0;
					}
					return 2;
				}
			}
			return startValue;
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			int newValue = reader.ReadInt32();
			if (version.IsPriorityOver(ChangeVersion))
			{
				setInterpolationTarget(newValue);
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(value);
		}
	}
}
