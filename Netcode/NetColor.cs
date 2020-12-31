using Microsoft.Xna.Framework;
using System.IO;

namespace Netcode
{
	public sealed class NetColor : NetField<Color, NetColor>
	{
		public byte R
		{
			get
			{
				return base.Value.R;
			}
			set
			{
				base.Value = new Color(value, G, B, A);
			}
		}

		public byte G
		{
			get
			{
				return base.Value.G;
			}
			set
			{
				base.Value = new Color(R, value, B, A);
			}
		}

		public byte B
		{
			get
			{
				return base.Value.B;
			}
			set
			{
				base.Value = new Color(R, G, value, A);
			}
		}

		public byte A
		{
			get
			{
				return base.Value.A;
			}
			set
			{
				base.Value = new Color(R, G, B, value);
			}
		}

		public NetColor()
		{
		}

		public NetColor(Color value)
			: base(value)
		{
		}

		public override void Set(Color newValue)
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

		public new bool Equals(NetColor other)
		{
			return value == other.value;
		}

		public bool Equals(Color other)
		{
			return value == other;
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			Color newValue = default(Color);
			newValue.PackedValue = reader.ReadUInt32();
			if (version.IsPriorityOver(ChangeVersion))
			{
				setInterpolationTarget(newValue);
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(value.PackedValue);
		}
	}
}
