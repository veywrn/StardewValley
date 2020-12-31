using System.IO;

namespace Netcode
{
	public sealed class NetLong : NetField<long, NetLong>
	{
		public NetLong()
		{
		}

		public NetLong(long value)
			: base(value)
		{
		}

		public override void Set(long newValue)
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

		protected override long interpolate(long startValue, long endValue, float factor)
		{
			return startValue + (long)((float)(endValue - startValue) * factor);
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			long newValue = reader.ReadInt64();
			if (version.IsPriorityOver(ChangeVersion))
			{
				setInterpolationTarget(newValue);
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(value);
		}

		public static implicit operator long(NetLong field)
		{
			return field.value;
		}
	}
}
