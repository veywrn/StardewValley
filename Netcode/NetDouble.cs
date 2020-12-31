using System.IO;

namespace Netcode
{
	public sealed class NetDouble : NetField<double, NetDouble>
	{
		public NetDouble()
		{
		}

		public NetDouble(double value)
			: base(value)
		{
		}

		public override void Set(double newValue)
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

		protected override double interpolate(double startValue, double endValue, float factor)
		{
			return startValue + (endValue - startValue) * (double)factor;
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			double newValue = reader.ReadDouble();
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
