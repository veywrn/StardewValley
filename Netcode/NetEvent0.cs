using System;
using System.IO;

namespace Netcode
{
	public class NetEvent0 : AbstractNetSerializable
	{
		public delegate void Event();

		public readonly NetInt Counter = new NetInt();

		private int currentCount;

		public event Event onEvent;

		public NetEvent0(bool interpolate = false)
		{
			Counter.InterpolationEnabled = interpolate;
		}

		public void Fire()
		{
			int num = ++Counter.Value;
			Poll();
		}

		public void Poll()
		{
			if (Counter.Value != currentCount)
			{
				currentCount = Counter.Value;
				if (this.onEvent != null)
				{
					this.onEvent();
				}
			}
		}

		public void Clear()
		{
			Counter.Set(0);
			currentCount = 0;
		}

		public override void Read(BinaryReader reader, NetVersion version)
		{
			Counter.Read(reader, version);
		}

		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			Counter.ReadFull(reader, version);
			currentCount = Counter.Value;
		}

		public override void Write(BinaryWriter writer)
		{
			Counter.Write(writer);
		}

		public override void WriteFull(BinaryWriter writer)
		{
			Counter.WriteFull(writer);
		}

		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			childAction(Counter);
		}
	}
}
