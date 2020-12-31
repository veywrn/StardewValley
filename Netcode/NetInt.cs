using System.IO;

namespace Netcode
{
	public sealed class NetInt : NetField<int, NetInt>
	{
		public NetInt()
		{
		}

		public NetInt(int value)
			: base(value)
		{
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

		public new bool Equals(NetInt other)
		{
			return value == other.value;
		}

		public bool Equals(int other)
		{
			return value == other;
		}

		protected override int interpolate(int startValue, int endValue, float factor)
		{
			return startValue + (int)((float)(endValue - startValue) * factor);
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
