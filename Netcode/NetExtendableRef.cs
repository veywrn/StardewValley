using System;
using System.IO;

namespace Netcode
{
	public class NetExtendableRef<T, TSelf> : NetRefBase<T, TSelf> where T : class, INetObject<INetSerializable> where TSelf : NetExtendableRef<T, TSelf>
	{
		public NetExtendableRef()
		{
			base.notifyOnTargetValueChange = true;
		}

		public NetExtendableRef(T value)
			: this()
		{
			cleanSet(value);
		}

		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			if (targetValue != null)
			{
				childAction(targetValue.NetFields);
			}
		}

		protected override void ReadValueFull(T value, BinaryReader reader, NetVersion version)
		{
			value.NetFields.ReadFull(reader, version);
		}

		protected override void ReadValueDelta(BinaryReader reader, NetVersion version)
		{
			targetValue.NetFields.Read(reader, version);
		}

		private void clearValueParent(T targetValue)
		{
			if (targetValue.NetFields.Parent == this)
			{
				targetValue.NetFields.Parent = null;
			}
		}

		private void setValueParent(T targetValue)
		{
			if (base.Parent != null || base.Root == this)
			{
				targetValue.NetFields.Parent = this;
			}
			targetValue.NetFields.MarkClean();
		}

		protected override void targetValueChanged(T oldValue, T newValue)
		{
			base.targetValueChanged(oldValue, newValue);
			if (oldValue != null)
			{
				clearValueParent(oldValue);
			}
			if (newValue != null)
			{
				setValueParent(newValue);
			}
		}

		protected override void WriteValueFull(BinaryWriter writer)
		{
			targetValue.NetFields.WriteFull(writer);
		}

		protected override void WriteValueDelta(BinaryWriter writer)
		{
			targetValue.NetFields.Write(writer);
		}
	}
}
