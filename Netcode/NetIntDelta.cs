using System;
using System.IO;

namespace Netcode
{
	public sealed class NetIntDelta : NetField<int, NetIntDelta>
	{
		private int networkValue;

		public int DirtyThreshold;

		public int? Minimum;

		public int? Maximum;

		public NetIntDelta()
		{
			Interpolated(interpolate: false, wait: false);
		}

		public NetIntDelta(int value)
			: base(value)
		{
			Interpolated(interpolate: false, wait: false);
		}

		private int fixRange(int value)
		{
			if (Minimum.HasValue)
			{
				value = Math.Max(Minimum.Value, value);
			}
			if (Maximum.HasValue)
			{
				value = Math.Min(Maximum.Value, value);
			}
			return value;
		}

		public override void Set(int newValue)
		{
			newValue = fixRange(newValue);
			if (newValue != value)
			{
				cleanSet(newValue);
				if (Math.Abs(newValue - networkValue) > DirtyThreshold)
				{
					MarkDirty();
				}
			}
		}

		protected override int interpolate(int startValue, int endValue, float factor)
		{
			return startValue + (int)((float)(endValue - startValue) * factor);
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			int delta = reader.ReadInt32();
			networkValue = fixRange(networkValue + delta);
			setInterpolationTarget(fixRange(targetValue + delta));
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(targetValue - networkValue);
			networkValue = targetValue;
		}

		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			int fullValue = reader.ReadInt32();
			cleanSet(fullValue);
			networkValue = fullValue;
			ChangeVersion.Merge(version);
		}

		public override void WriteFull(BinaryWriter writer)
		{
			writer.Write(targetValue);
			networkValue = targetValue;
		}
	}
}
