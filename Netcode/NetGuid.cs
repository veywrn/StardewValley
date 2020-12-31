using System;
using System.IO;

namespace Netcode
{
	public sealed class NetGuid : NetField<Guid, NetGuid>
	{
		public NetGuid()
		{
		}

		public NetGuid(Guid value)
			: base(value)
		{
		}

		public override void Set(Guid newValue)
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
			Guid newValue = reader.ReadGuid();
			if (version.IsPriorityOver(ChangeVersion))
			{
				setInterpolationTarget(newValue);
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.WriteGuid(value);
		}
	}
}
